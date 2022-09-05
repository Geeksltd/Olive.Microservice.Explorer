using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using EnvDTE;
using MicroserviceExplorer.MicroserviceGenerator;
using MicroserviceExplorer.UI;
using MicroserviceExplorer.Utils;
using Newtonsoft.Json.Linq;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.Forms.MessageBox;
using Process = System.Diagnostics.Process;

namespace MicroserviceExplorer
{
	public partial class MainWindow : IDisposable
	{
		internal static DirectoryInfo ServicesDirectory { get; set; }

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
		public static readonly RoutedCommand NewMicroserviceCommand = new RoutedUICommand("NewMicroservice", "NewMicroserviceCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[]
		{
			new KeyGesture(Key.M, ModifierKeys.Control | ModifierKeys.Shift)
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
		readonly DispatcherTimer AutoRefreshTimer = new DispatcherTimer();
		readonly DispatcherTimer AutoRefreshProcessTimer = new DispatcherTimer();

		public MainWindow()
		{
			InitNotifyIcon();

			InitializeComponent();
			DataContext = MicroserviceGridItems;

			AutoRefreshTimer.Tick += OnAutoRefreshTimerOnTick;
			AutoRefreshTimer.Interval = new TimeSpan(0, 3, 0);

			AutoRefreshProcessTimer.Tick += OnAutoRefreshProcessTimerOnTick;
			AutoRefreshProcessTimer.Interval = new TimeSpan(0, 0, 2);
		}

		void RestartAutoRefreshProcess()
		{
			highPriority = null;
			AutoRefreshProcessTimer.Stop();
			OnAutoRefreshProcessTimerOnTick(null, null);
			AutoRefreshProcessTimer.Start();
		}

		bool AutoRefreshProcessTimerInProgress;
		void OnAutoRefreshProcessTimerOnTick(object sender, EventArgs args)
		{
			//return;

			if (AutoRefreshProcessTimerInProgress) return;

			AutoRefreshProcessTimer.IsEnabled = false;
			foreach (var service in MicroserviceGridItems)
			{
				if (service.WebsiteFolder.IsEmpty() || service.Port.IsEmpty()) continue;
				using (var backgroundWorker = new BackgroundWorker())
				{
					backgroundWorker.DoWork += async (sender1, e) =>
					{
						service.UpdateProcessStatus();
						service.VsDTE = await service.GetVSDTE();
					};

					backgroundWorker.RunWorkerAsync();
				}
			}

			AutoRefreshProcessTimer.IsEnabled = true;
		}

		void RestartAutoRefresh()
		{
			AutoRefreshTimer.Stop();
			OnAutoRefreshTimerOnTick(null, null);
			AutoRefreshTimer.Start();
		}

		bool AutoRefreshTimerInProgress;
		bool firstRun = true;
		async void OnAutoRefreshTimerOnTick(object sender, EventArgs args)
		{
			var projects = MicroserviceGridItems.Where(x => x.WebsiteFolder.HasValue()).ToArray();
			if (highPriority != null)
			{
				await Task.Run(() => FetchUpdates(highPriority)).ContinueWith(async (t) =>
				{
					Parallel.ForEach(projects.Except(highPriority), p =>
					{
						Task.Run(() => FetchUpdates(p)).Await();
					});
					//foreach (var p in projects.Except(highPriority))
					//	await Task.Run(() => FetchUpdates(p));
				});

				await Task.Run(() => CalculateGitUpdates(highPriority)).ContinueWith(async (t) =>
				{
					Parallel.ForEach(projects.Except(highPriority), p =>
					{
						Task.Run(() => CalculateGitUpdates(p)).Await();
					});
					//foreach (var p in projects.Except(highPriority))
					//	Task.Run(() => CalculateGitUpdates(p)).Await();
				});

				await Task.Run(() => LocalGitChanges(highPriority)).ContinueWith(async (t) =>
				{
					Parallel.ForEach(projects.Except(highPriority), p =>
					{
						Task.Run(() => LocalGitChanges(p)).Await();
					});
					//foreach (var p in projects.Except(highPriority))
					//	await Task.Run(() => LocalGitChanges(p));
				});

			}
			else
			{
				if (AutoRefreshTimerInProgress) return;
				else AutoRefreshTimerInProgress = true;

				int waitTime = 0;
				if (firstRun)
				{
					waitTime = 15;
					firstRun = false;
				}

				await Task.Factory.StartNew(() => System.Threading.Thread.Sleep(waitTime * 1000))
					   .ContinueWith(async (t) =>
					   {
						   Parallel.ForEach(projects.Except(highPriority), p =>
						   {
							   Task.Run(() => FetchUpdates(p)).Await();
						   });
						   //foreach (var p in projects)
						   // await Task.Run(async () =>
						   // {
						   //  if (highPriority == null) await FetchUpdates(p);
						   // });
					   });
				await Task.Factory.StartNew(() => System.Threading.Thread.Sleep(waitTime * 1000))
					   .ContinueWith(async (t) =>
					   {
						   Parallel.ForEach(projects.Except(highPriority), p =>
						   {
							   Task.Run(() => CalculateGitUpdates(p)).Await();
						   });
						   //foreach (var p in projects)
						   // await Task.Run(async () =>
						   // {
						   //  if (highPriority == null) await CalculateGitUpdates(p);
						   // });
					   });

				await Task.Factory.StartNew(() => System.Threading.Thread.Sleep(waitTime * 1000))
					   .ContinueWith(async (t) =>
					   {
						   Parallel.ForEach(projects.Except(highPriority), p =>
						   {
							   Task.Run(() => LocalGitChanges(p)).Await();
						   });
						   //foreach (var p in projects)
						   // await Task.Run(async () =>
						   // {
						   //  if (highPriority == null) await LocalGitChanges(p);
						   // });
					   });

			}

			AutoRefreshTimerInProgress = false;
		}

		static IEnumerable<SolutionProject> SolutionProjects
			=> Enum.GetValues(typeof(SolutionProject)).OfType<SolutionProject>();

		Task FetchUpdates(MicroserviceItem service)
		{
			var refresh = Task.Factory.StartNew(service.RefreshPackages, TaskCreationOptions.LongRunning);
			return refresh;
		}

		void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			if (!File.Exists(RecentsXml))
			{
				OpenProject_Executed(sender, null);
				if (!projectLoaded) ExitMenuItem_Click(sender, e);
				return;
			}

			ReloadRecentFiles();
			// logWindow.ShowInTaskbar = false;
			// logWindow.Hide();
			var recentFilesCount = _recentFiles.Count - 1;

			while (recentFilesCount > 0)
			{
				if (LoadFile(_recentFiles[recentFilesCount]))
					break;
				recentFilesCount--;
			}

			if (projectLoaded) return;

			File.Delete(RecentsXml);
			MainWindow_OnLoaded(sender, e);
		}

		void StartStop_OnClick(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			var service = GetServiceByTag(sender);
			switch (service.Status)
			{
				case MicroserviceItem.EnumStatus.Pending:
					MessageBox.Show("Microservice is loading.\nPlease Wait ...", @"Loading ...");
					break;
				case MicroserviceItem.EnumStatus.Run:
					service.Stop();
					break;
				case MicroserviceItem.EnumStatus.Stop:
					service.Start();
					break;
				case MicroserviceItem.EnumStatus.NoSourcerLocally:
					break;
				default:
					throw new ArgumentOutOfRangeException($@"Service.Status out of range");
			}
		}

		// void microserviceRunCheckingTimer_Tick(object sender, EventArgs e)
		// {
		//    var timer = (DispatcherTimer)sender;
		//    var service = (MicroserviceItem)timer.Tag;
		//    service.UpdateProcessStatus();
		//    if (service.ProcId < 0) return;

		//    timer.Stop();
		//    timer.Tick -= microserviceRunCheckingTimer_Tick;
		//    service.Status = MicroserviceItem.EnumStatus.Run;
		//    AutoRefreshTimer.Start();
		// }

		void OpenCode_OnClick(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			var service = GetServiceByTag(sender);
			var solutionFile = service.GetServiceSolutionFilePath();
			service.OpenVs(solutionFile);
		}

		void OpenProject_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			using (var openFileDialog = new FolderBrowserDialog
			{
				ShowNewFolderButton = false,
				Description = "Select your Hub folder"
			})
			{
				if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

				if (_recentFiles.None() || !_recentFiles.Contains(openFileDialog.SelectedPath))
				{
					if (_recentFiles.None())
						mnuRecentFiles.Items.Clear();

					_recentFiles.Add(openFileDialog.SelectedPath);
					AddRecentMenuItem(openFileDialog.SelectedPath);
					SaveRecentFilesXml();
				}

				LoadFile(openFileDialog.SelectedPath);
			}
		}

		void CloseMenuItem_OnClick(object sender, RoutedEventArgs e) => Close();

		void RefreshMenuItem_OnClick(object sender, ExecutedRoutedEventArgs e) => Refresh();

		void OpenExplorer_OnClick(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			var service = GetServiceByTag(sender);
			Process.Start(service.WebsiteFolder.AsDirectory().Parent?.FullName ??
						  throw new Exception("Microservice projFolder Not Exists ..."));
		}

		void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			FilterListBy(txtSearch.Text);
		}

		async void VsDebuggerAttach_OnClick(object sender, MouseButtonEventArgs e)
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

			await service.OpenVs(solutionFile: service.GetServiceSolutionFilePath());
			process.Attach();
		}

		void RunAllMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
		{
			foreach (var service in MicroserviceGridItems)
				if (service.Status == MicroserviceItem.EnumStatus.Stop)
					service.Start();
		}

		void StopAllMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
		{
			foreach (var service in MicroserviceGridItems)
				if (service.Status == MicroserviceItem.EnumStatus.Run)
					service.Stop();
		}

		void RunAllFilteredMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
		{
			foreach (var service in MicroserviceGridItems)
				if (service.Status == MicroserviceItem.EnumStatus.Stop)
					service.Start();
		}

		void StopAllFilteredMenuItem_Click(object sender, ExecutedRoutedEventArgs e)
		{
			foreach (var service in MicroserviceGridItems)
			{
				if (service.Status == MicroserviceItem.EnumStatus.Run)
					service.Stop();
			}
		}

		void WindowTitlebarControl_OnRefreshClicked(object sender, EventArgs e) => Refresh();

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
			// if (logWindow.IsVisible)
			//    logWindow.SetTheLogWindowBy(this);
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
				MessageBox.Show("Last Kestrel process was attached to none console window style.\n So if you want to see Kestrel log window, please stop and start Microserice again.", "There is not kestrel window-habdle");
			}
		}

		public void Dispose()
		{
			notifyIcon.Dispose();
			watcher.Dispose();
		}

		void ShowServiceLog_OnClick(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			var service = GetServiceByTag(sender);
			service.ShowLogWindow();
		}

		void UpdateAllNuget_Click(object sender, RoutedEventArgs e)
		{
			ServiceData.SelectMany(x => x.References).Do(x => x.ShouldUpdate = true);
			ServiceData.ForEach(x => x.UpdateSelectedPackages());
		}

		bool LoadFile(string hub)
		{
			ServicesDirectory = hub.AsDirectory().Parent;

			var serviceInfos = ServicesDirectory?.GetDirectories()
				.Where(x => File.Exists(x.FullName + @"\Website\appsettings.json"))
				.Select(x =>
						new ServiceInfo
						{
							Name = x.Name + GetServiceName(x.FullName + @"\Website\appSettings.json").Unless(x.Name).WithWrappers(" (", ")"),
							ProjectFolder = x.FullName,
							WebsiteFolder = Path.Combine(x.FullName, "Website"),
							LaunchSettingsPath = Path.Combine(x.FullName, @"Website\Properties", "launchSettings.json")
						});

			if (ServicesDirectory == null ||
				!CheckIfServicesDirectoryExist()) return false;

			servicesDirectoryLastWriteTime = ServicesDirectory.LastWriteTime;

			txtFileInfo.Text = ServicesDirectory.FullName;
			txtSolName.Text = ServicesDirectory.Name;
			if (serviceInfos == null) return false;

			foreach (var serviceInfo in serviceInfos)
			{
				var serviceName = serviceInfo.Name;
				var srv = ServiceData.SingleOrDefault(srvc => srvc.Service == serviceName);
				if (srv == null)
				{
					srv = new MicroserviceItem
					{
						MainWindow = this
					};
					ServiceData.Add(srv);
				}

				var port = "";
				var status = MicroserviceItem.EnumStatus.NoSourcerLocally;
				var parentFullName = ServicesDirectory?.FullName ?? "";
				var projFolder = serviceInfo.ProjectFolder;
				var websiteFolder = serviceInfo.WebsiteFolder;
				var launchSettings = serviceInfo.LaunchSettingsPath;
				var procId = -1;

				if (File.Exists(@launchSettings))
				{
					status = MicroserviceItem.EnumStatus.Pending;
					port = GetPortNumberFromLaunchSettingsFile(launchSettings);
				}
				else
					websiteFolder = null;

				srv.Status = status;
				srv.Service = serviceName;
				srv.Port = port;
				srv.ProcId = procId;
				srv.SolutionFolder = projFolder;
				srv.WebsiteFolder = websiteFolder;
			}

			FilterListBy(txtSearch.Text);

			projectLoaded = true;

			if (watcher == null)
				StartFileSystemWatcher(ServicesDirectory);

			Refresh();

			return true;
		}

		bool CheckIfServicesDirectoryExist()
		{
			if (!ServicesDirectory.Exists())
			{
				var result = MessageBox.Show(
					$@"file : {
							ServicesDirectory.FullName
						} \ndoes not exist anymore. \nDo you want to removed it from recent directories list?", @"Directory Not Found",
					System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
				_recentFiles.Remove(ServicesDirectory.FullName);
				if (result != System.Windows.Forms.DialogResult.Yes) return false;

				SaveRecentFilesXml();
				ReloadRecentFiles();

				ServicesDirectory = null;
				projectLoaded = false;
				return false;
			}

			return true;
		}

		void Refresh()
		{
			AutoRefreshProcessTimer.Stop();
			AutoRefreshTimer.Stop();

			if (ServicesDirectory != null)
				Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(() =>
				{
					try
					{
						RefreshFile(ServicesDirectory.FullName);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						throw;
					}
				})
				);
		}

		async void NewMicroservice_Click(object sender, ExecutedRoutedEventArgs e)
		{
			if (ServicesDirectory == null || !ServicesDirectory.Exists) return;

			var msw = new NewMicroservice.NewMicroservice
			{
				WindowStartupLocation = WindowStartupLocation.CenterScreen
			};

			var dialog = msw.ShowDialog();
			if (dialog != true) return;

			await new NewMicroserviceCreator(msw.ServiceName, msw.GitRepoUrl).Create();

			Refresh();
		}
	}
}

namespace System
{
	public static class TempExtDeleteMeAfterNugetUpdate
	{
		public static string GetFisrtFile(this string @this, string basePath)
		{
			if (!@this.Contains("*")) @this = "*" + @this;

			var path = basePath.AsDirectory();
			if (!path.Exists) return null;

			var file = path.GetFiles(@this).FirstOrDefault();
			return file?.FullName;
		}
	}
}