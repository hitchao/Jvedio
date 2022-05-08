using Jvedio.Core.DataBase;
using Jvedio.Entity.CommonSQL;
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

            // 初始化数据库连接
            appDatabaseMapper.Init();
            translationMapper.Init();
            magnetsMapper.Init();
            aIFaceMapper.Init();
            tagStampMapper.Init();
            searchHistoryMapper.Init();


            foreach (string key in Tables.AppData.TABLES.Keys)
            {
                appDatabaseMapper.createTable(key, Tables.AppData.TABLES[key]);
            }


            appConfigMapper.InitSqlite(GlobalVariable.DEFAULT_SQLITE_CONFIG_PATH);

            foreach (string key in Tables.AppConfig.TABLES.Keys)
            {
                appConfigMapper.createTable(key, Tables.AppConfig.TABLES[key]);
            }




            metaDataMapper.Init();
            videoMapper.Init();
            pictureMapper.Init();
            comicMapper.Init();
            gameMapper.Init();
            actorMapper.Init();
            urlCodeMapper.Init();
            associationMapper.Init();

            foreach (string key in Tables.Actor.TABLES.Keys)
            {
                actorMapper.createTable(key, Tables.Actor.TABLES[key]);
            }
            foreach (string key in Tables.Data.TABLES.Keys)
            {
                metaDataMapper.createTable(key, Tables.Data.TABLES[key]);
            }


            // 新增列
            foreach (string sql in Tables.SQL.SqlCommands)
            {
                try { metaDataMapper.executeNonQuery(sql); }
                catch (Exception ex) { Console.WriteLine(ex); }
            }



        }


    }
}
