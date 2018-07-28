using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueDownloader.Solution
{
    public class ConfigurationManifest
    {
        private const string Header = "RADS Configuration Manifest";

        public string ManifestVersion { get; private set; }
        public SolutionManifestLocalizedEntry LocalizedEntry { get; private set; }

        public ConfigurationManifest(SolutionManifestLocalizedEntry localizedEntry, string manifestVersion = "1.0.0.0")
        {
            ManifestVersion = manifestVersion;
            LocalizedEntry = localizedEntry;
        }

        public void Write(string fileLocation)
        {
            using (StreamWriter sw = new StreamWriter(fileLocation))
            {
                sw.WriteLine(Header);
                sw.WriteLine(ManifestVersion);
                sw.WriteLine(LocalizedEntry.Name);
                sw.WriteLine(LocalizedEntry.Projects.Count);
                foreach (SolutionManifestProjectEntry project in LocalizedEntry.Projects)
                    sw.WriteLine(project.Name);
            }
        }
    }
}
