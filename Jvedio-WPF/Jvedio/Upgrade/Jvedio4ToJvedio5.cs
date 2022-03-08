using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Jvedio.FileProcess;
using static Jvedio.GlobalVariable;
using static Jvedio.GlobalMapper;
using Jvedio.Utils;
using Newtonsoft.Json;
using Jvedio.Core;
using System.Windows.Controls.Primitives;
using ChaoControls.Style;

using Jvedio.Entity;
using Jvedio.Mapper;
using Jvedio.Core.SimpleORM;
using Jvedio.Test;
using Jvedio.Core.DataBase;
using Jvedio.Entity.CommonSQL;
using Jvedio.Core.Enums;
using Jvedio.Utils.Common;
using Jvedio.Utils.FileProcess;
using System.Text;

namespace Jvedio
{
    public static class Jvedio4ToJvedio5
    {

        private static UpgradeProcessWindow progressWindow = new UpgradeProcessWindow();


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
                appConfigMapper.insert(appConfig);

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
                appConfigMapper.insert(appConfig);

            }
        }

        public static async Task<bool> MoveDatabases(string[] files)
        {

            if (files == null || files.Length == 0) return false;
            bool result = false;
            string temp_file = Path.Combine(CurrentUserFolder, ".moving_databases");//如果存在改文件说明迁移数据失败，删除数据重新迁移

            // 如果中断数据迁移了，则删除所有已迁移的文件
            if (File.Exists(temp_file))
            {
                //string[] exist_files = FileHelper.TryScanDIr(DataPath, "*.sqlite", SearchOption.TopDirectoryOnly);


                //foreach (var item in exist_files)
                //{
                //    try { File.Delete(item); }
                //    catch (Exception ex)
                //    {
                //        Logger.LogE(ex);
                //        new Msgbox(Application.Current.MainWindow, ex.Message).Show();
                //        new Msgbox(Application.Current.MainWindow, "数据迁移失败，关闭程序").Show();
                //        Application.Current.Shutdown();
                //    }
                //}
            }
            FileHelper.TryWriteToFile(temp_file, DateHelper.Now());
            foreach (var item in files)
            {
                if (File.Exists(item))
                {
                    string name = Path.GetFileName(item);
                    bool success = await MoveOldData(item);
                    if (success) result = true;
                }
            }
            FileHelper.TryDeleteFile(temp_file); // 迁移后才删除

            return result;
        }

        private static void setProgress(string text, double current, double total)
        {
            if (App.Current != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    progressWindow.CurrentText.Text = text;
                    progressWindow.CurrentProgressBar.Value = current * 100;
                    progressWindow.TotalProgressBar.Value = total * 100;
                });
            }

        }


        private static void shutdown()
        {
            if (App.Current != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Application.Current.Shutdown();
                });
            }
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
                if (!File.Exists(origin)) return false;

                App.Current?.Dispatcher.Invoke((Action)delegate { progressWindow.Show(); });

                MySqlite oldSqlite = new MySqlite(origin);
                float task_num = 4;

                // 1. 迁移 Code
                setProgress("迁移 Code", 0, 0);
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
                        if (progressWindow.IsClosed) break;
                    }
                    urlCodeMapper.insertBatch(urlCodes, true);
                }
                db.Close();

                setProgress("迁移 Code", 0.5, 0);
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
                        if (progressWindow.IsClosed) break;
                    }
                    urlCodeMapper.insertBatch(urlCodes, true);
                }
                library.Close();
                setProgress("", 1, 1.0 / task_num);

                // 2. 迁移 actress
                if (progressWindow.IsClosed) shutdown();
                System.Data.SQLite.SQLiteDataReader actressReader = oldSqlite.RunSql("select * from actress");
                List<ActorInfo> actressList = new List<ActorInfo>();
                if (actressReader != null)
                {
                    setProgress("迁移 actress", 0, 1.0 / task_num);
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
                        if (progressWindow.IsClosed) break;
                    }
                    actorMapper.insertBatch(actressList, true);
                }
                actressReader.Close();
                setProgress("", 1, 3.0 / task_num);

                // 3. 迁移 movie
                // 还只能一个个迁移
                if (progressWindow.IsClosed) shutdown();
                double total_count = oldSqlite.SelectCountByTable("movie");
                System.Data.SQLite.SQLiteDataReader sr = oldSqlite.RunSql("select * from movie");

                if (sr != null)
                {
                    setProgress("迁移 movie", 0, 3 / task_num);
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
                        //handleActor(urlCodeMapper, detailMovie.actor, detailMovie.actorid, detailMovie.source);
                        //
                        //setProgress($"迁移 movie：{detailMovie.id}", idx / total_count, 3 / task_num);
                        //idx++;
                        //if (progressWindow.IsClosed) break;
                    }
                    //metaDataMapper.insertBatch(detailMovies.Select(item => item.toMetaData()).ToList());
                    //videoMapper.insertBatch(detailMovies.Select(item => item.toVideo()).ToList());
                    //handleChinesetitle(detailMovies);
                    handleActor(detailMovies);
                }
                //setProgress("迁移结束", 1, 1);

                sr.Close();
                oldSqlite.CloseDB();
                //if (progressWindow.IsClosed) shutdown();
                //if (App.Current != null)
                //    App.Current.Dispatcher.Invoke((Action)delegate { progressWindow.Close(); });
                return true;
            });
        }


        // todo id 为0
        private static void handleChinesetitle(List<DetailMovie> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string chinesetitle = list[i].chinesetitle;
                string title = list[i].title;
                if (string.IsNullOrEmpty(chinesetitle) || string.IsNullOrEmpty(title)) continue;
                TranslationMapper mapper = new TranslationMapper();
                Translation translation = new Translation();
                translation.SourceLang = "Japanese";
                translation.TargetLang = "Chinese";
                translation.SourceText = title;
                translation.TargetText = chinesetitle;
                translation.Platform = "youdao";
                mapper.insert(translation);
                string sql = "insert into metadata_to_translation(DataID,FieldType,TransaltionID) " +
                    $"values ({i + 1},'Title',{translation.TransaltionID})";
                metaDataMapper.executeNonQuery(sql);
            }

        }


        private static void handleActor(List<DetailMovie> list)
        {
            // 新增不存在的
            List<ActorInfo> actorInfos = actorMapper.selectList();
            HashSet<string> names = list.Select(x => x.actor).ToHashSet();
            HashSet<string> to_insert = new HashSet<string>();
            foreach (string name in names)
            {
                to_insert.UnionWith(name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet());
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

            Dictionary<string, int> actor_name_to_metadatas = new Dictionary<string, int>();

            for (int i = 0; i < list.Count; i++)
            {

                string actor = list[i].actor;
                string actorid = list[i].actorid;
                if (string.IsNullOrEmpty(actor)) continue;
                UrlCode urlCode = new UrlCode();
                string[] actorNames = actor.Split(new char[] { '/', ' ' });
                string[] actorIds = list[i].actorid.Split(new char[] { '/', ' ' });
                if (actorIds.Length != actorNames.Length) continue;
                for (int j = 0; j < actorNames.Length; j++)
                {
                    string actorName = actorNames[j];
                    if (string.IsNullOrEmpty(actorName)) continue;
                    urlCode.LocalValue = actorName;
                    urlCode.RemoteValue = actorIds[j];
                    urlCode.WebType = !string.IsNullOrEmpty(list[i].source) ? list[i].source.Replace("jav", "") : "";
                    urlCode.ValueType = "actor";
                    urlCodes.Add(urlCode);
                    if (dict.ContainsKey(actorName))
                    {
                        actor_name_to_metadatas.Add(actorName, i + 1);
                    }

                }
            }
            urlCodeMapper.insertBatch(urlCodes);

            if (actor_name_to_metadatas.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string key in actor_name_to_metadatas.Keys)
                {
                    builder.Append($"('{key}','{actor_name_to_metadatas[key]}'),");
                }
                if (builder[builder.Length - 1] == ',') builder.Remove(builder.Length - 1, 1);
                string sql = $"insert into actor_name_to_metadatas(ActorName,DataID) values {builder};";
                metaDataMapper.executeNonQuery(sql);


            }
        }

        public static void MoveRecentWatch()
        {
            RecentWatchedConfig watchedConfig = new RecentWatchedConfig();
            Dictionary<DateTime, List<string>> dict = watchedConfig.Read();





            foreach (DateTime dateTime in dict.Keys)
            {
                List<string> VIDList = dict[dateTime];
                foreach (string vid in VIDList)
                {



                    Console.WriteLine(vid);
                }
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
                        //导入新数据库中

                    }
                    aIFaceMapper.insertBatch(list);
                }
                oldSqlite.CloseDB();
            }
        }

        private static void testInsertMagnets()
        {
            List<Magnet> magnets = new List<Magnet>();
            for (int i = 0; i < 10; i++)
            {
                Magnet magnet = new Magnet();
                magnet.Size = 1034240;
                magnet.Title = "";
                magnet.MagnetLink = "magnet:?xt=urn:btih:04A0F7C02782B9C6D850E0E22BE1A32B3D06753E" + i;
                magnet.DataID = 1;
                magnet.Title = "1pondo-121213_713秘藏映象の單人遊戲絕品氣質美少女西尾かおり";
                magnet.Releasedate = "2014-10-30";
                magnets.Add(magnet);
            }
            magnetsMapper.insertBatch(magnets);
        }


        // todo MoveMagnets
        public static void MoveMagnets()
        {

            testInsertMagnets();


            //string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Magnets.sqlite");
            //string target = DataPath;
            //if (File.Exists(origin))
            //{
            //    MySqlite oldSqlite = new MySqlite(origin);
            //    AIFaceMapper faceMapper = new AIFaceMapper(target);
            //    System.Data.SQLite.SQLiteDataReader sr = oldSqlite.RunSql("select * from magnets");
            //    if (sr != null)
            //    {
            //        while (sr.Read())
            //        {
            //            Magnet faceInfo = new Magnet()
            //            {
            //                MagnetLink = sr["link"].ToString(),
            //                FaceShape = sr["face_shape"].ToString(),
            //                Race = sr["race"].ToString(),
            //                Emotion = sr["emotion"].ToString(),
            //            };

            //            int.TryParse(sr["age"].ToString(), out int age);
            //            int.TryParse(sr["glasses"].ToString(), out int glasses);
            //            int.TryParse(sr["mask"].ToString(), out int mask);
            //            float.TryParse(sr["beauty"].ToString(), out float beauty);

            //            Enum.TryParse<Gender>(sr["gender"].ToString(), out Gender gender);

            //            faceInfo.Age = age;
            //            faceInfo.Beauty = beauty;
            //            faceInfo.Gender = gender;
            //            faceInfo.Glasses = glasses != 0;
            //            faceInfo.Mask = mask != 0;
            //            faceInfo.Platform = "baidu";

            //            //导入新数据库中
            //            faceMapper.insert(faceInfo);
            //        }
            //    }

            //    oldSqlite.CloseDB();
            //    faceMapper.Dispose();
            //    FileHelper.TryMoveToRecycleBin(origin, 3);
            //}
        }
        public static void MoveTranslate()
        {
            //    string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Translate.sqlite");
            //    string target = DataPath;
            //    if (File.Exists(origin))
            //    {
            //        MySqlite oldSqlite = new MySqlite(origin);
            //        AIFaceMapper faceMapper = new AIFaceMapper(target);
            //        System.Data.SQLite.SQLiteDataReader baidu = oldSqlite.RunSql("select * from baidu");
            //        System.Data.SQLite.SQLiteDataReader youdao = oldSqlite.RunSql("select * from youdao");
            //        if (baidu != null)
            //        {
            //            while (baidu.Read())
            //            {
            //                Translation faceInfo = new Translation()
            //                {
            //                    SourceLang = "",
            //                    FaceShape = sr["face_shape"].ToString(),
            //                    Race = sr["race"].ToString(),
            //                    Emotion = sr["emotion"].ToString(),
            //                };

            //                int.TryParse(sr["age"].ToString(), out int age);
            //                int.TryParse(sr["glasses"].ToString(), out int glasses);
            //                int.TryParse(sr["mask"].ToString(), out int mask);
            //                float.TryParse(sr["beauty"].ToString(), out float beauty);

            //                Enum.TryParse<Gender>(sr["gender"].ToString(), out Gender gender);

            //                faceInfo.Age = age;
            //                faceInfo.Beauty = beauty;
            //                faceInfo.Gender = gender;
            //                faceInfo.Glasses = glasses != 0;
            //                faceInfo.Mask = mask != 0;
            //                faceInfo.Platform = "baidu";

            //                //导入新数据库中
            //                faceMapper.insert(faceInfo);
            //            }
            //        }

            //        oldSqlite.CloseDB();
            //        faceMapper.Dispose();
            //        FileHelper.TryMoveToRecycleBin(origin, 3);
            //    }
        }
    }
}
