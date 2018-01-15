using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueDownloader.Solution
{
    public class SolutionManifest
    {
        public string ManifestVersion { get; private set; }
        public string SolutionName { get; private set; }
        public string SolutionVersion { get; private set; }
        public List<SolutionProject> Projects { get; private set; } = new List<SolutionProject>();
        public List<LocalizedEntry> LocalizedEntries { get; private set; } = new List<LocalizedEntry>();

        public SolutionManifest(string[] manifestContents)
        {
            if (manifestContents[0] != "RADS Solution Manifest")
                throw new Exception("This is not a valid RADS Solution Manifest");

            ManifestVersion = manifestContents[1];
            SolutionName = manifestContents[2];
            SolutionVersion = manifestContents[3];
            int projectsCount = Int32.Parse(manifestContents[4]);
            int currentIndex = 5;
            for (int i = 0; i < projectsCount; i++)
            {
                Projects.Add(new SolutionProject(
                    manifestContents[currentIndex],
                    manifestContents[currentIndex + 1],
                    Int32.Parse(manifestContents[currentIndex + 2]),
                    Int32.Parse(manifestContents[currentIndex + 3])));
                currentIndex += 4;
            }
            int localizedEntriesCount = Int32.Parse(manifestContents[currentIndex]);
            currentIndex++;
            for (int i = 0; i < localizedEntriesCount; i++)
            {
                List<SolutionProject> localizedProjects = new List<SolutionProject>();
                string name = manifestContents[currentIndex];
                int unknown = Int32.Parse(manifestContents[currentIndex + 1]);
                int localizedProjectsCount = Int32.Parse(manifestContents[currentIndex + 2]);
                currentIndex += 3;
                for (int j = 0; j < localizedProjectsCount; j++)
                {
                    string projectName = manifestContents[currentIndex];
                    localizedProjects.Add(Projects.Find(x => x.Name.Equals(projectName)));
                    currentIndex++;
                }
                LocalizedEntries.Add(new LocalizedEntry(name, unknown, localizedProjects));
            }
        }
    }
}
