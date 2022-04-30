using Jvedio.Plot.Bar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static Jvedio.GlobalVariable;
using System.Windows.Media.Imaging;
using System.Net;
using Jvedio.Utils;
using Jvedio.Entity;

namespace Jvedio
{
    public static class CustomExtension
    {





        public static T GetQueryOrDefault<T>(this BitmapMetadata metadata, string query, T defaultValue)
        {
            if (metadata.ContainsQuery(query))
                return (T)Convert.ChangeType(metadata.GetQuery(query), typeof(T));
            return defaultValue;
        }




        public static string ToTagString(this string str)
        {
            if (str.Length != 1) return "";
            if (str == "1") return Jvedio.Language.Resources.HD;
            if (str == "2") return Jvedio.Language.Resources.Translated;
            if (str == "3") return Jvedio.Language.Resources.FlowOut;
            return str;
        }






        public static List<BarData> ToBarDatas(this Dictionary<string, double> dicSort)
        {
            List<BarData> result = new List<BarData>();
            foreach (var item in dicSort)
            {
                result.Add(new BarData()
                {
                    Value = item.Value,
                    ActualValue = item.Value,
                    Key = item.Key
                });
            }

            return result;
        }











        public static string ToStatusMessage(this HttpStatusCode status)
        {
            switch ((int)status)
            {
                case 403:
                    return Jvedio.Language.Resources.NotShowInCountry;
                case 404:
                    return Jvedio.Language.Resources.NoID;
                case 504:
                    return Jvedio.Language.Resources.TimeOut;
                case 302:
                    return Jvedio.Language.Resources.TooFrequent;
                default:
                    return status.ToString();
            }
        }




    }



}
