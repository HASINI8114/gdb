using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDB.App.Infrastructure.Repositories
{
    public class DataBaseProviderRegistration
    {
        public static void Register()
        {
            string providerName =
            ConfigurationManager
                .ConnectionStrings["GDBConnection"]
                .ProviderName;

            string factoryTypeName =
                ConfigurationManager.AppSettings["ProviderFactory"];

            Type factoryType =
                Type.GetType(factoryTypeName);

            var instanceField =
                factoryType.GetField(
                    "Instance",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Static);

            DbProviderFactory factory =
                (DbProviderFactory)instanceField.GetValue(null);

            DbProviderFactories.RegisterFactory(
                providerName,
                factory);
        }

    }
}
