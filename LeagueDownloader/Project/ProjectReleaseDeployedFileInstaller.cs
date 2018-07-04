using LeagueDownloader.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static Fantome.Libraries.League.IO.ReleaseManifest.ReleaseManifestFile.DeployMode;

namespace LeagueDownloader.Project
{
    public class ProjectReleaseDeployedFileInstaller : ProjectReleaseFileInstaller
    {
        public string DeployDirectory { get; private set; }
        public string SolutionDirectory { get; private set; }

        public ProjectReleaseDeployedFileInstaller(string installationDirectory, string releaseDirectory, string solutionName, string solutionVersion)
        {
            this.DeployDirectory = releaseDirectory + "/deploy";
            Directory.CreateDirectory(this.DeployDirectory);
            if (solutionName != null && solutionVersion != null)
            {
                this.SolutionDirectory = String.Format("{0}/RADS/solutions/{1}/releases/{2}/deploy", installationDirectory, solutionName, solutionVersion);
                Directory.CreateDirectory(this.SolutionDirectory);
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
            if (this.SolutionDirectory != null && remoteAsset.FileEntry.DeployMode == Deployed4)
            {
                // File will also be in solution folder
                string solutionPath = String.Format("{0}/{1}", this.SolutionDirectory, remoteAsset.FileFullPath);
                if (!File.Exists(solutionPath) || !Enumerable.SequenceEqual(remoteAsset.FileEntry.MD5, Utilities.CalculateMD5(solutionPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(solutionPath));
                    File.Copy(deployPath, solutionPath);
                }
            }
        }
    }
}