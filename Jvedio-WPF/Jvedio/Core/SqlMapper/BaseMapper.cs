using Jvedio.Core.Attributes;
using Jvedio.Core.Enum;
using Jvedio.Core.Exceptions;
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

        protected string SqlitePath { get; set; }
        protected string TableName { get; set; }
        protected SQLiteCommand cmd;
        protected SQLiteConnection cn;

        protected string PrimaryKeyName;
        protected IdType PrimaryKeyType;
        protected PropertyInfo[] Properties;


        public BaseMapper(string sqlitePath)
        {
            Init();
            SqlitePath = sqlitePath;
            if (!File.Exists(SqlitePath))
            {
                throw new FileNotFoundException();
            }
            cn = new SQLiteConnection("data source=" + SqlitePath);
            cn.Open();
            cmd = new SQLiteCommand();
            cmd.Connection = cn;
        }


        private void Init()
        {
            TableAttribute tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(TableAttribute));
            TableName = tableAttribute?.TableName;
            Properties = typeof(T).GetProperties();
            IEnumerable<PropertyInfo> enumerable = Properties.Where(prop => prop.IsDefined(typeof(TableIdAttribute), false));
            if (!enumerable.Any()) throw new PrimaryKeyTypeException();//必须设置主键

            PropertyInfo propertyInfo = enumerable.First();
            PrimaryKeyName = propertyInfo.Name;
            TableIdAttribute tableIdAttribute = propertyInfo.GetCustomAttributes(false).OfType<TableIdAttribute>().First();
            PrimaryKeyType = tableIdAttribute.type;
        }



        public int deleteByDict(Dictionary<string, object> dict)
        {
            throw new NotImplementedException();
        }

        public int deleteById(string id)
        {
            throw new NotImplementedException();
        }



        // TODO 生成 sql -> 数值类型
        private string generateInsertSql(T entity)
        {
            StringBuilder field_sql = new StringBuilder();
            StringBuilder value_sql = new StringBuilder();
            for (int i = 0; i <= Properties.Length - 1; i++)
            {
                string name = Properties[i].Name;
                Type type = Properties[i].PropertyType;
                object value = Properties[i].GetValue(entity);
                if (name == PrimaryKeyName) continue;
                if (value == null) continue;
                field_sql.Append(name);
                value_sql.Append($"'{value}'");
                field_sql.Append(",");
                value_sql.Append(",");

            }
            if (field_sql[field_sql.Length - 1] == ',') field_sql.Remove(field_sql.Length - 1, 1);
            if (value_sql[value_sql.Length - 1] == ',') value_sql.Remove(value_sql.Length - 1, 1);
            return $"INSERT INTO {TableName} ({field_sql}) values ({value_sql})";
        }

        public int insert(T entity)
        {
            string sqltext = generateInsertSql(entity);
            cmd.CommandText = sqltext;
            return cmd.ExecuteNonQuery();
            //return -1;
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
            throw new NotImplementedException();
        }

        public V toEntity<V>(SQLiteDataReader sr)
        {
            V reslut = System.Activator.CreateInstance<V>();
            foreach (PropertyInfo propertyInfo in Properties)
            {
                string name = propertyInfo.Name;
                string value = sr[name].ToString();
                if (propertyInfo.PropertyType == typeof(int))
                {
                    int.TryParse(value, out int intValue);
                    propertyInfo.SetValue(reslut, intValue);
                }
                else if (propertyInfo.PropertyType == typeof(string))
                {
                    propertyInfo.SetValue(reslut, value);
                }
                else if (propertyInfo.PropertyType == typeof(DateTime))
                {
                    DateTime.TryParse(value, out DateTime dateTimeValue);
                    propertyInfo.SetValue(reslut, dateTimeValue);
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

        public void Dispose()
        {
            cmd?.Dispose();
            cn?.Close();
        }
    }
}
