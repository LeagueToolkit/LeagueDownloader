using Fantome.Libraries.RADS.IO.SolutionManifest;
using System;
using System.IO;

namespace LeagueDownloader.Solution
{
    public class SolutionReleaseInstallation : IDisposable
    {
        public SolutionRelease SolutionRelease { get; private set; }
        public SolutionManifestLocalizedEntry LocalizedEntry { get; private set; }
        public string InstallationDirectory { get; private set; }

        public SolutionReleaseInstallation(SolutionRelease solutionRelease, SolutionManifestLocalizedEntry localizedEntry, string installationDirectory)
        {
            this.SolutionRelease = solutionRelease;
            this.LocalizedEntry = localizedEntry;

            this.InstallationDirectory = String.Format("{0}/RADS/solutions/{1}/releases/{2}", installationDirectory, this.SolutionRelease.Name, this.SolutionRelease.Version);
            Directory.CreateDirectory(this.InstallationDirectory);

            // Write the solution manifest
            this.SolutionRelease.SolutionManifest.Write(this.InstallationDirectory + "/solutionmanifest");

            // Create & Write a configurationmanifest
            ConfigurationManifest configurationManifest = new ConfigurationManifest(this.LocalizedEntry);
            configurationManifest.Write(this.InstallationDirectory + "/configurationmanifest");
        }

        public void Dispose()
        {
            File.Create(this.InstallationDirectory + "/S_OK").Close();
        }
    }
}
