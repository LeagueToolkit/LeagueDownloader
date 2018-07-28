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
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        public static byte[] CalculateMD5(byte[] fileData)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (MemoryStream stream = new MemoryStream(fileData))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}
