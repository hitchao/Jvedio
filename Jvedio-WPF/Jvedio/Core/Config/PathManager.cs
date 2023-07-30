using SuperControls.Style.Windows;
using System;
using System.IO;

namespace Jvedio.Core.Global
{

    /// <summary>
    /// 目录管理（不可使用 Logger 打印日志）
    /// </summary>
    public static class PathManager
    {
        static PathManager()
        {
            Init();
        }

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

        public static string[] ReferenceDllPaths { get; set; }

        // *************** 目录 ***************
        public static void Init()
        {
            CurrentUserFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Environment.UserName);
            try {
                Directory.CreateDirectory(CurrentUserFolder);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                CurrentUserFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                try {
                    Directory.CreateDirectory(CurrentUserFolder);
                } catch (Exception ex2) {
                    new MsgBox("数据目录创建失败 => " + ex2.Message).ShowDialog();
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
            BasePluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

            // 初始化文件夹
            InitDirs = new[] { BackupPath, LogPath, PicPath, ProjectImagePath, AllOldDataPath, Path.Combine(BasePluginsPath, "themes"), Path.Combine(BasePluginsPath, "crawlers") };
            oldDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase"); // Jvedio 5.0 之前的
            BasePicPath = string.Empty;
            PicPaths = new[] { "ScreenShot", "SmallPic", "BigPic", "ExtraPic", "Actresses", "Gif" };

            ReferenceDllPaths = new string[]{
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"x64\SQLite.Interop.dll") ,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"x86\SQLite.Interop.dll")
            };
        }
    }
}
