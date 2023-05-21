using Jvedio.Core.DataBase;
using Jvedio.Core.DataBase.Tables;
using static Jvedio.LogManager;
using Jvedio.Mapper;
using System;

namespace Jvedio
{
    public static class MapperManager
    {
        public static AppConfigMapper appConfigMapper = new AppConfigMapper();

        public static AppDatabaseMapper appDatabaseMapper = new AppDatabaseMapper();
        public static TranslationMapper translationMapper = new TranslationMapper();
        public static MagnetsMapper magnetsMapper = new MagnetsMapper();
        public static AIFaceMapper aIFaceMapper = new AIFaceMapper();
        public static TagStampMapper tagStampMapper = new TagStampMapper();
        public static SearchHistoryMapper searchHistoryMapper = new SearchHistoryMapper();

        public static MetaDataMapper metaDataMapper = new MetaDataMapper();
        public static VideoMapper videoMapper = new VideoMapper();
        public static PictureMapper pictureMapper = new PictureMapper();
        public static ComicMapper comicMapper = new ComicMapper();
        public static GameMapper gameMapper = new GameMapper();
        public static ActorMapper actorMapper = new ActorMapper();
        public static UrlCodeMapper urlCodeMapper = new UrlCodeMapper();
        public static AssociationMapper associationMapper = new AssociationMapper();

        private static bool Initialized { get; set; }

        public static void ResetInitState()
        {
            Initialized = false;
        }

        public static void Init()
        {
            if (Initialized)
                return;

            // todo 泛型似乎无法使用多态进行反射加载

            // 初始化数据库连接
            appDatabaseMapper.Init();
            translationMapper.Init();
            magnetsMapper.Init();
            aIFaceMapper.Init();
            tagStampMapper.Init();
            searchHistoryMapper.Init();

            foreach (string key in Sqlite.AppData.TABLES.Keys)
            {
                appDatabaseMapper.CreateTable(key, Sqlite.AppData.TABLES[key]);
            }

            appConfigMapper.InitSqlite(SqlManager.DEFAULT_SQLITE_CONFIG_PATH);

            foreach (string key in Sqlite.AppConfig.TABLES.Keys)
            {
                appConfigMapper.CreateTable(key, Sqlite.AppConfig.TABLES[key]);
            }

            metaDataMapper.Init();
            videoMapper.Init();
            pictureMapper.Init();
            comicMapper.Init();
            gameMapper.Init();
            actorMapper.Init();
            urlCodeMapper.Init();
            associationMapper.Init();

            foreach (string key in Sqlite.Actor.TABLES.Keys)
            {
                actorMapper.CreateTable(key, Sqlite.Actor.TABLES[key]);
            }

            foreach (string key in Sqlite.Data.TABLES.Keys)
            {
                metaDataMapper.CreateTable(key, Sqlite.Data.TABLES[key]);
            }

            // 新增列
            foreach (string sql in Sqlite.SQL.SqlCommands)
            {
                try
                {
                    metaDataMapper.ExecuteNonQuery(sql);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            Initialized = true;
        }

        public static void Dispose()
        {
            appConfigMapper.Dispose();

            appDatabaseMapper.Dispose();
            translationMapper.Dispose();
            magnetsMapper.Dispose();
            aIFaceMapper.Dispose();
            tagStampMapper.Dispose();
            searchHistoryMapper.Dispose();

            metaDataMapper.Dispose();
            videoMapper.Dispose();
            pictureMapper.Dispose();
            comicMapper.Dispose();
            gameMapper.Dispose();
            actorMapper.Dispose();
            urlCodeMapper.Dispose();
            associationMapper.Dispose();
        }
    }
}
