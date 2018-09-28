using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

        public static Classes.Web.Project GetProjectFile(this SolutionProject project, string solutionFolder)
        {
            var folder = solutionFolder.AsDirectory().GetSubDirectory(project.Path(), onlyWhenExists: false);
            if (!folder.Exists()) return null;
            var csproj = folder.GetFiles("*.csproj").SingleOrDefault();
            if (csproj == null) return null;

            var serializer = new XmlSerializer(typeof(Classes.Web.Project));
            using (var fileStream = File.OpenRead(csproj.FullName))
                return (Classes.Web.Project)serializer.Deserialize(fileStream);
        }
    }
}
