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
                TABLES.Add("common_images", "create table common_images( ImageID INTEGER PRIMARY KEY autoincrement,  Name VARCHAR(500), Path VARCHAR(1000), PathType INT DEFAULT 0, Ext VARCHAR(100), Size INTEGER, Height INT, Width INT,  Url TEXT, ExtraInfo TEXT, Source VARCHAR(100),  CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),  unique(PathType,Path) );");
                TABLES.Add("common_transaltions", "create table common_transaltions( TransaltionID INTEGER PRIMARY KEY autoincrement,  SourceLang VARCHAR(100), TargetLang VARCHAR(100), SourceText TEXT, TargetText TEXT, Platform VARCHAR(100),  CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) );");
                TABLES.Add("common_magnets", "BEGIN; create table common_magnets ( MagnetID INTEGER PRIMARY KEY autoincrement, Magnet VARCHAR(40), TorrentUrl VARCHAR(2000), VID VARCHAR(500), Title TEXT, Size INTEGER DEFAULT 0, Releasedate VARCHAR(10) DEFAULT '1900-01-01', Tag TEXT, DownloadNumber INT DEFAULT 0, ExtraInfo TEXT,  CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),  unique(Magnet) ); CREATE INDEX common_magnets_idx_VID ON common_magnets (VID); COMMIT;");
                TABLES.Add("common_url_code", "BEGIN; create table common_url_code ( CodeId INTEGER PRIMARY KEY autoincrement, VID VARCHAR(500), Code VARCHAR(100), WebType VARCHAR(100),  CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); CREATE INDEX common_url_code_idx_VID ON common_url_code (WebType,VID); COMMIT;");
            }
        }


    }
}
