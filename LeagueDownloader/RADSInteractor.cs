using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Fantome.Libraries.League.IO.ReleaseManifest;
using Fantome.Libraries.League.IO.RiotArchive;
using static Fantome.Libraries.League.IO.ReleaseManifest.ReleaseManifestFile.DeployMode;
using LeagueDownloader.Solution;
using static LeagueDownloader.Utilities;
using LeagueDownloader.Content;

namespace LeagueDownloader
{
    public class RADSInteractor
    {
        public string LeagueCDNBaseURL { get; private set; }

        private WebClient webClient = new WebClient();

        public RADSInteractor(string cdnBaseURL)
        {
            this.LeagueCDNBaseURL = cdnBaseURL;
        }

        public void InstallSolution(string directory, string solutionName, string solutionVersion, string localization, uint? deployMode)
        {
            // Downloading solution manifest
            if (String.Equals(solutionVersion, Constants.LatestVersionString))
                solutionVersion = GetLatestSolutionRelease(solutionName);

            Console.WriteLine("Downloading solution manifest for release {0}", solutionVersion);
            var solutionFolder = String.Format("{0}/RADS/solutions/{1}/releases/{2}", directory, solutionName, solutionVersion);
            System.IO.Directory.CreateDirectory(solutionFolder);
            webClient.DownloadFile(
                String.Format("{0}/solutions/{1}/releases/{2}/solutionmanifest", LeagueCDNBaseURL, solutionName, solutionVersion),
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
                    InstallProject(directory, project.Name, project.Version, deployMode, solutionName, solutionVersion);

                File.Create(solutionFolder + "/S_OK").Close();
            }
        }

        public void InstallProject(string directory, string projectName, string projectVersion, uint? deployMode, string solutionName = null, string solutionVersion = null)
        {
            var projectsURL = String.Format("{0}/projects/{1}", LeagueCDNBaseURL, projectName);

            if (String.Equals(projectVersion, Constants.LatestVersionString))
                projectVersion = GetLatestProjectRelease(projectName);

            var projectFolder = String.Format("{0}/RADS/projects/{1}", directory, projectName);
            var releaseFolder = String.Format("{0}/releases/{1}", projectFolder, projectVersion);
            var deployFolder = releaseFolder + "/deploy";
            var managedFilesFolder = projectFolder + "/managedfiles";
            var archivesFolder = projectFolder + "/filearchives";
            Directory.CreateDirectory(deployFolder);
            Directory.CreateDirectory(managedFilesFolder);
            Directory.CreateDirectory(archivesFolder);

            string solutionDeployFolder = null;
            if (solutionName != null)
                solutionDeployFolder = String.Format("{0}/RADS/solutions/{1}/releases/{2}/deploy", directory, solutionName, solutionVersion);

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
            var currentRAFs = new List<RAF>();
            foreach (ReleaseManifestFileEntry file in files)
            {
                var remoteAsset = new RemoteAsset(file, projectsURL, webClient);
                AssetContent assetContent = remoteAsset.AssetContent;
                Console.Write("■ Downloading {0}/{1}", remoteAsset.StringVersion, remoteAsset.FileFullPath);
                try
                {
                    // Change deploy mode if specified
                    if (deployMode != null)
                        file.DeployMode = (ReleaseManifestFile.DeployMode)deployMode;

                    if (file.DeployMode == RAFCompressed || file.DeployMode == RAFRaw)
                    {
                        // File has to be put in a RAF
                        if (!currentRAFs.Any() || currentArchiveVersion != remoteAsset.StringVersion)
                        {
                            if (currentRAFs.Any())
                                currentRAFs[0]?.Save();
                            foreach (RAF raf in currentRAFs)
                                raf.Dispose();
                            currentRAFs.Clear();
                            currentArchiveVersion = remoteAsset.StringVersion;
                            Directory.CreateDirectory(String.Format("{0}/{1}", archivesFolder, remoteAsset.StringVersion));
                            foreach (string rafFile in Directory.EnumerateFiles(String.Format("{0}/{1}", archivesFolder, remoteAsset.StringVersion), "*.raf"))
                                currentRAFs.Add(new RAF(rafFile));
                            if (!currentRAFs.Any())
                                currentRAFs.Add(new RAF(String.Format("{0}/{1}/Archive_1.raf", archivesFolder, remoteAsset.StringVersion)));
                        }
                        // Check if file is already in a RAF and in good shape
                        bool fileAlreadyDownloaded = false;
                        foreach (RAF raf in currentRAFs)
                        {
                            RAFFileEntry fileEntry = raf.Files.Find(x => x.Path.Equals(remoteAsset.FileFullPath, StringComparison.InvariantCultureIgnoreCase));
                            if (fileEntry != null)
                            {
                                if (!Enumerable.SequenceEqual(file.MD5, CalculateMD5(fileEntry.GetContent(file.DeployMode == RAFCompressed))))
                                {
                                    raf.Files.Remove(fileEntry);
                                }
                                else
                                {
                                    fileAlreadyDownloaded = true;
                                    break;
                                }
                            }
                        }
                        if (!fileAlreadyDownloaded)
                            currentRAFs[0].AddFile(remoteAsset.FileFullPath, assetContent.GetAssetData(file.DeployMode == RAFCompressed), false);
                    }
                    else if (file.DeployMode == Managed)
                    {
                        // File will be in managedfiles folder
                        var filePath = String.Format("{0}/{1}/{2}", managedFilesFolder, remoteAsset.StringVersion, remoteAsset.FileFullPath);
                        if (!File.Exists(filePath) || !Enumerable.SequenceEqual(file.MD5, CalculateMD5(filePath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                            assetContent.WriteAssetToFile(filePath, false);
                        }
                    }
                    else if (file.DeployMode == Deployed0 || file.DeployMode == Deployed4)
                    {
                        // File will be in deploy folder
                        var deployPath = String.Format("{0}/{1}", deployFolder, remoteAsset.FileFullPath);
                        if (!File.Exists(deployPath) || !Enumerable.SequenceEqual(file.MD5, CalculateMD5(deployPath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(deployPath));
                            assetContent.WriteAssetToFile(deployPath, false);
                        }
                        if (solutionDeployFolder != null && file.DeployMode == Deployed4)
                        {
                            // File will also be in solution folder
                            var solutionPath = String.Format("{0}/{1}", solutionDeployFolder, remoteAsset.FileFullPath);
                            if (!File.Exists(solutionPath) || !Enumerable.SequenceEqual(file.MD5, CalculateMD5(solutionPath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(solutionPath));
                                File.Copy(deployPath, solutionPath);
                            }
                        }
                    }
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("■");
                }
                catch (Exception)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("■");
                }
                Console.ResetColor();
            }
            foreach (RAF raf in currentRAFs)
                raf.Dispose();
            releaseManifest.Write(releaseFolder + "/releasemanifest");
            File.Create(releaseFolder + "/S_OK").Close();
        }

        public void ListFiles(string projectName, string projectVersion, string filter = null, string filesRevision = null)
        {
            List<ReleaseManifestFileEntry> files = EnumerateFiles(projectName, ref projectVersion, filter, filesRevision);
            foreach (ReleaseManifestFileEntry file in files)
                Console.WriteLine("{0}/{1}", GetReleaseString(file.Version), file.GetFullPath());
        }

        public void DownloadFiles(string directory, string projectName, string projectVersion, string filter = null, string filesRevision = null, bool saveManifest = false)
        {
            var projectsURL = String.Format("{0}/projects/{1}", LeagueCDNBaseURL, projectName);
            List<ReleaseManifestFileEntry> files = EnumerateFiles(projectName, ref projectVersion, filter, filesRevision, saveManifest ? directory : null);
            Console.WriteLine("{0} files to download", files.Count);
            foreach (ReleaseManifestFileEntry file in files)
            {
                string fileFullPath = file.GetFullPath();
                string fileOutputPath = String.Format("{0}/{1}/releases/{2}/files/{3}", directory, projectName, projectVersion, fileFullPath);
                DownloadFile(file, fileOutputPath, fileFullPath, projectsURL);
            }
        }

        public void RangeDownloadFiles(string directory, string projectName, bool ignoreOlderFiles = false, string filter = null, string startRevision = null, string endRevision = null, bool saveManifest = false)
        {
            var projectsURL = String.Format("{0}/projects/{1}", LeagueCDNBaseURL, projectName);

            List<string> releases = GetProjectReleases(projectName);
            uint startRevisionValue = startRevision == null ? 0 : GetReleaseValue(startRevision);
            uint endRevisionValue = endRevision == null ? GetReleaseValue(releases[0]) : GetReleaseValue(endRevision);

            // Check if specified releases exist
            startRevisionValue = Math.Max(startRevisionValue, GetReleaseValue(releases.Last()));
            endRevisionValue = Math.Min(endRevisionValue, GetReleaseValue(releases[0]));

            for (uint r = startRevisionValue; r <= endRevisionValue; r++)
            {
                string releaseString = GetReleaseString(r);
                Console.WriteLine("Retrieving files list for revision " + releaseString);
                List<ReleaseManifestFileEntry> files = EnumerateFiles(projectName, ref releaseString, filter, (r != startRevisionValue || ignoreOlderFiles) ? releaseString : null, saveManifest ? directory : null);
                foreach (ReleaseManifestFileEntry fileEntry in files)
                {
                    string fileFullPath = fileEntry.GetFullPath();
                    string fileOutputPath = String.Format("{0}/{1}/releases/{2}/files/{3}", directory, projectName, GetReleaseString(fileEntry.Version), fileFullPath);
                    DownloadFile(fileEntry, fileOutputPath, fileFullPath, projectsURL);
                }
            }
        }

        private void DownloadFile(ReleaseManifestFileEntry file, string fileOutputPath, string fileFullPath, string projectsURL)
        {
            // Check if file is already downloaded and is in perfect condition.
            if (File.Exists(fileOutputPath) && Enumerable.SequenceEqual(file.MD5, CalculateMD5(fileOutputPath)))
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(fileOutputPath));
            var remoteAsset = new RemoteAsset(file, projectsURL, webClient);
            Console.Write("■ Downloading {0}/{1}", remoteAsset.StringVersion, fileFullPath);
            try
            {
                remoteAsset.AssetContent.WriteAssetToFile(fileOutputPath, false);
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("■");
            }
            catch (Exception)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("■");
            }
            Console.ResetColor();
        }

        private List<ReleaseManifestFileEntry> EnumerateFiles(string projectName, ref string projectVersion, string filter = null, string filesRevision = null, string manifestOutputFolder = null)
        {
            if (String.Equals(projectVersion, Constants.LatestVersionString))
                projectVersion = GetLatestProjectRelease(projectName);

            if (String.Equals(filesRevision, Constants.LatestVersionString))
                filesRevision = projectVersion;

            var releaseURL = String.Format("{0}/projects/{1}/releases/{2}", LeagueCDNBaseURL, projectName, projectVersion);
            byte[] manifestData = webClient.DownloadData(releaseURL + "/releasemanifest");
            var releaseManifest = new ReleaseManifestFile(new MemoryStream(manifestData));

            if (manifestOutputFolder != null)
            {
                var manifestPath = String.Format("{0}/{1}/releases/{2}/releasemanifest", manifestOutputFolder, projectName, projectVersion);
                Directory.CreateDirectory(Path.GetDirectoryName(manifestPath));
                releaseManifest.Write(manifestPath);
            }

            var files = new List<ReleaseManifestFileEntry>();
            EnumerateManifestFolderFiles(releaseManifest.Project, files);
            if (filesRevision != null)
            {
                uint revisionValue = GetReleaseValue(filesRevision);
                files = files.FindAll(x => x.Version == revisionValue);
            }
            if (filter != null)
            {
                Regex regex = new Regex(filter, RegexOptions.IgnoreCase);
                files = files.FindAll(x => regex.IsMatch(x.GetFullPath()));
            }
            return files;
        }


        private List<string> GetSolutionReleases(string solutionName)
        {
            var releases = new List<string>();
            var releaseListingURL = String.Format("{0}/solutions/{1}/releases/releaselisting", LeagueCDNBaseURL, solutionName);
            using (var sr = new StringReader(webClient.DownloadString(releaseListingURL)))
            {
                string release;
                while ((release = sr.ReadLine()) != null)
                    releases.Add(release);
            }
            return releases;
        }

        private string GetLatestSolutionRelease(string solutionName)
        {
            return GetSolutionReleases(solutionName)[0];
        }

        private List<string> GetProjectReleases(string projectName)
        {
            var releases = new List<string>();
            var releaseListingURL = String.Format("{0}/projects/{1}/releases/releaselisting", LeagueCDNBaseURL, projectName);
            using (var sr = new StringReader(webClient.DownloadString(releaseListingURL)))
            {
                string release;
                while ((release = sr.ReadLine()) != null)
                    releases.Add(release);
            }
            return releases;
        }

        private string GetLatestProjectRelease(string projectName)
        {
            return GetProjectReleases(projectName)[0];
        }
    }
}