using System;
using System.Collections;
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
        ObservableCollection<MyNugetRef> _nugetList;
        readonly object _nugetCollectionLock = new object();
        public ObservableCollection<MyNugetRef> NugetList
        {
            get => _nugetList;
            set
            {
                _nugetList = value;
                BindingOperations.EnableCollectionSynchronization(_nugetList, _nugetCollectionLock);
                DataContext = _nugetList;
            }
        }

        public NugetUpdatesWindow()
        {
            InitializeComponent();
        }


        
        void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var chk = (CheckBox)sender;
            var list = _nugetList.Distinct(dist => dist.Project).Select(itm => itm.Project.ToString()).ToList();

            var chkTitle = chk.DataContext.ToString();
            var value = chk.IsChecked == null || !(bool) chk.IsChecked;
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
                    _nugetList.Where(ng=>ng.Project.ToString() == chkTitle).Do(itm=>
                    {
                        itm.Checked = !(chk.IsChecked == null || !(bool) chk.IsChecked);
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
            if (_nugetList.None(itm => itm.Checked))
                MessageBox.Show(@"There is not selected package to update ...", @"Please select an item atleast ");
            else
                DialogResult = true;
        }

        void OnPackageChecked_OnChecked(object sender, RoutedEventArgs e)
        {
            var chk = (CheckBox) sender;
            var nugetRef = (MyNugetRef)chk.Tag;
            if (chk.IsChecked.HasValue )
                nugetRef.Checked = chk.IsChecked.Value;
        }

        void UpdateAll_OnClick(object sender, RoutedEventArgs e)
        {
            _nugetList.Do(itm=>itm.Checked = true);
            DialogResult = true;
        }
    }

    public class ProjectTypeBackgroundColorConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myNugetRef = (MyNugetRef)value;
            if (myNugetRef == null || typeof(Brush) != targetType)
                return value;

            switch (myNugetRef.Project)
            {
                case MicroserviceItem.EnumProjects.Website:
                    return new SolidColorBrush(Colors.Aquamarine);
                case MicroserviceItem.EnumProjects.Domain:
                    return new SolidColorBrush(Colors.Cornsilk);
                case MicroserviceItem.EnumProjects.Model:
                    return new SolidColorBrush(Colors.GhostWhite);
                case MicroserviceItem.EnumProjects.UI:
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

            var myNugetRefs = values.Cast<MyNugetRef>().Distinct(dist => dist.Project).Select(itm => itm.Project.ToString()).ToList();
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
