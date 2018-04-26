using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MacroserviceExplorer
{
    partial class MainWindow
    {
        class GitStatus
        {
            public string Branch { get; set; }
            public int RemoteCommits { get; set; }
            public int LocalCommits { get; set; }
        }

        async Task<int> GetGitUpdates(MacroserviceGridItem service)
        {
            if (service.WebsiteFolder.IsEmpty()) return 0;


            var projFOlder = service.WebsiteFolder.AsDirectory().Parent;
            if (projFOlder == null || !Directory.Exists(Path.Combine(projFOlder.FullName, ".git")))
            {
                return 0;
            }

            string run()
            {
                StatusProgressStart();
                ShowStatusMessage("Start git fetch ...", tooltip: null, logMessage: false);
                try
                {
                    var fetchoutput = "git.exe".AsFile(searchEnvironmentPath: true)
                        .Execute("fetch", waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = projFOlder.FullName);

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MyDelegate(() => ShowStatusMessage($"git fetch completed ... ({service.Service})", fetchoutput)));

                    return "git.exe".AsFile(searchEnvironmentPath: true)
                        .Execute("status", waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = projFOlder.FullName);
                }
                catch (Exception e)
                {
                    StatusProgressStop();
                    ShowStatusMessage("Error on git fetch ...", tooltip: null, logMessage: false);
                    logWindow.LogMessage("Error on git command ..." , e.Message);
                    return null;
                }
            }
            var output = await Task.Run((Func<string>)run);
            var status = ReadGitInfo(output);
            if(status!=null)
                ShowStatusMessage($"getting git commit count completed ... ({service.Service}) with {status?.RemoteCommits ?? 0} commit(s) in {status?.Branch ?? "it's branch"}", output);

            StatusProgressStop();

            return status?.RemoteCommits ?? 0;
        }

        GitStatus ReadGitInfo(string input)
        {
            if (input.IsEmpty())
                return null;
            var pattern = @"Your branch is behind '(?<branch>[a-zA-Z/]*)' by (?<remoteCommits>\d*) commit";
            const RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;

            var match = Regex.Match(input, pattern, options);
            var branch = match.Groups["branch"];
            var remoteCommits = match.Groups["remoteCommits"];
            if (match.Success)
                return new GitStatus { Branch = branch.Value, RemoteCommits = remoteCommits.Value.To<int>() };

            pattern = @"Your branch and '(?<branch>[a-zA-Z/]*)' have diverged,\nand have (?<localCommits>\d*) and (?<remoteCommits>\d*) different commit";
            match = Regex.Match(input, pattern, options);
            branch = match.Groups["branch"];
            remoteCommits = match.Groups["remoteCommits"];
            var localCommits = match.Groups["localCommits"];

            return match.Success ? new GitStatus { Branch = branch.Value, RemoteCommits = remoteCommits.Value.To<int>(), LocalCommits = localCommits.Value.To<int>() } : null;
        }

        async Task GitUpdate(MacroserviceGridItem server)
        {

            autoRefreshTimer.Stop();
            var projFOlder = server.WebsiteFolder.AsDirectory().Parent;
            string run()
            {
                StatusProgressStart();

                try
                {
                    return "git.exe".AsFile(searchEnvironmentPath: true)
                        .Execute("pull", waitForExit: true,
                            configuration: x => x.StartInfo.WorkingDirectory = projFOlder?.FullName);
                }
                catch (Exception e)
                {
                    ShowStatusMessage("error on git pull ...", e.Message);
                    StatusProgressStop();
                    return e.Message;
                }

            }
            var output = await Task.Run((Func<string>)run);
            if (output.HasValue())
                ShowStatusMessage("git pull completed ...", output);

            StatusProgressStop();
            autoRefreshTimer.Start();
        }

    }
}
