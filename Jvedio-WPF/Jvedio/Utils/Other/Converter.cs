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

namespace Jvedio
{



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

    public class CookieNothingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            if (value.ToString() == "") return Jvedio.Language.Resources.Nothing;
            return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }


    public class MovieStampTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!Properties.Settings.Default.DisplayStamp) return Visibility.Hidden;

            if (value == null)
            {
                return Visibility.Hidden;
            }
            else
            {
                MovieStampType movieStampType = (MovieStampType)value;
                if (movieStampType == MovieStampType.无)
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Visible;
                }


            }



        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }



    public class ParseImagePathConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = "";
            if (value == null || string.IsNullOrEmpty(value.ToString())) return null;
            bool OnlyDir = false;
            if (parameter != null && parameter.ToString() == "OnlyDir") OnlyDir = true;
            path = Video.parseImagePath(value.ToString());
            if (OnlyDir)
                return System.IO.Path.GetDirectoryName(path);
            else
                return path;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class TagStampsConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            if (value.ToString().IndexOf(parameter.ToString().ToTagString()) >= 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }



    public class IntToVedioTypeConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Jvedio.Language.Resources.Normal;
            Enum.TryParse(value.ToString(), out VideoType videoType);
            if (videoType == VideoType.Normal)
            {
                return Jvedio.Language.Resources.Normal;
            }
            else if (videoType == VideoType.UnCensored)
            {
                return Jvedio.Language.Resources.Uncensored;
            }
            else if (videoType == VideoType.Censored)
            {
                return Jvedio.Language.Resources.Censored;
            }
            else if (videoType == VideoType.Europe)
            {
                return Jvedio.Language.Resources.Europe;
            }
            return Jvedio.Language.Resources.Normal;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
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
