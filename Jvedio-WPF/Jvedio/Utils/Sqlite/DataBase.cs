using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Jvedio.Comparer;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;

namespace Jvedio
{
    /// <summary>
    /// SqliteHelper
    /// </summary>
    public static class DataBase
    {
        public static string SQLITETABLE_MOVIE = "create table if not exists movie (id VARCHAR(50) PRIMARY KEY , title TEXT , filesize DOUBLE DEFAULT 0 , filepath TEXT , subsection TEXT , vediotype INT , scandate VARCHAR(30) , releasedate VARCHAR(10) DEFAULT '1900-01-01', visits INT  DEFAULT 0, director VARCHAR(50) , genre TEXT , tag TEXT , actor TEXT , actorid TEXT ,studio VARCHAR(50) , rating FLOAT  DEFAULT 0, chinesetitle TEXT , favorites INT  DEFAULT 0, label TEXT , plot TEXT , outline TEXT , year INT  DEFAULT 1900, runtime INT  DEFAULT 0, country VARCHAR(50) , countrycode INT DEFAULT 0 ,otherinfo TEXT, sourceurl TEXT, source VARCHAR(10),actressimageurl TEXT,smallimageurl TEXT,bigimageurl TEXT,extraimageurl TEXT)";
        public static string SQLITETABLE_ACTRESS = "create table if not exists actress ( id VARCHAR(50) PRIMARY KEY, name VARCHAR(50) ,birthday VARCHAR(10) ,age INT ,height INT ,cup VARCHAR(1), chest INT ,waist INT ,hipline INT ,birthplace VARCHAR(50) ,hobby TEXT, sourceurl TEXT, source VARCHAR(10),imageurl TEXT)";
        public static string SQLITETABLE_ACTRESS_LOVE = "create table if not exists actresslove ( name VARCHAR(50) PRIMARY KEY,islove INT )";
        public static string SQLITETABLE_LIBRARY = "create table if not exists library ( id VARCHAR(50) PRIMARY KEY, code VARCHAR(50))";
        public static string SQLITETABLE_JAVDB = "create table if not exists javdb ( id VARCHAR(50) PRIMARY KEY, code VARCHAR(50))";
        public static string SQLITETABLE_BAIDUAI = "create table if not exists baidu (id VARCHAR(50) PRIMARY KEY , age INT DEFAULT 0 , beauty FLOAT DEFAULT 0 , expression VARCHAR(20), face_shape VARCHAR(20), gender VARCHAR(20), glasses VARCHAR(20), race VARCHAR(20), emotion VARCHAR(20), mask VARCHAR(20))";
        public static string SQLITETABLE_BAIDUTRANSLATE = "create table if not exists baidu (id VARCHAR(50) PRIMARY KEY , title TEXT , translate_title TEXT, plot TEXT, translate_plot TEXT)";
        public static string SQLITETABLE_YOUDAO = "create table if not exists youdao (id VARCHAR(50) PRIMARY KEY , title TEXT , translate_title TEXT, plot TEXT, translate_plot TEXT)";
        public static string SQLITETABLE_MAGNETS = "create table if not exists magnets (link VARCHAR(60) PRIMARY KEY, id VARCHAR(50) , title TEXT , size  DOUBLE DEFAULT 0 , releasedate VARCHAR(10) DEFAULT '1900-01-01', tag TEXT)";


        public static string SqlitePath { get; set; }



        public static void Init()
        {
            SqlitePath = Properties.Settings.Default.DataBasePath;
        }




        public static void CreateTable(string sqltext)
        {
            MySqlite sqlite = new MySqlite(SqlitePath);
            sqlite.ExecuteSql(sqltext);
            sqlite.Close();
        }



        public static bool IsProperSqlite(string path)
        {
            if (!File.Exists(path)) return false;
            //是否具有表结构
            bool result = true;
            MySqlite db = new MySqlite(path, true);
            if (!db.IsTableExist("movie") || !db.IsTableExist("actress") || !db.IsTableExist("library") || !db.IsTableExist("javdb")) result = false;
            db.CloseDB();
            return result;
        }

        private static DataTable getDataTable(string src, string table)
        {
            DataTable dataTable = new DataTable();
            using (SQLiteConnection conn = new SQLiteConnection("data source=" + src))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.CommandText = "SELECT * FROM " + table;
                    SQLiteDataReader sQLiteDataReader = cmd.ExecuteReader();
                    dataTable.Load(sQLiteDataReader);
                }
            }
            return dataTable;
        }


        /// <summary>
        /// 数据库之间复制数据
        /// </summary>
        /// <param name="src">原数据库</param>
        /// <param name="dst">目标数据库</param>
        /// <param name="ct"></param>
        /// <param name="callback"></param>
        /// <param name="skipnulltitle"></param>
        public static void CopyDatabaseInfo(string src, string dst, CancellationToken ct, Action<int> callback = null, bool skipnulltitle = true)
        {

            // movie表
            DataTable dataTable = getDataTable(src, "movie");
            string sqltext = $"INSERT INTO movie(id  , title  , filesize  , filepath  , subsection  , vediotype  , scandate  , releasedate , visits , director  , genre  , tag  , actor  , actorid  ,studio  , rating , chinesetitle  , favorites  , label  , plot  , outline  , year   , runtime , country  , countrycode ,otherinfo , sourceurl , source ,actressimageurl ,smallimageurl ,bigimageurl ,extraimageurl ) " +
                "values(@id  , @title  , @filesize  , @filepath  , @subsection  , @vediotype  , @scandate  , @releasedate , @visits , @director  , @genre  , @tag  , @actor  , @actorid  ,@studio  , @rating , @chinesetitle  , @favorites  ,@label  , @plot  , @outline  , @year   , @runtime , @country  , @countrycode ,@otherinfo , @sourceurl , @source ,@actressimageurl ,@smallimageurl ,@bigimageurl ,@extraimageurl) " +
                "ON CONFLICT(id) DO UPDATE SET title=@title  , filesize=@filesize  , filepath=@filepath  , subsection=@subsection  , vediotype=@vediotype  , scandate=@scandate  , releasedate=@releasedate , visits=@visits , director=@director  , genre=@genre  , tag=@tag  , actor=@actor  , actorid=@actorid  ,studio=@studio  , rating=@rating , chinesetitle=@chinesetitle  ,favorites=@favorites  ,label=@label  , plot=@plot  , outline=@outline  , year=@year   , runtime=@runtime , country=@country  , countrycode=@countrycode ,otherinfo=@otherinfo , sourceurl=@sourceurl , source=@source ,actressimageurl=@actressimageurl ,smallimageurl=@smallimageurl ,bigimageurl=@bigimageurl ,extraimageurl=@extraimageurl";
            using (SQLiteConnection conn = new SQLiteConnection("data source=" + dst))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    double idx = 0;
                    double total = dataTable.Rows.Count > 0 ? dataTable.Rows.Count : 1;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        idx++;
                        int progress = (int)(idx / total * 100);
                        callback?.Invoke(progress);
                        ct.ThrowIfCancellationRequested();
                        EnumerableRowCollection<DataRow> dataRows = dataTable.AsEnumerable().Where(myRow => myRow.Field<string>("id") == row["id"].ToString());
                        DataRow dataRow = null;
                        if (dataRows != null && dataRows.Count() > 0)
                            dataRow = dataRows.First();
                        else continue;

                        if (skipnulltitle && string.IsNullOrEmpty(dataRow["title"].ToString())) continue;

                        cmd.CommandText = sqltext;

                        cmd.Parameters.Add("id", DbType.String).Value = dataRow["id"];
                        cmd.Parameters.Add("title", DbType.String).Value = dataRow["title"];
                        cmd.Parameters.Add("filesize", DbType.Double).Value = dataRow["filesize"];
                        cmd.Parameters.Add("filepath", DbType.String).Value = dataRow["filepath"];
                        cmd.Parameters.Add("subsection", DbType.String).Value = dataRow["subsection"];
                        cmd.Parameters.Add("vediotype", DbType.Int16).Value = dataRow["vediotype"];
                        cmd.Parameters.Add("scandate", DbType.String).Value = dataRow["scandate"];
                        cmd.Parameters.Add("releasedate", DbType.String).Value = dataRow["releasedate"];
                        cmd.Parameters.Add("visits", DbType.Int16).Value = dataRow["visits"];
                        cmd.Parameters.Add("director", DbType.String).Value = dataRow["director"];
                        cmd.Parameters.Add("genre", DbType.String).Value = dataRow["genre"];
                        cmd.Parameters.Add("tag", DbType.String).Value = dataRow["tag"];
                        cmd.Parameters.Add("actor", DbType.String).Value = dataRow["actor"];
                        cmd.Parameters.Add("actorid", DbType.String).Value = dataRow["actorid"];
                        cmd.Parameters.Add("studio", DbType.String).Value = dataRow["studio"];
                        cmd.Parameters.Add("rating", DbType.Double).Value = dataRow["rating"];
                        cmd.Parameters.Add("chinesetitle", DbType.String).Value = dataRow["chinesetitle"];
                        cmd.Parameters.Add("favorites", DbType.Int16).Value = dataRow["favorites"];
                        cmd.Parameters.Add("label", DbType.String).Value = dataRow["label"];
                        cmd.Parameters.Add("plot", DbType.String).Value = dataRow["plot"];
                        cmd.Parameters.Add("outline", DbType.String).Value = dataRow["outline"];
                        cmd.Parameters.Add("year", DbType.Int16).Value = dataRow["year"];
                        cmd.Parameters.Add("runtime", DbType.Int16).Value = dataRow["runtime"];
                        cmd.Parameters.Add("country", DbType.String).Value = dataRow["country"];
                        cmd.Parameters.Add("countrycode", DbType.Int16).Value = dataRow["countrycode"];
                        cmd.Parameters.Add("otherinfo", DbType.String).Value = dataRow["otherinfo"];
                        cmd.Parameters.Add("sourceurl", DbType.String).Value = dataRow["sourceurl"];
                        cmd.Parameters.Add("source", DbType.String).Value = dataRow["source"];
                        cmd.Parameters.Add("smallimageurl", DbType.String).Value = dataRow["smallimageurl"];
                        cmd.Parameters.Add("bigimageurl", DbType.String).Value = dataRow["bigimageurl"];
                        cmd.Parameters.Add("extraimageurl", DbType.String).Value = dataRow["extraimageurl"];
                        cmd.Parameters.Add("actressimageurl", DbType.String).Value = dataRow["actressimageurl"];
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // actress
            ct.ThrowIfCancellationRequested();
            dataTable = getDataTable(src, "actress");
            sqltext = $"insert into actress(id,name ,  birthday , age ,height, cup, chest  , waist , hipline ,birthplace  , hobby,sourceurl ,source,imageurl) values(@id,@name ,  @birthday , @age ,@height, @cup, @chest  , @waist , @hipline ,@birthplace  , @hobby,@sourceurl ,@source,@imageurl) on conflict(id) do update set name=@name ,  birthday=@birthday , age=@age  ,height=@height, cup=@cup, chest=@chest  , waist=@waist , hipline=@hipline ,birthplace=@birthplace  , hobby=@hobby,sourceurl=@sourceurl ,source=@source,imageurl=@imageurl";
            using (SQLiteConnection conn = new SQLiteConnection("data source=" + dst))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    double idx = 0;
                    double total = dataTable.Rows.Count > 0 ? dataTable.Rows.Count : 1;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        idx++;
                        int progress = (int)(idx / total * 100);
                        Console.WriteLine(progress);
                        callback?.Invoke(progress);



                        ct.ThrowIfCancellationRequested();
                        EnumerableRowCollection<DataRow> dataRows = dataTable.AsEnumerable().Where(myRow => myRow.Field<string>("id") == row["id"].ToString());
                        DataRow dataRow = null;
                        if (dataRows != null && dataRows.Count() > 0)
                            dataRow = dataRows.First();
                        else continue;
                        cmd.CommandText = sqltext;
                        cmd.Parameters.Add("id", DbType.String).Value = dataRow["id"];
                        cmd.Parameters.Add("name", DbType.String).Value = dataRow["name"];
                        cmd.Parameters.Add("birthday", DbType.String).Value = dataRow["birthday"];
                        cmd.Parameters.Add("age", DbType.Int16).Value = dataRow["age"];
                        cmd.Parameters.Add("height", DbType.Int16).Value = dataRow["height"];
                        cmd.Parameters.Add("cup", DbType.String).Value = dataRow["cup"];
                        cmd.Parameters.Add("chest", DbType.Int16).Value = dataRow["chest"];
                        cmd.Parameters.Add("waist", DbType.Int16).Value = dataRow["waist"];
                        cmd.Parameters.Add("hipline", DbType.Int16).Value = dataRow["hipline"];
                        cmd.Parameters.Add("birthplace", DbType.String).Value = dataRow["birthplace"];
                        cmd.Parameters.Add("hobby", DbType.String).Value = dataRow["hobby"];
                        cmd.Parameters.Add("sourceurl", DbType.String).Value = dataRow["sourceurl"];
                        cmd.Parameters.Add("source", DbType.String).Value = dataRow["source"];
                        cmd.Parameters.Add("imageurl", DbType.String).Value = dataRow["imageurl"];
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // library
            ct.ThrowIfCancellationRequested();
            dataTable = getDataTable(src, "library");
            sqltext = $"insert into library(id,code) values(@id,@code) on conflict(id) do update set code=@code ";
            using (SQLiteConnection conn = new SQLiteConnection("data source=" + dst))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    double idx = 0;
                    double total = dataTable.Rows.Count > 0 ? dataTable.Rows.Count : 1;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        idx++;
                        int progress = (int)(idx / total * 100);
                        Console.WriteLine(progress);
                        callback?.Invoke(progress);

                        ct.ThrowIfCancellationRequested();
                        EnumerableRowCollection<DataRow> dataRows = dataTable.AsEnumerable().Where(myRow => myRow.Field<string>("id") == row["id"].ToString());
                        DataRow dataRow = null;
                        if (dataRows != null && dataRows.Count() > 0)
                            dataRow = dataRows.First();
                        else continue;
                        cmd.CommandText = sqltext;
                        cmd.Parameters.Add("id", DbType.String).Value = dataRow["id"];
                        cmd.Parameters.Add("code", DbType.String).Value = dataRow["code"];
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // javdb
            ct.ThrowIfCancellationRequested();
            dataTable = getDataTable(src, "javdb");
            sqltext = $"insert into javdb(id,code) values(@id,@code) on conflict(id) do update set code=@code ";
            using (SQLiteConnection conn = new SQLiteConnection("data source=" + dst))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    double idx = 0;
                    double total = dataTable.Rows.Count > 0 ? dataTable.Rows.Count : 1;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        idx++;
                        int progress = (int)(idx / total * 100);
                        Console.WriteLine(progress);
                        callback?.Invoke(progress);

                        ct.ThrowIfCancellationRequested();
                        EnumerableRowCollection<DataRow> dataRows = dataTable.AsEnumerable().Where(myRow => myRow.Field<string>("id") == row["id"].ToString());
                        DataRow dataRow = null;
                        if (dataRows != null && dataRows.Count() > 0)
                            dataRow = dataRows.First();
                        else continue;
                        cmd.CommandText = sqltext;
                        cmd.Parameters.Add("id", DbType.String).Value = dataRow["id"];
                        cmd.Parameters.Add("code", DbType.String).Value = dataRow["code"];
                        cmd.ExecuteNonQuery();
                    }
                }
            }


            // actresslove
            ct.ThrowIfCancellationRequested();
            dataTable = getDataTable(src, "actresslove");
            sqltext = $"insert into actresslove(name,islove) values(@name,@islove) on conflict(name) do update set islove=@islove ";

            using (MySqlite mySqlite = new MySqlite(dst, true))
            {
                bool istableExist = mySqlite.IsTableExist("actresslove");
                if (!istableExist)
                {
                    mySqlite.CreateTable(DataBase.SQLITETABLE_ACTRESS_LOVE);
                }
            }




            using (SQLiteConnection conn = new SQLiteConnection("data source=" + dst))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    double idx = 0;
                    double total = dataTable.Rows.Count > 0 ? dataTable.Rows.Count : 1;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        idx++;
                        int progress = (int)(idx / total * 100);
                        Console.WriteLine(progress);
                        callback?.Invoke(progress);

                        ct.ThrowIfCancellationRequested();
                        EnumerableRowCollection<DataRow> dataRows = dataTable.AsEnumerable().Where(myRow => myRow.Field<string>("name") == row["name"].ToString());
                        DataRow dataRow = null;
                        if (dataRows != null && dataRows.Count() > 0)
                            dataRow = dataRows.First();
                        else continue;
                        cmd.CommandText = sqltext;
                        cmd.Parameters.Add("name", DbType.String).Value = dataRow["name"];
                        cmd.Parameters.Add("islove", DbType.Int32).Value = dataRow["islove"];
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        #region "SELECT"

        public static double SelectCountBySql(string sql)
        {
            using (MySqlite sqlite = new MySqlite(SqlitePath, true))
            {
                return sqlite.SelectCountByTable("movie", "id", sql);
            }
        }





        public static string GenerateSort()
        {
            int.TryParse(Properties.Settings.Default.SortType, out int st);
            Sort SortType = (Sort)st;
            bool SortDescending = Properties.Settings.Default.SortDescending;
            return $"ORDER BY {SortType.ToSqlString()} {(SortDescending ? "DESC" : "ASC")}";
        }

        public static List<Movie> SelectPartialInfo(string sql)
        {
            Init();
            string order = GenerateSort();
            using (SQLiteConnection cn = new SQLiteConnection("data source=" + SqlitePath))
            {
                cn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    List<Movie> result = new List<Movie>();

                    cmd.CommandText = $"{sql} {order}";
                    using (SQLiteDataReader sr = cmd.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            int vt = 1;
                            Movie movie = new Movie()
                            {
                                id = sr["id"].ToString(),
                                filepath = sr["filepath"].ToString(),
                                title = sr["title"].ToString(),
                                actor = sr["actor"].ToString(),
                                releasedate = sr["releasedate"].ToString(),
                                subsection = sr["subsection"].ToString()
                            };
                            int.TryParse(sr["releasedate"].ToString(), out vt);
                            int.TryParse(sr["favorites"].ToString(), out int favorites);
                            movie.vediotype = vt;
                            movie.favorites = favorites;
                            result.Add(movie);
                        }
                    }


                    List<Movie> movies = new List<Movie>();
                    movies.AddRange(result);
                    return result;
                }
            }
        }



        public static List<string> SelectAllID()
        {
            Init();
            using (SQLiteConnection cn = new SQLiteConnection("data source=" + SqlitePath))
            {
                cn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    List<string> result = new List<string>();

                    cmd.CommandText = "select id from movie";
                    using (SQLiteDataReader sr = cmd.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            result.Add(sr[0].ToString());
                        }
                    }
                    return result;
                }
            }
        }




        private static Dictionary<string, int> SelectLabelLikeByVedioType(string field, int vediotype, object splitchar = null)
        {
            Init();
            Dictionary<string, int> dicresult = new Dictionary<string, int>();
            using (SQLiteConnection cn = new SQLiteConnection("data source=" + SqlitePath))
            {
                cn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;

                    if (vediotype == 0)
                        cmd.CommandText = $"SELECT {field} FROM movie where vediotype>=0";
                    else
                        cmd.CommandText = $"SELECT {field} FROM movie where vediotype={vediotype}";


                    char[] splitChar = { ' ' };
                    if (splitChar != null) splitChar = (char[])splitchar;
                    using (SQLiteDataReader sr = cmd.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            if (sr[0] != null && !string.IsNullOrEmpty(sr[0].ToString()))
                            {
                                string[] label = sr[0].ToString().Split(splitChar);
                                for (int i = 0; i < label.Length; i++)
                                {
                                    if (!string.IsNullOrEmpty(label[i]))
                                    {
                                        if (dicresult.ContainsKey(label[i]))
                                            dicresult[label[i]] += 1;
                                        else
                                            dicresult.Add(label[i], 1);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return dicresult;
        }





        /// <summary>
        /// 获得标签，如：奇葩( 20 )
        /// </summary>
        /// <param name="vediotype"></param>
        /// <returns></returns>

        public static List<string> SelectLabelByVedioType(VedioType vediotype, string type = "label")
        {
            Dictionary<string, int> dicresult = SelectLabelLikeByVedioType(type, (int)vediotype);
            //降序
            var dicSort = from objDic in dicresult orderby objDic.Value descending select objDic;
            List<string> result = new List<string>();
            foreach (var item in dicSort)
                result.Add(item.Key + "( " + item.Value + " )");
            return result;
        }



        /// <summary>
        /// 获得类别列表
        /// </summary>
        /// <param name="vediotype"></param>
        /// <returns></returns>
        public static List<Genre> SelectGenreByVedioType(VedioType vediotype)
        {
            Dictionary<string, int> dicresult = SelectLabelLikeByVedioType("genre", (int)vediotype);
            //降序
            var dicSort = from objDic in dicresult orderby objDic.Value descending select objDic;
            List<Genre> result = new List<Genre>();
            Genre newgenre = new Genre();
            foreach (var item in dicSort)
            {
                if (vediotype == VedioType.欧美)
                {
                    if (GenreEurope[0].IndexOf(item.Key) > 0) { newgenre.theme.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreEurope[1].IndexOf(item.Key) > 0) { newgenre.role.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreEurope[2].IndexOf(item.Key) > 0) { newgenre.clothing.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreEurope[3].IndexOf(item.Key) > 0) { newgenre.body.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreEurope[4].IndexOf(item.Key) > 0) { newgenre.behavior.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreEurope[5].IndexOf(item.Key) > 0) { newgenre.playmethod.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreEurope[6].IndexOf(item.Key) > 0) { newgenre.other.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreEurope[7].IndexOf(item.Key) > 0) { newgenre.scene.Add(item.Key + "( " + item.Value + " )"); continue; }
                    newgenre.other.Add(item.Key + "( " + item.Value + " )");
                }
                else if (vediotype == VedioType.骑兵)
                {
                    if (GenreCensored[0].IndexOf(item.Key) > 0) { newgenre.theme.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreCensored[1].IndexOf(item.Key) > 0) { newgenre.role.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreCensored[2].IndexOf(item.Key) > 0) { newgenre.clothing.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreCensored[3].IndexOf(item.Key) > 0) { newgenre.body.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreCensored[4].IndexOf(item.Key) > 0) { newgenre.behavior.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreCensored[5].IndexOf(item.Key) > 0) { newgenre.playmethod.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreCensored[6].IndexOf(item.Key) > 0) { newgenre.other.Add(item.Key + "( " + item.Value + " )"); continue; }
                    newgenre.other.Add(item.Key + "( " + item.Value + " )");
                }
                else if (vediotype == VedioType.步兵)
                {
                    if (GenreUncensored[0].IndexOf(item.Key) > 0) { newgenre.theme.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[1].IndexOf(item.Key) > 0) { newgenre.role.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[2].IndexOf(item.Key) > 0) { newgenre.clothing.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[3].IndexOf(item.Key) > 0) { newgenre.body.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[4].IndexOf(item.Key) > 0) { newgenre.behavior.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[5].IndexOf(item.Key) > 0) { newgenre.playmethod.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[6].IndexOf(item.Key) > 0) { newgenre.other.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[7].IndexOf(item.Key) > 0) { newgenre.scene.Add(item.Key + "( " + item.Value + " )"); continue; }
                    newgenre.other.Add(item.Key + "( " + item.Value + " )");
                }
                else if (vediotype == VedioType.所有)
                {
                    if (GenreUncensored[0].IndexOf(item.Key) > 0 | GenreCensored[0].IndexOf(item.Key) > 0) { newgenre.theme.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[1].IndexOf(item.Key) > 0 | GenreCensored[1].IndexOf(item.Key) > 0) { newgenre.role.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[2].IndexOf(item.Key) > 0 | GenreCensored[2].IndexOf(item.Key) > 0) { newgenre.clothing.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[3].IndexOf(item.Key) > 0 | GenreCensored[3].IndexOf(item.Key) > 0) { newgenre.body.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[4].IndexOf(item.Key) > 0 | GenreCensored[4].IndexOf(item.Key) > 0) { newgenre.behavior.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[5].IndexOf(item.Key) > 0 | GenreCensored[5].IndexOf(item.Key) > 0) { newgenre.playmethod.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[6].IndexOf(item.Key) > 0 | GenreCensored[6].IndexOf(item.Key) > 0) { newgenre.other.Add(item.Key + "( " + item.Value + " )"); continue; }
                    if (GenreUncensored[7].IndexOf(item.Key) > 0) { newgenre.scene.Add(item.Key + "( " + item.Value + " )"); continue; }
                    newgenre.other.Add(item.Key + "( " + item.Value + " )");
                }
            }
            result.Add(newgenre);
            return result;
        }


        /// <summary>
        /// 获得演员，如：托尼( 20 )
        /// </summary>
        /// <param name="vediotype"></param>
        /// <returns></returns>
        public static List<Actress> SelectAllActorName(VedioType vediotype)
        {
            Dictionary<string, int> dicresult = SelectLabelLikeByVedioType("actor", (int)vediotype, actorSplitDict[(int)vediotype]);
            //降序
            var dicSort = from objDic in dicresult orderby objDic.Value descending select objDic;
            List<Actress> result = new List<Actress>();
            foreach (var item in dicSort)
            {
                Actress actress = new Actress()
                {
                    id = "",
                    num = item.Value,
                    name = (item.Key.IndexOf(";") > 0 ? item.Key.Split(';')[0] : item.Key)
                };
                result.Add(actress);
            }
            return result;
        }




        /// <summary>
        /// 通过 sql 获得影片列表
        /// </summary>
        /// <param name="sqltext"></param>
        /// <returns></returns>
        public static List<Movie> SelectMoviesBySql(string sqltext, string dbName = "")
        {
            if (dbName == "") Init();
            else
                SqlitePath = dbName.EndsWith(".sqlite") ? dbName : $"{dbName}.sqlite";

            List<Movie> result = new List<Movie>();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                result = mySqlite.SelectMoviesBySql(sqltext);
            }
            List<Movie> movies = new List<Movie>();
            movies.AddRange(result);
            return result;
        }


        /// <summary>
        /// 异步获得单个影片
        /// </summary>
        /// <param name="movieid"></param>
        /// <returns></returns>
        public static Movie SelectMovieByID(string movieid)
        {
            if (string.IsNullOrEmpty(movieid)) return null;
            Init();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                return mySqlite.SelectMovieBySql($"select * from movie where id='{movieid}'");
            }
        }


        /// <summary>
        /// 加载影片详细信息
        /// </summary>
        /// <param name="movieid"></param>
        /// <returns></returns>
        public static DetailMovie SelectDetailMovieById(string movieid)
        {
            DetailMovie result = null;
            if (string.IsNullOrEmpty(movieid)) return null;
            Init();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                result = mySqlite.SelectDetailMovieBySql($"select * from movie where id='{movieid}'");
            }
            if (result == null) return null;

            foreach (var item in result.genre.Split(' ')) { if (!string.IsNullOrEmpty(item) && item.IndexOf(' ') < 0) { result.genrelist.Add(item); } }
            foreach (var item in result.label.Split(' ')) { if (!string.IsNullOrEmpty(item) && item.IndexOf(' ') < 0) { result.labellist.Add(item); } }

            if (result.actor.Split(actorSplitDict[result.vediotype]).Count() == result.actorid.Split(actorSplitDict[result.vediotype]).Count() && result.actor.Split(actorSplitDict[result.vediotype]).Count() > 1)
            {
                //演员数目>1
                string[] Name = result.actor.Split(actorSplitDict[result.vediotype]);
                for (int i = 0; i < Name.Length; i++)
                {
                    if (!string.IsNullOrEmpty(Name[i]))
                    {
                        Actress actress = new Actress() { id = "", name = Name[i] };
                        result.actorlist.Add(actress);
                    }
                }
            }
            else
            {
                //演员数目<=1
                foreach (var item in result.actor.Split(actorSplitDict[result.vediotype]))
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        Actress actress = new Actress() { id = "", name = item };
                        result.actorlist.Add(actress);
                    }
                }
            }
            return result;


        }

        /// <summary>
        /// 读取演员信息
        /// </summary>
        /// <param name="actress"></param>
        /// <returns></returns>

        public static Actress SelectInfoByActress(Actress actress)
        {
            if (string.IsNullOrEmpty(actress.name)) { return actress; }
            Init();
            using (SQLiteConnection cn = new SQLiteConnection("data source=" + SqlitePath))
            {
                cn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = $"select * from actress where name='{actress.name}'";
                    using (SQLiteDataReader sr = cmd.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            actress.birthday = sr["birthday"].ToString();
                            actress.id = sr["id"].ToString();
                            int.TryParse(sr["age"].ToString(), out int v1); actress.age = v1;
                            int.TryParse(sr["height"].ToString(), out int v2); actress.height = v2;
                            int.TryParse(sr["chest"].ToString(), out int v3); actress.chest = v3;
                            int.TryParse(sr["waist"].ToString(), out int v4); actress.waist = v4;
                            int.TryParse(sr["hipline"].ToString(), out int v5); actress.hipline = v5;

                            actress.cup = sr["cup"].ToString();
                            actress.birthplace = sr["birthplace"].ToString();
                            actress.hobby = sr["hobby"].ToString();
                            actress.source = sr["source"].ToString();
                            actress.sourceurl = sr["sourceurl"].ToString();
                            actress.imageurl = sr["imageurl"].ToString();
                            break;
                        }
                    }

                    //载入是否喜爱
                    cmd.CommandText = $"select * from actresslove where name='{actress.name}'";
                    try
                    {
                        using (SQLiteDataReader sr = cmd.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                int.TryParse(sr["islove"].ToString(), out int v1); actress.like = v1;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        actress.like = 0;
                    }
                    return actress;
                }
            }



        }


        public static List<string> SelectActressNameByLove(int islove)
        {
            Init();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                return mySqlite.GetInfosBySql($"select * from actresslove where islove={islove}");
            }
        }


        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="table">表</param>
        /// <returns></returns>
        public static bool IsTableExist(string table)
        {
            Init();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                return mySqlite.IsTableExist(table);
            }
        }

        /// <summary>
        /// 执行数据库命令：select from where
        /// </summary>
        /// <param name="info"></param>
        /// <param name="table"></param>
        /// <param name="field"></param>
        /// <param name="fieldvalue"></param>
        /// <returns></returns>
        public static string SelectInfoByID(string info, string table, string id)
        {
            Init();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                return mySqlite.SelectByField(info, table, id);
            }
        }


        public static List<Magnet> SelectMagnetsByID(string ID)
        {

            List<Magnet> result = new List<Magnet>();
            using (MySqlite mySqlite = new MySqlite("Magnets"))
            {
                result = mySqlite.SelectMagnetsBySql($"select * from magnets where id='{ID}'");
            }
            return result;
        }


        #endregion

        #region "DELETE"

        //删除
        public static void DeleteByField(string table, string field, string value)
        {
            Init();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                mySqlite.DeleteByField(table, field, value);
            }
        }





        #endregion


        #region "UPDATE"

        /// <summary>
        /// 保存单个信息到数据库
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="value"></param>
        /// <param name="savetype"></param>
        public static void UpdateMovieByID(string id, string content, object value, string savetype = "Int")
        {
            Init();
            value = value.ToString().ToProperSql(false);
            using (SQLiteConnection cn = new SQLiteConnection("data source=" + SqlitePath))
            {
                cn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    string sqltext;
                    if (savetype == "Int")
                        sqltext = $"UPDATE movie SET {content} = {value} WHERE id = '{id}'";
                    else
                        sqltext = $"UPDATE movie SET {content} = '{value}' WHERE id = '{id}'";
                    cmd.CommandText = sqltext;
                    cmd.ExecuteNonQuery();
                }
            }
        }



        /// <summary>
        /// 删除已下载的信息
        /// </summary>
        /// <param name="id"></param>
        public static void ClearInfoByID(string id)
        {
            Init();
            using (SQLiteConnection cn = new SQLiteConnection("data source=" + SqlitePath))
            {
                cn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    string sqltext = $"update movie SET title=@title,rating=@rating ,plot=@plot,outline=@outline,  releasedate=@releasedate , director=@director  ,actorid=@actorid, tag=@tag, genre=@genre  , actor=@actor , studio=@studio ,year=@year  , runtime=@runtime,bigimageurl=@bigimageurl ,smallimageurl=@smallimageurl ,extraimageurl=@extraimageurl,actressimageurl=@actressimageurl,sourceurl=@sourceurl,source=@source where id ='{id}'";
                    cmd.CommandText = sqltext;
                    cmd.Parameters.Add("actorid", DbType.String).Value = "";
                    cmd.Parameters.Add("tag", DbType.String).Value = "";
                    cmd.Parameters.Add("runtime", DbType.Int32).Value = 0;
                    cmd.Parameters.Add("rating", DbType.Int32).Value = 0;
                    cmd.Parameters.Add("actressimageurl", DbType.String).Value = "";
                    cmd.Parameters.Add("title", DbType.String).Value = "";
                    cmd.Parameters.Add("plot", DbType.String).Value = "";
                    cmd.Parameters.Add("outline", DbType.String).Value = "";
                    cmd.Parameters.Add("releasedate", DbType.String).Value = "1970-01-01";
                    cmd.Parameters.Add("director", DbType.String).Value = "";
                    cmd.Parameters.Add("genre", DbType.String).Value = "";
                    cmd.Parameters.Add("actor", DbType.String).Value = "";
                    cmd.Parameters.Add("studio", DbType.String).Value = "";
                    cmd.Parameters.Add("year", DbType.Int32).Value = 1970;
                    cmd.Parameters.Add("extraimageurl", DbType.String).Value = "";
                    cmd.Parameters.Add("sourceurl", DbType.String).Value = "";
                    cmd.Parameters.Add("source", DbType.String).Value = "";
                    cmd.Parameters.Add("bigimageurl", DbType.String).Value = "";
                    cmd.Parameters.Add("smallimageurl", DbType.String).Value = "";
                    cmd.ExecuteNonQuery();
                }
            }




        }



        public static Movie DicToMovie(Dictionary<string, string> info)
        {

            Movie movie = new Movie(info["id"]);
            if (info.ContainsKey("vediotype"))
            {
                int vt = 1;
                int.TryParse(info["vediotype"], out vt);
                movie.vediotype = vt;
            }

            if (info.ContainsKey("year"))
            {
                int year = 1970;
                int.TryParse(info["year"], out year);
                movie.year = year;
            }

            if (info.ContainsKey("rating"))
            {
                float.TryParse(info["rating"], out float rating);
                movie.rating = rating;
            }

            if (info.ContainsKey("runtime"))
            {
                int.TryParse(info["runtime"], out int runtime);
                movie.runtime = runtime;
            }


            movie.title = info.ContainsKey("title") ? info["title"] : "";
            movie.plot = info.ContainsKey("plot") ? info["plot"] : "";
            movie.outline = info.ContainsKey("outline") ? info["outline"] : "";
            movie.releasedate = info.ContainsKey("releasedate") ? info["releasedate"] : "1970-01-01";
            movie.director = info.ContainsKey("director") ? info["director"] : "";
            movie.tag = info.ContainsKey("tag") ? info["tag"] : "";
            movie.genre = info.ContainsKey("genre") ? info["genre"] : "";
            movie.actor = info.ContainsKey("actor") ? info["actor"] : "";
            movie.actorid = info.ContainsKey("actorid") ? info["actorid"] : "";
            movie.studio = info.ContainsKey("studio") ? info["studio"] : "";
            movie.sourceurl = info.ContainsKey("sourceurl") ? info["sourceurl"] : "";
            movie.source = info.ContainsKey("source") ? info["source"] : "";
            movie.bigimageurl = info.ContainsKey("bigimageurl") ? info["bigimageurl"] : "";
            movie.smallimageurl = info.ContainsKey("smallimageurl") ? info["smallimageurl"] : "";
            movie.extraimageurl = info.ContainsKey("extraimageurl") ? info["extraimageurl"] : "";
            movie.actressimageurl = info.ContainsKey("actressimageurl") ? info["actressimageurl"] : "";
            return movie;

        }

        /// <summary>
        /// 插入下载的信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="webSite"></param>
        public static void UpdateInfoFromNet(Dictionary<string, string> info)
        {
            if (info == null || !info.ContainsKey("id") || string.IsNullOrEmpty(info["id"])) return;
            Init();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                mySqlite.InsertCrawledMovie(DicToMovie(info), "movie");
            }

        }

        #endregion


        #region "INSERT"


        /// <summary>
        /// 插入 完整 的数据
        /// </summary>
        /// <param name="movie"></param>
        public static void InsertFullMovie(Movie movie)
        {
            Init();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                mySqlite.InsertFullMovie(movie, "movie");
            }
        }

        /// <summary>
        /// 插入扫描的数据
        /// </summary>
        /// <param name="movie"></param>
        public static void InsertScanMovie(Movie movie)
        {
            Init();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                mySqlite.InsertScanedMovie(movie);
            }
        }

        public static void InsertSearchMovie(Movie movie)
        {
            Init();
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                mySqlite.InsertSearchMovie(movie, "movie");
            }
        }

        public static void SaveMovieCodeByID(string id, string table, string code)
        {
            using (MySqlite mySqlite = new MySqlite(SqlitePath, true))
            {
                mySqlite.InsertByField(table, "code", code, id);
            }
        }

        public static void SaveMagnets(List<Magnet> magnets)
        {
            foreach (var item in magnets)
            {
                using (MySqlite mySqlite = new MySqlite("Magnets"))
                {
                    mySqlite.InsertMagnet(item);
                }

            }
        }


        public static void SaveActressLikeByName(string name, int like)
        {
            Init();
            using (SQLiteConnection cn = new SQLiteConnection("data source=" + SqlitePath))
            {
                cn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;

                    cmd.CommandText = $"insert into  actresslove(name,islove) values(@name,@islove) ON CONFLICT(name) DO UPDATE SET islove=@islove";
                    cmd.Parameters.Add("name", DbType.String).Value = name;
                    cmd.Parameters.Add("islove", DbType.Int32).Value = like;
                    cmd.ExecuteNonQuery();
                }
            }




        }

        public static void InsertActress(Actress actress)
        {
            if (actress == null) { return; }
            if (string.IsNullOrEmpty(actress.name)) { return; }
            Init();
            using (SQLiteConnection cn = new SQLiteConnection("data source=" + SqlitePath))
            {
                cn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;

                    string sqltext = $"insert into actress(id,name ,  birthday , age ,height, cup, chest  , waist , hipline ,birthplace  , hobby,sourceurl ,source,imageurl) values(@id,@name ,  @birthday , @age ,@height, @cup, @chest  , @waist , @hipline ,@birthplace  , @hobby,@sourceurl ,@source,@imageurl) on conflict(id) do update set name=@name ,  birthday=@birthday , age=@age  ,height=@height, cup=@cup, chest=@chest  , waist=@waist , hipline=@hipline ,birthplace=@birthplace  , hobby=@hobby,sourceurl=@sourceurl ,source=@source,imageurl=@imageurl";
                    cmd.CommandText = sqltext;
                    cmd.Parameters.Add("id", DbType.String).Value = actress.id;
                    cmd.Parameters.Add("name", DbType.String).Value = actress.name;
                    cmd.Parameters.Add("birthday", DbType.String).Value = actress.birthday;
                    cmd.Parameters.Add("age", DbType.Int32).Value = actress.age;
                    cmd.Parameters.Add("height", DbType.Int32).Value = actress.height;
                    cmd.Parameters.Add("cup", DbType.String).Value = actress.cup;
                    cmd.Parameters.Add("chest", DbType.Int32).Value = actress.chest;
                    cmd.Parameters.Add("waist", DbType.Int32).Value = actress.waist;
                    cmd.Parameters.Add("hipline", DbType.Int32).Value = actress.hipline;
                    cmd.Parameters.Add("birthplace", DbType.String).Value = actress.birthplace;
                    cmd.Parameters.Add("hobby", DbType.String).Value = actress.hobby;
                    cmd.Parameters.Add("sourceurl", DbType.String).Value = actress.sourceurl;
                    cmd.Parameters.Add("source", DbType.String).Value = actress.source;
                    cmd.Parameters.Add("imageurl", DbType.String).Value = actress.imageurl;
                    var success = cmd.ExecuteNonQuery();
                    Console.WriteLine(success);
                }
            }
        }




        #endregion

        #region "Other Command"


        /// <summary>
        /// 组合搜索，获得所有筛选值
        /// </summary>
        /// <returns></returns>
        public async static Task<List<List<string>>> GetAllFilter()
        {
            Init();
            if (!File.Exists(Properties.Settings.Default.DataBasePath) || !IsTableExist("movie")) return null;
            return await Task.Run(() =>
            {
                using (SQLiteConnection cn = new SQLiteConnection("data source=" + SqlitePath))
                {
                    cn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand())
                    {
                        cmd.Connection = cn;
                        //年份
                        List<string> Year = new List<string>();
                        cmd.CommandText = "SELECT releasedate FROM movie";
                        string year = "1900";
                        using (SQLiteDataReader sr = cmd.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                string date = sr[0].ToString();
                                if (!string.IsNullOrEmpty(date) && date.IndexOf(" ") < 0)
                                {
                                    try { year = Regex.Match(date, @"\d{4}").Value; }
                                    catch { }
                                    if (year == "0000") year = "1900";
                                    if (!Year.Contains(year)) Year.Add(year);
                                }
                            }
                        }
                        Year.Sort();

                        //类别
                        List<string> Genre = new List<string>();
                        cmd.CommandText = "SELECT genre FROM movie";
                        using (SQLiteDataReader sr = cmd.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                sr[0].ToString().Split(' ').ToList().ForEach(arg =>
                                {
                                    if (arg.Length > 0 & arg.IndexOf(' ') < 0)
                                        if (!Genre.Contains(arg)) Genre.Add(arg);
                                });
                            }
                        }
                        Genre.Sort();

                        //演员
                        List<string> Actor = new List<string>();
                        cmd.CommandText = "SELECT actor,vediotype FROM movie";
                        using (SQLiteDataReader sr = cmd.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                int.TryParse(sr[1].ToString(), out int vt);
                                sr[0].ToString().Split(actorSplitDict[vt]).ToList().ForEach(arg =>
                                {
                                    if (arg.Length > 0 & arg.IndexOf(' ') < 0)
                                        if (!Actor.Contains(arg)) Actor.Add(arg);
                                });
                            }
                        }
                        Actor.Sort();

                        //标签
                        List<string> Label = new List<string>();
                        cmd.CommandText = "SELECT label FROM movie";
                        using (SQLiteDataReader sr = cmd.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                sr[0].ToString().Split(' ').ToList().ForEach(arg =>
                                {
                                    if (arg.Length > 0 & arg.IndexOf(' ') < 0)
                                        if (!Label.Contains(arg)) Label.Add(arg);
                                });
                            }
                        }
                        Label.Sort();

                        //时长
                        //45 90 120
                        List<string> Runtime = new List<string>();
                        cmd.CommandText = "SELECT runtime FROM movie";
                        using (SQLiteDataReader sr = cmd.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                if (Runtime.Count >= 4) break;
                                int.TryParse(sr[0].ToString(), out int runtime);
                                if (runtime <= 45 && !Runtime.Contains("0-45"))
                                {
                                    Runtime.Add("0-45");
                                }

                                else if ((runtime >= 45 & runtime <= 90) && !Runtime.Contains("45-90"))
                                {
                                    Runtime.Add("45-90");
                                }

                                else if ((runtime >= 90 & runtime <= 120) && !Runtime.Contains("90-120"))
                                {
                                    Runtime.Add("90-120");
                                }
                                else if (runtime >= 120 && !Runtime.Contains("120-999"))
                                {
                                    Runtime.Add("120-999");
                                }
                            }
                        }

                        Runtime.Sort(new RatingComparer());

                        //文件大小
                        //1 2 3 
                        List<string> FileSize = new List<string>();
                        cmd.CommandText = "SELECT filesize FROM movie";
                        using (SQLiteDataReader sr = cmd.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                if (FileSize.Count >= 4) break;
                                double.TryParse(sr[0].ToString(), out double filesize);
                                filesize /= 1073741824D;// B to GB
                                if (filesize <= 1 && !FileSize.Contains("0-1"))
                                {
                                    FileSize.Add("0-1");
                                }
                                else if ((filesize >= 1 & filesize <= 2) && !FileSize.Contains("1-2"))
                                {
                                    FileSize.Add("1-2");
                                }
                                else if ((filesize >= 2 & filesize <= 3) && !FileSize.Contains("2-3"))
                                {
                                    FileSize.Add("2-3");
                                }
                                else if (filesize >= 3 && !FileSize.Contains("3-999"))
                                {
                                    FileSize.Add("3-999");
                                }
                            }
                        }

                        FileSize.Sort(new RatingComparer());


                        //评分
                        //20 40 60 80 
                        List<string> Rating = new List<string>();
                        cmd.CommandText = "SELECT rating FROM movie";
                        using (SQLiteDataReader sr = cmd.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                if (Rating.Count >= 4) break;
                                double.TryParse(sr[0].ToString(), out double rating);
                                if (rating <= 20 && !Rating.Contains("0-20"))
                                {
                                    Rating.Add("0-20");
                                }
                                else if ((rating >= 20 & rating <= 40) && !Rating.Contains("20-40"))
                                {
                                    Rating.Add("20-40");
                                }
                                else if ((rating >= 40 & rating <= 60) && !Rating.Contains("40-60"))
                                {
                                    Rating.Add("40-60");
                                }

                                else if ((rating >= 60 & rating <= 80) && !Rating.Contains("60-80"))
                                {
                                    Rating.Add("60-80");
                                }

                                else if (rating >= 80 && !Rating.Contains("80-100"))
                                {
                                    Rating.Add("80-100");
                                }
                            }
                        }

                        Rating.Sort(new RatingComparer());

                        List<List<string>> result = new List<List<string>>
                    {
                        Year,
                        Genre,
                        Actor,
                        Label,
                        Runtime,
                        FileSize,
                        Rating
                    };
                        return result;

                    }
                }
            });
        }

        /// <summary>
        /// 读取 Access 到 SQlite
        /// </summary>
        /// <param name="path"></param>
        public static void InsertFromAccess(string path = "")
        {
            Init();
            using (SQLiteConnection cn = new SQLiteConnection("data source=" + SqlitePath))
            {
                cn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    string dbpath = AppDomain.CurrentDomain.BaseDirectory + "\\mdb\\MainDatabase.mdb";
                    if (!string.IsNullOrEmpty(path)) { dbpath = path; }
                    if (!File.Exists(dbpath)) return;
                    OleDbConnection con = new OleDbConnection();
                    OleDbCommand OleDbcmd = new OleDbCommand();
                    con.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dbpath + ";Mode=Read";
                    con.Open();
                    OleDbcmd.Connection = con;
                    OleDbcmd.CommandText = "select * from Main";
                    using (OleDbDataReader sr = OleDbcmd.ExecuteReader())
                    {
                        while (sr.Read())
                        {

                            string t = string.IsNullOrEmpty(sr["daorushijian"].ToString()) ? "1900-01-01 01:01:01" : sr["daorushijian"].ToString();
                            DateTime scandate = DateTime.Now;
                            bool success = DateTime.TryParse(t, out scandate);
                            if (!success && t.Length == "20200830221052".Length)
                            {
                                DateTime.TryParse(t.Substring(0, 4) + "-" + t.Substring(4, 2) + "-" + t.Substring(6, 2) + " " + t.Substring(8, 2) + ":" + t.Substring(10, 2) + ":" + t.Substring(12, 2), out scandate);
                            }
                            try
                            {
                                Movie AccessMovie = new Movie()
                                {
                                    id = sr["fanhao"].ToString(),
                                    title = FileProcess.Unicode2String(sr["mingcheng"].ToString()),
                                    filesize = string.IsNullOrEmpty(sr["wenjiandaxiao"].ToString()) ? 0 : double.Parse(sr["wenjiandaxiao"].ToString()),
                                    filepath = sr["weizhi"].ToString(),
                                    vediotype = string.IsNullOrEmpty(sr["shipinleixing"].ToString()) ? 0 : int.Parse(sr["shipinleixing"].ToString()),
                                    scandate = scandate.ToString("yyyy-MM-dd HH:mm:ss"),//导入时间
                                    releasedate = string.IsNullOrEmpty(sr["faxingriqi"].ToString()) ? "1900-01-01" : sr["faxingriqi"].ToString(),
                                    visits = string.IsNullOrEmpty(sr["fangwencishu"].ToString()) ? 0 : int.Parse(sr["fangwencishu"].ToString()),
                                    director = FileProcess.Unicode2String(sr["daoyan"].ToString()),
                                    genre = FileProcess.Unicode2String(sr["leibie"].ToString()),
                                    tag = FileProcess.Unicode2String(sr["xilie"].ToString()),
                                    actor = FileProcess.Unicode2String(sr["yanyuan"].ToString()),
                                    actorid = "",
                                    sourceurl = "",
                                    source = "",
                                    studio = FileProcess.Unicode2String(sr["faxingshang"].ToString()),
                                    favorites = string.IsNullOrEmpty(sr["love"].ToString()) ? 0 : int.Parse(sr["love"].ToString()),
                                    label = FileProcess.Unicode2String(sr["biaoqian"].ToString()),
                                    runtime = string.IsNullOrEmpty(sr["changdu"].ToString()) ? 0 : int.Parse(sr["changdu"].ToString()),
                                    rating = 0,
                                    year = string.IsNullOrEmpty(sr["faxingriqi"].ToString()) ? 1900 : int.Parse(sr["faxingriqi"].ToString().Split('-')[0]),
                                    countrycode = 0,
                                    otherinfo = scandate.ToString("yyyy-MM-dd HH:mm:ss"),
                                    subsection = "",
                                    chinesetitle = "",
                                    plot = "",
                                    outline = "",
                                    country = ""
                                };
                                InsertFullMovie(AccessMovie); //导入到 Sqlite 中
                            }
                            catch (Exception e)
                            {
                                Logger.LogD(e);
                                continue;
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }



}
