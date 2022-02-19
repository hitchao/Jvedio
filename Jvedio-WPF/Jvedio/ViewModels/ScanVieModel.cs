using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ChaoLibrary;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

namespace Jvedio.ViewModel
{
    public class ScanVieModel : ViewModelBase
    {

        public MemoryStream bitmapImage;

        public MemoryStream BitmapImage
        {
            get { return bitmapImage; }
            set
            {
                bitmapImage = value;
            }
        }


        public List<BitmapSource> imagelist;

        public List<BitmapSource> ImageList
        {
            get { return imagelist; }
            set
            {
                imagelist = value;
            }
        }



        public void DisposeImage()
        {

        }
        public System.Drawing.Bitmap byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)returnImage;
            return bitmap;
        }




        public void LoadImage()
        {
            ChaoDataBase cdb = new ChaoDataBase();
            List<byte[]> list = new List<byte[]>();
            list = cdb.SelectImage("bigpic");
            ImageList = new List<BitmapSource>();
            cdb.CloseDB();
            BitmapImage = new MemoryStream(list[0]);
            list.ForEach(arg => {
                ImageList.Add(BitmapConversion.BitmapToBitmapSource((byteArrayToImage(arg))));
                //ImageList.Add(loadBitmap(byteArrayToImage(arg)));
                //ImageList.Add(new MemoryStream(arg));
            });
        }

    }


    //public static class BitmapConversion
    //{

    //    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    //    public static extern bool DeleteObject(IntPtr hObject);

    //    public static BitmapSource BitmapToBitmapSource(Bitmap bmp)
    //    {
    //        IntPtr hBitmap = bmp.GetHbitmap();
    //        BitmapSource source;
    //        try
    //        {
    //            source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
    //               hBitmap, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
    //        }
    //        finally
    //        {
    //            DeleteObject(hBitmap);

    //        }
    //        return source;

    //    }

    //}


}
