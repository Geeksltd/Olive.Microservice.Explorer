using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace MacroserviceExplorer
{
    partial class MainWindow
    {
        System.Windows.Forms.NotifyIcon notifyIcon;
        bool exit;

        void ExitMenuItem_Click(object sender, EventArgs e)
        {
            exit = true;
            logWindow.Close();
            Close();
            Application.Current.Shutdown(0);
        }

        void TrayOpenWindow(object sender, EventArgs eventArgs)
        {
            Visibility = Visibility.Visible;
        }

        void InitNotifyIcon()
        {
            using (var components = new Container())
            {
                notifyIcon = new System.Windows.Forms.NotifyIcon(components)
                {
                    ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(),
                    Icon = Properties.Resources.Olive,
                    Text = @"Olive Macroservice Explorer",
                    Visible = true,
                };
                notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripMenuItem("Open Explorer Window", null, TrayOpenWindow));

                notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripMenuItem("Exit", null, ExitMenuItem_Click));

                //notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
                notifyIcon.Click += TrayOpenWindow;
                //notifyIcon.MouseUp += notifyIcon_MouseUp;
            }

        }

        void StatusProgressStart()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(() =>
            {
                statusProgress.IsIndeterminate = true;
            }));
        }

        void StatusProgressStop()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(() =>
            {
                statusProgress.IsIndeterminate = false;
            }));

        }

        void ShowStatusMessage(string message, string tooltip = null, bool logMessage = true)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(() =>
            {
                txtStatusMessage.Text = message;
                txtStatusMessage.ToolTip = tooltip;
                if (logMessage)
                    logWindow.LogMessage(message, tooltip);
            }));

        }

        void MainWindow_OnClosed(object sender, EventArgs e)
        {
            StopWatcher();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Visibility = Visibility.Hidden;
            logWindow.Visibility = Visibility;
            e.Cancel = !exit;
        }

    }
}
