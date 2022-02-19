using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SqlMapper
{
    public interface IMapper<T>
    {

        int insert(T entity);

        int deleteById(string id);

        int deleteByDict(Dictionary<string, object> dict);

        int updateById(T entity);
        int update(T entity);
        T selectById(string id);
        List<T> selectByDict(Dictionary<string, object> dict);

        T selectOne();

        int selectCount();
    }
}
