using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml.Serialization;
using NuGet;

namespace MicroserviceExplorer
{
    partial class MainWindow
    {
        class GitStatus
        {
            public string Branch { get; set; }
            public int GitRemoteCommits { get; set; }
            public int LocalCommits { get; set; }
        }

        void CalculateGitUpdates(MicroserviceItem service)
        {
            if (service.WebsiteFolder.IsEmpty()) return;

            var gitFetch = new BackgroundWorker();

            gitFetch.DoWork += (sender, doWorkEventArgs) =>
            {
                var projFOlder = service.WebsiteFolder.AsDirectory().Parent;
                if (projFOlder == null || !Directory.Exists(Path.Combine(projFOlder.FullName, ".git")))
                {
                    doWorkEventArgs.Result = 0;
                    return;
                }
                service.GitUpdates = "0";
                try
                {
                    var fetchoutput = "git.exe".AsFile(searchEnvironmentPath: true)
                        .Execute("fetch", waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = projFOlder.FullName);

                    service.LogMessage($"git fetch completed ... ({service.Service})");

                    var output = "git.exe".AsFile(searchEnvironmentPath: true)
                        .Execute("status", waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = projFOlder.FullName);

                    var status = ReadGitInfo(output);
                    if (status != null && status.GitRemoteCommits > 0)
                        service.LogMessage($"There are {status.GitRemoteCommits} git commit(s) available to update .");

                    service.GitUpdates = status?.GitRemoteCommits.ToString();
                    
                }
                catch (Exception e)
                {
                    service.LogMessage("Error on git command ...", e.Message);
                }
                service.GitUpdateIsInProgress = false;
            };

            gitFetch.RunWorkerAsync(service);
        }


        GitStatus ReadGitInfo(string input)
        {
            if (input.IsEmpty())
                return null;
            var pattern = @"Your branch is behind '(?<branch>[a-zA-Z/]*)' by (?<remoteCommits>\d*) commit";
            const RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;

            var match = Regex.Match(input, pattern, options);
            var branch = match.Groups["branch"];
            var remoteCommits = match.Groups["remoteCommits"];
            if (match.Success)
                return new GitStatus { Branch = branch.Value, GitRemoteCommits = remoteCommits.Value.To<int>() };

            pattern = @"Your branch and '(?<branch>[a-zA-Z/]*)' have diverged,\nand have (?<localCommits>\d*) and (?<remoteCommits>\d*) different commit";
            match = Regex.Match(input, pattern, options);
            branch = match.Groups["branch"];
            remoteCommits = match.Groups["remoteCommits"];
            var localCommits = match.Groups["localCommits"];

            if (match.Success)
                return new GitStatus
                {
                    Branch = branch.Value,
                    GitRemoteCommits = remoteCommits.Value.To<int>(),
                    LocalCommits = localCommits.Value.To<int>()
                };

            return null;
        }

        async Task GitUpdate(MicroserviceItem service)
        {

            AutoRefreshTimer.Stop();
            var projFOlder = service.WebsiteFolder.AsDirectory().Parent;
            string run()
            {
                StatusProgressStart();

                try
                {
                    return "git.exe".AsFile(searchEnvironmentPath: true)
                        .Execute("pull", waitForExit: true,
                            configuration: x => x.StartInfo.WorkingDirectory = projFOlder?.FullName);
                }
                catch (Exception e)
                {
                    service.LogMessage("error on git pull ...", e.Message);
                    //StatusProgressStop();
                    return e.Message;
                }

            }
            var output = await Task.Run((Func<string>)run);
            if (output.HasValue())
                service.LogMessage("git pull completed.", output);

            CalculateGitUpdates(service);

            StatusProgressStop();
            AutoRefreshTimer.Start();
        }

        //=======================================================
        void FetchProjectNugetPackages(MicroserviceItem service, MicroserviceItem.EnumProjects projEnum)
        {
            var projFolder = service.GetAbsoluteProjFolder(projEnum);
            if (projFolder.IsEmpty()) return;

            var projCsFile = ".csproj".GetFisrtFile(projFolder);
            if (projCsFile.IsEmpty()) return;

            var serializer = new XmlSerializer(typeof(Classes.Web.Project));
            Classes.Web.Project proj;
            using (var fileStream = File.OpenRead(projCsFile))
                proj = (Classes.Web.Project)serializer.Deserialize(fileStream);

            var nugetPackageSet = new Dictionary<string, string>();

            foreach (var itm in proj.ItemGroup)
                if (itm.PackageReference != null && itm.PackageReference.Any())
                {
                    service.Projects[projEnum].PackageReferences = itm.PackageReference.Select(x =>
                        new NugetRef
                        {
                            Include = x.Include,
                            Version = x.Version
                        }).ToList();
                }

            var nugetInitworker = new BackgroundWorker();

            nugetInitworker.DoWork += OnNugetInitworkerOnDoWork;
            nugetInitworker.RunWorkerCompleted += OnNugetInitworkerOnRunWorkerCompleted;

            nugetInitworker.RunWorkerAsync(new { service, projEnum });


        }

        readonly IPackageRepository nugetPackageRepo = PackageRepositoryFactory.Default.CreateRepository(@"https://packages.nuget.org/api/v2");

        void OnNugetInitworkerOnDoWork(object sender, DoWorkEventArgs args)
        {
            var arg = (dynamic)args.Argument;
            var srv = (MicroserviceItem)arg.service;
            var projEnum = (MicroserviceItem.EnumProjects)arg.projEnum;
            srv.NugetFetchTasks++;
            //srv.NugetStatusImage = "Pending";

            var packageReferences = srv.Projects[projEnum]?.PackageReferences;
            if (packageReferences == null) return;
            try
            {
                srv.LogMessage($"Check for nuget packages updates in [{projEnum}] project started.");
                foreach (var pkgref in packageReferences)
                {
                    var latestPkgVersion = nugetPackageRepo.FindPackages(pkgref.Include, null, allowPrereleaseVersions: false, allowUnlisted: false).FirstOrDefault(package => package.IsLatestVersion);
                    if (latestPkgVersion != null)
                        pkgref.NewVersion = latestPkgVersion.Version.ToOriginalString();
                }
            }
            catch (Exception e)
            {
                srv.NugetUpdateErrorMessage = e.Message;
                args.Result = new { service = srv, projEnum };
                return;
            }
            args.Result = new { service = srv, projEnum };
        }


        void OnNugetInitworkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            var arg = (dynamic)args.Result;
            var srv = (MicroserviceItem)arg.service;
            var projEnum = (MicroserviceItem.EnumProjects)arg.projEnum;

            srv.NugetFetchTasks--;
            if (srv.NugetUpdateErrorMessage.HasValue())
            {
                srv.NugetStatusImage = "Warning";
                srv.LogMessage($"Nuget update checking has been finished with no result for [{projEnum}] project. ", srv.NugetUpdateErrorMessage);
                srv.NugetUpdateErrorMessage = null;
                return;
            }

            foreach (var projectRef in srv.Projects)
                if (projectRef.Value.PackageReferences != null)
                    foreach (var nugetRef in projectRef.Value.PackageReferences)
                        if (nugetRef.IsLatestVersion)
                        {
                            if (srv.AddNugetUpdatesList(projectRef.Key, nugetRef.Include, nugetRef.Version, nugetRef.NewVersion))
                                srv.LogMessage($"\t > Package '{nugetRef.Include}' updated, from version [{nugetRef.Version}] to [{nugetRef.NewVersion}] in [{projEnum}] project.");
                        }

            srv.NugetStatusImage = null;
            srv.LogMessage($"Nuget update Completed for the [{projEnum}] project.");
            var worker = (BackgroundWorker)sender;
            worker.Dispose();
        }


        void UpdateNugetPackages(MicroserviceItem service, NugetUpdatesWindow nugetUpdatesWindow)
        {
            using (var nugetUpdateWorker = new BackgroundWorker())
            {
                nugetUpdateWorker.DoWork += (s1, e1) =>
                {
                    service.NugetFetchTasks++;
                    foreach (var nugetRef in nugetUpdatesWindow.NugetList.Where(itm => itm.Checked).ToArray())
                    {
                        if (!UpdateNugetPackages(service, nugetRef.Project, nugetRef.Include, nugetRef.NewVersion,
                            nugetRef.Version)) continue;

                        nugetRef.IsLatestVersion = true;
                        service.DelNugetPAckageFromUpdatesList(nugetRef.Project, nugetRef.Include);
                    }
                };

                nugetUpdateWorker.RunWorkerCompleted += (o, args) =>
                {
                    var worker = (BackgroundWorker)o;
                    service.NugetFetchTasks--;
                    worker.Dispose();
                };
                nugetUpdateWorker.RunWorkerAsync();
            }
        }

        bool UpdateNugetPackages(MicroserviceItem service, MicroserviceItem.EnumProjects projEnum, string packageName, string version, string fromVersion)
        {
            var projFolder = service.GetAbsoluteProjFolder(projEnum);
            if (projFolder.IsEmpty()) return false;

            service.LogMessage($"&&& > nuget update package started ... [{service.Service} -> {projEnum} -> {packageName}] {fromVersion} to {version}", $"Command : \n {projFolder}>dotnet.exe add package {packageName} -v {version}");

            string response;
            try
            {
                response = "dotnet.exe".AsFile(searchEnvironmentPath: true)
                    .Execute($"add package {packageName} -v {version}", waitForExit: true,
                        configuration: x => x.StartInfo.WorkingDirectory = projFolder);

            }
            catch (Exception e)
            {
                service.LogMessage($"!!!!!! > nuget update error on [{service.Service} -> {projEnum} -> {packageName} ({fromVersion} to {version})] :",
                    e.Message);
                return false;
            }

            service.LogMessage(
                $"nuget package update completed, [{service.Service} -> {projEnum} -> {packageName}] with result :",
                response);


            return true;
        }
    }
}
