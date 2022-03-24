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
        long selectCount(string sql);

        List<Dictionary<string, object>> select(IWrapper<T> wrapper);
        List<Dictionary<string, object>> select(string sql);


        #endregion


        bool insert(T entity);

        int deleteById(object id);

        int deleteByDict(Dictionary<string, object> dict);

        int updateById(T entity);
        int update(T entity);
        int update(IWrapper<T> wrapper);


        bool removeDataBase(string db_name);

    }
}
