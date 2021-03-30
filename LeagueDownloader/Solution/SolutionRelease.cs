using System;
using System.Text;
using System.Net;
using Fantome.Libraries.RADS.IO.SolutionManifest;
using static Fantome.Libraries.RADS.IO.ReleaseManifest.ReleaseManifestFile;

namespace LeagueDownloader.Solution
{
    public class SolutionRelease
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string LeagueCDNBaseURL { get; private set; }
        public SolutionManifest SolutionManifest { get; private set; }

        public SolutionRelease(string name, string version, string leagueCDNBaseURL)
        {
            this.Name = name;
            this.Version = version;
            this.LeagueCDNBaseURL = leagueCDNBaseURL;
            this.SolutionManifest = GetSolutionManifest();
        }

        private SolutionManifest GetSolutionManifest()
        {
            byte[] solutionManifestData = new WebClient().DownloadData(String.Format("{0}/solutions/{1}/releases/{2}/solutionmanifest", this.LeagueCDNBaseURL, this.Name, this.Version));
            return new SolutionManifest(Encoding.ASCII.GetString(solutionManifestData).Split(new[] { "\r\n" }, StringSplitOptions.None));
        }

        public SolutionReleaseInstallation CreateInstallation(string installDirectory, string localization, DeployMode? deployMode)
        {
            SolutionManifestLocalizedEntry localizedEntry = this.SolutionManifest.LocalizedEntries.Find(x => x.Name.Equals(localization, StringComparison.InvariantCultureIgnoreCase));
            if (localizedEntry != null)
            {
                return new SolutionReleaseInstallation(this, localizedEntry, installDirectory, deployMode);
            }
            else
            {
                throw new Exception("The specified localized entry was not found.");
            }
        }
    }
}
