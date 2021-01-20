using System;
using System.Linq;
using System.Xml.Linq;

namespace MicroserviceExplorer
{
    public static class Extensions
    {
        public static string Path(this SolutionProject projEnum)
        {
            switch (projEnum)
            {
                case SolutionProject.Website: return "Website";
                case SolutionProject.Domain: return "Domain";
                case SolutionProject.Model: return "M#\\Model";
                case SolutionProject.UI: return "M#\\UI";
                default: throw new NotSupportedException();
            }
        }

        public static (string name, string ver)[] GetNugetPackages(this SolutionProject project, string solutionFolder)
        {
            var folder = solutionFolder.AsDirectory().GetSubDirectory(project.Path(), onlyWhenExists: false);
            if (!folder.Exists()) return null;
            var csproj = folder.GetFiles("*.csproj").WithMax(x => x.LastWriteTimeUtc);
            if (csproj == null) return null;
            var text = csproj.ReadAllText().Trim();

            return text.To<XDocument>().Root.RemoveNamespaces().Descendants(XName.Get("PackageReference"))
                   .Select(v => new { Package = v.GetValue<string>("@Include"), Version = v.GetValue<string>("@Version") })
                   .Select(x => (name: x.Package, ver: x.Version))
                   .ToArray();
        }
    }
}