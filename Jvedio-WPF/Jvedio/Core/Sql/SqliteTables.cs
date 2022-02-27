using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Sql
{
    public static class SqliteTables
    {
        public static class AppData
        {
            public static Dictionary<string, string> TABLES = new Dictionary<string, string>();
            static AppData()
            {
                TABLES.Add("app_databases", "BEGIN; create table app_databases ( DBId INTEGER PRIMARY KEY autoincrement, Path TEXT DEFAULT '', Name VARCHAR(500), Size INTEGER DEFAULT 0, Count INTEGER DEFAULT 0, DataType INT DEFAULT 0, ImagePath TEXT DEFAULT '', ViewCount INT DEFAULT 0,  CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), unique(DataType, Name), unique(Path) ); CREATE INDEX name_idx ON app_databases (Name); CREATE INDEX type_idx ON app_databases (DataType); COMMIT;");
                TABLES.Add("app_configs", "BEGIN; create table app_configs ( ConfigId INTEGER PRIMARY KEY autoincrement, ConfigName VARCHAR(100), ConfigValue TEXT DEFAULT '', CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); CREATE INDEX app_configs_name_idx ON app_configs (ConfigName); COMMIT;");
                TABLES.Add("common_ai_face", "BEGIN; create table common_ai_face ( AIId INTEGER PRIMARY KEY autoincrement, Age INT DEFAULT 0, Beauty FLOAT DEFAULT 0, Expression VARCHAR(100), FaceShape VARCHAR(100), Gender INT DEFAULT 0, Glasses INT DEFAULT 0, Race VARCHAR(100), Emotion VARCHAR(100), Mask INT DEFAULT 0, Platform VARCHAR(100), ExtraInfo TEXT, CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); COMMIT;");
                TABLES.Add("common_images", "create table common_images( ImageID INTEGER PRIMARY KEY autoincrement,  Name VARCHAR(500), Path VARCHAR(1000), PathType INT DEFAULT 0, Ext VARCHAR(100), Size INTEGER, Height INT, Width INT,  Url TEXT, ExtraInfo TEXT, Source VARCHAR(100),  CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),  unique(PathType,Path) );");
                TABLES.Add("common_transaltions", "create table common_transaltions( TransaltionID INTEGER PRIMARY KEY autoincrement,  SourceLang VARCHAR(100), TargetLang VARCHAR(100), SourceText TEXT, TargetText TEXT, Platform VARCHAR(100),  CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) );");
                TABLES.Add("common_magnets", "BEGIN; create table common_magnets ( MagnetID INTEGER PRIMARY KEY autoincrement, MagnetLink VARCHAR(40), TorrentUrl VARCHAR(2000), DataID INTEGER, Title TEXT, Size INTEGER DEFAULT 0, Releasedate VARCHAR(10) DEFAULT '1900-01-01', Tag TEXT, DownloadNumber INT DEFAULT 0, ExtraInfo TEXT, CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), unique(MagnetLink) ); CREATE INDEX common_magnets_idx_DataID ON common_magnets (DataID); COMMIT;");
                TABLES.Add("common_url_code", "BEGIN; create table common_url_code ( CodeId INTEGER PRIMARY KEY autoincrement, LocalValue VARCHAR(500), ValueType  VARCHAR(20) DEFAULT 'video', RemoteValue VARCHAR(100), WebType VARCHAR(100), CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); CREATE INDEX common_url_code_idx_VID ON common_url_code (ValueType,WebType,LocalValue); COMMIT;");
            }
        }

        public static class Actor
        {
            public static Dictionary<string, string> TABLES = new Dictionary<string, string>();
            static Actor()
            {
                TABLES.Add("actor_info", "BEGIN; create table actor_info( ActorID INTEGER PRIMARY KEY autoincrement, ActorName VARCHAR(500), NameFlag INT DEFAULT 0, Country VARCHAR(500), Nation VARCHAR(500), BirthPlace VARCHAR(500), Birthday VARCHAR(100), BloodType VARCHAR(100), Height INT, Weight INT, Gender INT DEFAULT 0, Hobby VARCHAR(500), Cup VARCHAR(1) DEFAULT '0', Chest INT, Waist INT, Hipline INT, WebType  VARCHAR(100), WebUrl  VARCHAR(2000), ImagePath TEXT, ExtraInfo TEXT, CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), unique(ActorName,NameFlag) ); CREATE INDEX actor_info_name_idx ON actor_info (ActorName,NameFlag); COMMIT;");
                TABLES.Add("actor_name_to_metadatas", "BEGIN; create table actor_name_to_metadatas( ID INTEGER PRIMARY KEY autoincrement, ActorName VARCHAR(500), NameFlag INT DEFAULT 0, DataID INT, unique(ActorName,NameFlag,DataID) ); CREATE INDEX actor_name_to_metadatas_idx_Name ON actor_name_to_metadatas (ActorName,NameFlag); CREATE INDEX actor_name_to_metadatas_idx_DataID ON actor_name_to_metadatas (DataID); COMMIT;");
                TABLES.Add("actor_alias", "BEGIN; create table actor_alias( ID INTEGER PRIMARY KEY autoincrement, ActorName VARCHAR(500), NameFlag INT DEFAULT 0, AliasName VARCHAR(500), AliasNameFlag INT DEFAULT 0, Descriptions TEXT, unique(ActorName,NameFlag,AliasName,AliasNameFlag) ); CREATE INDEX actor_alias_name_idx ON actor_alias (ActorName,NameFlag); COMMIT;");
                TABLES.Add("actor_info_images", "create table actor_info_images( ID INTEGER PRIMARY KEY autoincrement, ActorName VARCHAR(500), NameFlag INT DEFAULT 0, ImageID INTEGER, ExtraInfo TEXT, unique( ActorName , NameFlag , ImageID ) );");
            }
        }

        public static class Data
        {
            public static Dictionary<string, string> TABLES = new Dictionary<string, string>();
            static Data()
            {
                TABLES.Add("metadata", "BEGIN; create table if not exists metadata ( DataID INTEGER PRIMARY KEY autoincrement, Title TEXT, Size  INTEGER DEFAULT 0, Path TEXT, Hash VARCHAR(32), Country VARCHAR(50), ReleaseDate VARCHAR(30) DEFAULT '1900-01-01', ReleaseYear INT DEFAULT 1900, ViewCount INT DEFAULT 0, DataType INT DEFAULT 0, Rating FLOAT DEFAULT 0.0, RatingCount INT DEFAULT 0, FavoriteCount INT DEFAULT 0, Genre TEXT, Tag TEXT, Grade FLOAT DEFAULT 0.0, Label TEXT, ViewDate VARCHAR(30), FirstScanDate VARCHAR(30), LastScanDate VARCHAR(30), CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); CREATE INDEX metadata_idx_ReleaseDate ON metadata (ReleaseDate); CREATE INDEX metadata_idx_DataType ON metadata (DataType); CREATE INDEX metadata_idx_Hash ON metadata (Hash); CREATE INDEX metadata_idx_ViewDate ON metadata (ViewDate); CREATE INDEX metadata_idx_FirstScanDate ON metadata (FirstScanDate); CREATE INDEX metadata_idx_LastScanDate ON metadata (LastScanDate); COMMIT;");
                TABLES.Add("metadata_relevance", "BEGIN; create table metadata_relevance( Id INTEGER PRIMARY KEY autoincrement, RelevanceType INT DEFAULT 0, DataID INTEGER, TargetDataID INTEGER ); CREATE INDEX metadata_relevance_idx_DataID ON metadata_relevance (DataID,RelevanceType); COMMIT;");
                TABLES.Add("metadata_video", "BEGIN; create table metadata_video( DataID INTEGER PRIMARY KEY autoincrement, VID VARCHAR(500), VideoType INT DEFAULT 0, Director VARCHAR(100), Studio TEXT, Publisher TEXT, Plot TEXT, Outline TEXT, Duration INT DEFAULT 0, ImageUrls TEXT DEFAULT '', PreviewImagePaths TEXT, ScreenShotPaths TEXT, GifImagePath TEXT, BigImagePath TEXT, SmallImagePath TEXT, WebType  VARCHAR(100), WebUrl  VARCHAR(2000), ExtraInfo TEXT, unique(DataID,VID) ); CREATE INDEX metadata_video_idx_VID ON metadata_video (VID); CREATE INDEX metadata_video_idx_VideoType ON metadata_video (VideoType); COMMIT;");
                TABLES.Add("metadata_to_image", "BEGIN; create table metadata_to_image( id INTEGER PRIMARY KEY autoincrement, DataID INTEGER, ImageID INTEGER, ImageType INTEGER ); CREATE INDEX metadata_to_image_idx_DataID ON metadata_to_image (DataID); COMMIT;");
                TABLES.Add("metadata_to_translation", "BEGIN; create table metadata_to_translation( ID INTEGER PRIMARY KEY autoincrement, FieldType VARCHAR(100), TransaltionID INT ); CREATE INDEX metadata_to_translation_idx_ID_FieldType ON metadata_to_translation (ID,FieldType); COMMIT;");
            }
        }


    }
}
