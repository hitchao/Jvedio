using Jvedio.Core.Enums;
using Jvedio.Logs;
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
                SqliteMapper = null;
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                MySQLMapper.Dispose();
                MySQLMapper = null;
            }
        }

        public override int executeNonQuery(string sql, Action<Exception> callBack = null)
        {
            try
            {
                if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
                {
                    return SqliteMapper.executeNonQuery(sql);
                }
                else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
                {
                    return MySQLMapper.executeNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                callBack?.Invoke(ex);

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
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.selectById(wrapper);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.selectById(wrapper);
            }
            return default(T);
        }

        public override long selectCount(IWrapper<T> wrapper = null)
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.selectCount(wrapper);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.selectCount(wrapper);
            }
            return 0;
        }



        public override long selectCount(string sql)
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.selectCount(sql);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.selectCount(sql);
            }
            return 0;
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
            try
            {
                if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
                {
                    return SqliteMapper.selectList(wrapper);
                }
                else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
                {
                    return MySQLMapper.selectList(wrapper);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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

        public override List<Dictionary<string, object>> select(IWrapper<T> wrapper)
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.select(wrapper);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.select(wrapper);
            }
            return null;
        }

        public override List<Dictionary<string, object>> select(string sql)
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.select(sql);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.select(sql);
            }
            return null;
        }

        public override object insertAndGetID(T entity)
        {
            if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.SQLite)
            {
                return SqliteMapper.insertAndGetID(entity);
            }
            else if (GlobalVariable.CurrentDataBaseType == Enums.DataBaseType.MySQL)
            {
                return MySQLMapper.insertAndGetID(entity);
            }
            return null;
        }
    }
}
