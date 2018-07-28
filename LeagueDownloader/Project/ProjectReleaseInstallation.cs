using System;
using System.IO;
using LeagueDownloader.Content;
using static Fantome.Libraries.RADS.IO.ReleaseManifest.ReleaseManifestFile.DeployMode;

namespace LeagueDownloader.Project
{
    public class ProjectReleaseInstallation : IDisposable
    {
        public ProjectRelease ProjectRelease { get; private set; }
        public string ProjectDirectory { get; private set; }
        public string ProjectReleaseDirectory { get; private set; }
        public ProjectReleaseArchivedFileInstaller ProjectReleaseArchivedFileInstaller { get; private set; }
        public ProjectReleaseManagedFileInstaller ProjectReleaseManagedFileInstaller { get; private set; }
        public ProjectReleaseDeployedFileInstaller ProjectReleaseDeployedFileInstaller { get; private set; }

        public ProjectReleaseInstallation(ProjectRelease projectRelease, string installationDirectory, string solutionName = null, string solutionVersion = null)
        {
            this.ProjectRelease = projectRelease;
            this.ProjectDirectory = String.Format("{0}/RADS/projects/{1}", installationDirectory, projectRelease.Name);
            this.ProjectReleaseDirectory = String.Format("{0}/releases/{1}", this.ProjectDirectory, projectRelease.Version);

            string solutionReleaseDirectory = null;
            if (solutionName != null && solutionVersion != null)
            {
                solutionReleaseDirectory = String.Format("{0}/RADS/solutions/{1}/releases/{2}", installationDirectory, solutionName, solutionVersion);
            }

            this.ProjectReleaseArchivedFileInstaller = new ProjectReleaseArchivedFileInstaller(this.ProjectDirectory);
            this.ProjectReleaseManagedFileInstaller = new ProjectReleaseManagedFileInstaller(this.ProjectDirectory);
            this.ProjectReleaseDeployedFileInstaller = new ProjectReleaseDeployedFileInstaller(this.ProjectReleaseDirectory, solutionReleaseDirectory);
        }

        public void InstallFile(RemoteAsset remoteAsset)
        {
            ProjectReleaseFileInstaller fileInstaller;
            switch (remoteAsset.FileEntry.DeployMode)
            {
                case RAFCompressed:
                case RAFRaw:
                    fileInstaller = this.ProjectReleaseArchivedFileInstaller;
                    break;
                case Managed:
                    fileInstaller = this.ProjectReleaseManagedFileInstaller;
                    break;
                case Deployed0:
                case Deployed4:
                    fileInstaller = this.ProjectReleaseDeployedFileInstaller;
                    break;
                default:
                    throw new Exception("Unknown deploy mode");
            }
            fileInstaller.InstallFile(remoteAsset);
        }

        public void Dispose()
        {
            this.ProjectReleaseArchivedFileInstaller.Dispose();
            File.Create(ProjectReleaseDirectory + "/S_OK").Close();
        }
    }
}
