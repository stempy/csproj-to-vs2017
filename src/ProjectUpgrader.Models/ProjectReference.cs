namespace ProjectUpgrader.Models
{
    public class ProjectReference
    {
        public string Name { get; set; }

        public string Include { get; set; }
        public string HintPath { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsSpecificVersion { get; set; }
        public string Version { get; set; }
        public string Source { get; set; }

        public bool IsNugetPackage =>
            (!string.IsNullOrEmpty(HintPath)) && HintPath.ToLower().Contains("packages\\");

        public bool IsFrameworkAssembly =>
            ((string.IsNullOrEmpty(HintPath) && !Name.StartsWith("Microsoft.AspNet"))
                && (Name.StartsWith("System") || Name.StartsWith("Microsoft")));

        //public string ReferenceElement { get; set; }
        public ProjectReferenceType ReferenceType { get; set; }

        public override string ToString()
        {
            return $"{Name}, [{Version}] {HintPath}";
        }
    }

    public enum ProjectReferenceType
    {
        Reference,
        ProjectReference,
        PackageReference
    }
}
