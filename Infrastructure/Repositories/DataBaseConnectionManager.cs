using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDB.App.Infrastructure.Repositories
{
    public class DataBaseConnectionManager
    {
        public static DbConnection GetConnection()
        {
            var settings = ConfigurationManager.ConnectionStrings["GDBConnection"];

            string connectionString = settings.ConnectionString;
            string providerName = settings.ProviderName;

            DbProviderFactory factory = DbProviderFactories.GetFactory(providerName);

            DbConnection connection = factory.CreateConnection();

            connection.ConnectionString = connectionString;

            return connection;
        }
    }
}
