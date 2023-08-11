
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jvedio.Core.Extensions
{
    /// <summary>
    /// 滚轮速度控制
    /// <para>参考：<see href="https://stackoverflow.com/a/52075782/13454100">stackoverflow</see></para>
    /// </summary>
    public class WheelSpeedScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty SpeedFactorProperty =
            DependencyProperty.Register(nameof(SpeedFactor),
                                        typeof(Double),
                                        typeof(WheelSpeedScrollViewer),
                                        new PropertyMetadata(3.0));

        public Double SpeedFactor {
            get { return (Double)GetValue(SpeedFactorProperty); }
            set { SetValue(SpeedFactorProperty, value); }
        }


        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (ScrollInfo is ScrollContentPresenter scp &&
                ComputedVerticalScrollBarVisibility == Visibility.Visible) {
                scp.SetVerticalOffset(VerticalOffset - e.Delta * SpeedFactor);
                e.Handled = true;
            }
        }

        //protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        //{
        //    if (ScrollInfo is ScrollContentPresenter scp &&
        //        ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if (Animating)
        //            return;
        //        double originOffset = this.VerticalOffset; ;
        //        double targetOffset = VerticalOffset - e.Delta * SpeedFactor;
        //        Console.WriteLine("targetOffset = " + targetOffset);
        //        //scp.SetVerticalOffset(targetOffset);
        //        //this.BeginAnimation(VerticalOffsetProperty, null);
        //        DoubleAnimation verticalAnimation = new DoubleAnimation();
        //        verticalAnimation.From = originOffset;
        //        verticalAnimation.To = targetOffset;
        //        verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(10));
        //        Animating = true;
        //        verticalAnimation.Completed += delegate
        //        {
        //            Animating = false;
        //        };
        //        this.BeginAnimation(VerticalOffsetProperty, verticalAnimation);
        //        e.Handled = true;
        //    }
        //}
    }
}
