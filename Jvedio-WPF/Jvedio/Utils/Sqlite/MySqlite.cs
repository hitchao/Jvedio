using DynamicData.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Jvedio.Utils;
using Jvedio.Utils.Sqlite;

namespace Jvedio
{
    public class MySqlite : Sqlite
    {

        public MySqlite(string path) : this(path,false)
        {
            
        }

        public MySqlite(string path, bool absolute ) : base(path)
        {
            SqlitePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.EndsWith(".sqlite") ? path : path + ".sqlite");
            if (absolute && path != "") SqlitePath = path;
            cn = new SQLiteConnection("data source=" + SqlitePath);
            cn.Open();
            cmd = new SQLiteCommand();
            cmd.Connection = cn;
        }


        public void CloseDB()
        {
            this.Close();
        }




        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="sqltext"></param>
        public void CreateTable(string sqltext)
        {
            this.ExecuteSql(sqltext);
        }


        private DetailMovie GetDetailMovieFromSQLiteDataReader(SQLiteDataReader sr)
        {
            if (sr==null || sr["id"] == null || string.IsNullOrEmpty(sr["id"].ToString())) return null;
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

            if (Properties.Settings.Default.ShowFileNameIfTitleEmpty &&
                string.IsNullOrEmpty(detailMovie.title) &&
                !string.IsNullOrEmpty(detailMovie.filepath))
            {
                detailMovie.title = Path.GetFileNameWithoutExtension(detailMovie.filepath);
            }
            return detailMovie;
        }


        private Magnet GetMagnetFromSQLiteDataReader(SQLiteDataReader sr)
        {
            if (sr==null || sr["id"] == null || string.IsNullOrEmpty(sr["id"].ToString())) return null;
            Magnet result = new Magnet()
            {
                id = sr["id"].ToString(),
                title = sr["title"].ToString(),
                size = double.Parse(sr["size"].ToString()),
                link = sr["link"].ToString(),
                releasedate = sr["releasedate"].ToString()
            };

            string tag = sr["tag"].ToString();
            if (tag.IndexOf(' ') > 0)
            {
                foreach (var item in tag.Split(' '))
                {
                    if (item.Length > 0) result.tag.Add(item);
                }
            }
            else if (tag.Length > 0)
            {
                result.tag.Add(tag);
            }

            return result;
        }

        public List<Movie> SelectMoviesBySql(string sqltext)
        {
            List<Movie> result = new List<Movie>();
            if (!IsTableExist("movie")) return result;
            if (string.IsNullOrEmpty(sqltext)) return result;
            else cmd.CommandText = sqltext;
            using (SQLiteDataReader sr = cmd.ExecuteReader())
            {
                try
                {
                    while (sr.Read())
                    {

                        Movie movie = GetDetailMovieFromSQLiteDataReader(sr);
                        if (movie != null) result.Add(movie);
                    }

                }
                catch (Exception e) { Logger.LogD(e); }
            }
            return result;
        }

        public List<Magnet> SelectMagnetsBySql(string sqltext)
        {
            List<Magnet> result = new List<Magnet>();
            if (string.IsNullOrEmpty(sqltext)) return result;
            else cmd.CommandText = sqltext;

            using (SQLiteDataReader sr = cmd.ExecuteReader())
            {
                try
                {
                    while (sr.Read())
                    {

                        Magnet magnet = GetMagnetFromSQLiteDataReader(sr);
                        result.Add(magnet);
                    }

                }
                catch (Exception e) { Logger.LogD(e); }
            }
            return result;
        }

        public Movie SelectMovieBySql(string sqltext)
        {
            Movie result = null;
            if (string.IsNullOrEmpty(sqltext)) return result;
            else cmd.CommandText = sqltext;
            try
            {
                using (SQLiteDataReader sr = cmd.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        Movie movie = GetDetailMovieFromSQLiteDataReader(sr);
                        if (movie != null) return movie;
                    }
                }
            }
            catch (Exception e) { Logger.LogD(e); }
            return result;
        }


        public DetailMovie SelectDetailMovieBySql(string sqltext)
        {
            DetailMovie result = null;
            if (string.IsNullOrEmpty(sqltext)) return result;
            else cmd.CommandText = sqltext;

            using (SQLiteDataReader sr = cmd.ExecuteReader())
            {
                try
                {
                    while (sr.Read())
                    {
                        DetailMovie detailMovie = GetDetailMovieFromSQLiteDataReader(sr);
                        if (detailMovie != null) return detailMovie;
                    }

                }
                catch (Exception e) { Logger.LogD(e); }
            }
            return result;
        }

        public List<DetailMovie> SelectDetailMoviesBySql(string sqltext)
        {
            List<DetailMovie> result = new List<DetailMovie>();
            if (string.IsNullOrEmpty(sqltext)) return result;
            else cmd.CommandText = sqltext;

            using (SQLiteDataReader sr = cmd.ExecuteReader())
            {
                try
                {
                    while (sr.Read())
                    {

                        DetailMovie detailMovie = GetDetailMovieFromSQLiteDataReader(sr);
                        if (detailMovie != null) result.Add(detailMovie);
                    }

                }
                catch (Exception e) { Logger.LogD(e); }
            }
            return result;
        }

        public void InsertFullMovie(Movie movie, string table)
        {
            if (movie.vediotype == 0) return;//不能插入未指定类型的视频
            movie.id = movie.id.Replace("FC2PPV", "FC2");
            string sqltext = $"INSERT INTO {table}(id  , title  , filesize  , filepath  , subsection  , vediotype  , scandate  , releasedate , visits , director  , genre  , tag  , actor  , actorid  ,studio  , rating , chinesetitle  , favorites  , label  , plot  , outline  , year   , runtime , country  , countrycode ,otherinfo , sourceurl , source ,actressimageurl ,smallimageurl ,bigimageurl ,extraimageurl ) " +
               "values(@id  , @title  , @filesize  , @filepath  , @subsection  , @vediotype  , @scandate  , @releasedate , @visits , @director  , @genre  , @tag  , @actor  , @actorid  ,@studio  , @rating , @chinesetitle  , @favorites  ,@label  , @plot  , @outline  , @year   , @runtime , @country  , @countrycode ,@otherinfo , @sourceurl , @source ,@actressimageurl ,@smallimageurl ,@bigimageurl ,@extraimageurl) " +
               "ON CONFLICT(id) DO UPDATE SET title=@title  , filesize=@filesize  , filepath=@filepath  , subsection=@subsection  , vediotype=@vediotype  , scandate=@scandate  , releasedate=@releasedate , visits=@visits , director=@director  , genre=@genre  , tag=@tag  , actor=@actor  , actorid=@actorid  ,studio=@studio  , rating=@rating , chinesetitle=@chinesetitle  ,favorites=@favorites  ,label=@label  , plot=@plot  , outline=@outline  , year=@year   , runtime=@runtime , country=@country  , countrycode=@countrycode ,otherinfo=@otherinfo , sourceurl=@sourceurl , source=@source ,actressimageurl=@actressimageurl ,smallimageurl=@smallimageurl ,bigimageurl=@bigimageurl ,extraimageurl=@extraimageurl";

            if (movie.vediotype != 3) movie.id = movie.id.ToUpper();//除了 欧美影片之外，其他影片必须大写
            cmd.CommandText = sqltext;
            cmd.Parameters.Add("id", DbType.String).Value = movie.id;
            cmd.Parameters.Add("title", DbType.String).Value = movie.title;
            cmd.Parameters.Add("filesize", DbType.Double).Value = movie.filesize;
            cmd.Parameters.Add("filepath", DbType.String).Value = movie.filepath;
            cmd.Parameters.Add("subsection", DbType.String).Value = movie.subsection;
            cmd.Parameters.Add("vediotype", DbType.Int16).Value = movie.vediotype;
            cmd.Parameters.Add("scandate", DbType.String).Value = movie.scandate;
            cmd.Parameters.Add("releasedate", DbType.String).Value = movie.releasedate;
            cmd.Parameters.Add("visits", DbType.Int16).Value = movie.visits;
            cmd.Parameters.Add("director", DbType.String).Value = movie.director;
            cmd.Parameters.Add("genre", DbType.String).Value = movie.genre;
            cmd.Parameters.Add("tag", DbType.String).Value = movie.tag;
            cmd.Parameters.Add("actor", DbType.String).Value = movie.actor;
            cmd.Parameters.Add("actorid", DbType.String).Value = movie.actorid;
            cmd.Parameters.Add("studio", DbType.String).Value = movie.studio;
            cmd.Parameters.Add("rating", DbType.Double).Value = movie.rating;
            cmd.Parameters.Add("chinesetitle", DbType.String).Value = movie.chinesetitle;
            cmd.Parameters.Add("favorites", DbType.Int16).Value = movie.favorites;
            cmd.Parameters.Add("label", DbType.String).Value = movie.label;
            cmd.Parameters.Add("plot", DbType.String).Value = movie.plot;
            cmd.Parameters.Add("outline", DbType.String).Value = movie.outline;
            cmd.Parameters.Add("year", DbType.Int16).Value = movie.year;
            cmd.Parameters.Add("runtime", DbType.Int16).Value = movie.runtime;
            cmd.Parameters.Add("country", DbType.String).Value = movie.country;
            cmd.Parameters.Add("countrycode", DbType.Int16).Value = movie.countrycode;
            cmd.Parameters.Add("otherinfo", DbType.String).Value = movie.otherinfo;
            cmd.Parameters.Add("sourceurl", DbType.String).Value = movie.sourceurl;
            cmd.Parameters.Add("source", DbType.String).Value = movie.source;
            cmd.Parameters.Add("smallimageurl", DbType.String).Value = movie.smallimageurl;
            cmd.Parameters.Add("bigimageurl", DbType.String).Value = movie.bigimageurl;
            cmd.Parameters.Add("extraimageurl", DbType.String).Value = movie.extraimageurl;
            cmd.Parameters.Add("actressimageurl", DbType.String).Value = movie.actressimageurl;
            cmd.ExecuteNonQuery();
        }



        public void InsertCrawledMovie(Movie movie, string table)
        {
            //20个数据
            string sqltext = $"INSERT INTO {table}(id  , title    , releasedate  , director  , genre  , tag  , actor  , actorid  ,studio  , rating       , plot  , outline  , year   , runtime     , sourceurl , source ,actressimageurl ,smallimageurl ,bigimageurl ,extraimageurl ) " +
               "values(@id  , @title    , @releasedate ,  @director  , @genre  , @tag  , @actor  , @actorid  ,@studio  , @rating    , @plot  , @outline  , @year   , @runtime  , @sourceurl , @source ,@actressimageurl ,@smallimageurl ,@bigimageurl ,@extraimageurl) " +
               "ON CONFLICT(id) DO UPDATE SET title=@title    , releasedate=@releasedate  , director=@director  , genre=@genre  , tag=@tag  , actor=@actor  , actorid=@actorid  ,studio=@studio  , rating=@rating   , plot=@plot  , outline=@outline  , year=@year   , runtime=@runtime  , sourceurl=@sourceurl , source=@source ,actressimageurl=@actressimageurl ,smallimageurl=@smallimageurl ,bigimageurl=@bigimageurl ,extraimageurl=@extraimageurl";
            if (movie.vediotype != 3) movie.id = movie.id.ToUpper();//除了 欧美影片之外，其他影片必须大写
            cmd.CommandText = sqltext;
            cmd.Parameters.Add("id", DbType.String).Value = movie.id;
            cmd.Parameters.Add("title", DbType.String).Value = movie.title;
            cmd.Parameters.Add("releasedate", DbType.String).Value = movie.releasedate;
            cmd.Parameters.Add("director", DbType.String).Value = movie.director;
            cmd.Parameters.Add("genre", DbType.String).Value = movie.genre;
            cmd.Parameters.Add("tag", DbType.String).Value = movie.tag;
            cmd.Parameters.Add("actor", DbType.String).Value = movie.actor;
            cmd.Parameters.Add("actorid", DbType.String).Value = movie.actorid;
            cmd.Parameters.Add("studio", DbType.String).Value = movie.studio;
            cmd.Parameters.Add("rating", DbType.Double).Value = movie.rating;
            cmd.Parameters.Add("plot", DbType.String).Value = movie.plot;
            cmd.Parameters.Add("outline", DbType.String).Value = movie.outline;
            cmd.Parameters.Add("year", DbType.Int16).Value = movie.year;
            cmd.Parameters.Add("runtime", DbType.Int16).Value = movie.runtime;
            cmd.Parameters.Add("sourceurl", DbType.String).Value = movie.sourceurl;
            cmd.Parameters.Add("source", DbType.String).Value = movie.source;
            cmd.Parameters.Add("smallimageurl", DbType.String).Value = movie.smallimageurl;
            cmd.Parameters.Add("bigimageurl", DbType.String).Value = movie.bigimageurl;
            cmd.Parameters.Add("extraimageurl", DbType.String).Value = movie.extraimageurl;
            cmd.Parameters.Add("actressimageurl", DbType.String).Value = movie.actressimageurl;
            cmd.ExecuteNonQuery();
        }


        public void InsertSearchMovie(Movie movie, string table)
        {
            //20个数据
            if (movie.vediotype != 3) movie.id = movie.id.ToUpper();//除了 欧美影片之外，其他影片必须大写
            string sqltext = $"INSERT OR IGNORE INTO {table}(id  , title    , releasedate,vediotype,actor,otherinfo,sourceurl)  values(@id  , @title    , @releasedate,@vediotype,@actor,@otherinfo,@sourceurl) ";
            cmd.CommandText = sqltext;
            cmd.Parameters.Add("id", DbType.String).Value = movie.id;
            cmd.Parameters.Add("title", DbType.String).Value = movie.title;
            cmd.Parameters.Add("releasedate", DbType.String).Value = movie.releasedate;
            cmd.Parameters.Add("vediotype", DbType.Int16).Value = movie.vediotype;
            cmd.Parameters.Add("actor", DbType.String).Value = movie.actor;
            cmd.Parameters.Add("otherinfo", DbType.String).Value = movie.otherinfo;
            cmd.Parameters.Add("sourceurl", DbType.String).Value = movie.sourceurl;
            cmd.ExecuteNonQuery();
        }


        public void InsertScanedMovie(Movie movie)
        {
            string sqltext = "INSERT INTO movie(id , filesize ,filepath  , vediotype  ,scandate,subsection,otherinfo) values(@id , @filesize ,@filepath  , @vediotype  ,@scandate,@subsection,@otherinfo) ON CONFLICT(id) DO UPDATE SET filesize=@filesize,filepath=@filepath,scandate=@scandate,otherinfo=@otherinfo,vediotype=@vediotype,subsection=@subsection";
            cmd.CommandText = sqltext;
            if (movie.vediotype != 3) movie.id = movie.id.ToUpper();//除了 欧美影片之外，其他影片必须大写
            cmd.Parameters.Add("id", DbType.String).Value = movie.id;
            cmd.Parameters.Add("filesize", DbType.Double).Value = movie.filesize;
            cmd.Parameters.Add("filepath", DbType.String).Value = movie.filepath;
            cmd.Parameters.Add("vediotype", DbType.Int16).Value = movie.vediotype;
            cmd.Parameters.Add("scandate", DbType.String).Value = movie.scandate;
            cmd.Parameters.Add("otherinfo", DbType.String).Value = movie.otherinfo;
            cmd.Parameters.Add("subsection", DbType.String).Value = movie.subsection;
            cmd.ExecuteNonQuery();
        }


        public void InsertMagnet(Magnet magnet)
        {
            string sqltext = "INSERT OR IGNORE INTO magnets(link , id ,title  , size  ,releasedate,tag) values(@link , @id ,@title  , @size  ,@releasedate,@tag)";
            cmd.CommandText = sqltext;
            cmd.Parameters.Add("link", DbType.String).Value = magnet.link;
            cmd.Parameters.Add("id", DbType.String).Value = magnet.id;
            cmd.Parameters.Add("title", DbType.String).Value = magnet.title;
            cmd.Parameters.Add("size", DbType.Double).Value = magnet.size;
            cmd.Parameters.Add("releasedate", DbType.String).Value = magnet.releasedate;
            cmd.Parameters.Add("tag", DbType.String).Value = string.Join(" ", magnet.tag);
            cmd.ExecuteNonQuery();
        }


        public void InsertByField(string table, string field, string value, string id)
        {
            cmd.CommandText = $"insert into  {table}(id,{field}) values(@id,@{field}) ON CONFLICT(id) DO UPDATE SET {field}=@{field}";
            cmd.Parameters.Add("id", DbType.String).Value = id;
            cmd.Parameters.Add(field, DbType.String).Value = value;
            cmd.ExecuteNonQuery();
        }

        public void SaveBaiduAIByID(string id, Dictionary<string, string> dic)
        {
            cmd.CommandText = $"insert into  baidu (id,age,beauty,expression,face_shape,gender,glasses,race,emotion,mask) values(@id,@age,@beauty,@expression,@face_shape,@gender,@glasses,@race,@emotion,@mask) ON CONFLICT(id) DO UPDATE SET age=@age,beauty=@beauty,expression=@expression,face_shape=@face_shape,gender=@gender,glasses=@glasses,race=@race,emotion=@emotion,mask=@mask";
            cmd.Parameters.Add("id", DbType.String).Value = id;
            cmd.Parameters.Add("age", DbType.Int32).Value = dic["age"];
            cmd.Parameters.Add("beauty", DbType.Single).Value = dic["beauty"];
            cmd.Parameters.Add("expression", DbType.String).Value = dic["expression"];
            cmd.Parameters.Add("face_shape", DbType.String).Value = dic["face_shape"];
            cmd.Parameters.Add("gender", DbType.String).Value = dic["gender"];
            cmd.Parameters.Add("glasses", DbType.String).Value = dic["glasses"];
            cmd.Parameters.Add("race", DbType.String).Value = dic["race"];
            cmd.Parameters.Add("emotion", DbType.String).Value = dic["emotion"];
            cmd.Parameters.Add("mask", DbType.Boolean).Value = int.Parse(dic["mask"]) != 0;
            cmd.ExecuteNonQuery();
        }

        public void SaveYoudaoTranslateByID(string id, string value1, string value2, string type)
        {
            cmd.CommandText = $"insert into  youdao (id,{type},translate_{type}) values(@id,@{type},@translate_{type}) ON CONFLICT(id) DO UPDATE SET {type}=@{type},translate_{type}=@translate_{type}";
            cmd.Parameters.Add("id", DbType.String).Value = id;
            cmd.Parameters.Add(type, DbType.String).Value = value1;
            cmd.Parameters.Add($"translate_{type}", DbType.String).Value = value2;
            cmd.ExecuteNonQuery();
        }



        public string GetInfoBySql(string sql)
        {
            string result = "";
            cmd.CommandText = sql;
            try
            {
                using (SQLiteDataReader sr = cmd.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        if (sr[0] != null) result = sr[0].ToString();
                        if (result != "") { break; }
                    }
                }
            }
            catch (Exception ex) { Logger.LogD(ex); }
            return result;
        }


        public List<string> GetInfosBySql(string sql)
        {
            List<string> result = new List<string>();
            cmd.CommandText = sql;
            try
            {
                using (SQLiteDataReader sr = cmd.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        if (sr[0] != null) result.Add(sr[0].ToString());
                    }
                }
            }
            catch (Exception ex) { Logger.LogD(ex); }

            return result;
        }


    }



}
