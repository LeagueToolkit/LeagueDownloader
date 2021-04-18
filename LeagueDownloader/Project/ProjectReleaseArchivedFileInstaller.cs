using System.IO;
using LeagueDownloader.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Fantome.Libraries.RADS.IO.RiotArchive;
using static Fantome.Libraries.RADS.IO.ReleaseManifest.ReleaseManifestFile.DeployMode;

namespace LeagueDownloader.Project
{
    public class ProjectReleaseArchivedFileInstaller : ProjectReleaseFileInstaller, IDisposable
    {
        public string ArchivesDirectory { get; private set; }

        private readonly List<ArchiveVersionDirectory> rafDirectories = new List<ArchiveVersionDirectory>();

        public ProjectReleaseArchivedFileInstaller(string installationDirectory)
        {
            this.ArchivesDirectory = installationDirectory + "/filearchives";
            Directory.CreateDirectory(this.ArchivesDirectory);
        }

        private ArchiveVersionDirectory GetArchiveDirectory(string version)
        {
            ArchiveVersionDirectory foundDirectory = this.rafDirectories.Find(x => x.Version.Equals(version));
            if (foundDirectory == null)
            {
                foundDirectory = new ArchiveVersionDirectory(version, String.Format("{0}/{1}", this.ArchivesDirectory, version));
                this.rafDirectories.Add(foundDirectory);
            }
            return foundDirectory;
        }

        public override void InstallFile(RemoteAsset remoteAsset)
        {
            ArchiveVersionDirectory foundDirectory = this.GetArchiveDirectory(remoteAsset.StringVersion);
            foreach (RAF raf in foundDirectory.Archives)
            {
                RAFFileEntry fileEntry = raf.Files.Find(x => x.Path.Equals(remoteAsset.FileFullPath, StringComparison.InvariantCultureIgnoreCase));
                if (fileEntry != null)
                {
                    raf.Files.Remove(fileEntry);
                }
            }

            foundDirectory.Archives[0].AddFile(remoteAsset.FileFullPath, remoteAsset.AssetContent.GetAssetData(remoteAsset.FileEntry.DeployMode == RAFCompressed), false);
            foundDirectory.Archives[0].Save();
        }

        public override bool IsFileInstalled(RemoteAsset remoteAsset)
        {
            ArchiveVersionDirectory foundDirectory = this.GetArchiveDirectory(remoteAsset.StringVersion);

            foreach (RAF raf in foundDirectory.Archives)
            {
                RAFFileEntry fileEntry = raf.Files.Find(x => x.Path.Equals(remoteAsset.FileFullPath, StringComparison.InvariantCultureIgnoreCase));
                if (fileEntry != null)
                {
                    try
                    {
                        byte[] fileEntryContent = fileEntry.GetContent(remoteAsset.FileEntry.DeployMode == RAFCompressed);
                        return Enumerable.SequenceEqual(remoteAsset.FileEntry.MD5, Utilities.CalculateMD5(fileEntryContent));
                    }
                    catch (Exception) { return false; }
                }
            }

            return false;
        }

        public void Dispose()
        {
            foreach (ArchiveVersionDirectory rafDirectory in this.rafDirectories)
                foreach (RAF raf in rafDirectory.Archives)
                    raf.Dispose();
        }

        private class ArchiveVersionDirectory
        {
            public string Version { get; private set; }
            public string DirectoryPath { get; private set; }
            public List<RAF> Archives { get; private set; } = new List<RAF>();

            public ArchiveVersionDirectory(string version, string directoryPath)
            {
                this.Version = version;
                this.DirectoryPath = directoryPath;

                Directory.CreateDirectory(this.DirectoryPath);
                foreach (string rafFile in Directory.EnumerateFiles(this.DirectoryPath, "*.raf"))
                    this.Archives.Add(new RAF(rafFile));

                if (!this.Archives.Any())
                    this.Archives.Add(new RAF(this.DirectoryPath + "/Archive_1.raf"));
            }
        }
    }
}