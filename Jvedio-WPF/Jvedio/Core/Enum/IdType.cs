using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Enum
{
    public enum IdType
    {
        /**
         * 数据库ID自增
         */
        AUTO,
        /**
         * 该类型为未设置主键类型
         */
        NONE,
        /**
         * 用户输入ID
         * 该类型可以通过自己注册自动填充插件进行填充
         */
        INPUT,

        /* 以下3种类型、只有当插入对象ID 为空，才自动填充。 */
        /**
         * 全局唯一ID (idWorker)
         */
        ID_WORKER,
        /**
         * 全局唯一ID (UUID)
         */
        UUID,
        /**
         * 字符串全局唯一ID (idWorker 的字符串表示)
         */
        ID_WORKER_STR
    }
}
