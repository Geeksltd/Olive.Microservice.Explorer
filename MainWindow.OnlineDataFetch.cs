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

        int CalculateGitUpdates(MicroserviceItem service)
        {
            if (service.WebsiteFolder.IsEmpty()) return 0;


            var projFOlder = service.WebsiteFolder.AsDirectory().Parent;
            if (projFOlder == null || !Directory.Exists(Path.Combine(projFOlder.FullName, ".git")))
                return 0;

            string run()
            {
                StatusProgressStart();
                ShowStatusMessage("Start git fetch ...", tooltip: null, logMessage: false);
                try
                {
                    var fetchoutput = "git.exe".AsFile(searchEnvironmentPath: true)
                        .Execute("fetch", waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = projFOlder.FullName);

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(() => ShowStatusMessage($"git fetch completed ... ({service.Service})", fetchoutput)));

                    return "git.exe".AsFile(searchEnvironmentPath: true)
                        .Execute("status", waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = projFOlder.FullName);
                }
                catch (Exception e)
                {
                    StatusProgressStop();
                    ShowStatusMessage("Error on git fetch ...", tooltip: null, logMessage: false);
                    logWindow.LogMessage("Error on git command ...", e.Message);
                    service.GitUpdateIsInProgress = false;
                    return null;
                }
            }

            service.GitUpdateIsInProgress = true;
            var output = Task.Run((Func<string>)run).Result;
            service.GitUpdateIsInProgress = false;

            var status = ReadGitInfo(output);

            if (status != null)
            {
                service.GitUpdates = status.GitRemoteCommits.ToString();
                ShowStatusMessage(
                    $"getting git commit count completed ... ({service.Service}) with {status?.GitRemoteCommits ?? 0} commit(s) in {status?.Branch ?? "it's branch"}",
                    output);
            }
            else
                service.GitUpdates = null;


            StatusProgressStop();

            return status?.GitRemoteCommits ?? 0;
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
                    ShowStatusMessage("error on git pull ...", e.Message);
                    StatusProgressStop();
                    return e.Message;
                }

            }
            var output = await Task.Run((Func<string>)run);
            if (output.HasValue())
                ShowStatusMessage("git pull completed ...", output);

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

            using (var nugetInitworker = new BackgroundWorker())
            {
                nugetInitworker.DoWork += OnNugetInitworkerOnDoWork;
                nugetInitworker.RunWorkerCompleted += OnNugetInitworkerOnRunWorkerCompleted;

                logWindow.LogMessage($"*** > Nuget update Check Async Started ({service.Service} - {projEnum})");
                nugetInitworker.RunWorkerAsync(new { service, projEnum });
            }

        }

        readonly IPackageRepository nugetPackageRepo = PackageRepositoryFactory.Default.CreateRepository(@"https://packages.nuget.org/api/v2");

        void OnNugetInitworkerOnDoWork(object sender, DoWorkEventArgs args)
        {
            var arg = (dynamic)args.Argument;
            var srv = (MicroserviceItem)arg.service;
            var projEnum = (MicroserviceItem.EnumProjects)arg.projEnum;
            srv.NugetFetchTasks++;
            //srv.NugetStatusImage = "Pending";
            lock (_lock)
            {
                var packageReferences = srv.Projects[projEnum]?.PackageReferences;
                if (packageReferences == null) return;
                try
                {
                    logWindow.LogMessage($"### > Begin Nuget Packages update check ... ({srv.Service} - {projEnum})");
                    foreach (var pkgref in packageReferences)
                    {
                        var latestPkgVersion = nugetPackageRepo.FindPackages(pkgref.Include, null, allowPrereleaseVersions: false, allowUnlisted: false).FirstOrDefault(package => package.IsLatestVersion);
                        if (latestPkgVersion != null)
                            pkgref.NewVersion = latestPkgVersion.Version.ToOriginalString();
                    }


                    logWindow.LogMessage($"=== > End Nuget Packages update checking. ({srv.Service} - {projEnum})");
                }
                catch (Exception e)
                {
                    srv.NugetUpdateErrorMessage = e.Message;
                    args.Result = srv;
                    return;
                }
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
                logWindow.LogMessage($"!!! > Nuget update checking has been finished with no result ... ({srv.Service} - {projEnum}) ", srv.NugetUpdateErrorMessage);
                srv.NugetUpdateErrorMessage = null;
                return;
            }

            foreach (var projectRef in srv.Projects)
                if (projectRef.Value.PackageReferences != null)
                    foreach (var nugetRef in projectRef.Value.PackageReferences)
                        if (nugetRef.IsLatestVersion)
                        {
                            if (srv.AddNugetUpdatesList(projectRef.Key, nugetRef.Include, nugetRef.Version, nugetRef.NewVersion))
                                logWindow.LogMessage($"\t > Package '{nugetRef.Include}' updated, from version [{nugetRef.Version}] to [{nugetRef.NewVersion}] in ({srv.Service} - {projEnum}) project.");
                        }

            srv.NugetStatusImage = null;
            logWindow.LogMessage($"@@@ > Nuget update Completed ({srv.Service} - {projEnum})");
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

        readonly object _lock = new object();
        bool UpdateNugetPackages(MicroserviceItem service, MicroserviceItem.EnumProjects projEnum, string packageName, string version, string fromVersion)
        {
            var projFolder = service.GetAbsoluteProjFolder(projEnum);
            if (projFolder.IsEmpty()) return false;

            if (service.NugetUpdates > 0)
            {
                MessageBox.Show("Nuget Package updating is in progress now, Please try later ...", "Update is in progress");
                return false;
            }

            lock (_lock)
            {
                logWindow.LogMessage($"&&& > nuget update package started ... [{service.Service} -> {projEnum} -> {packageName}] {fromVersion} to {version}", $"Command : \n {projFolder}>dotnet.exe add package {packageName} -v {version}");

                string response;
                try
                {
                    response = "dotnet.exe".AsFile(searchEnvironmentPath: true)
                        .Execute($"add package {packageName} -v {version}", waitForExit: true,
                            configuration: x => x.StartInfo.WorkingDirectory = projFolder);

                }
                catch (Exception e)
                {
                    logWindow.LogMessage($"!!!!!! > nuget update error on [{service.Service} -> {projEnum} -> {packageName} ({fromVersion} to {version})] :",
                        e.Message);
                    return false;
                }

                logWindow.LogMessage(
                    $"nuget package update completed, [{service.Service} -> {projEnum} -> {packageName}] with result :",
                    response);
            }

            return true;
        }
    }
}
