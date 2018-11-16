﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MicroserviceExplorer.Utils;
using MessageBox = System.Windows.Forms.MessageBox;

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
        void BrowsMicroservice(MicroserviceItem service)
        {
            if (service.Status != MicroserviceItem.EnumStatus.Run)
            {
                var ans =MessageBox.Show(@"Microservice not started, Do you want to start service first ?", @"Start Service" , MessageBoxButtons.YesNoCancel);
                switch (ans)
                {
                    case System.Windows.Forms.DialogResult.Cancel:
                        return;
                    case System.Windows.Forms.DialogResult.Yes:
                        service.Start();
                        return;
                    case System.Windows.Forms.DialogResult.None:
                    case System.Windows.Forms.DialogResult.OK:
                    case System.Windows.Forms.DialogResult.Abort:
                    case System.Windows.Forms.DialogResult.Retry:
                    case System.Windows.Forms.DialogResult.Ignore:
                    case System.Windows.Forms.DialogResult.No:
                        break;

                }
            }
                Helper.Launch($"http://localhost:{service.Port}");
        }

        void Chrome_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var service = GetServiceByTag(sender);
            BrowsMicroservice(service);
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
