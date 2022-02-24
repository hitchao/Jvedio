using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SqlMapper
{
    /// <summary>
    /// 仿 Mybatis-Plus
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMapper<T>
    {

        bool insert(T entity);

        bool deleteById(object id);

        int deleteByDict(Dictionary<string, object> dict);

        int updateById(T entity);
        int update(T entity);
        T selectById(string id);
        List<T> selectByDict(Dictionary<string, object> dict);

        T selectOne();

        int selectCount();

        List<T> selectAll();
    }
}
