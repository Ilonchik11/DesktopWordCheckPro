using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCheckPro
{
    public class DatabaseConnection
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString;
        private static SqlConnection _connection;

        private DatabaseConnection()
        {
            _connection = new SqlConnection(ConnectionString);
        }

        public static SqlConnection GetConnection()
        {
            if (_connection == null)
            {
                lock (typeof(DatabaseConnection))
                {
                    if (_connection == null)
                    {
                        _connection = new SqlConnection(ConnectionString);
                    }
                }
            }

            return _connection;
        }

        public static void OpenConnection()
        {
            if (_connection != null && _connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public static void CloseConnection()
        {
            if (_connection != null && _connection.State != System.Data.ConnectionState.Closed)
            {
                _connection.Close();
            }
        }
    }
}
