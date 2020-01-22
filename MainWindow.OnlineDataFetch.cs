using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MicroserviceExplorer
{
    partial class MainWindow
    {
        class GitStatus
        {
            public string Branch { get; set; }
            public int GitRemoteCommits { get; set; }
            public int LocalCommits { get; set; }
        }

        async Task CalculateGitUpdates(MicroserviceItem service)
        {
            if (service.WebsiteFolder.IsEmpty()) return;
            service.RollProgress($"Fetch '{service.Service}' local git updates ...");
            var projFOlder = service.WebsiteFolder.AsDirectory().Parent;
            if (projFOlder == null || !Directory.Exists(Path.Combine(projFOlder.FullName, ".git"))) return;

            service.GitUpdates = "0";
            try
            {
                var fetchoutput = "git.exe".AsFile(searchEnvironmentPath: true)
                    .Execute("fetch", waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = projFOlder.FullName);

                service.LogMessage($"git fetch completed ... ({service.Service})");

                var output = "git.exe".AsFile(searchEnvironmentPath: true)
                    .Execute("status", waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = projFOlder.FullName);

                var status = ReadGitInfo(output);
                if (status != null && status.GitRemoteCommits > 0)
                    service.LogMessage($"There are {status.GitRemoteCommits} git commit(s) available to update .");

                service.GitUpdates = status?.GitRemoteCommits.ToString();
            }
            catch (Exception e)
            {
                service.LogMessage("Error on git command ...", e.Message);
                service.StopProgress($"Error on git command, please open the '{service}' log window for detail.");
            }

            service.GitUpdateIsInProgress = false;
            service.StopProgress();
        }
        void LocalGitChanges(MicroserviceItem service)
        {
            service.LocalGitChanges = "...";
            service.LocalGitTooltip = "no changes.";
            
            if (service.WebsiteFolder.IsEmpty()) return;

            var projFOlder = service.WebsiteFolder.AsDirectory().Parent;
            if (projFOlder == null || !Directory.Exists(Path.Combine(projFOlder.FullName, ".git"))) return;

            try
            {
                var fetchoutput = "git.exe".AsFile(searchEnvironmentPath: true)
                     .Execute("status --short", waitForExit: true, configuration: x => x.StartInfo.WorkingDirectory = projFOlder.FullName);

                var changes = new List<string>(
                           fetchoutput.Split(new string[] { "\r\n" },
                           StringSplitOptions.RemoveEmptyEntries)).Count();

                if (changes > 0)
                    service.LocalGitChanges = changes.ToString();

                service.LocalGitTooltip = changes > 0 ? changes.ToString() + " uncommited changes." : "no changes.";
            }
            catch (Exception e)
            {
                service.LogMessage("Error on git command ...", e.Message);
            }
        }

        GitStatus ReadGitInfo(string input)
        {
            if (input.IsEmpty()) return null;
            var pattern = @"Your branch is behind '(?<branch>[a-zA-Z/]*)' by (?<remoteCommits>\d*) commit";
            const RegexOptions OPTIONS = RegexOptions.Multiline | RegexOptions.IgnoreCase;

            var match = Regex.Match(input, pattern, OPTIONS);
            var branch = match.Groups["branch"];
            var remoteCommits = match.Groups["remoteCommits"];
            if (match.Success)
                return new GitStatus { Branch = branch.Value, GitRemoteCommits = remoteCommits.Value.To<int>() };

            pattern = @"Your branch and '(?<branch>[a-zA-Z/]*)' have diverged,\nand have (?<localCommits>\d*) and (?<remoteCommits>\d*) different commit";
            match = Regex.Match(input, pattern, OPTIONS);
            branch = match.Groups["branch"];
            remoteCommits = match.Groups["remoteCommits"];
            var localCommits = match.Groups["localCommits"];

            if (match.Success)
                return new GitStatus
                {
                    Branch = branch.Value,
                    GitRemoteCommits = remoteCommits.Value.To<int>(),
                    LocalCommits = localCommits.Value.To<int>()
                };

            return null;
        }

        async Task GitUpdate(MicroserviceItem service)
        {
            AutoRefreshTimer.Stop();
            var projFOlder = service.WebsiteFolder.AsDirectory().Parent;
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
                    service.LogMessage("error on git pull ...", e.Message);
                    // StatusProgressStop();
                    return e.Message;
                }
            }

            var output = await Task.Run((Func<string>)run);
            if (output.HasValue())
                service.LogMessage("git pull completed.", output);

            CalculateGitUpdates(service);

            StatusProgressStop();
            AutoRefreshTimer.Start();
        }
    }
}