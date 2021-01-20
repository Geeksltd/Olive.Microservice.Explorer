using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace MicroserviceExplorer
{
    partial class MainWindow
    {
        System.Windows.Forms.NotifyIcon notifyIcon;
        bool exit;

        void ExitMenuItem_Click(object sender, EventArgs e)
        {
            exit = true;
            Close();
            Application.Current.Shutdown(0);
        }

        void TrayOpenWindow(object sender, EventArgs eventArgs) => Visibility = Visibility.Visible;

        void InitNotifyIcon()
        {
            var components = new Container();

            notifyIcon = new System.Windows.Forms.NotifyIcon(components)
            {
                ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(),
                Icon = Properties.Resources.Olive,
                Text = @"Olive Microservice Explorer",
                Visible = true,
            };
            notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripMenuItem("Open Explorer Window", null, TrayOpenWindow));

            notifyIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripMenuItem("Exit", null, ExitMenuItem_Click));

            // notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            notifyIcon.Click += TrayOpenWindow;
            // notifyIcon.MouseUp += notifyIcon_MouseUp;
        }

        void StatusProgressStart()
        {
            Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(() =>
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

        // void ShowStatusMessage(string message, string tooltip = null, MicroserviceItem service = null)
        // {
        //    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(() =>
        //    {
        //        txtStatusMessage.Text = message;
        //        txtStatusMessage.ToolTip = tooltip;

        //        service?.LogMessage(message, tooltip);
        //    }));

        // }

        void MainWindow_OnClosed(object sender, EventArgs e)
        {
            StopWatcher();

            notifyIcon?.Dispose();
        }

        void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Visibility = Visibility.Hidden;
            e.Cancel = !exit;
        }
    }
}