using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Jvedio.Core.Extensions
{
    /// <summary>
    /// 单选菜单栏
    /// <para>参考：<see href="https://stackoverflow.com/a/3652980/13454100">stackoverflow</see></para>
    /// </summary>
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

        public static string GetGroupName(MenuItem element)
        {
            return element.GetValue(GroupNameProperty).ToString();
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Add an entry to the group name collection
            var menuItem = d as MenuItem;

            if (menuItem != null) {
                string newGroupName = e.NewValue.ToString();
                string oldGroupName = e.OldValue.ToString();
                if (string.IsNullOrEmpty(newGroupName)) {
                    // Removing the toggle button from grouping
                    RemoveCheckboxFromGrouping(menuItem);
                } else {
                    // Switching to a new group
                    if (newGroupName != oldGroupName) {
                        if (!string.IsNullOrEmpty(oldGroupName)) {
                            // Remove the old group mapping
                            RemoveCheckboxFromGrouping(menuItem);
                        }

                        ElementToGroupNames.Add(menuItem, e.NewValue.ToString());
                        menuItem.Checked += MenuItemChecked;
                    }
                }
            }
        }

        private static void RemoveCheckboxFromGrouping(MenuItem checkBox)
        {
            ElementToGroupNames.Remove(checkBox);
            checkBox.Checked -= MenuItemChecked;
        }

        static void MenuItemChecked(object sender, RoutedEventArgs e)
        {
            var menuItem = e.OriginalSource as MenuItem;
            foreach (var item in ElementToGroupNames) {
                if (item.Key != menuItem && item.Value == GetGroupName(menuItem)) {
                    item.Key.IsChecked = false;
                }
            }
        }
    }
}
