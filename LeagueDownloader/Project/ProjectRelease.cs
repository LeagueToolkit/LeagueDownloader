﻿using System;
using System.Collections.Generic;
using Fantome.Libraries.RADS.IO.ReleaseManifest;
using System.Net;
using LeagueDownloader.Content;

namespace LeagueDownloader.Project
{
    public class ProjectRelease
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string LeagueCDNBaseURL { get; private set; }
        public ReleaseManifestFile ReleaseManifest { get; private set; }
        public string ProjectBaseURL { get; private set; }
        public string ReleaseBaseURL { get; private set; }
        public bool? ForceCompression { get; private set; }

        public ProjectRelease(string name, string version, string leagueCDNBaseURL, bool? forceCompression)
        {
            this.Name = name;
            this.Version = version;
            this.LeagueCDNBaseURL = leagueCDNBaseURL;
            this.ProjectBaseURL = String.Format("{0}/projects/{1}", this.LeagueCDNBaseURL, this.Name);
            this.ReleaseBaseURL = String.Format("{0}/releases/{1}", this.ProjectBaseURL, this.Version);
            this.ReleaseManifest = new ReleaseManifestFile(new WebClient().OpenRead(this.ReleaseBaseURL + "/releasemanifest"));
            this.ForceCompression = forceCompression;
        }

        public List<ReleaseManifestFileEntry> EnumerateFiles()
        {
            List<ReleaseManifestFileEntry> files = new List<ReleaseManifestFileEntry>();
            EnumerateManifestFolderFiles(this.ReleaseManifest.Project, files);
            files.Sort((x, y) => (x.Version.CompareTo(y.Version)));
            return files;
        }

        private static void EnumerateManifestFolderFiles(ReleaseManifestFolderEntry folder, List<ReleaseManifestFileEntry> currentList)
        {
            currentList.AddRange(folder.Files);
            foreach (ReleaseManifestFolderEntry subFolder in folder.Folders)
                EnumerateManifestFolderFiles(subFolder, currentList);
        }

        public RemoteAsset GetRemoteAsset(ReleaseManifestFileEntry fileEntry)
        {
            return new RemoteAsset(fileEntry, this.ProjectBaseURL, ForceCompression);
        }
    }
}
