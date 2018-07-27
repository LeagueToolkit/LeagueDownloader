using Fantome.Libraries.League.IO.ReleaseManifest;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LeagueDownloader
{
    public class Utilities
    {
        public static byte[] DecompressZlib(byte[] inputData)
        {
            byte[] data = new byte[inputData.Length - 6];
            using (MemoryStream ms = new MemoryStream(inputData))
            {
                ms.Seek(2, SeekOrigin.Begin);
                ms.Read(data, 0, data.Length);
            }
            return Inflate(data);
        }

        public static void DecompressZlib(string compressedFilePath, string outputPath)
        {
            using (FileStream fs = File.OpenRead(compressedFilePath))
            {
                using (SubStream ss = new SubStream(fs, 2, fs.Length - 6))
                {
                    Inflate(ss, outputPath);
                }
            }
        }

        public static void Inflate(Stream compressedStream, string outputPath)
        {
            using (FileStream rawStream = new FileStream(outputPath, FileMode.Create))
            {
                using (DeflateStream decompressionStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(rawStream);
                }
            }
        }

        public static byte[] Inflate(byte[] compressedData)
        {
            byte[] decompressedData = null;
            using (MemoryStream compressedStream = new MemoryStream(compressedData))
            {
                using (MemoryStream rawStream = new MemoryStream())
                {
                    using (DeflateStream decompressionStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(rawStream);
                    }
                    decompressedData = rawStream.ToArray();
                }
            }
            return decompressedData;
        }

        public static string GetReleaseString(uint releaseValue)
        {
            return String.Format("{0}.{1}.{2}.{3}", (releaseValue & 0xFF000000) >> 24, (releaseValue & 0x00FF0000) >> 16, (releaseValue & 0x0000FF00) >> 8, releaseValue & 0x000000FF);
        }

        public static uint GetReleaseValue(string releaseString)
        {
            string[] releaseValues = releaseString.Split('.');
            return (uint)((Byte.Parse(releaseValues[0]) << 24) | (Byte.Parse(releaseValues[1]) << 16) | (Byte.Parse(releaseValues[2]) << 8) | Byte.Parse(releaseValues[3]));
        }

        public static byte[] CalculateMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        public static byte[] CalculateMD5(byte[] fileData)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream(fileData))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}
