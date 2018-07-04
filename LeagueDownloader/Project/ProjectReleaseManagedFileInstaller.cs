using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LeagueDownloader.Content;

namespace LeagueDownloader.Project
{
    public class ProjectReleaseManagedFileInstaller : ProjectReleaseFileInstaller
    {
        public string ManagedFilesDirectory { get; private set; }

        public ProjectReleaseManagedFileInstaller(string installationFolder)
        {
            this.ManagedFilesDirectory = installationFolder + "/managedfiles";
            Directory.CreateDirectory(this.ManagedFilesDirectory);
        }

        public override void InstallFile(RemoteAsset remoteAsset)
        {
            string filePath = String.Format("{0}/{1}/{2}", this.ManagedFilesDirectory, remoteAsset.StringVersion, remoteAsset.FileFullPath);
            if (!File.Exists(filePath) || !Enumerable.SequenceEqual(remoteAsset.FileEntry.MD5, Utilities.CalculateMD5(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                remoteAsset.AssetContent.WriteAssetToFile(filePath, false);
            }
        }
    }
}