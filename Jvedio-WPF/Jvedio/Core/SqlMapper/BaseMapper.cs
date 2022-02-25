using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using Jvedio.Core.Exceptions;
using Jvedio.Utils.Common;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SqlMapper
{
    public class BaseMapper<T> : IMapper<T>, IDisposable
    {

        private class Key
        {
            public string Name { get; set; }
            public Type PropertyType { get; set; }
            public IdType IdType { get; set; }

            public Key()
            {
                IdType = IdType.AUTO;
            }
        }

        protected string SqlitePath { get; set; }
        protected string TableName { get; set; }
        protected SQLiteCommand cmd;
        protected SQLiteConnection cn;

        private Key PrimaryKey = new Key();//主键

        protected PropertyInfo[] Properties;
        protected List<string> ExtraFields;

        public BaseMapper(string sqlitePath)
        {
            Init();
            SqlitePath = sqlitePath;
            cn = new SQLiteConnection("data source=" + SqlitePath);
            cn.Open();
            cmd = new SQLiteCommand();
            cmd.Connection = cn;
        }


        private void Init()
        {
            TableAttribute tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(TableAttribute));
            TableName = tableAttribute?.TableName;
            Properties = typeof(T).GetProperties();
            IEnumerable<PropertyInfo> enumerable = Properties.Where(prop => prop.IsDefined(typeof(TableIdAttribute), false));
            if (!enumerable.Any()) throw new PrimaryKeyTypeException();//必须设置主键

            // 无该字段的
            ExtraFields = new List<string>();
            IEnumerable<PropertyInfo> tableFields = Properties.Where(prop => prop.IsDefined(typeof(TableFieldAttribute), false));
            foreach (var item in tableFields)
            {
                if (!item.GetCustomAttributes(false).OfType<TableFieldAttribute>().First().exist)
                {
                    ExtraFields.Add(item.Name);
                }
            }
            PropertyInfo propertyInfo = enumerable.First();
            PrimaryKey.Name = propertyInfo.Name;
            PrimaryKey.PropertyType = propertyInfo.PropertyType;
            TableIdAttribute tableIdAttribute = propertyInfo.GetCustomAttributes(false).OfType<TableIdAttribute>().First();
            PrimaryKey.IdType = tableIdAttribute.type;
        }



        public int deleteByDict(Dictionary<string, object> dict)
        {
            throw new NotImplementedException();
        }

        public bool deleteById(object id)
        {
            string sqltext = $"delete from {TableName} where {generateWhere(id)}";
            return executeNonQuery(sqltext) > 0;
        }

        public bool deleteByIds(List<string> idList)
        {
            if (idList == null || idList.Count == 0) return true;
            if (idList.Count == 1) return deleteById(idList[0]);
            string sqltext = $"delete from {TableName} where {generateBatchWhere(idList)}";
            return executeNonQuery(sqltext) > 0;
        }





        /// <exception cref="SQLiteException">插入时产生的异常</exception>
        public bool insert(T entity)
        {
            string sqltext = generateInsertSql(entity);
            int insert = (int)executeNonQuery(sqltext);
            if (insert > 0)
            {
                string id = selectLastInsertRowId();
                if (!string.IsNullOrEmpty(id))
                {
                    if (PrimaryKey.PropertyType == typeof(long))
                    {
                        entity.GetType().GetProperty(PrimaryKey.Name).SetValue(entity, long.Parse(id));
                    }
                    else if (PrimaryKey.PropertyType == typeof(string))
                    {
                        entity.GetType().GetProperty(PrimaryKey.Name).SetValue(entity, id);
                    }
                    return true;
                }
            }
            return false;
        }

        public List<T> selectByDict(Dictionary<string, object> dict)
        {
            throw new NotImplementedException();
        }

        public T selectById(string id)
        {
            throw new NotImplementedException();
        }

        public int selectCount()
        {
            throw new NotImplementedException();
        }

        public T selectOne()
        {
            throw new NotImplementedException();
        }

        public int update(T entity)
        {
            throw new NotImplementedException();
        }


        public int updateById(T entity)
        {
            string sql = generateUpdateSql(entity);
            try { return (int)executeNonQuery(sql); }
            catch { throw; }
        }

        public bool updateFieldById(string field, string value, object id)
        {
            Type type = typeof(T).GetProperty(field).PropertyType;

            string sql = $"update {TableName} set {field}={value} where {generateWhere(id)}";
            if (type == typeof(string))
                sql = $"update {TableName} set {field}='{value}' where {generateWhere(id)}";
            return executeNonQuery(sql) > 0;
        }



        public bool increaseFieldById(string field, object id)
        {
            string sql = $"update {TableName} set {field}={field}+1 where {generateWhere(id)}";
            return executeNonQuery(sql) > 0;
        }


        public V toEntity<V>(SQLiteDataReader sr)
        {
            V reslut = System.Activator.CreateInstance<V>();
            foreach (PropertyInfo p in Properties)
            {
                string name = p.Name;
                if (ExtraFields.Contains(name)) continue;
                string value = sr[name].ToString();
                if (p.PropertyType.IsEnum)
                {
                    int count = Enum.GetNames(p.PropertyType).Length;
                    int v = -1;
                    int.TryParse(value, out v);
                    if (v < 0 || v >= count) v = 0;
                    else p.SetValue(reslut, Enum.Parse(p.PropertyType, value));

                }
                else if (p.PropertyType == typeof(int) || p.PropertyType == typeof(int?))
                {
                    int.TryParse(value, out int intValue);
                    p.SetValue(reslut, intValue);
                }
                else if (p.PropertyType == typeof(long) || p.PropertyType == typeof(long?))
                {
                    long.TryParse(value, out long Value);
                    p.SetValue(reslut, Value);
                }
                else if (p.PropertyType == typeof(float) || p.PropertyType == typeof(float?))
                {
                    float.TryParse(value, out float Value);
                    p.SetValue(reslut, Value);
                }
                else if (p.PropertyType == typeof(bool))
                {
                    p.SetValue(reslut, value == "1" ? true : false);
                }
                else if (p.PropertyType == typeof(string))
                {
                    p.SetValue(reslut, value);
                }
            }
            return reslut;
        }
        public List<T> selectAll()
        {
            List<T> result = new List<T>();
            if (TableName == null) return null;
            cmd.CommandText = $"select * from {TableName}";
            using (SQLiteDataReader sr = cmd.ExecuteReader())
            {
                while (sr.Read())
                {
                    T entity = toEntity<T>(sr);
                    result.Add(entity);
                }
            }
            return result;
        }

        public int executeNonQuery(string sql)
        {
            cmd.CommandText = sql;
            Console.WriteLine(new DateTime().ToLongTimeString() + " => " + sql);
            try { return cmd.ExecuteNonQuery(); }
            catch { throw; }
        }

        public string selectLastInsertRowId()
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

        public void createTable(string tableName, string createTableSql)
        {
            if (!isTableExists(tableName))
            {
                executeNonQuery(createTableSql);
            }
        }


        private string generateUpdateSql(T entity)
        {
            StringBuilder sql = new StringBuilder();
            object PrimaryKeyValue = "";
            for (int i = 0; i <= Properties.Length - 1; i++)
            {
                string name = Properties[i].Name;
                if (ExtraFields.Contains(name)) continue;
                if (name == PrimaryKey.Name)
                {
                    PrimaryKeyValue = Properties[i].GetValue(entity);
                    continue;
                }

                Type type = Properties[i].PropertyType;
                object value = Properties[i].GetValue(entity);
                if (value == null) continue;
                if (type.IsEnum)
                {
                    if (value == null) value = 0;
                    value = (int)value;
                    sql.Append($"{name}={value}");
                }
                else if (TypeHelper.IsNumeric(type))
                {
                    sql.Append($"{name}={value}");
                }
                else
                {
                    sql.Append($"{name}='{value}'");
                }
                sql.Append(",");

            }
            if (sql[sql.Length - 1] == ',') sql.Remove(sql.Length - 1, 1);

            return $"UPDATE {TableName} SET {sql} WHERE {generateWhere(PrimaryKeyValue)}";
        }




        private string generateWhere(object value)
        {
            string where = $" {PrimaryKey.Name}={value}";
            if (PrimaryKey.PropertyType == typeof(string))
                where = $" {PrimaryKey.Name}='{value}'";
            return where;
        }
        private string generateBatchWhere(List<string> values)
        {
            StringBuilder builder = new StringBuilder();
            if (PrimaryKey.PropertyType == typeof(string))
            {
                values.ForEach(value =>
                {
                    builder.Append($"'{value}',");
                });
            }
            else
            {
                // int/long
                values.ForEach(value =>
                {
                    builder.Append($"{value},");
                });
            }
            if (builder[builder.Length - 1] == ',') builder.Remove(builder.Length - 1, 1);
            return $" {PrimaryKey.Name} in ({builder})";
        }

        private string generateInsertSql(T entity)
        {
            StringBuilder field_sql = new StringBuilder();
            StringBuilder value_sql = new StringBuilder();
            for (int i = 0; i <= Properties.Length - 1; i++)
            {
                string name = Properties[i].Name;
                if (name == PrimaryKey.Name || ExtraFields.Contains(name)) continue;

                Type type = Properties[i].PropertyType;
                object value = Properties[i].GetValue(entity);
                if (value == null) continue;
                if (type.IsEnum)
                {
                    if (value == null) value = 0;
                    value = (int)value;
                    value_sql.Append(value);
                }
                else if (TypeHelper.IsNumeric(type))
                {
                    value_sql.Append(value);
                }
                else if (type == typeof(bool))
                {
                    value_sql.Append((bool)value ? 1 : 0);
                }
                else
                {
                    value_sql.Append($"'{value}'");
                }
                field_sql.Append(name);
                field_sql.Append(",");
                value_sql.Append(",");

            }
            if (field_sql[field_sql.Length - 1] == ',') field_sql.Remove(field_sql.Length - 1, 1);
            if (value_sql[value_sql.Length - 1] == ',') value_sql.Remove(value_sql.Length - 1, 1);
            return $"INSERT INTO {TableName} ({field_sql}) values ({value_sql})";
        }

        public bool isTableExists(string tableName)
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

        public void Dispose()
        {
            cmd?.Dispose();
            cn?.Close();
        }
    }
}
