using System.Collections.Generic;
using ProjectUpgrader.Models;

namespace ProjectUpgrader.Upgraders
{
    public class ReferenceToPackageReferenceUpgrader
    {
        public IEnumerable<PackageReference> UpgradeReferencesToPackageReference(IEnumerable<ProjectReference> projectReferences)
        {
            var pr = new List<PackageReference>();
            foreach(var r in projectReferences)
            {
                pr.Add(new PackageReference()
                {
                    Name = r.Name,
                    Version = "*"
                });
            }
            return pr;
        }

    }
}
