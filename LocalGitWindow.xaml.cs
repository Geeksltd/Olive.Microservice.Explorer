using System;
using System.Collections.Generic;
using System.Globalization;
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
        public LocalGitWindow(string serviceAddress)
        {
            InitializeComponent();
            _serviceAddress = serviceAddress;
            BindUnCommitedChanges();
            BindUnPushedChanges();
        }
        void BindUnPushedChanges()
        {
            txtCommited.Text = RunGitCommand("reflog --name-only");
        }
        void BindUnCommitedChanges()
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
        void btnCommit_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var stageCommandResult = RunGitCommand("add .");

                if (string.IsNullOrEmpty(stageCommandResult))
                {
                    RunGitCommand("commit -m 'commited-from-olive-microservice-explorer.'");
                    BindUnCommitedChanges();
                    BindUnPushedChanges();
                }
                else
                    MessageBox.Show(stageCommandResult, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                var pattern = @"no changes added to commit";
                const RegexOptions OPTIONS = RegexOptions.Multiline | RegexOptions.IgnoreCase;
                var commitNotChangeMatch = Regex.Match(ex.Message, pattern, OPTIONS);
                if (commitNotChangeMatch.Success)
                    MessageBox.Show("There is no changes to commit.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        void btnPush_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(@"Push action not implemented yet.");
        }
        void btnDeploy_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(@"Deploy action not implemented yet.");
        }
        void btnDoAll_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(@"Do All action not implemented yet.");
        }
        string RunGitCommand(string command)
        {
            try
            {
                return "git.exe".AsFile(searchEnvironmentPath: true)
                   .Execute(command, waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = _serviceAddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
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
