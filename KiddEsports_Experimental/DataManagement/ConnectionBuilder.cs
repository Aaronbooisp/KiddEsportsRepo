using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace DataManagement
{
    public class ConnectionBuilder
    {
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["KiddEsportsConnection"].ConnectionString;
        }

        public static SqlConnection GetSqlConnection()
        {
            return new SqlConnection(GetConnectionString());
        }
    }
}