using MySql.Data.MySqlClient;
using System;

namespace Jvedio.Core.DataBase
{
    public class DBConnection
    {
        private DBConnection()
        {
        }

        public string Server { get; set; }

        public int Port { get; set; }

        public string DatabaseName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public MySqlConnection Connection { get; set; }

        private static DBConnection _instance = null;

        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
                if (string.IsNullOrEmpty(DatabaseName))
                    return false;
                string connectString = string.Format("server={0};port={1}; database={2}; user={3}; password={4}", Server, Port, DatabaseName, UserName, Password);
                Connection = new MySqlConnection(connectString);
                Connection.Open();
            }

            return true;
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}
