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
using Jvedio.Core.Scan;
using JvedioLib;
using JvedioLib.Security;
using Jvedio.Logs;
using Jvedio.Utils.IO;

namespace Jvedio
{
    public class ScanHelper
    {
        public static double MinFileSize { get; set; }          //最小文件大小吗，单位 B
        public static string SubSectionFeature { get; set; }
        public static List<string> FilePattern { get; set; }    //文件格式


        private const int DEFAULT_MIN_FILESIZE = 1 * 1024 * 1024;

        static ScanHelper()
        {
            MinFileSize = DEFAULT_MIN_FILESIZE;
            SubSectionFeature = "-,_,cd,-cd,hd,whole";
            FilePattern = new List<string>();
        }

        public static void InitSearchPattern()
        {
            //视频后缀来自 Everything (位置：搜索-管理筛选器-视频-编辑)
            FilePattern = ScanTask.VIDEO_EXTENSIONS_LIST;
        }


        public (List<Video> import, Dictionary<string, NotImportReason> notImport, List<string> failNFO)
            parseMovie(List<string> filepaths, List<string> FileExt, CancellationToken ct, bool insertNFO = true, Action<string> callBack = null, long minFileSize = 0)
        {
            List<Video> import = new List<Video>();
            List<string> failNFO = new List<string>();
            Dictionary<string, NotImportReason> notImport = new Dictionary<string, NotImportReason>();

            List<string> nfoPaths = new List<string>();
            List<string> videoPaths = new List<string>();

            minFileSize = (long)GlobalConfig.ScanConfig.MinFileSize * 1024 * 1024;
            if (minFileSize < 0) minFileSize = DEFAULT_MIN_FILESIZE;

            if (filepaths != null || filepaths.Count > 0)
            {
                foreach (var item in filepaths)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    string path = item.ToLower().Trim();
                    if (string.IsNullOrEmpty(path)) continue;


                    if (path.EndsWith(".nfo"))
                        nfoPaths.Add(item);
                    else
                    {
                        if (FileExt.Contains(Path.GetExtension(item).ToLower()))
                            videoPaths.Add(item);
                        else
                            notImport.Add(item, NotImportReason.NotInExtension);
                    }

                }

                // 1. 先识别 nfo 再导入视频，避免路径覆盖
                if (insertNFO && nfoPaths.Count > 0)
                {
                    foreach (var item in nfoPaths)
                    {
                        Movie movie = null;
                        try
                        {
                            movie = Movie.GetInfoFromNfo(item);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            continue;
                        }

                        if (movie != null)
                        {
                            Video video = movie.toVideo();
                            video.Path = item;
                            video.LastScanDate = DateHelper.Now();
                            video.Hash = Encrypt.FasterMd5(video.Path);
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
                        List<Video> videos = DistinctMovie(videoPaths, ct, (list) =>
                        {
                            foreach (var key in list.Keys)
                            {
                                notImport.Add(key, list[key]);
                            }

                        });
                        // 检查是否大于给定大小的影片
                        foreach (var item in videos.Where(arg => arg.Size < minFileSize).Select(arg => arg.Path))
                        {
                            if (notImport.ContainsKey(item)) continue;
                            notImport.Add(item, NotImportReason.SizeTooSmall);
                        }

                        videos.RemoveAll(arg => arg.Size < minFileSize);
                        import.AddRange(videos);
                    }
                    catch (OperationCanceledException)
                    {
                        callBack?.Invoke($"{Jvedio.Language.Resources.Cancel}");
                    }
                    catch (Exception ex)
                    {
                        callBack?.Invoke(ex.Message);
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
            bool IsSubSection = true;
            List<string> notSubSection = new List<string>();
            if (FilePathList == null || FilePathList.Count == 0) return (false, new List<string>(), new List<string>());

            string fatherPath = new FileInfo(FilePathList[0]).Directory.FullName;
            bool sameFatherPath = FilePathList.All(arg => fatherPath.ToLower().Equals(FileHelper.TryGetFullName(arg)?.ToLower()));

            if (!sameFatherPath)
            {
                //并不是所有父目录都相同，提取出父目录最多的文件
                Dictionary<string, int> fatherPathDic = new Dictionary<string, int>();
                FilePathList.ForEach(path =>
                {
                    string father_path = new FileInfo(path).Directory.FullName;
                    if (fatherPathDic.ContainsKey(father_path))
                        fatherPathDic[father_path] += 1;
                    else
                        fatherPathDic.Add(father_path, 1);
                });

                object maxKey = CommonDataHelper.TryGetMaxCountKey(fatherPathDic);
                string maxValueKey = maxKey == null ? "" : maxKey.ToString();
                if (!string.IsNullOrEmpty(maxValueKey))
                {
                    try
                    {
                        var notsub = fatherPathDic.Where(arg => arg.Key != null &&
                            !maxValueKey.Equals(FileHelper.TryGetFullName(arg.Key))).ToList();
                        notsub.ForEach(arg => notSubSection.Add(arg.Key));
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                    FilePathList = FilePathList.Where(arg => maxValueKey.Equals(FileHelper.TryGetFullName(arg))).ToList();
                }
            }


            //目录都相同，判断是否分段视频的特征


            // -1  cd1  _1   fhd1  
            StringBuilder builder = new StringBuilder();
            foreach (var item in GetSubSectionFeature())
                builder.Append(item + "|");
            string regexFeature = "(" + builder.ToString().Substring(0, builder.Length - 1) + ")[1-9]{1}";

            builder.Clear();
            List<string> originPaths = FilePathList.Select(arg => arg).ToList();
            FilePathList = originPaths.Select(arg => Path.GetFileNameWithoutExtension(arg).ToLower()).OrderBy(arg => arg).ToList();
            foreach (var item in FilePathList)
                foreach (var re in Regex.Matches(item, regexFeature))
                    builder.Append(re.ToString());

            string MatchesName = builder.ToString();
            for (int i = 1; i <= FilePathList.Count; i++)
            {
                IsSubSection &= MatchesName.IndexOf(i.ToString()) >= 0;
            }


            if (!IsSubSection)
            {
                IsSubSection = true;
                //数字后面存在 A,B,C……
                //XXX-000-A  
                //XXX-000-B
                builder.Clear();
                foreach (var item in GetSubSectionFeature())
                    builder.Append(item + "|");

                regexFeature = "((" + builder.ToString().Substring(0, builder.Length - 1) + ")|[0-9]{1,})[a-n]{1}";


                builder.Clear();
                foreach (var item in FilePathList)
                    foreach (var re in Regex.Matches(item, regexFeature, RegexOptions.IgnoreCase))
                        builder.Append(re.ToString());
                MatchesName = builder.ToString().ToLower();

                string characters = "abcdefghijklmn";
                for (int i = 0; i < Math.Min(FilePathList.Count, characters.Length); i++)
                    IsSubSection &= MatchesName.IndexOf(characters[i]) >= 0;

            }


            // 排序文件名
            originPaths = originPaths.OrderBy(arg => arg).ToList();
            if (!IsSubSection)
                return (IsSubSection, new List<string>(), originPaths);

            return (IsSubSection, originPaths, notSubSection);
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
        public List<Video> DistinctMovie(List<string> VideoPaths, CancellationToken ct, Action<Dictionary<string, NotImportReason>> sameVideoCallBack)
        {
            List<Video> result = new List<Video>();
            Dictionary<string, List<string>> VIDDict = new Dictionary<string, List<string>>();

            string sep = GlobalVariable.Separator.ToString();

            List<string> noVIDList = new List<string>();
            //检查无识别码的视频
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
                    // 一个 VID 对应 一个路径
                    string path = paths[0];

                    result.Add(ParseVideo(VID, path));
                }
                else
                {
                    // 一个 VID 对应多个路径
                    //路径个数大于1 才为分段视频
                    (bool isSubSection, List<string> subSectionList, List<string> notSubSection) = (false, new List<string>(), new List<string>());
                    try
                    {
                        (isSubSection, subSectionList, notSubSection) = HandleSubSection(paths);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("解析分段视频错误");
                        Logger.Error(ex);
                    }

                    if (isSubSection)
                    {
                        //文件大小视为所有文件之和
                        long size = subSectionList.Sum(arg => FileHelper.TryGetFileLength(arg));
                        string subsection = string.Join(sep, subSectionList);
                        string firstPath = subSectionList[0];
                        result.Add(ParseVideo(VID, firstPath, subsection, size));
                    }
                    else
                    {
                        // todo 不是分段视频，但是几个视频的识别码一致，检测一下 hash，判断是否重复
                        // 重复的视频
                        // 仅导入最大的视频
                        int maxIndex = 0;
                        long maxLenght = 0;
                        for (int i = 0; i < notSubSection.Count; i++)
                        {
                            string path = notSubSection[i];
                            long len = FileHelper.TryGetFileLength(path);
                            if (len > maxLenght)
                            {
                                maxLenght = len;
                                maxIndex = i;
                            }

                        }


                        Dictionary<string, NotImportReason> list = new Dictionary<string, NotImportReason>();
                        for (int i = 0; i < notSubSection.Count; i++)
                        {
                            string path = notSubSection[i];
                            if (i == maxIndex) continue;
                            list.Add(path, NotImportReason.RepetitiveVID);
                        }
                        if (notSubSection.Count > 0)
                            result.Add(ParseVideo(VID, notSubSection[maxIndex]));
                        sameVideoCallBack?.Invoke(list);
                    }
                }
                ct.ThrowIfCancellationRequested();
            }


            foreach (string path in noVIDList)
            {
                result.Add(ParseVideo("", path, calcHash: true));
            }
            return result;
        }



        private Video ParseVideo(string VID, string path, string subsection = "", long size = -1, bool calcHash = false)
        {
            FileInfo fileInfo = new FileInfo(path);// 原生的速度最快
            Video video = new Video()
            {
                Path = path,
                VID = VID,
                Size = fileInfo.Length,
                VideoType = (VideoType)Identify.GetVideoType(VID),
                FirstScanDate = DateHelper.Now(),
                CreateDate = DateHelper.toLocalDate(fileInfo.CreationTime),
            };
            if (size >= 0) video.Size = size;
            if (!string.IsNullOrEmpty(subsection)) video.SubSection = subsection;
            if (calcHash) video.Hash = Encrypt.FasterMd5(path);
            return video;
        }

    }
}
