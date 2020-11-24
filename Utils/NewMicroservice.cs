using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using MicroserviceExplorer.UI;
using Newtonsoft.Json.Linq;
using MessageBox = System.Windows.Forms.MessageBox;
using Process = System.Diagnostics.Process;

namespace MicroserviceExplorer
{
    public class NewMicroserviceCreator
    {
        static DirectoryInfo ServicesDirectory => MainWindow.ServicesDirectory;
        string ServiceName, GitRepoUrl, PortNumber, Domain;

        public NewMicroserviceCreator(string serviceName, string gitRepoUrl)
        {
            ServiceName = serviceName;
            GitRepoUrl = gitRepoUrl;
        }

        public async Task Create()
        {
            var hubAddress = Path.Combine(ServicesDirectory.FullName, "hub");

            var appSettingsProductionAllText = File.ReadAllText(Path.Combine(hubAddress, "website", "appsettings.Production.json"));
            var appSettingsProductionJObject = JObject.Parse(appSettingsProductionAllText);
            Domain = appSettingsProductionJObject["Authentication"]["Cookie"]["Domain"].ToString();
            PortNumber = GetNextPortNumberFromHubServices(ServicesDirectory, out var serviesXmlPath).ToString();

            var serviceDirectoryPath = Path.Combine(ServicesDirectory.FullName, ServiceName);
            var serviceDirectory = new DirectoryInfo(serviceDirectoryPath);

            if (!serviceDirectory.Exists)
                serviceDirectory.Create();

            var tmpFolder = await CreateTemplateAsync(serviceDirectoryPath);

            if (tmpFolder.IsEmpty()) return;

            AddMicroserviceToHubServices(serviesXmlPath);
            Execute(serviceDirectoryPath, "build.bat", null);

            try
            {
                Execute(serviceDirectoryPath, "git", "init");
                Execute(serviceDirectoryPath, "git", $"remote add origin {GitRepoUrl}");
                Execute(serviceDirectoryPath, "git", "add .");
                Execute(serviceDirectoryPath, "git", "commit -m \"Initial commit\"");
                Execute(serviceDirectoryPath, "git", "push");
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Error :{ex.Message}", @"Template initialization failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        async Task<string> CreateTemplateAsync(string solutionFolder)
        {
            var downloadedFilesExtractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(downloadedFilesExtractPath);
            try
            {
                if (!await DownloadAsync(TemplateWebAddress, downloadedFilesExtractPath, ZIP_FILE_NAME)) return null;

                var zipFilePath = Path.Combine(downloadedFilesExtractPath, ZIP_FILE_NAME);
                ZipFile.ExtractToDirectory(zipFilePath, downloadedFilesExtractPath);
                File.Delete(zipFilePath);
                Rename(downloadedFilesExtractPath);

                CopyFiles(solutionFolder, downloadedFilesExtractPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"Error in download or create new Microservice", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return downloadedFilesExtractPath;
        }

        void CopyFiles(string solutionFolder, string extractPathPath)
        {
            try
            {
                var template = Directory.GetDirectories(extractPathPath).FirstOrDefault();
                if (template.IsEmpty()) return;
                var templateDirectory = Directory.GetDirectories(template).FirstOrDefault();
                var tempDIrObj = new DirectoryInfo(templateDirectory);
                if (tempDIrObj.Name != TEMPLATE_FOLDER_NAME) return;
                CopyFolderContents(templateDirectory, solutionFolder);
                DeleteDirectory(extractPathPath);
            }
            catch
            {
                // ignored
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;

            // Delete all files from the Directory
            foreach (var file in Directory.GetFiles(path))
                File.Delete(file);

            // Delete all child Directories
            foreach (var directory in Directory.GetDirectories(path))
                DeleteDirectory(directory);

            // Delete a Directory
            Directory.Delete(path);
        }

        public static bool CopyFolderContents(string sourcePath, string destinationPath)
        {
            sourcePath = sourcePath.EndsWith(@"\") ? sourcePath : sourcePath + @"\";
            destinationPath = destinationPath.EndsWith(@"\") ? destinationPath : destinationPath + @"\";

            try
            {
                if (Directory.Exists(sourcePath))
                {
                    if (Directory.Exists(destinationPath) == false)
                        Directory.CreateDirectory(destinationPath);

                    foreach (var files in Directory.GetFiles(sourcePath))
                    {
                        var fileInfo = files.AsFile();
                        fileInfo.CopyTo($@"{destinationPath}\{fileInfo.Name}", overwrite: true);
                    }

                    foreach (var drs in Directory.GetDirectories(sourcePath))
                    {
                        var directoryInfo = new DirectoryInfo(drs);
                        if (CopyFolderContents(drs, destinationPath + directoryInfo.Name) == false)
                            return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected void Rename(string downloadedFilesExtractPath)
        {
            var replacements = new Dictionary<string, string>
            {
                {"MY.MICROSERVICE.NAME", ServiceName},
                {"MY.SOLUTION",ServicesDirectory.Name  },
                {"my-solution-domain",Domain.TrimStart('*').TrimStart('.') },
                {"mysolution",Domain.Remove(".")},
                {"9012", PortNumber }
            };

            foreach (var item in replacements)
                RenameHelper(downloadedFilesExtractPath, item.Key, item.Value);
        }

        public static string SerializeToString<T>(T value)
        {
            var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(value.GetType());
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, value, emptyNamepsaces);
                return stream.ToString();
            }
        }

        public static async Task<bool> DownloadAsync(string sourceWebAddress, string destPath, string fileName)
        {
            var destFullPath = Path.Combine(destPath, fileName);
            try
            {
                Loading loadingWindow;
                HttpResponseMessage response;
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(new TimeSpan(0, 0, 2, 30));
                    loadingWindow = new Loading(cancellationTokenSource);

                    loadingWindow.Show();
                    var httpClient = new HttpClient();
                    response = await httpClient.GetAsync(sourceWebAddress, HttpCompletionOption.ResponseContentRead, cancellationTokenSource.Token);
                }

                response.EnsureSuccessStatusCode();

                using (var fileStream = new FileStream(destFullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    await response.Content.CopyToAsync(fileStream);

                loadingWindow.Close();
                return true;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error in downloading template file \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OperationCanceledException ex)
            {
                MessageBox.Show("Project template download canceled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in downloading template file \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        protected static void RenameHelper(string destPath, string templateKey, string templateValue)
        {
            foreach (var file in Directory.GetFiles(destPath, "*.*", SearchOption.AllDirectories))
            {
                var fileContent = File.ReadAllText(file);
                if (fileContent.Contains(templateKey, caseSensitive: false))
                {
                    fileContent = fileContent.Replace(templateKey, templateValue);
                    fileContent = fileContent.Replace(templateKey.ToLower(), templateValue.ToLowerOrEmpty());

                    File.WriteAllText(file, fileContent);
                }

                var fileName = Path.GetFileName(file);
                if (fileName.Contains(templateKey))
                    File.Move(file, file.Replace(templateKey, templateValue));
            }
        }

        void AddMicroserviceToHubServices(string serviesXmlPath)
        {
            var services = XDocument.Load(serviesXmlPath);
            var node = new XElement(ServiceName,
                new XAttribute("url", $"http://localhost:{PortNumber}"),
                new XAttribute("production", $"https://{ServiceName}.{Domain}"));

            services.Root?.AddFirst(node);
            services.Save(serviesXmlPath);
        }

        int GetNextPortNumberFromHubServices(DirectoryInfo serviceJsonDir, out string serviesXmlPath)
        {
            serviesXmlPath = null;
            var hubDir = serviceJsonDir;
            while (hubDir?.Parent != null && !Directory.Exists(Path.Combine(hubDir.FullName, "hub")))
                hubDir = hubDir.Parent;

            if (hubDir == null) return 0;

            var servicePath = Path.Combine(hubDir.FullName, "hub", "website", "services.xml");
            if (!File.Exists(servicePath)) return 0;

            serviesXmlPath = servicePath;

            var services = XElement.Load(servicePath);
            var nextPortNumber = (from srv in services.FirstNode.ElementsAfterSelf()
                                  where srv.Attribute("url") != null && srv.Attribute("url").Value.Replace("://", "").Contains(":")
                                  let y = srv.Attribute("url")?.Value.Replace("://", "").TrimBefore(":", caseSensitive: true, trimPhrase: true)
                                  where int.TryParse(y, out _)
                                  select Convert.ToInt32(y)).Max() + 1;
            return nextPortNumber;
        }

        public string TemplateWebAddress => @"https://github.com/Geeksltd/Olive.Mvc.Microservice.Template/archive/master.zip";
        const string ZIP_FILE_NAME = "master.zip", TEMPLATE_FOLDER_NAME = "Template";

        public static void Execute(string workingDirectory, string command, string args, bool createNoWindow = true)
        {
            var output = new StringBuilder();
            var proc = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {

                    FileName = command,
                    Arguments = args ?? "",
                    WorkingDirectory = workingDirectory ?? "",
                    CreateNoWindow = createNoWindow,
                    UseShellExecute = true,
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    //Verb = "runas",
                    //StandardOutputEncoding = Encoding.UTF8,
                    //StandardErrorEncoding = Encoding.UTF8
                }
            };

            // proc.ErrorDataReceived += (sender, e) =>
            // {
            //    if (e.Data.IsEmpty()) return;
            //    output.AppendLine(e.Data);
            // };

            // proc.OutputDataReceived += (DataReceivedEventHandler)((sender, e) =>
            // {
            //    if (e.Data == null) return;
            //    output.AppendLine(e.Data);
            // });

            // proc.Exited += (object sender, EventArgs e) =>
            // {
            //    //Console.Beep();
            // };
            proc.Start();

            // proc.BeginOutputReadLine();
            // proc.BeginErrorReadLine();

            proc.WaitForExit();

            if ((uint)proc.ExitCode > 0U)
                throw new Exception($"External command failed: \"{command}\" {args}\r\n\r\n{(object)output}");

            // return output.ToString();
        }
    }
}