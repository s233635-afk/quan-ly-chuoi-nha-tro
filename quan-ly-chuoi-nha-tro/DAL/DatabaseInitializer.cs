using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace QuanLyNhaTro.DAL
{
    internal static class DatabaseInitializer
    {
        private static readonly SemaphoreSlim InitLock = new SemaphoreSlim(1, 1);
        private static bool initialized;

        public static async Task EnsureInitializedAsync(string connectionString, int commandTimeoutSeconds, bool loadSampleData)
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
                            bool complete = await HasAllTablesAsync(masterBuilder.ConnectionString, databaseName, commandTimeoutSeconds).ConfigureAwait(false);
                            if (complete)
                            {
                                initialized = true;
                                return;
                            }

                            await DropDatabaseAsync(connection, databaseName, commandTimeoutSeconds).ConfigureAwait(false);
                        }
                    }

                    using (var cmd = new SqlCommand(
                        "CREATE DATABASE [" + databaseName + "]", connection))
                    {
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    connection.ChangeDatabase(databaseName);
                    await ExecuteSqlScriptAsync(connection, @"Database\setup_database_final.sql", commandTimeoutSeconds).ConfigureAwait(false);
                    if (loadSampleData)
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

        internal static bool ReadBoolAppSetting(string key, bool fallback)
        {
            try
            {
                var raw = System.Configuration.ConfigurationManager.AppSettings[key];
                if (bool.TryParse(raw, out bool value)) return value;
            }
            catch
            {
                // ignore
            }

            return fallback;
        }

        private static async Task<bool> HasAllTablesAsync(string masterConnectionString, string databaseName, int timeout)
        {
            var required = new[]
            {
                "Roles","Users","Branches","BranchSections","RoomTypes","RoomStatuses","Rooms",
                "Tenants","Dependents","TenantRoomHistory","Deposits","Contracts","UtilityTypes",
                "UtilityReadings","Invoices","Payments","MaintenanceTickets","Assets",
                "Notifications","SystemSettings","UserBankSettings","BranchBankSettings"
            };

            var builder = new SqlConnectionStringBuilder(masterConnectionString)
            {
                InitialCatalog = databaseName
            };

            using (var conn = new SqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                var inList = string.Join(",", required.Select(t => "N'" + t.Replace("'", "''") + "'"));
                using (var cmd = new SqlCommand(
                    $"SELECT name FROM sys.tables WHERE name IN ({inList})", conn))
                {
                    cmd.CommandTimeout = timeout;
                    var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            if (!reader.IsDBNull(0))
                                found.Add(reader.GetString(0));
                        }
                    }

                    foreach (var table in required)
                    {
                        if (!found.Contains(table))
                            return false;
                    }
                }
            }

            return true;
        }

        private static async Task DropDatabaseAsync(SqlConnection masterConnection, string databaseName, int timeout)
        {
            using (var cmd = new SqlCommand(
                "ALTER DATABASE [" + databaseName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [" + databaseName + "];",
                masterConnection))
            {
                cmd.CommandTimeout = timeout;
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
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
