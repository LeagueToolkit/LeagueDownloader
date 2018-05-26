using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueDownloader
{
    class Program
    {
        abstract class CommonInstallOptions
        {
            [Option('o', "output-folder", Default = null, Required = true, HelpText = "Output folder.")]
            public string OutputFolder { get; set; }

            [Option('u', "cdn-url", Default = "http://l3cdn.riotgames.com/releases/live", Required = false, HelpText = "CDN url to use.")]
            public string CDNBaseURL { get; set; }
        }

        [Verb("solution", HelpText = "Install a complete solution.")]
        class SolutionOptions : CommonInstallOptions
        {
            [Option('n', "name", Required = true, HelpText = "Solution name (e.g. lol_game_client_sln).")]
            public string Name { get; set; }

            [Option('v', "version", Required = true, HelpText = "Solution version (e.g. 0.0.1.68).")]
            public string Version { get; set; }

            [Option('l', "localization", Required = true, HelpText = "Localization (e.g. en_gb).")]
            public string Localization { get; set; }

            [Option('d', "deploy-mode", Required = false, Default = null, HelpText = "Forced deploy mode.")]
            public uint? DeployMode { get; set; }
        }

        [Verb("project", HelpText = "Install a project.")]
        class ProjectOptions : CommonInstallOptions
        {
            [Option('n', "name", Required = true, HelpText = "Project name (e.g. lol_game_client).")]
            public string Name { get; set; }

            [Option('v', "version", Required = true, HelpText = "Project version (e.g. 0.0.1.7).")]
            public string Version { get; set; }

            [Option('d', "deploy-mode", Required = false, Default = null, HelpText = "Forced deploy mode.")]
            public uint? DeployMode { get; set; }
        }

        abstract class CommonSelectionOptions
        {
            [Option('n', "name", Required = true, HelpText = "Project name (e.g. lol_game_client).")]
            public string ProjectName { get; set; }

            [Option('f', "filter", Required = false, Default = null, HelpText = "Files/Folder filter (e.g. LEVELS/Map1/env.ini or LEVELS/Map1/).")]
            public string Filter { get; set; }

            [Option('u', "cdn-url", Default = "http://l3cdn.riotgames.com/releases/live", Required = false, HelpText = "CDN url to use.")]
            public string CDNBaseURL { get; set; }
        }

        abstract class CommonListOptions : CommonSelectionOptions
        {
            [Option('v', "version", Required = false, Default = "LATEST", HelpText = "Project version (e.g. 0.0.1.7).")]
            public string ProjectVersion { get; set; }

            [Option('r', "revision", Required = false, Default = null, HelpText = "Files revision (e.g. 0.0.1.7).")]
            public string FilesRevision { get; set; }
        }

        [Verb("list", HelpText = "List files.")]
        class ListOptions : CommonListOptions { }

        [Verb("download", HelpText = "Download files.")]
        class DownloadOptions : CommonListOptions
        {
            [Option('o', "output-folder", Default = null, Required = true, HelpText = "Output folder.")]
            public string OutputFolder { get; set; }

            [Option("save-manifest", Required = false, Default = false, HelpText = "Save the downloaded release manifest.")]
            public bool SaveManifest { get; set; }
        }

        [Verb("range-download", HelpText = "Download files in a range of revisions.")]
        class RangeDownloadOptions : CommonSelectionOptions
        {
            [Option('o', "output-folder", Default = null, Required = true, HelpText = "Output folder.")]
            public string OutputFolder { get; set; }

            [Option("start-revision", Required = false, Default = null, HelpText = "Files revision (e.g. 0.0.1.7).")]
            public string StartRevision { get; set; }

            [Option("end-revision", Required = false, Default = null, HelpText = "Files revision (e.g. 0.0.1.7).")]
            public string EndRevision { get; set; }

            [Option("ignore-older-files", Required = false, Default = false, HelpText = "Ignore files revised earlier than the specified start revision.")]
            public bool IgnoreOlderFiles { get; set; }

            [Option("save-manifest", Required = false, Default = false, HelpText = "Save the downloaded release manifest.")]
            public bool SaveManifest { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<SolutionOptions, ProjectOptions, ListOptions, DownloadOptions, RangeDownloadOptions>(args)
              .MapResult(
                (SolutionOptions opts) => InstallSolution(opts),
                (ProjectOptions opts) => InstallProject(opts),
                (ListOptions opts) => ListFiles(opts),
                (DownloadOptions opts) => DownloadFiles(opts),
                (RangeDownloadOptions opts) => RangeDownloadFiles(opts),
                errs => 1);
        }

        static int InstallSolution(SolutionOptions opts)
        {
            var radsInteractor = new RADSInteractor(opts.CDNBaseURL);
            radsInteractor.InstallSolution(opts.OutputFolder, opts.Name, opts.Version, opts.Localization, opts.DeployMode);
            return 1;
        }

        static int InstallProject(ProjectOptions opts)
        {
            var radsInteractor = new RADSInteractor(opts.CDNBaseURL);
            radsInteractor.InstallProject(opts.OutputFolder, opts.Name, opts.Version, opts.DeployMode);
            return 1;
        }

        static int ListFiles(ListOptions opts)
        {
            var radsInteractor = new RADSInteractor(opts.CDNBaseURL);
            radsInteractor.ListFiles(opts.ProjectName, opts.ProjectVersion, opts.Filter, opts.FilesRevision);
            return 1;
        }

        static int DownloadFiles(DownloadOptions opts)
        {
            var radsInteractor = new RADSInteractor(opts.CDNBaseURL);
            radsInteractor.DownloadFiles(opts.OutputFolder, opts.ProjectName, opts.ProjectVersion, opts.Filter, opts.FilesRevision, opts.SaveManifest);
            return 1;
        }

        static int RangeDownloadFiles(RangeDownloadOptions opts)
        {
            var radsInteractor = new RADSInteractor(opts.CDNBaseURL);
            radsInteractor.RangeDownloadFiles(opts.OutputFolder, opts.ProjectName, opts.IgnoreOlderFiles, opts.Filter, opts.StartRevision, opts.EndRevision, opts.SaveManifest);
            return 1;
        }
    }
}
