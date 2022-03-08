using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM
{
    public class SelectWrapper<T> : IWrapper<T>
    {


        /// <summary>
        /// 为空的时候默认为 select * 
        /// </summary>
        protected HashSet<string> SelectColumns { get; set; }

        public SelectWrapper()
        {
            SelectColumns = new HashSet<string>();
        }




        public IWrapper<T> Asc()
        {
            throw new NotImplementedException();
        }

        public IWrapper<T> Desc()
        {
            throw new NotImplementedException();
        }

        public IWrapper<T> GroupBy(string column)
        {
            throw new NotImplementedException();
        }

        public IWrapper<T> Limit(long offset, long row_count)
        {
            throw new NotImplementedException();
        }

        public IWrapper<T> Limit(long row_count)
        {
            throw new NotImplementedException();
        }

        public IWrapper<T> Select(params string[] columns)
        {
            foreach (var item in columns)
            {
                if (item == "*")
                {
                    SelectColumns.Clear();
                    break;
                }
                else
                {
                    SelectColumns.Add(item);
                }
            }
            return this;
        }

        public IWrapper<T> Where(string field, object value)
        {
            throw new NotImplementedException();
        }
    }
}
