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

            this.SolutionReleaseDirectory = solutionReleaseDirectory;
            if (this.SolutionReleaseDirectory != null)
            {
                Directory.CreateDirectory(this.SolutionReleaseDirectory);
            }
        }

        public override void InstallFile(RemoteAsset remoteAsset)
        {
            string deployPath = String.Format("{0}/{1}", this.DeployDirectory, remoteAsset.FileFullPath);
            if (!File.Exists(deployPath) || !Enumerable.SequenceEqual(remoteAsset.FileEntry.MD5, Utilities.CalculateMD5(deployPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(deployPath));
                remoteAsset.AssetContent.WriteAssetToFile(deployPath, false);
            }
            if (this.SolutionReleaseDirectory != null && remoteAsset.FileEntry.DeployMode == Deployed4)
            {
                // File will also be in solution folder
                string solutionPath = String.Format("{0}/{1}", this.SolutionReleaseDirectory, remoteAsset.FileFullPath);
                if (!File.Exists(solutionPath) || !Enumerable.SequenceEqual(remoteAsset.FileEntry.MD5, Utilities.CalculateMD5(solutionPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(solutionPath));
                    File.Copy(deployPath, solutionPath);
                }
            }
        }
    }
}