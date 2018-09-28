using MicroserviceExplorer.Annotations;
using MicroserviceExplorer.Classes.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MicroserviceExplorer
{
    public class NugetReference : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Latest { get; set; }
        public bool ShouldUpdate;
        static Dictionary<string, string> NugetVersions = new Dictionary<string, string>();
        static DateTime NugetVersionsValidUntil = DateTime.Now.AddDays(-1);

        #region INotifyPropertyChanged Implementations

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion        

        public SolutionProject Project { get; }
        public MicroserviceItem Service { get; }

        public NugetReference(ProjectItemGroupPackageReference @ref, MicroserviceItem service, SolutionProject project)
        {
            Name = @ref.Include;
            Version = @ref.Version;
            Latest = FindLatest();
            Project = project;
            Service = service;
        }

        public bool IsUpToDate => Version == Latest;

        public string FindLatest()
        {
            if (NugetVersionsValidUntil < DateTime.Now)
            {
                NugetVersions = new Dictionary<string, string>();
                NugetVersionsValidUntil = DateTime.Now.AddMinutes(5);
            }

            if (NugetVersions.TryGetValue(Name, out var result)) return result;

            lock (NugetVersions)
            {
                try
                {
                    result = $"https://www.nuget.org/packages/{Name}/".AsUri()
                    .Download().ToLower()
                        .Substring($"<meta property=\"og:title\" content=\"{Name.ToLower()} ", "\"", inclusive: false);
                }
                catch (Exception ex) when (ex.Message.Contains("404"))
                {
                    return Version;
                }

                if (result.IsEmpty()) throw new Exception("Failed to find latest nuget version for " + Name);
                return NugetVersions[Name] = result;
            }
        }

        public void Update()
        {
            var command = $"add package {Name} -v {Latest}";
            var dir = Service.SolutionFolder.AsDirectory().GetSubDirectory(Project.Path()).FullName;

            Service.LogMessage($"Updating nuget package:\n{dir}>dotnet.exe {command}");

            string response = null;

            for (var retries = 5; retries > 0; retries--)
            {
                try
                {
                    response = "dotnet.exe".AsFile(searchEnvironmentPath: true)
                        .Execute(command, waitForExit: true,
                            configuration: x => x.StartInfo.WorkingDirectory = dir);

                    OnPropertyChanged(nameof(Version));

                }
                catch (Exception e)
                {
                    if (e.Message.Contains("The process cannot access the file")) continue;

                    Service.LogMessage($"nuget update error on [{Project} -> {Name} ({Version} to {Latest})] :",
                        e.Message);
                    return;
                }
            }

            Version = Latest;
        }
    }
}
