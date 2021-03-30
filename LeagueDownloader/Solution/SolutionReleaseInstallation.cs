using Fantome.Libraries.RADS.IO.SolutionManifest;
using System;
using System.IO;
using static Fantome.Libraries.RADS.IO.ReleaseManifest.ReleaseManifestFile;

namespace LeagueDownloader.Solution
{
    public class SolutionReleaseInstallation : IDisposable
    {
        public DeployMode? OverrideDeployMode { get; private set; }
        public SolutionRelease SolutionRelease { get; private set; }
        public SolutionManifestLocalizedEntry LocalizedEntry { get; private set; }
        public string InstallationDirectory { get; private set; }

        public SolutionReleaseInstallation(SolutionRelease solutionRelease, SolutionManifestLocalizedEntry localizedEntry, string installationDirectory, DeployMode? deployMode)
        {
            this.OverrideDeployMode = deployMode;
            this.SolutionRelease = solutionRelease;
            this.LocalizedEntry = localizedEntry;

            if (OverrideDeployMode != DeployMode.Garena)
            {
                this.InstallationDirectory = String.Format("{0}/RADS/solutions/{1}/releases/{2}", installationDirectory, this.SolutionRelease.Name, this.SolutionRelease.Version);
                Directory.CreateDirectory(this.InstallationDirectory);

                // Write the solution manifest
                this.SolutionRelease.SolutionManifest.Write(this.InstallationDirectory + "/solutionmanifest");

                // Create & Write a configurationmanifest
                ConfigurationManifest configurationManifest = new ConfigurationManifest(this.LocalizedEntry);
                configurationManifest.Write(this.InstallationDirectory + "/configurationmanifest");
            }
        }

        public void Dispose()
        {
            if (OverrideDeployMode != DeployMode.Garena)
            {
                File.Create(this.InstallationDirectory + "/S_OK").Close();
            }
        }
    }
}
