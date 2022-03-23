using Jvedio.Core.Enums;
using Jvedio.Core.SimpleORM;
using Jvedio.Core.SimpleORM.Wrapper;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using Jvedio.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.GlobalMapper;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    public static class Jvedio4ToJvedio5
    {
        public static void MoveScanPathConfig(string[] files)
        {
            string ScanPathConfig = Path.Combine(oldDataPath, "ScanPathConfig");
            if (File.Exists(ScanPathConfig))
            {

                Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
                foreach (string file in files)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    System.Collections.Specialized.StringCollection collection = new ScanPathConfig(name).Read();
                    List<string> list = collection.Cast<string>().ToList();
                    dict.Add(name, list);
                }
                string json = JsonConvert.SerializeObject(dict);
                AppConfig appConfig = new AppConfig();
                appConfig.ConfigName = "ScanPaths";
                appConfig.ConfigValue = json;
                appConfigMapper.insert(appConfig, InsertMode.Replace);

            }
        }

        public static void MoveRecentWatch()
        {
            string RecentWatch = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RecentWatch");
            if (File.Exists(RecentWatch))
            {
                RecentWatchedConfig recentWatchedConfig = new RecentWatchedConfig();
                Dictionary<DateTime, List<string>> dict = recentWatchedConfig.Read();
                foreach (DateTime key in dict.Keys)
                {
                    List<string> list = dict[key];
                    if (list != null && list.Count > 0)
                    {
                        string sql = $"update metadata set ViewDate = '{key.toLocalDate()}' " +
                                    "where DataID in (select DataID from metadata_video " +
                                    $"where VID in ('{string.Join("','", list)}')); ";
                        metaDataMapper.executeNonQuery(sql);
                    }
                }
            }
        }


        public static Dictionary<string, object> getFromServer(Server server, string serverName)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("ServerName", serverName);
            dict.Add("Url", server.Url);
            dict.Add("IsEnable", server.IsEnable);
            dict.Add("LastRefreshDate", server.LastRefreshDate);
            dict.Add("Cookie", server.Cookie);
            return dict;
        }

        public static void MoveServersConfig()
        {
            string ServersConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServersConfig");
            if (File.Exists(ServersConfigPath))
            {
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                Servers servers = ServerConfig.Instance.ReadAll();
                list.Add(getFromServer(servers.Bus, "BUS"));
                list.Add(getFromServer(servers.BusEurope, "BUSEUROPE"));
                list.Add(getFromServer(servers.Library, "LIBRARY"));
                list.Add(getFromServer(servers.FC2, "FC2"));
                list.Add(getFromServer(servers.Jav321, "JAV321"));
                list.Add(getFromServer(servers.DMM, "DMM"));
                list.Add(getFromServer(servers.DB, "DB"));
                list.Add(getFromServer(servers.MOO, "MOO"));
                string json = JsonConvert.SerializeObject(list);
                AppConfig appConfig = new AppConfig();
                appConfig.ConfigName = "Servers";
                appConfig.ConfigValue = json;
                appConfigMapper.insert(appConfig, InsertMode.Replace);

            }
        }


        private static void setProgress(float current, string logText)
        {
            if (window_Progress != null)
            {
                window_Progress.MainProgress = current;
                window_Progress.LogText = logText;
            }
        }


        private static Window_Progress window_Progress;
        public static async Task<bool> MoveDatabases(string[] files)
        {
            if (files == null || files.Length == 0) return true;
            bool result = false;
            window_Progress = new Window_Progress("迁移数据", logText: "");

            // 不等待
            Task.Run(() => { App.Current.Dispatcher.Invoke(() => { window_Progress.ShowDialog(); }); });
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                if (File.Exists(file))
                {
                    bool success = await MoveOldData(file);

                    if (success)
                        result = true;
                    setProgress((100 * (float)i + 1) / (float)files.Length, $"迁移数据：{Path.GetFileName(file)}");
                }
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
        public static async Task<bool> MoveOldData(string origin)
        {
            return await Task.Run(() =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Console.WriteLine($"=======开始迁移数据：{Path.GetFileName(origin)}=========");

                MySqlite oldSqlite = new MySqlite(origin);
                // 1. 迁移 Code
                System.Data.SQLite.SQLiteDataReader db = oldSqlite.RunSql("select * from javdb");
                List<UrlCode> urlCodes = new List<UrlCode>();
                if (db != null)
                {
                    while (db.Read())
                    {
                        UrlCode urlCode = new UrlCode()
                        {
                            LocalValue = db["id"].ToString(),
                            RemoteValue = db["code"].ToString(),
                            WebType = "db",
                            ValueType = "video",
                        };
                        urlCodes.Add(urlCode);
                    }
                    urlCodeMapper.insertBatch(urlCodes);
                }
                db?.Close();

                System.Data.SQLite.SQLiteDataReader library = oldSqlite.RunSql("select * from library");
                urlCodes = new List<UrlCode>();
                if (library != null)
                {
                    while (library.Read())
                    {
                        UrlCode urlCode = new UrlCode()
                        {
                            LocalValue = library["id"].ToString(),
                            RemoteValue = library["code"].ToString(),
                            WebType = "library",
                            ValueType = "video",
                        };
                        urlCodes.Add(urlCode);
                    }
                    urlCodeMapper.insertBatch(urlCodes);
                }
                library?.Close();
                Console.WriteLine($"urlCodeMapper 用时：{watch.ElapsedMilliseconds} ms");
                watch.Restart();

                // 2. 迁移 actress
                System.Data.SQLite.SQLiteDataReader actressReader = oldSqlite.RunSql("select * from actress");
                List<ActorInfo> actressList = new List<ActorInfo>();
                if (actressReader != null)
                {
                    while (actressReader.Read())
                    {
                        Actress actress = new Actress();
                        actress.birthday = actressReader["birthday"].ToString();
                        actress.id = actressReader["id"].ToString();
                        actress.name = actressReader["name"].ToString();
                        int.TryParse(actressReader["age"].ToString(), out int age); actress.age = age;
                        int.TryParse(actressReader["height"].ToString(), out int height); actress.height = height;
                        int.TryParse(actressReader["chest"].ToString(), out int chest); actress.chest = chest;
                        int.TryParse(actressReader["waist"].ToString(), out int waist); actress.waist = waist;
                        int.TryParse(actressReader["hipline"].ToString(), out int hipline); actress.hipline = hipline;

                        actress.cup = actressReader["cup"].ToString();
                        actress.birthplace = actressReader["birthplace"].ToString();
                        actress.hobby = actressReader["hobby"].ToString();
                        actress.source = actressReader["source"].ToString();
                        actress.sourceurl = actressReader["sourceurl"].ToString();
                        actress.imageurl = actressReader["imageurl"].ToString();
                        ActorInfo actorInfo = actress.toActorInfo();
                        actressList.Add(actorInfo);
                    }
                    actorMapper.insertBatch(actressList);
                }
                actressReader?.Close();

                Console.WriteLine($"actorMapper 用时：{watch.ElapsedMilliseconds} ms");
                watch.Restart();

                // 3. 迁移 movie
                double total_count = oldSqlite.SelectCountByTable("movie");

                // 新建库
                AppDatabase appDatabase = new AppDatabase();
                appDatabase.Name = Path.GetFileNameWithoutExtension(origin);
                appDatabase.Count = (long)total_count;
                appDatabase.DataType = DataType.Video;
                appDatabaseMapper.insert(appDatabase);
                System.Data.SQLite.SQLiteDataReader sr = oldSqlite.RunSql("select * from movie");
                List<Video> videos = new List<Video>();
                if (sr != null)
                {
                    List<DetailMovie> detailMovies = new List<DetailMovie>();
                    while (sr.Read())
                    {
                        DetailMovie detailMovie = new DetailMovie()
                        {
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
                            source = sr["source"].ToString()
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
                    }

                    long before = metaDataMapper.selectCount();
                    long nextID = before + 1;
                    metaDataMapper.insertBatch(detailMovies.Select(item => item.toMetaData()).ToList());

                    Console.WriteLine($"metaDataMapper 用时：{watch.ElapsedMilliseconds} ms");
                    watch.Restart();
                    //videos = new List<Video>();
                    detailMovies.ForEach(arg =>
                    {
                        before++;
                        Video video = arg.toVideo();
                        video.DataID = before;
                        video.Path = arg.filepath;
                        video.Size = (long)arg.filesize;
                        video.Genre = arg.genre;
                        video.Tag = arg.tag;
                        video.Label = arg.label;
                        videos.Add(video);
                    });

                    videoMapper.insertBatch(videos);

                    Console.WriteLine($"videoMapper 用时：{watch.ElapsedMilliseconds} ms");
                    watch.Restart();


                    handleChinesetitle(detailMovies);
                    handleActor(detailMovies, nextID);

                    Console.WriteLine($"handleActor 用时：{watch.ElapsedMilliseconds} ms");
                    watch.Restart();
                }
                sr?.Close();
                oldSqlite.CloseDB();
                watch.Stop();

                // 建立标签戳索引

                //SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
                //wrapper.Select("Size", "Genre", "Tag", "Label", "Path").Eq("metadata.DBId", appDatabase.DBId).Eq("metadata.DataType", 0);
                //string sql = $"{wrapper.toSelect(false)} FROM metadata_video " +
                //            "JOIN metadata " +
                //            "on metadata.DataID=metadata_video.DataID " + wrapper.toWhere(false);

                //List<Dictionary<string, object>> temp = metaDataMapper.select(sql);
                //videos = metaDataMapper.toEntity<Video>(temp, typeof(Video).GetProperties(), false);


                List<string> list = new List<string>();
                foreach (Video video in videos)
                {
                    if (Identify.IsHDV(video.Size) || video.Genre?.IndexOfAnyString(TagStrings_HD) >= 0 || video.Tag?.IndexOfAnyString(TagStrings_HD) >= 0 || video.Label?.IndexOfAnyString(TagStrings_HD) >= 0)
                    {
                        list.Add($"({video.DataID},1)");
                    }

                    if (Identify.IsCHS(video.Path) || video.Genre?.IndexOfAnyString(TagStrings_Translated) >= 0 || video.Tag?.IndexOfAnyString(TagStrings_Translated) >= 0 || video.Label?.IndexOfAnyString(TagStrings_Translated) >= 0)
                    {
                        list.Add($"({video.DataID},2)");
                    }
                }
                if (list.Count > 0)
                {
                    string sql = $"insert into metadata_to_tagstamp (DataID,TagID) values {string.Join(",", list)}";
                    videoMapper.executeNonQuery(sql);
                }

                return true;
            });
        }

        private static void handleChinesetitle(List<DetailMovie> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string chinesetitle = list[i].chinesetitle;
                string title = list[i].title;
                if (string.IsNullOrEmpty(chinesetitle) || string.IsNullOrEmpty(title)) continue;
                TranslationMapper mapper = new TranslationMapper();
                Translation translation = new Translation();
                translation.SourceLang = Jvedio.Core.Enums.Language.Japanese.ToString();
                translation.TargetLang = Jvedio.Core.Enums.Language.Chinese.ToString();
                translation.SourceText = title;
                translation.TargetText = chinesetitle;
                translation.Platform = Jvedio.Core.Enums.TranslationPlatform.youdao.ToString();
                mapper.insert(translation);
                string sql = "insert into metadata_to_translation(DataID,FieldType,TransaltionID) " +
                    $"values ({i + 1},'Title',{translation.TransaltionID})";
                metaDataMapper.executeNonQuery(sql);
            }

        }


        private static void handleActor(List<DetailMovie> list, long beforeID)
        {
            // 新增不存在的
            List<ActorInfo> actorInfos = actorMapper.selectList();
            HashSet<string> names = list.Select(x => x.actor).ToHashSet();
            HashSet<string> to_insert = new HashSet<string>();
            foreach (string name in names)
            {
                to_insert.UnionWith(name.Split(new char[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet());
            }

            HashSet<string> hashSet = actorInfos.Select(x => x.ActorName).ToHashSet();
            to_insert.ExceptWith(hashSet);
            if (to_insert.Count > 0)
            {
                string sql = $"insert into actor_info(WebType,Gender,ActorName) values ('bus',1,'{string.Join("'),('bus',1,'", to_insert)}')";
                metaDataMapper.executeNonQuery(sql);
            }
            actorInfos = actorMapper.selectList();
            Dictionary<string, long> dict = actorInfos.ToDictionary(x => x.ActorName, x => x.ActorID);
            List<UrlCode> urlCodes = new List<UrlCode>();

            List<object[]> actor_name_to_metadatas = new List<object[]>();

            long count = metaDataMapper.selectCount();

            for (int i = 0; i < list.Count; i++)
            {

                string actor = list[i].actor; // 演员A 演员B 演员C
                string actorid = list[i].actorid;
                if (string.IsNullOrEmpty(actor)) continue;
                UrlCode urlCode = new UrlCode();
                string[] actorNames = actor.Split(new char[] { '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string[] actorIds = list[i].actorid.Split(new char[] { '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (actorIds.Length == actorNames.Length)
                {
                    // 如果 id 和名字数量一样，插入  urlCode
                    for (int j = 0; j < actorNames.Length; j++)
                    {
                        string actorName = actorNames[j];
                        if (string.IsNullOrEmpty(actorName)) continue;
                        urlCode.LocalValue = actorName;
                        urlCode.RemoteValue = actorIds[j];
                        urlCode.WebType = !string.IsNullOrEmpty(list[i].source) ? list[i].source.Replace("jav", "") : "";
                        urlCode.ValueType = "actor";
                        urlCodes.Add(urlCode);


                    }
                }
                for (int j = 0; j < actorNames.Length; j++)
                {
                    string actorName = actorNames[j];
                    if (dict.ContainsKey(actorName))
                    {
                        object[] o = new object[2];
                        o[0] = actorName;
                        o[1] = beforeID + i; // DataID
                        actor_name_to_metadatas.Add(o);
                    }
                }



            }
            urlCodeMapper.insertBatch(urlCodes, InsertMode.Ignore);

            if (actor_name_to_metadatas.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (object[] item in actor_name_to_metadatas)
                {
                    builder.Append($"('{item[0]}','{item[1]}'),");
                }
                if (builder[builder.Length - 1] == ',') builder.Remove(builder.Length - 1, 1);
                string sql = $"insert or ignore into actor_name_to_metadatas(ActorName,DataID) values {builder};";
                metaDataMapper.executeNonQuery(sql);


            }
        }



        // todo 修复 AI 存储问题
        public static void MoveAI()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AI.sqlite");
            if (File.Exists(origin))
            {
                MySqlite oldSqlite = new MySqlite(origin);
                System.Data.SQLite.SQLiteDataReader sr = oldSqlite.RunSql("select * from baidu");
                List<AIFaceInfo> list = new List<AIFaceInfo>();
                if (sr != null)
                {
                    while (sr.Read())
                    {
                        AIFaceInfo faceInfo = new AIFaceInfo()
                        {
                            Expression = sr["expression"].ToString(),
                            FaceShape = sr["face_shape"].ToString(),
                            Race = sr["race"].ToString(),
                            Emotion = sr["emotion"].ToString(),
                        };

                        int.TryParse(sr["age"].ToString(), out int age);
                        int.TryParse(sr["glasses"].ToString(), out int glasses);
                        int.TryParse(sr["mask"].ToString(), out int mask);
                        float.TryParse(sr["beauty"].ToString(), out float beauty);

                        Enum.TryParse<Gender>(sr["gender"].ToString(), out Gender gender);

                        faceInfo.Age = age;
                        faceInfo.Beauty = beauty;
                        faceInfo.Gender = gender;
                        faceInfo.Glasses = glasses != 0;
                        faceInfo.Mask = mask != 0;
                        faceInfo.Platform = "baidu";
                        list.Add(faceInfo);

                    }
                    aIFaceMapper.insertBatch(list);
                }
                sr?.Close();
                oldSqlite.CloseDB();
            }
        }

        public static void MoveMagnets()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Magnets.sqlite");
            if (File.Exists(origin))
            {
                MySqlite oldSqlite = new MySqlite(origin);
                System.Data.SQLite.SQLiteDataReader sr = oldSqlite.RunSql("select * from magnets");
                if (sr != null)
                {
                    List<Magnet> magnets = new List<Magnet>();
                    HashSet<string> set = new HashSet<string>();
                    while (sr.Read())
                    {
                        Magnet magnet = new Magnet()
                        {
                            MagnetLink = sr["link"].ToString(),
                            Title = sr["title"].ToString(),
                            Releasedate = sr["releasedate"].ToString(),
                            Tag = sr["tag"].ToString().Replace(' ', GlobalVariable.Separator),
                            VID = sr["id"].ToString(),
                        };
                        set.Add(magnet.VID);
                        float.TryParse(sr["size"].ToString(), out float size);
                        magnet.Size = (long)(size * 1024 * 1024);
                        magnets.Add(magnet);
                    }
                    SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
                    wrapper.Select("VID", "DataID").In("VID", set);
                    List<Video> videos = videoMapper.selectList(wrapper);
                    Dictionary<string, long> dict = new Dictionary<string, long>();
                    if (videos != null && videos.Count > 0) dict = videos.ToDictionary(x => x.VID, y => y.DataID);
                    for (int i = 0; i < magnets.Count; i++)
                    {
                        string vid = magnets[i].VID;
                        if (dict.ContainsKey(vid)) magnets[i].DataID = dict[vid];
                    }
                    magnetsMapper.insertBatch(magnets);
                }
                sr?.Close();
                oldSqlite.CloseDB();
            }
        }
        public static void MoveSearchHistory()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SearchHistory");
            if (!File.Exists(origin)) return;
            using (StreamReader sr = new StreamReader(origin))
            {
                string data = sr.ReadToEnd();
                if (!string.IsNullOrEmpty(data))
                {
                    List<SearchHistory> histories = new List<SearchHistory>();
                    List<string> VID_list = data.Split(new char[] { '\'' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (string VID in VID_list)
                    {
                        SearchHistory history = new SearchHistory();
                        history.SearchField = SearchField.video;
                        history.SearchValue = VID;
                        histories.Add(history);
                    }
                    searchHistoryMapper.insertBatch(histories);
                }
            }
        }

        public static void MoveMyList()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mylist.sqlite");
            if (File.Exists(origin))
            {
                MySqlite oldSqlite = new MySqlite(origin);
                List<string> tables = oldSqlite.GetAllTable();
                Dictionary<string, List<string>> datas = new Dictionary<string, List<string>>();
                System.Data.SQLite.SQLiteDataReader sr;

                foreach (string table in tables)
                {
                    List<string> list = new List<string>();
                    sr = oldSqlite.RunSql($"select * from {table}");
                    if (sr != null)
                    {
                        while (sr.Read())
                        {
                            list.Add(sr.GetString(0));
                        }
                    }
                    sr.Close();
                    datas.Add(table, list);
                }
                oldSqlite.CloseDB();
                HashSet<string> set = new HashSet<string>();
                foreach (string key in datas.Keys)
                {
                    set.UnionWith(datas[key]);
                }

                SelectWrapper<Video> selectWrapper = new SelectWrapper<Video>();
                selectWrapper.Select("VID", "DataID").In("VID", set);
                List<Video> videos = videoMapper.selectList(selectWrapper);
                Dictionary<string, long> dict = new Dictionary<string, long>();
                if (videos != null && videos.Count > 0)
                {
                    dict = videos.ToLookup(t => t.VID, t => t.DataID).ToDictionary(t => t.Key, t => t.First());
                }
                if (dict.Count > 0)
                {
                    foreach (string key in datas.Keys)
                    {
                        List<string> id_list = datas[key];
                        // 新增一个清单
                        CustomList list = new CustomList();
                        list.ListName = key;
                        list.Count = id_list.Count;
                        customListMapper.insert(list);
                        List<string> values = new List<string>();
                        foreach (string VID in id_list)
                        {
                            if (!dict.ContainsKey(VID)) continue;
                            long dataID = dict[VID];
                            if (dataID <= 0) continue;
                            values.Add($"({list.ListID},{dataID})");
                        }
                        string sql = "insert or replace into common_custom_list_to_metadata(ListID,DataID) " +
                                 $"values {string.Join(",", values)}";
                        metaDataMapper.executeNonQuery(sql);
                    }
                }
            }

        }
        public static void MoveTranslate()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Translate.sqlite");
            if (File.Exists(origin))
            {
                MySqlite oldSqlite = new MySqlite(origin);
                System.Data.SQLite.SQLiteDataReader sr = oldSqlite.RunSql("select * from youdao");

                // 先获得当前 transaltion 表的最大 id
                SelectWrapper<Translation> wrapper = new SelectWrapper<Translation>();
                wrapper.Select("MAX(TransaltionID) as TransaltionID");
                long l = 0;
                Translation translation = translationMapper.selectOne(wrapper);
                if (translation != null) l = translation.TransaltionID;
                if (sr != null)
                {
                    List<Translation> translations = new List<Translation>();
                    HashSet<string> set = new HashSet<string>();
                    while (sr.Read())
                    {
                        Translation title = new Translation()
                        {
                            SourceText = sr["title"].ToString(),
                            TargetText = sr["translate_title"].ToString(),
                            SourceLang = Jvedio.Core.Enums.Language.Japanese.ToString(),
                            TargetLang = Jvedio.Core.Enums.Language.Chinese.ToString(),
                            Platform = TranslationPlatform.youdao.ToString(),
                            VID = sr["id"].ToString(),
                        };
                        Translation plot = new Translation()
                        {
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
                    }
                    l += 1;
                    translationMapper.insertBatch(translations);

                    SelectWrapper<Video> selectWrapper = new SelectWrapper<Video>();
                    selectWrapper.Select("VID", "DataID").In("VID", set);
                    List<Video> videos = videoMapper.selectList(selectWrapper);
                    Dictionary<string, long> dict = new Dictionary<string, long>();
                    if (videos != null && videos.Count > 0)
                    {
                        dict = videos.ToDictionary(x => x.VID, y => y.DataID);
                    }

                    if (dict.Count > 0)
                    {
                        for (int i = 0; i < translations.Count; i++)
                        {
                            long dataID = -1;
                            if (dict.ContainsKey(translation.VID)) dataID = dict[translation.VID];
                            if (dataID <= 0) continue;
                            string FieldType = i % 2 == 0 ? "Title" : "Plot";
                            string sql = "insert or replace into metadata_to_translation(DataID,FieldType,TransaltionID) " +
                                        $"values ({dataID},'{FieldType}',{i + l})";
                            metaDataMapper.executeNonQuery(sql);
                        }
                    }




                }
                sr?.Close();
                oldSqlite.CloseDB();
            }
        }


    }
}
