using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper;
using Jvedio.Windows;
using Newtonsoft.Json;
using SuperControls.Style.Windows;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Upgrade
{
    // 【重点】

    [Obsolete]
    public static class Jvedio4ToJvedio5
    {
        public static void MoveScanPathConfig(string[] files)
        {
            string scanPathConfig = Path.Combine(PathManager.oldDataPath, "ScanPathConfig");
            if (!File.Exists(scanPathConfig))
                return;

            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            foreach (string file in files) {
                if (string.IsNullOrEmpty(file))
                    continue;
                string name = Path.GetFileNameWithoutExtension(file);
                System.Collections.Specialized.StringCollection collection = null;
                try {
                    collection = new ScanPathConfig(name).Read();
                    if (collection == null || collection.Count == 0)
                        continue;
                    List<string> list = collection.Cast<string>().ToList();
                    dict.Add(name, list);
                } catch (Exception ex) {
                    Logger.Error(ex);
                    continue;
                }
            }

            List<AppDatabase> appDatabases = null;
            try {
                appDatabases = appDatabaseMapper.SelectList();
            } catch (Exception ex) {
                Logger.Error(ex);
                return;
            }

            foreach (string name in dict.Keys) {
                if (string.IsNullOrEmpty(name) || dict[name].Count <= 0)
                    continue;
                AppDatabase db = appDatabases.Where(arg => !string.IsNullOrEmpty(arg.Name) &&
                    name.ToLower().Equals(arg.Name.ToLower())).FirstOrDefault();
                if (db == null)
                    continue;

                List<string> list = dict[name];
                if (list == null || list.Count <= 0)
                    continue;
                string json = JsonConvert.SerializeObject(list);
                db.ScanPath = json;
                try {
                    appDatabaseMapper.UpdateById(db);
                } catch (Exception ex) {
                    Logger.Error(ex);
                    continue;
                }
            }
        }

        public static void MoveRecentWatch()
        {
            string recentWatchPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RecentWatch");
            if (!File.Exists(recentWatchPath))
                return;
            RecentWatchedConfig recentWatchedConfig = new RecentWatchedConfig();
            Dictionary<DateTime, List<string>> dict = null;
            try {
                dict = recentWatchedConfig.Read();
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            if (dict == null || dict.Count <= 0)
                return;

            foreach (DateTime key in dict.Keys) {
                List<string> list = dict[key];
                if (list != null && list.Count > 0) {
                    string sql = $"update metadata set ViewDate = '{key.ToLocalDate()}' " +
                                "where DataID in (select DataID from metadata_video " +
                                $"where VID in ('{string.Join("','", list)}')); ";
                    metaDataMapper.ExecuteNonQuery(sql);
                }
            }
        }

        private static void setProgress(float current, string logText)
        {
            App.Current.Dispatcher.Invoke(() => {
                if (window_Progress != null) {
                    window_Progress.MainProgress = current;
                    window_Progress.LogText = logText;
                }
            });
        }

        private static Window_Progress window_Progress;

        public static async Task<bool> MoveDatabases(string[] files)
        {
            if (files == null || files.Length == 0)
                return true;
            bool result = false;
            window_Progress = new Window_Progress("迁移数据", logText: string.Empty);

            // 不等待
            Task.Run(() => { App.Current.Dispatcher.Invoke(() => { window_Progress.ShowDialog(); }); });
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                if (!File.Exists(file))
                    continue;
                result = await MoveOldData(file, (err) => {
                    MsgBox.Show(err);
                });
                setProgress((100 * (float)i + 1) / (float)files.Length, $"迁移数据：{Path.GetFileName(file)}");
            }

            window_Progress.Close();
            return result;
        }

        /// <summary>
        /// 移动旧数据库到新数据库
        /// </summary>
        /// <remarks>迁移 4.6.1.1 之前的数据</remarks>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static async Task<bool> MoveOldData(string origin, Action<string> errorCallBack = null)
        {
            return await Task.Run(() => {
                MySqlite oldSqlite = null;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Logger.Info($"=======开始迁移数据：{Path.GetFileName(origin)}=========");
                string dbName = "ja";
                try {
                    oldSqlite = new MySqlite(origin);
                } catch (Exception ex) {
                    Logger.Error(ex);
                    return false;
                }

                if (oldSqlite == null)
                    return false;

                // 1. 迁移 Code
                dbName += "vdb";
                bool dbExist = oldSqlite.IsTableExist(dbName);
                if (dbExist) {
                    System.Data.SQLite.SQLiteDataReader db = null;
                    try {
                        db = oldSqlite.RunSql("select * from " + dbName);
                    } catch (Exception ex) {
                        Logger.Error(ex);
                    }

                    if (db != null) {
                        List<UrlCode> urlCodes = new List<UrlCode>();
                        while (db.Read()) {
                            try {
                                UrlCode urlCode = new UrlCode() {
                                    // todo 列可能不存在
                                    LocalValue = db["id"].ToString(),
                                    RemoteValue = db["code"].ToString(),
                                    WebType = "db",
                                    ValueType = "video",
                                };
                                urlCodes.Add(urlCode);
                            } catch (Exception ex) {
                                Logger.Error(ex);
                                continue;
                            }
                        }

                        urlCodeMapper.InsertBatch(urlCodes);
                    }

                    db?.Close();
                }

                dbExist = oldSqlite.IsTableExist("library");
                if (dbExist) {
                    System.Data.SQLite.SQLiteDataReader library = null;
                    try {
                        library = oldSqlite.RunSql("select * from library");
                    } catch (Exception ex) {
                        Logger.Error(ex);
                    }

                    if (library != null) {
                        List<UrlCode> urlCodes = new List<UrlCode>();
                        while (library.Read()) {
                            try {
                                UrlCode urlCode = new UrlCode() {
                                    LocalValue = library["id"].ToString(),
                                    RemoteValue = library["code"].ToString(),
                                    WebType = "library",
                                    ValueType = "video",
                                };
                                urlCodes.Add(urlCode);
                            } catch (Exception ex) {
                                Logger.Error(ex);
                                continue;
                            }
                        }

                        urlCodeMapper.InsertBatch(urlCodes);
                    }

                    library?.Close();
                }

                Logger.Info($"urlCodeMapper 用时：{watch.ElapsedMilliseconds} ms");
                watch.Restart();

                // 2. 迁移 actress
                dbExist = oldSqlite.IsTableExist("actress");
                if (dbExist) {
                    System.Data.SQLite.SQLiteDataReader actressReader = null;
                    try {
                        actressReader = oldSqlite.RunSql("select * from actress");
                    } catch (Exception ex) {
                        Logger.Error(ex);
                    }

                    if (actressReader != null) {
                        List<ActorInfo> actressList = new List<ActorInfo>();
                        while (actressReader.Read()) {
                            try {
                                Actress actress = new Actress();
                                actress.birthday = actressReader["birthday"].ToString();
                                actress.id = actressReader["id"].ToString();
                                actress.name = actressReader["name"].ToString();
                                int.TryParse(actressReader["age"].ToString(), out int age);
                                actress.age = age;
                                int.TryParse(actressReader["height"].ToString(), out int height);
                                actress.height = height;
                                int.TryParse(actressReader["chest"].ToString(), out int chest);
                                actress.chest = chest;
                                int.TryParse(actressReader["waist"].ToString(), out int waist);
                                actress.waist = waist;
                                int.TryParse(actressReader["hipline"].ToString(), out int hipline);
                                actress.hipline = hipline;

                                actress.cup = actressReader["cup"].ToString();
                                actress.birthplace = actressReader["birthplace"].ToString();
                                actress.hobby = actressReader["hobby"].ToString();
                                actress.source = actressReader["source"].ToString();
                                actress.sourceurl = actressReader["sourceurl"].ToString();
                                actress.imageurl = actressReader["imageurl"].ToString();
                                ActorInfo actorInfo = actress.toActorInfo();
                                actressList.Add(actorInfo);
                            } catch (Exception e) {
                                Logger.Error(e);
                                continue;
                            }
                        }

                        actorMapper.InsertBatch(actressList);
                    }

                    actressReader?.Close();
                }

                Logger.Info($"actorMapper 用时：{watch.ElapsedMilliseconds} ms");
                watch.Restart();

                // 3. 迁移 movie
                dbExist = oldSqlite.IsTableExist("movie");
                List<Video> videos = new List<Video>();
                if (dbExist) {
                    double total_count = oldSqlite.SelectCountByTable("movie");

                    // 新建库
                    AppDatabase appDatabase = new AppDatabase();
                    appDatabase.Name = Path.GetFileNameWithoutExtension(origin);
                    appDatabase.Count = (long)total_count;
                    appDatabase.DataType = DataType.Video;
                    try {
                        appDatabaseMapper.Insert(appDatabase);
                    } catch (Exception e) {
                        // 库创建失败了
                        errorCallBack?.Invoke($"库 {appDatabase.Name} 创建失败");
                        Logger.Error(e);
                        return false;
                    }

                    System.Data.SQLite.SQLiteDataReader sr = null;
                    try {
                        sr = oldSqlite.RunSql("select * from movie");
                    } catch (Exception ex) {
                        Logger.Error(ex);
                    }

                    if (sr != null) {
                        List<DetailMovie> detailMovies = new List<DetailMovie>();
                        while (sr.Read()) {
                            try {
                                DetailMovie detailMovie = new DetailMovie() {
                                    id = sr["id"].ToString(),
                                    title = sr["title"].ToString(),
                                    filepath = sr["filepath"].ToString(),
                                    subsection = sr["subsection"].ToString(),
                                    scandate = sr["scandate"].ToString(),
                                    releasedate = sr["releasedate"].ToString(),
                                    director = sr["director"].ToString(),
                                    genre = sr["genre"].ToString(),
                                    tag = sr["tag"].ToString(),
                                    actor = sr["actor"].ToString(),
                                    actorid = sr["actorid"].ToString(),
                                    studio = sr["studio"].ToString(),
                                    chinesetitle = sr["chinesetitle"].ToString(),
                                    label = sr["label"].ToString(),
                                    plot = sr["plot"].ToString(),
                                    outline = sr["outline"].ToString(),
                                    country = sr["country"].ToString(),
                                    otherinfo = sr["otherinfo"].ToString(),
                                    actressimageurl = sr["actressimageurl"].ToString(),
                                    smallimageurl = sr["smallimageurl"].ToString(),
                                    bigimageurl = sr["bigimageurl"].ToString(),
                                    extraimageurl = sr["extraimageurl"].ToString(),
                                    sourceurl = sr["sourceurl"].ToString(),
                                    source = sr["source"].ToString(),
                                };

                                detailMovie.DBId = appDatabase.DBId;

                                double.TryParse(sr["filesize"].ToString(), out double filesize);
                                int.TryParse(sr["vediotype"].ToString(), out int vediotype);
                                int.TryParse(sr["visits"].ToString(), out int visits);
                                int.TryParse(sr["favorites"].ToString(), out int favorites);
                                int.TryParse(sr["year"].ToString(), out int year);
                                int.TryParse(sr["countrycode"].ToString(), out int countrycode);
                                int.TryParse(sr["runtime"].ToString(), out int runtime);
                                float.TryParse(sr["rating"].ToString(), out float rating);
                                detailMovie.filesize = filesize;
                                detailMovie.vediotype = vediotype;
                                detailMovie.visits = visits;
                                detailMovie.rating = rating;
                                detailMovie.favorites = favorites;
                                detailMovie.year = year;
                                detailMovie.countrycode = countrycode;
                                detailMovie.runtime = runtime;
                                detailMovies.Add(detailMovie);
                            } catch (Exception ex) {
                                Logger.Error(ex);
                                continue;
                            }
                        }

                        long before = 0;
                        try {
                            before = metaDataMapper.SelectCount();
                        } catch (Exception ex) {
                            Logger.Error(ex);
                            return false;
                        }

                        try {
                            metaDataMapper.InsertBatch(detailMovies.Select(item => item.toMetaData()).ToList());
                        } catch (Exception ex) {
                            Logger.Error(ex);
                            return false;
                        }

                        Logger.Info($"metaDataMapper 用时：{watch.ElapsedMilliseconds} ms");

                        watch.Restart();
                        detailMovies.ForEach(arg => {
                            before++;
                            Video video = arg.toVideo();
                            video.DataID = before;
                            video.Path = arg.filepath;
                            video.ActorNames = arg.actor;
                            video.OldActorIDs = arg.actorid;
                            video.Size = (long)arg.filesize;
                            video.Genre = arg.genre.Replace(' ', SuperUtils.Values.ConstValues.Separator);
                            video.Series = arg.tag.Replace(' ', SuperUtils.Values.ConstValues.Separator);
                            video.Label = arg.label.Replace(' ', SuperUtils.Values.ConstValues.Separator);
                            videos.Add(video);
                        });

                        try {
                            videoMapper.InsertBatch(videos);
                        } catch (Exception ex) {
                            Logger.Error(ex);
                            return false;
                        }

                        Logger.Info($"videoMapper 用时：{watch.ElapsedMilliseconds} ms");
                        watch.Restart();

                        try {
                            handleChinesetitle(detailMovies);
                            handleActor(videos);
                            handleLabel(videos);
                        } catch (Exception ex) {
                            Logger.Error(ex);
                        }

                        Logger.Info($"handleActor 用时：{watch.ElapsedMilliseconds} ms");
                        watch.Restart();
                    }

                    sr?.Close();
                }

                oldSqlite.CloseDB();

                watch.Stop();
                List<string> list = new List<string>();
                foreach (Video video in videos) {
                    // 高清
                    if (video.IsHDV())
                        list.Add($"({video.DataID},1)");

                    // 中文
                    if (video.IsCHS())
                        list.Add($"({video.DataID},2)");
                }

                if (list.Count > 0) {
                    string sql = $"insert into metadata_to_tagstamp (DataID,TagID) values {string.Join(",", list)}";

                    videoMapper.ExecuteNonQuery(sql);
                }

                return true;
            });
        }

        private static void handleChinesetitle(List<DetailMovie> list)
        {
            for (int i = 0; i < list.Count; i++) {
                string chinesetitle = list[i].chinesetitle;
                string title = list[i].title;
                if (string.IsNullOrEmpty(chinesetitle) || string.IsNullOrEmpty(title))
                    continue;
                TranslationMapper mapper = new TranslationMapper();
                Translation translation = new Translation();
                translation.SourceLang = Jvedio.Core.Enums.Language.Japanese.ToString();
                translation.TargetLang = Jvedio.Core.Enums.Language.Chinese.ToString();
                translation.SourceText = title;
                translation.TargetText = chinesetitle;
                translation.Platform = Jvedio.Core.Enums.TranslationPlatform.youdao.ToString();
                mapper.Insert(translation);
                string sql = "insert into metadata_to_translation(DataID,FieldType,TransaltionID) " +
                    $"values ({i + 1},'Title',{translation.TranslationID})";
                metaDataMapper.ExecuteNonQuery(sql);
            }
        }

        public static void handleLabel(List<Video> list)
        {
            StringBuilder builder = new StringBuilder();
            HashSet<string> labelSet = new HashSet<string>();
            Dictionary<long, List<string>> label_dict = new Dictionary<long, List<string>>();
            foreach (Video video in list) {
                string lab = video.Label;
                if (string.IsNullOrEmpty(lab))
                    continue;
                List<string> labels = lab.Split(new char[] { ' ', SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries).Select(arg => arg.Trim()).ToList();
                if (labels.Count <= 0)
                    continue;
                labelSet.UnionWith(labels);
                label_dict.Add(video.DataID, labels);
            }

            List<string> dataId_to_LabelID = new List<string>();
            foreach (long dataID in label_dict.Keys) {
                List<string> labels = label_dict[dataID];
                foreach (string label in labels) {
                    dataId_to_LabelID.Add($"({dataID},'{label}')");
                }
            }

            if (dataId_to_LabelID.Count > 0) {
                string insert_sql =
                $"insert or replace into metadata_to_label(DataID,LabelName) values {string.Join(",", dataId_to_LabelID)}";
                metaDataMapper.ExecuteNonQuery(insert_sql);
            }
        }

        private static void handleActor(List<Video> list)
        {
            // 新增不存在的
            List<ActorInfo> actorInfos = actorMapper.SelectList();
            HashSet<string> names = list.Select(x => x.ActorNames).ToHashSet(); // 演员名字
            HashSet<string> to_insert = new HashSet<string>();
            foreach (string name in names) {
                to_insert.UnionWith(name.Split(new char[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet());
            }

            HashSet<string> hashSet = actorInfos.Select(x => x.ActorName).ToHashSet();
            to_insert.ExceptWith(hashSet);
            if (to_insert.Count > 0) {
                string sql = $"insert into actor_info(WebType,Gender,ActorName) values ('bus',1,'{string.Join("'),('bus',1,'", to_insert)}')";
                actorMapper.ExecuteNonQuery(sql);
            }

            actorInfos = actorMapper.SelectList();

            // Dictionary<string, long> dict = actorInfos.ToDictionary(x => x.ActorName, x => x.ActorID);
            Dictionary<string, long> dict = actorInfos.ToLookup(x => x.ActorName, y => y.ActorID).ToDictionary(x => x.Key, x => x.First());
            List<UrlCode> urlCodes = new List<UrlCode>();

            List<string> insert_list = new List<string>();
            long count = metaDataMapper.SelectCount();
            for (int i = 0; i < list.Count; i++) {
                string actor = list[i].ActorNames; // 演员A 演员B 演员C
                string actorid = list[i].OldActorIDs;
                if (string.IsNullOrEmpty(actor))
                    continue;
                UrlCode urlCode = new UrlCode();
                string[] actorNames = actor.Split(new char[] { '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string[] actorIds = list[i].OldActorIDs.Split(new char[] { '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (actorIds.Length == actorNames.Length) {
                    // 如果 id 和名字数量一样，插入  urlCode
                    for (int j = 0; j < actorNames.Length; j++) {
                        string actorName = actorNames[j];
                        if (string.IsNullOrEmpty(actorName))
                            continue;
                        urlCode.LocalValue = actorName;
                        urlCode.RemoteValue = actorIds[j];
                        urlCode.WebType = list[i].WebType;
                        urlCode.ValueType = "actor";
                        urlCodes.Add(urlCode);
                    }
                }

                for (int j = 0; j < actorNames.Length; j++) {
                    string actorName = actorNames[j];
                    if (!dict.ContainsKey(actorName))
                        continue;
                    insert_list.Add($"({dict[actorName]},{list[i].DataID})");
                }
            }

            urlCodeMapper.InsertBatch(urlCodes, InsertMode.Ignore);

            if (insert_list.Count > 0) {
                string sql = $"insert or ignore into metadata_to_actor(ActorID,DataID) " +
                    $"values {string.Join(",", insert_list)};";
                metaDataMapper.ExecuteNonQuery(sql);
            }
        }

        public static void MoveAI()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AI.sqlite");
            if (!File.Exists(origin))
                return;
            MySqlite oldSqlite = null;
            System.Data.SQLite.SQLiteDataReader sr = null;
            try {
                oldSqlite = new MySqlite(origin);
                sr = oldSqlite.RunSql("select * from baidu");
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            if (oldSqlite == null || sr == null)
                return;
            List<AIFaceInfo> list = new List<AIFaceInfo>();
            while (sr.Read()) {
                try {
                    AIFaceInfo faceInfo = new AIFaceInfo() {
                        Expression = sr["expression"]?.ToString(),
                        FaceShape = sr["face_shape"]?.ToString(),
                        Race = sr["race"]?.ToString(),
                        Emotion = sr["emotion"]?.ToString(),
                    };

                    int.TryParse(sr["age"].ToString(), out int age);
                    int.TryParse(sr["glasses"].ToString(), out int glasses);
                    int.TryParse(sr["mask"].ToString(), out int mask);
                    float.TryParse(sr["beauty"].ToString(), out float beauty);

                    Enum.TryParse(sr["gender"].ToString(), out Gender gender);

                    faceInfo.Age = age;
                    faceInfo.Beauty = beauty;
                    faceInfo.Gender = gender;
                    faceInfo.Glasses = glasses != 0;
                    faceInfo.Mask = mask != 0;
                    faceInfo.Platform = "baidu";
                    list.Add(faceInfo);
                } catch (Exception ex) {
                    Logger.Error(ex);
                    continue;
                }
            }

            try {
                aIFaceMapper.InsertBatch(list);
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            sr.Close();
            oldSqlite.CloseDB();
        }

        public static void MoveMagnets()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Magnets.sqlite");
            if (!File.Exists(origin))
                return;
            MySqlite oldSqlite = null;
            System.Data.SQLite.SQLiteDataReader sr = null;

            try {
                oldSqlite = new MySqlite(origin);
                sr = oldSqlite.RunSql("select * from magnets");
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            if (oldSqlite == null || sr == null)
                return;

            List<Magnet> magnets = new List<Magnet>();
            HashSet<string> set = new HashSet<string>();
            while (sr.Read()) {
                try {
                    Magnet magnet = new Magnet() {
                        MagnetLink = sr["link"].ToString(),
                        Title = sr["title"].ToString(),
                        Releasedate = sr["releasedate"].ToString(),
                        Tag = sr["tag"].ToString().Replace(' ', SuperUtils.Values.ConstValues.Separator),
                        VID = sr["id"].ToString(),
                    };
                    set.Add(magnet.VID);
                    float.TryParse(sr["size"].ToString(), out float size);
                    magnet.Size = (long)(size * 1024 * 1024);
                    magnets.Add(magnet);
                } catch (Exception ex) {
                    Logger.Error(ex);
                    continue;
                }
            }

            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Select("VID", "DataID").In("VID", set);
            List<Video> videos = null;
            try {
                videos = videoMapper.SelectList(wrapper);
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            Dictionary<string, long> dict = new Dictionary<string, long>();
            try {
                if (videos != null && videos.Count > 0)
                    dict = videos.ToLookup(x => x.VID, y => y.DataID).ToDictionary(x => x.Key, x => x.First());
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            for (int i = 0; i < magnets.Count; i++) {
                string vid = magnets[i].VID;
                if (dict.ContainsKey(vid))
                    magnets[i].DataID = dict[vid];
            }

            try {
                magnetsMapper.InsertBatch(magnets);
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            sr.Close();
            oldSqlite.CloseDB();
        }

        public static void MoveSearchHistory()
        {
            // string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SearchHistory");
            // if (!File.Exists(origin)) return;
            // string data = FileHelper.TryReadFile(origin);
            // if (!string.IsNullOrEmpty(data))
            // {
            //    List<SearchHistory> histories = new List<SearchHistory>();
            //    List<string> VID_list = data.Split(new char[] { '\'' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //    foreach (string VID in VID_list)
            //    {
            //        if (string.IsNullOrEmpty(VID)) continue;
            //        SearchHistory history = new SearchHistory();
            //        history.SearchField = SearchField.video;
            //        history.SearchValue = VID;
            //        histories.Add(history);
            //    }
            //    try { searchHistoryMapper.InsertBatch(histories); }
            //    catch (Exception ex)
            //    {
            //        Logger.Error(ex);
            //    }
            // }
        }

        /// <summary>
        /// 清单和 Label 合并，统一为 Label
        /// </summary>
        public static void MoveMyList()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mylist.sqlite");
            if (!File.Exists(origin))
                return;
            MySqlite oldSqlite = null;
            System.Data.SQLite.SQLiteDataReader sr = null;
            try {
                oldSqlite = new MySqlite(origin);
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            if (oldSqlite == null)
                return;

            List<string> tables = oldSqlite.GetAllTable();

            Dictionary<string, List<string>> datas = new Dictionary<string, List<string>>();
            foreach (string table in tables) {
                if (string.IsNullOrEmpty(table))
                    continue;
                List<string> list = new List<string>();
                try {
                    sr = oldSqlite.RunSql($"select * from {table}");
                    if (sr == null)
                        continue;
                    while (sr.Read()) {
                        try {
                            list.Add(sr.GetString(0));
                        } catch (Exception ex) {
                            Logger.Error(ex);
                            continue;
                        }
                    }

                    datas.Add(table, list);
                } catch (Exception ex) {
                    Logger.Error(ex);
                    continue;
                } finally {
                    sr?.Close();
                    sr = null;
                }
            }

            oldSqlite.CloseDB();

            HashSet<string> set = new HashSet<string>();
            foreach (string key in datas.Keys) {
                set.UnionWith(datas[key]);
            }

            SelectWrapper<Video> selectWrapper = new SelectWrapper<Video>();
            selectWrapper.Select("VID", "DataID").In("VID", set);
            List<Video> videos = null;
            Dictionary<string, long> dict = new Dictionary<string, long>();
            try {
                videos = videoMapper.SelectList(selectWrapper);
                if (videos != null && videos.Count > 0)
                    dict = videos.ToLookup(t => t.VID, t => t.DataID).ToDictionary(t => t.Key, t => t.First());
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            if (dict.Count > 0) {
                foreach (string key in datas.Keys) {
                    List<string> id_list = datas[key];
                    string labelName = key;
                    List<string> values = new List<string>();
                    foreach (string vID in id_list) {
                        if (string.IsNullOrEmpty(vID) || !dict.ContainsKey(vID))
                            continue;
                        long dataID = dict[vID];
                        if (dataID <= 0)
                            continue;
                        values.Add($"('{SqlStringFormat.Format(labelName)}',{SqlStringFormat.Format(dataID)})");
                    }

                    string sql = "insert or replace into metadata_to_label(LabelName,DataID) " +
                             $"values {string.Join(",", values)}";
                    metaDataMapper.ExecuteNonQuery(sql);
                }
            }
        }

        public static void MoveTranslate()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Translate.sqlite");
            if (!File.Exists(origin))
                return;

            MySqlite oldSqlite = null;
            System.Data.SQLite.SQLiteDataReader sr = null;
            try {
                oldSqlite = new MySqlite(origin);
                sr = oldSqlite.RunSql("select * from youdao");
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            if (oldSqlite == null || sr == null)
                return;

            // 先获得当前 transaltion 表的最大 id
            SelectWrapper<Translation> wrapper = new SelectWrapper<Translation>();
            wrapper.Select("MAX(TransaltionID) as TransaltionID");
            long l = 0;
            Translation translation = null;
            try {
                translation = translationMapper.SelectOne(wrapper);
            } catch (Exception ex) {
                Logger.Error(ex);
                return;
            }

            if (translation != null)
                l = translation.TranslationID;

            List<Translation> translations = new List<Translation>();
            HashSet<string> set = new HashSet<string>();
            while (sr.Read()) {
                try {
                    Translation title = new Translation() {
                        SourceText = sr["title"].ToString(),
                        TargetText = sr["translate_title"].ToString(),
                        SourceLang = Jvedio.Core.Enums.Language.Japanese.ToString(),
                        TargetLang = Jvedio.Core.Enums.Language.Chinese.ToString(),
                        Platform = TranslationPlatform.youdao.ToString(),
                        VID = sr["id"].ToString(),
                    };
                    Translation plot = new Translation() {
                        SourceText = sr["plot"].ToString(),
                        TargetText = sr["translate_plot"].ToString(),
                        SourceLang = Jvedio.Core.Enums.Language.Japanese.ToString(),
                        TargetLang = Jvedio.Core.Enums.Language.Chinese.ToString(),
                        Platform = TranslationPlatform.youdao.ToString(),
                        VID = sr["id"].ToString(),
                    };
                    set.Add(title.VID);
                    translations.Add(title);
                    translations.Add(plot);
                } catch (Exception ex) {
                    Logger.Error(ex);
                    continue;
                }
            }

            l += 1;     // 在原基础上 +1
            try {
                translationMapper.InsertBatch(translations);
            } catch (Exception ex) {
                Logger.Error(ex);
                return;
            }

            SelectWrapper<Video> selectWrapper = new SelectWrapper<Video>();
            selectWrapper.Select("VID", "DataID").In("VID", set);
            List<Video> videos = null;
            Dictionary<string, long> dict = new Dictionary<string, long>();
            try {
                videos = videoMapper.SelectList(selectWrapper);
                if (videos != null && videos.Count > 0)
                    dict = videos.ToLookup(x => x.VID, y => y.DataID).ToDictionary(x => x.Key, x => x.First());
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            if (dict != null && dict.Count > 0) {
                for (int i = 0; i < translations.Count; i++) {
                    long dataID = -1;
                    Translation trans = translations[i];
                    if (!string.IsNullOrEmpty(trans.VID) && dict.ContainsKey(trans.VID))
                        dataID = dict[trans.VID];
                    if (dataID <= 0)
                        continue;
                    string fieldType = i % 2 == 0 ? "Title" : "Plot";
                    string sql = "insert or replace into metadata_to_translation(DataID,FieldType,TransaltionID) " +
                                $"values ({dataID},'{fieldType}',{i + l})";
                    metaDataMapper.ExecuteNonQuery(sql);
                }
            }

            sr.Close();
            oldSqlite.CloseDB();
        }
    }
}
