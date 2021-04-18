using System;
using System.Linq;
using System.IO;
using LeagueDownloader.Content;

namespace LeagueDownloader.Project
{
    public class ProjectReleaseManagedFileInstaller : ProjectReleaseFileInstaller
    {
        public string ManagedFilesDirectory { get; private set; }

        public ProjectReleaseManagedFileInstaller(string installationDirectory)
        {
            this.ManagedFilesDirectory = installationDirectory + "/managedfiles";
            Directory.CreateDirectory(this.ManagedFilesDirectory);
        }

        private string GetFilePath(RemoteAsset remoteAsset)
        {
            return String.Format("{0}/{1}/{2}", this.ManagedFilesDirectory, remoteAsset.StringVersion, remoteAsset.FileFullPath);
        }

        public override void InstallFile(RemoteAsset remoteAsset)
        {
            string filePath = GetFilePath(remoteAsset);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            remoteAsset.AssetContent.WriteAssetToFile(filePath, false);
        }

        public override bool IsFileInstalled(RemoteAsset remoteAsset)
        {
            string filePath = GetFilePath(remoteAsset);
            if (!File.Exists(filePath) || !Enumerable.SequenceEqual(remoteAsset.FileEntry.MD5, Utilities.CalculateMD5(filePath)))
            {
                return false;
            }
            return true;
        }
    }
}