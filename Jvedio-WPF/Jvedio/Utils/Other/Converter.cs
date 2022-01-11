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
using Jvedio.Utils;

namespace Jvedio
{


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
            if (value == null) return Jvedio.Language.Resources.Uncensored;

            int.TryParse(value.ToString(), out int vt);
            if (vt == 1)
            {
                return Jvedio.Language.Resources.Uncensored;
            }
            else if (vt == 2)
            {
                return Jvedio.Language.Resources.Censored;
            }
            else if (vt == 3)
            {
                return Jvedio.Language.Resources.Europe;
            }
            return Jvedio.Language.Resources.All;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}
