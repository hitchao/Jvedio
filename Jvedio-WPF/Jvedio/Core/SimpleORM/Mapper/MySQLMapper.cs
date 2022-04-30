using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM
{
    public class MySQLMapper<T> : AbstractMapper<T>
    {
        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override int executeNonQuery(string sql)
        {
            throw new NotImplementedException();
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }

        public override object insertAndGetID(T entity)
        {
            throw new NotImplementedException();
        }

        public override bool isTableExists(string tableName)
        {
            throw new NotImplementedException();
        }

        public override bool removeDataBase(string db_name)
        {
            throw new NotImplementedException();
        }

        public override List<Dictionary<string, object>> select(IWrapper<T> wrapper)
        {
            throw new NotImplementedException();
        }

        public override List<Dictionary<string, object>> select(string sql)
        {
            throw new NotImplementedException();
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

        public override long selectCount(string sql)
        {
            throw new NotImplementedException();
        }

        public override string selectLastInsertRowId()
        {
            throw new NotImplementedException();
        }

        public override List<T> selectList(IWrapper<T> wrapper = null)
        {
            throw new NotImplementedException();
        }

        public override T selectOne(IWrapper<T> wrapper = null)
        {
            throw new NotImplementedException();
        }
    }
}
