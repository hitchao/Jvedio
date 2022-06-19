using Jvedio.Core.DataBase;
using Jvedio.Core.DataBase.Tables;
using Jvedio.Entity.CommonSQL;
using Jvedio.Logs;
using Jvedio.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio
{
    public static class GlobalMapper
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

        public static void Init()
        {
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
                appDatabaseMapper.createTable(key, Sqlite.AppData.TABLES[key]);
            }


            appConfigMapper.InitSqlite(GlobalVariable.DEFAULT_SQLITE_CONFIG_PATH);

            foreach (string key in Sqlite.AppConfig.TABLES.Keys)
            {
                appConfigMapper.createTable(key, Sqlite.AppConfig.TABLES[key]);
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
                actorMapper.createTable(key, Sqlite.Actor.TABLES[key]);
            }
            foreach (string key in Sqlite.Data.TABLES.Keys)
            {
                metaDataMapper.createTable(key, Sqlite.Data.TABLES[key]);
            }


            // 新增列
            foreach (string sql in Sqlite.SQL.SqlCommands)
            {
                try { metaDataMapper.executeNonQuery(sql); }
                catch (Exception ex) { Logger.Error(ex); }
            }



        }


    }
}
