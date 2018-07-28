using System.IO;
using System.Net;
using Ionic.Zlib;

namespace LeagueDownloader.Content
{
    public class AssetContent
    {
        public bool IsCompressed { get; private set; }
        public string RemoteURL { get; private set; }

        public AssetContent(bool isCompressed, string remoteURL)
        {
            this.IsCompressed = isCompressed;
            this.RemoteURL = remoteURL;
        }

        public byte[] GetAssetData(bool zlibCompressed)
        {
            WebResponse response = WebRequest.Create(this.RemoteURL).GetResponse();
            using (MemoryStream ms = new MemoryStream())
            {
                if (this.IsCompressed == zlibCompressed)
                {
                    response.GetResponseStream().CopyTo(ms);
                }
                else
                {
                    using (ZlibStream zlibStream = new ZlibStream(response.GetResponseStream(), this.IsCompressed ? CompressionMode.Decompress : CompressionMode.Compress))
                    {
                        zlibStream.CopyTo(ms);
                    }
                }
                return ms.ToArray();
            }
        }

        public void WriteAssetToFile(string filePath, bool zlibCompressed)
        {
            WebResponse response = WebRequest.Create(this.RemoteURL).GetResponse();
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                if (this.IsCompressed == zlibCompressed)
                {
                    response.GetResponseStream().CopyTo(fs);
                }
                else
                {
                    using (ZlibStream zlibStream = new ZlibStream(response.GetResponseStream(), this.IsCompressed ? CompressionMode.Decompress : CompressionMode.Compress))
                    {
                        zlibStream.CopyTo(fs);
                    }
                }
            }
        }
    }
}
