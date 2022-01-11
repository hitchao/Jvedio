using Jvedio.Utils.ImageAndVedio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Jvedio
{
    public class GifImage : Image
    {
        private bool _isInitialized;
        private Int32Animation _animation;

        private List<BitmapSource> bitmapImages;
        private Gif gif;
        public int FrameIndex
        {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }

        private void Initialize()
        {
            if (!File.Exists(GifSource))
            {
                this.Source = GlobalVariable.DefaultBigImage;
            }
            else
            {
                gif = new Gif(this.GifSource);
                var decoder = gif.GetDecoder();
                _animation = new Int32Animation(0, decoder.Frames.Count - 1, gif.GetTotalDuration());
                _animation.RepeatBehavior = RepeatBehavior.Forever;
                this.Source = null;
                this.Source = gif.GetFirstFrame();
                _isInitialized = true;
            }

        }

        


        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (bitmapImages == null && gif!=null) bitmapImages = gif.GetAllFrame().BitmapSources;
            if (bitmapImages!=null && bitmapImages .Count>0) this.StartAnimation();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.StopAnimation();
            bitmapImages = null;
            GC.Collect();
            base.OnMouseLeave(e);
        }





        static GifImage()
        {
            VisibilityProperty.OverrideMetadata(typeof(GifImage),
                new FrameworkPropertyMetadata(VisibilityPropertyChanged));
        }

        private static void VisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

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
        public bool AutoStart
        {
            get { return (bool)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }

        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register("AutoStart", typeof(bool), typeof(GifImage), new UIPropertyMetadata(false, AutoStartPropertyChanged));

        private static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (sender as GifImage).StartAnimation();
        }

        public string GifSource
        {
            get { return (string)GetValue(GifSourceProperty); }
            set { SetValue(GifSourceProperty, value); }
        }

        public static readonly DependencyProperty GifSourceProperty =
            DependencyProperty.Register("GifSource", typeof(string), typeof(GifImage), new UIPropertyMetadata(string.Empty, GifSourcePropertyChanged));

        private static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as GifImage).Initialize();
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

    }
}
