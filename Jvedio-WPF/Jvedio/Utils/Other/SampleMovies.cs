using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;

namespace Jvedio.Utils
{


    /// <summary>
    /// 生成 jvedio 的数据库
    /// </summary>
    public  class SampleMovies
    {
        public string jp = "名称";
        public int number = 1000;
        private int defaultmax = 500;

        public SampleMovies(int number)
        {
            this.number = number;
        }

        public SampleMovies()
        {

        }

        public void Create()
        {
            int max = number;
            string savepath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database", $"sample_{max}.sqlite");
            MySqlite db = new MySqlite(savepath, true);
            db.CreateTable(DataBase.SQLITETABLE_MOVIE);
            db.CreateTable(DataBase.SQLITETABLE_ACTRESS);
            db.CreateTable(DataBase.SQLITETABLE_LIBRARY);
            db.CreateTable(DataBase.SQLITETABLE_JAVDB);
            db.CloseDB();

            List<string> ID = GetID(max);

            using (MySqlite mySqlite = new MySqlite(savepath, true))
            {
                for (int i = 0; i < max; i++)
                {
                    Movie movie = new Movie()
                    {
                        id = ID[i],
                        favorites = new Random(Guid.NewGuid().GetHashCode()).Next(0, 6),
                        visits = new Random(Guid.NewGuid().GetHashCode()).Next(0, 100),
                        title = GetSomeText(30, i),
                        runtime= new Random(Guid.NewGuid().GetHashCode()).Next(0, 300),
                        rating = (float)(new Random(Guid.NewGuid().GetHashCode()).Next(0, 100))/10,
                        filesize =Math.Abs( 5 * 1024 * new Random(Guid.NewGuid().GetHashCode()).Next(0, 1024 * 1024)),
                        subsection = i % 100 == 0 ? "path1;path2" : "",
                        scandate = DateTime.Now.AddDays(-new Random(Guid.NewGuid().GetHashCode()).Next(-500,500)).ToString("yyyy-MM-dd HH:mm:ss"),
                        otherinfo = DateTime.Now.AddDays(-new Random(Guid.NewGuid().GetHashCode()).Next(-500, 500)). ToString("yyyy-MM-dd HH:mm:ss"),
                        releasedate= DateTime.Now.AddDays(-new Random(Guid.NewGuid().GetHashCode()).Next(-500, 500)).ToString("yyyy-MM-dd"),
                        vediotype = new Random(Guid.NewGuid().GetHashCode()).Next(1, 4),
                        tag = "系列" + new Random(Guid.NewGuid().GetHashCode()).Next(defaultmax),
                        director = "导演" + new Random(Guid.NewGuid().GetHashCode()).Next(defaultmax),
                        studio = "发行商" + new Random(Guid.NewGuid().GetHashCode()).Next(defaultmax)
                    };
                    movie.genre = GetGenre(movie);
                    movie.actor = GetActor(max);
                    movie.label = GetLabel(max);
                    mySqlite.InsertFullMovie(movie, "movie");
                    Console.WriteLine(i);
                }
            }
        }

        public string GetSomeText(int maxlength,int seed)
        {
            string result = "";
            for (int i = 0; i < new Random(seed).Next(maxlength); i++)
            {
                result += jp[new Random(seed + i).Next(jp.Length)];
            }
            return result;
        }




        //生成 n 个不重复的识别码
        private List<string> GetID(int maxcount)
        {
            List<string> result = new List<string>();
            List<string> eng = GetEng();
            List<string> num = GetNumber();
            List<int> engidx = new List<int>();
            while (engidx.Count < maxcount)
            {
                int idx=new Random(Guid.NewGuid().GetHashCode()).Next(0, eng.Count);
                if (!engidx.Contains(idx)) engidx.Add(idx);
            }

            for (int i = 0; i < maxcount; i++)
            {
                result.Add(eng[engidx[i]] + "-"+ num[ new Random(Guid.NewGuid().GetHashCode()).Next(0, 1000)]);
            }
            return result;
        }
        private List<string> GetEng()
        {
            List<string> result  = new List<string>();
            string vcab = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            for (int i = 0; i < vcab.Length; i++)
            {
                for (int j = 0; j < vcab.Length; j++)
                {
                    for (int k = 0; k < vcab.Length; k++)
                    {
                        for (int m= 0; m < vcab.Length; m++)
                        {
                            result.Add(vcab[i].ToString() + vcab[j] + vcab[k]+vcab[m]);
                        }
                    }
                }
            }

            return result;
        }

        private List<string> GetNumber()
        {
            List<string> result = new List<string>();
            string num = "0123456789";
            for (int v = 0; v < 10; v++)
            {
                for (int b = 0; b < 10; b++)
                {
                    for (int n = 0; n < 10; n++)
                    {
                        result.Add( num[v].ToString() + num[b] + num[n]);
                    }
                }
            }
            return result;
        }




        private string GetGenre(Movie movie)
        {
            List<string> result = new List<string>();
            int max = new Random().Next(0, 20);
            for (int i = 0; i < max; i++)
            {
                if (movie.vediotype == 1)
                {
                    var l = GenreUncensored[new Random(i * max).Next(0, 6)].Split(',').ToList();
                    result.Add(l[new Random(i * max + 1).Next(0, l.Count - 1)]);
                }
                else if (movie.vediotype == 2)
                {
                    var l = GenreCensored[new Random(i * max).Next(0, 6)].Split(',').ToList();
                    result.Add(l[new Random(i * max + 1).Next(0, l.Count - 1)]);
                }
                else if (movie.vediotype == 3)
                {
                    var l = GenreEurope[new Random(i * max).Next(0, 6)].Split(',').ToList();
                    result.Add(l[new Random(i * max + 1).Next(0, l.Count - 1)]);
                }
            }
            return string.Join(" ", result);
        }

        private string GetActor(int maxcount)
        {
            List<string> result = new List<string>();
            int max = new Random().Next(0, 50);
            for (int i = 0; i < max; i++)
            {
                result.Add("演员" + new Random(i * max).Next(1, maxcount));
            }
            return string.Join("/", result);
        }
        private string GetLabel( int maxcount)
        {
            List<string> result = new List<string>();
            int max = new Random().Next(0, 10);
            for (int i = 0; i < max; i++)
            {
                result.Add("标签" + new Random(i * max).Next(1, maxcount));
            }
            if (new Random(max).Next(maxcount) % 10 == 0) result.Add("高清");
            if (new Random(max+1).Next(maxcount) % 20 == 0) result.Add("中文");
            if (new Random(max + 1).Next(maxcount) % 100 == 0) result.Add("流出");
            return string.Join(" ", result);
        }

    }
}
