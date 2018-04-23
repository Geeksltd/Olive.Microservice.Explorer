using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;

namespace MacroserviceExplorer
{
    partial class MainWindow
    {
        readonly LogWindow logWindow;

        void OpenLogWindowMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ShowLogWindow();
        }

        void ShowLogWindow()
        {
            logWindow.Show();
            logWindow.SetTheLogWindowBy(this);
            logWindow.Focus();
        }

    }
}
