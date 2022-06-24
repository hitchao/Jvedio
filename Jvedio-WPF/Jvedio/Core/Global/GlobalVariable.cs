
using Jvedio.Core.Enums;
using Jvedio.Entity.CommonSQL;
using Jvedio.Utils.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Jvedio
{

    // todo 低耦合
    public static class GlobalVariable
    {
        static GlobalVariable()
        {
            Init();

        }

        // *************** 网址 ***************
        public static readonly string ReleaseUrl = "https://github.com/hitchao/Jvedio/releases";
        public static readonly string UpgradeSource = "https://hitchao.github.io";
        public static readonly string UpdateUrl = "https://hitchao.github.io/jvedioupdate/latest.json";
        //public static readonly string UpdateUrl = "https://hitchao.github.io:444/jvedioupdate/latest.json";   // 旧版本
        //public static readonly string UpdateUrl = "https://hitchao.github.io/jvedioupdate/Version";           // 旧版本
        public static readonly string UpdateExeVersionUrl = "https://hitchao.github.io/jvedioupdate/update";
        public static readonly string UpdateExeUrl = "https://hitchao.github.io/jvedioupdate/JvedioUpdate.exe";
        //public static readonly string NoticeUrl = "https://hitchao.github.io/JvedioWebPage/notice";           // 旧版本
        public static readonly string NoticeUrl = "https://hitchao.github.io/jvedioupdate/notice.json";
        public static readonly string FeedBackUrl = "https://github.com/hitchao/Jvedio/issues";
        public static readonly string WikiUrl = "https://github.com/hitchao/Jvedio/wiki/02_Beginning";
        public static readonly string WebPageUrl = "https://hitchao.github.io/JvedioWebPage/";
        public static readonly string ThemeDIY = "https://hitchao.github.io/JvedioWebPage/theme.html";
        public static readonly string PLUGIN_LIST_URL = "https://hitchao.github.io/Jvedio-Plugin/pluginlist.json";
        public static readonly string PLUGIN_LIST_BASE_URL = "https://hitchao.github.io/Jvedio-Plugin/";
        public static readonly string FFMPEG_URL = "https://www.gyan.dev/ffmpeg/builds/";
        //public static readonly string FFMPEG_URL = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z";



        [Obsolete]
        public static readonly string YoudaoUrl = "https://github.com/hitchao/Jvedio/wiki";
        [Obsolete]
        public static readonly string BaiduUrl = "https://github.com/hitchao/Jvedio/wiki";


        // *************** 目录 ***************
        public static string CurrentUserFolder { get; set; }
        public static string oldDataPath { get; set; }
        public static string AllOldDataPath { get; set; }


        public static string BackupPath { get; set; }
        public static string LogPath { get; set; }
        public static string PicPath { get; set; }
        public static string BasePicPath { get; set; }
        public static string ProjectImagePath { get; set; }
        public static string TranslateDataBasePath { get; set; }
        public static string BasePluginsPath { get; set; }
        public static string ScanConfigPath { get; set; }
        public static string ServersConfigPath { get; set; }

        public static string UserConfigPath { get; set; }

        public static string[] PicPaths { get; set; }
        public static string[] InitDirs { get; set; }

        // *************** 目录 ***************



        // *************** 数据库***************
        /*
         * 如果是 sqlite => xxx.sqlite ；如果是 Mysql/PostgreSql => 数据库名称：xxx
         * 使用 SQLITE 存储用户的配置，用户的数据可以采用多数据库形式
         * DB_TABLENAME_JVEDIO_DATA ,对于 SQLITE 来说是文件名，对于 Mysql 来说是库名
         */
        public static string DB_TABLENAME_APP_CONFIG { get; set; }
        public static string DB_TABLENAME_APP_DATAS { get; set; }
        public static string DEFAULT_SQLITE_PATH { get; set; }
        public static string DEFAULT_SQLITE_CONFIG_PATH { get; set; }

        public static DataBaseType CurrentDataBaseType { get; set; }

        public static bool ClickGoBackToStartUp { get; set; }//是否是点击了返回去到 Startup

        // *************** 数据库***************



        public static string SupportVideoFormat { get; set; }
        public static string SupportPictureFormat { get; set; }        //bmp,gif,ico,jpe,jpeg,jpg,png
        public static bool DataBaseBusy { get; set; }


        public static string[] FontExt { get; set; }
        public static DataType CurrentDataType { get; set; }


        /* 分隔符 */
        public static char Separator { get; set; }
        public static string DEFAULT_NULL_STRING { get; set; }

        //禁止的文件名符号 https://docs.microsoft.com/zh-cn/previous-versions/s6feh8zw(v=vs.110)?redirectedfrom=MSDN
        public static char[] BANFILECHAR { get; set; }

        // 标签戳，全局缓存，避免每次都查询
        public static List<TagStamp> TagStamps { get; set; }

        //如果包含以下文本，则显示对应的标签戳
        public static string[] TagStrings_HD { get; set; }
        public static string[] TagStrings_Translated { get; set; }


        //默认图片
        public static BitmapSource BackgroundImage { get; set; }
        public static BitmapImage DefaultSmallImage { get; set; }
        public static BitmapImage DefaultBigImage { get; set; }
        public static BitmapImage DefaultActorImage { get; set; }

        public static TimeSpan FadeInterval { get; set; }

        public static FontFamily GlobalFont { get; set; }



        #region "热键"
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public const int HOTKEY_ID = 2415;
        public static uint VK;
        public static IntPtr _windowHandle;
        public static HwndSource _source;
        public static bool WindowsVisible = true;
        public static List<string> OpeningWindows = new List<string>();
        public static List<Key> funcKeys = new List<Key>();     // 功能键 [1,3] 个
        public static Key key = Key.None;                       // 基础键 1 个
        public static List<Key> _funcKeys = new List<Key>();
        public static Key _key = Key.None;

        public enum Modifiers
        {
            None = 0x0000,
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Win = 0x0008
        }

        public static bool IsProperFuncKey(List<Key> keyList)
        {
            bool result = true;
            List<Key> keys = new List<Key>() { Key.LeftCtrl, Key.LeftAlt, Key.LeftShift };

            foreach (Key item in keyList)
            {
                if (!keys.Contains(item))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        #endregion


        public static void LoadBgImage()
        {
            //设置背景
            GlobalVariable.BackgroundImage = null;
            if (Properties.Settings.Default.EnableBgImage)
            {
                string path = Properties.Settings.Default.BackgroundImage;
                if (!File.Exists(path)) path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background.jpg");
                GC.Collect();

                if (File.Exists(path))
                    GlobalVariable.BackgroundImage = ImageHelper.BitmapImageFromFile(path);

            }
        }

        public static void Init()
        {
            CurrentUserFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Environment.UserName);
            try
            {
                Directory.CreateDirectory(CurrentUserFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                CurrentUserFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                try
                {
                    Directory.CreateDirectory(CurrentUserFolder);
                }
                catch (Exception ex2)
                {
                    MessageBox.Show("数据目录创建失败 => " + ex2.Message);
                    App.Current.Shutdown();
                }
            }

            AllOldDataPath = Path.Combine(CurrentUserFolder, "olddata");
            BackupPath = Path.Combine(CurrentUserFolder, "backup");
            LogPath = Path.Combine(CurrentUserFolder, "log");
            PicPath = Path.Combine(CurrentUserFolder, "pic");
            ProjectImagePath = Path.Combine(CurrentUserFolder, "image", "library");
            TranslateDataBasePath = Path.Combine(CurrentUserFolder, "Translate.sqlite");
            ScanConfigPath = Path.Combine(CurrentUserFolder, "ScanPathConfig.xml");
            ServersConfigPath = Path.Combine(CurrentUserFolder, "ServersConfigPath.xml");
            UserConfigPath = Path.Combine(CurrentUserFolder, "user-config.xml");
            DB_TABLENAME_APP_CONFIG = Path.Combine(CurrentUserFolder, "app_configs");
            DB_TABLENAME_APP_DATAS = Path.Combine(CurrentUserFolder, "app_datas");
            DEFAULT_SQLITE_PATH = Path.Combine(CurrentUserFolder, DB_TABLENAME_APP_DATAS + ".sqlite");
            DEFAULT_SQLITE_CONFIG_PATH = Path.Combine(CurrentUserFolder, DB_TABLENAME_APP_CONFIG + ".sqlite");


            BasePluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

            //初始化文件夹
            InitDirs = new[] { BackupPath, LogPath, PicPath, ProjectImagePath, AllOldDataPath, Path.Combine(BasePluginsPath, "themes"), Path.Combine(BasePluginsPath, "crawlers") };
            oldDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase");// Jvedio 5.0 之前的
            BasePicPath = "";
            PicPaths = new[] { "ScreenShot", "SmallPic", "BigPic", "ExtraPic", "Actresses", "Gif" };

            CurrentDataBaseType = DataBaseType.SQLite;
            ClickGoBackToStartUp = false;
            SupportVideoFormat = $"{Jvedio.Language.Resources.NormalVedio}(*.avi, *.mp4, *.mkv, *.mpg, *.rmvb)| *.avi; *.mp4; *.mkv; *.mpg; *.rmvb|{Jvedio.Language.Resources.OtherVedio}((*.rm, *.mov, *.mpeg, *.flv, *.wmv, *.m4v)| *.rm; *.mov; *.mpeg; *.flv; *.wmv; *.m4v|{Jvedio.Language.Resources.AllFile} (*.*)|*.*";
            SupportPictureFormat = $"图片(*.bmp, *.jpe, *.jpeg, *.jpg, *.png)|*.bmp;*.jpe;*.jpeg;*.jpg;*.png";
            DataBaseBusy = false;
            BANFILECHAR = new char[] { '\\', '#', '%', '&', '*', '|', ':', '"', '<', '>', '?', '/', '.' };
            CurrentDataType = DataType.Video;
            DefaultSmallImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_S.png", UriKind.Relative));
            DefaultBigImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
            DefaultActorImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_A.png", UriKind.Relative));
            Separator = (char)007;
            DEFAULT_NULL_STRING = "NULL";
            FadeInterval = TimeSpan.FromMilliseconds(150);//淡入淡出时间
            FontExt = new[] { ".otf", ".ttf" };
            TagStamps = new List<TagStamp>();
            TagStrings_HD = new string[] { "hd", "高清" };
            TagStrings_Translated = new string[] { "中文", "日本語", "Translated", "English" };

            InitProperties();
        }

        public static void InitProperties()
        {
            //每页数目
            Properties.Settings.Default.OnlyShowSubSection = false;
        }


    }
}
