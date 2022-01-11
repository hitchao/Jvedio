
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

namespace Jvedio
{
    public static class GlobalVariable
    {
        public delegate void ThemeChangeHandler();
        public static event ThemeChangeHandler ThemeChange;

        public static readonly string ReleaseUrl = "https://github.com/hitchao/Jvedio/releases";
        public static readonly string YoudaoUrl = "https://github.com/hitchao/Jvedio/wiki/HowToSetYoudaoTranslation";
        public static readonly string BaiduUrl = "https://github.com/hitchao/Jvedio/wiki/HowToSetBaiduAI";
        public static readonly string UpgradeSource = "https://hitchao.github.io";
        public static readonly string UpdateUrl = "https://hitchao.github.io/jvedioupdate/Version";
        public static readonly string UpdateExeVersionUrl = "https://hitchao.github.io/jvedioupdate/update";
        public static readonly string UpdateExeUrl = "https://hitchao.github.io/jvedioupdate/JvedioUpdate.exe";
        public static readonly string NoticeUrl = "https://hitchao.github.io/JvedioWebPage/notice";
        public static readonly string FeedBackUrl = "https://github.com/hitchao/Jvedio/issues";
        public static readonly string WikiUrl = "https://github.com/hitchao/Jvedio/wiki";
        public static readonly string WebPageUrl = "https://hitchao.github.io/JvedioWebPage/";
        public static readonly string ThemeDIY = "https://hitchao.github.io/JvedioWebPage/theme.html";



        public static string BasePicPath = Directory.Exists(Properties.Settings.Default.BasePicPath) ?
            Properties.Settings.Default.BasePicPath : AppDomain.CurrentDomain.BaseDirectory + "Pic\\";
        public static string[] InitDirs = new[] { "log", "DataBase", "BackUp", "Pic", "Plugins/Themes", "Plugins/Crawlers" };//初始化文件夹
        public static string[] PicPaths = new[] { "ScreenShot", "SmallPic", "BigPic", "ExtraPic", "Actresses", "Gif" };

        public static string[] FontExt = new[] { ".otf", ".ttf" };




        public static Stopwatch stopwatch = new Stopwatch();//计时

        public static string InfoDataBasePath = AppDomain.CurrentDomain.BaseDirectory + "Info.sqlite";

        //禁止的文件名符号
        //https://docs.microsoft.com/zh-cn/previous-versions/s6feh8zw(v=vs.110)?redirectedfrom=MSDN
        public static readonly char[] BANFILECHAR = { '\\', '#', '%', '&', '*', '|', ':', '"', '<', '>', '?', '/', '.' };


        public static Servers JvedioServers;
        public static Dictionary<string, string> UrlCookies;// key 网址 value 对应的 cookie

        //骑兵、步兵识别码
        public static List<string> Censored = new List<string>();
        public static List<string> Uncensored = new List<string>();




        // jav321 转换规则
        public static Dictionary<string, string> Jav321IDDict = new Dictionary<string, string>();


        //按类别中分类
        public static string[] GenreEurope = new string[8];
        public static string[] GenreCensored = new string[7];
        public static string[] GenreUncensored = new string[8];

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
        public static BitmapImage DefaultSmallImage;
        public static BitmapImage DefaultBigImage;
        public static BitmapImage DefaultActorImage;


        public static TimeSpan FadeInterval = TimeSpan.FromMilliseconds(150);//淡入淡出时间

        //AES加密秘钥
        public static string[] EncryptKeys = new string[] { "ShS69pNGvLac6ZF+", "Yv4x4beWwe+vhFwg", "+C+bPEbF5W4v3/H0" };

        public static string[] NeedCookie = new[] { "DB", "DMM", "MOO" };

        public static bool AutoAddPrefix = false;
        public static string Prefix = "FC2-";
        public static int DefaultNewMovieType = 0;

        public static string AIDataBasePath = AppDomain.CurrentDomain.BaseDirectory + "AI.sqlite";
        public static string TranslateDataBasePath = AppDomain.CurrentDomain.BaseDirectory + "Translate.sqlite";

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

        public static void InitVariable()
        {
            JvedioServers = ServerConfig.Instance.ReadAll();

            Properties.Settings.Default.Save();

            //每页数目
            //Properties.Settings.Default.DisplayNumber = 100;
            //Properties.Settings.Default.FlowNum = 20;
            //Properties.Settings.Default.ActorDisplayNum = 30;
            Properties.Settings.Default.VedioType = "0";
            Properties.Settings.Default.ShowViewMode = "0";
            Properties.Settings.Default.OnlyShowPlay = false;
            Properties.Settings.Default.OnlyShowSubSection = false;

            //添加演员分隔符
            if (!actorSplitDict.ContainsKey(0)) actorSplitDict.Add(0, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(1)) actorSplitDict.Add(1, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(2)) actorSplitDict.Add(2, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(3)) actorSplitDict.Add(3, new char[] { '/' });//欧美






            GenreEurope[0] = Resource_String.GenreEurope.Split('|')[0];
            GenreEurope[1] = Resource_String.GenreEurope.Split('|')[1];
            GenreEurope[2] = Resource_String.GenreEurope.Split('|')[2];
            GenreEurope[3] = Resource_String.GenreEurope.Split('|')[3];
            GenreEurope[4] = Resource_String.GenreEurope.Split('|')[4];
            GenreEurope[5] = Resource_String.GenreEurope.Split('|')[5];
            GenreEurope[6] = Resource_String.GenreEurope.Split('|')[6];
            GenreEurope[7] = Resource_String.GenreEurope.Split('|')[7];

            GenreCensored[0] = Resource_String.GenreCensored.Split('|')[0];
            GenreCensored[1] = Resource_String.GenreCensored.Split('|')[1];
            GenreCensored[2] = Resource_String.GenreCensored.Split('|')[2];
            GenreCensored[3] = Resource_String.GenreCensored.Split('|')[3];
            GenreCensored[4] = Resource_String.GenreCensored.Split('|')[4];
            GenreCensored[5] = Resource_String.GenreCensored.Split('|')[5];
            GenreCensored[6] = Resource_String.GenreCensored.Split('|')[6];

            GenreUncensored[0] = Resource_String.GenreUncensored.Split('|')[0];
            GenreUncensored[1] = Resource_String.GenreUncensored.Split('|')[1];
            GenreUncensored[2] = Resource_String.GenreUncensored.Split('|')[2];
            GenreUncensored[3] = Resource_String.GenreUncensored.Split('|')[3];
            GenreUncensored[4] = Resource_String.GenreUncensored.Split('|')[4];
            GenreUncensored[5] = Resource_String.GenreUncensored.Split('|')[5];
            GenreUncensored[6] = Resource_String.GenreUncensored.Split('|')[6];
            GenreUncensored[7] = Resource_String.GenreUncensored.Split('|')[7];

            //配置 ffmpeg 路径
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path) && File.Exists("ffmpeg.exe"))
            {
                Properties.Settings.Default.FFMPEG_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            }

        }







    }
}
