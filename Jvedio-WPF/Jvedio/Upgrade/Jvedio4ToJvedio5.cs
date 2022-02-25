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
using Jvedio.Utils;
using Newtonsoft.Json;
using Jvedio.Core;
using System.Windows.Controls.Primitives;
using ChaoControls.Style;

using Jvedio.Entity;
using Jvedio.Mapper;
using Jvedio.Core.SqlMapper;
using Jvedio.Test;
using Jvedio.Core.Sql;
using Jvedio.Entity.CommonSQL;
using Jvedio.Core.Enums;

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
                GlobalVariable.AppConfigMapper.insert(appConfig);
                FileHelper.TryMoveToRecycleBin(ScanPathConfig);
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
                GlobalVariable.AppConfigMapper.insert(appConfig);
                FileHelper.TryMoveToRecycleBin(ServersConfigPath);
            }
        }

        /// <summary>
        /// 移动旧数据库到新数据库
        /// </summary>
        /// <remarks>迁移 4.6.1.1 之前的数据</remarks>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool MoveOldData(string origin, string target)
        {
            if (File.Exists(origin))
            {
                MySqlite oldSqlite = new MySqlite(origin);
                MetaDataMapper dataMapper = new MetaDataMapper(target);
                ActorMapper actorMapper = new ActorMapper(target);
                foreach (string key in SqliteTables.Actor.TABLES.Keys)
                {
                    actorMapper.createTable(key, SqliteTables.Actor.TABLES[key]);
                }
                foreach (string key in SqliteTables.Data.TABLES.Keys)
                {
                    dataMapper.createTable(key, SqliteTables.Data.TABLES[key]);
                }

                System.Data.SQLite.SQLiteDataReader sr = oldSqlite.RunSql("select * from movie");
                if (sr != null)
                {
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

                        //导入新数据库中
                        //newConnection.insertMovie(detailMovie);
                        break; // 测试
                    }
                }

                oldSqlite.CloseDB();
                dataMapper.Dispose();
                actorMapper.Dispose();
                return true;
            }
            return false;
        }

        public static void MoveRecentWatch()
        {

        }


        public static void MoveAI()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AI.sqlite");
            string target = GlobalVariable.AppDataPath;
            if (File.Exists(origin))
            {
                MySqlite oldSqlite = new MySqlite(origin);
                AIFaceMapper faceMapper = new AIFaceMapper(target);
                System.Data.SQLite.SQLiteDataReader sr = oldSqlite.RunSql("select * from baidu");
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

                        //导入新数据库中
                        faceMapper.insert(faceInfo);
                    }
                }

                oldSqlite.CloseDB();
                faceMapper.Dispose();
                FileHelper.TryMoveToRecycleBin(origin, 3);
            }
        }


        // todo MoveMagnets
        public static void MoveMagnets()
        {
            string origin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Magnets.sqlite");
            string target = GlobalVariable.AppDataPath;
            if (File.Exists(origin))
            {
                MySqlite oldSqlite = new MySqlite(origin);
                AIFaceMapper faceMapper = new AIFaceMapper(target);
                System.Data.SQLite.SQLiteDataReader sr = oldSqlite.RunSql("select * from baidu");
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

                        //导入新数据库中
                        faceMapper.insert(faceInfo);
                    }
                }

                oldSqlite.CloseDB();
                faceMapper.Dispose();
                FileHelper.TryMoveToRecycleBin(origin, 3);
            }
        }
        public static void MoveTranslate()
        {

        }
    }
}
