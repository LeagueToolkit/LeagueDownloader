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

        [Verb("install-solution", HelpText = "Install a complete solution.")]
        class SolutionOptions : CommonOptions
        {

        }

        [Verb("install-project", HelpText = "Install a project.")]
        class ProjectOptions : CommonOptions
        {
            [Option('n', "name", Required = true, HelpText = "Project name.")]
            public string Name { get; set; }

            [Option('v', "version", Required = true, HelpText = "Project version.")]
            public string Version { get; set; }
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
            return 1;
        }

        static int InstallProject(ProjectOptions opts)
        {
            var radsInstaller = new RADSInstaller(opts.OutputFolder, opts.Platform);
            radsInstaller.InstallProject(opts.Name, opts.Version, "lol_game_client_sln", "0.0.1.100");
            return 1;
        }

    }
}
