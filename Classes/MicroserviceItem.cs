using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using EnvDTE;
using EnvDTE80;
using MicroserviceExplorer.Annotations;
using MicroserviceExplorer.Classes.Web;
using MicroserviceExplorer.TCPIP;
using MicroserviceExplorer.Utils;
using NuGet;
using Process = System.Diagnostics.Process;
using Thread = System.Threading.Thread;

namespace MicroserviceExplorer
{
    public sealed class MicroserviceItem : INotifyPropertyChanged
    {
        public readonly Dictionary<EnumProjects, ProjectRef> Projects = new Dictionary<EnumProjects, ProjectRef>
        {
            { EnumProjects.Website , new ProjectRef()},
            { EnumProjects.Domain , new ProjectRef()},
            { EnumProjects.Model , new ProjectRef()},
            { EnumProjects.UI , new ProjectRef()},
        };

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

        public enum EnumStatus
        {
            NoSourcerLocally = 1,
            Stop = 2,
            Run = 3,
            Pending = 4
        }
        EnumStatus _status;
        public EnumStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(RunImage));
                OnPropertyChanged(nameof(RunImageOpacity));
                OnPropertyChanged(nameof(ServiceColor));
                OnPropertyChanged(nameof(ServiceFontWeight));
                OnPropertyChanged(nameof(ServiceTooltip));
                OnPropertyChanged(nameof(VisibleDebug));
            }
        }

        public string Service { get; set; }

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

        public double RunImageOpacity => Status == EnumStatus.Run ? 1 : runImageOpacity;

        int _procId;
        public int ProcId
        {
            get => _procId;
            set
            {
                _procId = value;
                if (_procId > 0)
                    ProcessName = Process.GetProcessById(_procId).ProcessName;

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
            }
        }

        public Visibility VisibleDebug => VsDTE == null || ProcId <= 0 ? Visibility.Collapsed : Visibility.Visible;

        public object DebuggerIcon
        {
            get
            {
                if (VsDTE != null)
                    return VsDTE.Mode == vsIDEMode.vsIDEModeDebug
                        ? "Resources/debug_stop.png"
                        : "Resources/debug.png";

                OnPropertyChanged(nameof(VisibleDebug));
                return null;
            }
        }

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

        public object GitUpdateImage => GitUpdates.HasValue() ? "Resources/git.png" : null;

        public object GitStatusImage => GitUpdateIsInProgress ? "Resources/git_progress.gif" : null;



        public Visibility VisibleKestrel => ProcId <= 0 ? Visibility.Collapsed : Visibility.Visible;



        public int NugetUpdates => NugetUpdatesList.Count;

        public string NugetUpdateErrorMessage { get; set; }
        public ObservableCollection<MyNugetRef> NugetUpdatesList { get; } = new ObservableCollection<MyNugetRef>();

        public string GetAbsoluteProjFolder(EnumProjects projEnum)
        {
            string projFolder;
            switch (projEnum)
            {
                case EnumProjects.Website:
                    projFolder = WebsiteFolder;
                    break;
                case EnumProjects.Domain:
                    projFolder = Path.Combine(SolutionFolder, "Domain");
                    break;
                case EnumProjects.Model:
                    projFolder = Path.Combine(SolutionFolder, "M#", "Model");
                    break;
                case EnumProjects.UI:
                    projFolder = Path.Combine(SolutionFolder, "M#", "UI");
                    break;
                default:
                    projFolder = null;
                    break;
            }
            return projFolder;
        }

        public bool AddNugetUpdatesList(EnumProjects project, string packageName, string oldVersion, string newVersion)
        {
            lock (NugetUpdatesList)
            {
                var res = false;
                var nugetRef = NugetUpdatesList.SingleOrDefault(nu => nu.Include == packageName);
                if (nugetRef != null)
                    NugetUpdatesList.Remove(nugetRef);
                else
                    res = true;

                nugetRef = new MyNugetRef { Project = project, Include = packageName, Version = oldVersion, NewVersion = newVersion, IsLatestVersion = false };
                NugetUpdatesList.Add(nugetRef);

                OnPropertyChanged(nameof(NugetUpdates));
                return res;
            }
        }

        public void DelNugetPAckageFromUpdatesList(EnumProjects project, string packageName)
        {
            NugetUpdatesList.RemoveAll(x => x.Project == project && x.Include == packageName);
            OnPropertyChanged(nameof(NugetUpdates));
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
                }
                else
                    NugetStatusImage = "Pending";

                _nugetFetchTasks = value;
                OnPropertyChanged(nameof(NugetFetchTasks));
                OnPropertyChanged(nameof(NugetStatusImage));
                OnPropertyChanged(nameof(NugetUpdateIsInProgress));
            }
        }

        bool _gitUpdateIsInProgress;

        public bool GitUpdateIsInProgress
        {
            get => _gitUpdateIsInProgress;
            set
            {
                _gitUpdateIsInProgress = value;
                OnPropertyChanged(nameof(GitStatusImage));
            }
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

        public enum EnumProjects
        {
            Website,
            Domain,
            Model,
            UI
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
            
            Status = ProcId < 0 ? Status : EnumStatus.Run;

        }

        public void OpenVs(FileInfo solutionFile)
        {
            var dte2 = VsDTE ?? GetVSDTE(solutionFile);
            if (dte2 != null)
            {
                dte2.MainWindow.Visible = true;
                dte2.MainWindow.SetFocus();
            }
            else
                Process.Start(solutionFile.FullName);
        }

        public DTE2 GetVSDTE() => GetVSDTE(GetServiceSolutionFilePath());

        static DTE2 GetVSDTE(FileSystemInfo solutionFile)
        {
            if (solutionFile == null)
                return null;
            try
            {
                return Helper.GetVsInstances().FirstOrDefault(dte2 => String.Equals(dte2.Solution.FullName,
                    solutionFile.FullName, StringComparison.CurrentCultureIgnoreCase));
            }
            catch
            {

                return null;
            }

        }

        public FileInfo GetServiceSolutionFilePath()
        {
            return !Directory.Exists(WebsiteFolder) ? null : WebsiteFolder.AsDirectory().Parent?.GetFiles("*.sln").FirstOrDefault();
        }
    }

    public class ProjectRef
    {
        //public List<NugetRef> NugetRefs => new List<NugetRef>();
        public List<NugetRef> PackageReferences { get; set; }
    }

    public class NugetRef : ProjectItemGroupPackageReference
    {
        string _newVersion;

        public string NewVersion
        {
            get => _newVersion;
            set
            {
                if (value.IsEmpty() || !int.TryParse(value.Remove("."), out _))
                    return;

                _newVersion = value;
                if (_newVersion.HasValue() && Version.HasValue() &&
                    new Version(_newVersion).CompareTo(new Version(Version)) > 0)
                {
                    IsLatestVersion = true;
                }
            }
        }

        public bool IsLatestVersion { get; set; }
    }

    public class MyNugetRef : NugetRef
    {
        public MicroserviceItem.EnumProjects Project { get; set; }
        public bool Checked { get; set; }
    }
}
