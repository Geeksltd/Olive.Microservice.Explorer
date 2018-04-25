using System.Windows;

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
