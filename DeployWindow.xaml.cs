using System;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using MicroserviceExplorer.Utils;

namespace MicroserviceExplorer
{
    /// <summary>
    /// Interaction logic for DeployWindow.xaml
    /// </summary>
    public partial class DeployWindow : Window
    {
        readonly string _serverUrlFile, _serviceName;

        public DeployWindow(string serverUrlFile, string serviceName)
        {
            InitializeComponent();
            _serverUrlFile = serverUrlFile;
            _serviceName = serviceName;
            BindServerUrl();
        }

        void BindServerUrl()
        {
            if (File.Exists(_serverUrlFile))
            {
                var server = XElement.Load(_serverUrlFile);
                var url = server.Attribute("url")?.Value;
                if (!url.None()) txtJenkinsUrl.Text = url;
            }
        }

        void SaveServerUrl()
        {
            var doc = new XmlDocument();
            doc.LoadXml($"<Jenkins url=\"{txtJenkinsUrl.Text}\" />");
            var writer = XmlWriter.Create(_serverUrlFile);
            doc.Save(writer);
        }

        void btnDeploy_Click(object sender, RoutedEventArgs e)
        {
            if (!txtJenkinsUrl.Text.None())
            {
                SaveServerUrl();
                Helper.Launch($"{txtJenkinsUrl.Text}/job/{_serviceName}/build");
            }
        }
    }
}