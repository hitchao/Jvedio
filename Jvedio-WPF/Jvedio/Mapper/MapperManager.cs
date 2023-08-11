using Jvedio.Core.DataBase;
using Jvedio.Core.DataBase.Tables;
using Jvedio.Mapper;
using System;
using static Jvedio.App;

namespace Jvedio
{
    public static class MapperManager
    {
        public static AppConfigMapper appConfigMapper { get; set; } = new AppConfigMapper();

        public static AppDatabaseMapper appDatabaseMapper { get; set; } = new AppDatabaseMapper();
        public static TranslationMapper translationMapper { get; set; } = new TranslationMapper();
        public static MagnetsMapper magnetsMapper { get; set; } = new MagnetsMapper();
        public static AIFaceMapper aIFaceMapper { get; set; } = new AIFaceMapper();
        public static TagStampMapper tagStampMapper { get; set; } = new TagStampMapper();
        public static SearchHistoryMapper searchHistoryMapper { get; set; } = new SearchHistoryMapper();

        public static MetaDataMapper metaDataMapper { get; set; } = new MetaDataMapper();
        public static VideoMapper videoMapper { get; set; } = new VideoMapper();
        public static PictureMapper pictureMapper { get; set; } = new PictureMapper();
        public static ComicMapper comicMapper { get; set; } = new ComicMapper();
        public static GameMapper gameMapper { get; set; } = new GameMapper();
        public static ActorMapper actorMapper { get; set; } = new ActorMapper();
        public static UrlCodeMapper urlCodeMapper { get; set; } = new UrlCodeMapper();
        public static AssociationMapper associationMapper { get; set; } = new AssociationMapper();

        private static bool Loaded { get; set; }

        public static void ResetInitState()
        {
            Loaded = false;
        }

        public static bool Init()
        {
            if (Loaded)
                return true;

            // todo 泛型似乎无法使用多态进行反射加载

            // 初始化数据库连接
            appDatabaseMapper.Init();
            translationMapper.Init();
            magnetsMapper.Init();
            aIFaceMapper.Init();
            tagStampMapper.Init();
            searchHistoryMapper.Init();

            foreach (string key in Sqlite.AppData.TABLES.Keys) {
                appDatabaseMapper.CreateTable(key, Sqlite.AppData.TABLES[key]);
            }

            appConfigMapper.InitSqlite(SqlManager.DEFAULT_SQLITE_CONFIG_PATH);

            foreach (string key in Sqlite.AppConfig.TABLES.Keys) {
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

            foreach (string key in Sqlite.Actor.TABLES.Keys) {
                actorMapper.CreateTable(key, Sqlite.Actor.TABLES[key]);
            }

            foreach (string key in Sqlite.Data.TABLES.Keys) {
                metaDataMapper.CreateTable(key, Sqlite.Data.TABLES[key]);
            }

            // 新增列
            foreach (string sql in Sqlite.SQL.SqlCommands) {
                try {
                    metaDataMapper.ExecuteNonQuery(sql);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }

            Loaded = true;
            Logger.Info("init mapper ok");
            return Loaded;
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

            Logger.Info("dispose mapper ok");
        }
    }
}
