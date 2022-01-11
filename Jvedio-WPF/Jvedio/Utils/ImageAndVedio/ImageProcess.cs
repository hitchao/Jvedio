using Jvedio.Utils.ImageAndVedio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;
namespace Jvedio
{
    public static class ImageProcess
    {

        public static void SetImage(ref Movie movie)
        {
            //加载图片
            BitmapImage smallimage = GetBitmapImage(movie.id, "SmallPic");
            BitmapImage bigimage =GetBitmapImage(movie.id, "BigPic");
            if (smallimage == null) smallimage = DefaultSmallImage;
            if (bigimage == null) bigimage = DefaultBigImage;
            movie.smallimage = smallimage;
            movie.bigimage = bigimage;
        }

        public static BitmapImage GetActorImage( string name)
        {
            //加载图片
            BitmapImage image = GetBitmapImage(name, "Actresses");
            if (image == null) image = DefaultActorImage;
            return image;
        }



        /// <summary>
        /// 获得屏幕额截图
        /// </summary>
        /// <returns></returns>
        public static BitmapImage GetScreenShot()
        {
            Rectangle bounds = System.Windows.Forms.Screen.GetBounds(System.Drawing.Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
                }
                return BitmapToBitmapImage(bitmap,true);
            }
        }



        public static string ImageToBase64(Bitmap bitmap, string fileFullName = "")
        {
            try
            {
                if (fileFullName != "")
                {
                    Bitmap bmp = new Bitmap(fileFullName);
                    using(MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] arr = new byte[ms.Length]; 
                        ms.Position = 0;
                        ms.Read(arr, 0, (int)ms.Length);
                        return Convert.ToBase64String(arr);
                    }
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] arr = new byte[ms.Length]; ms.Position = 0;
                        ms.Read(arr, 0, (int)ms.Length); ms.Close();
                        return Convert.ToBase64String(arr);
                    }
                }

            }
            catch
            {
                return null;
            }
        }

        public static Bitmap Base64ToBitmap(string base64)
        {
            base64 = base64.Replace("data:image/png;base64,", "").Replace("data:image/jgp;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");//将base64头部信息替换
            byte[] bytes = Convert.FromBase64String(base64);
            using (MemoryStream memStream = new MemoryStream(bytes))
            {
                return new Bitmap(Image.FromStream(memStream));
            }
        }

        public static Int32Rect GetActressRect(BitmapSource bitmapSource, Int32Rect int32Rect)
        {
            if (bitmapSource.PixelWidth > 125 && bitmapSource.PixelHeight > 125)
            {
                int width = 250;
                int y = int32Rect.Y + (int32Rect.Height / 2) - width / 2; ;
                int x = int32Rect.X + (int32Rect.Width / 2) - width / 2;
                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x + width > bitmapSource.PixelWidth) x = bitmapSource.PixelWidth - width;
                if (y + width > bitmapSource.PixelHeight) y = bitmapSource.PixelHeight - width;
                return new Int32Rect(x, y, width, width);
            }
            else
                return Int32Rect.Empty;

        }

        public static Int32Rect GetRect(BitmapSource bitmapSource, Int32Rect int32Rect)
        {
            // 150*200
            if (bitmapSource.PixelWidth >= bitmapSource.PixelHeight)
            {
                int y = 0;
                int width = (int)(0.75 * bitmapSource.PixelHeight);
                int x = int32Rect.X + (int32Rect.Width / 2) - width / 2;
                int height = bitmapSource.PixelHeight;
                if (x < 0) x = 0;
                if (x + width > bitmapSource.PixelWidth) x = bitmapSource.PixelWidth - width;
                return new Int32Rect(x, y, width, height);
            }
            else
            {
                int x = 0;
                int height = (int)(0.75 * bitmapSource.PixelWidth);
                int y = int32Rect.Y + (int32Rect.Height / 2) - height / 2;
                int width = bitmapSource.PixelWidth;
                if (y < 0) y = 0;
                if (y + height > bitmapSource.PixelHeight) x = bitmapSource.PixelHeight - height;
                return new Int32Rect(x, y, width, height);
            }

        }

        public static BitmapSource CutImage(BitmapSource bitmapSource, Int32Rect cut)
        {
            //计算Stride
            var stride = bitmapSource.Format.BitsPerPixel * cut.Width / 8;
            byte[] data = new byte[cut.Height * stride];
            bitmapSource.CopyPixels(cut, data, stride, 0);
            return BitmapSource.Create(cut.Width, cut.Height, 0, 0, PixelFormats.Bgr32, null, data, stride);
        }

        public static Bitmap ImageSourceToBitmap(ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;
            Bitmap bmp = new System.Drawing.Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
            new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride); bmp.UnlockBits(data);
            return bmp;
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap, bool isPng = false)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                if(isPng)
                    bitmap.Save(stream, ImageFormat.Png);
                else
                    bitmap.Save(stream, ImageFormat.Jpeg);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();

                return result;
            }
        }


        public static void SaveImage(string ID, byte[] ImageByte, ImageType imageType, string Url)
        {
            string FilePath;
            string ImageDir;
            if (imageType == ImageType.BigImage)
            {
                ImageDir = BasePicPath + $"BigPic\\";
                FilePath = ImageDir + $"{ID}.jpg";
            }
            else if (imageType == ImageType.ExtraImage)
            {
                ImageDir = BasePicPath + $"ExtraPic\\{ID}\\";
                FilePath = ImageDir + Path.GetFileName(new Uri(Url).LocalPath);
            }
            else if (imageType == ImageType.SmallImage)
            {
                ImageDir = BasePicPath + $"SmallPic\\";
                FilePath = ImageDir + $"{ID}.jpg";
            }
            else
            {
                ImageDir = BasePicPath + $"Actresses\\";
                FilePath = ImageDir + $"{ID}.jpg";
            }

            if (!Directory.Exists(ImageDir)) Directory.CreateDirectory(ImageDir);
           FileProcess.ByteArrayToFile(ImageByte, FilePath);
        }


        //TODO
        /// <summary>
        /// 防止图片被占用
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static BitmapImage BitmapImageFromFile(string filepath,double DecodePixelWidth=0)
        {
            try
            {
                using (var fs = new FileStream(filepath, System.IO.FileMode.Open))
                {
                    var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    //ms.Close();
                    if (DecodePixelWidth!=0) bitmap.DecodePixelWidth = (int)DecodePixelWidth;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;//加载时才会将图像载入内存，因此不需要 ms.Close();
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception e) { Logger.LogF(e); }
            return null;
        }







        public static BitmapImage GetBitmapImage(string filename, string imagetype, double DecodePixelWidth = 0)
        {
            filename = BasePicPath + $"{imagetype}\\{filename}.jpg";
            if (File.Exists(filename))
                return BitmapImageFromFile(filename, DecodePixelWidth);
            else
                return null;
        }

        public static BitmapImage GetExtraImage(string filepath)
        {
            if (File.Exists(filepath))
                return BitmapImageFromFile(filepath);
            else
                return null;
        }


    }




}
