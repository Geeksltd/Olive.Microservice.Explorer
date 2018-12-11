using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
namespace MicroserviceExplorer
{
    public partial class LocalGitWindow : Window
    {
        readonly string _serviceAddress;
        readonly string _hubAddress;
        readonly string _serviceName;

        public LocalGitWindow(string serviceAddress, string hubAddress, string serviceName)
        {
            InitializeComponent();
            _serviceAddress = serviceAddress;
            _hubAddress = hubAddress;
            _serviceName = serviceName;
            BindUnCommitedChanges();
            BindUnPushedChanges();
        }
        void btnCommit_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                btnCommit.Content = "Committing...";
                btnCommit.IsEnabled = false;

                using (var backgroundWorker = new BackgroundWorker())
                {
                    backgroundWorker.RunWorkerCompleted += CommitBackgroundWorker_RunWorkerCompleted;

                    backgroundWorker.DoWork += (sender1, e1) =>
                    {
                        var stageCommandResult = RunGitCommand("add .");

                        if (string.IsNullOrEmpty(stageCommandResult) || stageCommandResult.StartsWith("warning: LF"))
                        {
                            RunGitCommand("commit -m 'commited-from-olive-microservice-explorer.'");

                        }
                        else
                            MessageBox.Show(stageCommandResult, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    };

                    backgroundWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                var pattern = @"no changes added to commit";
                const RegexOptions OPTIONS = RegexOptions.Multiline | RegexOptions.IgnoreCase;
                var commitNotChangeMatch = Regex.Match(ex.Message, pattern, OPTIONS);
                if (commitNotChangeMatch.Success)
                    MessageBox.Show("There is no any changes to commit.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        void btnPush_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                btnPush.Content = "Pushing...";
                btnPush.IsEnabled = false;

                using (var backgroundWorker = new BackgroundWorker())
                {
                    backgroundWorker.RunWorkerCompleted += PushBackgroundWorker_RunWorkerCompleted;

                    backgroundWorker.DoWork += (sender1, e1) =>
                    {
                        RunGitCommand("push");
                    };

                    backgroundWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        void btnDeploy_OnClick(object sender, RoutedEventArgs e)
        {
            Deploy();
        }
        void btnDoAll_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                btnDoAll.Content = "Please wait...";
                btnDoAll.IsEnabled = false;

                using (var backgroundWorker = new BackgroundWorker())
                {
                    backgroundWorker.RunWorkerCompleted += DoAllBackgroundWorker_RunWorkerCompleted;

                    backgroundWorker.DoWork += (sender1, e1) =>
                    {

                        var stageCommandResult = RunGitCommand("add .");

                        if (string.IsNullOrEmpty(stageCommandResult) || stageCommandResult.StartsWith("warning: LF"))
                        {
                            var fetchFullOutput = RunGitCommand("status --short");

                            var changes = new List<string>(
                                         fetchFullOutput.Split(new string[] { "\r\n" },
                                         StringSplitOptions.RemoveEmptyEntries));
                            if (changes.Any())
                                RunGitCommand("commit -m 'commited-from-olive-microservice-explorer.'");
                            RunGitCommand("push");
                        }
                        else
                            MessageBox.Show(stageCommandResult, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    };

                    backgroundWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                var pattern = @"no changes added to commit";
                const RegexOptions OPTIONS = RegexOptions.Multiline | RegexOptions.IgnoreCase;
                var commitNotChangeMatch = Regex.Match(ex.Message, pattern, OPTIONS);
                if (commitNotChangeMatch.Success)
                    MessageBox.Show("There is no any changes to commit.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CommitBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindUnCommitedChanges();
            BindUnPushedChanges();
            btnCommit.Content = "Commit";
        }
        private void PushBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindUnCommitedChanges();
            BindUnPushedChanges();
            btnPush.Content = "Push";
        }
        private void DoAllBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindUnCommitedChanges();
            BindUnPushedChanges();
            Deploy();
            btnDoAll.Content = "Commit, push and deploy";
        }
        void BindUnPushedChanges()
        {
            try
            {
                string result = RunGitCommand("log @{u}.. ");

                if (result.None())
                {
                    txtCommited.Text = "There is no any unpushed changes.";
                    btnPush.IsEnabled = false;
                }
                else
                {
                    txtCommited.Text = result;
                    btnPush.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        void BindUnCommitedChanges()
        {
            try
            {
                var fetchFullOutput = RunGitCommand("status --short");

                var changes = new List<string>(
                             fetchFullOutput.Split(new string[] { "\r\n" },
                             StringSplitOptions.RemoveEmptyEntries));

                var changesList = changes.Select(x =>
                new LocalGitChange
                {
                    Description = x.Substring(3),
                    Type =
                        x.StartsWith("?? ") ? "Add" :
                        x.StartsWith(" M ") ? "Modify" :
                        x.StartsWith(" D ") ? "Delete" :
                        ""
                });

                DataContext = changesList.ToList();
                btnCommit.IsEnabled = changes.Count() > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        void Deploy()
        {
            var serverUrlFile = Path.Combine(_hubAddress, "website", "DeployServer.xml");
            var depolyWindow = new DeployWindow(serverUrlFile, _serviceName)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = $"Deploying {_serviceName} service"
            };
            depolyWindow.ShowDialog();

        }
        string RunGitCommand(string command)
        {
            return "git.exe".AsFile(searchEnvironmentPath: true)
               .Execute(command, waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = _serviceAddress);
        }
    }

    class LocalGitChange
    {
        public string Description { get; set; }
        public string Type { get; set; }
    }

    public class ChangeTypeBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var change = (LocalGitChange)value;
            if (change == null || typeof(Brush) != targetType)
                return value;

            switch (change.Type)
            {
                case "Add":
                    return new SolidColorBrush(Colors.LightGreen);
                case "Modify":
                    return new SolidColorBrush(Colors.LightYellow);
                case "Delete":
                    return new SolidColorBrush(Colors.Pink);
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
