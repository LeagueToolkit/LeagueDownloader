using LeagueDownloader.Content;
using System;
using System.Linq;
using System.IO;
using static Fantome.Libraries.RADS.IO.ReleaseManifest.ReleaseManifestFile.DeployMode;

namespace LeagueDownloader.Project
{
    public class ProjectReleaseDeployedFileInstaller : ProjectReleaseFileInstaller
    {
        public string DeployDirectory { get; private set; }
        public string SolutionReleaseDirectory { get; private set; }

        public ProjectReleaseDeployedFileInstaller(string releaseDirectory, string solutionReleaseDirectory)
        {
            this.DeployDirectory = releaseDirectory + "/deploy";
            Directory.CreateDirectory(this.DeployDirectory);

            if (solutionReleaseDirectory != null)
            {
                this.SolutionReleaseDirectory = solutionReleaseDirectory + "/deploy";
                Directory.CreateDirectory(this.SolutionReleaseDirectory);
            }
        }

        private string GetFileDeployPath(RemoteAsset remoteAsset)
        {
            return String.Format("{0}/{1}", this.DeployDirectory, remoteAsset.FileFullPath);
        }

        private string GetFileSolutionPath(RemoteAsset remoteAsset)
        {
            return String.Format("{0}/{1}", this.SolutionReleaseDirectory, remoteAsset.FileFullPath);
        }

        public override void InstallFile(RemoteAsset remoteAsset)
        {
            string deployPath = GetFileDeployPath(remoteAsset);
            Directory.CreateDirectory(Path.GetDirectoryName(deployPath));
            remoteAsset.AssetContent.WriteAssetToFile(deployPath, false);

            if (this.SolutionReleaseDirectory != null && remoteAsset.FileEntry.DeployMode == Deployed4)
            {
                // File will also be in solution folder
                string solutionPath = GetFileSolutionPath(remoteAsset);
                Directory.CreateDirectory(Path.GetDirectoryName(solutionPath));
                File.Copy(deployPath, solutionPath);
            }
        }

        public override bool IsFileInstalled(RemoteAsset remoteAsset)
        {
            string deployPath = GetFileDeployPath(remoteAsset);
            if (!File.Exists(deployPath) || !Enumerable.SequenceEqual(remoteAsset.FileEntry.MD5, Utilities.CalculateMD5(deployPath)))
            {
                return false;
            }

            if (this.SolutionReleaseDirectory != null && remoteAsset.FileEntry.DeployMode == Deployed4)
            {
                string solutionPath = GetFileSolutionPath(remoteAsset);
                if (!File.Exists(solutionPath) || !Enumerable.SequenceEqual(remoteAsset.FileEntry.MD5, Utilities.CalculateMD5(solutionPath)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}