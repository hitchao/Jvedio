using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;
namespace Jvedio
{
    public static class Identify
    {
        public static double MinHDVFileSize = 2;//多少 GB 视为高清
        
        public static string[] CHS = new string[] { "中字", "中文字幕", "字幕", "中文", "translated", "translate" };
        public static string[] HDV = new string[] { "hd", "high_definition", "high definition", "高清", "2K", "4K", "8K", "16K", "32K" };
        public static string[] FLOWOUT;

        static Identify()
        {
            FLOWOUT=Resource_String.FLOWOUT.Split(',');
        }


        public static void InitFanhaoList()
        {
            Censored = new List<string>();
            Uncensored = new List<string>();
            Censored.AddRange(Resource_String.Censored.Split(',').Where(arg => !string.IsNullOrEmpty(arg) && arg.Length > 0).ToList());
            Uncensored.AddRange(Resource_String.Uncensored.Split(',').Where(arg => !string.IsNullOrEmpty(arg) && arg.Length > 0).ToList());
        }

        public static string GetFanhaoFromDMMUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            string result = "";
            var values = url.Split('/').ToList();
            string cid = "";
            foreach (var item in values)
            {
                if (!string.IsNullOrEmpty(item) && item.IndexOf("cid=") >= 0)
                {
                    cid = item;
                    break;
                }
            }
            if (cid.IndexOf("cid=") >= 0)
            {
                return GetFanhao(cid.Replace("cid=", ""));
            }
            return result;
        }

        public static bool IsFlowOut(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) return false;
            string name = new FileInfo(filepath).Name.ToLower();
            return FLOWOUT.Any(arg => name.IndexOf(arg) >= 0);
        }

        public static bool IsCHS(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) return false;
            string name = Path.GetFileNameWithoutExtension(filepath).ToLower();
            //-c后面没有英文
            if (name.IndexOf("-c") > 0 || name.IndexOf("_c") > 0)
            {
                int idx = name.LastIndexOf("-c");
                if (idx > 0 && idx == name.Length - 2) return true;
                else if (idx > 0 && idx < name.Length - 2 && !name[idx + 2].IsLetter()) return true;

                idx = name.LastIndexOf("_c");
                if (idx > 0 && idx == name.Length - 2) return true;
                else if (idx > 0 && idx < name.Length - 2 && !name[idx + 2].IsLetter()) return true;
                return CHS.Any(arg => name.IndexOf(arg) >= 0);
            }
            else
            {
                return CHS.Any(arg => name.IndexOf(arg) >= 0);
            }




        }


        //TODO 根据码率、分辨率来判断视频清晰程度
        public static bool IsHDV(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) return false;
            FileInfo fileInfo = new FileInfo(filepath);
            string name = fileInfo.Name.ToLower();
            if (!File.Exists(filepath)) return HDV.Any(arg => name.IndexOf(arg) >= 0);
            long filesize = fileInfo.Length;
            return filesize > 0 && filesize / 1024 / 1024 / 1024 >= MinHDVFileSize;
        }


        public static string GetEng(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";
            Match match = Regex.Match(content, @"[a-z]+", RegexOptions.IgnoreCase);
            if (match != null)
                return match.Value;
            else
                return "";
        }

        public static string GetNum(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";
            Match match = Regex.Match(content, @"[0-9]+");
            if (match != null)
                return match.Value;
            else
                return "";
        }


        /// <summary>
        /// 获得视频类型
        /// </summary>
        /// <param name="FileName">番号</param>
        /// <returns></returns>
        public static VedioType GetVideoType(string movieId)
        {
            if (string.IsNullOrEmpty(movieId)) return VedioType.所有;
            if (movieId.ToLower().IndexOf("s2m") >= 0) return VedioType.步兵;
            if (movieId.ToLower().IndexOf("CW3D2DBD".ToLower()) >= 0) return VedioType.步兵;
            if (movieId.ToLower().IndexOf("t28") >= 0) return VedioType.骑兵;

            // 一本道、メス豚、天然むすめ
            if (movieId.IndexOf("_") > 0) { return VedioType.步兵; }
            else
            {
                if (movieId.IndexOf("-") > 0)
                {
                    //分割番号
                    string fanhao1 = movieId.Split('-')[0];
                    string fanhao2 = movieId.Split('-')[1];

                    if (fanhao1.All(char.IsDigit))
                    {
                        //全数字：加勒比
                        return VedioType.步兵;
                    }
                    else
                    {
                        //优先匹配步兵
                        if (Uncensored.Contains(fanhao1)) { return VedioType.步兵; }
                        else if (Censored.Contains(fanhao1)) { return VedioType.骑兵; }

                        else
                        {

                            // 剩下的如果还没匹配到，看看是否为 XXXX-000格式

                            if (GetEng(fanhao1) != "" && GetNum(fanhao2) != "")
                                return VedioType.骑兵;
                            else
                                return 0;
                        }
                    }
                }
                else
                {
                    // 没有符号 -
                    movieId = movieId.ToUpper();
                    if ((movieId.StartsWith("N") && movieId.Replace("N", "").All(char.IsDigit)) || (movieId.StartsWith("K") && movieId.Replace("K", "").All(char.IsDigit)))
                    {
                        return VedioType.步兵; //Tokyo
                    }
                    else
                    {
                        movieId = GetFanhaoByRegExp(movieId, "[A-Z][A-Z]+");//至少两个英文字母
                        if (!string.IsNullOrEmpty(movieId))
                        {
                            if (Uncensored.Contains(movieId))
                                return VedioType.步兵;
                            else
                                return 0;
                        }
                        return 0;
                    }

                }
            }
        }

        public static string GetFanhaoByRegExp(string FileName, string myPattern)
        {
            if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(myPattern)) return "";
            MatchCollection mc = Regex.Matches(FileName, myPattern, RegexOptions.IgnoreCase);
            if (mc.Count > 0)
                return mc[0].Value.ToUpper();
            else
                return "";
        }





        //TODO 自定义欧美番号格式
        public static string GetEuFanhao(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            string pattern = @"[A-Za-z]+\.[0-9]{2}\.[0-9]{2}\.[0-9]{2}";
            string FileName = File.Exists(str) ? new FileInfo(str).Name : str;
            //BigWetButts.20.06.16
            MatchCollection mc = Regex.Matches(FileName, pattern, RegexOptions.IgnoreCase);
            if (mc.Count > 0)
                return mc[0].Value;
            else
                return "";
        }




        /// <summary>
        /// 从一个字符串或文件路径找提取出番号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetFanhao(string str)
        {
            // 未解决
            // baidu123.com_SMD-124.mp4
            // feichai9831@第第第第@SKY-227.avi

            string FileName = File.Exists(str) ? new FileInfo(str).Name : str; ;
            FileName = FileName.ToLower();
            string Fanhao;

            Fanhao = GetFanhaoByRegExp(FileName, @"t28(-|_)?\d{3}");
            if (Fanhao != "") return "T28-" + Fanhao.Replace("-", "").Replace("_", "").Substring(3);

            Fanhao = GetFanhaoByRegExp(FileName, @"heyzo\s?\)?\(?_?(hd|lt)?\+?-?_?\d{4}");
            if (Fanhao != "") return AddGang(Fanhao.Replace("hd", "").Replace("lt", "").Replace("_", ""));

            Fanhao = GetFanhaoByRegExp(FileName, @"heydouga(-|_)?\d{4}(-|_)?\d{3,}");
            if (Fanhao != "") return AddGang(Fanhao.Replace("_", ""));


            //TODO FC2数字少于 4 位数没法导入
            if (FileName.IndexOf("fc2") >= 0 || FileName.IndexOf("fc") >= 0)
            {
                Fanhao = GetFanhaoByRegExp(FileName, @"\d{4,}");
                if (Fanhao != "") return "FC2-" + Fanhao;
            }

            //ABCD-S12
            Fanhao = GetFanhaoByRegExp(FileName, @"[a-z]{3,4}-s(-|_)?\d{2,}");
            if (Fanhao != "") return GetFanhaoByRegExp(Fanhao, @"[a-z]{3,4}-s") + GetFanhaoByRegExp(Fanhao, @"\d{2,}");


            Fanhao = GetFanhaoByRegExp(FileName, @"s2m[a-z]{0,2}(-|_)?\d{2,}");
            if (Fanhao != "") return GetFanhaoByRegExp(Fanhao, @"s2m[a-z]{0,2}") + "-" + GetFanhaoByRegExp(Fanhao, @"\d{2,}");

            Fanhao = GetFanhaoByRegExp(FileName, @"cw3d2dbd(-|_)?\d{2,}");
            if (Fanhao != "") return "cw3d2dbd".ToUpper() + "-" + GetFanhaoByRegExp(Fanhao, @"\d{2,}");

            Fanhao = GetFanhaoByRegExp(FileName, @"ibw(-|_)?\d{2,}z?");
            if (Fanhao != "") return "IBW-" + GetFanhaoByRegExp(Fanhao, @"\d{2,}z?");



            //メス豚 000000_000_00
            Fanhao = GetFanhaoByRegExp(FileName, @"(?![0-9])*\d{6}(_|-)\d{3}(_|-)\d{2}(?![0-9])");
            if (Fanhao != "") return Fanhao.Replace("-", "_");

            //一本道 000000_000，中间连接符为 _ 前6位，后3位
            //加勒比 000000-000，中间连接符为 - 前6位，后3位
            Fanhao = GetFanhaoByRegExp(FileName, @"(?![0-9])*\d{6}(_|-)\d{3}(?![0-9])");
            if (Fanhao != "") return Fanhao;

            //天然むすめ 000000_00
            Fanhao = GetFanhaoByRegExp(FileName, @"(?![0-9])*\d{6}(_|-)\d{2}(?![0-9])");
            if (Fanhao != "") return Fanhao.Replace("-", "_");

            Fanhao = GetFanhaoByRegExp(FileName, @"(k|n)\d{4}");
            if (Fanhao != "")
            {
                if (!IsEnglishExistBefore(FileName, Fanhao)) return Fanhao;
            }

            Fanhao = GetFanhaoByRegExp(FileName, @"[A-Za-z]{2,}(-|_)?\d{2,}");
            if (Fanhao != "") return AddGang(Fanhao.Replace("_", "-"));


            //如果番号仍然为空，识别特殊番号
            //XXXX-123X
            Fanhao = GetFanhaoByRegExp(FileName, @"[A-Za-z]{2,}(-|_)?\d+[A-Za-z]");
            if (Fanhao != "") return GetFanhaoByRegExp(Fanhao, @"[A-Za-z]{2,}") + "-" + GetFanhaoByRegExp(Fanhao, @"\d+[A-Za-z]");


            // 1000girl
            //141212-MIO
            Fanhao = GetFanhaoByRegExp(FileName, @"\d+-[A-Za-z]+");
            if (Fanhao != "") return GetFanhaoByRegExp(Fanhao, @"\d+-[A-Za-z]+");

            //C-1234
            Fanhao = GetFanhaoByRegExp(FileName, @"C-\d+");
            if (Fanhao != "") return GetFanhaoByRegExp(Fanhao, @"C-\d+");

            //自定义增加正则
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ScanRe))
            {
                foreach (var item in Properties.Settings.Default.ScanRe.Split(';'))
                {
                    if (item?.Length > 0)
                    {
                        Fanhao = GetFanhaoByRegExp(FileName, item);
                        if (Fanhao != "") return GetFanhaoByRegExp(Fanhao, item);
                    }
                }
            }
            return "";
        }


        public static bool IsEnglishExistBefore(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2)) return false;
            int index = str1.ToUpper().IndexOf(str2.ToUpper());
            if (index <= 0)
                return false;
            else
                return str1[index - 1].IsLetter();
        }

        /// <summary>
        /// 添加连接符 -
        /// </summary>
        /// <param name="Fanhao"></param>
        /// <returns></returns>
        public static string AddGang(string Fanhao)
        {
            if (string.IsNullOrEmpty(Fanhao)) return "";
            MatchCollection mc = Regex.Matches(Fanhao, @"\d+");
            string[] paras = new string[mc.Count + 1];
            paras[0] = GetFanhaoByRegExp(Fanhao, "[A-Za-z]+");
            if (mc.Count > 0)
            {
                for (int i = 0; i < mc.Count; i++) paras[i + 1] = mc[i].Value;
                return string.Join("-", paras);
            }
            else
            {
                return "";
            }
        }
    }
}