using Jvedio.Core.pojo.data;
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
    public sealed class ConfigConnection
    {
        private readonly static Dictionary<string, string> tables =
            new Dictionary<string, string>() {
                {
                    "databases","BEGIN; create table databases ( ID INTEGER PRIMARY KEY autoincrement, Path TEXT DEFAULT '', Name VARCHAR(500), Size DOUBLE DEFAULT 0, Count INT DEFAULT 0, Type VARCHAR(20) DEFAULT 'Video', ImagePath TEXT DEFAULT '', ViewCount INT DEFAULT 0, CreateDate VARCHAR(30), unique(Type,Name), unique(Path) ); CREATE INDEX name_idx ON databases (Name); CREATE INDEX type_idx ON databases (Type); COMMIT;"
                }
            };

        // 单例模式，懒加载：https://www.cnblogs.com/leolion/p/10241822.html
        private static readonly Lazy<ConfigConnection> lazy = new Lazy<ConfigConnection>(() => new ConfigConnection());

        public static ConfigConnection Instance { get { return lazy.Value; } }

        private readonly static string SqliteConfigPath = Path.Combine(GlobalVariable.CurrentUserFolder, "config.sqlite");

        private MySqlite Sqlite;

        private ConfigConnection()
        {
            Init();
        }

        public bool UpdateSqliteInfoPath(SqliteInfo sqliteInfo)
        {
            if (Sqlite.IsTableExist("databases"))
            {
                SQLiteCommand cmd = Sqlite.cmd;

                cmd.CommandText = $"UPDATE databases SET Path=@Path,Name=@Name where ID=@ID";

                cmd.Parameters.Add("ID", DbType.Int64).Value = sqliteInfo.ID;
                cmd.Parameters.Add("Path", DbType.String).Value = sqliteInfo.Path;
                cmd.Parameters.Add("Name", DbType.String).Value = sqliteInfo.Name;
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            return false;
        }


        public bool UpdateSqliteInfoField(string field,string value,int id)
        {
            if (Sqlite.IsTableExist("databases"))
            {
                SQLiteCommand cmd = Sqlite.cmd;
                cmd.CommandText = $"UPDATE databases SET {field}='{value}' where ID={id}";
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            return false;
        }

        public bool DeleteByIds(List<int> ID)
        {
            if (Sqlite.IsTableExist("databases"))
            {
                InfoType type = GlobalVariable.CurrentInfoType;
                SQLiteCommand cmd = Sqlite.cmd;
                string id = string.Join(",", ID);
                cmd.CommandText = $"delete from databases where ID in ({id}) and type='{type.ToString()}'";
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            return false;
        }

        public void IncreaseField(string field,int id)
        {
            if (Sqlite.IsTableExist("databases"))
            {
                SQLiteCommand cmd = Sqlite.cmd;
                cmd.CommandText = $"update databases set {field}={field}+1 where ID = {id}";
                int result = cmd.ExecuteNonQuery();
            }
        }

        public bool UpdateSqliteInfo(SqliteInfo sqliteInfo)
        {
            if (Sqlite.IsTableExist("databases"))
            {
                SQLiteCommand cmd = Sqlite.cmd;

                cmd.CommandText = $"UPDATE databases SET Path=@Path,Name=@Name," +
                    $"Size=@Size,Count=@Count,Type=@Type," +
                    $"ViewCount=@ViewCount where ID=@ID";

                cmd.Parameters.Add("ID", DbType.Int64).Value = sqliteInfo.ID;
                cmd.Parameters.Add("Path", DbType.String).Value = sqliteInfo.Path;
                cmd.Parameters.Add("Name", DbType.String).Value = sqliteInfo.Name;
                cmd.Parameters.Add("Size", DbType.Double).Value = sqliteInfo.Size;
                cmd.Parameters.Add("Count", DbType.Int64).Value = sqliteInfo.Count;
                cmd.Parameters.Add("Type", DbType.String).Value = sqliteInfo.Type;
                cmd.Parameters.Add("ViewCount", DbType.Int64).Value = sqliteInfo.ViewCount;

                int result=cmd.ExecuteNonQuery();
                return result > 0;
            }
            return false;
        }
        
        public bool InsertSqliteInfo(SqliteInfo sqliteInfo)
        {
            if (Sqlite.IsTableExist("databases"))
            {
                SQLiteCommand cmd = Sqlite.cmd;

                cmd.CommandText = $"insert into databases (Count,Path,Name,Size,Type,ImagePath,ViewCount,CreateDate) " +
                    $"values (@Count,@Path,@Name,@Size,@Type,@ImagePath,@ViewCount,@CreateDate)";

                cmd.Parameters.Add("Path", DbType.String).Value = sqliteInfo.Path;
                cmd.Parameters.Add("Name", DbType.String).Value = sqliteInfo.Name;
                cmd.Parameters.Add("Size", DbType.Double).Value = sqliteInfo.Size;
                cmd.Parameters.Add("Count", DbType.Int64).Value = sqliteInfo.Count;
                cmd.Parameters.Add("Type", DbType.String).Value = sqliteInfo.Type;
                cmd.Parameters.Add("ImagePath", DbType.String).Value = sqliteInfo.ImagePath;
                cmd.Parameters.Add("ViewCount", DbType.Int64).Value = sqliteInfo.ViewCount;
                cmd.Parameters.Add("CreateDate", DbType.String).Value = sqliteInfo.CreateDate;

                int result=cmd.ExecuteNonQuery();
                return result > 0;
            }
            return false;
        }

        public List<SqliteInfo> SelectSqliteInfo()
        {
            InfoType type = GlobalVariable.CurrentInfoType;
            List<SqliteInfo> result = new List<SqliteInfo>();
            if (!Sqlite.IsTableExist("databases")) return result;
            Sqlite.cmd.CommandText = $"select * from databases where type='{type.ToString()}'";
            using (SQLiteDataReader sr = Sqlite.cmd.ExecuteReader())
            {
                while (sr.Read())
                {
                    if (sr == null || sr["ID"] == null) continue;
                    double.TryParse(sr["Size"].ToString(), out double Size);
                    int.TryParse(sr["ID"].ToString(), out int ID);
                    int.TryParse(sr["Count"].ToString(), out int Count);
                    int.TryParse(sr["ViewCount"].ToString(), out int ViewCount);

                    SqliteInfo info = new SqliteInfo()
                    {
                        ID = ID,
                        Path = sr["Path"].ToString(),
                        Name = sr["Name"].ToString(),
                        ImagePath = sr["ImagePath"].ToString(),
                        CreateDate = sr["CreateDate"].ToString(),
                        Type = type,
                        Size = Size,
                        Count = Count,
                        ViewCount = ViewCount
                    };
                    result.Add(info);
                }
            }
            return result;
        }


        private void Init()
        {
            Sqlite = new MySqlite(SqliteConfigPath);
            foreach (string key in tables.Keys)
            {
                if (!Sqlite.IsTableExist(key))
                {
                    Sqlite.CreateTable(tables[key]);
                }
            }
        }

        private void Dispose()
        {
            Sqlite?.CloseDB();
        }





    }
}
