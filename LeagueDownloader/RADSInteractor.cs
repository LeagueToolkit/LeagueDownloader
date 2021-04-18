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

            using (DownloadSession downloadSession = new DownloadSession(String.Format("Installing solution {0} (version {1})", solutionName, solutionVersion)))
            {
                SolutionRelease solutionRelease = downloadSession.GetSolutionRelease(solutionName, solutionVersion, this.LeagueCDNBaseURL);

                using (SolutionReleaseInstallation solutionReleaseInstallation = solutionRelease.CreateInstallation(directory, localization))
                {
                    foreach (SolutionManifestProjectEntry project in solutionReleaseInstallation.LocalizedEntry.Projects)
                    {
                        InstallProject(downloadSession, directory, project.Name, project.Version, deployMode, solutionName, solutionVersion);
                    }
                }
            }
        }

        public void InstallProject(DownloadSession parentDownloadSession, string directory, string projectName, string projectVersion, uint? deployMode, string solutionName = null, string solutionVersion = null)
        {
            if (String.Equals(projectVersion, Constants.LatestVersionString))
            {
                projectVersion = GetLatestProjectRelease(projectName);
            }

            DownloadSession downloadSession = parentDownloadSession != null ? parentDownloadSession : new DownloadSession(String.Format("Installing project {0} (version {1})", projectName, projectVersion));
            downloadSession.ResetRegisteredFiles();

            ProjectRelease projectRelease = downloadSession.GetProjectRelease(projectName, projectVersion, this.LeagueCDNBaseURL);
            if (deployMode != null)
            {
                projectRelease.EnumerateFiles().ForEach(x => x.DeployMode = (ReleaseManifestFile.DeployMode)deployMode);
            }

            using (ProjectReleaseInstallation installation = new ProjectReleaseInstallation(projectRelease, directory, solutionName, solutionVersion))
            {
                foreach (ReleaseManifestFileEntry file in projectRelease.EnumerateFiles())
                {
                    RemoteAsset remoteAsset = projectRelease.GetRemoteAsset(file);
                    downloadSession.RegisterFile(remoteAsset, installation.GetFileInstaller(remoteAsset));
                }
                downloadSession.DoWork();
                
                if (parentDownloadSession == null)
                {
                    downloadSession.Dispose();
                }
            }
        }

        public void ListFiles(string projectName, string projectVersion, string filter = null, string filesRevision = null)
        {
            if (String.Equals(projectVersion, Constants.LatestVersionString))
            {
                projectVersion = GetLatestProjectRelease(projectName);
            }                

            ProjectRelease projectRelease = new ProjectRelease(projectName, projectVersion, this.LeagueCDNBaseURL);

            List<ReleaseManifestFileEntry> files = FilterFiles(projectRelease.EnumerateFiles(), filter, filesRevision);
            foreach (ReleaseManifestFileEntry file in files)
                Console.WriteLine("{0}/{1}", GetReleaseString(file.Version), file.GetFullPath());
        }

        public void DownloadFiles(string directory, string projectName, string projectVersion, string filter = null, string filesRevision = null, bool saveManifest = false)
        {
            if (String.Equals(projectVersion, Constants.LatestVersionString))
                projectVersion = GetLatestProjectRelease(projectName);

            using (DownloadSession downloadSession = new DownloadSession(String.Format("Downloading files from project {0} (version {1})", projectName, projectVersion)))
            {
                ProjectRelease projectRelease = downloadSession.GetProjectRelease(projectName, projectVersion, this.LeagueCDNBaseURL);
                if (saveManifest)
                {
                    string manifestPath = String.Format("{0}/{1}/releases/{2}/releasemanifest", directory, projectName, projectVersion);
                    Directory.CreateDirectory(Path.GetDirectoryName(manifestPath));
                    projectRelease.ReleaseManifest.Write(manifestPath);
                }

                List<ReleaseManifestFileEntry> files = FilterFiles(projectRelease.EnumerateFiles(), filter, filesRevision);

                ProjectReleaseFileInstaller fileInstaller = new ProjectReleaseSimpleFileInstaller(directory, projectRelease, false);
                foreach (ReleaseManifestFileEntry file in files)
                {
                    downloadSession.RegisterFile(projectRelease.GetRemoteAsset(file), fileInstaller);
                }
                downloadSession.DoWork();
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
            using (DownloadSession downloadSession = new DownloadSession(String.Format("Downloading files from project {0} (revisions {1} to {2})", projectName, GetReleaseString(startRevisionValue), GetReleaseString(endRevisionValue))))
            {
                for (uint r = startRevisionValue; r <= endRevisionValue; r++)
                {
                    downloadSession.ResetRegisteredFiles();
                    string releaseString = GetReleaseString(r);
                    ProjectRelease projectRelease;
                    try
                    {
                        projectRelease = downloadSession.GetProjectRelease(projectName, releaseString, this.LeagueCDNBaseURL);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (saveManifest)
                    {
                        string manifestPath = String.Format("{0}/{1}/releases/{2}/releasemanifest", directory, projectName, releaseString);
                        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath));
                        projectRelease.ReleaseManifest.Write(manifestPath);
                    }

                    List<ReleaseManifestFileEntry> files = FilterFiles(projectRelease.EnumerateFiles(), filter, (r != startRevisionValue || ignoreOlderFiles) ? releaseString : null);
                    if (files.Count > 0)
                    {

                        ProjectReleaseFileInstaller fileInstaller = new ProjectReleaseSimpleFileInstaller(directory, projectRelease, true);
                        foreach (ReleaseManifestFileEntry file in files)
                        {
                            downloadSession.RegisterFile(projectRelease.GetRemoteAsset(file), fileInstaller);
                        }
                        downloadSession.DoWork();
                    }
                }
            }
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