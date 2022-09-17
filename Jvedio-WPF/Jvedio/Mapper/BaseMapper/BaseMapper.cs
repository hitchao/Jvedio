using Jvedio.Core.DataBase;
using Jvedio.Core.Enums;
using Jvedio.Core.Logs;
using SuperUtils.Framework.ORM.Mapper;
using SuperUtils.Framework.ORM.Wrapper;
using System;
using System.Collections.Generic;

namespace Jvedio.Mapper.BaseMapper
{
    public class BaseMapper<T> : AbstractMapper<T>
    {
        static SqliteMapper<T> SqliteMapper { get; set; }

        static MySQLMapper<T> MySQLMapper { get; set; }

        public override void Dispose()
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                SqliteMapper.Dispose();
                SqliteMapper = null;
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                MySQLMapper.Dispose();
                MySQLMapper = null;
            }
        }

        public override int ExecuteNonQuery(string sql)
        {
            try
            {
                if (Main.CurrentDataBaseType == DataBaseType.SQLite)
                {
                    return SqliteMapper.ExecuteNonQuery(sql);
                }
                else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
                {
                    return MySQLMapper.ExecuteNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return -1;
        }

        public override void Init()
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                if (SqliteMapper == null) SqliteMapper = new SqliteMapper<T>(SqlManager.DEFAULT_SQLITE_PATH);
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                if (MySQLMapper == null) MySQLMapper = new MySQLMapper<T>();
            }
        }

        public void InitSqlite(string sqlitePath)
        {
            if (SqliteMapper == null) SqliteMapper = new SqliteMapper<T>(sqlitePath);
        }

        public override bool IsTableExists(string tableName)
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                return SqliteMapper.IsTableExists(tableName);
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                return MySQLMapper.IsTableExists(tableName);
            }
            return false;
        }

        public override List<T> SelectByDict(Dictionary<string, object> dict, IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override T SelectById(IWrapper<T> wrapper)
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                return SqliteMapper.SelectById(wrapper);
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                return MySQLMapper.SelectById(wrapper);
            }
            return default(T);
        }

        public override long SelectCount(IWrapper<T> wrapper = null)
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                return SqliteMapper.SelectCount(wrapper);
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                return MySQLMapper.SelectCount(wrapper);
            }
            return 0;
        }

        public override long SelectCount(string sql)
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                return SqliteMapper.SelectCount(sql);
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                return MySQLMapper.SelectCount(sql);
            }
            return 0;
        }

        public override string SelectLastInsertRowId()
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                return SqliteMapper.SelectLastInsertRowId();
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                return MySQLMapper.SelectLastInsertRowId();
            }
            return null;
        }

        public override List<T> SelectList(IWrapper<T> wrapper = null)
        {
            try
            {
                if (Main.CurrentDataBaseType == DataBaseType.SQLite)
                {
                    return SqliteMapper.SelectList(wrapper);
                }
                else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
                {
                    return MySQLMapper.SelectList(wrapper);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        public override T SelectOne(IWrapper<T> wrapper = null)
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                return SqliteMapper.SelectOne(wrapper);
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                return MySQLMapper.SelectOne(wrapper);
            }
            return default(T);
        }

        public override List<Dictionary<string, object>> Select(IWrapper<T> wrapper)
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                return SqliteMapper.Select(wrapper);
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                return MySQLMapper.Select(wrapper);
            }
            return null;
        }

        public override List<Dictionary<string, object>> Select(string sql)
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                return SqliteMapper.Select(sql);
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                return MySQLMapper.Select(sql);
            }
            return null;
        }

        public override object InsertAndGetID(T entity)
        {
            if (Main.CurrentDataBaseType == DataBaseType.SQLite)
            {
                return SqliteMapper.InsertAndGetID(entity);
            }
            else if (Main.CurrentDataBaseType == DataBaseType.MySQL)
            {
                return MySQLMapper.InsertAndGetID(entity);
            }
            return null;
        }

        public override bool DeleteDataBase(string db_name)
        {
            throw new NotImplementedException();
        }

        public override int Update(IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }
    }
}
