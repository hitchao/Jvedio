using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;


/// <summary>
/// 转换基类
/// </summary>
namespace Jvedio.Utils.Converter
{

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


    public class ShowFilterConverter : IValueConverter
    {
        //数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value) return 600;
            else return 0;
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








    public class ViewTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            ViewType myViewType = ViewType.默认;
            Enum.TryParse<ViewType>(value.ToString(), out myViewType);

            return myViewType.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(ViewType), parameter.ToString(), true) : null;
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

    public class ImageTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;
            return (((MyImageType)value).ToString() == parameter.ToString());
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(MyImageType), parameter.ToString(), true) : null;
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








    public class PlusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == " + ")
                return Visibility.Collapsed;
            else
                return Visibility.Visible;

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class LabelToListConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return " + ";
            List<string> result = value.ToString().Split(' ').ToList();
            result.Insert(0, " + ");
            return result;

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "";
            List<string> vs = value as List<string>;
            vs.Remove(" + ");
            return string.Join(" ", vs);
        }
    }



    public class FontFamilyConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null || value.ToString() == "") return "微软雅黑";

            return value.ToString();

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    //setings




    public class SkinStringToCheckedConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null) return false;


            if (value.ToString() == parameter.ToString())
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null) return null;
            if ((bool)value)
                return parameter.ToString();
            else
                return null;
        }
    }



    public class SkinTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            return (((Skin)value).ToString() == parameter.ToString());
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(Skin), parameter.ToString(), true) : null;
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




    public class LanguageTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;
            return (((MyLanguage)value).ToString() == parameter.ToString());
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(MyLanguage), parameter.ToString(), true) : null;
        }
    }


    public class MultiIntToMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int padUD = 5;
            int padLR = 5;
            int.TryParse(values[0].ToString(), out padUD);
            int.TryParse(values[1].ToString(), out padLR);

            return new Thickness(padLR, padUD, padLR, padUD);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class MyColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Brushes.Black;
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value.ToString()));
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
            {
                if (parameter.ToString() == "BorderBrush")
                    return Brushes.Gold;
                else if (parameter.ToString() == "Background")
                    return Brushes.LightGreen;
            }

            if (parameter.ToString() == "BorderBrush")
                return Brushes.Gold;
            else
                return Brushes.LightGreen;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                if (parameter.ToString() == "BorderBrush")
                    return System.Drawing.Color.Gold;
                else if (parameter.ToString() == "Background")
                    return System.Drawing.Color.LightGreen;
            }

            return "#123455";



        }
    }


    public class FontFamilyToSelectedIndexConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return 0;

            int i = -1;
            foreach (FontFamily item in Fonts.SystemFontFamilies)
            {
                i++;
                if (item.ToString() == value.ToString()) return i;
            }
            return 0;
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



    public class BoolToFontBoldConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return FontWeights.Normal;

            if ((bool)value)
                return FontWeights.Bold;
            else
                return FontWeights.Normal;


        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }



    public class BoolToFontItalicConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return FontStyles.Normal;

            if ((bool)value)
                return FontStyles.Italic;
            else
                return FontStyles.Normal;


        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }


    public class BoolToUnderLineConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return "";

            if ((bool)value)
                return "Underline";
            else
                return "";


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



    public class PlotToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null) return Visibility.Hidden;
            if (value.ToString() == "" | string.IsNullOrEmpty(value.ToString()))
                return Visibility.Hidden;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }








}
