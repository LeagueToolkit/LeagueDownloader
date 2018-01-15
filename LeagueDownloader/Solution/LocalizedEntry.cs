using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueDownloader.Solution
{
    public class LocalizedEntry
    {
        public string Name { get; private set; }
        public int Unknown { get; private set; }
        public List<SolutionProject> Projects { get; private set; }

        public LocalizedEntry(string name, int unknown, List<SolutionProject> projects)
        {
            Name = name;
            Unknown = unknown;
            Projects = projects;
        }
    }
}
