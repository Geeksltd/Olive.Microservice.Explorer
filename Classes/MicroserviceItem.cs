using EnvDTE;
using EnvDTE80;
using MicroserviceExplorer.Annotations;
using MicroserviceExplorer.Classes.Web;
using MicroserviceExplorer.TCPIP;
using MicroserviceExplorer.Utils;
using NuGet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using Process = System.Diagnostics.Process;
using Thread = System.Threading.Thread;

namespace MicroserviceExplorer
{
    public enum SolutionProject { Website, Domain, Model, UI }

    public sealed class MicroserviceItem : INotifyPropertyChanged
    {
        public List<NugetReference> References = new List<NugetReference>();

        public List<NugetReference> OldReferences => References.Except(x => x.IsUpToDate).ToList();

        static IEnumerable<SolutionProject> StandardProjects
           => Enum.GetValues(typeof(SolutionProject)).OfType<SolutionProject>();

        public void RefreshPackages()
        {
            RollProgress($"Checking '{Service}' Microservice for nuget updates ...  ");
            References = new List<NugetReference>();
            foreach (var project in StandardProjects)
            {
                var settings = project.GetProjectFile(SolutionFolder);
                var refs = settings.ItemGroup.SelectMany(x => x.PackageReference.OrEmpty()).ToArray();
                var nugetRefs = refs.Select(x => new NugetReference(x, this, project))
                            .Except(x => x.Name.StartsWith("Microsoft.AspNetCore.")).ToList();

                References.AddRange(nugetRefs);
            }

            OnPropertyChanged(nameof(NugetUpdates));
            OnPropertyChanged(nameof(NugetUpdatesTooltip));
            StopProgress();
        }

        int _nugetFetchTasks;
        readonly double runImageOpacity = .2;

        #region INotifyPropertyChanged Implementations

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Overrides of ToString Object

        public override string ToString() => $"\'{Service}\' port : {Port} Status : {Status}";

        #endregion

        private readonly LogWindow Logwindow;
        public MainWindow MainWindow { get; set; }

        public void UpdateProgress(int progress, string msg = null)
        {
            MainWindow?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new MainWindow.MyDelegate(() =>
            {
                MainWindow.statusProgress.Value = progress;
                MainWindow.txtStatusMessage.Text = msg;
            }));
        }
        public void RollProgress(string msg = null)
        {
            MainWindow?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new MainWindow.MyDelegate(() =>
            {
                MainWindow.statusProgress.IsIndeterminate = true;
                MainWindow.txtStatusMessage.Text = msg;
            }));
        }

        public void StopProgress(string msg = null)
        {
            MainWindow?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new MainWindow.MyDelegate(() =>
            {
                MainWindow.statusProgress.Value = 0;
                MainWindow.statusProgress.IsIndeterminate = false;
                MainWindow.txtStatusMessage.Text = msg;
            }));

        }

        public MicroserviceItem()
        {
            Logwindow = new LogWindow { Servic = this };
        }
        public enum EnumStatus
        {
            NoSourcerLocally = 1,
            Stop = 2,
            Run = 3,
            Pending = 4
        }

        private bool FirstStatus = true;
        EnumStatus _status;
        public EnumStatus Status
        {
            get => _status;
            set
            {
                if (value != _status)
                    switch (value)
                    {
                        case EnumStatus.Run:
                            Logwindow.LogMessage("Service Started.");
                            break;
                        case EnumStatus.Stop:
                            if (FirstStatus)
                                FirstStatus = false;
                            else
                                Logwindow.LogMessage("Service Stoped.");
                            break;
                    }

                _status = value;

                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(RunImage));
                OnPropertyChanged(nameof(RunImageOpacity));
                OnPropertyChanged(nameof(ServiceColor));
                OnPropertyChanged(nameof(ServiceFontWeight));
                OnPropertyChanged(nameof(ServiceTooltip));
                OnPropertyChanged(nameof(VisibleDebug));
                OnPropertyChanged(nameof(VsCodeIcon));
            }
        }

        public string Service
        {
            get => _service;
            set
            {
                _service = value;
                Logwindow.Title = $"{Service} Microservice Log Window";
            }
        }

        public FontWeight ServiceFontWeight => Status == EnumStatus.Run ? FontWeights.Bold : FontWeights.Regular;

        public Brush ServiceColor
        {
            get
            {
                switch (Status)
                {
                    case EnumStatus.NoSourcerLocally:
                        return Brushes.DimGray;
                    case EnumStatus.Stop:
                        return Brushes.DarkRed;
                    case EnumStatus.Run:
                        return Brushes.Green;
                    default:
                        return Brushes.Black;
                }
            }
        }

        public string ServiceTooltip
        {
            get
            {
                switch (Status)
                {
                    case EnumStatus.NoSourcerLocally:
                        return "Source not available locally";
                    case EnumStatus.Stop:
                        return "Service Stopped locally";
                    case EnumStatus.Run:
                        return $"Service is Running locally ( '{ProcessName}' process Id : {ProcId})";
                    default:
                        return "";
                }
            }
        }

        public string BuildTooltip
        {
            get
            {
                switch (BuildStatus)
                {
                    case "off":
                        return "Building Microservice project has been stopped.";
                    case "Running":
                        return "Building Microservice project...";
                    case "Failed":
                        return "Building Microservice project has been failed.";
                    default:
                        return "Build Microservice Project";
                }
            }
        }

        public string Port { get; set; }

        public string LiveUrl { get; set; }
        public string UatUrl { get; set; }

        public object RunImage
        {
            get
            {
                switch (Status)
                {
                    case EnumStatus.Stop:
                        return "Resources/run2.png";
                    case EnumStatus.Run:
                        return "Resources/pause.png";
                    case EnumStatus.Pending:
                        return "Resources/gears.gif";
                    default:
                        return null;
                }

            }
        }


        public object BuildImage
        {
            get
            {
                switch (BuildStatus)
                {
                    case "off":
                        return "Resources/run2.png";
                    case "failed":
                        return "Resources/pause.png";
                    case "running":
                        return "Resources/gears.gif";
                    default:
                        return null;
                }

            }
        }

        public double RunImageOpacity => Status == EnumStatus.Run ? 1 : runImageOpacity;

        int _procId;
        public int ProcId
        {
            get => _procId;
            set
            {
                _procId = value;
                if (_procId > 0)
                {
                    ProcessName = Process.GetProcessById(_procId).ProcessName;
                    Status = EnumStatus.Run;
                }
                else
                {
                    ProcessName = null;
                    Status = EnumStatus.Stop;
                }

                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(ProcId));
                OnPropertyChanged(nameof(ProcessName));
                OnPropertyChanged(nameof(VisibleKestrel));
            }
        }

        public bool VsIsOpen { get; set; }

        public string ProcessName { get; private set; }

        string _websiteFolder;
        public string WebsiteFolder
        {
            get => _websiteFolder;
            set
            {
                _websiteFolder = value;
                OnPropertyChanged(nameof(WebsiteFolder));
                OnPropertyChanged(nameof(VisibleCode));
            }
        }

        public string SolutionFolder { get; set; }

        public object PortIcon => Port.TryParseAs<int>().HasValue ? null : "Resources/Warning.png";

        public string PortTooltip => PortIcon != null ? $"launchsettings.json File Not Found in this location :\n{WebsiteFolder}\\Properties\\launchSettings.json" : null;

        public Visibility VisibleCode => PortTooltip.IsEmpty() ? Visibility.Visible : Visibility.Hidden;

        public object VsCodeIcon => VsDTE == null ? "Resources/VS.png" : "Resources/VS2.png";

        public object Tag { get; set; }

        DTE2 _vsDTE;
        string _gitUpdates;

        public DTE2 VsDTE
        {
            get => _vsDTE;
            set
            {
                _vsDTE = value;
                OnPropertyChanged(nameof(VsDTE));
                OnPropertyChanged(nameof(VsCodeIcon));
                OnPropertyChanged(nameof(VisibleDebug));
                OnPropertyChanged(nameof(DebuggerIcon));
            }
        }

        public Visibility VisibleDebug => VsDTE == null || ProcId <= 0 ? Visibility.Collapsed : Visibility.Visible;

        public object DebuggerIcon
        {
            get
            {
                string icon = null;
                if (VsDTE != null)
                    try
                    {
                        icon = VsDTE.Mode == vsIDEMode.vsIDEModeDebug
                            ? "Resources/debug_stop.png"
                            : "Resources/debug.png";
                    }
                    catch (Exception e)
                    {
                        icon = "Resources/debug.png";
                    }

                OnPropertyChanged(nameof(VisibleDebug));
                return icon;
            }
        }

        internal async Task UpdateSelectedPackages()
        {
            NugetIsUpdating = true;

            OnPropertyChanged(nameof(NugetUpdates));
            OnPropertyChanged(nameof(NugetIsUpdating));

            var toUpdate = References.Where(x => x.ShouldUpdate && !x.IsUpToDate);

            foreach (var item in toUpdate)
                item.Update();

            NugetIsUpdating = false;

            OnPropertyChanged(nameof(NugetIsUpdating));
            OnPropertyChanged(nameof(NugetUpdates));
            OnPropertyChanged(nameof(NugetUpdatesTooltip));
        }

        public Visibility VisibleKestrel => ProcId <= 0 ? Visibility.Collapsed : Visibility.Visible;


        string _nugetUpdates;

        public bool NugetIsUpdating { get; set; }

        public string NugetUpdates
        {
            get
            {
                if (!NugetIsUpdating)
                {
                    if (OldReferences.None())
                    {
                        NugetUpdatesTooltip = "some";
                        return "...";
                    }
                    else
                    {
                        NugetUpdatesTooltip = OldReferences.Count().ToString();
                        return OldReferences.Count().ToString();
                    }
                }
                else
                {
                    return "Updating";
                }

            }
            set { _nugetUpdates = value; }
        }

        public string NugetUpdatesTooltip { get; set; }

        public string NugetUpdateErrorMessage { get; set; }

        public string GetAbsoluteProjFolder(SolutionProject projEnum)
        {
            string projFolder;
            switch (projEnum)
            {
                case SolutionProject.Website:
                    projFolder = WebsiteFolder;
                    break;
                case SolutionProject.Domain:
                    projFolder = Path.Combine(SolutionFolder, "Domain");
                    break;
                case SolutionProject.Model:
                    projFolder = Path.Combine(SolutionFolder, "M#", "Model");
                    break;
                case SolutionProject.UI:
                    projFolder = Path.Combine(SolutionFolder, "M#", "UI");
                    break;
                default:
                    projFolder = null;
                    break;
            }
            return projFolder;
        }

        string _nugetStatusImage;

        public string NugetStatusImage
        {
            get
            {
                switch (_nugetStatusImage)
                {
                    case "Pending":
                        return "Resources/loading.gif";
                    case "Warning":
                        return "Resources/warning1.gif";
                    default:
                        return null;
                }
            }
            set => _nugetStatusImage = value;
        }

        bool _nugetUpdateIsInProgress;
        public bool NugetUpdateIsInProgress
        {
            get => _nugetUpdateIsInProgress;
            set
            {
                _nugetUpdateIsInProgress = value;
                OnPropertyChanged(nameof(NugetUpdateIsInProgress));
                OnPropertyChanged(nameof(NugetFetchTasks));
            }
        }

        public int NugetFetchTasks
        {
            get => _nugetFetchTasks;
            set
            {
                if (value <= 0 && !NugetUpdateIsInProgress)
                {
                    NugetStatusImage = null;
                    value = 0;
                    NugetStatusImage = "Stop";
                    BuildStatus = null;
                }
                else
                {
                    NugetStatusImage = "Pending";
                    BuildStatus = "off";
                }

                _nugetFetchTasks = value;
                OnPropertyChanged(nameof(NugetFetchTasks));
                OnPropertyChanged(nameof(NugetStatusImage));
                OnPropertyChanged(nameof(NugetUpdateIsInProgress));

                OnPropertyChanged(nameof(BuildStatus));
                OnPropertyChanged(nameof(BuildIcon));
            }
        }
        // NUGET ----------------------------------------

        // BUILD ----------------------------------------
        string _buildStatus;

        public string BuildStatus
        {
            get { return _buildStatus; }
            set
            {
                _buildStatus = value;
                OnPropertyChanged(nameof(BuildIcon));
                OnPropertyChanged(nameof(BuildTooltip));
                OnPropertyChanged(nameof(BuildImage));
            }
        }

        public string BuildIcon
        {
            get
            {
                switch (BuildStatus?.ToLower())
                {
                    case "off":
                        return "Resources/build.gif";
                    case "pending":
                        return "Resources/building.gif";
                    case "running":
                        return "Resources/building.gif";
                    case "failed":
                        return "Resources/build-stop.gif";
                    default:
                        return "Resources/build.gif";
                }
            }
        }
        // BUILD ----------------------------------------

        // GIT --------------------------------------------------------

        public string GitUpdates
        {
            get => _gitUpdates;
            set
            {
                _gitUpdates = value;
                if (_gitUpdates == "0")
                    _gitUpdates = null;

                OnPropertyChanged(nameof(GitUpdates));
                OnPropertyChanged(nameof(GitUpdateImage));
            }
        }

        string _localGitUpdates = "...";
        public string LocalGitTooltip { get; set; }

        public Visibility LocalGitHasChange => _localGitUpdates != "..." ? Visibility.Visible : Visibility.Hidden;
        public string LocalGitChanges
        {
            get => _localGitUpdates;
            set
            {
                _localGitUpdates = value;

                OnPropertyChanged(nameof(LocalGitChanges));
                OnPropertyChanged(nameof(LocalGitTooltip));
                OnPropertyChanged(nameof(LocalGitHasChange));

            }
        }

        public object GitUpdateImage => GitUpdates.HasValue() ? "Resources/git.png" : null;

        public object GitStatusImage => GitUpdateIsInProgress ? "Resources/git_progress.gif" : null;

        bool _gitUpdateIsInProgress;
        public bool GitUpdateIsInProgress
        {
            get => _gitUpdateIsInProgress;
            set
            {
                _gitUpdateIsInProgress = value;
                if (_gitUpdateIsInProgress)
                    BuildStatus = "off";
                OnPropertyChanged(nameof(GitStatusImage));
            }
        }

        // GIT --------------------------------------------------------




        public void Start()
        {
            //AutoRefreshTimer.Stop();
            Status = EnumStatus.Pending;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run --no-build --project " + WebsiteFolder,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    //RedirectStandardOutput = true
                    WorkingDirectory = WebsiteFolder,
                    UseShellExecute = true,
                }
            };

            proc.Start();

            //Console.Beep();

            //var microserviceRunCheckingTimer = new DispatcherTimer { Tag = service };
            //microserviceRunCheckingTimer.Tick += microserviceRunCheckingTimer_Tick;
            //microserviceRunCheckingTimer.Interval = new TimeSpan(0, 0, 1);
            //microserviceRunCheckingTimer.Start();
        }

        public void Stop()
        {

            Status = EnumStatus.Pending;
            try
            {
                Process.GetProcessById(ProcId).Kill();
            }
            catch
            {
                // Already stopped.
                // No logging is needed.
            }
            Thread.Sleep(300);
            ProcId = GetProcessIdByPortNumber(Port.To<int>());
            if (ProcId < 0)
                Status = EnumStatus.Stop;
        }

        int GetProcessIdByPortNumber(int port)
        {
            var tcpRow = ManagedIpHelper.GetExtendedTcpTable(sorted: true)
                .FirstOrDefault(tcprow => tcprow.LocalEndPoint.Port == port);

            if (tcpRow == null) return -1;
            if (Process.GetProcessById(tcpRow.ProcessId).ProcessName.ToLower() != "dotnet") return -1;
            return tcpRow.ProcessId;
        }

        public void UpdateProcessStatus()
        {
            ProcId = GetProcessIdByPortNumber(Port.To<int>());
        }

        public async Task OpenVs(FileInfo solutionFile)
        {
            var dte2 = VsDTE ?? await GetVSDTE(solutionFile);
            if (dte2 != null)
            {
                dte2.MainWindow.Visible = true;
                dte2.MainWindow.SetFocus();
            }
            else
                Process.Start(solutionFile.FullName);
        }

        public async Task<DTE2> GetVSDTE() => await GetVSDTE(GetServiceSolutionFilePath());

        static async Task<DTE2> GetVSDTE(FileSystemInfo solutionFile)
        {
            if (solutionFile == null) return null;

            DTE2 result = null;

            await Task.WhenAny(Task.Delay(1.Seconds()),
                Task.Run(() => result = FindVSDTE(solutionFile)));

            return result;
        }

        static DTE2 FindVSDTE(FileSystemInfo solutionFile)
        {
            try
            {
                return new Helper().GetVsInstances().FirstOrDefault(dte2 => string.Equals(dte2.Solution.FullName,
                    solutionFile.FullName, StringComparison.CurrentCultureIgnoreCase));
            }
            catch
            {
                // Failed? 
                return null;
            }
        }


        public FileInfo GetServiceSolutionFilePath()
        {
            return !Directory.Exists(WebsiteFolder) ? null : WebsiteFolder.AsDirectory().Parent?.GetFiles("*.sln").FirstOrDefault();
        }

        private string _service;

        public void LogMessage(string message, string desc = null)
        {
            Logwindow.LogMessage(message, desc);
            OnPropertyChanged(nameof(LogWindowVisibility));
        }


        public void ShowLogWindow()
        {
            Logwindow.Show();
        }

        public Visibility LogWindowVisibility => Logwindow.TextLog.IsEmpty() ? Visibility.Collapsed : Visibility.Visible;
    }
}