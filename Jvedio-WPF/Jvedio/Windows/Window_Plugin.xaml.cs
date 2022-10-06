using FontAwesome.WPF;
using Jvedio.CommonNet;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Logs;
using Jvedio.Core.Plugins;
using Jvedio.Core.Plugins.Crawler;
using Jvedio.Entity;
using Jvedio.Mapper;
using Jvedio.ViewModel;
using JvedioLib.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperControls.Style;
using SuperUtils.Common;
using SuperUtils.External;
using SuperUtils.Framework.MarkDown;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Jvedio.Core.Global.UrlManager;
using static Jvedio.VisualTools.WindowHelper;

namespace Jvedio
{
    /// <summary>
    /// Interaction logic for Window_Plugin.xaml
    /// </summary>
    public partial class Window_Plugin : SuperControls.Style.BaseWindow
    {
        public VieModel_Plugin vieModel { get; set; }
        public Window_Plugin()
        {
            InitializeComponent();
            vieModel = new VieModel_Plugin();
            this.DataContext = vieModel;
        }

        private void BaseWindow_ContentRendered(object sender, EventArgs e)
        {

            // 设置插件排序
            var menuItems = pluginSortBorder.ContextMenu.Items.OfType<MenuItem>().ToList();
            for (int i = 0; i < menuItems.Count; i++)
            {
                menuItems[i].Click += SortMenu_Click;
                menuItems[i].IsCheckable = true;
            }

            // 同步远程插件
            ConfigManager.PluginConfig.FetchPluginMetaData(() =>
            {
                // 更新插件状态
                Dispatcher.Invoke(() => { SetRemotePluginMetaData(); });
            });

            vieModel.RenderPlugins();
            SetRemotePluginMetaData();

            pluginDetailGrid.Visibility = Visibility.Hidden;
        }

        private List<PluginMetaData> ParsePluginMetaDataFromJson(string pluginList)
        {
            List<string> plugin_key = new List<string>() { "crawlers", "themes" };
            List<PluginMetaData> result = new List<PluginMetaData>();
            try
            {
                Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(pluginList);
                if (dict != null && dict.Count > 0)
                {
                    foreach (string key in plugin_key)
                    {
                        if (!dict.Any(arg => arg.Key.Equals(key) || dict[key] == null))
                            continue;

                        List<Dictionary<string, string>> datas = null;
                        JArray data = null;
                        try
                        {
                            data = (JArray)dict[key];
                            datas = data.ToObject<List<Dictionary<string, string>>>();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }

                        if (datas == null || datas.Count <= 0) continue;
                        foreach (Dictionary<string, string> item in datas)
                        {
                            if (!item.ContainsKey("PluginID") || item["PluginID"] == null) continue;
                            PluginMetaData metaData = new PluginMetaData();
                            if (item.ContainsKey("Version") && item["Version"] != null)
                                metaData.NewVersion = item["Version"].ToString();
                            if (item.ContainsKey("PluginName") && item["PluginName"] != null)
                                metaData.PluginName = item["PluginName"].ToString();
                            if (item.ContainsKey("Date") && item["Date"] != null)
                                metaData.ReleaseNotes.Date = item["Date"].ToString();
                            if (item.ContainsKey("Desc") && item["Desc"] != null)
                                metaData.ReleaseNotes.Desc = item["Desc"].ToString();
                            if ("crawlers".Equals(key.ToLower()))
                            {
                                metaData.SetPluginID(PluginType.Crawler, item["PluginID"].ToString());
                                metaData.PluginType = PluginType.Crawler;
                            }
                            else if ("themes".Equals(key.ToLower()))
                            {
                                metaData.SetPluginID(PluginType.Theme, item["PluginID"].ToString());
                                metaData.PluginType = PluginType.Theme;
                            }

                            metaData.SetRemoteUrl();
                            if (item.ContainsKey("ImageUrl") && item["ImageUrl"] != null)
                                metaData.ImageUrl = item["ImageUrl"].ToString();
                            result.Add(metaData);
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return null;
        }

        private void SetRemotePluginMetaData()
        {
            // 未安装创建
            vieModel.AllFreshPlugins = new List<PluginMetaData>();
            string pluginList = ConfigManager.PluginConfig.PluginList;
            if (!string.IsNullOrEmpty(pluginList))
            {
                List<PluginMetaData> pluginMetaDatas = ParsePluginMetaDataFromJson(pluginList);
                if (pluginMetaDatas?.Count > 0)
                {
                    foreach (PluginMetaData info in pluginMetaDatas)
                    {
                        PluginMetaData installed = vieModel.InstalledPlugins.Where(arg => arg.PluginID.Equals(info.PluginID)).FirstOrDefault();
                        if (installed == null)
                        {
                            // 新插件
                            vieModel.AllFreshPlugins.Add(info);
                        }
                        else
                        {
                            // 检查更新
                            if (installed.ReleaseNotes.Version.CompareTo(info.NewVersion) < 0)
                            {
                                installed.HasNewVersion = true;
                                installed.NewVersion = info.NewVersion;
                            }
                        }
                    }
                }
            }

            vieModel.CurrentFreshPlugins = new ObservableCollection<PluginMetaData>();
            foreach (var item in vieModel.GetSortResult(vieModel.AllFreshPlugins))
                vieModel.CurrentFreshPlugins.Add(item);
        }

        private void SortMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            for (int i = 0; i < contextMenu.Items.Count; i++)
            {
                MenuItem item = (MenuItem)contextMenu.Items[i];
                if (item == menuItem)
                {
                    item.IsChecked = true;
                    if (i == vieModel.PluginSortIndex)
                    {
                        vieModel.PluginSortDesc = !vieModel.PluginSortDesc;
                    }

                    vieModel.PluginSortIndex = i;
                }
                else item.IsChecked = false;
            }

            vieModel.RenderPlugins();
        }


        private void pluginViewListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            object item = pluginViewListBox.SelectedItem;
            if (item != null && item is PluginMetaData pluginMetaData)
            {
                vieModel.CurrentPlugin = vieModel.InstalledPlugins.Where(arg => arg.PluginID.Equals(pluginMetaData.PluginID)).FirstOrDefault();
                richTextBox.Document = MarkDown.parse(vieModel.CurrentPlugin.ReleaseNotes.MarkDown);
                pluginDetailGrid.Visibility = Visibility.Visible;
            }
        }

        private async void freshViewListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            object item = freshViewListBox.SelectedItem;
            if (item != null && item is PluginMetaData pluginMetaData)
            {
                PluginMetaData data = vieModel.AllFreshPlugins.Where(arg => arg.PluginID.Equals(pluginMetaData.PluginID)).FirstOrDefault();
                vieModel.CurrentPlugin = data;
                richTextBox.Document.Blocks.Clear();
                pluginDetailGrid.Visibility = Visibility.Visible;
                PluginMetaData fetchedData = await PluginManager.FetchPlugin(data);
                if (fetchedData != null && vieModel.CurrentPlugin.PluginID.Equals(fetchedData.PluginID))
                {
                    vieModel.CurrentPlugin = null;
                    vieModel.CurrentPlugin = fetchedData;
                    System.Windows.Documents.FlowDocument doc = MarkDown.parse(fetchedData.ReleaseNotes.MarkDown);
                    this.Dispatcher.Invoke(() =>
                    {
                        richTextBox.Document = doc;
                    });
                }
            }
        }

        private void SavePluginEnabled(object sender, RoutedEventArgs e)
        {
            ConfigManager.Settings.PluginEnabled = new Dictionary<string, bool>();
            bool enabled = (bool)(sender as SuperControls.Style.Switch).IsChecked;
            foreach (PluginMetaData plugin in PluginManager.PluginList)
            {
                if (plugin.PluginID.Equals(vieModel.CurrentPlugin.PluginID))
                    plugin.Enabled = enabled;
                ConfigManager.Settings.PluginEnabled.Add(plugin.PluginID, plugin.Enabled);
            }

            ConfigManager.Settings.PluginEnabledJson = JsonConvert.SerializeObject(ConfigManager.Settings.PluginEnabled);
            vieModel.SetServers();
        }


        private void SearchPlugin(object sender, RoutedEventArgs e)
        {
            SearchBox searchBox = sender as SearchBox;
            vieModel.PluginSearch = searchBox.Text;
            vieModel.RenderPlugins();
        }

        private void DownloadPlugin(object sender, RoutedEventArgs e)
        {
            string pluginID = vieModel.CurrentPlugin.PluginID;
            PluginMetaData pluginMetaData;
            bool installing = false;
            pluginMetaData = vieModel.InstalledPlugins.Where(arg => arg.PluginID.Equals(pluginID)).FirstOrDefault();
            if (pluginMetaData != null)
            {
                installing = true;
            }
            else
            {
                pluginMetaData = vieModel.CurrentFreshPlugins.Where(arg => arg.PluginID.Equals(pluginID)).FirstOrDefault();
            }

            pluginMetaData.Installing = installing;

            // 加入到下载列表中
            if (PluginManager.DownloadingList.Any(arg => arg.Equals(pluginMetaData.PluginID)))
            {
                MessageCard.Warning($"【{pluginMetaData.PluginName}】下载中或已下载");
                return;
            }

            PluginManager.DownloadingList.Add(pluginMetaData.PluginID);
            PluginManager.DownloadPlugin(pluginMetaData);
        }


        private async void RefreshPluginList(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            border.IsEnabled = false;

            await Task.Delay(500);

            border.IsEnabled = true;
        }

        private void SetViewEnabled(object sender, RoutedEventArgs e)
        {
            (bool isChecked, int idx) = SetChecked(sender as MenuItem);
            if (!isChecked)
            {
                vieModel.SortEnabledIndex = -1;
            }
            else
            {
                vieModel.SortEnabledIndex = idx;
            }

            vieModel.RefreshCurrentPlugins();
        }

        private void SetPluginType(object sender, RoutedEventArgs e)
        {
            (bool isChecked, int idx) = SetChecked(sender as MenuItem);
            if (!isChecked)
            {
                vieModel.SortPluginType = PluginType.None;
            }
            else
            {
                vieModel.SortPluginType = (PluginType)idx;
            }

            vieModel.RefreshCurrentPlugins();
        }

        private (bool, int) SetChecked(MenuItem menuItem)
        {
            bool isChecked = menuItem.IsChecked;
            MenuItem parent = menuItem.Parent as MenuItem;
            foreach (MenuItem item in parent.Items)
            {
                item.IsChecked = false;
            }

            menuItem.IsChecked = isChecked;
            return (isChecked, parent.Items.IndexOf(menuItem));
        }

        private void PluginHandle(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu parent = menuItem.Parent as ContextMenu;
            int idx = parent.Items.IndexOf(menuItem);
            PluginHandle(idx);
        }

        private void PluginHandle(int idx)
        {
            ConfigManager.Settings.PluginEnabled = new Dictionary<string, bool>();
            bool enabled = false;
            if (idx == 0)
            {
                enabled = true;
            }
            else if (idx == 1)
            {
                enabled = false;
            }

            foreach (PluginMetaData plugin in PluginManager.PluginList)
            {
                plugin.Enabled = enabled;
                ConfigManager.Settings.PluginEnabled.Add(plugin.PluginID, plugin.Enabled);
            }

            ConfigManager.Settings.PluginEnabledJson = JsonConvert.SerializeObject(ConfigManager.Settings.PluginEnabled);
            vieModel.SetServers();
            vieModel.RefreshCurrentPlugins();
        }

        private void ShowAuthorInfo(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.CurrentPlugin == null || vieModel.CurrentPlugin.Authors == null ||
                vieModel.CurrentPlugin.Authors.Count <= 0)
                return;
            Border border = sender as Border;
            if (border == null) return;
            string name = border.Tag.ToString();
            if (string.IsNullOrEmpty(name)) return;
            AuthorInfo authorInfo = vieModel.CurrentPlugin.Authors.Where(arg => arg.Name.Equals(name)).FirstOrDefault();
            if (authorInfo == null) return;
            authorInfoItemsControl.ItemsSource = null;
            authorInfoItemsControl.ItemsSource = authorInfo.Infos;
            authorInfoPopup.PlacementTarget = border;
            authorInfoPopup.IsOpen = true;
        }

        private void OpenAuthorUrl(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            if (textBlock == null) return;
            string url = textBlock.Text;
            if (string.IsNullOrEmpty(url)) return;
            if (url.IsProperUrl())
                FileHelper.TryOpenUrl(url);
        }

        private void ShowUploadHelp(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(PLUGIN_UPLOAD_HELP);
        }

        private void DeletePlugin(object sender, RoutedEventArgs e)
        {
            PluginMetaData metaData = vieModel.CurrentPlugin;
            if (metaData != null)
            {
                string name = metaData.PluginName;
                Msgbox msgbox = new Msgbox(this, $"确定删除插件 {name} ？");
                if (msgbox.ShowDialog() == false) return;

                List<string> list = JsonUtils.TryDeserializeObject<List<string>>(ConfigManager.PluginConfig.DeleteList);
                if (list == null) list = new List<string>();
                if (!list.Contains(metaData.PluginID))
                {
                    list.Add(metaData.PluginID);
                }
                ConfigManager.PluginConfig.DeleteList = JsonUtils.TrySerializeObject(list);
                ConfigManager.PluginConfig.Save();
                MessageCard.Success("该插件已添加到移除列表，重启后生效！");
            }
        }

        private void ShowContextMenu(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Border border = sender as Border;
                ContextMenu contextMenu = border.ContextMenu;
                contextMenu.PlacementTarget = border;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.IsOpen = true;
            }

            e.Handled = true;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
