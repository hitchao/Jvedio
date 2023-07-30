using SuperUtils.Media;
using System;
using System.Runtime.Caching;
using System.Windows.Media.Imaging;

namespace Jvedio.Core.Media
{
    public static class ImageCache
    {
        private static MemoryCache _Cache = MemoryCache.Default;

        static ImageCache()
        {

        }

        public static BitmapImage Get(string path, int DecodePixelWidth = 0)
        {
            //App.Logger.Debug($"memory limit[{_Cache.CacheMemoryLimit}], physical memory limit[{_Cache.PhysicalMemoryLimit}] ");
            object o = _Cache.Get(path);
            if (o != null && o is BitmapImage image)
                return image;

            // 读取该文件，加入缓存
            BitmapImage img = ImageHelper.ReadImageFromFile(path, DecodePixelWidth);
            if (img == null)
                return null;
            Add(path, img);
            return img;
        }

        private static bool Add(string path, BitmapImage image)
        {
            CacheItem item = new CacheItem(path, image);
            if (!_Cache.Contains(path)) {
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.SlidingExpiration = TimeSpan.FromSeconds(10);
                _Cache.Add(item, policy);
            }

            return true;
        }

        public static void Clear()
        {
            _Cache.Dispose();
            GC.Collect();
        }
    }
}
