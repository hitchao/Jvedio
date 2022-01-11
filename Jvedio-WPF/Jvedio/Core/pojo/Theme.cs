using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using DynamicData.Annotations;
using System.Runtime.CompilerServices;

namespace Jvedio.Core.pojo
{
    public class Theme : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string Font { get; set; }
        public DisplayProperty DisplayProperty { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Theme()
        {
            DisplayProperty = new DisplayProperty();
        }


    }

    public class DisplayProperty
    {
        public DisplayStyle Title { get; set; }//标题栏
        public DisplayStyle Side { get; set; }//侧边栏
        public DisplayStyle Display { get; set; }//展示界面
        public DisplayStyle Tools { get; set; }//工具栏
        public DisplayStyle Search { get; set; }//搜索栏
        public DisplayStyle Menu { get; set; }//菜单栏
        public DisplayStyle Global { get; set; }//默认

        public DisplayProperty()
        {
            //默认为黑色
            Title = new DisplayStyle("#22252A", "#AFAFAF");
            Side = new DisplayStyle("#101013", "#AFAFAF");
            Display = new DisplayStyle("#1B1B1F", "#AFAFAF");
            Tools = new DisplayStyle("#383838", "#AFAFAF");
            Search = new DisplayStyle("#18191B", "#FFFFFF");
            Menu = new DisplayStyle("#252526", "#AFAFAF");
            Global = new DisplayStyle("#22252A", "#AFAFAF");
        }
    }


    public class DisplayStyle
    {
        public Color MainBackground { get; set; }//主背景色
        public Color SubBackground { get; set; }//副背景色
        public Color MainForeground { get; set; }//主前景色
        public Color SubForeground { get; set; }//副前景色
        public int MainFontSize { get; set; }//主字体大小
        public int SubFontSize { get; set; }//副字体大小

        public DisplayStyle() { }

        public DisplayStyle(string color1, string color2)
        {
            this.MainBackground = (Color)ColorConverter.ConvertFromString(color1);
            this.MainForeground = (Color)ColorConverter.ConvertFromString(color2);
        }


    }

}
