using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using NuGet;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MacroserviceExplorer
{
    partial class MainWindow
    {
        FileInfo servicesJsonFile;
        public Visibility FileOpened { get; set; }

        const string services_file_name = "services.json";
        bool projextLoaded;
        DateTime servicesJsonFileLastWriteTime;

        async Task<bool> LoadFile(string filePath)
        {
            servicesJsonFile = new FileInfo(filePath);

            if (!servicesJsonFile.Exists)
            {

                var result = MessageBox.Show($"file : {servicesJsonFile.FullName} \ndoes not exist anymore. \nDo you want to removed it from recent files list?", "File Not Found", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
                _recentFiles.Remove(servicesJsonFile.FullName);
                if (result != System.Windows.Forms.DialogResult.Yes) return false;

                SaveRecentFilesXml();
                ReloadRecentFiles();

                servicesJsonFile = null;
                projextLoaded = false;
                return false;
            }

            servicesJsonFileLastWriteTime = servicesJsonFile.LastWriteTime;

            //try
            {

                var servicesAllText = File.ReadAllText(servicesJsonFile.FullName);
                var servicesJObject = Newtonsoft.Json.Linq.JObject.Parse(servicesAllText);
                txtFileInfo.Text = servicesJsonFile.FullName;
                txtSolName.Text = servicesJObject["Solution"]["FullName"].ToString();

                var children = servicesJObject["Services"].Children();
                foreach (var jToken in children)
                {
                    var serviceName = jToken.Path.Replace("Services.", "");
                    var srv = serviceData.SingleOrDefault(srvc => srvc.Service == serviceName);
                    if (srv == null)
                    {
                        srv = new MacroserviceGridItem();
                        serviceData.Add(srv);
                    }
                    string liveUrl = null;
                    string uatUrl = null;
                    foreach (var url in jToken.Children())
                    {
                        liveUrl = url["LiveUrl"].ToString();
                        uatUrl = url["UatUrl"].ToString();
                    }

                    var port = "";
                    var status = MacroserviceGridItem.enumStatus.NoSourcerLocally;
                    var parentFullName = servicesJsonFile.Directory?.Parent?.FullName ?? "";
                    var projFolder = Path.Combine(parentFullName, serviceName);
                    var websiteFolder = Path.Combine(projFolder, "website");
                    var launchSettings = Path.Combine(websiteFolder, "properties", "launchSettings.json");
                    var procId = -1;

                    if (File.Exists(launchSettings))
                    {
                        var launchSettingsAllText = File.ReadAllText(launchSettings);
                        var launchSettingsJObject = Newtonsoft.Json.Linq.JObject.Parse(launchSettingsAllText);
                        var appUrl = launchSettingsJObject["profiles"]["Website"]["applicationUrl"].ToString();
                        port = appUrl.Substring(appUrl.LastIndexOf(":", StringComparison.Ordinal) + 1);
                        status = MacroserviceGridItem.enumStatus.Stop;
                        if (!int.TryParse(port, out var _))
                        {
                            var pos = 0;
                            var portNumer = "";
                            while (pos < port.Length - 1 && char.IsDigit(port[pos]))
                                portNumer += port[pos++];
                            port = portNumer;
                        }

                        procId = GetProcessIdByPortNumber(port.To<int>());
                        if (procId > 0)
                            status = MacroserviceGridItem.enumStatus.Run;
                    }
                    else
                        websiteFolder = null;

                    srv.Status = status;
                    srv.Service = serviceName;
                    srv.Port = port;
                    srv.LiveUrl = liveUrl;
                    srv.UatUrl = uatUrl;
                    srv.ProcId = procId;
                    srv.SolutionFolder = projFolder;
                    srv.WebsiteFolder = websiteFolder;

                    foreach (MacroserviceGridItem.enumProjects proj in Enum.GetValues(typeof(MacroserviceGridItem.enumProjects)))
                        FetchProjectNugetPackages(srv, proj);

                    srv.VsDTE = GetVSDTE(srv);

                    if (!srv.WebsiteFolder.HasValue())
                        continue;

                    var gitUpdates = await GetGitUpdates(srv);
                    srv.GitUpdates = gitUpdates.ToString();

                }


                FilterListBy(txtSearch.Text);
            }
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //    return false;
            //}

            if (watcher == null)
                StartFileSystemWatcher(servicesJsonFile);

            projextLoaded = true;
            return true;
        }

        async Task<bool> RefreshFile(string filePath)
        {
            var srvFile = new FileInfo(filePath);
            if (srvFile.LastWriteTime != servicesJsonFileLastWriteTime)
            {
                return await LoadFile(filePath);
            }

            foreach (var srv in serviceData.ToArray())
            {
                var projFolder = Path.Combine(servicesJsonFile.Directory?.Parent?.FullName ?? "", srv.Service);
                var websiteFolder = Path.Combine(projFolder, "website");
                var launchSettings = Path.Combine(websiteFolder, "properties", "launchSettings.json");
                if (File.Exists(launchSettings))
                {
                    var launchSettingsAllText = File.ReadAllText(launchSettings);
                    var launchSettingsJObject = Newtonsoft.Json.Linq.JObject.Parse(launchSettingsAllText);
                    var appUrl = launchSettingsJObject["profiles"]["Website"]["applicationUrl"].ToString();
                    var port = appUrl.Substring(appUrl.LastIndexOf(":", StringComparison.Ordinal) + 1);

                    if (!int.TryParse(port, out var _))
                    {
                        var pos = 0;
                        var portNumer = "";
                        while (pos < port.Length - 1 && char.IsDigit(port[pos]))
                            portNumer += port[pos++];
                        port = portNumer;
                    }

                    srv.Port = port;
                }
                else
                {
                    srv.Status = MacroserviceGridItem.enumStatus.NoSourcerLocally;
                    srv.WebsiteFolder = null;
                    srv.ProcId = -1;
                    srv.VsDTE = null;
                    srv.Port = null;
                    srv.VsIsOpen = false;
                    continue;
                }


                srv.ProcId = GetProcessIdByPortNumber(srv.Port.To<int>());

                if (srv.WebsiteFolder.HasValue())
                {
                    srv.Status = srv.ProcId > 0 ? MacroserviceGridItem.enumStatus.Run : MacroserviceGridItem.enumStatus.Stop;

                    var gitUpdates = await GetGitUpdates(srv);
                    srv.GitUpdates = gitUpdates.ToString();
                    srv.VsDTE = GetVSDTE(srv);

                }
                else
                    srv.Status = MacroserviceGridItem.enumStatus.NoSourcerLocally;

            }

            return true;
        }

        void FetchProjectNugetPackages(MacroserviceGridItem service, MacroserviceGridItem.enumProjects projEnum)
        {
            string projFolder;
            switch (projEnum)
            {
                case MacroserviceGridItem.enumProjects.Website:
                    projFolder = service.WebsiteFolder;
                    break;
                case MacroserviceGridItem.enumProjects.Domain:
                    projFolder = Path.Combine(service.SolutionFolder, "Domain");
                    break;
                case MacroserviceGridItem.enumProjects.Model:
                    projFolder = Path.Combine(service.SolutionFolder, "M#", "Model");
                    break;
                case MacroserviceGridItem.enumProjects.UI:
                    projFolder = Path.Combine(service.SolutionFolder, "M#", "UI");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(projEnum), projEnum, null);
            }

            if (projFolder.IsEmpty()) return;
            var project = ".csproj".GetFisrtFile(projFolder);
            if (project.IsEmpty()) return;

            var serializer = new XmlSerializer(typeof(Classes.web.Project));
            Classes.web.Project proj;
            using (var fileStream = File.OpenRead(project))
                proj = (Classes.web.Project)serializer.Deserialize(fileStream);

            var nugetPackageRepo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
            var nugetPackageSet = new Dictionary<string, string>();
            //List<IPackage> _packages = new List<IPackage>();
            foreach (var itm in proj.ItemGroup)
                if (itm.PackageReference != null && itm.PackageReference.Length > 0)
                {

                    service.Projects[projEnum].PackageReferences = itm.PackageReference.Select(x =>
                        new NugetRef()
                        {
                            Include = x.Include,
                            Version = x.Version
                        }).ToList();

                    var nugetInitworker = new BackgroundWorker();
                    nugetInitworker.DoWork += (sender, args) =>
                    {
                        var srv = (MacroserviceGridItem)args.Argument;

                        foreach (MacroserviceGridItem.enumProjects prj in Enum.GetValues(typeof(MacroserviceGridItem.enumProjects)))
                        {

                            var packageReferences = srv.Projects[prj]?.PackageReferences;
                            if (packageReferences == null) continue;

                            var packages = nugetPackageRepo.FindPackages(packageReferences.Where(x => !nugetPackageSet.ContainsKey(x.Include))
                                                                                          .Select(x => x.Include))
                                                           .ToList();

                            foreach (var packageRef in packageReferences)
                            {
                                packageRef.NewVersion = packages.FirstOrDefault(package => package.IsLatestVersion)?.Version.ToFullString();

                                if (!nugetPackageSet.ContainsKey(packageRef.Include))
                                    nugetPackageSet.Add(packageRef.Include, packageRef.NewVersion);
                            }

                            packageReferences.Where(pr => pr.NewVersion.IsEmpty()).Do(pref =>
                            {
                                pref.NewVersion = nugetPackageSet[pref.Include];
                            });
                        }
                        args.Result = srv;

                    };

                    nugetInitworker.RunWorkerCompleted += (sender, args) =>
                    {
                        var srv = (MacroserviceGridItem)args.Result;
                        foreach (var projectRef in srv.Projects)
                            if (projectRef.Value.PackageReferences != null)
                                foreach (var nugetRef in projectRef.Value.PackageReferences)
                                    if (nugetRef.IsLatestVersion)
                                        srv.NugetUpdates++;

                        logWindow.LogMessage($"Run Worker Completed ({srv.Service})");
                        var worker = (BackgroundWorker)sender;
                        worker.Dispose();
                    };

                    nugetInitworker.RunWorkerAsync(service);
                }
        }


        FileSystemWatcher watcher;
        void StartFileSystemWatcher(FileInfo fileInfo)
        {
            if (watcher != null)
                StopWatcher();

            watcher = new FileSystemWatcher(fileInfo.Directory?.FullName ?? throw new InvalidOperationException($"File '{fileInfo.FullName}' does not exists anymore ..."), services_file_name);

            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;
        }

        void StopWatcher()
        {
            if (watcher == null) return;

            watcher.EnableRaisingEvents = false;
            watcher.Changed -= Watcher_Changed;
            watcher.Dispose();
            watcher = null;
        }
        public delegate void MyDelegate();

        void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!string.Equals(e.FullPath, servicesJsonFile.FullName,
                StringComparison.CurrentCultureIgnoreCase)) return;



            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    break;
                case WatcherChangeTypes.Deleted:
                    break;
                case WatcherChangeTypes.Changed:
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(async () => await Refresh()));
                    break;
                case WatcherChangeTypes.Renamed:
                    break;
                case WatcherChangeTypes.All:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
