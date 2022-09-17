using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{
    public class Notice
    {
        public string Date { get; set; }
        public NoticeType NoticeType { get; set; }
        public string Message { get; set; }

    }
}
