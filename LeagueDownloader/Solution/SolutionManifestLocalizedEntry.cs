using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueDownloader.Solution
{
    public class SolutionManifestLocalizedEntry
    {
        public string Name { get; private set; }
        public int Unknown { get; private set; }
        public List<SolutionManifestProjectEntry> Projects { get; private set; }

        public SolutionManifestLocalizedEntry(string name, int unknown, List<SolutionManifestProjectEntry> projects)
        {
            Name = name;
            Unknown = unknown;
            Projects = projects;
        }
    }
}
