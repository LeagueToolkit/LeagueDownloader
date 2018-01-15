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

            [Option('p', "platform", Default = "live", Required = false, HelpText = "Platform")]
            public string Platform { get; set; }
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

        abstract class CommonListOptions
        {
            [Option('n', "name", Required = true, HelpText = "Project name (e.g. lol_game_client).")]
            public string ProjectName { get; set; }

            [Option('v', "version", Required = false, Default = "LATEST", HelpText = "Project version (e.g. 0.0.1.7).")]
            public string ProjectVersion { get; set; }

            [Option('r', "revision", Required = false, Default = null, HelpText = "Files revision (e.g. 0.0.1.7).")]
            public string FilesRevision { get; set; }

            [Option('f', "filter", Required = false, Default = null, HelpText = "Files/Folder filter (e.g. LEVELS/Map1/env.ini or LEVELS/Map1/).")]
            public string Filter { get; set; }

            [Option('p', "platform", Default = "live", Required = false, HelpText = "Platform")]
            public string Platform { get; set; }
        }

        [Verb("list", HelpText = "List files.")]
        class ListOptions : CommonListOptions { }

        [Verb("download", HelpText = "Download files.")]
        class DownloadOptions : CommonListOptions
        {
            [Option('o', "output-folder", Default = null, Required = true, HelpText = "Output folder.")]
            public string OutputFolder { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<SolutionOptions, ProjectOptions, ListOptions, DownloadOptions>(args)
              .MapResult(
                (SolutionOptions opts) => InstallSolution(opts),
                (ProjectOptions opts) => InstallProject(opts),
                (ListOptions opts) => ListFiles(opts),
                (DownloadOptions opts) => DownloadFiles(opts),
                errs => 1);
        }

        static int InstallSolution(SolutionOptions opts)
        {
            var radsInteractor = new RADSInteractor(opts.Platform);
            radsInteractor.InstallSolution(opts.OutputFolder, opts.Name, opts.Version, opts.Localization, opts.DeployMode);
            return 1;
        }

        static int InstallProject(ProjectOptions opts)
        {
            var radsInteractor = new RADSInteractor(opts.Platform);
            radsInteractor.InstallProject(opts.OutputFolder, opts.Name, opts.Version, opts.DeployMode);
            return 1;
        }

        static int ListFiles(ListOptions opts)
        {
            var radsInteractor = new RADSInteractor(opts.Platform);
            radsInteractor.ListFiles(opts.ProjectName, opts.ProjectVersion, opts.Filter, opts.FilesRevision);
            return 1;
        }

        static int DownloadFiles(DownloadOptions opts)
        {
            var radsInteractor = new RADSInteractor(opts.Platform);
            radsInteractor.DownloadFiles(opts.OutputFolder, opts.ProjectName, opts.ProjectVersion, opts.Filter, opts.FilesRevision);
            return 1;
        }
    }
}
