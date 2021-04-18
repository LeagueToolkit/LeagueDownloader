using LeagueDownloader.Content;
using System;
using System.IO;
using System.Linq;

namespace LeagueDownloader.Project
{
    public class ProjectReleaseSimpleFileInstaller : ProjectReleaseFileInstaller
    {
        public string InstallationDirectory { get; private set; }

        public ProjectRelease ProjectRelease { get; private set; }

        public bool FileVersionInPath { get; private set; }

        public ProjectReleaseSimpleFileInstaller(string installationDirectory, ProjectRelease projectRelease, bool fileVersionInPath)
        {
            this.InstallationDirectory = installationDirectory;
            this.ProjectRelease = projectRelease;
            this.FileVersionInPath = fileVersionInPath;
        }

        private string GetOutputPath(RemoteAsset remoteAsset)
        {
            return String.Format("{0}/{1}/releases/{2}/files/{3}", InstallationDirectory, ProjectRelease.Name, FileVersionInPath ? remoteAsset.StringVersion : ProjectRelease.Version, remoteAsset.FileFullPath);
        }

        public override void InstallFile(RemoteAsset remoteAsset)
        {
            string outputPath = GetOutputPath(remoteAsset);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            remoteAsset.AssetContent.WriteAssetToFile(outputPath, false);
        }

        public override bool IsFileInstalled(RemoteAsset remoteAsset)
        {
            string outputPath = GetOutputPath(remoteAsset);
            return File.Exists(outputPath) && Enumerable.SequenceEqual(remoteAsset.FileEntry.MD5, Utilities.CalculateMD5(outputPath));
        }
    }
}
