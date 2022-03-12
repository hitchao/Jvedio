using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM
{
    public class BaseMapper<T> : AbstractMapper<T>
    {

        static SqliteMapper<T> SqliteMapper { get; set; }
        static MySQLMapper<T> MySQLMapper { get; set; }


        public override void Dispose()
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                SqliteMapper.Dispose();
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                MySQLMapper.Dispose();
            }
        }

        public override int executeNonQuery(string sql)
        {

            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.executeNonQuery(sql);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.executeNonQuery(sql);
            }
            return -1;
        }

        public override void Init()
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                if (SqliteMapper == null) SqliteMapper = new SqliteMapper<T>(GlobalVariable.DEFAULT_SQLITE_PATH);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                if (MySQLMapper == null) MySQLMapper = new MySQLMapper<T>();
            }
        }

        public void InitSqlite(string sqlitePath)
        {
            if (SqliteMapper == null) SqliteMapper = new SqliteMapper<T>(sqlitePath);
        }


        public override bool isTableExists(string tableName)
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.isTableExists(tableName);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.isTableExists(tableName);
            }
            return false;
        }

        public override List<T> selectByDict(Dictionary<string, object> dict, IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override T selectById(IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override long selectCount(IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override string selectLastInsertRowId()
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.selectLastInsertRowId();
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.selectLastInsertRowId();
            }
            return null;
        }

        public override List<T> selectList(IWrapper<T> wrapper = null)
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.selectList(wrapper);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.selectList(wrapper);
            }
            return null;
        }

        public override bool removeDataBase(string db_name)
        {
            throw new NotImplementedException();
        }

        public override T selectOne(IWrapper<T> wrapper = null)
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.selectOne(wrapper);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.selectOne(wrapper);
            }
            return default(T);
        }
    }
}
