namespace ProjectUpgrader.Models
{
    public class PackageReference
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool IsLegacyPackage { get; set; }

        public override string ToString()
        {
            return $"{Name}, {Version}";
        }
    }
}
