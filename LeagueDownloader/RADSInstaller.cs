using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fantome.Libraries.League.IO.ReleaseManifest;
using Fantome.Libraries.League.IO.RiotArchive;
using static Fantome.Libraries.League.IO.ReleaseManifest.ReleaseManifestFile.DeployMode;
using System.IO.Compression;
using LeagueDownloader.Solution;

namespace LeagueDownloader
{
    public class RADSInstaller
    {
        private static readonly string LeagueCDN = "http://l3cdn.riotgames.com";

        public string Directory { get; private set; }
        public string Platform { get; private set; }

        private WebClient webClient = new WebClient();

        public RADSInstaller(string directory, string platform)
        {
            this.Directory = directory;
            this.Platform = platform;
        }

        public void InstallSolution(string solutionName, string solutionVersion, string localization, uint? deployMode)
        {
            // Downloading solution manifest
            Console.WriteLine("Downloading solution manifest for release {0}", solutionVersion);
            var solutionFolder = String.Format("{0}/RADS/solutions/{1}/releases/{2}", Directory, solutionName, solutionVersion);
            System.IO.Directory.CreateDirectory(solutionFolder);
            webClient.DownloadFile(
                String.Format("{0}/releases/{1}/solutions/{2}/releases/{3}/solutionmanifest", LeagueCDN, Platform, solutionName, solutionVersion),
                solutionFolder + "/solutionmanifest");
            var solutionManifest = new SolutionManifest(File.ReadAllLines(solutionFolder + "/solutionmanifest"));
            LocalizedEntry localizedEntry = solutionManifest.LocalizedEntries.Find(x => x.Name.Equals(localization, StringComparison.InvariantCultureIgnoreCase));
            if (localizedEntry != null)
            {
                // Creating configuration manifest
                var configurationManifest = new ConfigurationManifest(localizedEntry);
                configurationManifest.Write(solutionFolder + "/configurationmanifest");

                // Downloading each project
                foreach (SolutionProject project in localizedEntry.Projects)
                    InstallProject(project.Name, project.Version, deployMode, solutionName, solutionVersion);

                File.Create(solutionFolder + "/S_OK").Close();
            }
        }

        public void InstallProject(string projectName, string projectVersion, uint? deployMode, string solutionName = null, string solutionVersion = null)
        {
            var projectsURL = String.Format("{0}/releases/{1}/projects/{2}", LeagueCDN, Platform, projectName);

            var projectFolder = String.Format("{0}/RADS/projects/{1}", Directory, projectName);
            var releaseFolder = String.Format("{0}/releases/{1}", projectFolder, projectVersion);
            var deployFolder = releaseFolder + "/deploy";
            var managedFilesFolder = projectFolder + "/managedfiles";
            var archivesFolder = projectFolder + "/filearchives";
            System.IO.Directory.CreateDirectory(deployFolder);
            System.IO.Directory.CreateDirectory(managedFilesFolder);
            System.IO.Directory.CreateDirectory(archivesFolder);

            string solutionDeployFolder = null;
            if (solutionName != null)
                solutionDeployFolder = String.Format("{0}/RADS/solutions/{1}/releases/{2}/deploy", Directory, solutionName, solutionVersion);

            // Getting release manifest
            Console.WriteLine("Downloading manifest for project {0}, release {1}...", projectName, projectVersion);
            var currentProjectURL = String.Format("{0}/releases/{1}", projectsURL, projectVersion);
            webClient.DownloadFile(currentProjectURL + "/releasemanifest", releaseFolder + "/releasemanifest");
            var releaseManifest = new ReleaseManifestFile(releaseFolder + "/releasemanifest");

            // Downloading files
            var files = new List<ReleaseManifestFileEntry>();
            EnumerateManifestFolderFiles(releaseManifest.Project, files);
            files.Sort((x, y) => (x.Version.CompareTo(y.Version)));

            string currentArchiveVersion = null;
            RAF currentRAF = null;
            foreach (ReleaseManifestFileEntry file in files)
            {
                string fileFullPath = file.GetFullPath();
                string fileVersion = GetReleaseString(file.Version);
                Console.WriteLine("Downloading file {0}/{1}", fileVersion, fileFullPath);
                bool compressed = false;
                var fileURL = String.Format("{0}/releases/{1}/files/{2}", projectsURL, fileVersion, fileFullPath);
                if (file.DeployMode != Deployed0)
                {
                    fileURL += ".compressed";
                    compressed = true;
                }
                byte[] fileData = webClient.DownloadData(fileURL);

                // Change deploy mode if specified
                if (deployMode != null)
                    file.DeployMode = (ReleaseManifestFile.DeployMode)deployMode;

                if (file.DeployMode == RAFCompressed || file.DeployMode == RAFRaw)
                {
                    // File has to be put in a RAF
                    if (currentRAF == null || currentArchiveVersion != fileVersion)
                    {
                        currentRAF?.Save();
                        currentRAF?.Dispose();
                        currentArchiveVersion = fileVersion;
                        currentRAF = new RAF(String.Format("{0}/{1}/Archive_1.raf", archivesFolder, fileVersion));
                    }
                    if (compressed)
                        currentRAF.AddFile(fileFullPath, file.DeployMode == RAFCompressed ? fileData : DecompressZlib(fileData), false);
                    else
                        currentRAF.AddFile(fileFullPath, fileData, file.DeployMode == RAFCompressed);
                }
                else if (file.DeployMode == Managed)
                {
                    // File will be in managedfiles folder
                    var filePath = String.Format("{0}/{1}/{2}", managedFilesFolder, fileVersion, fileFullPath);
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    File.WriteAllBytes(filePath, compressed ? DecompressZlib(fileData) : fileData);
                }
                else if (file.DeployMode == Deployed0 || file.DeployMode == Deployed4)
                {
                    // File will be in deploy folder
                    var deployPath = String.Format("{0}/{1}", deployFolder, fileFullPath);
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(deployPath));
                    byte[] decompressedData = compressed ? DecompressZlib(fileData) : fileData;
                    File.WriteAllBytes(deployPath, decompressedData);
                    if (solutionDeployFolder != null && file.DeployMode == Deployed4)
                    {
                        // File will also be in solution folder
                        var solutionPath = String.Format("{0}/{1}", solutionDeployFolder, fileFullPath);
                        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(solutionPath));
                        File.WriteAllBytes(solutionPath, decompressedData);
                    }
                }
            }
            currentRAF?.Dispose();
            releaseManifest.Save();
            File.Create(releaseFolder + "/S_OK").Close();
        }

        private static byte[] DecompressZlib(byte[] inputData)
        {
            byte[] data = new byte[inputData.Length - 6];
            using (MemoryStream ms = new MemoryStream(inputData))
            {
                ms.Seek(2, SeekOrigin.Begin);
                ms.Read(data, 0, data.Length);
            }
            return Inflate(data);
        }

        private static byte[] Inflate(byte[] compressedData)
        {
            byte[] decompressedData = null;
            using (MemoryStream compressedStream = new MemoryStream(compressedData))
            {
                using (MemoryStream rawStream = new MemoryStream())
                {
                    using (DeflateStream decompressionStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(rawStream);
                    }
                    decompressedData = rawStream.ToArray();
                }
            }
            return decompressedData;
        }

        private static void EnumerateManifestFolderFiles(ReleaseManifestFolderEntry folder, List<ReleaseManifestFileEntry> currentList)
        {
            currentList.AddRange(folder.Files);
            foreach (ReleaseManifestFolderEntry subFolder in folder.Folders)
                EnumerateManifestFolderFiles(subFolder, currentList);
        }

        private static string GetReleaseString(uint releaseValue)
        {
            return String.Format("{0}.{1}.{2}.{3}", (releaseValue & 0xFF000000) >> 24, (releaseValue & 0x00FF0000) >> 16, (releaseValue & 0x0000FF00) >> 8, releaseValue & 0x000000FF);
        }
    }
}
