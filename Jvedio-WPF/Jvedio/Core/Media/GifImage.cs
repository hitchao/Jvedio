using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Jvedio.Core.Media
{

    // todo 重构
    public class GifImage : Image
    {

        #region "属性"

        private bool _isInitialized { get; set; }
        private Int32Animation _animation { get; set; }

        private List<BitmapSource> bitmapImages { get; set; }
        private Gif gif { get; set; }

        private static bool ShowGif { get; set; }

        #endregion

        #region "DependencyProperty"
        public int FrameIndex {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }

        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));

        static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            var gifImage = obj as GifImage;
            gifImage.Source = gifImage.bitmapImages[(int)ev.NewValue];
        }

        /// <summary>
        /// Defines whether the animation starts on it's own
        /// </summary>
        // public bool AutoStart
        // {
        //    get { return (bool)GetValue(AutoStartProperty); }
        //    set { SetValue(AutoStartProperty, value); }
        // }

        // public static readonly DependencyProperty AutoStartProperty =
        //    DependencyProperty.Register("AutoStart", typeof(bool), typeof(GifImage), new UIPropertyMetadata(false, AutoStartPropertyChanged));

        // private static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        // {
        //    if ((bool)e.NewValue)
        //        (sender as GifImage).StartAnimation();
        // }
        public string GifSource {
            get { return (string)GetValue(GifSourceProperty); }
            set { SetValue(GifSourceProperty, value); }
        }

        public static readonly DependencyProperty GifSourceProperty =
            DependencyProperty.Register("GifSource", typeof(string), typeof(GifImage), new UIPropertyMetadata(string.Empty, GifSourcePropertyChanged));

        private static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (ShowGif)
                (sender as GifImage).Initialize();
        }

        public static readonly DependencyProperty SmallImageSourceProperty =
            DependencyProperty.Register("SmallImageSource", typeof(ImageSource), typeof(GifImage), new UIPropertyMetadata(null, ImageSourcePropertyChanged));

        public ImageSource SmallImageSource {
            get { return (ImageSource)GetValue(SmallImageSourceProperty); }
            set { SetValue(SmallImageSourceProperty, value); }
        }

        private static void ImageSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            GifImage gifImage = sender as GifImage;
            gifImage.Initialize();
        }

        public static readonly DependencyProperty BigImageSourceProperty =
            DependencyProperty.Register("BigImageSource", typeof(ImageSource), typeof(GifImage), new UIPropertyMetadata(null, ImageSourcePropertyChanged));

        public ImageSource BigImageSource {
            get { return (ImageSource)GetValue(BigImageSourceProperty); }
            set { SetValue(BigImageSourceProperty, value); }
        }

        public enum ViewSourceType
        {
            None,
            SmallImage,
            BigImage,
            Gif,
        }

        public static readonly DependencyProperty SourceTypeProperty =
            DependencyProperty.Register("SourceType", typeof(ViewSourceType), typeof(GifImage), new UIPropertyMetadata(ViewSourceType.None, SourceTypePropertyChanged));

        public ViewSourceType SourceType {
            get { return (ViewSourceType)GetValue(SourceTypeProperty); }
            set { SetValue(SourceTypeProperty, value); }
        }

        private static void SourceTypePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ViewSourceType type = (ViewSourceType)e.NewValue;
            ShowGif = false;
            GifImage gifImage = sender as GifImage;
            gifImage.Initialize();
        }

        #endregion

        static GifImage()
        {
            VisibilityProperty.OverrideMetadata(typeof(GifImage),
                new FrameworkPropertyMetadata(VisibilityPropertyChanged));
        }

        private static void VisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void Initialize()
        {
            this.Source = null; // 移除之前绑定的 Image
            if (SourceType == ViewSourceType.SmallImage) {
                Source = SmallImageSource;
            } else if (SourceType == ViewSourceType.BigImage) {
                Source = BigImageSource;
            } else if (SourceType == ViewSourceType.Gif) {
                ShowGif = true;
                if (File.Exists(GifSource)) {
                    gif = new Gif(this.GifSource);
                    var decoder = gif.GetDecoder();
                    _animation = new Int32Animation(0, decoder.Frames.Count - 1, gif.GetTotalDuration());
                    _animation.RepeatBehavior = RepeatBehavior.Forever;
                    this.Source = null;
                    this.Source = gif.GetFirstFrame();
                    _isInitialized = true;
                } else {
                    this.Source = MetaData.DefaultBigImage;
                }
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (ShowGif && gif != null) {
                if (bitmapImages == null)
                    bitmapImages = gif.GetAllFrame().BitmapSources;
                if (bitmapImages != null && bitmapImages.Count > 0)
                    this.StartAnimation();
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (ShowGif && gif != null) {
                stop();
            }

            base.OnMouseLeave(e);
        }

        private void stop()
        {
            this.StopAnimation();
            bitmapImages = null;
            GC.Collect();
        }

        /// <summary>
        /// Starts the animation
        /// </summary>
        public void StartAnimation()
        {
            if (!_isInitialized)
                this.Initialize();

            BeginAnimation(FrameIndexProperty, _animation);
        }

        /// <summary>
        /// Stops the animation
        /// </summary>
        public void StopAnimation()
        {
            BeginAnimation(FrameIndexProperty, null);
        }

        public void Dispose()
        {
            this.BigImageSource = null;
            this.SmallImageSource = null;
            this.gif = null;
            this.bitmapImages = null;
            this._animation = null;
            this.Source = null;
            this.GifSource = null;
        }
    }
}
