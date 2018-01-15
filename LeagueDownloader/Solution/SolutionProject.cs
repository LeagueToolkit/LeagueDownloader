using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueDownloader.Solution
{
    public class SolutionProject
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public int Unknown1 { get; private set; }
        public int Unknown2 { get; private set; }

        public SolutionProject(string name, string version, int unknown1, int unknown2)
        {
            Name = name;
            Version = version;
            Unknown1 = unknown1;
            Unknown2 = unknown2;
        }
    }
}
