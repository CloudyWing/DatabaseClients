using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace CloudyWing.DatabaseClients {
    public static class ConfigurationManager {
        private static readonly Dictionary<string, DbProviderFactory> dbProviderFactoryMaps =
            new Dictionary<string, DbProviderFactory>(StringComparer.OrdinalIgnoreCase) {
                ["Odbc"] = OdbcFactory.Instance,
                ["OracleClient"] = OracleClientFactory.Instance,
                ["SqlClient"] = SqlClientFactory.Instance
            };

        internal static string DefaultConnection { get; private set; }

        internal static DbProviderFactory DbProviderFactory { get; private set; }

        public static void LoadConfiguration(IConfigurationSource source) {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.Add(source);
            SetConfiguration(configBuilder.Build());
        }

        public static void LoadJsonConfiguration(string path) {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            configBuilder.AddJsonFile(path);
            SetConfiguration(configBuilder.Build());
        }

        public static void LoadJsonConfiguration(string path, bool optional) {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            configBuilder.AddJsonFile(path, optional);
            SetConfiguration(configBuilder.Build());
        }

        public static void LoadJsonConfiguration(string path, bool optional, bool reloadOnChange) {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            configBuilder.AddJsonFile(path, optional, reloadOnChange);
            SetConfiguration(configBuilder.Build());
        }

        public static void LoadAppConfiguration() {
            DefaultConnection = System.Configuration.ConfigurationManager
                .ConnectionStrings["DefaultConnection"].ConnectionString;

            string dbProviderKey = System.Configuration.ConfigurationManager
                .AppSettings["DbProviderFactory"];

            DbProviderFactory = dbProviderFactoryMaps[dbProviderKey];
        }

        private static void SetConfiguration(IConfigurationRoot configuration) {
            DefaultConnection = configuration.GetSection("ConnectionStrings")
                .GetSection("DefaultConnection").Value;

            string dbProviderKey = configuration.GetSection("AppSettings")
                .GetSection("DbProviderFactory").Value;

            DbProviderFactory = dbProviderFactoryMaps[dbProviderKey];
        }
    }
}