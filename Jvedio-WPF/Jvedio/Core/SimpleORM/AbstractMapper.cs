using Jvedio.Core.Attributes;
using Jvedio.Core.DataBase;
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


        /// <summary>
        /// 注解定义 Field 为 exist=false 的列表
        /// </summary>
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

        public abstract long selectCount(IWrapper<T> wrapper = null);
        public abstract long selectCount(string sql);
        public abstract List<Dictionary<string, object>> select(IWrapper<T> wrapper);
        public abstract List<Dictionary<string, object>> select(string sql);

        #endregion


        #region "公共方法"



        public abstract void Init();
        public abstract void Dispose();

        public abstract bool isTableExists(string tableName);


        public abstract int executeNonQuery(string sql);

        #endregion



        public int insert(T entity, InsertMode mode = InsertMode.Replace)
        {
            if (entity == null) return -1;
            string sqltext = generateBatchInsertSql(new List<T> { entity }, mode);
            lock (Connection.WriteLock)
            {
                return executeNonQuery(sqltext);
            }
        }





        private void InitReflectionProperties()
        {
            TableAttribute tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(TableAttribute));
            TableName = tableAttribute?.TableName;
            // 不获得父类的属性
            Properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public
                                                | System.Reflection.BindingFlags.Instance
                                                | System.Reflection.BindingFlags.DeclaredOnly);
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

        public int deleteById(object id)
        {
            string sqltext = $"delete from {TableName} where {generateWhere(id)}";
            return executeNonQuery(sqltext);
        }

        public int deleteByIds(List<string> idList)
        {
            if (idList == null || idList.Count == 0) return 0;
            if (idList.Count == 1) return deleteById(idList[0]);
            string sqltext = $"delete from {TableName} where {generateBatchWhere(idList)}";
            return executeNonQuery(sqltext);
        }



        public int insertBatch(ICollection<T> collections, InsertMode mode = InsertMode.Replace)
        {
            if (collections == null || collections.Count == 0) return 0;
            string sqltext = generateBatchInsertSql(collections, mode);
            lock (Connection.WriteLock)
            {
                return executeNonQuery(sqltext);
            }
        }



        /// <exception cref="SQLiteException">插入时产生的异常</exception>
        public bool insert(T entity)
        {
            if (entity == null) return false;
            string sqltext = generateInsertSql(entity);
            int insert = 0;
            lock (Connection.WriteLock)
            {
                insert = (int)executeNonQuery(sqltext);
            }
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



        public abstract T selectOne(IWrapper<T> wrapper = null);

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
                sql = $"update {TableName} set {field}='{SqliteHelper.format(value)}' where {generateWhere(id)}";
            return executeNonQuery(sql) > 0;
        }

        public int updateField(string field, string value, IWrapper<T> wrapper)
        {
            Type type = typeof(T).GetProperty(field).PropertyType;

            string sql = $"update {TableName} set {field}={value} {wrapper.toWhere()}";
            if (type == typeof(string))
                sql = $"update {TableName} set {field}='{SqliteHelper.format(value)}' {wrapper.toWhere()}";
            return executeNonQuery(sql);
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <seealso cref="https://stackoverflow.com/questions/15501779/sqlite-bulk-update-statement"/>
        /// <seealso cref="https://stackoverflow.com/questions/17079697/update-command-with-case-in-sqlite"/>
        /// <param name="collections"></param>
        /// <returns></returns>
        public int updateBatch(ICollection<T> collections, params string[] updateFields)
        {
            if (collections == null || collections.Count == 0) return 0;
            string sqltext = generateBatchUpdateSql(collections, updateFields);
            return executeNonQuery(sqltext);
        }


        public bool increaseFieldById(string field, object id)
        {
            string sql = $"update {TableName} set {field}={field}+1 where {generateWhere(id)}";
            return executeNonQuery(sql) > 0;
        }


        public List<V> toEntity<V>(List<Dictionary<string, object>> list, PropertyInfo[] Properties, bool existField = true)
        {
            List<V> result = new List<V>();
            if (Properties == null) Properties = this.Properties;
            foreach (Dictionary<string, object> row in list)
            {
                V entity = System.Activator.CreateInstance<V>();
                foreach (PropertyInfo p in Properties)
                {
                    string name = p.Name;
                    if (existField && ExtraFields.Contains(name)) continue;
                    if (!row.ContainsKey(name) || row[name] == null) continue;// 为 null 
                    string value = row[name].ToString();
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
                    else if (p.PropertyType == typeof(char))
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            p.SetValue(entity, value[0]);
                        }

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
                    sql.Append($"{name}='{SqliteHelper.format(value)}'");
                }
                sql.Append(",");

            }
            if (Properties.ToList().Where(x => x.Name == "UpdateDate").Any())
            {
                sql.Append($"UpdateDate='{DateHelper.Now()}'");
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
        private string generateBatchInsertSql(ICollection<T> collection, InsertMode mode = InsertMode.Normal)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            List<string> values = new List<string>();
            List<string> field_sql = new List<string>();
            int idx = 0;
            foreach (T entity in collection)
            {

                List<object> value_sql = new List<object>();
                for (int i = 0; i <= Properties.Length - 1; i++)
                {
                    string name = Properties[i].Name;
                    if (name == PrimaryKey.Name || ExtraFields.Contains(name)) continue;
                    Type type = Properties[i].PropertyType;
                    object value = Properties[i].GetValue(entity);
                    if (name == "CreateDate" || name == "UpdateDate")
                    {
                        if (value == null || string.IsNullOrEmpty(value.ToString()))
                            value_sql.Add($"'{DateTime.Now.toLocalDate()}'");
                        else
                            value_sql.Add($"'{SqliteHelper.format(value)}'");
                    }
                    else
                    {
                        value_sql.Add(getValueByType(type, value));
                    }

                    if (idx == 0)
                    {
                        field_sql.Add(name);
                    }
                }
                idx++;
                values.Add(string.Join(",", value_sql));
            }
            string all_sql = string.Join("),(", values);
            string insert = "INSERT INTO";
            if (mode == InsertMode.Ignore) insert = "INSERT OR IGNORE INTO";
            else if (mode == InsertMode.Replace) insert = "INSERT OR REPLACE INTO";
            else if (mode == InsertMode.Update) insert = "INSERT OR UPDATE INTO";
            string result = $"{insert} {TableName} ({string.Join(",", field_sql)}) values ({all_sql})";
            return result;
        }


        private string generateBatchUpdateSql(ICollection<T> collection, params string[] updateFields)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            PropertyInfo[] updateProperties = Properties;
            if (updateFields != null || updateFields.Length > 0)
                updateProperties = Properties.Where(arg => updateFields.Contains(arg.Name)).ToArray();

            /*
             * 
             * UPDATE  metadata
                SET     
                Path = CASE DataID
                    WHEN 1 THEN 'E:\123.mp4'
                    WHEN 2 THEN 'E:\456.mp4'
                    END,
                Title = CASE DataID
                    WHEN 1 THEN '123'
                    WHEN 2 THEN '456'
                    END
                WHERE DataID IN (1, 2)
             * 
             */

            StringBuilder set_sql = new StringBuilder();
            HashSet<object> primaryKeyValues = new HashSet<object>();
            for (int i = 0; i <= updateProperties.Length - 1; i++)
            {
                string name = updateProperties[i].Name;
                if (ExtraFields.Contains(name)) continue;
                if (updateProperties[i].Name == PrimaryKey.Name
                    || updateProperties[i].Name == "UpdateDate"
                    ) continue;// 跳过主键 和 UpdateDate
                set_sql.Append($" {name} = CASE {PrimaryKey.Name} ");
                foreach (T entity in collection)
                {
                    PropertyInfo primaryKeyInfo = Properties.Where(arg => arg.Name == PrimaryKey.Name).First();
                    object PrimaryKeyValue = primaryKeyInfo.GetValue(entity);
                    primaryKeyValues.Add(PrimaryKeyValue);
                    PropertyInfo property = Properties.Where(arg => arg.Name == updateProperties[i].Name).First();

                    Type type = property.PropertyType;
                    object value = property.GetValue(entity);
                    if (value == null) continue;
                    if (type.IsEnum)
                    {
                        if (value == null) value = 0;
                        value = (int)value;
                        set_sql.Append($"WHEN {PrimaryKeyValue} THEN {value} ");
                    }
                    else if (TypeHelper.IsNumeric(type))
                    {
                        set_sql.Append($"WHEN {PrimaryKeyValue} THEN {value} ");
                    }
                    else
                    {
                        set_sql.Append($"WHEN {PrimaryKeyValue} THEN '{SqliteHelper.format(value)}' ");
                    }
                }
                set_sql.Append("END,");
            }
            if (Properties.Any(arg => arg.Name.Equals("UpdateDate")))
                set_sql.Append($"UpdateDate='{DateHelper.Now()}'");
            if (set_sql[set_sql.Length - 1] == ',') set_sql.Remove(set_sql.Length - 1, 1);
            return $"UPDATE {TableName} SET {set_sql} WHERE {PrimaryKey.Name} in ('{string.Join("','", primaryKeyValues)}')";
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

        public int update(IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public abstract object insertAndGetID(T entity);
    }
}
