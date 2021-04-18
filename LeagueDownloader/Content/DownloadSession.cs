using LeagueDownloader.Project;
using LeagueDownloader.Solution;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueDownloader.Content
{
    public class DownloadSession : IDisposable
    {
        private readonly Dictionary<RemoteAsset, ProjectReleaseFileInstaller> registeredFiles = new Dictionary<RemoteAsset, ProjectReleaseFileInstaller>();

        private readonly ProgressBar progressBar;

        private List<RemoteAsset> failedDownloads = new List<RemoteAsset>();

        private List<RemoteAsset> failedInstallations = new List<RemoteAsset>();

        public DownloadSession(String message)
        {
            Console.Clear();
            this.progressBar = new ProgressBar(1, message, new ProgressBarOptions { DisableBottomPercentage = true, CollapseWhenFinished = true });
        }

        public void ResetRegisteredFiles()
        {
            this.registeredFiles.Clear();
        }

        public void RegisterFile(RemoteAsset remoteAsset, ProjectReleaseFileInstaller fileInstaller)
        {
            this.registeredFiles.Add(remoteAsset, fileInstaller);
        }

        public ProjectRelease GetProjectRelease(string name, string version, string leagueCDNBaseURL)
        {
            using (ChildProgressBar child = progressBar.Spawn(1, String.Format("Downloading manifest for project {0} (version {1})", name, version), new ProgressBarOptions { DisableBottomPercentage = false }))
            {
                ProjectRelease projectRelease = new ProjectRelease(name, version, leagueCDNBaseURL);
                child.Tick(String.Format("Downloaded manifest for project {0} (version {1})", name, version));
                return projectRelease;
            }
        }

        public SolutionRelease GetSolutionRelease(string name, string version, string leagueCDNBaseURL)
        {
            using (ChildProgressBar child = progressBar.Spawn(1, String.Format("Downloading manifest for solution {0} (version {1})", name, version), new ProgressBarOptions { DisableBottomPercentage = false }))
            {
                SolutionRelease solutionRelease = new SolutionRelease(name, version, leagueCDNBaseURL);
                child.Tick(String.Format("Downloaded manifest for solution {0} (version {1})", name, version));
                return solutionRelease;
            }
        }

        private Dictionary<RemoteAsset, ProjectReleaseFileInstaller> GetFilesToInstall()
        {
            Dictionary<RemoteAsset, ProjectReleaseFileInstaller> filesToInstall = new Dictionary<RemoteAsset, ProjectReleaseFileInstaller>();
            using (ChildProgressBar child = progressBar.Spawn(registeredFiles.Count, "Checking already installed files", new ProgressBarOptions { DisableBottomPercentage = false }))
            {
                foreach (KeyValuePair<RemoteAsset, ProjectReleaseFileInstaller> fileToInstall in registeredFiles)
                {
                    child.Tick(String.Format("Checking {0}/{1}", fileToInstall.Key.StringVersion, fileToInstall.Key.FileFullPath));
                    if (!fileToInstall.Value.IsFileInstalled(fileToInstall.Key))
                    {
                        filesToInstall.Add(fileToInstall.Key, fileToInstall.Value);
                    }
                }
                child.Tick("Checking done");
            }

            return filesToInstall;
        }

        private double GetBytesToDownload(Dictionary<RemoteAsset, ProjectReleaseFileInstaller> filesToInstall)
        {
            double bytesToDownload = 0;
            foreach (KeyValuePair<RemoteAsset, ProjectReleaseFileInstaller> file in filesToInstall) {
                bytesToDownload += file.Key.FileEntry.SizeCompressed > 0 ? file.Key.FileEntry.SizeCompressed : file.Key.FileEntry.SizeRaw;
            }

            return bytesToDownload;
        }

        private Task<List<RemoteAsset>> GetDownloadTask(SynchronizedCollection<RemoteAsset> downloadedFiles, Dictionary<RemoteAsset, ProjectReleaseFileInstaller> filesToInstall)
        {
            return Task.Run(() =>
           {
               double bytesToDownload = GetBytesToDownload(filesToInstall);
               List<RemoteAsset> failedDownloads = new List<RemoteAsset>();
               using (ChildProgressBar child = progressBar.Spawn(filesToInstall.Count, String.Format("Downloading files ({0:0.##} MB)", bytesToDownload / (1024*1024)), new ProgressBarOptions { DisableBottomPercentage = false }))
               {
                   ParallelOptions opts = new ParallelOptions() { MaxDegreeOfParallelism = 10 };
                   Parallel.ForEach(filesToInstall, opts, file =>
                   {
                       int tries = 0;
                       bool downloaded = false;
                       while (!downloaded)
                       {
                           try
                           {
                               tries++;
                               file.Key.AssetContent.DownloadAssetData();
                               child.Tick();
                               downloadedFiles.Add(file.Key);
                               downloaded = true;
                           }
                           catch (Exception e)
                           {
                               if (tries == 10)
                               {
                                   failedDownloads.Add(file.Key);
                                   downloaded = true;
                               }
                           }
                       }
                   });
                   child.Tick("Download done");
               }
               return failedDownloads;
           });
        }

        private Task<List<RemoteAsset>> GetInstallTask(SynchronizedCollection<RemoteAsset> downloadedFiles, Task downloadTask, Dictionary<RemoteAsset, ProjectReleaseFileInstaller> filesToInstall)
        {
            return Task.Run(() =>
            {
                List<RemoteAsset> failedInstalls = new List<RemoteAsset>();
                using (ChildProgressBar child = progressBar.Spawn(filesToInstall.Count, "Installing files", new ProgressBarOptions { DisableBottomPercentage = false }))
                {
                    while (!downloadTask.IsCompleted || downloadedFiles.Count > 0)
                    {
                        List<RemoteAsset> packagedFiles = new List<RemoteAsset>();
                        for (int i = 0; i < downloadedFiles.Count; i++)
                        {
                            RemoteAsset fileToPackage = downloadedFiles[i];
                            child.Tick(String.Format("Installing {0}/{1}", fileToPackage.StringVersion, fileToPackage.FileFullPath));

                            try
                            {
                                filesToInstall[fileToPackage].InstallFile(fileToPackage);
                            }
                            catch (Exception)
                            {
                                failedInstalls.Add(fileToPackage);
                            }

                            fileToPackage.AssetContent.FlushAssetData();
                            packagedFiles.Add(fileToPackage);
                        }
                        packagedFiles.ForEach(x => downloadedFiles.Remove(x));
                    }
                    child.Tick("Installation done");
                }
                return failedInstalls;
            });
        }

        private void WriteReport()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (failedDownloads.Count > 0)
            {                
                Console.WriteLine(String.Format("{0} file(s) could not be downloaded", failedDownloads.Count));
                foreach (RemoteAsset remoteAsset in failedDownloads) {
                    Console.WriteLine(String.Format("{0}/{1}", remoteAsset.StringVersion, remoteAsset.FileFullPath));
                }
            }

            if (failedInstallations.Count > 0)
            {
                Console.WriteLine(String.Format("{0} file(s) could not be downloaded", failedInstallations.Count));
                foreach (RemoteAsset remoteAsset in failedInstallations)
                {
                    Console.WriteLine(String.Format("{0}/{1}", remoteAsset.StringVersion, remoteAsset.FileFullPath));
                }
            }
            Console.ResetColor();
        }

        public void DoWork()
        {
            Dictionary<RemoteAsset, ProjectReleaseFileInstaller> filesToInstall = GetFilesToInstall();

            if (filesToInstall.Count > 0)
            {
                SynchronizedCollection<RemoteAsset> downloadedFiles = new SynchronizedCollection<RemoteAsset>();
                Task<List<RemoteAsset>> downloadTask = GetDownloadTask(downloadedFiles, filesToInstall);
                Task<List<RemoteAsset>> installTask = GetInstallTask(downloadedFiles, downloadTask, filesToInstall);

                installTask.Wait();
                failedDownloads.AddRange(downloadTask.Result);
                failedInstallations.AddRange(installTask.Result);
            }
        }

        public void Dispose()
        {
           this.progressBar.Dispose();
           this.WriteReport();
        }
    }
}
