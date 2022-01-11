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
            if (str == "1" ) return  Jvedio.Language.Resources.HD ;
            if (str == "2" ) return Jvedio.Language.Resources.Translated;
            if (str == "3"  ) return Jvedio.Language.Resources.FlowOut;
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

        public static string ToSqlField(this string content)
        {
            if (content == Jvedio.Language.Resources.ID)
            {
                return "id";
            }else if (content == Jvedio.Language.Resources.Title)
            {
                return "title";
            }
            else if (content == Jvedio.Language.Resources.TranslatedTitle)
            {
                return "chinesetitle";
            }
            else if (content == Jvedio.Language.Resources.VedioType)
            {
                return "vediotype";
            }
            else if (content == Jvedio.Language.Resources.ReleaseDate)
            {
                return "releasedate";
            }
            else if (content == Jvedio.Language.Resources.Year)
            {
                return "year";
            }
            else if (content == Jvedio.Language.Resources.Duration)
            {
                return "runtime";
            }
            else if (content == Jvedio.Language.Resources.Country)
            {
                return "country";
            }
            else if (content == Jvedio.Language.Resources.Director)
            {
                return "director";
            }
            else if (content == Jvedio.Language.Resources.Genre)
            {
                return "genre";
            }
            else if (content == Jvedio.Language.Resources.Label)
            {
                return "label";
            }
            else if (content == Jvedio.Language.Resources.Actor)
            {
                return "actor";
            }
            else if (content == Jvedio.Language.Resources.Studio)
            {
                return "studio";
            }
            else if (content == Jvedio.Language.Resources.Rating)
            {
                return "rating";
            }
            else
            {
                return "";
            }

        }


        public static string[] ToFileName(this DetailMovie movie)
        {
            string txt = Properties.Settings.Default.RenameFormat;
            FileInfo fileInfo = new FileInfo(movie.filepath);
            string oldName = movie.filepath;
            string name = Path.GetFileNameWithoutExtension(movie.filepath);
            string dir = fileInfo.Directory.FullName;
            string ext = fileInfo.Extension;
            string newName = "";
            MatchCollection matches = Regex.Matches(txt, "\\{[a-z]+\\}");
            if (matches != null && matches.Count > 0)
            {
                newName = txt;
                foreach (Match match in matches)
                {
                    string property = match.Value.Replace("{", "").Replace("}", "");
                    ReplaceWithValue(property, movie, ref newName);
                }
            }

            //替换掉特殊字符
            foreach (char item in BANFILECHAR) { newName = newName.Replace(item.ToString(), ""); }

            if (Properties.Settings.Default.DelRenameTitleSpace) newName = newName.Replace(" ", "");
            if (movie.hassubsection)
            {
                string[] result = new string[movie.subsectionlist.Count];
                for (int i = 0; i < movie.subsectionlist.Count; i++)
                {
                    if(Properties.Settings.Default.AddLabelTagWhenRename && Identify.IsCHS(oldName))
                    {
                        result[i] = Path.Combine(dir, $"{newName}-{i + 1}_{Jvedio.Language.Resources.Translated}{ext}");
                    }
                    else
                    {
                        result[i] = Path.Combine(dir, $"{newName}-{i + 1}{ext}");
                    }
                    
                }
                return result;
            }
            else
            {
                if (Properties.Settings.Default.AddLabelTagWhenRename && Identify.IsCHS(oldName))
                {
                    return new string[] { Path.Combine(dir, $"{newName}_{Jvedio.Language.Resources.Translated}{ext}") };
                }
                else
                {
                    return new string[] { Path.Combine(dir, $"{newName}{ext}") };
                }
            }


        }



        private static void ReplaceWithValue(string property,Movie movie,ref string result)
        {
            string inSplit = Properties.Settings.Default.InSplit.Replace("无", "");
            PropertyInfo[] PropertyList = movie.GetType().GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                if (name == property)
                {
                    object o = item.GetValue(movie);
                    if (o != null)
                    {
                        string value = o.ToString();

                        if (property == "actor" || property == "genre" || property == "label")
                            value = value.Replace(" ", inSplit).Replace("/", inSplit);

                        if (property == "vediotype")
                        {
                            int v = 1;
                            int.TryParse(value, out v);
                            if (v == 1)
                                value = Jvedio.Language.Resources.Uncensored;
                            else if (v == 2)
                                value = Jvedio.Language.Resources.Censored;
                            else if (v == 3)
                                value = Jvedio.Language.Resources.Europe;
                        }

                       



                        if (string.IsNullOrEmpty(value))
                        {
                            //如果值为空，则删掉前面的分隔符
                            int idx = result.IndexOf("{" + property + "}");
                            if (idx >= 1)
                            {
                                result = result.Remove(idx - 1,1);
                            }
                            result = result.Replace("{" + property + "}","");
                        }
                        else
                            result = result.Replace("{" + property + "}", value);



                    }
                    else
                    {
                        int idx = result.IndexOf("{" + property + "}");
                        if (idx >= 1)
                        {
                            result = result.Remove(idx - 1);
                        }
                        result = result.Replace("{" + property + "}", "");
                    }
                    break;
                }
            }
            
        }




    }



}
