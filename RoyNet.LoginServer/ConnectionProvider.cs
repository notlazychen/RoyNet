using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace RoyNet.LoginServer
{
    public class ConnectionProvider
    {
        private static readonly string DBConnectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
        public static IDbConnection Connection
        {
            get { return new MySqlConnection(DBConnectionString); }
        }
    }
}
