using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace CloudyWing.DatabaseClients {
    public sealed class CommandExecutor {
        public CommandExecutor() {
            Initialize();
        }

        public CommandExecutor(CommandType commandType) : this() {
            CommandType = commandType;
        }

        public CommandExecutor(string connectionName) : this() {
            ConnectionName = connectionName;
        }

        public CommandExecutor(string connectionName, DbProviderFactory providerFactory) : this() {
            ConnectionName = connectionName;
            DbProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        }

        public DbProviderFactory DbProviderFactory { get; set; }

        public string ConnectionName { get; set; }

        public string CommandText { get; set; }

        public int CommandTimeout { get; set; }

        public CommandType CommandType { get; set; }

        public ParameterCollection Parameters { get; } = new ParameterCollection();

        public IDataReader CreateDataReader(CommandBehavior behavior = CommandBehavior.SequentialAccess) {
            return CreateCommand(CreateConnection()).ExecuteReader(CommandBehavior.CloseConnection | behavior);
        }

        public DataTable CreateDataTable() {
            using (IDataReader dr = CreateDataReader()) {
                DataTable dt = new DataTable();
                dt.Load(dr);
                return dt;
            }
        }

        public object QueryScalar() {
            using (IDbConnection conn = CreateConnection())
            using (IDbCommand cmd = CreateCommand(conn)) {
                return cmd.ExecuteScalar();
            }
        }

        public int Execute() {
            using (IDbConnection conn = CreateConnection())
            using (IDbCommand cmd = CreateCommand(conn)) {
                return cmd.ExecuteNonQuery();
            }
        }

        public void Initialize() {
            // 自已依需求決定預設值
            DbProviderFactory = ConfigurationManager.DbProviderFactory;
            ConnectionName = "DefaultConnection";
            CommandText = null;
            CommandTimeout = 30;
            CommandType = CommandType.Text;
            Parameters.Clear();
        }

        private IDbConnection CreateConnection() {
            IDbConnection conn = DbProviderFactory.CreateConnection();
            conn.ConnectionString = ConfigurationManager.DefaultConnection;
            conn.Open();
            return conn;
        }

        private IDbCommand CreateCommand(IDbConnection connection) {
            IDbCommand cmd = connection.CreateCommand();
            string sql = CommandText;
            cmd.CommandTimeout = CommandTimeout;
            cmd.CommandType = CommandType;

            foreach (ParameterMetadata metadata in Parameters) {
                Regex regex = new Regex(GetParameterNamePattern(metadata.ParameterName), RegexOptions.IgnoreCase);
                Match match = regex.Match(sql);
                // 統一不會因為多宣告的parameter而出錯(SqlCommand允許，OracleCommand不允許)
                if (!match.Success) {
                    continue;
                }

                if (IsEnumerable(metadata.Value)) {
                    string prefix = GetParameterNamePrefix();
                    List<string> postfixedNames = new List<string>();
                    int count = 0;

                    foreach (object value in metadata.Value as IEnumerable) {
                        IDbDataParameter parameter = cmd.CreateParameter();
                        metadata.ApplyParameter(parameter);
                        parameter.ParameterName = $"{prefix}_{metadata.ParameterName}_{count++}";
                        parameter.Value = value;

                        cmd.Parameters.Add(parameter);
                        postfixedNames.Add(match.Value[0] + parameter.ParameterName);
                    }

                    if (count > 0) {
                        sql = regex.Replace(sql, $"({string.Join(", ", postfixedNames)})");
                    } else {
                        sql = regex.Replace(sql, $"(NULL)");
                    }
                } else {
                    IDbDataParameter parameter = cmd.CreateParameter();
                    metadata.ApplyParameter(parameter);
                    cmd.Parameters.Add(parameter);
                }
            }
            cmd.CommandText = sql;

            return cmd;
        }

        private bool IsEnumerable(object value) {
            return !(value is string) && value is IEnumerable;
        }

        private string GetParameterNamePattern(string name) {
            // HACK 應該是可以用了，但最好視實際情況寫更精準點
            return @"(?<!@|\?|:)[@?:](" + Regex.Escape(name) + @"(?=[\W])|" + Regex.Escape(name) + "$)";
        }

        private string GetParameterNamePrefix() {
            return "CloudyWing" + new Random().Next(0, 999).ToString("000");
        }
    }
}