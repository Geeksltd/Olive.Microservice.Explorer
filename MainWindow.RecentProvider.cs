using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;

namespace MacroserviceExplorer
{
    partial class MainWindow
    {
        List<string> _recentFiles = new List<string>();
        const string RecentsXml = "Recents.xml";

        async void RecentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await LoadFile(((MenuItem)e.Source).Header.ToString());
        }

        void SaveRecentFilesXml()
        {
            var serializer = new XmlSerializer(typeof(List<string>));
            using (var sww = new StringWriter())
            using (var writer = XmlWriter.Create(sww))
            {
                serializer.Serialize(writer, _recentFiles);
                File.WriteAllText(RecentsXml, sww.ToString().Replace("utf-16", "utf-8"));
            }
        }

        void ReloadRecentFiles()
        {
            var serializer = new XmlSerializer(typeof(List<string>));
            mnuRecentFiles.Items.Clear();
            using (var reader = XmlReader.Create(RecentsXml))
                _recentFiles = (List<string>)serializer.Deserialize(reader);

            foreach (var recentFile in _recentFiles)
                AddRecentMenuItem(recentFile);

        }
        void AddRecentMenuItem(string recentFile)
        {
            var menuItem = new MenuItem { Header = recentFile };
            menuItem.Click += RecentMenuItem_Click;
            mnuRecentFiles.Items.Insert(0, menuItem);
            var hasClearAll = false;
            foreach (var o in mnuRecentFiles.Items)
            {
                if (o is MenuItem item1 && item1.Header.ToString() != "Clear All") continue;
                hasClearAll = true;
            }

            if (!hasClearAll)
                AddClearRecentMenuItem();
        }
        void AddClearRecentMenuItem()
        {
            var menuItem = new MenuItem { Header = "Clear All" };
            menuItem.Click += (sender, args) =>
            {
                mnuRecentFiles.Items.Clear();
                mnuRecentFiles.Items.Add(new MenuItem { Header = "[Empty]" });
                RecentsXml.AsFile().Delete();
            };
            mnuRecentFiles.Items.Add(new Separator());
            mnuRecentFiles.Items.Add(menuItem);
        }

    }
}
