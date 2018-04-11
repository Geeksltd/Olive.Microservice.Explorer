using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using MacroserviceExplorer.Annotations;

namespace MacroserviceExplorer
{
    public class MacroserviceGridItem : INotifyPropertyChanged
    {
        int _status;

        public int Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusImage));
                OnPropertyChanged(nameof(RunImage));
                OnPropertyChanged(nameof(RunImageOpacity));
                OnPropertyChanged(nameof(ServiceColor));
                OnPropertyChanged(nameof(ServiceFontWeight));
                OnPropertyChanged(nameof(ServiceTooltip));
            }
        }

        public string StatusImage
        {
            get
            {
                switch (Status)
                {
                    case 1:
                        return "Resources/Gray.png";
                    case 2:
                        return "Resources/Red.png";
                    case 3:
                        return "Resources/Green.png";
                    case 4:
                        return "Resources/loading.gif";
                    default:
                        return "";
                }
            }
        }

        public string Service { get; set; }

        public FontWeight ServiceFontWeight => Status == 3 ? FontWeights.Bold : FontWeights.Regular;

        public System.Windows.Media.Brush ServiceColor
        {
            get
            {
                switch (Status)
                {
                    case 1:
                        return System.Windows.Media.Brushes.DimGray;
                    case 2:
                        return System.Windows.Media.Brushes.DarkRed;
                    case 3:
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
                    case 1:
                        return "Source not available locally";
                    case 2:
                        return "Service Stopped locally";
                    case 3:
                        return "Service is Running locally";
                    default:
                        return "";
                }
            }
        }

        public string Port { get; set; }

        public string LiveUrl { get; set; }
        public string UatUrl { get; set; }

        public string RunImage
        {
            get
            {
                switch (Status)
                {
                    case 2:
                        return "Resources/Run2.png";
                    case 3:
                        return "Resources/Pause2.jpg";
                    case 4:
                        return "Resources/gears.gif";
                    default:
                        return "";
                }

            }
        }

        public double RunImageOpacity => Status == 3 ? 1 : .2;

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
                //OnPropertyChanged(nameof(Tooltip));
            }
        }

        public string ProcessName { get; private set; }

        public string WebsiteFolder { get; set; }

        public string PortIcon => int.TryParse(Port, out var _) ? null : "Resources/Warning.png";
        public string PortTooltip => !string.IsNullOrEmpty(PortIcon) ? $"launchsettings.json File Not Found in this location :\n{WebsiteFolder}\\Properties\\launchSettings.json" : null;

        public Visibility VisibleCode => string.IsNullOrEmpty(PortTooltip) ? Visibility.Visible : Visibility.Hidden;

        public object Tag { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
