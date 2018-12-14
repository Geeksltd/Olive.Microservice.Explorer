using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MicroserviceExplorer
{
    /// <summary>
    /// Interaction logic for NugetUpdatesWindow.xaml
    /// </summary>
    public partial class NugetUpdatesWindow : Window
    {
        List<NugetReference> _nugetList;
        readonly object _nugetCollectionLock = new object();
        public List<NugetReference> NugetList
        {
            get => _nugetList;
            set
            {
                _nugetList = value;
                BindingOperations.EnableCollectionSynchronization(_nugetList, _nugetCollectionLock);
                DataContext = _nugetList;
            }
        }


        public NugetUpdatesWindow(bool isUpdating)
        {
            InitializeComponent();

            if (isUpdating)
            {
                btnUpdate.Content = btnUpdateAll.Content = "Updating...";
                btnUpdate.IsEnabled = btnUpdateAll.IsEnabled = false;
            }
        }

        void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var chk = (CheckBox)sender;
            var list = _nugetList.Distinct(dist => dist.Project).Select(itm => itm.Project.ToString()).ToList();

            var chkTitle = chk.DataContext.ToString();
            var value = chk.IsChecked == null || !(bool)chk.IsChecked;
            switch (chkTitle)
            {
                case "All":
                    foreach (var proj in list)
                        SetCheckBoxValue(proj, !value);

                    SetCheckBoxValue("None", value);
                    break;

                case "None":

                    foreach (var proj in list)
                        SetCheckBoxValue(proj, value);

                    SetCheckBoxValue("All", value);
                    break;
                default:
                    _nugetList.Where(ng => ng.Project.ToString() == chkTitle).Do(itm =>
                      {
                          itm.ShouldUpdate = !(chk.IsChecked == null || !(bool)chk.IsChecked);
                      });
                    CollectionViewSource.GetDefaultView(_nugetList).Refresh();
                    break;
            }
        }

        void SetCheckBoxValue(string proj, bool value)
        {
            var findChild = FindChild<CheckBox>(this, proj);
            if (findChild != null)
                findChild.IsChecked = value;
        }

        static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (!(child is T childType))
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
                else if (childName.HasValue())
                {
                    if (!(child is FrameworkElement frameworkElement) || frameworkElement.Tag == null || frameworkElement.Tag.ToString() != childName) continue;
                    foundChild = (T)child;
                    break;
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        void BtnUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            if (_nugetList.None(itm => itm.ShouldUpdate))
                MessageBox.Show(@"There is not selected package to update ...", @"Please select an item at least ");
            else
                DialogResult = true;
        }

        void OnPackageChecked_OnChecked(object sender, RoutedEventArgs e)
        {
            var chk = (CheckBox)sender;
            var nugetRef = (NugetReference)chk.Tag;
            if (chk.IsChecked.HasValue)
                nugetRef.ShouldUpdate = chk.IsChecked.Value;

            _nugetList.Where(x => x == nugetRef).Do(itm => itm.ShouldUpdate = true);
            nugetRef.Service.References = _nugetList;

        }

        void UpdateAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_nugetList.None())
            {
                _nugetList.Do(itm => itm.ShouldUpdate = true);

                var button = (Button)sender;
                var nugetRef = ((List<NugetReference>)button.Tag).First();
                nugetRef.Service.References = _nugetList;

                DialogResult = true;
            }
            else
                MessageBox.Show(@"Please wait, packages are loading... ", @"Loading");

        }
    }
    public class ProjectTypeBackgroundColorConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myNugetRef = (NugetReference)value;
            if (myNugetRef == null || typeof(Brush) != targetType)
                return value;

            switch (myNugetRef.Project)
            {
                case SolutionProject.Website:
                    return new SolidColorBrush(Colors.Aquamarine);
                case SolutionProject.Domain:
                    return new SolidColorBrush(Colors.Cornsilk);
                case SolutionProject.Model:
                    return new SolidColorBrush(Colors.GhostWhite);
                case SolutionProject.UI:
                    return new SolidColorBrush(Colors.PaleTurquoise);
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            return value;
        }

        #endregion
    }
    public class DistinctConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable values))
                return null;

            var myNugetRefs = values.Cast<NugetReference>().Distinct(dist => dist.Project).Select(itm => itm.Project.ToString()).ToList();
            myNugetRefs.Insert(0, "All");
            myNugetRefs.Add("None");
            return myNugetRefs;
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
