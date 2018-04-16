using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using MacroserviceExplorer.Annotations;
using Process = System.Diagnostics.Process;

namespace MacroserviceExplorer
{
    public class MacroserviceGridItem : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementations

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Overrides of ToString Object

        public override string ToString()
        {
            return $"\'{Service}\' port : {Port} Status : {Status}";
        }

        #endregion

        public enum enumStatus
        {
            NoSourcerLocally = 1,
            Stop = 2,
            Run = 3,
            Pending = 4
        }
        enumStatus _status;
        public enumStatus Status
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

        public FontWeight ServiceFontWeight => Status == enumStatus.Run ? FontWeights.Bold : FontWeights.Regular;

        public System.Windows.Media.Brush ServiceColor
        {
            get
            {
                switch (Status)
                {
                    case enumStatus.NoSourcerLocally:
                        return System.Windows.Media.Brushes.DimGray;
                    case enumStatus.Stop:
                        return System.Windows.Media.Brushes.DarkRed;
                    case enumStatus.Run:
                        return System.Windows.Media.Brushes.Green;
                    default:
                        return System.Windows.Media.Brushes.Black;
                }
            }
        }

        public string ServiceTooltip
        {
            get
            {
                switch (Status)
                {
                    case enumStatus.NoSourcerLocally:
                        return "Source not available locally";
                    case enumStatus.Stop:
                        return "Service Stopped locally";
                    case enumStatus.Run:
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
                    case enumStatus.Stop:
                        return "Resources/Run2.png";
                    case enumStatus.Run:
                        return "Resources/Pause2.jpg";
                    case enumStatus.Pending:
                        return "Resources/gears.gif";
                    default:
                        return null;
                }

            }
        }

        public double RunImageOpacity => Status == enumStatus.Stop ? 1 : .2;

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

        public object PortIcon => int.TryParse(Port, out var _) ? null : "Resources/Warning.png";

        public string PortTooltip => PortIcon != null ? $"launchsettings.json File Not Found in this location :\n{WebsiteFolder}\\Properties\\launchSettings.json" : null;

        public Visibility VisibleCode => string.IsNullOrEmpty(PortTooltip) ? Visibility.Visible : Visibility.Hidden;

        public object VsCodeIcon => VsDTE == null ? "Resources/VS.png" : "Resources/VS2.png";

        public object Tag { get; set; }

        DTE2 _vsDTE;
        private int _nugetUpdates;
        private string _gitUpdates;
        private object _gitUpdateImage;

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

        public int NugetUpdates
        {
            get => _nugetUpdates;
            set
            {
                _nugetUpdates = value;
                OnPropertyChanged(nameof(NugetUpdates));
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

        public object GitUpdateImage => GitUpdates.HasValue()  ? "Resources/git.png" : null;
    }
}
