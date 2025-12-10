using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace QuanLyNhaTro.DAL
{
    public partial class DatabaseHelper
    {
        private async Task<DataTable> GetTableAsync(string sql, Action<SqlCommand> parameterize = null)
        {
            DataTable dt = new DataTable();

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    parameterize?.Invoke(cmd);
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Thử nhiều câu lệnh truy vấn để tránh lỗi thiếu cột trên các DB khác nhau.
        /// Ưu tiên câu lệnh chi tiết, fallback về SELECT * nếu thất bại.
        /// </summary>
        private async Task<DataTable> GetTableSafeAsync(string tableName, Action<SqlCommand> parameterize = null, params string[] sqlAttempts)
        {
            // Nếu bảng không tồn tại trên DB hiện tại, trả về bảng rỗng để UI vẫn hoạt động
            if (!await TableExistsAsync(tableName))
            {
                return new DataTable();
            }

            Exception lastEx = null;
            foreach (var sql in sqlAttempts)
            {
                try
                {
                    return await GetTableAsync(sql, parameterize);
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                }
            }

            throw lastEx ?? new Exception("Không thể tải dữ liệu.");
        }

        public Task<DataTable> GetRoomsAsync()
        {
            return GetTableSafeAsync(
                "Rooms",
                null,
                @"SELECT RoomId, BranchId, RoomNumber, RoomType, Area, RentPrice, UtilitiesPrice, MaxPersons, Description, Status 
                  FROM Rooms ORDER BY RoomNumber",
                "SELECT * FROM Rooms"
            );
        }

        public Task<DataTable> GetUsersByRoleAsync(int roleId)
        {
            return GetTableSafeAsync(
                "Users",
                cmd => cmd.Parameters.AddWithValue("@roleId", roleId),
                @"SELECT UserId, UserName, FullName, Email, Phone, RoleId, IsActive, CreatedDate 
                  FROM Users WHERE RoleId = @roleId",
                "SELECT * FROM Users WHERE RoleId = @roleId",
                "SELECT * FROM Users"
            );
        }

        public Task<DataTable> GetTenantsAsync()
        {
            return GetTableSafeAsync(
                "Tenants",
                null,
                @"SELECT TenantId, BranchId, FullName, IDNumber, DateOfBirth, Gender, Phone, Email, Nationality, Address, Occupation 
                  FROM Tenants ORDER BY FullName",
                "SELECT * FROM Tenants"
            );
        }

        public Task<DataTable> GetContractsAsync()
        {
            return GetTableSafeAsync(
                "Contracts",
                null,
                @"SELECT ContractId, RoomId, TenantId, StartDate, EndDate, RentPrice, UtilitiesPrice, DepositAmount, Status 
                  FROM Contracts",
                "SELECT * FROM Contracts"
            );
        }

        public Task<DataTable> GetDepositsAsync()
        {
            return GetTableSafeAsync(
                "Deposits",
                null,
                @"SELECT DepositId, TenantId, Amount, DepositDate, Status 
                  FROM Deposits",
                "SELECT * FROM Deposits"
            );
        }

        public Task<DataTable> GetUtilitiesAsync()
        {
            return GetTableSafeAsync(
                "Utilities",
                null,
                @"SELECT UtilityId, RoomId, UtilityType, UsageAmount, UsageDate, UnitPrice, TotalPrice 
                  FROM Utilities",
                "SELECT * FROM Utilities"
            );
        }

        public Task<DataTable> GetInvoicesAsync()
        {
            return GetTableSafeAsync(
                "Invoices",
                null,
                @"SELECT InvoiceId, RoomId, TenantId, InvoiceDate, Month, Year, RentAmount, UtilitiesAmount, Status 
                  FROM Invoices",
                "SELECT * FROM Invoices"
            );
        }

        public Task<DataTable> GetMaintenanceAsync()
        {
            return GetTableSafeAsync(
                "MaintenanceRecords",
                null,
                @"SELECT MaintenanceId, RoomId, IssueType, Description, ReportDate, Status, Resolution 
                  FROM MaintenanceRecords",
                "SELECT * FROM MaintenanceRecords"
            );
        }

        public Task<DataTable> GetAssetsAsync()
        {
            return GetTableSafeAsync(
                "Assets",
                null,
                @"SELECT AssetId, RoomId, AssetType, AssetName, Quantity, PurchaseDate, Condition, Location 
                  FROM Assets",
                "SELECT * FROM Assets"
            );
        }

        public Task<DataTable> GetNotificationsAsync()
        {
            return GetTableSafeAsync(
                "Notifications",
                null,
                @"SELECT NotificationId, UserId, Title, Message, Status, CreatedDate 
                  FROM Notifications ORDER BY CreatedDate DESC",
                "SELECT * FROM Notifications"
            );
        }

        public Task<DataTable> GetSystemSettingsAsync()
        {
            return GetTableSafeAsync(
                "SystemSettings",
                null,
                @"SELECT SettingKey, SettingValue, Description FROM SystemSettings",
                "SELECT * FROM SystemSettings"
            );
        }

        /// <summary>
        /// Kiểm tra nhanh xem bảng có tồn tại không để tránh lỗi “Invalid object name”.
        /// </summary>
        private async Task<bool> TableExistsAsync(string tableName)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string sql = @"SELECT CASE WHEN OBJECT_ID(@tbl, 'U') IS NULL THEN 0 ELSE 1 END";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tbl", tableName);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
        }
    }
}
