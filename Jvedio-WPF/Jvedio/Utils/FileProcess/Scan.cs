using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using QueryEngine;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Permissions;
using Jvedio.Utils;
using System.Text;

namespace Jvedio
{
    public static class Scan
    {
        public static double MinFileSize = Properties.Settings.Default.ScanMinFileSize * 1024 * 1024;//最小文件大小
        public static List<string> FilePattern = new List<string>();//文件格式
        public static List<string> ImagePattern = new List<string>() { ".jpg", ".png", ".jpeg", ".bmp" };

        public static void InitSearchPattern()
        {
            //视频后缀来自 Everything (位置：搜索-管理筛选器-视频-编辑)
            FilePattern = new List<string>();
            string ScanVetioType = Resource_String.ScanVetioType;
            foreach (var item in ScanVetioType.Split(','))
                FilePattern.Add("." + item.ToLower());
        }


        public static double InsertWithNfo(List<string> filepaths, CancellationToken ct, Action<string> messageCallBack = null, bool IsEurope = false)
        {
            if (filepaths == null || filepaths.Count == 0) return 0;
            List<string> nfoPaths = new List<string>();
            List<string> videoPaths = new List<string>();

            foreach (var item in filepaths)
            {
                if (item.ToLower().EndsWith(".nfo"))
                    nfoPaths.Add(item);
                else
                    videoPaths.Add(item);
            }

            //先导入 nfo 再导入视频，避免路径覆盖
            if (Properties.Settings.Default.ScanNfo && nfoPaths.Count > 0)
            {
                Logger.LogScanInfo(Environment.NewLine + "-----【" + DateTime.Now.ToString() + "】-----");
                Logger.LogScanInfo(Environment.NewLine + $"{Jvedio.Language.Resources.ScanNFO} => {nfoPaths.Count}  " + Environment.NewLine);

                double total = 0;
                //导入 nfo 文件
                nfoPaths.ForEach(item =>
                {
                    if (File.Exists(item))
                    {
                        Movie movie = FileProcess.GetInfoFromNfo(item);
                        if (movie != null && !string.IsNullOrEmpty(movie.id))
                        {
                            DataBase.InsertFullMovie(movie);
                            total += 1;
                            Logger.LogScanInfo(Environment.NewLine + $"{Jvedio.Language.Resources.SuccessImportToDataBase} => {item}  ");
                        }

                    }
                });


                Logger.LogScanInfo(Environment.NewLine + $"{Jvedio.Language.Resources.ImportNFONumber}： {total}" + Environment.NewLine);
                messageCallBack?.Invoke($"{Jvedio.Language.Resources.ImportNFONumber}： {total}");



            }


            //导入视频
            if (videoPaths.Count > 0)
            {
                try
                {
                    double _num = DistinctMovieAndInsert(videoPaths, ct, IsEurope);
                    messageCallBack?.Invoke($"{Jvedio.Language.Resources.ImportVideioNumber}：{_num}，详情请看日志");
                    return _num;
                }
                catch (OperationCanceledException ex)
                {
                    Logger.LogF(ex);
                    messageCallBack?.Invoke($"{Jvedio.Language.Resources.Cancel}");
                }
            }
            return 0;
        }




        public static bool IsProperMovie(string FilePath)
        {
            return File.Exists(FilePath) &&
                FilePattern.Contains(System.IO.Path.GetExtension(FilePath).ToLower()) &&
                new System.IO.FileInfo(FilePath).Length >= MinFileSize;
        }




        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static List<string> ScanAllDrives()
        {
            List<string> result = new List<string>();
            try
            {
                var entries = Engine.GetAllFilesAndDirectories();
                entries.ForEach(arg =>
                {
                    if (arg is FileAndDirectoryEntry & !arg.IsFolder)
                    {
                        result.Add(arg.FullFileName);
                    }
                });
            }
            catch (Exception e)
            {
                Logger.LogE(e);
            }

            //扫描根目录
            StringCollection stringCollection = new StringCollection();
            foreach (var item in Environment.GetLogicalDrives())
            {
                try
                {
                    if (Directory.Exists(item)) stringCollection.Add(item);
                }
                catch (Exception e)
                {
                    Logger.LogE(e);
                    continue;
                }
            }
            result.AddRange(ScanTopPaths(stringCollection));
            return FirstFilter(result);
        }


        //根据 视频后缀文件大小筛选
        public static List<string> FirstFilter(List<string> FilePathList, string ID = "")
        {
            if (FilePathList == null || FilePathList.Count == 0) return new List<string>();
            try
            {
                if (ID == "")
                {
                    return FilePathList
                        .Where(s => FilePattern.Contains(Path.GetExtension(s).ToLower()))
                        .Where(s => !File.Exists(s) || new FileInfo(s).Length >= MinFileSize).
                        OrderBy(s => s).ToList();
                }
                else
                {
                    return FilePathList
                        .Where(s => FilePattern.Contains(Path.GetExtension(s).ToLower()))
                        .Where(s => !File.Exists(s) || new FileInfo(s).Length >= MinFileSize)
                        .Where(s => { try { return Identify.GetFanhao(new FileInfo(s).Name).ToUpper() == ID.ToUpper(); } catch { Logger.LogScanInfo($"错误路径：{s}"); return false; } })
                        .OrderBy(s => s).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            return new List<string>();
        }

        public static List<string> ScanTopPaths(StringCollection stringCollection)
        {
            List<string> result = new List<string>();
            if (stringCollection == null || stringCollection.Count == 0) return result;
            foreach (var item in stringCollection)
            {
                try
                {
                    foreach (var path in Directory.GetFiles(item, "*.*", SearchOption.TopDirectoryOnly)) result.Add(path);
                }
                catch { continue; }
            }
            return result;
        }

        public static List<string> GetSubSectionFeature()
        {
            List<string> result = new List<string>();
            string SubSectionFeature = Resource_String.SubSectionFeature;
            string SplitFeature = SubSectionFeature.Replace("，", ",");
            if (SplitFeature.Split(',').Count() > 0)
            {
                foreach (var item in SplitFeature.Split(','))
                {
                    if (!result.Contains(item)) result.Add(item);
                }
            }
            return result;
        }


        /// <summary>
        /// 给出一组视频路径，返回否是分段视频，并返回分段的视频列表，以及不是分段的视频列表
        /// </summary>
        /// <param name="FilePathList"></param>
        /// <returns></returns>

        public static (bool, List<string>, List<string>) IsSubSection(List<string> FilePathList)
        {
            bool result = true;
            List<string> notSubSection = new List<string>();
            if (FilePathList == null || FilePathList.Count == 0) return (false, new List<string>(), new List<string>());
            string FatherPath = new FileInfo(FilePathList[0]).Directory.FullName;
            bool IsAllFileSameFP = FilePathList.All(arg => new FileInfo(arg).Directory.FullName == FatherPath);
            if (!IsAllFileSameFP)
            {
                //并不是所有父目录都相同，提取出父目录最多的文件
                Dictionary<string, int> fatherpathDic = new Dictionary<string, int>();
                FilePathList.ForEach(path =>
                {
                    string fatherPath = new FileInfo(path).Directory.FullName;
                    if (fatherpathDic.ContainsKey(fatherPath))
                        fatherpathDic[fatherPath] += 1;
                    else
                        fatherpathDic.Add(fatherPath, 1);
                });
                string maxValueKey = fatherpathDic.FirstOrDefault(x => x.Value == fatherpathDic.Values.Max()).Key;
                if (!string.IsNullOrEmpty(maxValueKey))
                {
                    try
                    {
                        var notsub = fatherpathDic.Where(arg => arg.Key != null && new FileInfo(arg.Key)?.Directory.FullName != maxValueKey).ToList();
                        notsub.ForEach(arg => notSubSection.Add(arg.Key));
                    }
                    catch (Exception e) { Logger.LogE(e); }
                    FilePathList = FilePathList.Where(arg => new FileInfo(arg).Directory.FullName == maxValueKey).ToList();
                }
            }


            //目录都相同，判断是否分段视频的特征


            // -1  cd1  _1   fhd1  
            string regexFeature = "";
            foreach (var item in GetSubSectionFeature()) { regexFeature += item + "|"; }
            regexFeature = "(" + regexFeature.Substring(0, regexFeature.Length - 1) + ")[1-9]{1}";


            string MatchesName = "";
            foreach (var item in FilePathList)
            {
                foreach (var re in Regex.Matches(item, regexFeature)) { MatchesName += re.ToString(); }
            }

            for (int i = 1; i <= FilePathList.Count; i++)
            {
                result &= MatchesName.IndexOf(i.ToString()) >= 0;
            }


            if (!result)
            {
                result = true;
                //数字后面存在 A,B,C……
                //XXX-000-A  
                //XXX-000-B
                regexFeature = "";
                foreach (var item in GetSubSectionFeature()) { regexFeature += item + "|"; }
                regexFeature = "((" + regexFeature.Substring(0, regexFeature.Length - 1) + ")|[0-9]{1,})[a-n]{1}";

                MatchesName = "";
                foreach (var item in FilePathList)
                {
                    foreach (var re in Regex.Matches(item, regexFeature, RegexOptions.IgnoreCase)) { MatchesName += re.ToString(); }
                }
                MatchesName = MatchesName.ToLower();
                string characters = "abcdefghijklmn";
                for (int i = 0; i < Math.Min(FilePathList.Count, characters.Length); i++) { result &= MatchesName.IndexOf(characters[i]) >= 0; }
            }
            return (result, FilePathList, notSubSection);
        }






        public static List<string> ScanPaths(StringCollection stringCollection, CancellationToken cancellationToken)
        {
            List<string> result = new List<string>();
            if (stringCollection == null || stringCollection.Count == 0) return result;
            foreach (var item in stringCollection)
            {
                try
                {
                    result.AddRange(GetAllFilesFromFolder(item, cancellationToken));
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine(ex.Message);
                    return result;
                }
            }
            return result
                    .Where(s => FilePattern.Contains(System.IO.Path.GetExtension(s).ToLower()))
                    .Where(s => !File.Exists(s) || new System.IO.FileInfo(s).Length >= MinFileSize).OrderBy(s => s).ToList();
        }

        public static List<string> ScanNFO(StringCollection stringCollection, CancellationToken cancellationToken, Action<string> callBack)
        {
            List<string> result = new List<string>();
            if (stringCollection == null || stringCollection.Count == 0) return result;
            foreach (var item in stringCollection)
            {
                try
                {
                    result.AddRange(GetAllFilesFromFolder(item, cancellationToken, "*.*", callBack));
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine(ex.Message);
                    return result.Where(s => Path.GetExtension(s).ToLower().IndexOf("nfo") > 0).ToList();
                }
            }
            return result.Where(s => Path.GetExtension(s).ToLower().IndexOf("nfo") > 0).ToList();
        }




        public static List<string> GetAllFilesFromFolder(string root, CancellationToken cancellationToken, string pattern = "", Action<string> callBack = null)
        {
            Queue<string> folders = new Queue<string>();
            List<string> files = new List<string>();
            folders.Enqueue(root);
            while (folders.Count != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string currentFolder = folders.Dequeue();
                try
                {
                    string[] filesInCurrent = System.IO.Directory.GetFiles(currentFolder, pattern == "" ? "*.*" : pattern, System.IO.SearchOption.TopDirectoryOnly);
                    files.AddRange(filesInCurrent);
                    foreach (var file in filesInCurrent) { callBack?.Invoke(file); }
                }
                catch
                {
                }
                try
                {
                    string[] foldersInCurrent = System.IO.Directory.GetDirectories(currentFolder, pattern == "" ? "*.*" : pattern, System.IO.SearchOption.TopDirectoryOnly);
                    foreach (string _current in foldersInCurrent)
                    {
                        folders.Enqueue(_current);
                        callBack?.Invoke(_current);
                    }
                }
                catch
                {
                }
            }
            return files;
        }



        /// <summary>
        /// 分类视频并导入
        /// </summary>
        /// <param name="MoviePaths"></param>
        /// <param name="ct"></param>
        /// <param name="IsEurope"></param>
        /// <returns></returns>
        public static double DistinctMovieAndInsert(List<string> MoviePaths, CancellationToken ct, bool IsEurope = false)
        {
            Logger.LogScanInfo(Environment.NewLine + "-----【" + DateTime.Now.ToString() + "】-----");
            Logger.LogScanInfo(Environment.NewLine + $"{Jvedio.Language.Resources.ScanVideo} => {MoviePaths.Count} " + Environment.NewLine);

            List<string> properIdList = new List<string>();
            StringBuilder logStr = new StringBuilder();
            string id = "";
            VedioType vt = 0;
            double insertCount = 0;//总的导入数目
            double unidentifyCount = 0;//无法识别的数目

            //检查未识别出番号的视频
            foreach (var item in MoviePaths)
            {
                if (File.Exists(item))
                {
                    id = IsEurope ? Identify.GetEuFanhao(new FileInfo(item).Name) : Identify.GetFanhao(new FileInfo(item).Name);

                    if (IsEurope) { if (string.IsNullOrEmpty(id)) vt = 0; else vt = VedioType.欧美; }
                    else vt = Identify.GetVideoType(id);


                    if (vt != 0) properIdList.Add(item);
                    else
                    {
                        logStr.Append("   " + item + Environment.NewLine);
                        unidentifyCount++;
                    }
                }
            }
            Logger.LogScanInfo(Environment.NewLine + $"【{Jvedio.Language.Resources.NotRecognizeNumber} ：{unidentifyCount}】" + Environment.NewLine + logStr.ToString());

            //检查 重复|分段 视频
            Dictionary<string, List<string>> repeatlist = new Dictionary<string, List<string>>();
            StringBuilder logSubSection = new StringBuilder();
            foreach (var item in properIdList)
            {
                if (File.Exists(item))
                {

                    id = IsEurope ? Identify.GetEuFanhao(new FileInfo(item).Name) : Identify.GetFanhao(new FileInfo(item).Name);
                    if (!repeatlist.ContainsKey(id))
                    {
                        List<string> pathlist = new List<string> { item };
                        repeatlist.Add(id, pathlist);
                    }
                    else
                    {
                        repeatlist[id].Add(item);//每个 id 对应一组视频路径，视频路径最多的视为分段视频
                    }
                }
            }

            List<string> removelist = new List<string>();
            List<List<string>> subsectionlist = new List<List<string>>();
            foreach (KeyValuePair<string, List<string>> kvp in repeatlist)
            {
                if (kvp.Value.Count > 1)
                {
                    //路径个数大于1 才为分段视频
                    (bool issubsection, List<string> filepathlist, List<string> notsubsection) = IsSubSection(kvp.Value);
                    if (issubsection)
                    {
                        subsectionlist.Add(filepathlist);
                        if (filepathlist.Count < kvp.Value.Count)
                        {
                            //其中几个不是分段视频
                            logSubSection.Append($"   {Jvedio.Language.Resources.ID} ：{kvp.Key}" + Environment.NewLine);
                            removelist.AddRange(notsubsection);
                            logSubSection.Append($"      {Jvedio.Language.Resources.ImportSubSection}： {filepathlist.Count} ，：{string.Join(";", filepathlist)}" + Environment.NewLine);
                            notsubsection.ForEach(arg =>
                            {
                                logSubSection.Append($"      {Jvedio.Language.Resources.NotImport} ：{arg}" + Environment.NewLine);
                            });
                        }
                    }
                    else
                    {
                        //TODO
                        logSubSection.Append($"   {Jvedio.Language.Resources.ID}：{kvp.Key}" + Environment.NewLine);
                        (string maxfilepath, List<string> Excludelsist) = ExcludeMaximumSize(kvp.Value);
                        removelist.AddRange(Excludelsist);
                        logSubSection.Append($"      {Jvedio.Language.Resources.ImportFile} ：{maxfilepath}，{Jvedio.Language.Resources.FileSize} ：{new FileInfo(maxfilepath).Length}" + Environment.NewLine);
                        Excludelsist.ForEach(arg =>
                        {
                            logSubSection.Append($"      {Jvedio.Language.Resources.NotImport} ：{arg}，{Jvedio.Language.Resources.FileSize} ：{new FileInfo(arg).Length}" + Environment.NewLine);
                        });
                    }

                }
            }
            Logger.LogScanInfo(Environment.NewLine + $"【 {Jvedio.Language.Resources.RepeatVideo}：{removelist.Count + subsectionlist.Count}】" + Environment.NewLine + logSubSection.ToString());

            List<string> insertList = properIdList.Except(removelist).ToList();//需要导入的视频

            //导入分段视频
            foreach (var item in subsectionlist)
            {
                insertList = insertList.Except(item).ToList();
                ct.ThrowIfCancellationRequested();
                string subsection = "";
                FileInfo fileinfo = new FileInfo(item[0]);//获得第一个视频的文件信息
                id = IsEurope ? Identify.GetEuFanhao(fileinfo.Name) : Identify.GetFanhao(fileinfo.Name);
                if (IsEurope) { if (string.IsNullOrEmpty(id)) continue; else vt = VedioType.欧美; } else { vt = Identify.GetVideoType(id); }
                if (string.IsNullOrEmpty(id) || vt == 0) continue;

                //文件大小视为所有文件之和
                double filesize = 0;
                for (int i = 0; i < item.Count; i++)
                {
                    if (!File.Exists(item[i])) { continue; }
                    FileInfo fi = new FileInfo(item[i]);
                    subsection += item[i] + ";";
                    filesize += fi.Length;
                }

                //获取创建日期
                //TODO 国际化
                string createDate = "";
                try { createDate = fileinfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"); }
                catch { }
                if (createDate == "") createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Movie movie = new Movie()
                {
                    filepath = item[0],
                    id = id,
                    filesize = filesize,
                    vediotype = (int)vt,
                    subsection = subsection.Substring(0, subsection.Length - 1),
                    otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    scandate = createDate
                };

                DataBase.InsertScanMovie(movie);
                insertCount += 1;
            }

            //导入剩余的所有视频
            foreach (var item in insertList)
            {
                ct.ThrowIfCancellationRequested();
                if (!File.Exists(item)) continue;
                FileInfo fileinfo = new FileInfo(item);

                string createDate = "";
                try { createDate = fileinfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"); }
                catch { }
                if (createDate == "") createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                id = IsEurope ? Identify.GetEuFanhao(fileinfo.Name) : Identify.GetFanhao(fileinfo.Name);
                if (string.IsNullOrEmpty(id)) continue;

                if (IsEurope) vt = VedioType.欧美;
                else vt = Identify.GetVideoType(id);

                if (vt == 0) continue;

                Movie movie = new Movie()
                {
                    filepath = item,
                    id = id,
                    filesize = fileinfo.Length,
                    vediotype = (int)vt,
                    otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    scandate = createDate
                };
                DataBase.InsertScanMovie(movie);
                insertCount += 1;
            }

            Logger.LogScanInfo(Environment.NewLine + $"{Jvedio.Language.Resources.TotalImport} => {insertCount}，{Jvedio.Language.Resources.ImportAttention}" + Environment.NewLine);

            //TODO 从主数据库中复制信息
            //从 主数据库中 复制信息
            //if (Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower() != "info")
            //{
            //    try
            //    {
            //        string src = AppDomain.CurrentDomain.BaseDirectory + "DataBase\\info.sqlite";
            //        string dst = AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower()}.sqlite"; ;
            //        DataBase.CopyDatabaseInfo(src, dst);
            //    }
            //    catch { }
            //}
            return insertCount;
        }

        public static (string, List<string>) ExcludeMaximumSize(List<string> pathlist)
        {
            double maxsize = 0;
            int maxsizeindex = 0;
            int i = 0;
            foreach (var item in pathlist)
            {
                if (File.Exists(item))
                {
                    double filesize = new FileInfo(item).Length;
                    if (maxsize < filesize) { maxsize = filesize; maxsizeindex = i; }
                }
                i++;
            }
            string maxsizepth = pathlist[maxsizeindex];
            pathlist.RemoveAt(maxsizeindex);
            return (maxsizepth, pathlist);
        }

    }
}
