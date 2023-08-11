using Jvedio.Core.Global;
using System.IO;
using static Jvedio.App;

namespace Jvedio.Core.DataBase
{
    public static class SqlManager
    {
        static SqlManager()
        {
            Init();
        }

        public static string DB_TABLENAME_APP_CONFIG { get; set; }

        public static string DB_TABLENAME_APP_DATAS { get; set; }

        public static string DEFAULT_SQLITE_PATH { get; set; }

        public static string DEFAULT_SQLITE_CONFIG_PATH { get; set; }

        public static bool DataBaseBusy { get; set; }

        // *************** 数据库***************
        public static void Init()
        {
            DB_TABLENAME_APP_CONFIG = Path.Combine(PathManager.CurrentUserFolder, "app_configs");
            DB_TABLENAME_APP_DATAS = Path.Combine(PathManager.CurrentUserFolder, "app_datas");
            DEFAULT_SQLITE_PATH = Path.Combine(PathManager.CurrentUserFolder, DB_TABLENAME_APP_DATAS + ".sqlite");
            DEFAULT_SQLITE_CONFIG_PATH = Path.Combine(PathManager.CurrentUserFolder, DB_TABLENAME_APP_CONFIG + ".sqlite");

            DataBaseBusy = false;

            Logger.Info("sql manager init ok");

        }
    }
}
