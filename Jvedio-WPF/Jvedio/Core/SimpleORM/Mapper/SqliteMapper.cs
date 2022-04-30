using Jvedio.Core.Enums;
using Jvedio.Utils.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM
{
    public class SqliteMapper<T> : AbstractMapper<T>
    {
        // todo 排它锁下的读问题：database is locked
        // 需要一个连接池
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
            string sql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            cmd.CommandText = sql;
            Log.Info(sql);
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
            try
            {
                return cmd.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
        }

        public override string selectLastInsertRowId()
        {
            string sql = "SELECT last_insert_rowid()";
            cmd.CommandText = sql;
            Log.Info(sql);
            using (SQLiteDataReader sr = cmd.ExecuteReader())
            {
                while (sr.Read())
                {
                    return sr[0].ToString();
                }
            }
            return null;
        }


        private string generateSelectSqlByWrapper(IWrapper<T> wrapper)
        {
            if (wrapper == null) return null;
            return wrapper.toSelect() + $" from {TableName} " + wrapper.toWhere() + wrapper.toOrder();
        }

        public override T selectOne(IWrapper<T> wrapper = null)
        {
            if (TableName == null) return default(T);
            Dictionary<string, object> dict = new Dictionary<string, object>();

            string sql = generateSelectSqlByWrapper(wrapper);
            if (string.IsNullOrEmpty(sql)) sql = $"select * from {TableName}";
            cmd.CommandText = sql;
            Log.Info(sql);
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    dict = Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue);
                    break;
                }
            }
            List<T> result = toEntity<T>(new List<Dictionary<string, object>>() { { dict } }, null);
            if (result == null || result.Count == 0) return default(T);
            return result[0];
        }

        public override List<T> selectList(IWrapper<T> wrapper = null)
        {
            if (TableName == null) return null;
            return toEntity<T>(select(wrapper), null);
        }

        public override List<Dictionary<string, object>> select(IWrapper<T> wrapper)
        {
            if (TableName == null) return null;
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            string sql = generateSelectSqlByWrapper(wrapper);
            if (string.IsNullOrEmpty(sql)) sql = $"select * from {TableName}";
            return select(sql);
        }

        // todo 数据库排它锁
        public override List<Dictionary<string, object>> select(string sql)
        {
            if (TableName == null) return null;
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            using (SQLiteCommand cmd = new SQLiteCommand(sql))
            {
                cmd.Connection = cn;
                cmd.CommandText = sql;
                Log.Info(sql);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Dictionary<string, object> values = Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue);
                        list.Add(values);
                    }
                }
            }


            return list;
        }



        public override T selectById(IWrapper<T> wrapper)
        {
            if (TableName == null) return default(T);
            List<T> list = toEntity<T>(select(wrapper), null);
            if (list == null || list.Count == 0) return default(T);
            return list[0];
        }

        public override List<T> selectByDict(Dictionary<string, object> dict, IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override long selectCount(IWrapper<T> wrapper)
        {
            string sql = $"select count(*) from {TableName} ";

            if (wrapper != null) sql += wrapper.toWhere(); ;



            return selectCount(sql);
        }




        public override bool removeDataBase(string db_name)
        {
            this.Dispose();
            return FileHelper.TryDeleteFile(db_name);
        }

        public override long selectCount(string sql)
        {
            long count = 0L;
            // 新建一个 SQLiteCommand
            using (SQLiteCommand cmd = new SQLiteCommand(sql))
            {
                cmd.Connection = cn;

                Log.Info(sql);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        count = reader.GetInt64(0);
                    }
                }
            }
            return count;
        }

        public override object insertAndGetID(T entity)
        {
            insert(entity);
            return entity.GetType().GetProperty(PrimaryKey.Name).GetValue(entity);
        }
    }
}
