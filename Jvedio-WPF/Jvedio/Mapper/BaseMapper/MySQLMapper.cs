using SuperUtils.Framework.ORM.Mapper;
using System;
using System.Collections.Generic;

namespace Jvedio.Mapper.BaseMapper
{
    internal class MySQLMapper<T> : AbstractMapper<T>
    {
        public override bool DeleteDataBase(string db_name)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery(string sql)
        {
            throw new NotImplementedException();
        }

        public override object InsertAndGetID(T entity)
        {
            throw new NotImplementedException();
        }

        public override bool IsTableExists(string tableName)
        {
            throw new NotImplementedException();
        }

        public override List<Dictionary<string, object>> Select(SuperUtils.Framework.ORM.Wrapper.IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override List<Dictionary<string, object>> Select(string sql)
        {
            throw new NotImplementedException();
        }

        public override List<T> SelectByDict(Dictionary<string, object> dict, SuperUtils.Framework.ORM.Wrapper.IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override T SelectById(SuperUtils.Framework.ORM.Wrapper.IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override long SelectCount(SuperUtils.Framework.ORM.Wrapper.IWrapper<T> wrapper = null)
        {
            throw new NotImplementedException();
        }

        public override long SelectCount(string sql)
        {
            throw new NotImplementedException();
        }

        public override string SelectLastInsertRowId()
        {
            throw new NotImplementedException();
        }

        public override List<T> SelectList(SuperUtils.Framework.ORM.Wrapper.IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override T SelectOne(SuperUtils.Framework.ORM.Wrapper.IWrapper<T> wrapper = null)
        {
            throw new NotImplementedException();
        }

        public override int Update(SuperUtils.Framework.ORM.Wrapper.IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }
    }
}
