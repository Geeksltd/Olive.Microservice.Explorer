using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Threading;

namespace MicroserviceExplorer
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window,INotifyPropertyChanged
    {
        public static readonly DependencyProperty TextLogProperty =
            DependencyProperty.Register("TextLog", typeof(string), typeof(Window) /*, new PropertyMetadata(false) */);

        readonly int stringLineLength = 30;

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

        public string TextLog
        {
            get => (string)GetValue(TextLogProperty);
            set
            {
                var offset = txtLog.VerticalOffset;
                SetValue(TextLogProperty, value);
                OnPropertyChanged(nameof(TextLog));
                if (IsFocused || Visibility != Visibility.Visible) return;

                txtLog.ScrollToVerticalOffset(offset);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void LogMessage(string message, string description = null)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MainWindow.MyDelegate(() =>
            {
                TextLog += $"{LocalTime.Now.ToLongTimeString()}  \t{message}{Environment.NewLine}";
                if (description.HasValue())
                    TextLog += "decription : \t" + description?.Replace("\n", "\n\t\t") + Environment.NewLine;

                TextLog += $"{new string('-', stringLineLength)}{Environment.NewLine}";

            }));

        }

        public void SetTheLogWindowBy(MainWindow mainWindow)
        {
            const int border = 7;
            Top = mainWindow.Top;
            Height = mainWindow.Height + border;
            Left = mainWindow.Left + mainWindow.Width - border;

        }

        void ClearLogMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            TextLog = null;
        }


        void SaveLogMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("Log.txt",TextLog);
        }
    }

    public class MyContext
    {
        public string TextLog { get; set; }
    }
}
