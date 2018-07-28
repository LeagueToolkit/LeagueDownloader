using System.IO;

namespace LeagueDownloader.Solution
{
    public class ConfigurationManifest
    {
        private const string Header = "RADS Configuration Manifest";

        public string ManifestVersion { get; private set; }
        public SolutionManifestLocalizedEntry LocalizedEntry { get; private set; }

        public ConfigurationManifest(SolutionManifestLocalizedEntry localizedEntry, string manifestVersion = "1.0.0.0")
        {
            this.ManifestVersion = manifestVersion;
            this.LocalizedEntry = localizedEntry;
        }

        public void Write(string fileLocation)
        {
            using (StreamWriter sw = new StreamWriter(fileLocation))
            {
                sw.WriteLine(Header);
                sw.WriteLine(this.ManifestVersion);
                sw.WriteLine(this.LocalizedEntry.Name);
                sw.WriteLine(this.LocalizedEntry.Projects.Count);
                foreach (SolutionManifestProjectEntry project in this.LocalizedEntry.Projects)
                    sw.WriteLine(project.Name);
            }
        }
    }
}
