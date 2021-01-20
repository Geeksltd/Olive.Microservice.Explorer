using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NuGet;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MicroserviceExplorer
{
    public class ServiceInfo
    {
        public string Name { get; set; }
        public string ProjectFolder { get; set; }
        public string WebsiteFolder { get; set; }
        public string LaunchSettingsPath { get; set; }
    }

    partial class MainWindow
    {
        
        public Visibility FileOpened { get; set; }

        bool projectLoaded;
        DateTime servicesDirectoryLastWriteTime;
        MicroserviceItem highPriority;
        FileSystemWatcher watcher;

        string GetServiceName(string appSettingPath)
        {
            var launchSettingsAllText = File.ReadAllText(appSettingPath);
            var launchSettingsJObject = Newtonsoft.Json.Linq.JObject.Parse(launchSettingsAllText);
            return launchSettingsJObject["Microservice"]["Me"]["Name"].ToString();
        }

      
        

        bool RefreshFile(string filePath)
        {
            var srvFile = filePath.AsFile();
            if (srvFile.LastWriteTime != servicesDirectoryLastWriteTime)
                return LoadFile(filePath);

            foreach (var srv in ServiceData.ToArray())
            {
                var projFolder = Path.Combine(ServicesDirectory?.FullName ?? "", srv.SolutionFolder);
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

            if (port.TryParseAs<int>().HasValue) return port;

            var pos = 0;
            var portNumer = "";
            while (pos < port.Length - 1 && char.IsDigit(port[pos]))
                portNumer += port[pos++];
            port = portNumer;
            return port;
        }

        void StartFileSystemWatcher(DirectoryInfo directoryInfo)
        {
            if (watcher != null) StopWatcher();

            watcher = new FileSystemWatcher(directoryInfo.FullName ?? throw new InvalidOperationException($"Directory '{directoryInfo.FullName}' does not exists anymore ..."), directoryInfo.FullName);
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
            if (!string.Equals(e.FullPath, ServicesDirectory.FullName,
                StringComparison.CurrentCultureIgnoreCase)) return;

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Renamed:
                case WatcherChangeTypes.All:
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Deleted:
                    break;
                case WatcherChangeTypes.Changed:
                    Refresh();
                    break;

                default:
                    throw new ArgumentOutOfRangeException("");
            }
        }

        async void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var service = GetServiceByTag(sender);

            highPriority = service;
            OnAutoRefreshTimerOnTick(null, null);

            var nugetUpdatesWindow = new NugetUpdatesWindow(service.NugetIsUpdating)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                NugetList = service.OldReferences,
                Title = service.Service + " Microservice Nuget Updates",
            };

            var showDialog = nugetUpdatesWindow.ShowDialog();
            switch (showDialog)
            {
                case true:
                    service.UpdateSelectedPackages();
                    break;
                default:
                    break;
            }
        }

        void LocalGitActions_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var service = GetServiceByTag(sender);

            var projFOlder = service.WebsiteFolder.AsDirectory().Parent;
            var hubAddress = Path.Combine(ServicesDirectory.FullName, "hub");

            var localGitWindow = new LocalGitWindow(projFOlder.FullName, hubAddress, service.Service)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = service.Service + " Microservice Local Git",
            };

            var showDialog = localGitWindow.ShowDialog();
        }

        void BuildButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);

            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += (sender1, e1) =>
                {
                    service.BuildStatus = "Pending";
                    service.LogMessage($"{service.Service} Microservice build started ...");

                    try
                    {
                        var processInfo = new ProcessStartInfo();
                        processInfo.FileName = "CMD.EXE";
                        processInfo.WorkingDirectory = service.SolutionFolder;
                        processInfo.Arguments = "/K " + Path.Combine(service.SolutionFolder, "Build.bat");
                        var process = Process.Start(processInfo);
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        service.BuildStatus = "Failed";
                        service.LogMessage($"Build error.", ex.Message);
                        e1.Result = false;
                        return;
                    }

                    e1.Result = true;
                };
                worker.RunWorkerCompleted += (o, args) =>
                {
                    var result = (bool)args.Result;
                    if (result)
                    {
                        service.LogMessage($"{service.Service} Microservice build finished successfully.");
                        service.BuildStatus = "off";
                    }
                    else
                        service.BuildStatus = "Failed";
                };

                worker.RunWorkerAsync();
            }
        }
    }
}