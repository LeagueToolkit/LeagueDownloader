using LeagueDownloader.Content;

namespace LeagueDownloader.Project
{
    public abstract class ProjectReleaseFileInstaller
    {
        public abstract void InstallFile(RemoteAsset remoteAsset);

        public abstract bool IsFileInstalled(RemoteAsset remoteAsset);
    }
}