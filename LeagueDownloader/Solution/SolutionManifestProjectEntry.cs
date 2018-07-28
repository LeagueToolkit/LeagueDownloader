namespace LeagueDownloader.Solution
{
    public class SolutionManifestProjectEntry
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public int Unknown1 { get; private set; }
        public int Unknown2 { get; private set; }

        public SolutionManifestProjectEntry(string name, string version, int unknown1, int unknown2)
        {
            Name = name;
            Version = version;
            Unknown1 = unknown1;
            Unknown2 = unknown2;
        }
    }
}
