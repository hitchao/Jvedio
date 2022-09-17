using Jvedio.Core.Global;
using System.IO;

namespace Jvedio.Core.DataBase
{
    public static class SqlManager
    {
        static SqlManager()
        {
            Init();
        }

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

        public static bool DataBaseBusy { get; set; }

        // *************** 数据库***************

        public static void Init()
        {
            DB_TABLENAME_APP_CONFIG = Path.Combine(PathManager.CurrentUserFolder, "app_configs");
            DB_TABLENAME_APP_DATAS = Path.Combine(PathManager.CurrentUserFolder, "app_datas");
            DEFAULT_SQLITE_PATH = Path.Combine(PathManager.CurrentUserFolder, DB_TABLENAME_APP_DATAS + ".sqlite");
            DEFAULT_SQLITE_CONFIG_PATH = Path.Combine(PathManager.CurrentUserFolder, DB_TABLENAME_APP_CONFIG + ".sqlite");

            DataBaseBusy = false;
        }
    }
}
