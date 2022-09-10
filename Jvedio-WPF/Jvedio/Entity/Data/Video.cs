using SuperUtils.Framework.ORM.Attributes;
using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity.CommonSQL;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using Jvedio.Utils.IO;
using JvedioLib.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Jvedio.Utils.IO;
using SuperUtils.Framework.ORM.Wrapper;

namespace Jvedio.Entity
{

    // todo 检视

    [Table(tableName: "metadata_video")]
#pragma warning disable CS0659 // “Video”重写 Object.Equals(object o) 但不重写 Object.GetHashCode()
    public class Video : MetaData
#pragma warning restore CS0659 // “Video”重写 Object.Equals(object o) 但不重写 Object.GetHashCode()
    {


        public Video() : this(true) { }

        public Video(bool _initDefaultImage = true)
        {
            if (_initDefaultImage)
                InitDefaultImage();
        }

        // 延迟加载图片
        public void InitDefaultImage()
        {
            SmallImage = GlobalVariable.DefaultSmallImage;
            BigImage = GlobalVariable.DefaultBigImage;
            GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
            PreviewImageList = new ObservableCollection<BitmapSource>();
        }

        public static SelectWrapper<Video> InitWrapper()
        {
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("metadata.DBId", GlobalConfig.Main.CurrentDBId)
                .Eq("metadata.DataType", 0);
            return wrapper;
        }

        public static void setTagStamps(ref Video video)
        {
            if (video == null || string.IsNullOrEmpty(video.TagIDs)) return;
            List<long> list = video.TagIDs.Split(',').Select(arg => long.Parse(arg)).ToList();
            if (list != null && list.Count > 0)
            {
                video.TagStamp = new ObservableCollection<TagStamp>();
                foreach (var item in GlobalVariable.TagStamps.Where(arg => list.Contains(arg.TagID)).ToList())
                    video.TagStamp.Add(item);
            }
        }


        public static void handleEmpty(ref Video video)
        {
            if (Properties.Settings.Default.ShowFileNameIfTitleEmpty
                && !string.IsNullOrEmpty(video.Path) && string.IsNullOrEmpty(video.Title))
                video.Title = System.IO.Path.GetFileNameWithoutExtension(video.Path);
            if (Properties.Settings.Default.ShowCreateDateIfReleaseDateEmpty
                && !string.IsNullOrEmpty(video.LastScanDate) && string.IsNullOrEmpty(video.ReleaseDate))
                video.ReleaseDate = DateHelper.toLocalDate(video.LastScanDate);
        }


        [TableId(IdType.AUTO)]
        public long MVID { get; set; }
#pragma warning disable CS0108 // “Video.DataID”隐藏继承的成员“MetaData.DataID”。如果是有意隐藏，请使用关键字 new。
        public long DataID { get; set; }
#pragma warning restore CS0108 // “Video.DataID”隐藏继承的成员“MetaData.DataID”。如果是有意隐藏，请使用关键字 new。
        public string VID { get; set; }

        public string Series { get; set; }
        private VideoType _VideoType;

        public VideoType VideoType
        {
            get { return _VideoType; }
            set
            {
                _VideoType = value;
                OnPropertyChanged();
            }
        }
        public string Director { get; set; }
        public string Studio { get; set; }
        public string Publisher { get; set; }
        public string Plot { get; set; }
        public string Outline { get; set; }
        public int Duration { get; set; }

        [TableField(exist: false)]
        public List<string> SubSectionList { get; set; }

        private string _SubSection = "";
        public string SubSection
        {
            get { return _SubSection; }
            set
            {
                _SubSection = value;
                SubSectionList = value.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (SubSectionList.Count >= 2) HasSubSection = true;
                OnPropertyChanged();
            }
        }
        [TableField(exist: false)]
        public bool HasSubSection { get; set; }


        [TableField(exist: false)]
        public ObservableCollection<string> PreviewImagePathList { get; set; }

        [TableField(exist: false)]
        public ObservableCollection<BitmapSource> PreviewImageList { get; set; }
        public string ImageUrls { get; set; }

        public string WebType { get; set; }
        public string WebUrl { get; set; }
        public string ExtraInfo { get; set; }


        private BitmapSource _smallimage;

        [TableField(exist: false)]
        public BitmapSource SmallImage { get { return _smallimage; } set { _smallimage = value; OnPropertyChanged(); } }

        private BitmapSource _bigimage;
        [TableField(exist: false)]
        public BitmapSource BigImage { get { return _bigimage; } set { _bigimage = value; OnPropertyChanged(); } }
        private Uri _GifUri;

        [TableField(exist: false)]
        public Uri GifUri { get { return _GifUri; } set { _GifUri = value; OnPropertyChanged(); } }


        [TableField(exist: false)]
#pragma warning disable CS0108 // “Video.TagStamp”隐藏继承的成员“MetaData.TagStamp”。如果是有意隐藏，请使用关键字 new。
        public ObservableCollection<TagStamp> TagStamp { get; set; }
#pragma warning restore CS0108 // “Video.TagStamp”隐藏继承的成员“MetaData.TagStamp”。如果是有意隐藏，请使用关键字 new。
        [TableField(exist: false)]
#pragma warning disable CS0108 // “Video.TagIDs”隐藏继承的成员“MetaData.TagIDs”。如果是有意隐藏，请使用关键字 new。
        public string TagIDs { get; set; }
#pragma warning restore CS0108 // “Video.TagIDs”隐藏继承的成员“MetaData.TagIDs”。如果是有意隐藏，请使用关键字 new。



        private string _ActorNames;

        [TableField(exist: false)]
        public string ActorNames
        {
            get { return _ActorNames; }
            set
            {
                _ActorNames = value;
                if (!string.IsNullOrEmpty(value))
                    ActorNameList = value.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();

                OnPropertyChanged();
            }
        }

        [TableField(exist: false)]
        public List<string> ActorNameList { get; set; }

        /// <summary>
        /// 旧数据库的 actorID 列表
        /// </summary>
        [TableField(exist: false)]
        public string OldActorIDs { get; set; }


        private List<ActorInfo> _ActorInfos;

        [TableField(exist: false)]
        public List<ActorInfo> ActorInfos
        {
            get { return _ActorInfos; }
            set
            {
                _ActorInfos = value;
                if (value != null)
                {

                    ActorNames = string.Join(GlobalVariable.Separator.ToString(),
                        value.Select(arg => arg.ActorName).ToList());
                }
                OnPropertyChanged();
            }
        }
        [TableField(exist: false)]
        public List<Magnet> Magnets { get; set; }


        [TableField(exist: false)]
        public bool HasAssociation { get; set; }


        [TableField(exist: false)]
        public List<long> AssociationList { get; set; }


        // 仅用于 NFO 导入的时候的图片地址
        [TableField(exist: false)]
        public List<string> ActorThumbs { get; set; }

        public bool toDownload()
        {
            return string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(WebUrl) || string.IsNullOrEmpty(ImageUrls);
        }

        /// <summary>
        /// ext 必须要带上 '.'
        /// </summary>
        /// <param name="imageType"></param>
        /// <param name="ext">ext 必须要带上 '.'</param>
        /// <returns></returns>
        private string getImagePath(ImageType imageType, string ext = null)
        {
            string result = "";
            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            string basePicPath = GlobalConfig.Settings.PicPaths[pathType.ToString()].ToString();
            if (pathType != PathType.RelativeToData)
            {
                if (pathType == PathType.RelativeToApp)
                    basePicPath = System.IO.Path.Combine(GlobalVariable.CurrentUserFolder, basePicPath);
                string saveDir = "";
                if (imageType == ImageType.Big)
                    saveDir = System.IO.Path.Combine(basePicPath, "BigPic");
                else if (imageType == ImageType.Small)
                    saveDir = System.IO.Path.Combine(basePicPath, "SmallPic");
                else if (imageType == ImageType.Preview)
                    saveDir = System.IO.Path.Combine(basePicPath, "ExtraPic");
                else if (imageType == ImageType.ScreenShot)
                    saveDir = System.IO.Path.Combine(basePicPath, "ScreenShot");
                else if (imageType == ImageType.Gif)
                    saveDir = System.IO.Path.Combine(basePicPath, "Gif");
                if (!Directory.Exists(saveDir)) FileHelper.TryCreateDir(saveDir);
                if (!string.IsNullOrEmpty(VID))
                    result = System.IO.Path.Combine(saveDir, $"{VID}{(string.IsNullOrEmpty(ext) ? "" : ext)}");
                else
                    result = System.IO.Path.Combine(saveDir, $"{Hash}{(string.IsNullOrEmpty(ext) ? "" : ext)}");
            }
            else
            {

            }
            if (!string.IsNullOrEmpty(result))
                return System.IO.Path.GetFullPath(result);
            return "";
        }






        public override string ToString()
        {
            return ClassUtils.toString(this);
        }

        public MetaData toMetaData()
        {
            MetaData metaData = (MetaData)this;
            metaData.DataID = this.DataID;
            return metaData;
        }


        public Dictionary<string, string> ToDictionary()
        {
            List<string> fields = new List<string> { "VideoType", "DataID", "VID", "Size", "Path", "Hash", "DataType" };
            Dictionary<string, string> dict = new Dictionary<string, string>();
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            foreach (PropertyInfo info in propertyInfos)
            {
                if (fields.Contains(info.Name))
                {
                    object value = info.GetValue(this);
                    if (value == null) value = "";
                    dict.Add(info.Name, value.ToString());
                }
            }
            return dict;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Video video = obj as Video;
            return video != null && (video.DataID == this.DataID || video.MVID == this.MVID);
        }

        private static string parseRelativeImageFileName(string path)
        {

            string dirName = System.IO.Path.GetDirectoryName(path);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
            List<string> list = FileHelper.TryGetAllFiles(dirName, "*.*").ToList();
            list = list.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(System.IO.Path.GetExtension(arg).ToLower())).ToList();


            foreach (string item in list)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(item).ToLower().IndexOf(fileName) >= 0)
                    return item;
            }
            return FileHelper.TryGetFullPath(path);
        }

        private static string parseRelativePath(string path)
        {
            string rootDir = System.IO.Path.GetDirectoryName(path);
            List<string> list = DirHelper.TryGetDirList(rootDir).ToList();
            string dirName = System.IO.Path.GetFileName(path);
            foreach (string item in list)
            {
                if (System.IO.Path.GetFileName(item).ToLower().IndexOf(dirName.ToLower()) >= 0)
                    return item;
            }
            return FileHelper.TryGetFullPath(path);
        }

        public string getSmallImage(string ext = ".jpg", bool searchExt = true)
        {
            string smallImagePath = getImagePath(ImageType.Small, ext);
            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path))
            {

                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)GlobalConfig.Settings.PicPaths[pathType.ToString()];
                string smallPath = System.IO.Path.Combine(basePicPath, dict["SmallImagePath"]);
                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(smallPath))) smallPath += ext;
                smallImagePath = parseRelativeImageFileName(smallPath);

            }
            // 替换成其他扩展名
            if (searchExt && !File.Exists(smallImagePath))
                smallImagePath = FileHelper.FindWithExt(smallImagePath, ScanTask.PICTURE_EXTENSIONS_LIST);
            return FileHelper.TryGetFullPath(smallImagePath);
        }

        public string getBigImage(string ext = ".jpg", bool searchExt = true)
        {
            string bigImagePath = getImagePath(ImageType.Big, ext);

            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path))
            {

                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)GlobalConfig.Settings.PicPaths[pathType.ToString()];
                string bigPath = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["BigImagePath"]));
                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(bigPath))) bigPath += ext;
                bigImagePath = parseRelativeImageFileName(bigPath);
            }
            // 替换成其他扩展名
            if (searchExt && !File.Exists(bigImagePath))
                bigImagePath = FileHelper.FindWithExt(bigImagePath, ScanTask.PICTURE_EXTENSIONS_LIST);
            return FileHelper.TryGetFullPath(bigImagePath);
        }

        public string getExtraImage()
        {
            string imagePath = getImagePath(ImageType.Preview);

            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path))
            {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)GlobalConfig.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["PreviewImagePath"]));
                imagePath = parseRelativePath(path);
            }
            return FileHelper.TryGetFullPath(imagePath);
        }

        public string getScreenShot()
        {
            string imagePath = getImagePath(ImageType.ScreenShot);

            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path))
            {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)GlobalConfig.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["ScreenShotPath"]));
                imagePath = parseRelativePath(path);
            }
            return imagePath;
        }
        public string getGifPath()
        {
            string imagePath = getImagePath(ImageType.Gif, ".gif");

            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path))
            {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)GlobalConfig.Settings.PicPaths[pathType.ToString()];
                string path = FileHelper.TryGetFullPath(System.IO.Path.Combine(basePicPath, dict["Gif"]));
                imagePath = parseRelativePath(path);
            }
            return FileHelper.TryGetFullPath(imagePath);
        }

        public bool parseDictInfo(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0) return false;
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            foreach (PropertyInfo info in propertyInfos)
            {
                if (dict.ContainsKey(info.Name))
                {
                    object value = dict[info.Name];
                    if (value == null) continue;
                    if (value is List<string> list)
                    {
                        info.SetValue(this, string.Join(GlobalVariable.Separator.ToString(), list));
                    }
                    else if (value is string str)
                    {
                        if (info.PropertyType == typeof(string))
                        {
                            info.SetValue(this, str);
                        }
                        else if (info.PropertyType == typeof(int))
                        {
                            int.TryParse(str, out int val);
                            info.SetValue(this, val);
                        }
                    }

                }
            }
            // 图片地址
            ImageUrls = parseImageUrlFromDict(dict);
            return true;
        }


        private string parseImageUrlFromDict(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0 || string.IsNullOrEmpty(ImageUrls)) return "";
            Dictionary<string, object> result = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(ImageUrls);
            if (result == null) result = new Dictionary<string, object>();
            if (dict.ContainsKey("SmallImageUrl")) result["SmallImageUrl"] = dict["SmallImageUrl"];
            if (dict.ContainsKey("BigImageUrl")) result["BigImageUrl"] = dict["BigImageUrl"];
            if (dict.ContainsKey("ExtraImageUrl")) result["ExtraImageUrl"] = dict["ExtraImageUrl"];
            if (dict.ContainsKey("ActressImageUrl")) result["ActressImageUrl"] = dict["ActressImageUrl"];
            if (dict.ContainsKey("ActorNames")) result["ActorNames"] = dict["ActorNames"];
            return JsonConvert.SerializeObject(result);
        }


        public void SaveNfo()
        {
            if (!GlobalConfig.Settings.SaveInfoToNFO) return;
            string dir = GlobalConfig.Settings.NFOSavePath;
            bool overrideInfo = GlobalConfig.Settings.OverrideInfo; ;

            string saveName = $"{VID.ToProperFileName()}.nfo";
            if (string.IsNullOrEmpty(VID)) saveName = $"{System.IO.Path.GetFileNameWithoutExtension(Path)}.nfo";

            string saveFileName = "";

            if (Directory.Exists(dir))
                saveFileName = System.IO.Path.Combine(dir, saveName);

            //与视频同路径，视频存在才行
            if (!Directory.Exists(dir) && File.Exists(Path))
                saveFileName = System.IO.Path.Combine(new FileInfo(Path).DirectoryName, saveName);

            if (string.IsNullOrEmpty(saveFileName)) return;
            if (overrideInfo || !File.Exists(saveFileName))
                NFOHelper.SaveToNFO(this, saveFileName);

        }

        public static string ToSqlField(string content)
        {
            if (content == Jvedio.Language.Resources.ID)
            {
                return "VID";
            }
            else if (content == Jvedio.Language.Resources.Title)
            {
                return "Title";
            }
            //else if (content == Jvedio.Language.Resources.TranslatedTitle)
            //{
            //    return "chinesetitle";
            //}
            else if (content == Jvedio.Language.Resources.VideoType)
            {
                return "VideoType";
            }
            else if (content == Jvedio.Language.Resources.Tag)
            {
                return "Series";
            }
            else if (content == Jvedio.Language.Resources.ReleaseDate)
            {
                return "ReleaseDate";
            }
            else if (content == Jvedio.Language.Resources.Year)
            {
                return "ReleaseYear";
            }
            else if (content == Jvedio.Language.Resources.Duration)
            {
                return "Duration";
            }
            else if (content == Jvedio.Language.Resources.Country)
            {
                return "Country";
            }
            else if (content == Jvedio.Language.Resources.Director)
            {
                return "Director";
            }
            else if (content == Jvedio.Language.Resources.Genre)
            {
                return "Genre";
            }
            else if (content == Jvedio.Language.Resources.Label)
            {
                return "Label";
            }
            else if (content == Jvedio.Language.Resources.Actor)
            {
                return "ActorNames";
            }
            else if (content == Jvedio.Language.Resources.Studio)
            {
                return "Studio";
            }
            else if (content == Jvedio.Language.Resources.Rating)
            {
                return "Rating";
            }

            else
            {
                return "";
            }

        }


        public string[] ToFileName()
        {
            bool addTag = GlobalConfig.RenameConfig.AddRenameTag;
            string formatString = GlobalConfig.RenameConfig.FormatString;
            FileInfo fileInfo = new FileInfo(Path);
            string name = System.IO.Path.GetFileNameWithoutExtension(Path);
            string dir = fileInfo.Directory.FullName;
            string ext = fileInfo.Extension;
            string newName = "";
            MatchCollection matches = Regex.Matches(formatString, "\\{[a-zA-Z]+\\}");
            PropertyInfo[] PropertyList = this.GetType().GetProperties();

            if (matches != null && matches.Count > 0)
            {
                newName = formatString;
                foreach (Match match in matches)
                {
                    string property = match.Value.Replace("{", "").Replace("}", "");
                    try
                    {
                        ReplaceWithValue(ref newName, property, PropertyList);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
            }

            //替换掉特殊字符
            foreach (char item in GlobalVariable.BANFILECHAR)
            {
                newName = newName.Replace(item.ToString(), "");
            }
            if (GlobalConfig.RenameConfig.RemoveTitleSpace) newName = newName.Trim();

            if (HasSubSection)
            {
                string[] result = new string[SubSectionList.Count];
                for (int i = 0; i < SubSectionList.Count; i++)
                {
                    if (addTag && Identify.IsCHS(Path))
                        result[i] = System.IO.Path.Combine(dir, $"{newName}-{i + 1}_{Jvedio.Language.Resources.Translated}{ext}");
                    else
                        result[i] = System.IO.Path.Combine(dir, $"{newName}-{i + 1}{ext}");
                }
                return result;
            }
            else
            {
                if (addTag && Identify.IsCHS(Path))
                    return new string[] { System.IO.Path.Combine(dir, $"{newName}_{Jvedio.Language.Resources.Translated}{ext}") };
                else
                    return new string[] { System.IO.Path.Combine(dir, $"{newName}{ext}") };
            }


        }



        private void ReplaceWithValue(ref string result, string property, PropertyInfo[] PropertyList)
        {
            string inSplit = GlobalConfig.RenameConfig.InSplit.Equals("[null]") ? "" : GlobalConfig.RenameConfig.InSplit;
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                if (name == property)
                {
                    object o = item.GetValue(this);
                    if (o != null)
                    {
                        string value = o.ToString();

                        if (property == "ActorNames" || property == "Genre" || property == "Label")
                            value = value.Replace(GlobalVariable.Separator.ToString(), inSplit);

                        if (property == "VideoType")
                        {
                            int v = 0;
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
                                result = result.Remove(idx - 1, 1);
                            }
                            result = result.Replace("{" + property + "}", "");
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


        public static string[] HDV = new string[] { "hd", "high_definition", "high definition", "高清", "2k", "4k", "8k", "16k", "32k" };


        public bool IsHDV()
        {
            return Identify.IsHDV(Size) || Identify.IsHDV(Path) || Genre?.IndexOfAnyString(GlobalVariable.TagStrings_HD) >= 0 ||
                    Series?.IndexOfAnyString(GlobalVariable.TagStrings_HD) >= 0 || Label?.IndexOfAnyString(GlobalVariable.TagStrings_HD) >= 0;
        }

        public bool IsCHS()
        {
            return Identify.IsCHS(Path) || Genre?.IndexOfAnyString(GlobalVariable.TagStrings_Translated) >= 0 ||
                     Series?.IndexOfAnyString(GlobalVariable.TagStrings_Translated) >= 0 || Label?.IndexOfAnyString(GlobalVariable.TagStrings_Translated) >= 0;
        }

    }
}
