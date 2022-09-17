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
using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Utils;
using SuperUtils.Common;

namespace Jvedio
{



    public class MultiUnitConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0 || parameter == null) return "";
            string[] units = parameter.ToString().Split(',');

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                if (i == values.Length - 1)
                {
                    string value = values[i].ToString();
                    if (!string.IsNullOrEmpty(value))
                        builder.Append(value);
                }
                else
                {
                    int.TryParse(values[i].ToString(), out int value);
                    if (value > 0)
                    {
                        builder.Append(value.ToString());
                        if (i < units.Length) builder.Append($"{units[i]}  ");
                    }
                }

            }
            return builder.ToString().TrimEnd();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class MultiLargerConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0) return "";
            int param = 0;
            if (parameter != null)
                param = int.Parse(parameter.ToString());

            StringBuilder builder = new StringBuilder();

            foreach (var item in values)
            {
                int.TryParse(item.ToString(), out int value);
                if (value > param) builder.Append(value.ToString() + "  ");
            }
            return builder.ToString().TrimEnd();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class NotEqualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            char c1 = char.Parse(value.ToString());
            char c2 = char.Parse(parameter.ToString());
            if (c1.Equals(c2)) return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }




    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) return Visibility.Collapsed;
            else return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class VideoTypeConverter : IValueConverter
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
            return (VideoType)intparameter;
        }


    }




    public class VidioTypeToIntConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Jvedio.Language.Resources.Normal;
            Enum.TryParse(value.ToString(), out VideoType videoType);
            return (int)videoType;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return VideoType.Normal;
            int.TryParse(value.ToString(), out int videoType);
            return (VideoType)videoType;
        }
    }

}
