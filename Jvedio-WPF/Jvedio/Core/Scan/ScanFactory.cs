using Jvedio.Core.Enums;
using System.Collections.Generic;

namespace Jvedio.Core.Scan
{
    public static class ScanFactory
    {
        public static ScanTask ProduceScanner(DataType dataType, List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            switch (dataType) {
                case DataType.Video:
                    return CreateVideoScanner(scanPaths, filePaths, fileExt);
                case DataType.Picture:
                    if (fileExt == null)
                        fileExt = ScanTask.PICTURE_EXTENSIONS_LIST;
                    return CreatePicScanner(scanPaths, filePaths, fileExt);
                case DataType.Comics:
                    if (fileExt == null)
                        fileExt = ScanTask.PICTURE_EXTENSIONS_LIST;
                    return CreateComicScanner(scanPaths, filePaths, fileExt);
                case DataType.Game:
                    return CreateGameScanner(scanPaths, filePaths, fileExt);
                default:
                    return CreateVideoScanner(scanPaths, filePaths, fileExt);
            }
        }

        private static ScanTask CreatePicScanner(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            return new PictureScan(scanPaths, filePaths, fileExt);
        }

        private static ScanTask CreateVideoScanner(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            return new ScanTask(scanPaths, filePaths, fileExt);
        }

        private static ScanTask CreateComicScanner(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            return new ComicScan(scanPaths, filePaths, fileExt);
        }

        private static ScanTask CreateGameScanner(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            return new GameScan(scanPaths, fileExt);
        }
    }
}
