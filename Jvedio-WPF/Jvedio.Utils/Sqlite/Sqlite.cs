using Jvedio;
using Jvedio.Utils.Interface;
using System;
using System.Collections.Generic;
using System.Data.SQLite;



namespace Jvedio.Utils.Sqlite
{
    public class Sqlite : IDisposable
    {

        public string SqlitePath { get; set; }
        protected SQLiteCommand cmd;
        protected SQLiteConnection cn;

        public Sqlite(string path)
        {
            SqlitePath = path;
        }


        public void Dispose()
        {
            this.Close();
        }

        public void Close()
        {
            cmd?.Dispose();
            cn?.Close();
        }


        public void Vacuum()
        {
            cmd.CommandText = "vacuum;";
            cmd.ExecuteNonQuery();
        }

        public bool IsTableExist(string table)
        {
            bool result = true;
            try
            {
                string sqltext = $"select * from {table}";
                cmd.CommandText = sqltext;
                cmd.ExecuteNonQuery();
            }
            catch { result = false; }
            return result;
        }

        public List<string> GetAllTable()
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE TYPE='table' ";
            List<string> tables = new List<string>();
            try
            {
                using(SQLiteDataReader sr = cmd.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        tables.Add(sr.GetString(0));
                    }
                }
            }
            catch (System.Data.SQLite.SQLiteException ex) {
                Console.WriteLine(ex.Message);
            }
            return tables;
        }


        public bool DeleteTable(string tablename)
        {
            if (IsTableExist(tablename))
            {
                try
                {
                    cmd.CommandText = $"DROP TABLE IF EXISTS {tablename}";
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); return false; }

            }
            else
                return false;

        }

        public bool RenameTable(string oldName, string newName)
        {
            if (IsTableExist(oldName))
            {
                try
                {
                    cmd.CommandText = $"ALTER TABLE {oldName} RENAME TO {newName}";
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); return false; }

            }

            return false;
        }




        public bool ExecuteSql(string sqltext)
        {
            try
            {
                cmd.CommandText = sqltext;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }


        public double SelectCountByTable(string table, string field="id",string sql="")
        {
            double result = 0;
            cmd.CommandText = $"SELECT count({field}) FROM {table} " + sql;
            using (SQLiteDataReader sr = cmd.ExecuteReader())
            {
                while (sr.Read())
                {
                    double.TryParse(sr[0].ToString(), out result);
                }
                return result;
            }

        }

        public string SelectByField(string info, string table,  string value, string filed="id")
        {
            string result = "";
            string sqltext = $"select {info} from {table} where {filed} ='{value}'";
            cmd.CommandText = sqltext;
            using (SQLiteDataReader sr = cmd.ExecuteReader())
            {
                while (sr.Read())
                {
                    result = sr[0].ToString();
                }
            }
            return result;

        }

        public bool DeleteByField(string table, string filed, string value)
        {
            try
            {
                cmd.CommandText = $"delete from {table} where {filed} = '{value}'"; ;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }
    }



}
