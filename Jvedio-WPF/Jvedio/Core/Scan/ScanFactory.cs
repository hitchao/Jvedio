using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Scan
{
    public static class ScanFactory
    {
        public static ScanTask produceScanner(DataType dataType, List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            switch (dataType)
            {
                case DataType.Video:
                    return genVideoScanner(scanPaths, filePaths, fileExt);
                case DataType.Picture:
                    if (fileExt == null) fileExt = ScanTask.PICTURE_EXTENSIONS_LIST;
                    return genPicScanner(scanPaths, filePaths, fileExt);
                case DataType.Comics:
                    if (fileExt == null) fileExt = ScanTask.PICTURE_EXTENSIONS_LIST;
                    return genComicScanner(scanPaths, filePaths, fileExt);
                case DataType.Game:
                    return genGameScanner(scanPaths, filePaths, fileExt);
                default:
                    return genVideoScanner(scanPaths, filePaths, fileExt);
            }
        }




        private static ScanTask genPicScanner(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            return new PictureScan(scanPaths, filePaths, fileExt);
        }

        private static ScanTask genVideoScanner(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            return new ScanTask(scanPaths, filePaths, fileExt);
        }
        private static ScanTask genComicScanner(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            return new ComicScan(scanPaths, filePaths, fileExt);
        }
        private static ScanTask genGameScanner(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            return new GameScan(scanPaths, fileExt);
        }
    }
}
