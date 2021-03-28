using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Fantome.Libraries.RADS.IO.ReleaseManifest;
using static LeagueDownloader.Utilities;
using LeagueDownloader.Solution;
using LeagueDownloader.Project;
using LeagueDownloader.Content;
using Fantome.Libraries.RADS.IO.SolutionManifest;

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
            if (String.Equals(solutionVersion, Constants.LatestVersionString))
                solutionVersion = GetLatestSolutionRelease(solutionName);

            Console.WriteLine("Downloading solution manifest for release {0}", solutionVersion);
            SolutionRelease solutionRelease = new SolutionRelease(solutionName, solutionVersion, this.LeagueCDNBaseURL);

            using (SolutionReleaseInstallation solutionReleaseInstallation = solutionRelease.CreateInstallation(directory, localization))
            {
                foreach (SolutionManifestProjectEntry project in solutionReleaseInstallation.LocalizedEntry.Projects)
                    InstallProject(directory, project.Name, project.Version, deployMode, solutionName, solutionVersion);
            }
        }

        public void InstallProject(string directory, string projectName, string projectVersion, uint? deployMode, string solutionName = null, string solutionVersion = null)
        {
            if (String.Equals(projectVersion, Constants.LatestVersionString))
                projectVersion = GetLatestProjectRelease(projectName);

            Console.WriteLine("Downloading manifest for project {0}, release {1}...", projectName, projectVersion);
            ProjectRelease projectRelease = new ProjectRelease(projectName, projectVersion, this.LeagueCDNBaseURL);

            using (ProjectReleaseInstallation installation = new ProjectReleaseInstallation(projectRelease, directory, solutionName, solutionVersion))
            {
                foreach (ReleaseManifestFileEntry file in projectRelease.EnumerateFiles())
                {
                    RemoteAsset remoteAsset = projectRelease.GetRemoteAsset(file);
                    Console.Write("■ Downloading {0}/{1}", remoteAsset.StringVersion, remoteAsset.FileFullPath);
                    try
                    {
                        installation.InstallFile(remoteAsset);
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    catch (Exception)
                    {                        
                        Console.ForegroundColor = ConsoleColor.Red;                        
                    }
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.WriteLine("■");
                    Console.ResetColor();
                }
            }
        }

        public void ListFiles(string projectName, string projectVersion, string filter = null, string filesRevision = null)
        {
            if (String.Equals(projectVersion, Constants.LatestVersionString))
                projectVersion = GetLatestProjectRelease(projectName);

            ProjectRelease projectRelease = new ProjectRelease(projectName, projectVersion, this.LeagueCDNBaseURL);

            List<ReleaseManifestFileEntry> files = FilterFiles(projectRelease.EnumerateFiles(), filter, filesRevision);
            foreach (ReleaseManifestFileEntry file in files)
                Console.WriteLine("{0}/{1}", GetReleaseString(file.Version), file.GetFullPath());
        }

        public void DownloadFiles(string directory, string projectName, string projectVersion, string filter = null, string filesRevision = null, bool saveManifest = false)
        {
            if (String.Equals(projectVersion, Constants.LatestVersionString))
                projectVersion = GetLatestProjectRelease(projectName);

            ProjectRelease projectRelease = new ProjectRelease(projectName, projectVersion, this.LeagueCDNBaseURL);
            if (saveManifest)
            {
                string manifestPath = String.Format("{0}/{1}/releases/{2}/releasemanifest", directory, projectName, projectVersion);
                Directory.CreateDirectory(Path.GetDirectoryName(manifestPath));
                projectRelease.ReleaseManifest.Write(manifestPath);
            }

            List<ReleaseManifestFileEntry> files = FilterFiles(projectRelease.EnumerateFiles(), filter, filesRevision);
            Console.WriteLine("{0} files to download", files.Count);
            foreach (ReleaseManifestFileEntry file in files)
            {
                string fileFullPath = file.GetFullPath();
                string fileOutputPath = String.Format("{0}/{1}/releases/{2}/files/{3}", directory, projectName, projectVersion, fileFullPath);
                DownloadFile(projectRelease, file, fileOutputPath, fileFullPath);
            }
        }

        public void RangeDownloadFiles(string directory, string projectName, bool ignoreOlderFiles = false, string filter = null, string startRevision = null, string endRevision = null, bool saveManifest = false)
        {
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
                ProjectRelease projectRelease;
                try
                {
                    projectRelease = new ProjectRelease(projectName, releaseString, this.LeagueCDNBaseURL);
                } catch (Exception)
                {
                    Console.WriteLine("Error getting manifest for release " + releaseString);
                    continue;
                }
                
                if (saveManifest)
                {
                    string manifestPath = String.Format("{0}/{1}/releases/{2}/releasemanifest", directory, projectName, releaseString);
                    Directory.CreateDirectory(Path.GetDirectoryName(manifestPath));
                    projectRelease.ReleaseManifest.Write(manifestPath);
                }

                List<ReleaseManifestFileEntry> files = FilterFiles(projectRelease.EnumerateFiles(), filter, (r != startRevisionValue || ignoreOlderFiles) ? releaseString : null);
                foreach (ReleaseManifestFileEntry fileEntry in files)
                {
                    string fileFullPath = fileEntry.GetFullPath();
                    string fileOutputPath = String.Format("{0}/{1}/releases/{2}/files/{3}", directory, projectName, GetReleaseString(fileEntry.Version), fileFullPath);
                    DownloadFile(projectRelease, fileEntry, fileOutputPath, fileFullPath);
                }
            }
        }

        private void DownloadFile(ProjectRelease projectRelease, ReleaseManifestFileEntry file, string fileOutputPath, string fileFullPath)
        {
            // Check if file is already downloaded and is in perfect condition.
            if (File.Exists(fileOutputPath) && Enumerable.SequenceEqual(file.MD5, CalculateMD5(fileOutputPath)))
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(fileOutputPath));
            RemoteAsset remoteAsset = projectRelease.GetRemoteAsset(file);
            Console.Write("■ Downloading {0}/{1}", remoteAsset.StringVersion, fileFullPath);
            try
            {
                remoteAsset.AssetContent.WriteAssetToFile(fileOutputPath, false);                
                Console.ForegroundColor = ConsoleColor.Green;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;                
            }
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine("■");
            Console.ResetColor();
        }

        private List<ReleaseManifestFileEntry> FilterFiles(List<ReleaseManifestFileEntry> fullList, string filter = null, string filesRevision = null)
        {
            List<ReleaseManifestFileEntry> files = new List<ReleaseManifestFileEntry>(fullList);
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
            List<string> releases = new List<string>();
            string releaseListingURL = String.Format("{0}/solutions/{1}/releases/releaselisting", LeagueCDNBaseURL, solutionName);
            using (StringReader sr = new StringReader(webClient.DownloadString(releaseListingURL)))
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
            List<string> releases = new List<string>();
            string releaseListingURL = String.Format("{0}/projects/{1}/releases/releaselisting", LeagueCDNBaseURL, projectName);
            using (StringReader sr = new StringReader(webClient.DownloadString(releaseListingURL)))
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