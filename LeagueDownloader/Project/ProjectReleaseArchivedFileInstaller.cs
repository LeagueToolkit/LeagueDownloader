using System.IO;
using LeagueDownloader.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Libraries.League.IO.RiotArchive;
using static Fantome.Libraries.League.IO.ReleaseManifest.ReleaseManifestFile.DeployMode;

namespace LeagueDownloader.Project
{
    public class ProjectReleaseArchivedFileInstaller : ProjectReleaseFileInstaller, IDisposable
    {
        public string ArchivesDirectory { get; private set; }

        private List<ArchiveVersionDirectory> rafDirectories = new List<ArchiveVersionDirectory>();

        public ProjectReleaseArchivedFileInstaller(string installationFolder)
        {
            this.ArchivesDirectory = installationFolder + "/filearchives";
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

            bool fileAlreadyDownloaded = false;
            foreach (RAF raf in foundDirectory.Archives)
            {
                RAFFileEntry fileEntry = raf.Files.Find(x => x.Path.Equals(remoteAsset.FileFullPath, StringComparison.InvariantCultureIgnoreCase));
                if (fileEntry != null)
                {
                    try
                    {
                        byte[] fileEntryContent = fileEntry.GetContent(remoteAsset.FileEntry.DeployMode == RAFCompressed);
                        if (Enumerable.SequenceEqual(remoteAsset.FileEntry.MD5, Utilities.CalculateMD5(fileEntryContent)))
                        {
                            fileAlreadyDownloaded = true;
                        }
                    }
                    catch (Exception ) { }

                    if (fileAlreadyDownloaded)
                    {
                        break;
                    }
                    else
                    {
                        raf.Files.Remove(fileEntry);
                    }
                }
            }
            if (!fileAlreadyDownloaded)
            {
                foundDirectory.Archives[0].AddFile(remoteAsset.FileFullPath, remoteAsset.AssetContent.GetAssetData(remoteAsset.FileEntry.DeployMode == RAFCompressed), false);
                foundDirectory.Archives[0].Save();
            }
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