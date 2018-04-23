using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MacroserviceExplorer.Utils;

namespace MacroserviceExplorer
{
    partial class MainWindow
    {
        public MacroserviceGridItem SelectedService { get; set; }
        public List<MacroserviceGridItem> serviceData = new List<MacroserviceGridItem>();
        public ObservableCollection<MacroserviceGridItem> MacroserviceGridItems = new ObservableCollection<MacroserviceGridItem>();

        MacroserviceGridItem GetServiceFromButtonTag(object sender)
        {
            var btn = (Button)sender;
            var serviceName = btn.Tag.ToString();
            return MacroserviceGridItems.Single(s => s.Service == serviceName);
        }

        void MakeChromeContextMenu(object sender, MacroserviceGridItem service)
        {
            var cm = new ContextMenu();
            if (int.TryParse(service.Port, out var port))
            {
                var webAddr = $"http://localhost:{port}";
                var localMenuItem = new MenuItem { Header = $"Local\t  {webAddr}" };
                localMenuItem.Click += BrowsItem_Click;
                localMenuItem.Tag = service;
                cm.Items.Add(localMenuItem);
            }

            var uatMenuItem = new MenuItem { Header = "UAT" };
            if (!string.IsNullOrEmpty(service.UatUrl))
            {
                uatMenuItem.Header += $"\t  {service.UatUrl}";
                uatMenuItem.Tag = service;
                uatMenuItem.Click += BrowsItem_Click;
            }
            else
                uatMenuItem.IsEnabled = false;
            cm.Items.Add(uatMenuItem);

            var liveMenuItem = new MenuItem { Header = "Live" };
            if (!string.IsNullOrEmpty(service.LiveUrl))
            {
                liveMenuItem.Header += $"\t  {service.LiveUrl}";
                liveMenuItem.Tag = service;
                liveMenuItem.Click += BrowsItem_Click;
            }
            else
                liveMenuItem.IsEnabled = false;

            cm.Items.Add(liveMenuItem);
            cm.PlacementTarget = (UIElement)sender;
            cm.IsOpen = true;
        }
        void BrowsItem_Click(object sender, RoutedEventArgs e)
        {
            var menuitem = (MenuItem)sender;
            menuitem.Click -= BrowsItem_Click;

            BrowsMacroservice(menuitem);
        }

        void BrowsMacroservice(MenuItem menuitem)
        {
            var service = (MacroserviceGridItem)menuitem.Tag;
            var address = menuitem.Header.ToString().Substring(menuitem.Header.ToString().IndexOf(" ", StringComparison.Ordinal) + 1);
            if (address.Contains("localhost:") && service.Status != MacroserviceGridItem.enumStatus.Run)
            {
                void OnServiceOnPropertyChanged(object obj, PropertyChangedEventArgs args)
                {
                    if (args.PropertyName != nameof(service.Status) || service.Status != MacroserviceGridItem.enumStatus.Run) return;
                    service.PropertyChanged -= OnServiceOnPropertyChanged;
                    Helper.Launch(address);
                }

                service.PropertyChanged += OnServiceOnPropertyChanged;
                StartService(service);
            }
            else
                Helper.Launch(address);
        }

        void Chrome_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceFromButtonTag(sender);

            MakeChromeContextMenu(sender, service);
        }

        void FilterListBy(string txtSearchText)
        {
            MacroserviceGridItems.Clear();
            if (txtSearch.Text.IsEmpty())
            {
                MacroserviceGridItems.AddRange(serviceData);
                return;
            }

            MacroserviceGridItems.AddRange(serviceData.Where(x => x.Service.ToLower().Contains(txtSearchText.ToLower()) || MSharpExtensions.OrEmpty(x.Port).Contains(txtSearchText)));
        }

    }
}
