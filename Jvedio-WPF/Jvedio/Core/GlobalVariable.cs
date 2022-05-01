
using Jvedio.Core.Enums;
using Jvedio.Core.SimpleORM;
using Jvedio.Core.DataBase;
using Jvedio.Entity;
using Jvedio.Mapper;
using Jvedio.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jvedio.Entity.CommonSQL;

namespace Jvedio
{
    public static class GlobalVariable
    {
        static GlobalVariable()
        {
            LoadBgImage();
        }


        public delegate void ThemeChangeHandler();
        public static event ThemeChangeHandler ThemeChange;


        public const char Separator = (char)007;


        // *************** 网址 ***************
        public static readonly string ReleaseUrl = "https://github.com/hitchao/Jvedio/releases";
        public static readonly string YoudaoUrl = "https://github.com/hitchao/Jvedio/wiki/HowToSetYoudaoTranslation";
        public static readonly string BaiduUrl = "https://github.com/hitchao/Jvedio/wiki/HowToSetBaiduAI";
        public static readonly string UpgradeSource = "https://hitchao.github.io";
        public static readonly string UpdateUrl = "https://hitchao.github.io/jvedioupdate/latest.json";
        //public static readonly string UpdateUrl = "https://hitchao.github.io/jvedioupdate/Version";// 旧版本
        public static readonly string UpdateExeVersionUrl = "https://hitchao.github.io/jvedioupdate/update";
        public static readonly string UpdateExeUrl = "https://hitchao.github.io/jvedioupdate/JvedioUpdate.exe";
        //public static readonly string NoticeUrl = "https://hitchao.github.io/JvedioWebPage/notice";// 旧版本
        public static readonly string NoticeUrl = "https://hitchao.github.io/jvedioupdate/notice.json";
        public static readonly string FeedBackUrl = "https://github.com/hitchao/Jvedio/issues";
        public static readonly string WikiUrl = "https://github.com/hitchao/Jvedio/wiki";
        public static readonly string WebPageUrl = "https://hitchao.github.io/JvedioWebPage/";
        public static readonly string ThemeDIY = "https://hitchao.github.io/JvedioWebPage/theme.html";
        public static readonly string PLUGIN_LIST_URL = "https://hitchao.github.io/Jvedio-Plugin/pluginlist.json";
        public static readonly string PLUGIN_LIST_BASE_URL = "https://hitchao.github.io/Jvedio-Plugin/";




        // *************** 目录 ***************
        public static string CurrentUserFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Environment.UserName);// todo CurrentUserFolder 不一定能创建
        public static string oldDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase");// Jvedio 5.0 之前的
        public static string AllOldDataPath = Path.Combine(CurrentUserFolder, "olddata");


        public static string BackupPath = Path.Combine(CurrentUserFolder, "backup");
        public static string LogPath = Path.Combine(CurrentUserFolder, "log");
        public static string PicPath = Path.Combine(CurrentUserFolder, "pic");
        public static string BasePicPath = "";
        public static string ProjectImagePath = Path.Combine(CurrentUserFolder, "image", "library");
        public static string TranslateDataBasePath = Path.Combine(CurrentUserFolder, "Translate.sqlite");
        public static string BasePluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        public static string ScanConfigPath = Path.Combine(CurrentUserFolder, "ScanPathConfig.xml");
        public static string ServersConfigPath = Path.Combine(CurrentUserFolder, "ServersConfigPath.xml");

        public static string UserConfigPath = Path.Combine(CurrentUserFolder, "user-config.xml");

        public static string[] PicPaths = new[] { "ScreenShot", "SmallPic", "BigPic", "ExtraPic", "Actresses", "Gif" };
        public static string[] InitDirs = new[] {
            BackupPath,LogPath,PicPath,ProjectImagePath,AllOldDataPath,
           Path.Combine(BasePluginsPath,"themes"), Path.Combine(BasePluginsPath,"crawlers") };//初始化文件夹

        // *************** 目录 ***************



        // *************** 数据库***************
        /// <summary>
        /// 如果是 sqlite => xxx.sqlite ；如果是 Mysql/PostgreSql => 数据库名称：xxx
        /// 使用 SQLITE 存储用户的配置，用户的数据可以采用多数据库形式
        /// DB_TABLENAME_JVEDIO_DATA ,对于 SQLITE 来说是文件名，对于 Mysql 来说是库名
        /// </summary>
        public static readonly string DB_TABLENAME_APP_CONFIG = Path.Combine(CurrentUserFolder, "app_configs");
        public static readonly string DB_TABLENAME_APP_DATAS = Path.Combine(CurrentUserFolder, "app_datas");
        public static readonly string DEFAULT_SQLITE_PATH =
            Path.Combine(CurrentUserFolder, DB_TABLENAME_APP_DATAS + ".sqlite");
        public static readonly string DEFAULT_SQLITE_CONFIG_PATH =
            Path.Combine(CurrentUserFolder, DB_TABLENAME_APP_CONFIG + ".sqlite");

        public static DataBaseType CurrentDataBaseType = DataBaseType.SQLite;
        public static bool ClickGoBackToStartUp = false;//是否是点击了返回去到 Startup



        // *************** 数据库***************

        //bmp,gif,ico,jpe,jpeg,jpg,png
        public static string SupportVideoFormat = $"{Jvedio.Language.Resources.NormalVedio}(*.avi, *.mp4, *.mkv, *.mpg, *.rmvb)| *.avi; *.mp4; *.mkv; *.mpg; *.rmvb|{Jvedio.Language.Resources.OtherVedio}((*.rm, *.mov, *.mpeg, *.flv, *.wmv, *.m4v)| *.rm; *.mov; *.mpeg; *.flv; *.wmv; *.m4v|{Jvedio.Language.Resources.AllFile} (*.*)|*.*";
        public static string SupportPictureFormat = $"图片(*.bmp, *.jpe, *.jpeg, *.jpg, *.png)|*.bmp;*.jpe;*.jpeg;*.jpg;*.png";
        public static bool DataBaseBusy = false;



        // *************** Mapper ***************



        public static string[] FontExt = new[] { ".otf", ".ttf" };
        public static DataType CurrentDataType = DataType.Video;



        public static Stopwatch stopwatch = new Stopwatch();//计时

        //禁止的文件名符号
        //https://docs.microsoft.com/zh-cn/previous-versions/s6feh8zw(v=vs.110)?redirectedfrom=MSDN
        public static readonly char[] BANFILECHAR = { '\\', '#', '%', '&', '*', '|', ':', '"', '<', '>', '?', '/', '.' };


        public static Dictionary<string, string> UrlCookies;// key 网址 value 对应的 cookie


        public static List<string> Censored = new List<string>();
        public static List<string> Uncensored = new List<string>();


        // 标签戳，全局缓存，避免每次都查询
        public static List<TagStamp> TagStamps = new List<TagStamp>();


        //演员分隔符
        public static Dictionary<int, char[]> actorSplitDict = new Dictionary<int, char[]>();//key 分别是 123 骑兵步兵欧美

        //如果包含以下文本，则显示对应的标签戳
        public static string[] TagStrings_HD = new string[] { "hd", "高清" };
        public static string[] TagStrings_Translated = new string[] { "中文", "日本語", "Translated", "English" };
        public static string[] TagStrings_FlowOut = new string[] { "流出", "FlowOut" };

        //最近播放
        public static Dictionary<DateTime, List<string>> RecentWatched = new Dictionary<DateTime, List<string>>();

        //默认图片
        public static BitmapSource BackgroundImage;
        public static BitmapImage DefaultSmallImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_S.png", UriKind.Relative));
        public static BitmapImage DefaultBigImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
        public static BitmapImage DefaultActorImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_A.png", UriKind.Relative));



        public static TimeSpan FadeInterval = TimeSpan.FromMilliseconds(150);//淡入淡出时间

        //AES加密秘钥
        public static string[] EncryptKeys = new string[] { "ShS69pNGvLac6ZF+", "Yv4x4beWwe+vhFwg", "+C+bPEbF5W4v3/H0" };

        public static string[] NeedCookie = new[] { "DB", "DMM", "MOO" };


        public static string SubSectionFeature = "-,_,cd,hd,whole";
        public static string AIBaseImage64 = "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";



        public static FontFamily GlobalFont = null;



        #region "热键"
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public const int HOTKEY_ID = 2415;
        public static uint VK;
        public static IntPtr _windowHandle;
        public static HwndSource _source;
        public static bool IsHide = false;
        public static List<string> OpeningWindows = new List<string>();
        public static List<Key> funcKeys = new List<Key>(); //功能键 [1,3] 个
        public static Key key = Key.None;//基础键 1 个
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
                    GlobalVariable.BackgroundImage = ImageProcess.BitmapImageFromFile(path);

            }
        }

        public static void InitDataBase()
        {
            Enum.TryParse(Properties.Settings.Default.DataBaseType, true, out DataBaseType dataBaseType);
            CurrentDataBaseType = dataBaseType;
        }

        public static void InitVariable()
        {




            //每页数目
            //Properties.Settings.Default.DisplayNumber = 100;
            //Properties.Settings.Default.FlowNum = 20;
            //Properties.Settings.Default.ActorDisplayNum = 30;
            Properties.Settings.Default.VideoType = "0";
            Properties.Settings.Default.ShowViewMode = "0";
            Properties.Settings.Default.OnlyShowPlay = false;
            Properties.Settings.Default.OnlyShowSubSection = false;

            //添加演员分隔符
            if (!actorSplitDict.ContainsKey(0)) actorSplitDict.Add(0, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(1)) actorSplitDict.Add(1, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(2)) actorSplitDict.Add(2, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(3)) actorSplitDict.Add(3, new char[] { '/' });//欧美









        }







    }
}
