﻿using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MicroserviceExplorer.NewMicroservice
{
	/// <summary>
	/// Interaction logic for NewMicroservice.xaml
	/// </summary>
	public partial class NewMicroservice : Window
	{
		public string ServiceName
		{
			get => txtServiceName.Text;
			set => txtServiceName.Text = value;
		}

		public string GitRepoUrl
		{
			get => txtGitRepoUrl.Text;
			set => txtGitRepoUrl.Text = value;
		}
		public NewMicroservice() => InitializeComponent();

		void cancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

		void btnCreate_Click(object sender, RoutedEventArgs e) => DialogResult = true;

		public static string Execute3(string workingDirectory, string command, string args, bool createNoWindow = true)
		{
			var output = new StringBuilder();
			var proc = new System.Diagnostics.Process
			{
				EnableRaisingEvents = true,
				StartInfo = new ProcessStartInfo
				{

					FileName = command,
					Arguments = args ?? "",
					WorkingDirectory = workingDirectory ?? "",
					CreateNoWindow = createNoWindow,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					Verb = "runas",
					StandardOutputEncoding = Encoding.UTF8,
					StandardErrorEncoding = Encoding.UTF8
				}
			};

			proc.ErrorDataReceived += (sender, e) =>
			{
				if (e.Data.IsEmpty()) return;
				output.AppendLine(e.Data);
			};

			proc.OutputDataReceived += (sender, e) =>
			{
				if (e.Data.IsEmpty()) return;
				output.AppendLine(e.Data);
			};

			// proc.Exited += (sender,e) =>
			// {
			//     Console.Beep();
			// };
			proc.Start();

			proc.BeginOutputReadLine();
			proc.BeginErrorReadLine();

			proc.WaitForExit(3000);

			if ((uint)proc.ExitCode > 0U)
				throw new Exception($"External command failed: \"{command}\" {args}\r\n\r\n{output.ToString()}");

			return output.ToString();
		}

		void TxtProjectName_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			var validateGitRepoUrl = ValidateGitRepoUrl(txtGitRepoUrl.Text);
			btnCreate.IsEnabled = CanCreate() && validateGitRepoUrl;
		}

		bool ValidateGitRepoUrl(string url)
		{
			return true;
			if (url.IsEmpty()) return false;

			var pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
			var regex = new Regex(pattern, RegexOptions.Singleline);
			LblError.Content = regex.Match(url).Success ? "" : "Git Repository Url is not in correct format ...";
			return regex.Match(url).Success;
		}

		bool CanCreate() => !(txtServiceName.Text.IsEmpty() || txtGitRepoUrl.Text.IsEmpty());
	}
}