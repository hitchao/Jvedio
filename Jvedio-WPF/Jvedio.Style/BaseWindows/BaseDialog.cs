
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;

namespace Jvedio.Style
{

    public class BaseDialog : Window
    {

        public bool showbutton = true;//是否显示取消按钮
        bool IsFlashing = false;//是否因失去焦点而闪烁
        public BaseDialog(Window owner)
        {
            this.Style = (System.Windows.Style)App.Current.Resources["BaseDialogStyle"];
            this.Loaded += delegate { InitEvent(); };//初始化载入事件
            this.ContentRendered += (s, e) =>
            {
                ControlTemplate baseDialogControlTemplate = (ControlTemplate)App.Current.Resources["BaseDialogControlTemplate"];
                StackPanel sp = (StackPanel)baseDialogControlTemplate.FindName("ButtonStackPanel", this);
                if (!showbutton) sp.Visibility = Visibility.Collapsed;
            };
            this.Owner = owner;
        }

        public BaseDialog(Window owner, bool showbutton) : this(owner)
        {
            this.showbutton = showbutton;


        }


        private void InitEvent()
        {
            ControlTemplate baseDialogControlTemplate = (ControlTemplate)App.Current.Resources["BaseDialogControlTemplate"];

            Button closeBtn = (Button)baseDialogControlTemplate.FindName("BorderClose", this);
            closeBtn.Click += delegate (object sender, RoutedEventArgs e)
            {
                FadeOut();
            };


            Button cancel = (Button)baseDialogControlTemplate.FindName("CancelButton", this);
            cancel.Click += delegate (object sender, RoutedEventArgs e)
            {
                this.DialogResult = false;
            };

            Button confirm = (Button)baseDialogControlTemplate.FindName("ConfirmButton", this);
            confirm.Click += Confirm;

            Border borderTitle = (Border)baseDialogControlTemplate.FindName("BorderTitle", this);
            borderTitle.MouseMove += MoveWindow;
        }


        protected virtual void Confirm(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        protected override void OnActivated(EventArgs e)
        {
            IsFlashing = false;
            ControlTemplate baseDialogControlTemplate = (ControlTemplate)App.Current.Resources["BaseDialogControlTemplate"];
            Border border = (Border)baseDialogControlTemplate.FindName("MainBorder", this);
            DropShadowEffect dropShadowEffect = new DropShadowEffect() { Color = Colors.SkyBlue, BlurRadius = 20, Direction = -90, RenderingBias = RenderingBias.Quality, ShadowDepth = 0 };
            border.Effect = dropShadowEffect;
            base.OnActivated(e);
        }




        protected async override void OnDeactivated(EventArgs e)
        {
            if (IsFlashing) return;
            IsFlashing = true;
            //边缘闪动
            ControlTemplate baseDialogControlTemplate = (ControlTemplate)App.Current.Resources["BaseDialogControlTemplate"];
            Border border = (Border)baseDialogControlTemplate.FindName("MainBorder", this);
            DropShadowEffect dropShadowEffect1 = new DropShadowEffect() { Color = Colors.Red, BlurRadius = 20, Direction = -90, RenderingBias = RenderingBias.Quality, ShadowDepth = 0 };
            DropShadowEffect dropShadowEffect2 = new DropShadowEffect() { Color = Colors.SkyBlue, BlurRadius = 20, Direction = -90, RenderingBias = RenderingBias.Quality, ShadowDepth = 0 };


            for (int i = 0; i < 3; i++)
            {
                if (!IsFlashing) break;
                border.Effect = dropShadowEffect1;
                await Task.Delay(100);
                border.Effect = dropShadowEffect2;
                await Task.Delay(100);
            }
            if (IsFlashing) border.Effect = dropShadowEffect1;
            IsFlashing = false;
            base.OnDeactivated(e);
        }



        public void FadeOut()
        {
            this.DialogResult = false;
            this.Close();
        }



        private void MoveWindow(object sender, MouseEventArgs e)
        {
            //移动窗口
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }







}
