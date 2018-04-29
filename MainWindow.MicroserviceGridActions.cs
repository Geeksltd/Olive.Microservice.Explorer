using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MicroserviceExplorer.Utils;

namespace MicroserviceExplorer
{
    partial class MainWindow
    {
        public MicroserviceItem SelectedService { get; set; }
        public List<MicroserviceItem> ServiceData = new List<MicroserviceItem>();
        public ObservableCollection<MicroserviceItem> MicroserviceGridItems = new ObservableCollection<MicroserviceItem>();

        MicroserviceItem GetServiceByTag(object sender)
        {
            var element = (FrameworkElement)sender;
            //var serviceName = element.Tag.ToString();
            return element.Tag as MicroserviceItem;//MicroserviceGridItems.Single(s => s.Service == serviceName);
        }

        void MakeChromeContextMenu(object sender, MicroserviceItem service)
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
            if (service.UatUrl.HasValue())
            {
                uatMenuItem.Header += $"\t  {service.UatUrl}";
                uatMenuItem.Tag = service;
                uatMenuItem.Click += BrowsItem_Click;
            }
            else
                uatMenuItem.IsEnabled = false;
            cm.Items.Add(uatMenuItem);

            var liveMenuItem = new MenuItem { Header = "Live" };
            if (service.LiveUrl.HasValue())
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
            var service = (MicroserviceItem)menuitem.Tag;
            var address = menuitem.Header.ToString().Substring(menuitem.Header.ToString().IndexOf(" ", StringComparison.Ordinal) + 1);
            if (address.Contains("localhost:") && service.Status != MicroserviceItem.EnumStatus.Run)
            {
                void OnServiceOnPropertyChanged(object obj, PropertyChangedEventArgs args)
                {
                    if (args.PropertyName != nameof(service.Status) || service.Status != MicroserviceItem.EnumStatus.Run) return;
                    service.PropertyChanged -= OnServiceOnPropertyChanged;
                    Helper.Launch(address);
                }

                service.PropertyChanged += OnServiceOnPropertyChanged;
                Start(service);
            }
            else
                Helper.Launch(address);
        }

        void Chrome_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);

            MakeChromeContextMenu(sender, service);
        }

        void FilterListBy(string txtSearchText)
        {
            MicroserviceGridItems.Clear();
            if (txtSearch.Text.IsEmpty())
            {
                MicroserviceGridItems.AddRange(ServiceData);
                return;
            }

            MicroserviceGridItems.AddRange(ServiceData.Where(x => x.Service.ToLower().Contains(txtSearchText.ToLower()) || x.Port.OrEmpty().Contains(txtSearchText)));
        }

    }
}
