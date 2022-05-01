using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;
using Jvedio.Core;
using Jvedio.Entity;
using Jvedio.Utils.Enums;

namespace Jvedio
{
    /// <summary>
    /// IO、文件处理
    /// </summary>
    public static class FileProcess
    {

        public static void SetSkin(string themeName)
        {
            Theme theme = ThemeLoader.loadTheme(themeName);
            Application.Current.Resources["Color_BackgroundTitle"] = theme.DisplayProperty.Title.MainBackground;
            Application.Current.Resources["Color_BackgroundMain"] = theme.DisplayProperty.Display.MainBackground;
            Application.Current.Resources["Color_BackgroundSide"] = theme.DisplayProperty.Side.MainBackground;
            Application.Current.Resources["Color_BackgroundTab"] = theme.DisplayProperty.Tools.MainBackground;
            Application.Current.Resources["Color_BackgroundSearch"] = theme.DisplayProperty.Search.MainBackground;
            Application.Current.Resources["Color_BackgroundMenu"] = theme.DisplayProperty.Menu.MainBackground;
            Application.Current.Resources["Color_ForegroundGlobal"] = theme.DisplayProperty.Global.MainForeground;
            Application.Current.Resources["Color_ForegroundSearch"] = theme.DisplayProperty.Search.MainForeground;
            Application.Current.Resources["Color_BorderBursh"] = Colors.Transparent;

            //设置字体
            GlobalFont = new FontFamily("微软雅黑");
            if (theme.Font != null)
            {
                var fonts = Fonts.GetFontFamilies(new Uri(theme.Font));
                if (fonts != null && fonts.Count >= 1) GlobalFont = fonts.First();
            }
            foreach (Window window in App.Current.Windows)
            {
                window.FontFamily = GlobalFont;
            }

            //Console.WriteLine(fonts);

            //if (theme == "黑色")
            //{
            //    Application.Current.Resources["Color_BackgroundTitle"] = (Color)ColorConverter.ConvertFromString("#22252A");
            //    Application.Current.Resources["Color_BackgroundMain"] = (Color)ColorConverter.ConvertFromString("#1B1B1F");
            //    Application.Current.Resources["Color_BackgroundSide"] = (Color)ColorConverter.ConvertFromString("#101013");
            //    Application.Current.Resources["Color_BackgroundTab"] = (Color)ColorConverter.ConvertFromString("#383838");
            //    Application.Current.Resources["Color_BackgroundSearch"] = (Color)ColorConverter.ConvertFromString("#18191B");
            //    Application.Current.Resources["Color_BackgroundMenu"] = (Color)ColorConverter.ConvertFromString("#252526");
            //    Application.Current.Resources["Color_ForegroundGlobal"] = (Color)ColorConverter.ConvertFromString("#AFAFAF");
            //    Application.Current.Resources["Color_ForegroundSearch"] = Colors.White;
            //    Application.Current.Resources["Color_BorderBursh"] = Colors.Transparent;
            //}
            //else if (theme == "白色")
            //{
            //    Application.Current.Resources["Color_BackgroundTitle"] = (Color)ColorConverter.ConvertFromString("#E2E3E5");
            //    Application.Current.Resources["Color_BackgroundMain"] = (Color)ColorConverter.ConvertFromString("#F9F9F9");
            //    Application.Current.Resources["Color_BackgroundSide"] = (Color)ColorConverter.ConvertFromString("#F2F3F4");
            //    Application.Current.Resources["Color_BackgroundTab"] = (Color)ColorConverter.ConvertFromString("#FFF5EE");
            //    Application.Current.Resources["Color_BackgroundSearch"] = (Color)ColorConverter.ConvertFromString("#D1D1D1");
            //    Application.Current.Resources["Color_BackgroundMenu"] = Colors.White;
            //    Application.Current.Resources["Color_ForegroundGlobal"] = (Color)ColorConverter.ConvertFromString("#555555");
            //    Application.Current.Resources["Color_ForegroundSearch"] = Colors.Black;
            //    Application.Current.Resources["Color_BorderBursh"] = Colors.Gray;
            //}
            //else if (theme == "蓝色")

            //{
            //    Application.Current.Resources["Color_BackgroundTitle"] = (Color)ColorConverter.ConvertFromString("#0288D1");
            //    Application.Current.Resources["Color_BackgroundMain"] = (Color)ColorConverter.ConvertFromString("#2BA2D2");
            //    Application.Current.Resources["Color_BackgroundSide"] = (Color)ColorConverter.ConvertFromString("#03A9F5");
            //    Application.Current.Resources["Color_BackgroundTab"] = (Color)ColorConverter.ConvertFromString("#0288D1");
            //    Application.Current.Resources["Color_BackgroundSearch"] = (Color)ColorConverter.ConvertFromString("#87CEEB");
            //    Application.Current.Resources["Color_BackgroundMenu"] = (Color)ColorConverter.ConvertFromString("#0288D1");
            //    Application.Current.Resources["Color_ForegroundGlobal"] = Colors.White;
            //    Application.Current.Resources["Color_ForegroundSearch"] = Colors.White;
            //    Application.Current.Resources["Color_BorderBursh"] = (Color)ColorConverter.ConvertFromString("#95DCED");

            //}


            Application.Current.Resources["BackgroundTitle"] = new SolidColorBrush((Color)Application.Current.Resources["Color_BackgroundTitle"]);
            Application.Current.Resources["BackgroundMain"] = new SolidColorBrush((Color)Application.Current.Resources["Color_BackgroundMain"]);
            Application.Current.Resources["BackgroundSide"] = new SolidColorBrush((Color)Application.Current.Resources["Color_BackgroundSide"]);
            Application.Current.Resources["BackgroundTab"] = new SolidColorBrush((Color)Application.Current.Resources["Color_BackgroundTab"]);
            Application.Current.Resources["BackgroundSearch"] = new SolidColorBrush((Color)Application.Current.Resources["Color_BackgroundSearch"]);
            Application.Current.Resources["BackgroundMenu"] = new SolidColorBrush((Color)Application.Current.Resources["Color_BackgroundMenu"]);
            Application.Current.Resources["ForegroundGlobal"] = new SolidColorBrush((Color)Application.Current.Resources["Color_ForegroundGlobal"]);
            Application.Current.Resources["ForegroundSearch"] = new SolidColorBrush((Color)Application.Current.Resources["Color_ForegroundSearch"]);
            Application.Current.Resources["BorderBursh"] = new SolidColorBrush((Color)Application.Current.Resources["Color_BorderBursh"]);



        }
        public static Window GetWindowByName(string name)
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window.GetType().Name == name) return window;
            }
            return null;
        }



        public static string Unicode2String(string unicode)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                         unicode, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }





        /// <summary>
        /// 清除最近的观看记录
        /// </summary>
        /// <param name="dateTime"></param>
        public static void ClearDateBefore(int day)
        {
            if (!File.Exists("RecentWatch")) return;
            DateTime dateTime = DateTime.Now.AddDays(day);
            RecentWatchedConfig recentWatchedConfig = new RecentWatchedConfig();
            for (int i = 1; i < 60; i++)
            {
                DateTime date = dateTime.AddDays(-1 * i);
                recentWatchedConfig.Remove(date);
            }

        }

        /// <summary>
        /// 按照指定的条件筛选影片
        /// </summary>
        /// <param name="movies"></param>
        /// <returns></returns>
        public static List<Movie> FilterMovie(List<Movie> movies)
        {
            List<Movie> result = new List<Movie>();
            if (movies == null || movies.Count == 0) return result;
            result.AddRange(movies);
            //可播放|不可播放
            if (Properties.Settings.Default.OnlyShowPlay)
            {
                foreach (var item in movies)
                {
                    if (!File.Exists((item.filepath))) result.Remove(item);
                }
            }

            //分段|不分段
            if (Properties.Settings.Default.OnlyShowSubSection)
            {
                foreach (var item in movies)
                {
                    if (item.subsectionlist.Count <= 1) result.Remove(item);
                }
            }

            //视频类型
            int.TryParse(Properties.Settings.Default.VideoType, out int vt);
            if (vt > 0)
            {
                result = result.Where(arg => arg.vediotype == vt).ToList();
            }

            result = FilterImage(result);
            return result;
        }

        /// <summary>
        /// 筛选有图、无图
        /// </summary>
        /// <param name="originMovies"></param>
        /// <returns></returns>

        private static List<Movie> FilterImage(List<Movie> originMovies)
        {
            List<Movie> result = new List<Movie>();
            if (originMovies == null || originMovies.Count == 0) return result;
            result.AddRange(originMovies);
            int.TryParse(Properties.Settings.Default.ShowViewMode, out int idx);
            ViewType ShowViewMode = (ViewType)idx;
            MyImageType ShowImageMode = MyImageType.缩略图;
            if (Properties.Settings.Default.ShowImageMode.Length == 1)
            {
                ShowImageMode = (MyImageType)(int.Parse(Properties.Settings.Default.ShowImageMode));
            }


            if (ShowViewMode == ViewType.有图)
            {
                foreach (var item in originMovies)
                {
                    if (ShowImageMode == MyImageType.缩略图)
                    {
                        if (!File.Exists(BasePicPath + $"SmallPic\\{item.id}.jpg")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.海报图)
                    {
                        if (!File.Exists(BasePicPath + $"BigPic\\{item.id}.jpg")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.动态图)
                    {
                        if (!File.Exists(BasePicPath + $"Gif\\{item.id}.gif")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.预览图)
                    {
                        if (!Directory.Exists(BasePicPath + $"ExtraPic\\{item.id}\\")) { result.Remove(item); }
                        else
                        {
                            try { if (Directory.GetFiles(BasePicPath + $"ExtraPic\\{item.id}\\", "*.*", SearchOption.TopDirectoryOnly).Count() == 0) result.Remove(item); }
                            catch { }
                        }
                    }
                }
            }
            else if (ShowViewMode == ViewType.无图)
            {
                foreach (var item in originMovies)
                {
                    if (ShowImageMode == MyImageType.缩略图)
                    {
                        if (File.Exists(BasePicPath + $"SmallPic\\{item.id}.jpg")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.海报图)
                    {
                        if (File.Exists(BasePicPath + $"BigPic\\{item.id}.jpg")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.动态图)
                    {
                        if (File.Exists(BasePicPath + $"Gif\\{item.id}.gif")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.预览图)
                    {
                        if (Directory.Exists(BasePicPath + $"ExtraPic\\{item.id}\\"))
                        {
                            try { if (Directory.GetFiles(BasePicPath + $"ExtraPic\\{item.id}\\", "*.*", SearchOption.TopDirectoryOnly).Count() > 0) result.Remove(item); }
                            catch { }
                        }
                    }
                }
            }
            return result;
        }






        public static List<string> LabelToList(string label)
        {

            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(label)) return result;
            if (label.IndexOf(' ') > 0)
            {
                foreach (var item in label.Split(' '))
                {
                    if (item.Length > 0)
                        if (!result.Contains(item)) result.Add(item);
                }
            }
            else { if (label.Length > 0 && label.IndexOf(' ') < 0) result.Add(label.Replace(" ", "")); }
            return result;
        }

        public static bool ByteArrayToFile(byte[] byteArray, string fileName, Action<string> errorCallBack)
        {
            if (byteArray == null) return false;
            try
            {
                //这里仍然会抛出异常
                //System.NullReferenceException:“未将对象引用设置到对象的实例。”
                //byteArray 是 null。
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                errorCallBack.Invoke(ex.Message);
            }
            return false;
        }



        #region "配置xml"


        public static StringCollection ReadScanPathFromConfig(string name)
        {
            return new ScanPathConfig(name).Read();
        }


        public static void SaveScanPathToConfig(string name, List<string> paths)
        {
            ScanPathConfig scanPathConfig = new ScanPathConfig(name);
            scanPathConfig.Save(paths);
        }





        public static void SaveRecentWatched()
        {
            foreach (var keyValuePair in RecentWatched)
            {
                if (keyValuePair.Key <= DateTime.Now && keyValuePair.Key >= DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays))
                {
                    if (keyValuePair.Value.Count > 0)
                    {
                        List<string> IDs = keyValuePair.Value.Where(arg => !string.IsNullOrEmpty(arg)).ToList();
                        string date = keyValuePair.Key.Date.ToString("yyyy-MM-dd");
                        RecentWatchedConfig recentWatchedConfig = new RecentWatchedConfig(date);
                        recentWatchedConfig.Save(IDs);
                    }
                }
            }
        }


        #endregion

    }
}
