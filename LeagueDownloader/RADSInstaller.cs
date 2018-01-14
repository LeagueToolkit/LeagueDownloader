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

        public void InstallProject(string projectName, string projectVersion, string solutionName = null, string solutionVersion = null)
        {
            var projectsURL = String.Format("{0}/releases/{1}/projects/{2}/", LeagueCDN, Platform, projectName);

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
            Console.WriteLine("Downloading manifest...");
            var currentProjectURL = String.Format("{0}/releases/{1}", projectsURL, projectVersion);
            webClient.DownloadFile(currentProjectURL + "/releasemanifest", releaseFolder + "/releasemanifest");
            var releaseManifest = new ReleaseManifestFile(releaseFolder + "/releasemanifest");

            // Downloading files
            var files = new List<ReleaseManifestFileEntry>();
            EnumerateManifestFolderFiles(releaseManifest.Project, files);
            files.OrderBy(x => x.Version);

            string currentArchiveVersion = null;
            RAF currentRAF = null;
            foreach (ReleaseManifestFileEntry file in files)
            {
                string fileFullPath = file.GetFullPath();
                string fileVersion = GetReleaseString(file.Version);
                Console.WriteLine("Downloading file: {0}", fileFullPath);
                var fileURL = String.Format("{0}/releases/{1}/files/{2}.compressed", projectsURL, fileVersion, fileFullPath);
                byte[] fileData = webClient.DownloadData(fileURL);
                if (file.DeployMode == RAFCompressed || file.DeployMode == RAFRaw)
                {
                    // File has to be in a RAF
                    if (currentRAF == null || currentArchiveVersion != fileVersion)
                    {
                        currentRAF?.Save();
                        currentRAF?.Dispose();
                        currentArchiveVersion = fileVersion;
                        currentRAF = new RAF(String.Format("{0}/{1}/Archive_1.raf", archivesFolder, fileVersion));
                    }
                    currentRAF.AddFile(fileFullPath, file.DeployMode == RAFCompressed ? fileData : DecompressZlib(fileData), false);
                }
                else if (file.DeployMode == Managed)
                {
                    var filePath = String.Format("{0}/{1}/{2}", managedFilesFolder, fileVersion, fileFullPath);
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    File.WriteAllBytes(filePath, DecompressZlib(fileData));
                }
                else if (file.DeployMode == Deployed0 || file.DeployMode == Deployed4)
                {
                    var deployPath = String.Format("{0}/{1}", deployFolder, fileFullPath);
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(deployPath));
                    byte[] decompressedData = DecompressZlib(fileData);
                    File.WriteAllBytes(deployPath, decompressedData);
                    if (solutionDeployFolder != null && file.DeployMode == Deployed4)
                    {
                        // Also deploy in solution
                        var solutionPath = String.Format("{0}/{1}", solutionDeployFolder, fileFullPath);
                        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(solutionPath));
                        File.WriteAllBytes(solutionPath, decompressedData);
                    }
                }
            }
            currentRAF?.Dispose();
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
