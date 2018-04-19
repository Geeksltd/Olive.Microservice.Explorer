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
    public partial class LogWindow : Window
    {
        public MyContext Context { get; set; }
        
        public LogWindow()
        {
            Context = new MyContext { TextLog = "Olive Macroservice Explorer logger :"};
            InitializeComponent();
            DataContext = Context;
        }

        void LogWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Hide();
        }

        public void LogMessage(string logtext)
        {
            Context.TextLog += Environment.NewLine + logtext;
        }
    }

    public class MyContext
    {
        public string TextLog { get; set; }
    }
}
