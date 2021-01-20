using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MicroserviceExplorer.Utils
{
    public class MenuItemExtensions : DependencyObject
    {
        public static Dictionary<MenuItem, string> ElementToGroupNames = new Dictionary<MenuItem, string>();

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.RegisterAttached("GroupName",
                                         typeof(string),
                                         typeof(MenuItemExtensions),
                                         new PropertyMetadata(string.Empty, OnGroupNameChanged));

        public static void SetGroupName(MenuItem element, string value)
        {
            element.SetValue(GroupNameProperty, value);
        }

        static string GetGroupName(DependencyObject element) => element.GetValue(GroupNameProperty).ToString();

        static void OnGroupNameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            // Add an entry to the group name collection

            if (!(dependencyObject is MenuItem menuItem)) return;
            var newGroupName = dependencyPropertyChangedEventArgs.NewValue.ToString();
            var oldGroupName = dependencyPropertyChangedEventArgs.OldValue.ToString();
            if (newGroupName.IsEmpty())
            {
                // Removing the toggle button from grouping
                RemoveCheckboxFromGrouping(menuItem);
            }
            else
            {
                // Switching to a new group
                if (newGroupName == oldGroupName) return;

                if (oldGroupName.HasValue())
                {
                    // Remove the old group mapping
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