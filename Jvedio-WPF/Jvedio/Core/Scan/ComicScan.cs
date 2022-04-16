using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Scan
{
    public class ComicScan : PictureScan
    {
        public ComicScan(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null, DataType dataType = DataType.Picture) : base(scanPaths, filePaths, fileExt, dataType)
        {
            this.dataType = DataType.Comics;
        }
    }
}
