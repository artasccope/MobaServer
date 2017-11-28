using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MobaServer.Dao
{
    public partial class ConnFactory
    {
        private static MySqlConnection conn;
        private static string connString= "";

        public static MySqlConnection GetConnection() {
            if (conn == null)
                conn = new MySqlConnection(connString);

            return conn;
        }
    }
}
