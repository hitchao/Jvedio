using Jvedio.Utils.Common;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM
{
    public class SqliteMapper<T> : AbstractMapper<T>
    {
        protected SQLiteCommand cmd;
        protected SQLiteConnection cn;


        protected string SqlitePath { get; set; }

        public SqliteMapper(string sqlitePath)
        {
            this.SqlitePath = sqlitePath;
            Init();
        }


        public override void Init()
        {
            cn = new SQLiteConnection("data source=" + SqlitePath);
            cn.Open();
            cmd = new SQLiteCommand();
            cmd.Connection = cn;
        }




        public override bool isTableExists(string tableName)
        {
            cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            using (SQLiteDataReader sr = cmd.ExecuteReader())
            {
                while (sr.Read())
                {
                    return (sr[0] != null && !string.IsNullOrEmpty(sr[0].ToString()));
                }
            }
            return false;
        }

        public override void Dispose()
        {
            cmd?.Dispose();
            cn?.Close();
        }

        public override int executeNonQuery(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return 0;
            cmd.CommandText = sql;
            Console.WriteLine(DateHelper.Now() + " => " + sql);
            try { return cmd.ExecuteNonQuery(); }
            catch { throw; }
        }

        public override string selectLastInsertRowId()
        {
            cmd.CommandText = "SELECT last_insert_rowid()";
            using (SQLiteDataReader sr = cmd.ExecuteReader())
            {
                while (sr.Read())
                {
                    return sr[0].ToString();
                }
            }
            return null;
        }

        public override List<T> selectList(IWrapper<T> wrapper = null)
        {
            if (TableName == null) return null;
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            cmd.CommandText = $"select * from {TableName}";
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Dictionary<string, object> values = Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue);
                    //T entity = toEntity<T>(sr);
                    list.Add(values);
                }
            }
            return toEntity<T>(list, null);
        }


        public override T selectById(IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override List<T> selectByDict(Dictionary<string, object> dict, IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override long selectCount(IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override int insert(IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override bool removeDataBase(string db_name)
        {
            this.Dispose();
            return FileHelper.TryDeleteFile(db_name);
        }
    }
}
