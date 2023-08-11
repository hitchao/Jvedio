using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.App;

namespace Jvedio.Core.Media
{
    /// <summary>
    /// 预览图展示效果
    /// <para>参考：<see href="http://www.codescratcher.com/wpf/create-image-slideshow-wpf/#DownloadPopup">codescratcher</see></para>
    /// </summary>
    public class ImageSlide
    {

        #region "属性"
        private static string[] ValidImageExtensions { get; set; } = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

        private int CurrentSourceIndex { get; set; }
        private int CurrentCtrlIndex { get; set; }
        private int IntervalTimer { get; set; } = 2;

        private Image[] ImageControls { get; set; }
        private DispatcherTimer timerImageChange { get; set; }
        private List<ImageSource> Images { get; set; }
        private string strImagePath { get; set; } = string.Empty;

        private int MaxViewNum { get; set; } = 10; // 最多展示的图片数量
        private bool stop { get; set; }



        #endregion


        /// <summary>
        /// 主界面的预览图展示
        /// </summary>
        /// <param name="imagepath">要展示的图片的路径</param>
        /// <param name="image1">图片控件 1</param>
        /// <param name="image2">图片控件 2</param>
        public ImageSlide(string imagepath, Image image1, Image image2)
        {
            strImagePath = imagepath;
            ImageControls = new[] { image1, image2 };
            LoadImageFolder(strImagePath, 1); // 仅随机载入一张
            image1.Source = Images[0];
            timerImageChange = new DispatcherTimer();
            timerImageChange.Interval = new TimeSpan(0, 0, 0);
            timerImageChange.Tick += new EventHandler(timerImageChange_Tick);
        }

        public void Start()
        {
            stop = false;
            timerImageChange.Start();
            timerImageChange.Interval = new TimeSpan(0, 0, 0);
        }

        public void Stop()
        {
            stop = true;
            timerImageChange.Stop();
        }

        public void LoadAllImage()
        {
            LoadImageFolder(strImagePath, MaxViewNum);
        }

        private void LoadImageFolder(string folder, int number)
        {
            Images = null;
            GC.Collect();
            Images = new List<ImageSource>();
            if (Directory.Exists(folder)) {
                var sources = from file in new System.IO.DirectoryInfo(folder).GetFiles().AsParallel().Take(number)
                              where ValidImageExtensions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase)
                              orderby file.FullName
                              select CreateImageSource(file.FullName, true);
                Images.AddRange(sources);
            }

            if (Images.Count == 0)
                Images.Add(MetaData.DefaultBigImage);
        }

        private ImageSource CreateImageSource(string file, bool forcePreLoad)
        {
            if (forcePreLoad) {
                var src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(file, UriKind.Absolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                src.Freeze();
                return src;
            } else {
                var src = new BitmapImage(new Uri(file, UriKind.Absolute));
                src.Freeze();
                return src;
            }
        }

        private void timerImageChange_Tick(object sender, EventArgs e)
        {
            if (!stop)
                PlaySlideShow();

            if (timerImageChange.Interval == TimeSpan.FromSeconds(0))
                timerImageChange.Interval = TimeSpan.FromSeconds(IntervalTimer);
        }

        public void PlaySlideShow()
        {
            try {
                if (Images.Count <= 1)
                    return;
                var oldCtrlIndex = CurrentCtrlIndex;
                CurrentCtrlIndex = (CurrentCtrlIndex + 1) % 2;
                CurrentSourceIndex = (CurrentSourceIndex + 1) % Images.Count;

                Image imgFadeOut = ImageControls[oldCtrlIndex];
                Image imgFadeIn = ImageControls[CurrentCtrlIndex];
                ImageSource newSource = Images[CurrentSourceIndex];
                imgFadeIn.Source = newSource;
                Storyboard fadeOut = new Storyboard();
                DoubleAnimation FadeOutAnimation = new DoubleAnimation() {
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                };
                Storyboard.SetTargetProperty(FadeOutAnimation, new PropertyPath("Opacity"));
                fadeOut.Children.Add(FadeOutAnimation);
                fadeOut.Begin(imgFadeOut);

                Storyboard fadeIn = new Storyboard();
                DoubleAnimation FadeInAnimation = new DoubleAnimation() {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.25)),
                };
                Storyboard.SetTargetProperty(FadeInAnimation, new PropertyPath("Opacity"));
                fadeIn.Children.Add(FadeInAnimation);
                fadeIn.Begin(imgFadeIn);
                if (stop) {
                    Images = null;
                    GC.Collect();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }
    }
}
