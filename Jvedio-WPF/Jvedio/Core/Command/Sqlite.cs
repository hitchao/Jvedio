using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Command
{
    public static class Sqlite
    {
        public static RelayCommand<string> CreateVideoDataBase { get; set; }

        static Sqlite()
        {
            CreateVideoDataBase = new RelayCommand<string>(name=>CreateVideoData(name));
        }

        private static void CreateVideoData(string name)
        {
            if (!name.EndsWith(".sqlite")) name += ".sqlite";
            string path=Path.Combine(GlobalVariable.VideoDataPath, name );
            MySqlite db = new MySqlite(path);
            db.CreateTable(DataBase.SQLITETABLE_MOVIE);
            db.CreateTable(DataBase.SQLITETABLE_ACTRESS);
            db.CreateTable(DataBase.SQLITETABLE_LIBRARY);
            db.CreateTable(DataBase.SQLITETABLE_JAVDB);
            db.CloseDB();
        }



    }
}
