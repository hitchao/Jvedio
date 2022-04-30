using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM.Wrapper
{
    public class UpdateWrapper<T> : SelectWrapper<T>
    {

        public Dictionary<string, object> UpdateDict { get; set; }

        public UpdateWrapper()
        {
            UpdateDict = new Dictionary<string, object>();

        }

        public IWrapper<T> update(string field, object value)
        {

            UpdateDict.Add(field, value);
            return this;
        }


    }
}
