using System;
using System.IO;
using System.Net;
using Ionic.Zlib;

namespace LeagueDownloader.Content
{
    public class AssetContent
    {
        public bool IsCompressed { get; private set; }
        public string RemoteURL { get; private set; }
        public byte[] AssetData { get; private set; }

        public AssetContent(bool isCompressed, string remoteURL)
        {
            this.IsCompressed = isCompressed;
            this.RemoteURL = remoteURL;
        }

        public byte[] GetAssetData(bool zlibCompressed)
        {
            if (this.AssetData == null)
            {
                throw new Exception("Asset not downloaded.");
            }

            if (this.IsCompressed == zlibCompressed)
            {
                return this.AssetData;
            }
            else
            {
                using (MemoryStream ms = new MemoryStream(this.AssetData))
                {
                    using (MemoryStream msOut = new MemoryStream())
                    {
                        using (ZlibStream zlibStream = new ZlibStream(ms, this.IsCompressed ? CompressionMode.Decompress : CompressionMode.Compress))
                        {
                            zlibStream.CopyTo(msOut);
                            return msOut.ToArray();
                        }
                    }

                }
            }
        }

        public void WriteAssetToFile(string filePath, bool zlibCompressed)
        {
            if (this.AssetData == null)
            {
                throw new Exception("Asset not downloaded.");
            }

            using (MemoryStream ms = new MemoryStream(this.AssetData))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    if (this.IsCompressed == zlibCompressed)
                    {
                        ms.CopyTo(fs);
                    }
                    else
                    {
                        using (ZlibStream zlibStream = new ZlibStream(ms, this.IsCompressed ? CompressionMode.Decompress : CompressionMode.Compress))
                        {
                            zlibStream.CopyTo(fs);
                        }
                    }
                }
            }
        }

        public void DownloadAssetData()
        {
            using (WebClient webClient = new WebClient())
            {
                this.AssetData = webClient.DownloadData(this.RemoteURL);
            }
        }

        public void FlushAssetData()
        {
            this.AssetData = null;
        }
    }
}
