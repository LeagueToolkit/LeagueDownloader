using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueDownloader.Solution
{
    public class SolutionManifest
    {
        private const string Header = "RADS Solution Manifest";

        public string ManifestVersion { get; private set; }
        public string SolutionName { get; private set; }
        public string SolutionVersion { get; private set; }
        public List<SolutionManifestProjectEntry> Projects { get; private set; } = new List<SolutionManifestProjectEntry>();
        public List<SolutionManifestLocalizedEntry> LocalizedEntries { get; private set; } = new List<SolutionManifestLocalizedEntry>();

        public SolutionManifest(string[] manifestContents)
        {
            if (manifestContents[0] != Header)
                throw new Exception("This is not a valid RADS Solution Manifest");

            ManifestVersion = manifestContents[1];
            SolutionName = manifestContents[2];
            SolutionVersion = manifestContents[3];
            int projectsCount = Int32.Parse(manifestContents[4]);
            int currentIndex = 5;
            for (int i = 0; i < projectsCount; i++)
            {
                Projects.Add(new SolutionManifestProjectEntry(
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
                List<SolutionManifestProjectEntry> localizedProjects = new List<SolutionManifestProjectEntry>();
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
                LocalizedEntries.Add(new SolutionManifestLocalizedEntry(name, unknown, localizedProjects));
            }
        }

        public void Write(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(File.Create(filePath)) { NewLine = "\r\n" })
            {
                sw.WriteLine(Header);
                sw.WriteLine(ManifestVersion);
                sw.WriteLine(SolutionName);
                sw.WriteLine(SolutionVersion);
                sw.WriteLine(Projects.Count);
                foreach (SolutionManifestProjectEntry entry in Projects)
                {
                    sw.WriteLine(entry.Name);
                    sw.WriteLine(entry.Version);
                    sw.WriteLine(entry.Unknown1);
                    sw.WriteLine(entry.Unknown2);
                }
                sw.WriteLine(LocalizedEntries.Count);
                foreach (SolutionManifestLocalizedEntry entry in LocalizedEntries)
                {
                    sw.WriteLine(entry.Name);
                    sw.WriteLine(entry.Unknown);
                    sw.WriteLine(entry.Projects.Count);
                    foreach (SolutionManifestProjectEntry projectEntry in entry.Projects)
                    {
                        sw.WriteLine(projectEntry.Name);
                    }
                }
            }
        }
    }
}
