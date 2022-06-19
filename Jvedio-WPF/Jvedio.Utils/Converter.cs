
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/// <summary>
/// 转换基类
/// </summary>
namespace Jvedio.Utils.Converter
{
    public class RoundNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) return null;
            int round = 2;
            if (parameter != null && !string.IsNullOrEmpty(parameter.ToString()))
                int.TryParse(parameter.ToString(), out round);
            double.TryParse(value.ToString(), out double v);
            return Math.Round(v, round);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Base64ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = value as string;

            if (string.IsNullOrEmpty(s))
                return null;
            if (s.IndexOf(",") > 0)
                s = s.Split(',')[1];
            BitmapImage bi = new BitmapImage();

            bi.BeginInit();
            bi.StreamSource = new MemoryStream(System.Convert.FromBase64String(s));
            bi.EndInit();

            return bi;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class FilePathToDirConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = "";
            if (value == null) return null;
            path = value.ToString();
            if (string.IsNullOrEmpty(path)) return null;
            return System.IO.Path.GetDirectoryName(path);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class SmallerPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            int count = 2;
            if (parameter != null) int.TryParse(parameter.ToString(), out count);
            string[] strs = value.ToString().Split('\\');
            if (strs.Length <= count) return value.ToString();

            StringBuilder builder = new StringBuilder();
            for (int i = strs.Length - 1; i >= strs.Length - count; i--)
            {
                builder.Insert(0, "\\" + strs[i]);
            }

            return "..." + builder.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double.TryParse(value.ToString(), out double result);
            return (int)result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            else
            {
                int.TryParse(value.ToString(), out int idx);
                int.TryParse(parameter.ToString(), out int p);
                if (idx == p) return Visibility.Visible;
                else return Visibility.Collapsed;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    public class SmallerThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return new Thickness(0);
            else
            {
                int.TryParse(parameter.ToString(), out int t);
                Thickness thickness = (Thickness)value;
                if (thickness.Left + t <= 0)
                    return new Thickness(0);
                else
                    return new Thickness(thickness.Left + t);
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    public class OppositeBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    public class BiggerWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            double width = 0;
            double.TryParse(value.ToString(), out width);
            return width;

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    public class Hide2TextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == "0") return "隐藏";
            return "取消隐藏";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    public class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return 0;
            double.TryParse(value.ToString(), out double width);
            double.TryParse(parameter.ToString(), out double w);
            if (width + w > 0)
                return width + w;
            else
                return 0;


        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

 

    public class BoolToOppositeVisibilityConverter : IValueConverter
    {
        //数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value) return Visibility.Collapsed; else return Visibility.Visible;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        //数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value) return Visibility.Visible; else return Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    public class IntToCheckedConverter : IValueConverter
    {
        //数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null | parameter == null) { return false; }
            int intparameter = int.Parse(parameter.ToString());
            if ((int)value == intparameter)
                return true;
            else
                return false;
        }

        //选中项地址转换为数字

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null | parameter == null) return 0;
            int intparameter = int.Parse(parameter.ToString());
            return intparameter;
        }


    }


    public class StringToCheckedConverter : IValueConverter
    {
        //判断是否相同
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == parameter.ToString()) { return true; } else { return false; }
        }

        //选中项地址转换为数字

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter.ToString();
        }


    }













    public class WidthToMarginConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || double.Parse(value.ToString()) <= 0) return
                      150;
            else
                return double.Parse(value.ToString()) - 40;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class StringToUriStringConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "黑色")
                return $"pack://Application:,,,/Jvedio;;;Component/Resources/Skin/black/{parameter}.png";
            else if (value.ToString() == "白色")
                return $"pack://Application:,,,/Jvedio;;;Component/Resources/Skin/white/{parameter}.png";
            else if (value.ToString() == "蓝色")
                return $"pack://Application:,,,/Jvedio;;;Component/Resources/Skin/black/{parameter}.png";
            else
                return $"pack://Application:,,,/Jvedio;;;Component/Resources/Skin/black/{parameter}.png";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }




    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "2")
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }









    public class IntToVisibility : IValueConverter
    {
        //数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int v = int.Parse(value.ToString());
            if (v <= 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        //选中项地址转换为数字

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

 

 




    public class SmallerValueConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null) return 0;
            int.TryParse(value.ToString(), out int w1);
            int.TryParse(parameter.ToString(), out int w2);
            if (w1 - w2 < 0) return 0;
            else return w1 - w2;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

  


    public class BoolToImageStretchConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return Stretch.Uniform;

            if ((bool)value)
                return Stretch.UniformToFill;
            else
                return Stretch.Uniform;


        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }


 

    //Edit
    public class BitToGBConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "0 GB";

            //保留2位小数
            double.TryParse(value.ToString(), out double filesize);

            filesize = filesize / 1024 / 1024 / 1024;//GB
            if (filesize >= 0.9)
                return $"{Math.Round(filesize, 2)} GB";//GB
            else
                return $"{Math.Ceiling(filesize * 1024)} MB";//MB
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

 








}
