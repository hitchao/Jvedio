using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SqlMapper
{
    public class BaseMapper<T> : IMapper<T>
    {


        // 连接数据库
        private void Init()
        {

        }

        public BaseMapper()
        {
            Init();
        }




        public int deleteByDict(Dictionary<string, object> dict)
        {
            throw new NotImplementedException();
        }

        public int deleteById(string id)
        {
            throw new NotImplementedException();
        }

        public int insert(T entity)
        {
            throw new NotImplementedException();
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
    }
}
