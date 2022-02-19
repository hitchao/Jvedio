
using Jvedio.Core.pojo;
using Jvedio.Utils;
using Jvedio.Utils.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio
{
    public class VideoConnection : Sqlite
    {
        public static new Dictionary<string, string> tables =
            new Dictionary<string, string>() {
                {
                    "data","BEGIN; create table if not exists data ( DataID INTEGER PRIMARY KEY autoincrement, Title TEXT, Size  DOUBLE DEFAULT 0, Path TEXT, ReleaseDate VARCHAR(30) DEFAULT '1900-01-01', ReleaseYear INT DEFAULT 1900, ViewCount INT DEFAULT 0, DataType INT DEFAULT 0, Rating FLOAT DEFAULT 0.0, RatingCount INT DEFAULT 0, Favorited INT DEFAULT 0, Genre TEXT, Tag TEXT, Grade FLOAT DEFAULT 0.0, Label TEXT, CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')), UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')) ); CREATE INDEX data_idx_ReleaseDate ON data (ReleaseDate); COMMIT;"
                },{
                    "data_movie","BEGIN; create table data_movie( DataID INTEGER PRIMARY KEY autoincrement, UID VARCHAR(500), Director VARCHAR(100), Country VARCHAR(50), Studio TEXT, Publisher TEXT, Plot TEXT, Outline TEXT, Duration INT DEFAULT 0, PreviewImages TEXT, ExtraInfo TEXT, unique(DataID,UID) ); CREATE INDEX data_movie_idx_UID ON data_movie (UID); COMMIT;"
                },{
                    "data_movie_to_image","BEGIN; create table data_movie_to_image( id INTEGER PRIMARY KEY autoincrement, DataID INTEGER, ImageID INTEGER, ImageType INTEGER ); CREATE INDEX data_movie_to_image_idx_DataID ON data_movie_to_image (DataID); COMMIT;"
                },{
                    "data_to_translation","BEGIN; create table data_to_translation( ID INTEGER PRIMARY KEY autoincrement, FieldType VARCHAR(100), TransaltionID INT ); CREATE INDEX data_to_translation_idx_ID_FieldType ON data_to_translation (ID,FieldType); COMMIT;"
                },
            };

        public VideoConnection(string path) : base(path)
        {

        }

        public bool insertMovie(DetailMovie movie)
        {
            return false;
        }


    }
}
