﻿using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using Jvedio.Core.Exceptions;
using Jvedio.Utils.Common;
using Jvedio.Utils.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM
{
    public abstract class AbstractMapper<T> : IDisposable, IMapper<T>
    {

        public class Key
        {
            public string Name { get; set; }
            public Type PropertyType { get; set; }
            public IdType IdType { get; set; }

            public Key()
            {
                IdType = IdType.AUTO;
            }
        }
        protected string TableName { get; set; }

        public Key PrimaryKey = new Key();//主键
        protected PropertyInfo[] Properties;
        protected List<string> ExtraFields;

        public AbstractMapper()
        {
            InitReflectionProperties();
            //Init();
        }


        #region "Select"


        public abstract string selectLastInsertRowId();

        public abstract List<T> selectList(IWrapper<T> wrapper);

        public abstract T selectById(IWrapper<T> wrapper);

        public abstract List<T> selectByDict(Dictionary<string, object> dict, IWrapper<T> wrapper);

        public abstract long selectCount(IWrapper<T> wrapper);

        #endregion


        #region "公共方法"



        public abstract void Init();
        public abstract void Dispose();

        public abstract bool isTableExists(string tableName);


        public abstract int executeNonQuery(string sql);

        #endregion



        public abstract int insert(IWrapper<T> wrapper);





        private void InitReflectionProperties()
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



        public int insertBatch(ICollection<T> collections, bool ignore = false)
        {
            if (collections == null || collections.Count == 0) return 0;
            string sqltext = generateBatchInsertSql(collections, ignore);
            return executeNonQuery(sqltext);
        }



        /// <exception cref="SQLiteException">插入时产生的异常</exception>
        public bool insert(T entity)
        {
            if (entity == null) return false;
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
            if (entity == null) return 0;
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


        protected List<V> toEntity<V>(List<Dictionary<string, object>> list, PropertyInfo[] Properties)
        {
            List<V> result = new List<V>();
            if (Properties == null) Properties = this.Properties;
            foreach (Dictionary<string, object> row in list)
            {
                V entity = System.Activator.CreateInstance<V>();
                foreach (PropertyInfo p in Properties)
                {
                    string name = p.Name;
                    if (ExtraFields.Contains(name)) continue;
                    string value = row[name] == null ? "" : row[name].ToString();
                    if (p.PropertyType.IsEnum)
                    {
                        int count = Enum.GetNames(p.PropertyType).Length;
                        int v = -1;
                        int.TryParse(value, out v);
                        if (v < 0 || v >= count) v = 0;
                        else p.SetValue(entity, Enum.Parse(p.PropertyType, value));

                    }
                    else if (p.PropertyType == typeof(int) || p.PropertyType == typeof(int?))
                    {
                        int.TryParse(value, out int intValue);
                        p.SetValue(entity, intValue);
                    }
                    else if (p.PropertyType == typeof(long) || p.PropertyType == typeof(long?))
                    {
                        long.TryParse(value, out long Value);
                        p.SetValue(entity, Value);
                    }
                    else if (p.PropertyType == typeof(float) || p.PropertyType == typeof(float?))
                    {
                        float.TryParse(value, out float Value);
                        p.SetValue(entity, Value);
                    }
                    else if (p.PropertyType == typeof(bool))
                    {
                        p.SetValue(entity, value == "1" ? true : false);
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        p.SetValue(entity, value);
                    }
                }
                result.Add(entity);
            }
            return result;
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




        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string generateWhere(object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            string where = $" {PrimaryKey.Name}={value}";
            if (PrimaryKey.PropertyType == typeof(string))
                where = $" {PrimaryKey.Name}='{SqliteHelper.format(value)}'";
            return where;

        }

        /// <summary>
        /// 生成 where xxx in (...) 语句
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string generateBatchWhere(List<string> values)
        {
            if (values == null || values.Count == 0) throw new ArgumentNullException(nameof(values));
            StringBuilder builder = new StringBuilder();
            if (PrimaryKey.PropertyType == typeof(string))
            {
                values.ForEach(value =>
                {
                    builder.Append($"'{SqliteHelper.format(value)}',");
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


        private object getValueByType(Type type, object value)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (value == null) return "''"; ;
            if (type.IsEnum)
            {
                if (value == null) value = 0;
                value = (int)value;
                return value;
            }
            else if (TypeHelper.IsNumeric(type))
            {
                return value;
            }
            else if (type == typeof(bool))
            {
                return (bool)value ? 1 : 0;
            }
            else
            {
                return $"'{SqliteHelper.format(value)}'";
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string generateBatchInsertSql(ICollection<T> collection, bool ignore = false)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            List<string> values = new List<string>();
            StringBuilder field_sql = new StringBuilder();
            int idx = 0;
            foreach (T entity in collection)
            {

                StringBuilder value_sql = new StringBuilder();
                for (int i = 0; i <= Properties.Length - 1; i++)
                {
                    string name = Properties[i].Name;
                    if (name == PrimaryKey.Name || ExtraFields.Contains(name)) continue;
                    Type type = Properties[i].PropertyType;
                    object value = Properties[i].GetValue(entity);
                    if (name == "CreateDate" || name == "UpdateDate")
                    {
                        if (value == null || string.IsNullOrEmpty(value.ToString()))
                            value_sql.Append($"'{DateTime.Now.toLocalDate()}'");
                    }
                    else
                    {
                        value_sql.Append(getValueByType(type, value));
                    }

                    value_sql.Append(",");
                    if (idx == 0)
                    {
                        field_sql.Append(name);
                        field_sql.Append(",");
                    }
                }
                idx++;
                if (field_sql[field_sql.Length - 1] == ',') field_sql.Remove(field_sql.Length - 1, 1);
                if (value_sql[value_sql.Length - 1] == ',') value_sql.Remove(value_sql.Length - 1, 1);
                values.Add(value_sql.ToString());
            }
            string all_sql = string.Join("),(", values);
            string insert = "INSERT INTO";
            if (ignore) insert = "INSERT OR IGNORE INTO";
            string result = $"{insert} {TableName} ({field_sql}) values ({all_sql})";
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string generateInsertSql(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            StringBuilder field_sql = new StringBuilder();
            StringBuilder value_sql = new StringBuilder();
            for (int i = 0; i <= Properties.Length - 1; i++)
            {
                string name = Properties[i].Name;
                if (name == PrimaryKey.Name || ExtraFields.Contains(name)) continue;
                Type type = Properties[i].PropertyType;
                object value = Properties[i].GetValue(entity);
                if (value == null) continue;
                value_sql.Append(getValueByType(type, value));
                field_sql.Append(name);
                field_sql.Append(",");
                value_sql.Append(",");

            }
            if (field_sql[field_sql.Length - 1] == ',') field_sql.Remove(field_sql.Length - 1, 1);
            if (value_sql[value_sql.Length - 1] == ',') value_sql.Remove(value_sql.Length - 1, 1);
            return $"INSERT INTO {TableName} ({field_sql}) values ({value_sql})";
        }

        public abstract bool removeDataBase(string db_name);
    }
}