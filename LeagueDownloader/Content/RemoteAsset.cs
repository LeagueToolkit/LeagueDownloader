using Fantome.Libraries.RADS.IO.ReleaseManifest;
using System;

namespace LeagueDownloader.Content
{
    public class RemoteAsset
    {
        public ReleaseManifestFileEntry FileEntry { get; private set; }
        public string RemoteURL { get; private set; }
        public string StringVersion { get; private set; }
        public string FileFullPath { get; private set; }
        public bool IsCompressed { get; private set; }
        public AssetContent AssetContent { get; private set; }

        public RemoteAsset(ReleaseManifestFileEntry fileEntry, string projectURL)
        {
            this.FileEntry = fileEntry;
            this.StringVersion = Utilities.GetReleaseString(fileEntry.Version);
            this.FileFullPath = fileEntry.GetFullPath();
            this.IsCompressed = this.FileEntry.SizeCompressed > 0;
            this.RemoteURL = Uri.EscapeUriString(String.Format("{0}/releases/{1}/files/{2}", projectURL, this.StringVersion, this.FileFullPath));
            if (this.IsCompressed)
                this.RemoteURL += ".compressed";
            this.AssetContent = new AssetContent(this.IsCompressed, this.RemoteURL);
        }
    }
}