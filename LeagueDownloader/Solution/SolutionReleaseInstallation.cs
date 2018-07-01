using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueDownloader.Solution
{
    public class SolutionReleaseInstallation
    {
        public SolutionRelease SolutionRelease { get; private set; }
        public SolutionManifestLocalizedEntry LocalizedEntry { get; private set; }
        public string InstallationDirectory { get; private set; }

        public SolutionReleaseInstallation(SolutionRelease solutionRelease, SolutionManifestLocalizedEntry localizedEntry, string installationDirectory)
        {
            SolutionRelease = solutionRelease;
            LocalizedEntry = localizedEntry;

            InstallationDirectory = String.Format("{0}/RADS/solutions/{1}/releases/{2}", InstallationDirectory, SolutionRelease.Name, SolutionRelease.Version);
            Directory.CreateDirectory(InstallationDirectory);

            // Write the solution manifest
            SolutionRelease.SolutionManifest.Write(InstallationDirectory + "/solutionmanifest");

            // Create & Write a configurationmanifest
            ConfigurationManifest configurationManifest = new ConfigurationManifest(LocalizedEntry);
            configurationManifest.Write(InstallationDirectory + "/configurationmanifest");
        }

        public void ValdateInstallation()
        {
            File.Create(InstallationDirectory + "/S_OK").Close();
        }
    }
}
