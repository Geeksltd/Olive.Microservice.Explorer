using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MicroserviceExplorer
{
    partial class MainWindow
    {
        FileInfo ServicesJsonFile;
        public Visibility FileOpened { get; set; }

        const string Services_file_name = "services.json";
        bool ProjectLoaded;
        DateTime ServicesJsonFileLastWriteTime;

        FileSystemWatcher Watcher;

        bool LoadFile(string filePath)
        {
            ServicesJsonFile = filePath.AsFile();

            if (!ServicesJsonFile.Exists())
            {

                var result = MessageBox.Show($"file : {ServicesJsonFile.FullName} \ndoes not exist anymore. \nDo you want to removed it from recent files list?", "File Not Found", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
                _recentFiles.Remove(ServicesJsonFile.FullName);
                if (result != System.Windows.Forms.DialogResult.Yes) return false;

                SaveRecentFilesXml();
                ReloadRecentFiles();

                ServicesJsonFile = null;
                ProjectLoaded = false;
                return false;
            }

            ServicesJsonFileLastWriteTime = ServicesJsonFile.LastWriteTime;


            var servicesAllText = File.ReadAllText(ServicesJsonFile.FullName);
            var servicesJObject = Newtonsoft.Json.Linq.JObject.Parse(servicesAllText);
            txtFileInfo.Text = ServicesJsonFile.FullName;
            txtSolName.Text = servicesJObject["Solution"]["FullName"].ToString();

            var children = servicesJObject["Services"].Children();
            foreach (var jToken in children)
            {
                var serviceName = jToken.Path.Replace("Services.", "");
                var srv = ServiceData.SingleOrDefault(srvc => srvc.Service == serviceName);
                if (srv == null)
                {
                    srv = new MicroserviceItem();
                    ServiceData.Add(srv);
                }
                string liveUrl = null;
                string uatUrl = null;
                foreach (var url in jToken.Children())
                {
                    liveUrl = url["LiveUrl"].ToString();
                    uatUrl = url["UatUrl"].ToString();
                }

                var port = "";
                var status = MicroserviceItem.EnumStatus.NoSourcerLocally;
                var parentFullName = ServicesJsonFile.Directory?.Parent?.FullName ?? "";
                var projFolder = Path.Combine(parentFullName, serviceName);
                var websiteFolder = Path.Combine(projFolder, "website");
                var launchSettings = Path.Combine(websiteFolder, "properties", "launchSettings.json");
                var procId = -1;

                if (File.Exists(launchSettings))
                {
                    status = MicroserviceItem.EnumStatus.Pending;
                    port = GetPortNumberFromLaunchSettingsFile(launchSettings);
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
            }

            FilterListBy(txtSearch.Text);

            ProjectLoaded = true;

            if (Watcher == null)
                StartFileSystemWatcher(ServicesJsonFile);

            Refresh();

            return true;
        }

        bool RefreshFile(string filePath)
        {
            var srvFile = filePath.AsFile();
            if (srvFile.LastWriteTime != ServicesJsonFileLastWriteTime)
                return LoadFile(filePath);

            foreach (var srv in ServiceData.ToArray())
            {
                var projFolder = Path.Combine(ServicesJsonFile.Directory?.Parent?.FullName ?? "", srv.Service);
                var websiteFolder = Path.Combine(projFolder, "website");
                var launchSettings = Path.Combine(websiteFolder, "properties", "launchSettings.json");
                if (File.Exists(launchSettings))
                {
                    srv.Port = GetPortNumberFromLaunchSettingsFile(launchSettings);
                }
                else
                {
                    srv.Status = MicroserviceItem.EnumStatus.NoSourcerLocally;
                    srv.WebsiteFolder = null;
                    srv.ProcId = -1;
                    srv.VsDTE = null;
                    srv.Port = null;
                    srv.VsIsOpen = false;
                    continue;
                }

                if (srv.WebsiteFolder.IsEmpty())
                    srv.Status = MicroserviceItem.EnumStatus.NoSourcerLocally;
            }

            RestartAutoRefresh();
            RestartAutoRefreshProcess();
            return true;
        }

        static string GetPortNumberFromLaunchSettingsFile(string launchSettings)
        {
            var launchSettingsAllText = File.ReadAllText(launchSettings);
            var launchSettingsJObject = Newtonsoft.Json.Linq.JObject.Parse(launchSettingsAllText);
            var appUrl = launchSettingsJObject["profiles"]["Website"]["applicationUrl"].ToString();
            var port = appUrl.Substring(appUrl.LastIndexOf(":", StringComparison.Ordinal) + 1);

            if (port.TryParseAs<int>().HasValue)
                return port;

            var pos = 0;
            var portNumer = "";
            while (pos < port.Length - 1 && char.IsDigit(port[pos]))
                portNumer += port[pos++];
            port = portNumer;
            return port;
        }


        void StartFileSystemWatcher(FileInfo fileInfo)
        {
            if (Watcher != null)
                StopWatcher();

            Watcher = new FileSystemWatcher(fileInfo.Directory?.FullName ?? throw new InvalidOperationException($"File '{fileInfo.FullName}' does not exists anymore ..."), Services_file_name);

            Watcher.Changed += Watcher_Changed;
            Watcher.EnableRaisingEvents = true;
        }

        void StopWatcher()
        {
            if (Watcher == null) return;

            Watcher.EnableRaisingEvents = false;
            Watcher.Changed -= Watcher_Changed;
            Watcher.Dispose();
            Watcher = null;
        }
        public delegate void MyDelegate();

        void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!string.Equals(e.FullPath, ServicesJsonFile.FullName,
                StringComparison.CurrentCultureIgnoreCase)) return;



            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    break;
                case WatcherChangeTypes.Deleted:
                    break;
                case WatcherChangeTypes.Changed:
                    Refresh();
                    break;
                case WatcherChangeTypes.Renamed:
                    break;
                case WatcherChangeTypes.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("");
            }
        }

        async void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var service = GetServiceByTag(sender);

            var nugetUpdatesWindow = new NugetUpdatesWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                NugetList = service.OldReferences,
                Title = service.Service + " Microservice Nuget Updates"
            };

            var showDialog = nugetUpdatesWindow.ShowDialog();
            switch (showDialog)
            {
                case true:
                    await service.UpdateSelectedPackages();
                    break;
                default:
                    break;
            }
        }


        void BuildButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender1, e1) =>
            {
                service.BuildStatus = "Pending";
                service.LogMessage($"{service.Service} Microservice build started ...");
                foreach (SolutionProject projEnum in Enum.GetValues(typeof(SolutionProject)))
                {
                    var projFolder = service.GetAbsoluteProjFolder(projEnum);
                    if (projFolder.IsEmpty()) return;


                    try
                    {
                        service.BuildStatus = "Running";
                        var response = "dotnet.exe".AsFile(searchEnvironmentPath: true)
                            .Execute($"build", waitForExit: true,
                                configuration: x => x.StartInfo.WorkingDirectory = projFolder);

                    }
                    catch (Exception ex)
                    {
                        service.BuildStatus = "Failed";
                        service.LogMessage($"Build error on [{projEnum} :", ex.Message);
                        e1.Result = false;
                        return;
                    }
                    e1.Result = true;
                }
            };
            worker.RunWorkerCompleted += (o, args) =>
            {
                service.BuildStatus = "Running";
                var result = (bool)args.Result;
                if (result)
                    service.LogMessage($"{service.Service} Microservice build finished successfully.");
                else
                    service.BuildStatus = "Failed";
            };

            worker.RunWorkerAsync();


        }
    }
}
