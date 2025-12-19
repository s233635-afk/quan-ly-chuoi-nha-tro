using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace QuanLyNhaTro.DAL
{
    internal static class DatabaseInitializer
    {
        private static readonly SemaphoreSlim InitLock = new SemaphoreSlim(1, 1);
        private static bool initialized;

        public static async Task EnsureInitializedAsync(string connectionString, int commandTimeoutSeconds)
        {
            if (initialized) return;
            await InitLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (initialized) return;

                var builder = new SqlConnectionStringBuilder(connectionString);
                if (string.IsNullOrWhiteSpace(builder.InitialCatalog)) return;
                if (!IsLocalDatabaseSource(builder.DataSource)) return;

                string databaseName = builder.InitialCatalog;
                var masterBuilder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "master"
                };
                if (masterBuilder.ContainsKey("AttachDBFilename"))
                    masterBuilder.Remove("AttachDBFilename");

                using (var connection = new SqlConnection(masterBuilder.ConnectionString))
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM sys.databases WHERE name = @name", connection))
                    {
                        cmd.Parameters.AddWithValue("@name", databaseName);
                        var exists = (int)await cmd.ExecuteScalarAsync().ConfigureAwait(false) > 0;
                        if (exists)
                        {
                            initialized = true;
                            return;
                        }
                    }

                    await ExecuteSqlScriptAsync(connection, @"Database\setup_database_final.sql", commandTimeoutSeconds).ConfigureAwait(false);
                    connection.ChangeDatabase(databaseName);
                    await ExecuteSqlScriptAsync(connection, "sample_data.sql", commandTimeoutSeconds).ConfigureAwait(false);

                    initialized = true;
                }
            }
            finally
            {
                InitLock.Release();
            }
        }

        internal static bool IsLocalDatabaseSource(string dataSource)
        {
            if (string.IsNullOrWhiteSpace(dataSource)) return false;
            dataSource = dataSource.Trim();
            return dataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase)
                || dataSource.StartsWith("(local)", StringComparison.OrdinalIgnoreCase)
                || dataSource.StartsWith(".\\", StringComparison.OrdinalIgnoreCase)
                || dataSource.StartsWith("localhost", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task ExecuteSqlScriptAsync(SqlConnection connection, string relativePath, int timeout)
        {
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException($"Cannot initialize database because '{relativePath}' was not found.", scriptPath);

            string script = File.ReadAllText(scriptPath);
            var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;
                using (var cmd = new SqlCommand(batch, connection))
                {
                    cmd.CommandTimeout = timeout;
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
