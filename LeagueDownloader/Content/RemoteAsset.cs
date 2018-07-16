using Fantome.Libraries.League.IO.ReleaseManifest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        private WebClient _webClient;

        public RemoteAsset(ReleaseManifestFileEntry fileEntry, string projectURL, WebClient webClient)
        {
            this._webClient = webClient;
            this.FileEntry = fileEntry;
            this.StringVersion = Utilities.GetReleaseString(fileEntry.Version);
            this.FileFullPath = fileEntry.GetFullPath();
            this.IsCompressed = this.FileEntry.SizeCompressed > 0;
            this.RemoteURL = Uri.EscapeUriString(String.Format("{0}/releases/{1}/files/{2}", projectURL, this.StringVersion, this.FileFullPath));
            if (this.IsCompressed)
                this.RemoteURL += ".compressed";

            // Store larger files on file system.
            if (this.FileEntry.SizeRaw / (1048576) < 50)
            {
                this.AssetContent = new DataAssetContent(_webClient, this.IsCompressed, this.RemoteURL);
            }
            else
            {
                this.AssetContent = new FileAssetContent(_webClient, this.IsCompressed, this.RemoteURL);
            }
        }
    }
}