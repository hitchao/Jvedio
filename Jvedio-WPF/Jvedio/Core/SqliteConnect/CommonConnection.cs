
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
    public class CommonConnection : Sqlite
    {
        public static new Dictionary<string, string> tables =
            new Dictionary<string, string>() {
                {
                    "common_magnets","BEGIN; create table if not exists common_magnets ( MagnetID INTEGER PRIMARY KEY autoincrement, Magnet VARCHAR(40), TorrentUrl VARCHAR(2000), UID VARCHAR(500), Title TEXT, Size DOUBLE DEFAULT 0, Releasedate VARCHAR(10) DEFAULT '1900-01-01', Tag TEXT, Downloads INT DEFAULT 0, ExtraInfo TEXT, unique(Magnet) ); CREATE INDEX common_magnets_idx_UID ON common_magnets (UID); COMMIT;"
                },{
                    "data_movie","BEGIN; create table data_movie( DataID INTEGER PRIMARY KEY autoincrement, UID VARCHAR(500), Director VARCHAR(100), Country VARCHAR(50), Studio TEXT, Publisher TEXT, Plot TEXT, Outline TEXT, Duration INT DEFAULT 0, PreviewImages TEXT, ExtraInfo TEXT, unique(DataID,UID) ); CREATE INDEX data_movie_idx_UID ON data_movie (UID); COMMIT;"
                },{
                    "data_movie_to_image","BEGIN; create table data_movie_to_image( id INTEGER PRIMARY KEY autoincrement, DataID INTEGER, ImageID INTEGER, ImageType INTEGER ); CREATE INDEX data_movie_to_image_idx_DataID ON data_movie_to_image (DataID); COMMIT;"
                },{
                    "data_to_translation","BEGIN; create table data_to_translation( ID INTEGER PRIMARY KEY autoincrement, FieldType VARCHAR(100), TransaltionID INT ); CREATE INDEX data_to_translation_idx_ID_FieldType ON data_to_translation (ID,FieldType); COMMIT;"
                },
            };

        public CommonConnection(string path) : base(path)
        {

        }


    }
}
