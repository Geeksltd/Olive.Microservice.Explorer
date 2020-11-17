using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using EnvDTE;
using MicroserviceExplorer.MicroserviceGenerator;
using MicroserviceExplorer.UI;
using MicroserviceExplorer.Utils;
using Newtonsoft.Json.Linq;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.Forms.MessageBox;
using Process = System.Diagnostics.Process;

namespace MicroserviceExplorer
{
    public partial class MainWindow : IDisposable
    {
        #region Commands

        public static readonly RoutedCommand EditCommand = new RoutedUICommand("Edit", "EditCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.E, ModifierKeys.Control)
        }));

        public static readonly RoutedCommand RefreshCommand = new RoutedUICommand("Refresh", "RefreshCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.R, ModifierKeys.Control)
        }));

        public static readonly RoutedCommand CloseCommand = new RoutedUICommand("Close", "CloseCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.Q, ModifierKeys.Control)
        }));

        public static readonly RoutedCommand ExitCommand = new RoutedUICommand("Exit", "ExitCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.Q, ModifierKeys.Control | ModifierKeys.Shift)
        }));
        public static readonly RoutedCommand NewMicroserviceCommand = new RoutedUICommand("NewMicroservice", "NewMicroserviceCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.M, ModifierKeys.Control | ModifierKeys.Shift)
        }));
        public static readonly RoutedCommand RunAllCommand = new RoutedUICommand("RunAll", "RunAllCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.L, ModifierKeys.Control )
        }));

        public static readonly RoutedCommand StopAllCommand = new RoutedUICommand("StopAll", "StopAllCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Shift)
        }));

        public static readonly RoutedCommand RunAllFilteredCommand = new RoutedUICommand("RunAllFiltered", "RunAllFilteredCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Alt)
        }));

        public static readonly RoutedCommand StopAllFilteredCommand = new RoutedUICommand("StopAllFiltered", "StopAllFilteredCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift)
        }));

        public static readonly RoutedCommand AlwaysOnTopCommand = new RoutedUICommand("AlwaysOnTop", "AlwaysOnTopCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.T, ModifierKeys.Control)
        }));

        #endregion
        readonly DispatcherTimer AutoRefreshTimer = new DispatcherTimer();
        readonly DispatcherTimer AutoRefreshProcessTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitNotifyIcon();

            InitializeComponent();
            DataContext = MicroserviceGridItems;

            AutoRefreshTimer.Tick += OnAutoRefreshTimerOnTick;
            AutoRefreshTimer.Interval = new TimeSpan(0, 3, 0);

            AutoRefreshProcessTimer.Tick += OnAutoRefreshProcessTimerOnTick;
            AutoRefreshProcessTimer.Interval = new TimeSpan(0, 0, 2);
        }

        void RestartAutoRefreshProcess()
        {
            highPriority = null;
            AutoRefreshProcessTimer.Stop();
            OnAutoRefreshProcessTimerOnTick(null, null);
            AutoRefreshProcessTimer.Start();
        }

        bool AutoRefreshProcessTimerInProgress;
        void OnAutoRefreshProcessTimerOnTick(object sender, EventArgs args)
        {
            return;

            if (AutoRefreshProcessTimerInProgress) return;

            AutoRefreshProcessTimer.IsEnabled = false;
            foreach (var service in MicroserviceGridItems)
            {
                if (service.WebsiteFolder.IsEmpty() || service.Port.IsEmpty()) continue;
                using (var backgroundWorker = new BackgroundWorker())
                {
                    backgroundWorker.DoWork += async (sender1, e) =>
                    {
                        service.UpdateProcessStatus();
                        service.VsDTE = await service.GetVSDTE();
                    };

                    backgroundWorker.RunWorkerAsync();
                }
            }

            AutoRefreshProcessTimer.IsEnabled = true;
        }

        void RestartAutoRefresh()
        {
            AutoRefreshTimer.Stop();
            OnAutoRefreshTimerOnTick(null, null);
            AutoRefreshTimer.Start();
        }

        bool AutoRefreshTimerInProgress;
        bool firstRun = true;
        async void OnAutoRefreshTimerOnTick(object sender, EventArgs args)
        {
            var projects = MicroserviceGridItems.Where(x => x.WebsiteFolder.HasValue()).ToArray();
            if (highPriority != null)
            {
                await Task.Run(() => FetchUpdates(highPriority)).ContinueWith(async (t) =>
                {
                    foreach (var p in projects.Except(highPriority))
                        await Task.Run(() => FetchUpdates(p));
                });
            }
            else
            {
                if (AutoRefreshTimerInProgress) return;
                else AutoRefreshTimerInProgress = true;

                int waitTime = 0;
                if (firstRun)
                {
                    waitTime = 15;
                    firstRun = false;
                }

                await Task.Factory.StartNew(() => System.Threading.Thread.Sleep(waitTime * 1000))
                       .ContinueWith(async (t) =>
                       {
                           foreach (var p in projects)
                               await Task.Run(async () =>
                               {
                                   if (highPriority == null) await FetchUpdates(p);
                               });
                       });
            }

            AutoRefreshTimerInProgress = false;
        }

        static IEnumerable<SolutionProject> SolutionProjects
            => Enum.GetValues(typeof(SolutionProject)).OfType<SolutionProject>();

        Task FetchUpdates(MicroserviceItem service)
        {
            // Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(() =>
            // {

            // }));

            var refresh = Task.Factory.StartNew(service.RefreshPackages, TaskCreationOptions.LongRunning);
            // var git = Task.Factory.StartNew(async () => { LocalGitChanges(service); await CalculateGitUpdates(service); }, TaskCreationOptions.LongRunning);

            return refresh;
            // return Task.WhenAll(refresh, git);
        }

        void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(RecentsXml))
            {
                OpenProject_Executed(sender, null);
                if (!projectLoaded) ExitMenuItem_Click(sender, e);
                return;
            }

            ReloadRecentFiles();
            // logWindow.ShowInTaskbar = false;
            // logWindow.Hide();
            var recentFilesCount = _recentFiles.Count - 1;

            while (recentFilesCount > 0)
            {
                if (LoadFile(_recentFiles[recentFilesCount]))
                    break;
                recentFilesCount--;
            }

            if (projectLoaded) return;

            File.Delete(RecentsXml);
            MainWindow_OnLoaded(sender, e);
        }

        void StartStop_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            switch (service.Status)
            {
                case MicroserviceItem.EnumStatus.Pending:
                    MessageBox.Show("Microservice is loading.\nPlease Wait ...", @"Loading ...");
                    break;
                case MicroserviceItem.EnumStatus.Run:
                    service.Stop();
                    break;
                case MicroserviceItem.EnumStatus.Stop:
                    service.Start();
                    break;
                case MicroserviceItem.EnumStatus.NoSourcerLocally:
                    break;
                default:
                    throw new ArgumentOutOfRangeException($@"Service.Status out of range");
            }
        }

        // void microserviceRunCheckingTimer_Tick(object sender, EventArgs e)
        // {
        //    var timer = (DispatcherTimer)sender;
        //    var service = (MicroserviceItem)timer.Tag;
        //    service.UpdateProcessStatus();
        //    if (service.ProcId < 0) return;

        //    timer.Stop();
        //    timer.Tick -= microserviceRunCheckingTimer_Tick;
        //    service.Status = MicroserviceItem.EnumStatus.Run;
        //    AutoRefreshTimer.Start();
        // }

        void OpenCode_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            var solutionFile = service.GetServiceSolutionFilePath();
            service.OpenVs(solutionFile);
        }

        void OpenProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (var openFileDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = false,
                Description = "Select your Hub folder"
            })
            {
                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                if (_recentFiles.None() || !_recentFiles.Contains(openFileDialog.SelectedPath))
                {
                    if (_recentFiles.None())
                        mnuRecentFiles.Items.Clear();

                    _recentFiles.Add(openFileDialog.SelectedPath);
                    AddRecentMenuItem(openFileDialog.SelectedPath);
                    SaveRecentFilesXml();
                }

                LoadFile(openFileDialog.SelectedPath);
            }
        }

        void Refresh()
        {
            AutoRefreshProcessTimer.Stop();
            AutoRefreshTimer.Stop();

            if (ServicesDirectory != null)
                Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(() =>
                {
                    try
                    {
                        RefreshFile(ServicesDirectory.FullName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                })
                );
        }

        void CloseMenuItem_OnClick(object sender, RoutedEventArgs e) => Close();

        void RefreshMenuItem_OnClick(object sender, ExecutedRoutedEventArgs e) => Refresh();

        void OpenExplorer_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            Process.Start(service.WebsiteFolder.AsDirectory().Parent?.FullName ??
                          throw new Exception("Microservice projFolder Not Exists ..."));
        }

        void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterListBy(txtSearch.Text);
        }

        async void VsDebuggerAttach_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);

            if (service.VsDTE.Mode == vsIDEMode.vsIDEModeDebug)
            {
                service.VsDTE.Debugger.DetachAll();
                return;
            }

            var processes = service.VsDTE.Debugger.LocalProcesses.OfType<EnvDTE.Process>();
            var process = processes.SingleOrDefault(x => x.ProcessID == service.ProcId);
            if (process == null) return;

            await service.OpenVs(solutionFile: service.GetServiceSolutionFilePath());
            process.Attach();
        }

        void RunAllMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var service in MicroserviceGridItems)
                if (service.Status == MicroserviceItem.EnumStatus.Stop)
                    service.Start();
        }

        void StopAllMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var service in MicroserviceGridItems)
                if (service.Status == MicroserviceItem.EnumStatus.Run)
                    service.Stop();
        }

        void RunAllFilteredMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var service in MicroserviceGridItems)
                if (service.Status == MicroserviceItem.EnumStatus.Stop)
                    service.Start();
        }

        void StopAllFilteredMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var service in MicroserviceGridItems)
            {
                if (service.Status == MicroserviceItem.EnumStatus.Run)
                    service.Stop();
            }
        }

        void WindowTitlebarControl_OnRefreshClicked(object sender, EventArgs e) => Refresh();

        void MnuAlwaysOnTop_OnChecked(object sender, RoutedEventArgs e)
        {
            Topmost = mnuAlwaysOnTop.IsChecked;
        }

        void WindowOpacityMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            var template = SliderMenuItem.Template;
            var slider = (Slider)template.FindName("OpacitySlider", SliderMenuItem);
            slider.Value = menuItem.Header.ToString().TrimEnd('%').To<int>();
        }

        void OpacitySlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Opacity = e.NewValue / 100d;
            foreach (MenuItem item in OpacityMenuItem.Items)
            {
                if (item.Header == null || !item.Header.ToString().EndsWith("%"))
                    continue;

                item.IsChecked = Math.Abs(item.Header.ToString().TrimEnd('%').To<int>() - e.NewValue) < .0001;
            }
        }

        async void GitUpdate_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            await GitUpdate(service);
        }

        void MainWindow_OnLocationChanged(object sender, EventArgs e)
        {
            // if (logWindow.IsVisible)
            //    logWindow.SetTheLogWindowBy(this);
        }

        void ShowKestrelLog_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            var pid = service.ProcId;
            IntPtr mainWindowHandle;
            do
            {
                mainWindowHandle = Process.GetProcessById(pid).MainWindowHandle;
                var pr = ParentProcessUtilities.GetParentProcess(pid);
                pid = pr?.Id ?? 0;
            } while (mainWindowHandle == IntPtr.Zero && pid != 0);

            if (mainWindowHandle != IntPtr.Zero)
                WindowApi.ShowWindow(mainWindowHandle);
            else
            {
                MessageBox.Show("Last Kestrel process was attached to none console window style.\n So if you want to see Kestrel log window, please stop and start Microserice again.", "There is not kestrel window-habdle");
            }
        }

        public void Dispose()
        {
            notifyIcon.Dispose();
            watcher.Dispose();
        }

        void ShowServiceLog_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            service.ShowLogWindow();
        }

        void UpdateAllNuget_Click(object sender, RoutedEventArgs e)
        {
            ServiceData.SelectMany(x => x.References).Do(x => x.ShouldUpdate = true);
            ServiceData.ForEach(x => x.UpdateSelectedPackages());
        }

        async void NewMicroservice_Click(object sender, ExecutedRoutedEventArgs e)
        {
            var hubAddress = Path.Combine(ServicesDirectory.FullName, "hub");

            var msw = new NewMicroservice.NewMicroservice
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var dialog = msw.ShowDialog();
            if (dialog != true) return;

            if (!ServicesDirectory.Exists || ServicesDirectory == null) return;

            var appSettingsProductionAllText = File.ReadAllText(Path.Combine(hubAddress, "website", "appsettings.Production.json"));
            var appSettingsProductionJObject = JObject.Parse(appSettingsProductionAllText);
            var domain = appSettingsProductionJObject["Authentication"]["Cookie"]["Domain"].ToString();
            var serviceName = msw.ServiceName;
            var solutionName = ServicesDirectory.Name;
            var portNumber = GetNextPortNumberFromHubServices(ServicesDirectory, out var serviesXmlPath);
            var dbType = DBType.SqlServer;
            var connectionString = dbType.ConnectionString;

            // AddMicroserviceToHubServices(serviesXmlPath, serviceName, domain, portNumber);

            var serviceDirectoryPath = Path.Combine(ServicesDirectory.FullName, serviceName);
            var serviceDirectory = new DirectoryInfo(serviceDirectoryPath);

            if (!serviceDirectory.Exists)
                serviceDirectory.Create();

            var tmpFolder = await CreateTemplateAsync(serviceDirectoryPath, serviceName, solutionName, domain, portNumber.ToString(),
                dbType, connectionString);

            if (tmpFolder.IsEmpty()) return;

            AddMicroserviceToHubServices(serviesXmlPath, serviceName, domain, portNumber);
            Execute(serviceDirectoryPath, "build.bat", null);

            try
            {
                Execute(serviceDirectoryPath, "git", "init");
                Execute(serviceDirectoryPath, "git", $"remote add origin {msw.GitRepoUrl}");
                Execute(serviceDirectoryPath, "git", "add .");
                Execute(serviceDirectoryPath, "git", "commit -m \"Initial commit\"");
                Execute(serviceDirectoryPath, "git", "push");
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Error :{ex.Message}", @"Template initialization failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Refresh();
        }

        void AddMicroserviceToHubServices(string serviesXmlPath, string serviceName, string domain, int portNumber)
        {
            var services = XDocument.Load(serviesXmlPath);
            var node = new XElement(serviceName,
                new XAttribute("url", $"http://localhost:{portNumber}"),
                new XAttribute("production", $"https://{serviceName}.{domain}"));

            services.Root?.AddFirst(node);
            services.Save(serviesXmlPath);
        }

        int GetNextPortNumberFromHubServices(DirectoryInfo serviceJsonDir, out string serviesXmlPath)
        {
            serviesXmlPath = null;
            var hubDir = serviceJsonDir;
            while (hubDir?.Parent != null && !Directory.Exists(Path.Combine(hubDir.FullName, "hub")))
                hubDir = hubDir.Parent;

            if (hubDir == null) return 0;

            var servicePath = Path.Combine(hubDir.FullName, "hub", "website", "services.xml");
            if (!File.Exists(servicePath)) return 0;

            serviesXmlPath = servicePath;

            var services = XElement.Load(servicePath);
            var nextPortNumber = (from srv in services.FirstNode.ElementsAfterSelf()
                                  where srv.Attribute("url") != null && srv.Attribute("url").Value.Replace("://", "").Contains(":")
                                  let y = srv.Attribute("url")?.Value.Replace("://", "").TrimBefore(":", caseSensitive: true, trimPhrase: true)
                                  where int.TryParse(y, out _)
                                  select Convert.ToInt32(y)).Max() + 1;
            return nextPortNumber;
        }

        public string TemplateWebAddress => @"https://github.com/Geeksltd/Olive.Mvc.Microservice.Template/archive/master.zip";
        const string ZIP_FILE_NAME = "master.zip", TEMPLATE_FOLDER_NAME = "Template";

        async Task<string> CreateTemplateAsync(string solutionFolder, string serviceName, string solutionName, string domain, string port, DBType selectedDbType, string connestionString)
        {
            // DownloadedFilesExtractPath = Path.Combine(solutionFolder, "tempExtract");
            var downloadedFilesExtractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(downloadedFilesExtractPath);
            try
            {
                if (!await DownloadAsync(TemplateWebAddress, downloadedFilesExtractPath, ZIP_FILE_NAME)) return null;

                var zipFilePath = Path.Combine(downloadedFilesExtractPath, ZIP_FILE_NAME);
                ZipFile.ExtractToDirectory(zipFilePath, downloadedFilesExtractPath);
                File.Delete(zipFilePath);
                Rename(downloadedFilesExtractPath, serviceName, solutionName, domain, port, selectedDbType, connestionString);

                CopyFiles(solutionFolder, downloadedFilesExtractPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"Error in download or create new Microservice", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return downloadedFilesExtractPath;
        }

        void CopyFiles(string solutionFolder, string extractPathPath)
        {
            try
            {
                var template = Directory.GetDirectories(extractPathPath).FirstOrDefault();
                if (template.IsEmpty()) return;
                var templateDirectory = Directory.GetDirectories(template).FirstOrDefault();
                var tempDIrObj = new DirectoryInfo(templateDirectory);
                if (tempDIrObj.Name != TEMPLATE_FOLDER_NAME) return;
                CopyFolderContents(templateDirectory, solutionFolder);
                DeleteDirectory(extractPathPath);
            }
            catch
            {
                // ignored
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;

            // Delete all files from the Directory
            foreach (var file in Directory.GetFiles(path))
                File.Delete(file);

            // Delete all child Directories
            foreach (var directory in Directory.GetDirectories(path))
                DeleteDirectory(directory);

            // Delete a Directory
            Directory.Delete(path);
        }

        public static bool CopyFolderContents(string sourcePath, string destinationPath)
        {
            sourcePath = sourcePath.EndsWith(@"\") ? sourcePath : sourcePath + @"\";
            destinationPath = destinationPath.EndsWith(@"\") ? destinationPath : destinationPath + @"\";

            try
            {
                if (Directory.Exists(sourcePath))
                {
                    if (Directory.Exists(destinationPath) == false)
                        Directory.CreateDirectory(destinationPath);

                    foreach (var files in Directory.GetFiles(sourcePath))
                    {
                        var fileInfo = files.AsFile();
                        fileInfo.CopyTo($@"{destinationPath}\{fileInfo.Name}", overwrite: true);
                    }

                    foreach (var drs in Directory.GetDirectories(sourcePath))
                    {
                        var directoryInfo = new DirectoryInfo(drs);
                        if (CopyFolderContents(drs, destinationPath + directoryInfo.Name) == false)
                            return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected void Rename(string downloadedFilesExtractPath, string serviceName, string solutionName, string domain, string port, DBType selectedDbType, string connestionString)
        {
            domain = domain.TrimStart('*').TrimStart('.');
            var replacements = new Dictionary<string, string>
            {
                {"MY.MICROSERVICE.NAME", serviceName},
                {"MY.SOLUTION",solutionName  },
                {"my-solution-domain",domain },
                {"mysolution",domain.Remove(".")},
                {"9012", port }
            };

            foreach (var item in replacements)
                RenameHelper(downloadedFilesExtractPath, item.Key, item.Value);

            ApplyDbType(downloadedFilesExtractPath, selectedDbType, connestionString);
            // SetRandomPortNumber(downloadedFilesExtractPath);
        }

        protected static void ApplyDbType(string destPath, DBType selectedDbType, string constr)
        {
            ChangeConnectionString(destPath, constr);

            if (selectedDbType == DBType.SqlServer) return;

            FixDomainCsProjectReference(destPath, selectedDbType);
            FixSqlDialect(destPath, selectedDbType);
            FixStartUp(destPath, selectedDbType);
        }

        static void FixSqlDialect(string destPath, DBType selectedDbType)
        {
            var file = Directory.GetFiles(destPath, "Project.cs", SearchOption.AllDirectories).FirstOrDefault(x => x.Contains("M#\\Model\\"));
            if (file.IsEmpty()) throw new Exception("M#\\Model\\Project.cs was not found!");

            // var file = Path.Combine(destPath, "Model", "Project.cs");

            var text = File.ReadAllText(file);
            var index = text.IndexOf("\n", text.IndexOf("Name(\"", StringComparison.Ordinal), StringComparison.Ordinal) + 1;
            text = text.Insert(index, $"SqlDialect(MSharp.SqlDialect.{selectedDbType.Dialect});");

            File.WriteAllText(file, text);
        }

        static void FixStartUp(string destPath, DBType selectedDbType)
        {
            var file = Directory.GetFiles(destPath, "StartUp.cs", SearchOption.AllDirectories).SingleOrDefault();
            if (file.IsEmpty()) throw new Exception("StartUp.cs was not found!");

            var text = File.ReadAllText(file).ReplaceWholeWord("SqlServerManager", selectedDbType.Manager);
            File.WriteAllText(file, text);
        }

        static void FixDomainCsProjectReference(string destPath, DBType selectedDbType)
        {
            var domainFile = Directory.GetFiles(destPath, "Domain.csproj", SearchOption.AllDirectories).FirstOrDefault();
            if (domainFile.IsEmpty()) return;

            var serializer = new XmlSerializer(typeof(MicroserviceGenerator.Schema.Project));

            MicroserviceGenerator.Schema.Project proj;
            using (var fileStream = File.OpenRead(domainFile))
                proj = (MicroserviceGenerator.Schema.Project)serializer.Deserialize(fileStream);

            var sqlServerReference = proj.ItemGroup.Single(x => x.Include == "Olive.Entities.Data.SqlServer");
            sqlServerReference.Include = "Olive.Entities.Data." + selectedDbType.Provider;
            sqlServerReference.Version = selectedDbType.OliveVersion;

            var newProjStruct = SerializeToString(proj);
            File.WriteAllText(domainFile, newProjStruct);
        }

        public static string SerializeToString<T>(T value)
        {
            var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(value.GetType());
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, value, emptyNamepsaces);
                return stream.ToString();
            }
        }

        static void ChangeConnectionString(string destPath, string constr)
        {
            var jsonkey = @"""AppDatabase""";
            var appsettings = Directory.GetFiles(destPath, "appsettings.json", SearchOption.AllDirectories).Single(f => f.AsFile().DirectoryName?.ToLower().EndsWith("\\website") == true);
            var allLines = File.ReadAllLines(appsettings);
            for (var index = 0; index < allLines.Length; index++)
            {
                var line = allLines[index];
                if (line.Lacks(jsonkey)) continue;
                var hasComma = line.Trim().EndsWith(",");

                allLines[index] = $"\t{jsonkey}: \"{constr}\" {(hasComma ? "," : "")}";
                break;
            }

            File.WriteAllLines(appsettings, allLines);
        }

        protected static void RenameHelper(string destPath, string templateKey, string templateValue)
        {
            foreach (var file in Directory.GetFiles(destPath, "*.*", SearchOption.AllDirectories))
            {
                var fileContent = File.ReadAllText(file);
                if (fileContent.Contains(templateKey, caseSensitive: false))
                {
                    fileContent = fileContent.Replace(templateKey, templateValue);
                    fileContent = fileContent.Replace(templateKey.ToLower(), templateValue.ToLowerOrEmpty());

                    File.WriteAllText(file, fileContent);
                }

                var fileName = Path.GetFileName(file);
                if (fileName.Contains(templateKey))
                    File.Move(file, file.Replace(templateKey, templateValue));
            }
        }

        public static async System.Threading.Tasks.Task<bool> DownloadAsync(string sourceWebAddress, string destPath, string fileName)
        {
            var destFullPath = Path.Combine(destPath, fileName);
            try
            {
                Loading loadingWindow;
                HttpResponseMessage response;
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(new TimeSpan(0, 0, 2, 30));
                    loadingWindow = new Loading(cancellationTokenSource);

                    loadingWindow.Show();
                    var httpClient = new HttpClient();
                    response = await httpClient.GetAsync(sourceWebAddress, HttpCompletionOption.ResponseContentRead, cancellationTokenSource.Token);
                }

                response.EnsureSuccessStatusCode();

                using (var fileStream = new FileStream(destFullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    await response.Content.CopyToAsync(fileStream);

                loadingWindow.Close();
                return true;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error in downloading template file \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OperationCanceledException ex)
            {
                MessageBox.Show("Project template download canceled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in downloading template file \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        public static void Execute(string workingDirectory, string command, string args, bool createNoWindow = true)
        {
            var output = new StringBuilder();
            var proc = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {

                    FileName = command,
                    Arguments = args ?? "",
                    WorkingDirectory = workingDirectory ?? "",
                    CreateNoWindow = createNoWindow,
                    UseShellExecute = true,
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    //Verb = "runas",
                    //StandardOutputEncoding = Encoding.UTF8,
                    //StandardErrorEncoding = Encoding.UTF8
                }
            };

            // proc.ErrorDataReceived += (sender, e) =>
            // {
            //    if (e.Data.IsEmpty()) return;
            //    output.AppendLine(e.Data);
            // };

            // proc.OutputDataReceived += (DataReceivedEventHandler)((sender, e) =>
            // {
            //    if (e.Data == null) return;
            //    output.AppendLine(e.Data);
            // });

            // proc.Exited += (object sender, EventArgs e) =>
            // {
            //    //Console.Beep();
            // };
            proc.Start();

            // proc.BeginOutputReadLine();
            // proc.BeginErrorReadLine();

            proc.WaitForExit();

            if ((uint)proc.ExitCode > 0U)
                throw new Exception($"External command failed: \"{command}\" {args}\r\n\r\n{(object)output}");

            // return output.ToString();
        }
    }
}

namespace System
{
    public static class TempExtDeleteMeAfterNugetUpdate
    {
        public static string GetFisrtFile(this string @this, string basePath)
        {
            if (!@this.Contains("*")) @this = "*" + @this;

            var path = basePath.AsDirectory();
            if (!path.Exists) return null;

            var file = path.GetFiles(@this).FirstOrDefault();
            return file?.FullName;
        }
    }
}