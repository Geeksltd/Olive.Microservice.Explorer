using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using GCop.Core;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MacroserviceExplorer
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

        void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            //ICollectionView cvTasks = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
            if (!(e.Item is MyNugetRef t)) return;

            //if (this.cbCompleteFilter.IsChecked == true && t.Complete == true)
            //    e.Accepted = false;
            //else
            //    e.Accepted = true;
        }

        
        void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var chk = (CheckBox)sender;
            var list = _nugetList.Distinct(dist => dist.Project).Select(itm => itm.Project.ToString()).ToList();

            var chkTitle = chk.DataContext.ToString();
            switch (chkTitle)
            {
                case "All":
                    foreach (var proj in list)
                        SetCheckBoxValue(proj, chk.IsChecked != null && chk.IsChecked.Value);

                    SetCheckBoxValue("None", !(chk.IsChecked != null && chk.IsChecked.Value));
                    break;

                case "None":

                    foreach (var proj in list)
                        SetCheckBoxValue(proj, !(chk.IsChecked != null && chk.IsChecked.Value));

                    SetCheckBoxValue("All", !(chk.IsChecked != null && chk.IsChecked.Value));
                    break;
                default:
                    _nugetList.Where(ng=>ng.Project.ToString() == chkTitle).ForEach(itm=>
                    {
                        itm.Checked = (chk.IsChecked != null && chk.IsChecked.Value);
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
                else if (!string.IsNullOrEmpty(childName))
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
            if (!_nugetList.Any(itm => itm.Checked))
                MessageBox.Show("There is not selected package to update ...", "Please select an item atleast ");
            else
                DialogResult = true;
        }

        void OnPackageChecked_OnChecked(object sender, RoutedEventArgs e)
        {
            var chk = (CheckBox) sender;
            var nugetRef = (MyNugetRef)chk.Tag;
            if (chk.IsChecked != null)
                nugetRef.Checked = chk.IsChecked.Value;
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
                case MacroserviceGridItem.EnumProjects.Website:
                    return new SolidColorBrush(Colors.Aquamarine);
                case MacroserviceGridItem.EnumProjects.Domain:
                    return new SolidColorBrush(Colors.Cornsilk);
                case MacroserviceGridItem.EnumProjects.Model:
                    return new SolidColorBrush(Colors.GhostWhite);
                case MacroserviceGridItem.EnumProjects.UI:
                    return new SolidColorBrush(Colors.PaleTurquoise);
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
