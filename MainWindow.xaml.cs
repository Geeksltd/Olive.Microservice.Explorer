﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using EnvDTE;
using MicroserviceExplorer.Utils;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.Forms.MessageBox;
using Process = System.Diagnostics.Process;


namespace MicroserviceExplorer
{

    public partial class MainWindow : IDisposable
    {

        #region Commands

        public static readonly RoutedCommand EditCommand = new RoutedUICommand("Edit", "EditCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.E, ModifierKeys.Control)
        }));

        public static readonly RoutedCommand RefreshCommand = new RoutedUICommand("Refresh", "RefreshCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.R, ModifierKeys.Control)
        }));

        public static readonly RoutedCommand CloseCommand = new RoutedUICommand("Close", "CloseCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.Q, ModifierKeys.Control)
        }));

        public static readonly RoutedCommand ExitCommand = new RoutedUICommand("Exit", "ExitCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.Q, ModifierKeys.Control | ModifierKeys.Shift)
        }));

        public static readonly RoutedCommand RunAllCommand = new RoutedUICommand("RunAll", "RunAllCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.L, ModifierKeys.Control )
        }));

        public static readonly RoutedCommand StopAllCommand = new RoutedUICommand("StopAll", "StopAllCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Shift)
        }));

        public static readonly RoutedCommand RunAllFilteredCommand = new RoutedUICommand("RunAllFiltered", "RunAllFilteredCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Alt)
        }));

        public static readonly RoutedCommand StopAllFilteredCommand = new RoutedUICommand("StopAllFiltered", "StopAllFilteredCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift)
        }));

        public static readonly RoutedCommand AlwaysOnTopCommand = new RoutedUICommand("AlwaysOnTop", "AlwaysOnTopCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
        {
            new KeyGesture(Key.T, ModifierKeys.Control)
        }));

        #endregion

        public MainWindow()
        {
            InitNotifyIcon();

            InitializeComponent();

            logWindow = new LogWindow();

            this.Focus();
            DataContext = MacroserviceGridItems;
            StartAutoRefresh();
        }

        readonly DispatcherTimer AutoRefreshTimer = new DispatcherTimer();
        void StartAutoRefresh()
        {
            AutoRefreshTimer.Tick += async (sender, args) =>
            {

                logWindow.LogMessage("[Autorefresh Begin]");
                var gridHasFocused = srvGrid.IsFocused;

                MicroserviceGridItem selItem = null;
                if (SelectedService != null)
                    selItem = SelectedService;

                await Refresh();

                foreach (var service in MacroserviceGridItems)
                {
                    if (service.WebsiteFolder.IsEmpty() || service.Port.IsEmpty()) continue;

                    service.UpdateProcessStatus();
                }
                srvGrid.SelectedItem = selItem;

                if (gridHasFocused)
                    srvGrid.Focus();

                logWindow.LogMessage("[Autorefresh End]");

            };
            AutoRefreshTimer.Interval = new TimeSpan(0, 3, 0 );
            AutoRefreshTimer.Start();
        }


        async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(RecentsXml))
            {
                OpenProject_Executed(sender, null);
                if (!ProjextLoaded)
                    ExitMenuItem_Click(sender, e);
                return;
            }

            ReloadRecentFiles();
            //logWindow.ShowInTaskbar = false;
            //logWindow.Hide();
            while (_recentFiles.Any())
            {
                if (await LoadFile(_recentFiles[_recentFiles.Count - 1]))
                    break;
            }

            if (ProjextLoaded) return;

            File.Delete(RecentsXml);
            MainWindow_OnLoaded(sender, e);

        }
        

        void StartStop_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            switch (service.Status)
            {
                case MicroserviceGridItem.EnumStatus.Pending:
                    MessageBox.Show("Macroservice is loading.\nPlease Wait ...", @"Loading ...");
                    break;
                case MicroserviceGridItem.EnumStatus.Run:
                    service.Stop();
                    break;
                case MicroserviceGridItem.EnumStatus.Stop:
                    Start(service);
                    break;
                case MicroserviceGridItem.EnumStatus.NoSourcerLocally:
                    break;
                default:
                    throw new ArgumentOutOfRangeException($@"Service.Status out of range");
            }
        }


        void Start(MicroserviceGridItem service)
        {
            AutoRefreshTimer.Stop();
            service.Status = MicroserviceGridItem.EnumStatus.Pending;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run --no-build --project " + service.WebsiteFolder,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Minimized
                    //RedirectStandardOutput = true
                }
            };

            proc.Start();


            var dispatcherTimer = new DispatcherTimer { Tag = service };
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            var timer = (DispatcherTimer)sender;
            var service = (MicroserviceGridItem)timer.Tag;
            service.UpdateProcessStatus();
            if (service.ProcId < 0) return;

            timer.Stop();
            timer.Tick -= DispatcherTimer_Tick;
            service.Status = MicroserviceGridItem.EnumStatus.Run;
            AutoRefreshTimer.Start();
        }


        void OpenCode_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            var solutionFile = service.GetServiceSolutionFilePath();
            service.OpenVs(solutionFile);
        }

        async void OpenProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = $@"Services JSON File |{Services_file_name}",
                RestoreDirectory = true,
                Multiselect = false,
                SupportMultiDottedExtensions = true,
                Title = $@"Select {Services_file_name} file"
            })
            {
                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                if (_recentFiles.None() || !_recentFiles.Contains(openFileDialog.FileName))
                {
                    if (_recentFiles.None())
                        mnuRecentFiles.Items.Clear();

                    _recentFiles.Add(openFileDialog.FileName);
                    AddRecentMenuItem(openFileDialog.FileName);

                    SaveRecentFilesXml();
                }

                await LoadFile(openFileDialog.FileName);
            }
        }

        async Task Refresh()
        {

            if (ServicesJsonFile != null)
            {
                await RefreshFile(ServicesJsonFile.FullName);
            }

            if (!ProjextLoaded)
                AutoRefreshTimer.Stop();
        }

        void EditProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ServicesJsonFile != null)
                Process.Start("Notepad.exe", ServicesJsonFile.FullName);
        }

        void CloseMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void RefreshMenuItem_OnClick(object sender, ExecutedRoutedEventArgs e)
        {
            var refresh = Refresh();
        }

        void OpenExplorer_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            Process.Start(service.WebsiteFolder.AsDirectory().Parent?.FullName ?? throw new Exception("Macroservice projFolder Not Exists ..."));

        }
        void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterListBy(txtSearch.Text);

        }

        void VsDebuggerAttach_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);

            if (service.VsDTE.Mode == vsIDEMode.vsIDEModeDebug)
            {
                service.VsDTE.Debugger.DetachAll();
                return;
            }

            var processes = service.VsDTE.Debugger.LocalProcesses.OfType<EnvDTE.Process>();
            var process = processes.SingleOrDefault(x => x.ProcessID == service.ProcId);
            if (process == null) return;

            service.OpenVs(service.GetServiceSolutionFilePath());
            process.Attach();



        }

        void RunAllMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var service in MacroserviceGridItems)
                if (service.Status == MicroserviceGridItem.EnumStatus.Stop)
                    Start(service);
        }

        void StopAllMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var service in MacroserviceGridItems)
            {
                if (service.Status == MicroserviceGridItem.EnumStatus.Run)
                    service.Stop();
            }
        }

        void RunAllFilteredMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var service in MacroserviceGridItems)
                if (service.Status == MicroserviceGridItem.EnumStatus.Stop)
                    Start(service);

        }

        void StopAllFilteredMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var service in MacroserviceGridItems)
            {
                if (service.Status == MicroserviceGridItem.EnumStatus.Run)
                    service.Stop();
            }
        }

         void WindowTitlebarControl_OnRefreshClicked(object sender, EventArgs e)
         {
             var refresh = Refresh();
         }

        void MnuAlwaysOnTop_OnChecked(object sender, RoutedEventArgs e)
        {
            Topmost = mnuAlwaysOnTop.IsChecked;
        }

        void WindowOpacityMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            var template = SliderMenuItem.Template;
            var slider = (Slider)template.FindName("OpacitySlider", SliderMenuItem);
            slider.Value = menuItem.Header.ToString().TrimEnd('%').To<int>();
        }

        void OpacitySlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Opacity = e.NewValue / 100d;
            foreach (MenuItem item in OpacityMenuItem.Items)
            {
                if (item.Header == null || !item.Header.ToString().EndsWith("%"))
                    continue;

                item.IsChecked = Math.Abs(item.Header.ToString().TrimEnd('%').To<int>() - e.NewValue) < .0001;
            }
        }

        async void GitUpdate_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            await GitUpdate(service);
        }

        void MainWindow_OnLocationChanged(object sender, EventArgs e)
        {
            if (logWindow.IsVisible)
                logWindow.SetTheLogWindowBy(this);
        }

        void ShowKestrelLog_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            var pid = service.ProcId;
            IntPtr mainWindowHandle;
            do
            {
                mainWindowHandle = Process.GetProcessById(pid).MainWindowHandle;
                var pr = ParentProcessUtilities.GetParentProcess(pid);
                pid = pr?.Id ?? 0;
            } while (mainWindowHandle == IntPtr.Zero && pid != 0);

            if (mainWindowHandle != IntPtr.Zero)
                WindowApi.ShowWindow(mainWindowHandle);
            else
            {
                MessageBox.Show("Last Kestrel process was attached to none console window style.\n So if you want to see Kestrel log window, please stop and start macroserice again.", "There is not kestrel window-habdle");
            }
        }

        public void Dispose()
        {
            notifyIcon.Dispose();
            Watcher.Dispose();
        }
    }
}

namespace System
{
    public static class TempExtDeleteMeAfterNugetUpdate
    {
        public static string GetFisrtFile(this string @this, string basePath)
        {
            if (!@this.Contains("*"))
                @this = "*" + @this;

            var path = basePath.AsDirectory();
            if (!path.Exists) return null;

            var file = path.GetFiles(@this).FirstOrDefault();
            return file?.FullName;
        }
    }

}