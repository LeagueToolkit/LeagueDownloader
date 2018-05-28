using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace LeagueDownloader.Content
{
    public abstract class AssetContent
    {
        public bool IsCompressed { get; private set; }
        public string RemoteURL { get; private set; }
        protected WebClient _webClient;

        public AssetContent(WebClient webClient, bool isCompressed, string remoteURL)
        {
            this._webClient = webClient;
            this.IsCompressed = isCompressed;
            this.RemoteURL = remoteURL;
        }

        public abstract byte[] GetAssetData(bool zlibCompressed);

        public abstract void WriteAssetToFile(string filePath, bool zlibCompressed);
    }

    public class FileAssetContent : AssetContent
    {
        public FileAssetContent(WebClient webClient, bool isCompressed, string remoteURL) : base(webClient, isCompressed, remoteURL) { }

        private FileInfo ResolveContent(bool zlibCompressed)
        {
            this._webClient.DownloadFile(this.RemoteURL, "temp/temp01");
            if (this.IsCompressed == zlibCompressed)
            {
                return new FileInfo("temp/temp01");
            }
            else if (this.IsCompressed)
            {
                Utilities.DecompressZlib("temp/temp01", "temp/temp02");
                File.Delete("temp/temp01");
                return new FileInfo("temp/temp02");
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override byte[] GetAssetData(bool zlibCompressed)
        {
            return File.ReadAllBytes(this.ResolveContent(zlibCompressed).FullName);
        }

        public override void WriteAssetToFile(string filePath, bool zlibCompressed)
        {
            this.ResolveContent(zlibCompressed).MoveTo(filePath);
        }
    }

    public class DataAssetContent : AssetContent
    {
        public DataAssetContent(WebClient webClient, bool isCompressed, string remoteURL) : base(webClient, isCompressed, remoteURL) { }

        private byte[] ResolveContent(bool zlibCompressed)
        {
            byte[] data = this._webClient.DownloadData(this.RemoteURL);
            if (this.IsCompressed == zlibCompressed)
            {
                return data;
            }
            else if (this.IsCompressed)
            {
                return Utilities.DecompressZlib(data);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override byte[] GetAssetData(bool zlibCompressed)
        {
            return this.ResolveContent(zlibCompressed);
        }

        public override void WriteAssetToFile(string filePath, bool zlibCompressed)
        {
            File.WriteAllBytes(filePath, this.ResolveContent(zlibCompressed));
        }
    }
}
