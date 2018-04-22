using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MacroserviceExplorer
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window,INotifyPropertyChanged
    {
        public static readonly DependencyProperty TextLogProperty =
            DependencyProperty.Register("TextLog", typeof(string), typeof(Window) /*, new PropertyMetadata(false) */);

        public string TextLog
        {
            get => (string)GetValue(TextLogProperty);
            set
            {
                SetValue(TextLogProperty, value);
                OnPropertyChanged(nameof(TextLog));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public LogWindow()
        {
            InitializeComponent();
            TextLog = "Logger Started ...\n";
        }

        void LogWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        public void LogMessage(string message, string description = null)
        {
            TextLog += $"{DateTime.Now.ToLongTimeString()}  \t{message}{Environment.NewLine}";
            if (description.HasValue())
                TextLog += "decription : \t" + description?.Replace("\n","\n\t\t") + Environment.NewLine;

            TextLog += $"{new string('-', 30)}{Environment.NewLine}";
        }

        public void SetYourPosBy(MainWindow mainWindow)
        {
            const int border = 7;
            Top = mainWindow.Top;
            Height = mainWindow.Height + border;
            Left = mainWindow.Left + mainWindow.Width - border;

        }
    }

    public class MyContext
    {
        public string TextLog { get; set; }
    }
}
