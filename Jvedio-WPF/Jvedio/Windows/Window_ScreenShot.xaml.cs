using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    /// <summary>
    /// Window_ImageViewer.xaml 的交互逻辑
    /// </summary>
    public partial class Window_ScreenShot : Window
    {

        bool canChangeSize = false;
        Point sizePoint;
        public Window_ScreenShot(Window owner, ImageSource source)
        {
            InitializeComponent();
            if (GlobalVariable.GlobalFont != null) this.FontFamily = GlobalVariable.GlobalFont;//设置字体
            this.Owner = owner;
            ImageViewer.Source = source;
            //this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width/2;
            //this.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height/2;
            //this.Top = 0;
            //this.Left = 0;
            Canvas1.Visibility = Visibility.Hidden;
            var Ellipses = SizeControlGrid.Children.OfType<Ellipse>().ToList();
            foreach (var Ellipse in Ellipses)
            {
                Ellipse.MouseLeftButtonDown += (s, e) =>
                {
                    canMove = false;
                    canChangeSize = true;
                    sizePoint = new Point(Canvas.GetLeft(Rectangle1), Canvas.GetTop(Rectangle1));
                };
                Ellipse.MouseLeftButtonUp += (s, e) => canChangeSize = false;
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                var anim = new DoubleAnimation(0, (Duration)FadeInterval);
                anim.Completed += (s, _) => this.Close();
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            }
            else
            {
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ImageViewer.Source = null;
            base.OnClosed(e);
        }



        private void Grid_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        Point startPoint;
        Point rectangle;
        bool canMove = false;
        int mouseDownCount = 0;
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDownCount++;
            if (e.GetPosition(Rectangle1).X > 0 && e.GetPosition(Rectangle1).X < Rectangle1.Width &&
    e.GetPosition(Rectangle1).Y > 0 && e.GetPosition(Rectangle1).Y < Rectangle1.Height)
            {
                canMove = true;
            }


            CutImage.Source = null;
            rectangle = new Point(Canvas.GetLeft(Rectangle1), Canvas.GetTop(Rectangle1));
            startPoint = e.GetPosition(this);
            Canvas1.Visibility = Visibility.Visible;
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseDownCount++;
            canMove = false;
            this.Cursor = Cursors.Arrow;
            canChangeSize = false;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            //画矩形
            if (mouseDownCount == 1)
            {

                double width = e.GetPosition(this).X - startPoint.X;
                double height = e.GetPosition(this).Y - startPoint.Y;
                Rectangle1.Width = Math.Abs(width);
                Rectangle1.Height = Math.Abs(height);

                if (width >= 0)
                    Canvas.SetLeft(Rectangle1, startPoint.X);
                else
                    Canvas.SetLeft(Rectangle1, e.GetPosition(this).X);

                if (height >= 0)
                    Canvas.SetTop(Rectangle1, startPoint.Y);
                else
                    Canvas.SetTop(Rectangle1, e.GetPosition(this).Y);

                SetImage();
            }
            else if (canMove && mouseDownCount >= 2)
            {
                if (e.GetPosition(Rectangle1).X > 0 && e.GetPosition(Rectangle1).X < Rectangle1.Width &&
                    e.GetPosition(Rectangle1).Y > 0 && e.GetPosition(Rectangle1).Y < Rectangle1.Height)
                {
                    //移动矩形
                    double left = rectangle.X - startPoint.X + e.GetPosition(this).X;
                    double top = rectangle.Y - startPoint.Y + e.GetPosition(this).Y;

                    Canvas.SetLeft(Rectangle1, left);
                    Canvas.SetTop(Rectangle1, top);
                    SetImage();
                }

                Console.WriteLine("---1---");

            }
            else if (canChangeSize)
            {
                //调整矩形大小
                double height = e.GetPosition(this).Y - Canvas.GetTop(Rectangle1);

                if (height < 0)
                {
                    Canvas.SetTop(Rectangle1, e.GetPosition(this).Y);
                    Rectangle1.Height = sizePoint.Y - e.GetPosition(this).Y;
                }
                else
                {
                    Rectangle1.Height = Math.Abs(height);
                }
            }
        }

        private void SetImage()
        {
            Canvas.SetLeft(CutImage, Canvas.GetLeft(Rectangle1));
            Canvas.SetTop(CutImage, Canvas.GetTop(Rectangle1));
            CutImage.Width = Rectangle1.Width;
            CutImage.Height = Rectangle1.Height;
            Canvas.SetLeft(SizeBorder, Canvas.GetLeft(Rectangle1));
            Canvas.SetTop(SizeBorder, Canvas.GetTop(Rectangle1));
            SizeTextBlock.Text = $"{CutImage.Width} x {CutImage.Height}";

            Int32Rect rect = new Int32Rect();
            rect.X = (int)Canvas.GetLeft(Rectangle1);
            rect.Y = (int)Canvas.GetTop(Rectangle1);
            rect.Width = Math.Abs((int)Rectangle1.Width);
            rect.Height = Math.Abs((int)Rectangle1.Height);

            CutImage.Source = null;
            CutImage.Source = GetImageSourceByRect(rect);
        }


        private ImageSource GetImageSourceByRect(Int32Rect rect)
        {
            BitmapSource imageSource = (BitmapSource)ImageViewer.Source;
            try
            {
                if (rect.X >= 0 && rect.Y >= 0 && rect.Width <= imageSource.Width && rect.Height <= imageSource.Height)
                    return new CroppedBitmap(imageSource, rect);
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }



        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ImageViewer.Source = null;
            CutImage.Source = null;
            GC.Collect();
        }

        private void CutImage_MouseEnter(object sender, MouseEventArgs e)
        {
            if (mouseDownCount >= 2)
                this.Cursor = Cursors.SizeAll;
        }

        private void CutImage_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void ChangeWidthAndHeight(object sender, MouseEventArgs e)
        {

        }

        private void ChangeHeight(object sender, MouseEventArgs e)
        {
            if (canChangeSize)
            {
                double height = e.GetPosition(this).Y - Canvas.GetTop(Rectangle1);

                if (height < 0)
                {
                    Canvas.SetTop(Rectangle1, e.GetPosition(this).Y);
                    Rectangle1.Height = sizePoint.Y - e.GetPosition(this).Y;
                }
                else
                {
                    Rectangle1.Height = Math.Abs(height);
                }
            }
        }

        private void ChangeWidth(object sender, MouseEventArgs e)
        {

        }
    }
}
