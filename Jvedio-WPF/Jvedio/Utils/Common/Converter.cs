using Jvedio.Core.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Jvedio
{
    public class VideoTypeConverter : IValueConverter
    {
        // 数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null | parameter == null)
            {
                return false;
            }

            int intparameter = int.Parse(parameter.ToString());
            if ((int)value == intparameter)
                return true;
            else
                return false;
        }

        // 选中项地址转换为数字
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null | parameter == null) return 0;
            int intparameter = int.Parse(parameter.ToString());
            return (VideoType)intparameter;
        }
    }

    public class VideoTypeToIntConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return SuperControls.Style.LangManager.GetValueByKey("Normal");
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
