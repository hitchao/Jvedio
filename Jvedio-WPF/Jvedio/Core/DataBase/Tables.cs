using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.DataBase
{
    public static class Tables
    {

        public static class AppConfig
        {
            public static Dictionary<string, string> TABLES = new Dictionary<string, string>();
            static AppConfig()
            {
                TABLES.Add("app_configs", "BEGIN; create table app_configs ( ConfigId INTEGER PRIMARY KEY autoincrement, ConfigName VARCHAR(100), ConfigValue TEXT DEFAULT '', CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), unique(ConfigName) ); CREATE INDEX app_configs_idx_ConfigName ON app_configs (ConfigName); COMMIT;");
            }
        }
        public static class AppData
        {
            public static Dictionary<string, string> TABLES = new Dictionary<string, string>();
            static AppData()
            {
                TABLES.Add("app_databases", "BEGIN; create table app_databases ( DBId INTEGER PRIMARY KEY autoincrement, Name VARCHAR(500), Count INTEGER DEFAULT 0, DataType INT DEFAULT 0, ImagePath TEXT DEFAULT '', ViewCount INT DEFAULT 0, Hide INT DEFAULT 0, ScanPath TEXT, ExtraInfo TEXT, CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); CREATE INDEX name_idx ON app_databases (Name); CREATE INDEX type_idx ON app_databases (DataType); COMMIT;");
                TABLES.Add("common_ai_face", "BEGIN; create table common_ai_face ( AIId INTEGER PRIMARY KEY autoincrement, Age INT DEFAULT 0, Beauty FLOAT DEFAULT 0, Expression VARCHAR(100), FaceShape VARCHAR(100), Gender INT DEFAULT 0, Glasses INT DEFAULT 0, Race VARCHAR(100), Emotion VARCHAR(100), Mask INT DEFAULT 0, Platform VARCHAR(100), ExtraInfo TEXT, CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); COMMIT;");
                TABLES.Add("common_images", "create table common_images( ImageID INTEGER PRIMARY KEY autoincrement,  Name VARCHAR(500), Path VARCHAR(1000), PathType INT DEFAULT 0, Ext VARCHAR(100), Size INTEGER, Height INT, Width INT,  Url TEXT, ExtraInfo TEXT, Source VARCHAR(100),  CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),  unique(PathType,Path) );");
                TABLES.Add("common_transaltions", "create table common_transaltions( TransaltionID INTEGER PRIMARY KEY autoincrement,  SourceLang VARCHAR(100), TargetLang VARCHAR(100), SourceText TEXT, TargetText TEXT, Platform VARCHAR(100),  CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) );");
                TABLES.Add("common_magnets", "BEGIN; create table common_magnets ( MagnetID INTEGER PRIMARY KEY autoincrement, MagnetLink VARCHAR(40), TorrentUrl VARCHAR(2000), DataID INTEGER, VID INTEGER, Title TEXT, Size INTEGER DEFAULT 0, Releasedate VARCHAR(10) DEFAULT '1900-01-01', Tag TEXT, DownloadNumber INT DEFAULT 0, ExtraInfo TEXT, CreateDate VARCHAR(30) NOT NULL DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) NOT NULL DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), unique(MagnetLink) ); CREATE INDEX common_magnets_idx_DataID ON common_magnets (DataID); CREATE INDEX common_magnets_idx_VID ON common_magnets (VID); COMMIT;");
                TABLES.Add("common_url_code", "BEGIN; create table common_url_code ( CodeId INTEGER PRIMARY KEY autoincrement, LocalValue VARCHAR(500), ValueType  VARCHAR(20) DEFAULT 'video', RemoteValue VARCHAR(100), WebType VARCHAR(100), CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), unique(ValueType,WebType,LocalValue,RemoteValue) ); CREATE INDEX common_url_code_idx_ValueType_WebType_LocalValue ON common_url_code (ValueType,WebType,LocalValue); COMMIT;");
                TABLES.Add("common_search_history", "BEGIN; create table common_search_history ( id INTEGER PRIMARY KEY autoincrement, SearchValue TEXT, SearchField VARCHAR(200), ExtraInfo TEXT, CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); CREATE INDEX common_search_history_idx_SearchField ON common_search_history (SearchField); COMMIT;");
                TABLES.Add("common_tagstamp", "BEGIN; create table common_tagstamp ( TagID INTEGER PRIMARY KEY autoincrement, Foreground VARCHAR(100), Background VARCHAR(100), TagName VARCHAR(200), ExtraInfo TEXT, CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); insert into common_tagstamp(Background,Foreground,TagName) values('154,88,183,255','255,255,255,255','HD'),('154,205,50,255','255,255,255,255','Translated'); COMMIT;");
            }
        }

        public static class Actor
        {
            public static Dictionary<string, string> TABLES = new Dictionary<string, string>();
            static Actor()
            {
                TABLES.Add("actor_info", "BEGIN; create table actor_info( ActorID INTEGER PRIMARY KEY autoincrement, ActorName VARCHAR(500), Country VARCHAR(500), Nation VARCHAR(500), BirthPlace VARCHAR(500), Birthday VARCHAR(100), Age INT, BloodType VARCHAR(100), Height INT, Weight INT, Gender INT DEFAULT 0, Hobby VARCHAR(500), Cup VARCHAR(1) DEFAULT 'Z', Chest INT, Waist INT, Hipline INT, WebType  VARCHAR(100), WebUrl  VARCHAR(2000), Grade FLOAT DEFAULT 0.0, ExtraInfo TEXT, CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); CREATE INDEX actor_info_idx_ActorName ON actor_info (ActorName); COMMIT;");
            }
        }

        public static class Data
        {
            public static Dictionary<string, string> TABLES = new Dictionary<string, string>();
            static Data()
            {
                TABLES.Add("metadata", "BEGIN; create table if not exists metadata ( DataID INTEGER PRIMARY KEY autoincrement, DBId INTEGER, Title TEXT, Size  INTEGER DEFAULT 0, Path TEXT, Hash VARCHAR(32), Country VARCHAR(50), ReleaseDate VARCHAR(30), ReleaseYear INT DEFAULT 1900, ViewCount INT DEFAULT 0, DataType INT DEFAULT 0, Rating FLOAT DEFAULT 0.0, RatingCount INT DEFAULT 0, FavoriteCount INT DEFAULT 0, Genre TEXT, Grade FLOAT DEFAULT 0.0, ViewDate VARCHAR(30), FirstScanDate VARCHAR(30), LastScanDate VARCHAR(30), CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); CREATE INDEX metadata_idx_DataID ON metadata (DataID); CREATE INDEX metadata_idx_DBId_DataID ON metadata (DBId,DataType); CREATE INDEX metadata_idx_Hash ON metadata (Hash); CREATE INDEX metadata_idx_DBId_Hash ON metadata (DBId,Hash); CREATE INDEX metadata_idx_DBId_DataType_Hash ON metadata (DBId,DataType,Hash); CREATE INDEX metadata_idx_DBId_DataType_ReleaseDate ON metadata (DBId,DataType,ReleaseDate); CREATE INDEX metadata_idx_DBId_DataType_FirstScanDate ON metadata (DBId,DataType,FirstScanDate); CREATE INDEX metadata_idx_DBId_DataType_LastScanDate ON metadata (DBId,DataType,LastScanDate); CREATE INDEX metadata_idx_DBId_DataType_Grade ON metadata (DBId,DataType,Grade); CREATE INDEX metadata_idx_DBId_DataType_Size ON metadata (DBId,DataType,Size); CREATE INDEX metadata_idx_DBId_DataType_ViewDate ON metadata (DBId,DataType,ViewDate); COMMIT;");
                TABLES.Add("metadata_video", "BEGIN; create table metadata_video( MVID INTEGER PRIMARY KEY autoincrement, DataID INTEGER, VID VARCHAR(500), VideoType INT DEFAULT 0, Series TEXT, Director VARCHAR(100), Studio TEXT, Publisher TEXT, Plot TEXT, Outline TEXT, Duration INT DEFAULT 0, SubSection TEXT, ImageUrls TEXT DEFAULT '', WebType  VARCHAR(100), WebUrl  VARCHAR(2000), ExtraInfo TEXT, unique(DataID,VID) ); CREATE INDEX metadata_video_idx_DataID_VID ON metadata_video (DataID,VID); CREATE INDEX metadata_video_idx_VID ON metadata_video (VID); CREATE INDEX metadata_video_idx_VideoType ON metadata_video (VideoType); COMMIT;");
                TABLES.Add("metadata_to_translation", "BEGIN; create table metadata_to_translation( id INTEGER PRIMARY KEY autoincrement, DataID INTEGER, FieldType VARCHAR(100), TransaltionID INTEGER, unique(DataID,FieldType,TransaltionID) ); CREATE INDEX metadata_to_translation_idx_DataID_FieldType ON metadata_to_translation (DataID,FieldType); COMMIT;");
                TABLES.Add("metadata_to_tagstamp", "BEGIN; create table metadata_to_tagstamp( id INTEGER PRIMARY KEY autoincrement, DataID INTEGER, TagID INTEGER, unique(DataID,TagID) ); CREATE INDEX metadata_to_tagstamp_idx_DataID ON metadata_to_tagstamp (DataID); CREATE INDEX metadata_to_tagstamp_idx_TagID ON metadata_to_tagstamp (TagID); COMMIT;");
                TABLES.Add("metadata_to_label", "BEGIN; create table metadata_to_label( id INTEGER PRIMARY KEY autoincrement, DataID INTEGER, LabelName VARCHAR(200), unique(DataID,LabelName) ); CREATE INDEX metadata_to_label_idx_DataID ON metadata_to_label (DataID); CREATE INDEX metadata_to_label_idx_LabelName ON metadata_to_label (LabelName); COMMIT;");
                TABLES.Add("metadata_to_actor", "BEGIN;create table metadata_to_actor( ID INTEGER PRIMARY KEY autoincrement, ActorID INTEGER, DataID INT, unique(ActorID,DataID) ); CREATE INDEX metadata_to_actor_idx_ActorID ON metadata_to_actor (ActorID,DataID); CREATE INDEX metadata_to_actor_idx_DataID ON metadata_to_actor (DataID,ActorID); COMMIT;");
                TABLES.Add("metadata_picture", "BEGIN; create table metadata_picture( PID INTEGER PRIMARY KEY autoincrement, DataID INTEGER, Director VARCHAR(100), Studio TEXT, Publisher TEXT, Plot TEXT, Outline TEXT, PicCount INTEGER DEFAULT 0, PicPaths TEXT, VideoPaths TEXT, ExtraInfo TEXT, unique(DataID,PID) ); CREATE INDEX metadata_picture_idx_DataID_PID ON metadata_picture (DataID,PID); COMMIT;");
                TABLES.Add("metadata_comic", "BEGIN; create table metadata_comic( CID INTEGER PRIMARY KEY autoincrement, DataID INTEGER, Language VARCHAR(100), ComicType INT DEFAULT 0, Artist TEXT, Plot TEXT, Outline TEXT, PicCount INTEGER DEFAULT 0, PicPaths TEXT, WebType  VARCHAR(100), WebUrl  VARCHAR(2000), ExtraInfo TEXT, unique(DataID,CID) ); CREATE INDEX metadata_comic_idx_DataID_CID ON metadata_comic (DataID,CID); COMMIT;");
                TABLES.Add("metadata_game", "BEGIN; create table metadata_game( GID INTEGER PRIMARY KEY autoincrement, DataID INTEGER, Branch VARCHAR(100), OriginalPainting VARCHAR(200), VoiceActors VARCHAR(200), Play VARCHAR(200), Music VARCHAR(200), Singers VARCHAR(200), Plot TEXT, Outline TEXT, ExtraName TEXT, Studio TEXT, Publisher TEXT, WebType  VARCHAR(100), WebUrl  VARCHAR(2000), ExtraInfo TEXT, unique(DataID,GID) ); CREATE INDEX metadata_game_idx_DataID_GID ON metadata_game (DataID,GID); COMMIT;");
            }
        }


    }
}
