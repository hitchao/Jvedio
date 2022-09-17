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
using SuperUtils;
using SuperUtils.Sql;
using Jvedio.Entity;
using Jvedio.Core.Logs;

namespace Jvedio.Entity
{
    public class MySqlite : Sqlite
    {


        public MySqlite(string path) : base(path)
        {
            SqlitePath = path;
            cn = new SQLiteConnection("data source=" + SqlitePath);
            cn.Open();
            cmd = new SQLiteCommand();
            cmd.Connection = cn;
        }


        public void CloseDB()
        {
            this.Close();
        }

    }



}
