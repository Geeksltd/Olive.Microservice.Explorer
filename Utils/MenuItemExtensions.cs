using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MicroserviceExplorer.Utils
{

    public class MenuItemExtensions : DependencyObject
    {
        public static Dictionary<MenuItem, String> ElementToGroupNames = new Dictionary<MenuItem, String>();

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.RegisterAttached("GroupName",
                                         typeof(String),
                                         typeof(MenuItemExtensions),
                                         new PropertyMetadata(String.Empty, OnGroupNameChanged));

        public static void SetGroupName(MenuItem element, String value)
        {
            element.SetValue(GroupNameProperty, value);
        }

        static string GetGroupName(DependencyObject element) => element.GetValue(GroupNameProperty).ToString();

        static void OnGroupNameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            //Add an entry to the group name collection

            if (!(dependencyObject is MenuItem menuItem)) return;
            var newGroupName = dependencyPropertyChangedEventArgs.NewValue.ToString();
            var oldGroupName = dependencyPropertyChangedEventArgs.OldValue.ToString();
            if (newGroupName.IsEmpty())
            {
                //Removing the toggle button from grouping
                RemoveCheckboxFromGrouping(menuItem);
            }
            else
            {
                //Switching to a new group
                if (newGroupName == oldGroupName) return;

                if (oldGroupName.HasValue())
                {
                    //Remove the old group mapping
                    RemoveCheckboxFromGrouping(menuItem);
                }
                ElementToGroupNames.Add(menuItem, dependencyPropertyChangedEventArgs.NewValue.ToString());
                menuItem.Checked += MenuItemChecked;
            }
        }

        static void RemoveCheckboxFromGrouping(MenuItem checkBox)
        {
            ElementToGroupNames.Remove(checkBox);
            checkBox.Checked -= MenuItemChecked;
        }


        static void MenuItemChecked(object sender, RoutedEventArgs e)
        {
            var menuItem = e.OriginalSource as MenuItem;
            foreach (var item in ElementToGroupNames)
            {
                if (item.Key != menuItem && item.Value == GetGroupName(menuItem))
                {
                    item.Key.IsChecked = false;
                }
            }
        }
    }
}
