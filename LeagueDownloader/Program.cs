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
        abstract class CommonOptions
        {
            [Option('o', "output-folder", Default = null, Required = true, HelpText = "Output folder.")]
            public string OutputFolder { get; set; }

            [Option('p', "platform", Default = "live", Required = false, HelpText = "Platform")]
            public string Platform { get; set; }
        }

        [Verb("solution", HelpText = "Install a complete solution.")]
        class SolutionOptions : CommonOptions
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
        class ProjectOptions : CommonOptions
        {
            [Option('n', "name", Required = true, HelpText = "Project name (e.g. lol_game_client).")]
            public string Name { get; set; }

            [Option('v', "version", Required = true, HelpText = "Project version (e.g. 0.0.1.7).")]
            public string Version { get; set; }

            [Option('d', "deploy-mode", Required = false, Default = null, HelpText = "Forced deploy mode.")]
            public uint? DeployMode { get; set; }

        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<SolutionOptions, ProjectOptions>(args)
              .MapResult(
                (SolutionOptions opts) => InstallSolution(opts),
                (ProjectOptions opts) => InstallProject(opts),
                errs => 1);
        }

        static int InstallSolution(SolutionOptions opts)
        {
            var radsInstaller = new RADSInstaller(opts.OutputFolder, opts.Platform);
            radsInstaller.InstallSolution(opts.Name, opts.Version, opts.Localization, opts.DeployMode);
            return 1;
        }

        static int InstallProject(ProjectOptions opts)
        {
            var radsInstaller = new RADSInstaller(opts.OutputFolder, opts.Platform);
            radsInstaller.InstallProject(opts.Name, opts.Version, opts.DeployMode);
            return 1;
        }
    }
}
