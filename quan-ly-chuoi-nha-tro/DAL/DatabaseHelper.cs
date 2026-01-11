using System;
using System.Configuration;
using System.Data;
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
            "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=db_ac1f11_quanlynhatro;Integrated Security=True";

        private readonly string connectionString;
        private readonly int commandTimeoutSeconds;

        public DatabaseHelper()
        {
            connectionString = ResolveConnectionString();
            bool useLocalDb = ReadBoolAppSetting("UseLocalDb", false);
            if (useLocalDb)
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                if (DatabaseInitializer.IsLocalDatabaseSource(builder.DataSource))
                {
                    bool loadSampleData = ReadBoolAppSetting("LoadSampleData", false);
                    DatabaseInitializer.EnsureInitializedAsync(connectionString, DefaultDbCommandTimeoutSeconds, loadSampleData).GetAwaiter().GetResult();
                }
            }

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

        private static bool ReadBoolAppSetting(string key, bool fallback)
        {
            try
            {
                var raw = ConfigurationManager.AppSettings[key];
                if (bool.TryParse(raw, out bool value)) return value;
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
                bool useLocalDb = ReadBoolAppSetting("UseLocalDb", false);
                if (useLocalDb && !DatabaseInitializer.IsLocalDatabaseSource(builder.DataSource))
                {
                    builder = new SqlConnectionStringBuilder(FallbackConnectionString);
                }
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
            return await RegisterUserAsync(username, password, fullName, null, null);
        }

        public async Task<bool> RegisterUserAsync(string username, string password, string fullName, string email, string phone)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await OpenConnectionWithTimeoutAsync(conn);

                    const int defaultRoleId = 2; // Staff
                    string sql =
                        "INSERT INTO Users (Username, Password, Email, FullName, Phone, RoleId, BranchId, IsActive) " +
                        "VALUES (@user, @pass, @email, @name, @phone, @roleId, NULL, 1)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = commandTimeoutSeconds;
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", password);
                        cmd.Parameters.AddWithValue("@name", fullName);
                        cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);
                        cmd.Parameters.AddWithValue("@roleId", defaultRoleId);

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

                    string sql = "SELECT FullName FROM Users WHERE Username = @user AND Password = @pass AND IsActive = 1";

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

        public async Task<bool> ResetPasswordAsync(string username, string fullName, string email, string phone, string newPassword)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await OpenConnectionWithTimeoutAsync(conn);

                    string sql =
                        "UPDATE Users " +
                        "SET Password = @pass, UpdatedDate = GETDATE() " +
                        "WHERE Username = @user AND IsActive = 1 " +
                        "AND (@fullName IS NULL OR FullName = @fullName) " +
                        "AND (@email IS NULL OR Email = @email) " +
                        "AND (@phone IS NULL OR Phone = @phone)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = commandTimeoutSeconds;

                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", newPassword);
                        cmd.Parameters.AddWithValue("@fullName", string.IsNullOrWhiteSpace(fullName) ? (object)DBNull.Value : fullName);
                        cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);

                        int affected = await cmd.ExecuteNonQueryAsync();
                        return affected > 0;
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

        public async Task<(int UserId, int RoleId, int? BranchId)> GetUserAccessAsync(string username)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await OpenConnectionWithTimeoutAsync(conn);

                    const string sql = "SELECT UserId, RoleId, BranchId FROM Users WHERE Username = @user AND IsActive = 1";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = commandTimeoutSeconds;
                        cmd.Parameters.AddWithValue("@user", username);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                                return (0, 0, null);

                            int userId = reader["UserId"] != DBNull.Value ? Convert.ToInt32(reader["UserId"]) : 0;
                            int roleId = reader["RoleId"] != DBNull.Value ? Convert.ToInt32(reader["RoleId"]) : 0;
                            int? branchId = reader["BranchId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["BranchId"]) : null;
                            return (userId, roleId, branchId);
                        }
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
                    throw new Exception("Lỗi lấy thông tin truy cập: " + ex.Message);
                }
            }
        }

        public async Task<DataTable> GetContractByIdAsync(int contractId)
        {
            var table = new DataTable();
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await OpenConnectionWithTimeoutAsync(conn);
                    const string sql = "SELECT TOP 1 * FROM Contracts WHERE ContractId = @id";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = commandTimeoutSeconds;
                        cmd.Parameters.AddWithValue("@id", contractId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            table.Load(reader);
                        }
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
                    throw new Exception("Lỗi lấy hợp đồng: " + ex.Message);
                }
            }

            return table;
        }
    }
}
