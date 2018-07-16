using Fantome.Libraries.League.IO.ReleaseManifest;
using LeagueDownloader.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueDownloader.Project
{
    public abstract class ProjectReleaseFileInstaller
    {
        public abstract void InstallFile(RemoteAsset remoteAsset);
    }
}