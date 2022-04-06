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
using Jvedio.Core.Enums;
using System.Text;
using Jvedio.Entity;
using Jvedio.Utils.Common;

namespace Jvedio
{
    public class ScanHelper
    {
        public static double MinFileSize = Properties.Settings.Default.ScanMinFileSize * 1024 * 1024;//最小文件大小吗，单位 B
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


        public (List<Video> import, List<string> notImport, List<string> failNFO)
            parseMovie(List<string> filepaths, List<string> FileExt, CancellationToken ct, bool insertNFO = true, Action<string> callBack = null, long minFileSize = 0)
        {
            List<Video> import = new List<Video>();
            List<string> failNFO = new List<string>();
            List<string> notImport = new List<string>();

            List<string> nfoPaths = new List<string>();
            List<string> videoPaths = new List<string>();

            if (filepaths != null || filepaths.Count > 0)
            {
                foreach (var item in filepaths)
                {
                    if (item.ToLower().Trim().EndsWith(".nfo"))
                        nfoPaths.Add(item);
                    else
                    {
                        if (FileExt.Contains(Path.GetExtension(item)))
                            videoPaths.Add(item);
                        else
                            notImport.Add(item);
                    }

                }

                // 1. 先识别 nfo 再导入视频，避免路径覆盖
                if (insertNFO && nfoPaths.Count > 0)
                {
                    foreach (var item in nfoPaths)
                    {
                        DetailMovie movie = (DetailMovie)Movie.GetInfoFromNfo(item, minFileSize);
                        if (movie != null)
                        {
                            Video video = movie.toVideo();
                            video.LastScanDate = DateHelper.Now();
                            //video.Hash = Jvedio.Utils.Encrypt.Encrypt.FasterMd5(video.Path);
                            import.Add(video);
                        }

                        else
                            failNFO.Add(item);// 未从 nfo 中识别出有效的信息
                    }
                }


                // 2. 导入视频
                if (videoPaths.Count > 0)
                {
                    try
                    {
                        List<Video> videos = DistinctMovie(videoPaths, ct, callBack);
                        // 检查是否大于给定大小的影片
                        notImport.AddRange(videos.Where(arg => arg.Size < minFileSize).Select(arg => arg.Path));
                        videos.RemoveAll(arg => arg.Size < minFileSize);

                        import.AddRange(videos);
                    }
                    catch (OperationCanceledException ex)
                    {
                        callBack?.Invoke($"{Jvedio.Language.Resources.Cancel}");
                    }
                }
            }

            return (import, notImport, failNFO);
        }




        public static bool IsProperMovie(string FilePath)
        {
            return File.Exists(FilePath) &&
                FilePattern.Contains(System.IO.Path.GetExtension(FilePath).ToLower()) &&
                new System.IO.FileInfo(FilePath).Length >= MinFileSize;
        }




        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public List<string> ScanAllDrives()
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
        public List<string> FirstFilter(List<string> FilePathList, string ID = "")
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
                        .Where(s => { try { return Identify.GetVID(new FileInfo(s).Name).ToUpper() == ID.ToUpper(); } catch { Logger.LogScanInfo($"错误路径：{s}"); return false; } })
                        .OrderBy(s => s).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            return new List<string>();
        }

        public List<string> ScanTopPaths(StringCollection stringCollection)
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

        public static (bool, List<string>, List<string>) HandleSubSection(List<string> FilePathList)
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





        // 必须整合在
        public List<string> ScanPaths(StringCollection stringCollection, CancellationToken cancellationToken)
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

        public List<string> ScanNFO(StringCollection stringCollection, CancellationToken cancellationToken, Action<string> callBack)
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




        public List<string> GetAllFilesFromFolder(string root, CancellationToken cancellationToken, string pattern = "", Action<string> callBack = null)
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


        // todo Europe 影片
        /// <summary>
        /// 分类视频并导入
        /// </summary>
        /// <param name="VideoPaths"></param>
        /// <param name="ct"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public List<Video> DistinctMovie(List<string> VideoPaths, CancellationToken ct, Action<string> callBack)
        {
            List<Video> result = new List<Video>();
            Dictionary<string, List<string>> VIDDict = new Dictionary<string, List<string>>();

            List<string> noVIDList = new List<string>();
            //检查未识别出番号的视频
            foreach (string path in VideoPaths)
            {
                if (!File.Exists(path)) continue;


                string VID = Identify.GetVID(Path.GetFileNameWithoutExtension(path));
                if (string.IsNullOrEmpty(VID))
                {
                    // 无识别码
                    noVIDList.Add(path);
                }
                else
                {
                    // 有识别码
                    //检查 重复或分段的视频
                    if (!VIDDict.ContainsKey(VID))
                    {
                        List<string> pathlist = new List<string> { path };
                        VIDDict.Add(VID, pathlist);
                    }
                    else
                    {
                        VIDDict[VID].Add(path);//每个 VID 对应一组视频路径，视频路径 >=2 的可能为分段视频
                    }
                }

            }

            // 检查分段或者重复的视频
            foreach (string VID in VIDDict.Keys)
            {
                List<string> paths = VIDDict[VID];
                if (paths.Count <= 1)
                {
                    // 仅含有一个路径
                    string path = paths[0];
                    FileInfo fileInfo = new FileInfo(path);// 原生的速度最快
                    Video video = new Video()
                    {
                        Path = path,
                        VID = VID,
                        Size = fileInfo.Length,
                        VideoType = Identify.GetVideoType(VID),
                        FirstScanDate = DateHelper.Now(),
                        CreateDate = DateHelper.toLocalDate(fileInfo.CreationTime),

                        //Hash = Jvedio.Utils.Encrypt.Encrypt.FasterMd5(path),
                    };


                    result.Add(video);
                }
                else
                {
                    //路径个数大于1 才为分段视频
                    (bool issubsection, List<string> subsectionlist, List<string> notsubsection) = HandleSubSection(paths);
                    if (issubsection)
                    {
                        //文件大小视为所有文件之和
                        long size = subsectionlist.Sum(arg => new FileInfo(arg).Length);
                        string subsection = string.Join(GlobalVariable.Separator.ToString(), subsectionlist);

                        string firstPath = subsectionlist[0];
                        FileInfo fileInfo = new FileInfo(firstPath);// 原生的速度最快
                        Video video = new Video()
                        {
                            Path = firstPath,
                            VID = VID,
                            Size = size,
                            VideoType = Identify.GetVideoType(VID),
                            FirstScanDate = DateHelper.Now(),
                            CreateDate = DateHelper.toLocalDate(fileInfo.CreationTime),
                            SubSection = subsection,
                            //Hash = Jvedio.Utils.Encrypt.Encrypt.FasterMd5(firstPath),
                        };
                        result.Add(video);
                    }
                    else
                    {
                        // todo 不是分段视频，但是几个视频的识别码一致，检测一下 hash，判断是否重复
                        // todo 处理 notsubsection

                    }
                }
                ct.ThrowIfCancellationRequested();
            }


            foreach (string path in noVIDList)
            {
                FileInfo fileInfo = new FileInfo(path);// 原生的速度最快

                // 无识别码的视频计算其 Hash
                Video video = new Video()
                {
                    Path = path,
                    VID = "",
                    Size = fileInfo.Length,
                    VideoType = VideoType.Normal,
                    FirstScanDate = DateHelper.Now(),
                    CreateDate = DateHelper.toLocalDate(fileInfo.CreationTime),
                    Hash = Jvedio.Utils.Encrypt.Encrypt.FasterMd5(path),
                };
                result.Add(video);
            }
            return result;
        }

        //public (string, List<string>) ExcludeMaximumSize(List<string> pathlist)
        //{
        //    double maxsize = 0;
        //    int maxsizeindex = 0;

        //    for (int i = 0; i < pathlist.Count; i++)
        //    {
        //        string path = pathlist[i];
        //        if (!File.Exists(path)) continue;
        //        double filesize = new FileInfo(path).Length;
        //        if (maxsize < filesize)
        //        {
        //            maxsize = filesize;
        //            maxsizeindex = i;
        //        }
        //    }
        //    string maxsizepth = pathlist[maxsizeindex];
        //    pathlist.RemoveAt(maxsizeindex);
        //    return (maxsizepth, pathlist);
        //}

    }
}
