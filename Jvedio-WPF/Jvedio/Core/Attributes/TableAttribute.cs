using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Attributes
{

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class TableAttribute : System.Attribute
    {
        public string TableName;

        public TableAttribute(string tableName)
        {
            this.TableName = tableName;
        }
    }
}
