using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM
{
    /// <summary>
    /// 仿 Mybatis-Plus
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMapper<T>
    {

        #region "Select"

        string selectLastInsertRowId();

        List<T> selectList(IWrapper<T> wrapper);

        T selectById(IWrapper<T> wrapper);

        List<T> selectByDict(Dictionary<string, object> dict, IWrapper<T> wrapper);


        long selectCount(IWrapper<T> wrapper);


        #endregion


        bool insert(T entity);

        bool deleteById(object id);

        int deleteByDict(Dictionary<string, object> dict);

        int updateById(T entity);
        int update(T entity);


        bool removeDataBase(string db_name);

    }
}
