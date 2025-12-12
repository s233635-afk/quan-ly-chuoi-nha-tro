using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace QuanLyNhaTro.DAL
{
    public partial class DatabaseHelper
    {
        #region Dashboard Summary

        public async Task<DataTable> GetDashboardSummaryAsync()
        {
            var dt = new DataTable();
            dt.Columns.Add("TotalBranches", typeof(int));
            dt.Columns.Add("TotalRooms", typeof(int));
            dt.Columns.Add("TotalTenants", typeof(int));
            dt.Columns.Add("TotalContracts", typeof(int));
            dt.Columns.Add("TotalInvoices", typeof(int));
            dt.Columns.Add("OutstandingInvoiceCount", typeof(int));
            dt.Columns.Add("OutstandingAmount", typeof(decimal));
            dt.Columns.Add("TotalDeposits", typeof(int));
            dt.Columns.Add("DepositAmount", typeof(decimal));
            dt.Columns.Add("PaymentsThisMonth", typeof(decimal));
            dt.Columns.Add("OpenMaintenance", typeof(int));
            dt.Rows.Add(0, 0, 0, 0, 0, 0, 0m, 0, 0m, 0m, 0);

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                // COUNT(*) ít phụ thuộc schema; các chỉ số chi tiết sẽ try nhiều câu lệnh và fallback về 0.
                dt.Rows[0]["TotalBranches"] = await GetScalarIntBestEffortAsync(conn,
                    "SELECT COUNT(*) FROM Branches",
                    "SELECT COUNT(*) FROM Branch");

                dt.Rows[0]["TotalRooms"] = await GetScalarIntBestEffortAsync(conn,
                    "SELECT COUNT(*) FROM Rooms",
                    "SELECT COUNT(*) FROM Room");

                dt.Rows[0]["TotalTenants"] = await GetScalarIntBestEffortAsync(conn,
                    "SELECT COUNT(*) FROM Tenants",
                    "SELECT COUNT(*) FROM Tenant");

                dt.Rows[0]["TotalContracts"] = await GetScalarIntBestEffortAsync(conn,
                    "SELECT COUNT(*) FROM Contracts",
                    "SELECT COUNT(*) FROM Contract");

                dt.Rows[0]["TotalInvoices"] = await GetScalarIntBestEffortAsync(conn,
                    "SELECT COUNT(*) FROM Invoices",
                    "SELECT COUNT(*) FROM Invoice");

                // Hóa đơn còn nợ (ưu tiên RemainingAmount, fallback theo status)
                dt.Rows[0]["OutstandingInvoiceCount"] = await GetScalarIntBestEffortAsync(conn,
                    "SELECT COUNT(*) FROM Invoices WHERE RemainingAmount > 0",
                    "SELECT COUNT(*) FROM Invoices WHERE (TotalAmount - ISNULL(PaidAmount,0)) > 0",
                    "SELECT COUNT(*) FROM Invoices WHERE Status IN (0, 'Unpaid', 'UNPAID', N'Chưa thanh toán', N'Chua thanh toan')");

                dt.Rows[0]["OutstandingAmount"] = await GetScalarDecimalBestEffortAsync(conn,
                    "SELECT CAST(ISNULL(SUM(RemainingAmount),0) AS DECIMAL(18,2)) FROM Invoices WHERE RemainingAmount > 0",
                    "SELECT CAST(ISNULL(SUM(TotalAmount - ISNULL(PaidAmount,0)),0) AS DECIMAL(18,2)) FROM Invoices WHERE (TotalAmount - ISNULL(PaidAmount,0)) > 0");

                // Đặt cọc (DepositAmount hoặc Amount)
                dt.Rows[0]["TotalDeposits"] = await GetScalarIntBestEffortAsync(conn,
                    "SELECT COUNT(*) FROM Deposits",
                    "SELECT COUNT(*) FROM Deposit");

                dt.Rows[0]["DepositAmount"] = await GetScalarDecimalBestEffortAsync(conn,
                    "SELECT CAST(ISNULL(SUM(DepositAmount),0) AS DECIMAL(18,2)) FROM Deposits",
                    "SELECT CAST(ISNULL(SUM(Amount),0) AS DECIMAL(18,2)) FROM Deposits");

                // Thanh toán tháng này
                dt.Rows[0]["PaymentsThisMonth"] = await GetScalarDecimalBestEffortAsync(conn,
                    @"SELECT CAST(ISNULL(SUM(PaymentAmount),0) AS DECIMAL(18,2)) 
                      FROM Payments 
                      WHERE PaymentDate >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
                        AND PaymentDate < DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))",
                    @"SELECT CAST(ISNULL(SUM(Amount),0) AS DECIMAL(18,2)) 
                      FROM Payments 
                      WHERE PaymentDate >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
                        AND PaymentDate < DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))");

                // Bảo trì mở (MaintenanceTickets hoặc MaintenanceRecords)
                dt.Rows[0]["OpenMaintenance"] = await GetScalarIntBestEffortAsync(conn,
                    "SELECT COUNT(*) FROM MaintenanceTickets WHERE Status NOT IN ('Done','DONE',N'Hoàn tất',N'Hoan tat',2)",
                    "SELECT COUNT(*) FROM MaintenanceTickets",
                    "SELECT COUNT(*) FROM MaintenanceRecords WHERE Status NOT IN (2, 'Done','DONE',N'Hoàn tất',N'Hoan tat')",
                    "SELECT COUNT(*) FROM MaintenanceRecords");
            }

            return dt;
        }

        #endregion

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

        private async Task<int> GetScalarIntBestEffortAsync(SqlConnection conn, params string[] sqlAttempts)
        {
            foreach (var sql in sqlAttempts)
            {
                try
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        if (result == null || result == DBNull.Value) return 0;
                        return Convert.ToInt32(result);
                    }
                }
                catch
                {
                    // try next
                }
            }

            return 0;
        }

        private async Task<decimal> GetScalarDecimalBestEffortAsync(SqlConnection conn, params string[] sqlAttempts)
        {
            foreach (var sql in sqlAttempts)
            {
                try
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        if (result == null || result == DBNull.Value) return 0m;
                        return Convert.ToDecimal(result);
                    }
                }
                catch
                {
                    // try next
                }
            }

            return 0m;
        }

        #region Rooms CRUD

        private async Task<bool> ColumnExistsAsync(SqlConnection conn, string tableName, string columnName)
        {
            const string sql = @"SELECT CASE WHEN COL_LENGTH(@tbl, @col) IS NULL THEN 0 ELSE 1 END";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@tbl", tableName);
                cmd.Parameters.AddWithValue("@col", columnName);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && Convert.ToInt32(result) == 1;
            }
        }

        public async Task<int> AddRoomAsync(
            string roomNumber,
            int branchId,
            int? sectionId,
            int? roomTypeId,
            decimal? roomPrice,
            int? currentStatusId,
            int? floor,
            decimal? area,
            bool? isActive)
        {
            if (!await TableExistsAsync("Rooms"))
                throw new Exception("Bảng Rooms không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "Rooms";

                bool hasRoomNumber = await ColumnExistsAsync(conn, tbl, "RoomNumber");
                bool hasBranchId = await ColumnExistsAsync(conn, tbl, "BranchId");
                bool hasSectionId = await ColumnExistsAsync(conn, tbl, "SectionId");
                bool hasRoomTypeId = await ColumnExistsAsync(conn, tbl, "RoomTypeId");
                bool hasRoomType = await ColumnExistsAsync(conn, tbl, "RoomType"); // legacy/sample
                bool hasRoomPrice = await ColumnExistsAsync(conn, tbl, "RoomPrice");
                bool hasRentPrice = await ColumnExistsAsync(conn, tbl, "RentPrice"); // legacy/sample
                bool hasCurrentStatusId = await ColumnExistsAsync(conn, tbl, "CurrentStatusId");
                bool hasStatus = await ColumnExistsAsync(conn, tbl, "Status"); // legacy/sample
                bool hasFloor = await ColumnExistsAsync(conn, tbl, "Floor");
                bool hasArea = await ColumnExistsAsync(conn, tbl, "Area");
                bool hasIsActive = await ColumnExistsAsync(conn, tbl, "IsActive");
                bool hasCreatedDate = await ColumnExistsAsync(conn, tbl, "CreatedDate");
                bool hasUpdatedDate = await ColumnExistsAsync(conn, tbl, "UpdatedDate");

                // Build insert dynamically based on columns available
                var cols = new System.Collections.Generic.List<string>();
                var vals = new System.Collections.Generic.List<string>();

                if (hasRoomNumber)
                {
                    cols.Add("RoomNumber");
                    vals.Add("@RoomNumber");
                }

                if (hasBranchId)
                {
                    cols.Add("BranchId");
                    vals.Add("@BranchId");
                }

                if (hasSectionId)
                {
                    cols.Add("SectionId");
                    vals.Add("@SectionId");
                }

                if (hasRoomTypeId)
                {
                    cols.Add("RoomTypeId");
                    vals.Add("@RoomTypeId");
                }
                else if (hasRoomType)
                {
                    cols.Add("RoomType");
                    vals.Add("@RoomTypeId"); // reuse numeric value
                }

                if (hasRoomPrice)
                {
                    cols.Add("RoomPrice");
                    vals.Add("@RoomPrice");
                }
                else if (hasRentPrice)
                {
                    cols.Add("RentPrice");
                    vals.Add("@RoomPrice");
                }

                if (hasCurrentStatusId)
                {
                    cols.Add("CurrentStatusId");
                    vals.Add("@CurrentStatusId");
                }
                else if (hasStatus)
                {
                    cols.Add("Status");
                    vals.Add("@CurrentStatusId");
                }

                if (hasFloor)
                {
                    cols.Add("Floor");
                    vals.Add("@Floor");
                }

                if (hasArea)
                {
                    cols.Add("Area");
                    vals.Add("@Area");
                }

                if (hasIsActive)
                {
                    cols.Add("IsActive");
                    vals.Add("@IsActive");
                }

                if (hasCreatedDate)
                {
                    cols.Add("CreatedDate");
                    vals.Add("GETDATE()");
                }

                if (hasUpdatedDate)
                {
                    cols.Add("UpdatedDate");
                    vals.Add("GETDATE()");
                }

                if (cols.Count == 0)
                {
                    throw new Exception("Không xác định được cột Rooms để thêm mới.");
                }

                string sql = $@"
                    INSERT INTO Rooms ({string.Join(", ", cols)})
                    VALUES ({string.Join(", ", vals)});
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (hasRoomNumber)
                        cmd.Parameters.AddWithValue("@RoomNumber", roomNumber);
                    if (hasBranchId)
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                    if (hasSectionId)
                        cmd.Parameters.AddWithValue("@SectionId", sectionId.HasValue ? (object)sectionId.Value : DBNull.Value);

                    // Use @RoomTypeId as common param name even if mapping to RoomType
                    if (hasRoomTypeId || hasRoomType)
                        cmd.Parameters.AddWithValue("@RoomTypeId", roomTypeId.HasValue ? (object)roomTypeId.Value : DBNull.Value);

                    if (hasRoomPrice || hasRentPrice)
                        cmd.Parameters.AddWithValue("@RoomPrice", roomPrice.HasValue ? (object)roomPrice.Value : DBNull.Value);

                    if (hasCurrentStatusId || hasStatus)
                        cmd.Parameters.AddWithValue("@CurrentStatusId", currentStatusId.HasValue ? (object)currentStatusId.Value : DBNull.Value);

                    if (hasFloor)
                        cmd.Parameters.AddWithValue("@Floor", floor.HasValue ? (object)floor.Value : DBNull.Value);
                    if (hasArea)
                        cmd.Parameters.AddWithValue("@Area", area.HasValue ? (object)area.Value : DBNull.Value);
                    if (hasIsActive)
                        cmd.Parameters.AddWithValue("@IsActive", isActive.HasValue ? (object)isActive.Value : (object)true);

                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateRoomAsync(
            int roomId,
            string roomNumber,
            int branchId,
            int? sectionId,
            int? roomTypeId,
            decimal? roomPrice,
            int? currentStatusId,
            int? floor,
            decimal? area,
            bool? isActive)
        {
            if (!await TableExistsAsync("Rooms"))
                throw new Exception("Bảng Rooms không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "Rooms";

                bool hasRoomId = await ColumnExistsAsync(conn, tbl, "RoomId");
                bool hasRoomNumber = await ColumnExistsAsync(conn, tbl, "RoomNumber");
                bool hasBranchId = await ColumnExistsAsync(conn, tbl, "BranchId");
                bool hasSectionId = await ColumnExistsAsync(conn, tbl, "SectionId");
                bool hasRoomTypeId = await ColumnExistsAsync(conn, tbl, "RoomTypeId");
                bool hasRoomType = await ColumnExistsAsync(conn, tbl, "RoomType");
                bool hasRoomPrice = await ColumnExistsAsync(conn, tbl, "RoomPrice");
                bool hasRentPrice = await ColumnExistsAsync(conn, tbl, "RentPrice");
                bool hasCurrentStatusId = await ColumnExistsAsync(conn, tbl, "CurrentStatusId");
                bool hasStatus = await ColumnExistsAsync(conn, tbl, "Status");
                bool hasFloor = await ColumnExistsAsync(conn, tbl, "Floor");
                bool hasArea = await ColumnExistsAsync(conn, tbl, "Area");
                bool hasIsActive = await ColumnExistsAsync(conn, tbl, "IsActive");
                bool hasUpdatedDate = await ColumnExistsAsync(conn, tbl, "UpdatedDate");

                if (!hasRoomId)
                    throw new Exception("Không tìm thấy cột RoomId để cập nhật.");

                var sets = new System.Collections.Generic.List<string>();

                if (hasRoomNumber) sets.Add("RoomNumber = @RoomNumber");
                if (hasBranchId) sets.Add("BranchId = @BranchId");
                if (hasSectionId) sets.Add("SectionId = @SectionId");

                if (hasRoomTypeId) sets.Add("RoomTypeId = @RoomTypeId");
                else if (hasRoomType) sets.Add("RoomType = @RoomTypeId");

                if (hasRoomPrice) sets.Add("RoomPrice = @RoomPrice");
                else if (hasRentPrice) sets.Add("RentPrice = @RoomPrice");

                if (hasCurrentStatusId) sets.Add("CurrentStatusId = @CurrentStatusId");
                else if (hasStatus) sets.Add("Status = @CurrentStatusId");

                if (hasFloor) sets.Add("Floor = @Floor");
                if (hasArea) sets.Add("Area = @Area");
                if (hasIsActive) sets.Add("IsActive = @IsActive");
                if (hasUpdatedDate) sets.Add("UpdatedDate = GETDATE()");

                if (sets.Count == 0)
                    throw new Exception("Không có cột nào để cập nhật.");

                string sql = $@"UPDATE Rooms SET {string.Join(", ", sets)} WHERE RoomId = @RoomId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    if (hasRoomNumber) cmd.Parameters.AddWithValue("@RoomNumber", roomNumber);
                    if (hasBranchId) cmd.Parameters.AddWithValue("@BranchId", branchId);
                    if (hasSectionId) cmd.Parameters.AddWithValue("@SectionId", sectionId.HasValue ? (object)sectionId.Value : DBNull.Value);
                    if (hasRoomTypeId || hasRoomType) cmd.Parameters.AddWithValue("@RoomTypeId", roomTypeId.HasValue ? (object)roomTypeId.Value : DBNull.Value);
                    if (hasRoomPrice || hasRentPrice) cmd.Parameters.AddWithValue("@RoomPrice", roomPrice.HasValue ? (object)roomPrice.Value : DBNull.Value);
                    if (hasCurrentStatusId || hasStatus) cmd.Parameters.AddWithValue("@CurrentStatusId", currentStatusId.HasValue ? (object)currentStatusId.Value : DBNull.Value);
                    if (hasFloor) cmd.Parameters.AddWithValue("@Floor", floor.HasValue ? (object)floor.Value : DBNull.Value);
                    if (hasArea) cmd.Parameters.AddWithValue("@Area", area.HasValue ? (object)area.Value : DBNull.Value);
                    if (hasIsActive) cmd.Parameters.AddWithValue("@IsActive", isActive.HasValue ? (object)isActive.Value : (object)true);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteRoomAsync(int roomId)
        {
            if (!await TableExistsAsync("Rooms"))
                throw new Exception("Bảng Rooms không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                bool hasIsActive = await ColumnExistsAsync(conn, "Rooms", "IsActive");
                bool hasUpdatedDate = await ColumnExistsAsync(conn, "Rooms", "UpdatedDate");

                string sql;
                if (hasIsActive)
                {
                    sql = hasUpdatedDate
                        ? "UPDATE Rooms SET IsActive = 0, UpdatedDate = GETDATE() WHERE RoomId = @RoomId"
                        : "UPDATE Rooms SET IsActive = 0 WHERE RoomId = @RoomId";
                }
                else
                {
                    sql = "DELETE FROM Rooms WHERE RoomId = @RoomId";
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        public Task<DataTable> GetRoomsAsync()
        {
            return GetTableSafeAsync(
                "Rooms",
                null,
                @"SELECT r.RoomId,
                         r.RoomNumber,
                         r.BranchId,
                         r.SectionId,
                         r.RoomTypeId,
                         r.RoomPrice,
                         r.CurrentStatusId,
                         r.Floor,
                         r.Area,
                         r.IsActive,
                         r.CreatedDate,
                         r.UpdatedDate
                  FROM Rooms r
                  ORDER BY r.RoomNumber",
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

        #region CRUD Staff (RoleId = 2)

        public async Task<int> AddStaffUserAsync(string username, string password, string fullName, string email, string phone, bool isActive)
        {
            const string sql = @"
                INSERT INTO Users (Username, Password, FullName, Email, Phone, RoleId, IsActive, CreatedDate)
                VALUES (@Username, @Password, @FullName, @Email, @Phone, 2, @IsActive, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateStaffUserAsync(int userId, string fullName, string email, string phone, bool isActive, string newPassword = null)
        {
            string sql = @"
                UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    IsActive = @IsActive
                WHERE UserId = @UserId AND RoleId = 2";

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                sql = @"
                UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    IsActive = @IsActive,
                    Password = @Password
                WHERE UserId = @UserId AND RoleId = 2";
            }

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    if (!string.IsNullOrWhiteSpace(newPassword))
                    {
                        cmd.Parameters.AddWithValue("@Password", newPassword);
                    }

                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteStaffUserAsync(int userId)
        {
            const string sql = @"DELETE FROM Users WHERE UserId = @UserId AND RoleId = 2";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        public Task<DataTable> GetTenantsAsync()
        {
            return GetTableSafeAsync(
                "Tenants",
                null,
                @"SELECT TenantId,
                         FullName,
                         IdentityCard,
                         PhoneNumber,
                         Email,
                         BirthDate,
                         Address,
                         TemporaryRegistration,
                         TemporaryRegistrationDate,
                         TemporaryRegistrationExpiry,
                         IsActive,
                         CreatedDate,
                         UpdatedDate
                  FROM Tenants
                  ORDER BY FullName",
                "SELECT * FROM Tenants"
            );
        }

        public Task<DataTable> GetDependentsAsync()
        {
            return GetTableSafeAsync(
                "Dependents",
                null,
                @"SELECT DependentId,
                         TenantId,
                         FullName,
                         Relationship,
                         PhoneNumber,
                         CreatedDate
                  FROM Dependents",
                "SELECT * FROM Dependents"
            );
        }

        public Task<DataTable> GetTenantHistoryAsync()
        {
            return GetTableSafeAsync(
                "TenantRoomHistory",
                null,
                @"SELECT HistoryId,
                         TenantId,
                         RoomId,
                         CheckInDate,
                         CheckOutDate,
                         Status,
                         Notes,
                         CreatedDate
                  FROM TenantRoomHistory",
                "SELECT * FROM TenantRoomHistory"
            );
        }

        public Task<DataTable> GetContractsAsync()
        {
            return GetTableSafeAsync(
                "Contracts",
                null,
                @"SELECT ContractId,
                         ContractNumber,
                         TenantId,
                         RoomId,
                         SignDate,
                         StartDate,
                         EndDate,
                         RentalPrice,
                         DepositRequired,
                         Terms,
                         ContractPdfPath,
                         Status,
                         CreatedDate,
                         UpdatedDate
                  FROM Contracts",
                "SELECT * FROM Contracts"
            );
        }

        public Task<DataTable> GetDepositsAsync()
        {
            return GetTableSafeAsync(
                "Deposits",
                null,
                @"SELECT DepositId,
                         TenantId,
                         RoomId,
                         DepositAmount,
                         DepositDate,
                         DepositType,
                         Status,
                         ReturnedAmount,
                         ReturnedDate,
                         Notes,
                         CreatedDate
                  FROM Deposits",
                "SELECT * FROM Deposits"
            );
        }

        public Task<DataTable> GetUtilitiesAsync()
        {
            return GetTableSafeAsync(
                "UtilityReadings",
                null,
                @"SELECT ur.ReadingId,
                         ur.RoomId,
                         ut.UtilityName,
                         ut.UtilityCode,
                         ur.ReadingDate,
                         ur.PreviousReading,
                         ur.CurrentReading,
                         ur.UsageAmount,
                         ur.UnitPrice,
                         ur.TotalCost,
                         ur.CreatedDate
                  FROM UtilityReadings ur
                  LEFT JOIN UtilityTypes ut ON ut.UtilityTypeId = ur.UtilityTypeId
                  ORDER BY ur.ReadingDate DESC",
                "SELECT * FROM UtilityReadings"
            );
        }

        public Task<DataTable> GetUtilityTypesAsync()
        {
            return GetTableSafeAsync(
                "UtilityTypes",
                null,
                @"SELECT UtilityTypeId,
                         UtilityName,
                         UtilityCode,
                         Unit,
                         IsRecurring,
                         DefaultPrice,
                         Description,
                         IsActive,
                         CreatedDate
                  FROM UtilityTypes",
                "SELECT * FROM UtilityTypes"
            );
        }

        public Task<DataTable> GetInvoicesAsync()
        {
            return GetTableSafeAsync(
                "Invoices",
                null,
                @"SELECT InvoiceId,
                         InvoiceNumber,
                         TenantId,
                         RoomId,
                         InvoiceDate,
                         FromDate,
                         ToDate,
                         RentalCost,
                         UtilityCost,
                         OtherCost,
                         TotalAmount,
                         PaidAmount,
                         RemainingAmount,
                         Status,
                         DueDate,
                         CreatedDate,
                         UpdatedDate
                  FROM Invoices",
                "SELECT * FROM Invoices"
            );
        }

        public Task<DataTable> GetPaymentsAsync()
        {
            return GetTableSafeAsync(
                "Payments",
                null,
                @"SELECT PaymentId,
                         InvoiceId,
                         PaymentDate,
                         PaymentAmount,
                         PaymentMethod,
                         TransactionReference,
                         Notes,
                         CreatedDate
                  FROM Payments",
                "SELECT * FROM Payments"
            );
        }

        public Task<DataTable> GetMaintenanceAsync()
        {
            return GetTableSafeAsync(
                "MaintenanceTickets",
                null,
                @"SELECT TicketId,
                         TicketNumber,
                         RoomId,
                         RequestorType,
                         RequestorId,
                         IssueDescription,
                         Priority,
                         AssignedToUserId,
                         Status,
                         CreatedDate,
                         CompletedDate,
                         Notes
                  FROM MaintenanceTickets",
                "SELECT * FROM MaintenanceTickets"
            );
        }

        public Task<DataTable> GetAssetsAsync()
        {
            return GetTableSafeAsync(
                "Assets",
                null,
                @"SELECT AssetId,
                         AssetCode,
                         AssetName,
                         Category,
                         RoomId,
                         Quantity,
                         Condition,
                         PurchaseDate,
                         PurchasePrice,
                         Description,
                         IsActive,
                         CreatedDate,
                         UpdatedDate
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

        #region CRUD Tenants

        public async Task<int> AddTenantAsync(string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry, bool isActive)
        {
            const string sql = @"
                INSERT INTO Tenants
                    (FullName, IdentityCard, PhoneNumber, Email, BirthDate, Address,
                     TemporaryRegistration, TemporaryRegistrationDate, TemporaryRegistrationExpiry, IsActive, CreatedDate, UpdatedDate)
                VALUES
                    (@FullName, @IdentityCard, @PhoneNumber, @Email, @BirthDate, @Address,
                     @TempReg, @TempRegDate, @TempRegExpiry, @IsActive, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@IdentityCard", string.IsNullOrWhiteSpace(identityCard) ? (object)DBNull.Value : identityCard);
                    cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrWhiteSpace(phoneNumber) ? (object)DBNull.Value : phoneNumber);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@BirthDate", birthDate.HasValue ? (object)birthDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(address) ? (object)DBNull.Value : address);
                    cmd.Parameters.AddWithValue("@TempReg", string.IsNullOrWhiteSpace(tempReg) ? (object)DBNull.Value : tempReg);
                    cmd.Parameters.AddWithValue("@TempRegDate", tempRegDate.HasValue ? (object)tempRegDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TempRegExpiry", tempRegExpiry.HasValue ? (object)tempRegExpiry.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateTenantAsync(int tenantId, string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry, bool isActive)
        {
            const string sql = @"
                UPDATE Tenants SET
                    FullName = @FullName,
                    IdentityCard = @IdentityCard,
                    PhoneNumber = @PhoneNumber,
                    Email = @Email,
                    BirthDate = @BirthDate,
                    Address = @Address,
                    TemporaryRegistration = @TempReg,
                    TemporaryRegistrationDate = @TempRegDate,
                    TemporaryRegistrationExpiry = @TempRegExpiry,
                    IsActive = @IsActive,
                    UpdatedDate = GETDATE()
                WHERE TenantId = @TenantId";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@IdentityCard", string.IsNullOrWhiteSpace(identityCard) ? (object)DBNull.Value : identityCard);
                    cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrWhiteSpace(phoneNumber) ? (object)DBNull.Value : phoneNumber);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@BirthDate", birthDate.HasValue ? (object)birthDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(address) ? (object)DBNull.Value : address);
                    cmd.Parameters.AddWithValue("@TempReg", string.IsNullOrWhiteSpace(tempReg) ? (object)DBNull.Value : tempReg);
                    cmd.Parameters.AddWithValue("@TempRegDate", tempRegDate.HasValue ? (object)tempRegDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TempRegExpiry", tempRegExpiry.HasValue ? (object)tempRegExpiry.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteTenantAsync(int tenantId)
        {
            const string sql = @"DELETE FROM Tenants WHERE TenantId = @TenantId";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        #region CRUD Deposits

        public async Task<int> AddDepositAsync(int tenantId, int roomId, decimal depositAmount, DateTime? depositDate,
            string depositType, string status, decimal? returnedAmount, DateTime? returnedDate, string notes)
        {
            const string sql = @"
                INSERT INTO Deposits
                    (TenantId, RoomId, DepositAmount, DepositDate, DepositType, Status, ReturnedAmount, ReturnedDate, Notes, CreatedDate)
                VALUES
                    (@TenantId, @RoomId, @DepositAmount, @DepositDate, @DepositType, @Status, @ReturnedAmount, @ReturnedDate, @Notes, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@DepositAmount", depositAmount);
                    cmd.Parameters.AddWithValue("@DepositDate", depositDate.HasValue ? (object)depositDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepositType", string.IsNullOrWhiteSpace(depositType) ? (object)DBNull.Value : depositType);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);
                    cmd.Parameters.AddWithValue("@ReturnedAmount", returnedAmount.HasValue ? (object)returnedAmount.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReturnedDate", returnedDate.HasValue ? (object)returnedDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);

                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateDepositAsync(int depositId, int tenantId, int roomId, decimal depositAmount, DateTime? depositDate,
            string depositType, string status, decimal? returnedAmount, DateTime? returnedDate, string notes)
        {
            const string sql = @"
                UPDATE Deposits SET
                    TenantId = @TenantId,
                    RoomId = @RoomId,
                    DepositAmount = @DepositAmount,
                    DepositDate = @DepositDate,
                    DepositType = @DepositType,
                    Status = @Status,
                    ReturnedAmount = @ReturnedAmount,
                    ReturnedDate = @ReturnedDate,
                    Notes = @Notes
                WHERE DepositId = @DepositId";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DepositId", depositId);
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@DepositAmount", depositAmount);
                    cmd.Parameters.AddWithValue("@DepositDate", depositDate.HasValue ? (object)depositDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepositType", string.IsNullOrWhiteSpace(depositType) ? (object)DBNull.Value : depositType);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);
                    cmd.Parameters.AddWithValue("@ReturnedAmount", returnedAmount.HasValue ? (object)returnedAmount.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReturnedDate", returnedDate.HasValue ? (object)returnedDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteDepositAsync(int depositId)
        {
            const string sql = @"DELETE FROM Deposits WHERE DepositId = @DepositId";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DepositId", depositId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

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
