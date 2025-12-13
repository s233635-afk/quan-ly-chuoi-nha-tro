using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace QuanLyNhaTro.DAL
{
    public partial class DatabaseHelper
    {
        private const string ConnectionStringName = "QuanLyNhaTro";
        private const int DefaultDbConnectTimeoutSeconds = 10;
        private const int DefaultDbCommandTimeoutSeconds = 10;

        private const string FallbackConnectionString =
            "Data Source=SQL9001.site4now.net;Initial Catalog=db_ac1f11_quanlynhatro;User Id=db_ac1f11_quanlynhatro_admin;Password=admin123";

        private readonly string connectionString;
        private readonly int commandTimeoutSeconds;

        public DatabaseHelper()
        {
            connectionString = ResolveConnectionString();
            commandTimeoutSeconds = ReadIntAppSetting("DbCommandTimeoutSeconds", DefaultDbCommandTimeoutSeconds);
        }

        private static int ReadIntAppSetting(string key, int fallback)
        {
            try
            {
                var raw = ConfigurationManager.AppSettings[key];
                if (int.TryParse(raw, out int value) && value > 0) return value;
            }
            catch
            {
                // ignore
            }

            return fallback;
        }

        private static bool HasExplicitTimeout(string cs)
        {
            if (string.IsNullOrWhiteSpace(cs)) return false;
            return cs.IndexOf("connect timeout", StringComparison.OrdinalIgnoreCase) >= 0
                || cs.IndexOf("connection timeout", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ResolveConnectionString()
        {
            string raw = null;
            try
            {
                raw = ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString;
            }
            catch
            {
                // ignore
            }

            if (string.IsNullOrWhiteSpace(raw))
                raw = FallbackConnectionString;

            try
            {
                var builder = new SqlConnectionStringBuilder(raw);
                if (!HasExplicitTimeout(raw))
                    builder.ConnectTimeout = DefaultDbConnectTimeoutSeconds;
                return builder.ConnectionString;
            }
            catch
            {
                if (!HasExplicitTimeout(raw))
                    raw = raw.TrimEnd(';') + ";Connect Timeout=" + DefaultDbConnectTimeoutSeconds;
                return raw;
            }
        }

        private static int GetConnectTimeoutSeconds(string cs)
        {
            try
            {
                return new SqlConnectionStringBuilder(cs).ConnectTimeout;
            }
            catch
            {
                return DefaultDbConnectTimeoutSeconds;
            }
        }

        private static Exception CreateTimeoutException(string operation, Exception inner = null)
        {
            return new TimeoutException(
                $"Không thể {operation} (hết thời gian chờ). Vui lòng kiểm tra Internet hoặc cấu hình database trong App.config.",
                inner);
        }

        private async Task OpenConnectionWithTimeoutAsync(SqlConnection conn)
        {
            int connectTimeout = GetConnectTimeoutSeconds(conn.ConnectionString);
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(connectTimeout + 1)))
            {
                await conn.OpenAsync(cts.Token);
            }
        }

        public async Task<bool> RegisterUserAsync(string username, string password, string fullName)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await OpenConnectionWithTimeoutAsync(conn);

                    string sql = "INSERT INTO Users (Username, Password, FullName) VALUES (@user, @pass, @name)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = commandTimeoutSeconds;
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", password);
                        cmd.Parameters.AddWithValue("@name", fullName);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
                catch (TaskCanceledException ex)
                {
                    throw CreateTimeoutException("kết nối database", ex);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627)
                        throw new Exception("Tài khoản này đã tồn tại rồi!");
                    if (ex.Number == -2)
                        throw CreateTimeoutException("thực thi truy vấn database", ex);

                    throw new Exception("Lỗi database: " + ex.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi kết nối: " + ex.Message);
                }
            }
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await OpenConnectionWithTimeoutAsync(conn);

                    string sql = "SELECT FullName FROM Users WHERE Username = @user AND Password = @pass";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = commandTimeoutSeconds;
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", password);

                        var result = await cmd.ExecuteScalarAsync();
                        return result?.ToString();
                    }
                }
                catch (TaskCanceledException ex)
                {
                    throw CreateTimeoutException("kết nối database", ex);
                }
                catch (SqlException ex) when (ex.Number == -2)
                {
                    throw CreateTimeoutException("thực thi truy vấn database", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi kết nối database: " + ex.Message);
                }
            }
        }

        public async Task<int> GetUserRoleAsync(string username)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await OpenConnectionWithTimeoutAsync(conn);

                    string sql = "SELECT RoleId FROM Users WHERE Username = @user AND IsActive = 1";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = commandTimeoutSeconds;
                        cmd.Parameters.AddWithValue("@user", username);

                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int roleId))
                            return roleId;

                        return 0;
                    }
                }
                catch (TaskCanceledException ex)
                {
                    throw CreateTimeoutException("kết nối database", ex);
                }
                catch (SqlException ex) when (ex.Number == -2)
                {
                    throw CreateTimeoutException("thực thi truy vấn database", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi lấy quyền người dùng: " + ex.Message);
                }
            }
        }
    }
}
