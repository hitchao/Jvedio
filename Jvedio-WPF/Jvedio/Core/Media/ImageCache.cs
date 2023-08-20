using SuperUtils.Media;
using System;
using System.Runtime.Caching;
using System.Windows.Media.Imaging;

namespace Jvedio.Core.Media
{
    public static class ImageCache
    {
        /// <summary>
        /// 默认图片缓存时长
        /// </summary>
        public const long DEFAULT_CACHE_EXPIRATION = 10;



        private static MemoryCache _Cache { get; set; } = MemoryCache.Default;

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

            if (!ConfigManager.Settings.ImageCache)
                return img;


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
                policy.SlidingExpiration = TimeSpan.FromMinutes(ConfigManager.Settings.CacheExpiration);
                _Cache.Add(item, policy);
            }

            return true;
        }

        public static void Remove(string path)
        {
            if (_Cache.Contains(path)) {
                _Cache.Remove(path);
            }
        }

        public static void Clear()
        {
            _Cache?.Dispose();
            GC.Collect();
        }
    }
}
