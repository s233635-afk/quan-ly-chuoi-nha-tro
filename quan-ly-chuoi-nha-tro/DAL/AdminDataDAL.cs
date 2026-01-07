using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

                var allowed = ReadAdminAllowedBranchCodes();
                int[] scopedBranchIds = allowed.Length > 0 ? await ResolveScopedBranchIdsAsync(conn, allowed) : Array.Empty<int>();
                string branchIdInList = scopedBranchIds.Length > 0 ? string.Join(",", scopedBranchIds) : null;

                int maxRooms = ReadAdminMaxRooms();
                string roomIdSubquery = null;
                if (maxRooms > 0)
                {
                    // Chỉ giới hạn khi có scope chi nhánh rõ ràng (để tránh đếm 0 do config sai).
                    string where = "WHERE IsActive = 1";
                    if (branchIdInList != null)
                        where += $" AND BranchId IN ({branchIdInList})";
                    roomIdSubquery = $"SELECT TOP ({maxRooms}) RoomId FROM Rooms {where} ORDER BY RoomNumber, RoomId";
                }

                // COUNT(*) ít phụ thuộc schema; các chỉ số chi tiết sẽ try nhiều câu lệnh và fallback về 0.
                dt.Rows[0]["TotalBranches"] = await GetScalarIntBestEffortAsync(conn,
                    branchIdInList != null ? $"SELECT COUNT(*) FROM Branches WHERE IsActive = 1 AND BranchId IN ({branchIdInList})" : null,
                    "SELECT COUNT(*) FROM Branches WHERE IsActive = 1",
                    "SELECT COUNT(*) FROM Branches",
                    "SELECT COUNT(*) FROM Branch");

                dt.Rows[0]["TotalRooms"] = await GetScalarIntBestEffortAsync(conn,
                    roomIdSubquery != null ? $"SELECT COUNT(*) FROM ({roomIdSubquery}) x" : (branchIdInList != null ? $"SELECT COUNT(*) FROM Rooms WHERE IsActive = 1 AND BranchId IN ({branchIdInList})" : null),
                    "SELECT COUNT(*) FROM Rooms WHERE IsActive = 1",
                    "SELECT COUNT(*) FROM Rooms",
                    "SELECT COUNT(*) FROM Room");

                dt.Rows[0]["TotalTenants"] = await GetScalarIntBestEffortAsync(conn,
                    branchIdInList != null ? $@"SELECT COUNT(DISTINCT t.TenantId)
                      FROM Tenants t
                      INNER JOIN Contracts c ON c.TenantId = t.TenantId
                      INNER JOIN Rooms r ON r.RoomId = c.RoomId
                      WHERE t.IsActive = 1
                        AND r.BranchId IN ({branchIdInList})
                        {(roomIdSubquery != null ? $"AND r.RoomId IN ({roomIdSubquery})" : string.Empty)}" : null,
                    "SELECT COUNT(*) FROM Tenants WHERE IsActive = 1",
                    "SELECT COUNT(*) FROM Tenants",
                    "SELECT COUNT(*) FROM Tenant");

                dt.Rows[0]["TotalContracts"] = await GetScalarIntBestEffortAsync(conn,
                    branchIdInList != null ? $@"SELECT COUNT(*) FROM Contracts c
                      INNER JOIN Rooms r ON r.RoomId = c.RoomId
                      WHERE r.BranchId IN ({branchIdInList})
                        {(roomIdSubquery != null ? $"AND r.RoomId IN ({roomIdSubquery})" : string.Empty)}" : null,
                    "SELECT COUNT(*) FROM Contracts",
                    "SELECT COUNT(*) FROM Contract");

                dt.Rows[0]["TotalInvoices"] = await GetScalarIntBestEffortAsync(conn,
                    branchIdInList != null ? $@"SELECT COUNT(*) FROM Invoices i
                      INNER JOIN Rooms r ON r.RoomId = i.RoomId
                      WHERE r.BranchId IN ({branchIdInList})
                        {(roomIdSubquery != null ? $"AND r.RoomId IN ({roomIdSubquery})" : string.Empty)}" : null,
                    "SELECT COUNT(*) FROM Invoices",
                    "SELECT COUNT(*) FROM Invoice");

                // Hóa đơn còn nợ (ưu tiên RemainingAmount, fallback theo status)
                dt.Rows[0]["OutstandingInvoiceCount"] = await GetScalarIntBestEffortAsync(conn,
                    branchIdInList != null ? $@"SELECT COUNT(*) FROM Invoices i
                      INNER JOIN Rooms r ON r.RoomId = i.RoomId
                      WHERE i.RemainingAmount > 0
                        AND r.BranchId IN ({branchIdInList})
                        {(roomIdSubquery != null ? $"AND r.RoomId IN ({roomIdSubquery})" : string.Empty)}" : null,
                    "SELECT COUNT(*) FROM Invoices WHERE RemainingAmount > 0",
                    "SELECT COUNT(*) FROM Invoices WHERE (TotalAmount - ISNULL(PaidAmount,0)) > 0",
                    "SELECT COUNT(*) FROM Invoices WHERE Status IN (0, 'Unpaid', 'UNPAID', N'Chưa thanh toán', N'Chua thanh toan')");

                dt.Rows[0]["OutstandingAmount"] = await GetScalarDecimalBestEffortAsync(conn,
                    branchIdInList != null ? $@"SELECT CAST(ISNULL(SUM(i.RemainingAmount),0) AS DECIMAL(18,2))
                      FROM Invoices i
                      INNER JOIN Rooms r ON r.RoomId = i.RoomId
                      WHERE i.RemainingAmount > 0
                        AND r.BranchId IN ({branchIdInList})
                        {(roomIdSubquery != null ? $"AND r.RoomId IN ({roomIdSubquery})" : string.Empty)}" : null,
                    "SELECT CAST(ISNULL(SUM(RemainingAmount),0) AS DECIMAL(18,2)) FROM Invoices WHERE RemainingAmount > 0",
                    "SELECT CAST(ISNULL(SUM(TotalAmount - ISNULL(PaidAmount,0)),0) AS DECIMAL(18,2)) FROM Invoices WHERE (TotalAmount - ISNULL(PaidAmount,0)) > 0");

                // Đặt cọc (DepositAmount hoặc Amount)
                dt.Rows[0]["TotalDeposits"] = await GetScalarIntBestEffortAsync(conn,
                    branchIdInList != null ? $@"SELECT COUNT(*) FROM Deposits d
                      INNER JOIN Rooms r ON r.RoomId = d.RoomId
                      WHERE r.BranchId IN ({branchIdInList})
                        {(roomIdSubquery != null ? $"AND r.RoomId IN ({roomIdSubquery})" : string.Empty)}" : null,
                    "SELECT COUNT(*) FROM Deposits",
                    "SELECT COUNT(*) FROM Deposit");

                dt.Rows[0]["DepositAmount"] = await GetScalarDecimalBestEffortAsync(conn,
                    branchIdInList != null ? $@"SELECT CAST(ISNULL(SUM(d.DepositAmount),0) AS DECIMAL(18,2))
                      FROM Deposits d
                      INNER JOIN Rooms r ON r.RoomId = d.RoomId
                      WHERE r.BranchId IN ({branchIdInList})
                        {(roomIdSubquery != null ? $"AND r.RoomId IN ({roomIdSubquery})" : string.Empty)}" : null,
                    "SELECT CAST(ISNULL(SUM(DepositAmount),0) AS DECIMAL(18,2)) FROM Deposits",
                    "SELECT CAST(ISNULL(SUM(Amount),0) AS DECIMAL(18,2)) FROM Deposits");

                // Thanh toán tháng này
                dt.Rows[0]["PaymentsThisMonth"] = await GetScalarDecimalBestEffortAsync(conn,
                    branchIdInList != null ? $@"SELECT CAST(ISNULL(SUM(p.PaymentAmount),0) AS DECIMAL(18,2))
                      FROM Payments p
                      INNER JOIN Invoices i ON i.InvoiceId = p.InvoiceId
                      INNER JOIN Rooms r ON r.RoomId = i.RoomId
                      WHERE p.PaymentDate >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
                        AND p.PaymentDate < DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
                        AND r.BranchId IN ({branchIdInList})
                        {(roomIdSubquery != null ? $"AND r.RoomId IN ({roomIdSubquery})" : string.Empty)}" : null,
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
                    branchIdInList != null ? $@"SELECT COUNT(*) FROM MaintenanceTickets t
                      INNER JOIN Rooms r ON r.RoomId = t.RoomId
                      WHERE t.Status NOT IN ('Done','DONE',N'Hoàn tất',N'Hoan tat',2)
                        AND r.BranchId IN ({branchIdInList})
                        {(roomIdSubquery != null ? $"AND r.RoomId IN ({roomIdSubquery})" : string.Empty)}" : null,
                    "SELECT COUNT(*) FROM MaintenanceTickets WHERE Status NOT IN ('Done','DONE',N'Hoàn tất',N'Hoan tat',2)",
                    "SELECT COUNT(*) FROM MaintenanceTickets",
                    "SELECT COUNT(*) FROM MaintenanceRecords WHERE Status NOT IN (2, 'Done','DONE',N'Hoàn tất',N'Hoan tat')",
                    "SELECT COUNT(*) FROM MaintenanceRecords");
            }

            return dt;
        }

        #endregion

        private static async Task<int[]> ResolveScopedBranchIdsAsync(SqlConnection conn, string[] allowedCodes)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (allowedCodes == null || allowedCodes.Length == 0) return Array.Empty<int>();

            // 1) try by BranchCode from config
            try
            {
                string inList = BuildNvarcharInList(allowedCodes);
                using (var cmd = new SqlCommand($"SELECT BranchId FROM Branches WHERE IsActive = 1 AND BranchCode IN ({inList}) ORDER BY BranchId", conn))
                {
                    var list = new System.Collections.Generic.List<int>();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (!reader.IsDBNull(0))
                                list.Add(Convert.ToInt32(reader.GetValue(0)));
                        }
                    }
                    if (list.Count > 0) return list.ToArray();
                }
            }
            catch
            {
                // ignore
            }

            // 2) fallback by name contains "Cần Thơ"/"Can Tho"
            try
            {
                using (var cmd = new SqlCommand(
                    "SELECT TOP 2 BranchId FROM Branches WHERE IsActive = 1 AND (BranchName LIKE N'%Cần Thơ%' OR BranchName LIKE N'%Can Tho%') ORDER BY BranchId", conn))
                {
                    var list = new System.Collections.Generic.List<int>();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (!reader.IsDBNull(0))
                                list.Add(Convert.ToInt32(reader.GetValue(0)));
                        }
                    }
                    if (list.Count > 0) return list.ToArray();
                }
            }
            catch
            {
                // ignore
            }

            // 3) last resort: take first 2 active branches
            try
            {
                using (var cmd = new SqlCommand("SELECT TOP 2 BranchId FROM Branches WHERE IsActive = 1 ORDER BY BranchId", conn))
                {
                    var list = new System.Collections.Generic.List<int>();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (!reader.IsDBNull(0))
                                list.Add(Convert.ToInt32(reader.GetValue(0)));
                        }
                    }
                    return list.ToArray();
                }
            }
            catch
            {
                return Array.Empty<int>();
            }
        }

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
                if (string.IsNullOrWhiteSpace(sql)) continue;
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
                if (string.IsNullOrWhiteSpace(sql)) continue;
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

        private static string[] ReadAdminAllowedBranchCodes()
        {
            string raw = null;
            try { raw = ConfigurationManager.AppSettings["AdminAllowedBranchCodes"]; } catch { }

            // Missing key => default 2 chi nhánh Cần Thơ.
            if (raw == null) raw = "CT01,CT02";

            // Present-but-empty => no scope.
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();

            return raw
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static int ReadAdminMaxRooms()
        {
            string raw = null;
            try { raw = ConfigurationManager.AppSettings["AdminMaxRooms"]; } catch { }

            if (string.IsNullOrWhiteSpace(raw)) return 0;
            if (!int.TryParse(raw.Trim(), out var limit)) return 0;
            return limit > 0 ? limit : 0;
        }

        private static string BuildNvarcharInList(string[] values)
        {
            if (values == null || values.Length == 0) return "N''";
            return string.Join(",", values.Select(v => "N'" + (v ?? string.Empty).Replace("'", "''") + "'"));
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
            bool? isActive,
            int? occupants = null)
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
                bool hasOccupants = await ColumnExistsAsync(conn, tbl, "Occupants");
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
                if (hasOccupants) sets.Add("Occupants = @Occupants");
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
                    if (hasOccupants) cmd.Parameters.AddWithValue("@Occupants", occupants.HasValue ? (object)occupants.Value : DBNull.Value);
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

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        if (await TableExistsAsync("Payments") && await TableExistsAsync("Invoices"))
                        {
                            const string sqlPayments = @"
                                DELETE FROM Payments
                                WHERE InvoiceId IN (SELECT InvoiceId FROM Invoices WHERE RoomId = @RoomId);";
                            using (var cmd = new SqlCommand(sqlPayments, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@RoomId", roomId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("Invoices"))
                        {
                            const string sql = @"DELETE FROM Invoices WHERE RoomId = @RoomId;";
                            using (var cmd = new SqlCommand(sql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@RoomId", roomId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("Deposits"))
                        {
                            const string sql = @"DELETE FROM Deposits WHERE RoomId = @RoomId;";
                            using (var cmd = new SqlCommand(sql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@RoomId", roomId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("TenantHistory"))
                        {
                            const string sql = @"DELETE FROM TenantHistory WHERE RoomId = @RoomId;";
                            using (var cmd = new SqlCommand(sql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@RoomId", roomId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("TenantRoomHistory"))
                        {
                            const string sql = @"DELETE FROM TenantRoomHistory WHERE RoomId = @RoomId;";
                            using (var cmd = new SqlCommand(sql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@RoomId", roomId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("Contracts"))
                        {
                            const string sql = @"DELETE FROM Contracts WHERE RoomId = @RoomId;";
                            using (var cmd = new SqlCommand(sql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@RoomId", roomId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("UtilityReadings"))
                        {
                            const string sql = @"DELETE FROM UtilityReadings WHERE RoomId = @RoomId;";
                            using (var cmd = new SqlCommand(sql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@RoomId", roomId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("MaintenanceTickets"))
                        {
                            const string sql = @"DELETE FROM MaintenanceTickets WHERE RoomId = @RoomId;";
                            using (var cmd = new SqlCommand(sql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@RoomId", roomId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("Assets"))
                        {
                            const string sql = @"DELETE FROM Assets WHERE RoomId = @RoomId;";
                            using (var cmd = new SqlCommand(sql, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@RoomId", roomId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        const string sqlRoom = @"DELETE FROM Rooms WHERE RoomId = @RoomId;";
                        using (var cmd = new SqlCommand(sqlRoom, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@RoomId", roomId);
                            int affected = await cmd.ExecuteNonQueryAsync();
                            tx.Commit();
                            return affected > 0;
                        }
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        #endregion

        public async Task<DataTable> GetRoomsAsync()
        {
            var occupantsSelect = string.Empty;
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                if (await ColumnExistsAsync(conn, "Rooms", "Occupants"))
                    occupantsSelect = ", r.Occupants";
            }

            var sql = $@"SELECT r.RoomId,
                         r.RoomNumber,
                         r.BranchId,
                         b.BranchName,
                         r.SectionId,
                         s.SectionName,
                         r.RoomTypeId,
                         rt.RoomTypeName,
                         r.RoomPrice,
                         r.CurrentStatusId,
                         st.StatusName,
                         r.Floor,
                         r.Area,
                         r.IsActive,
                         r.CreatedDate,
                         r.UpdatedDate{occupantsSelect}
                  FROM Rooms r
                  LEFT JOIN Branches b ON b.BranchId = r.BranchId
                  LEFT JOIN BranchSections s ON s.SectionId = r.SectionId
                  LEFT JOIN RoomTypes rt ON rt.RoomTypeId = r.RoomTypeId
                  LEFT JOIN RoomStatuses st ON st.StatusId = r.CurrentStatusId
                  ORDER BY r.RoomNumber";

            return await GetTableSafeAsync(
                "Rooms",
                null,
                sql,
                "SELECT * FROM Rooms"
            );
        }

        public Task<DataTable> GetBranchesAsync()
        {
            return GetTableSafeAsync(
                "Branches",
                null,
                @"SELECT BranchId, BranchCode, BranchName, IsActive
                  FROM Branches
                  ORDER BY BranchName",
                "SELECT * FROM Branches"
            );
        }

        public Task<DataTable> GetBranchSectionsAsync(int? branchId = null)
        {
            if (branchId.HasValue)
            {
                return GetTableSafeAsync(
                    "BranchSections",
                    cmd => cmd.Parameters.AddWithValue("@BranchId", branchId.Value),
                    @"SELECT SectionId, BranchId, SectionCode, SectionName, Description, IsActive
                      FROM BranchSections
                      WHERE BranchId = @BranchId
                      ORDER BY SectionName",
                    "SELECT * FROM BranchSections WHERE BranchId = @BranchId",
                    "SELECT * FROM BranchSections"
                );
            }

            return GetTableSafeAsync(
                "BranchSections",
                null,
                @"SELECT SectionId, BranchId, SectionCode, SectionName, Description, IsActive
                  FROM BranchSections
                  ORDER BY SectionName",
                "SELECT * FROM BranchSections"
            );
        }

        public Task<DataTable> GetRoomTypesAsync()
        {
            return GetTableSafeAsync(
                "RoomTypes",
                null,
                @"SELECT RoomTypeId, RoomTypeName, DefaultPrice, Amenities, MaxCapacity, Description, IsActive
                  FROM RoomTypes
                  ORDER BY RoomTypeName",
                "SELECT * FROM RoomTypes"
            );
        }

        public Task<DataTable> GetRoomStatusesAsync()
        {
            return GetTableSafeAsync(
                "RoomStatuses",
                null,
                @"SELECT StatusId, StatusName, Description
                  FROM RoomStatuses
                  ORDER BY StatusId",
                "SELECT * FROM RoomStatuses"
            );
        }

        #region BranchSections CRUD

        public async Task<int> AddBranchSectionAsync(int branchId, string sectionCode, string sectionName, string description, bool isActive)
        {
            if (branchId <= 0) throw new Exception("Chi nhánh không hợp lệ.");
            if (string.IsNullOrWhiteSpace(sectionCode)) throw new Exception("Mã khu/dãy không được để trống.");
            if (string.IsNullOrWhiteSpace(sectionName)) throw new Exception("Tên khu/dãy không được để trống.");

            if (!await TableExistsAsync("BranchSections"))
                throw new Exception("Bảng BranchSections không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "BranchSections";
                bool hasBranchId = await ColumnExistsAsync(conn, tbl, "BranchId");
                bool hasSectionCode = await ColumnExistsAsync(conn, tbl, "SectionCode");
                bool hasSectionName = await ColumnExistsAsync(conn, tbl, "SectionName");
                bool hasDescription = await ColumnExistsAsync(conn, tbl, "Description");
                bool hasIsActive = await ColumnExistsAsync(conn, tbl, "IsActive");
                bool hasCreatedDate = await ColumnExistsAsync(conn, tbl, "CreatedDate");

                var cols = new System.Collections.Generic.List<string>();
                var vals = new System.Collections.Generic.List<string>();

                if (hasBranchId) { cols.Add("BranchId"); vals.Add("@BranchId"); }
                if (hasSectionCode) { cols.Add("SectionCode"); vals.Add("@SectionCode"); }
                if (hasSectionName) { cols.Add("SectionName"); vals.Add("@SectionName"); }
                if (hasDescription) { cols.Add("Description"); vals.Add("@Description"); }
                if (hasIsActive) { cols.Add("IsActive"); vals.Add("@IsActive"); }
                if (hasCreatedDate) { cols.Add("CreatedDate"); vals.Add("GETDATE()"); }

                string sql = $"INSERT INTO {tbl} ({string.Join(",", cols)}) VALUES ({string.Join(",", vals)}); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (hasBranchId) cmd.Parameters.AddWithValue("@BranchId", branchId);
                    if (hasSectionCode) cmd.Parameters.AddWithValue("@SectionCode", sectionCode.Trim());
                    if (hasSectionName) cmd.Parameters.AddWithValue("@SectionName", sectionName.Trim());
                    if (hasDescription) cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());
                    if (hasIsActive) cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                    var result = await cmd.ExecuteScalarAsync();
                    return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> UpdateBranchSectionAsync(int sectionId, int branchId, string sectionCode, string sectionName, string description, bool isActive)
        {
            if (sectionId <= 0) throw new Exception("ID khu/dãy không hợp lệ.");
            if (branchId <= 0) throw new Exception("Chi nhánh không hợp lệ.");
            if (string.IsNullOrWhiteSpace(sectionCode)) throw new Exception("Mã khu/dãy không được để trống.");
            if (string.IsNullOrWhiteSpace(sectionName)) throw new Exception("Tên khu/dãy không được để trống.");

            if (!await TableExistsAsync("BranchSections"))
                throw new Exception("Bảng BranchSections không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "BranchSections";
                bool hasBranchId = await ColumnExistsAsync(conn, tbl, "BranchId");
                bool hasSectionCode = await ColumnExistsAsync(conn, tbl, "SectionCode");
                bool hasSectionName = await ColumnExistsAsync(conn, tbl, "SectionName");
                bool hasDescription = await ColumnExistsAsync(conn, tbl, "Description");
                bool hasIsActive = await ColumnExistsAsync(conn, tbl, "IsActive");

                var sets = new System.Collections.Generic.List<string>();
                if (hasBranchId) sets.Add("BranchId=@BranchId");
                if (hasSectionCode) sets.Add("SectionCode=@SectionCode");
                if (hasSectionName) sets.Add("SectionName=@SectionName");
                if (hasDescription) sets.Add("Description=@Description");
                if (hasIsActive) sets.Add("IsActive=@IsActive");

                string sql = $"UPDATE {tbl} SET {string.Join(",", sets)} WHERE SectionId=@SectionId";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SectionId", sectionId);
                    if (hasBranchId) cmd.Parameters.AddWithValue("@BranchId", branchId);
                    if (hasSectionCode) cmd.Parameters.AddWithValue("@SectionCode", sectionCode.Trim());
                    if (hasSectionName) cmd.Parameters.AddWithValue("@SectionName", sectionName.Trim());
                    if (hasDescription) cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());
                    if (hasIsActive) cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteBranchSectionAsync(int sectionId)
        {
            if (sectionId <= 0) throw new Exception("ID khu/dãy không hợp lệ.");

            if (!await TableExistsAsync("BranchSections"))
                throw new Exception("Bảng BranchSections không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "BranchSections";
                bool hasIsActive = await ColumnExistsAsync(conn, tbl, "IsActive");

                string sql = hasIsActive
                    ? $"UPDATE {tbl} SET IsActive = 0 WHERE SectionId=@SectionId"
                    : $"DELETE FROM {tbl} WHERE SectionId=@SectionId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SectionId", sectionId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        #region RoomTypes CRUD

        public async Task<int> AddRoomTypeAsync(string roomTypeName, decimal? defaultPrice, string amenities, int? maxCapacity, string description, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(roomTypeName)) throw new Exception("Tên loại phòng không được để trống.");

            if (!await TableExistsAsync("RoomTypes"))
                throw new Exception("Bảng RoomTypes không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "RoomTypes";
                bool hasName = await ColumnExistsAsync(conn, tbl, "RoomTypeName");
                bool hasDefaultPrice = await ColumnExistsAsync(conn, tbl, "DefaultPrice");
                bool hasAmenities = await ColumnExistsAsync(conn, tbl, "Amenities");
                bool hasMaxCapacity = await ColumnExistsAsync(conn, tbl, "MaxCapacity");
                bool hasDescription = await ColumnExistsAsync(conn, tbl, "Description");
                bool hasIsActive = await ColumnExistsAsync(conn, tbl, "IsActive");
                bool hasCreatedDate = await ColumnExistsAsync(conn, tbl, "CreatedDate");

                var cols = new System.Collections.Generic.List<string>();
                var vals = new System.Collections.Generic.List<string>();

                if (hasName) { cols.Add("RoomTypeName"); vals.Add("@RoomTypeName"); }
                if (hasDefaultPrice) { cols.Add("DefaultPrice"); vals.Add("@DefaultPrice"); }
                if (hasAmenities) { cols.Add("Amenities"); vals.Add("@Amenities"); }
                if (hasMaxCapacity) { cols.Add("MaxCapacity"); vals.Add("@MaxCapacity"); }
                if (hasDescription) { cols.Add("Description"); vals.Add("@Description"); }
                if (hasIsActive) { cols.Add("IsActive"); vals.Add("@IsActive"); }
                if (hasCreatedDate) { cols.Add("CreatedDate"); vals.Add("GETDATE()"); }

                string sql = $"INSERT INTO {tbl} ({string.Join(",", cols)}) VALUES ({string.Join(",", vals)}); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (hasName) cmd.Parameters.AddWithValue("@RoomTypeName", roomTypeName.Trim());
                    if (hasDefaultPrice) cmd.Parameters.AddWithValue("@DefaultPrice", (object)defaultPrice ?? DBNull.Value);
                    if (hasAmenities) cmd.Parameters.AddWithValue("@Amenities", string.IsNullOrWhiteSpace(amenities) ? (object)DBNull.Value : amenities.Trim());
                    if (hasMaxCapacity) cmd.Parameters.AddWithValue("@MaxCapacity", (object)maxCapacity ?? DBNull.Value);
                    if (hasDescription) cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());
                    if (hasIsActive) cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);

                    var result = await cmd.ExecuteScalarAsync();
                    return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> UpdateRoomTypeAsync(int roomTypeId, string roomTypeName, decimal? defaultPrice, string amenities, int? maxCapacity, string description, bool isActive)
        {
            if (roomTypeId <= 0) throw new Exception("ID loại phòng không hợp lệ.");
            if (string.IsNullOrWhiteSpace(roomTypeName)) throw new Exception("Tên loại phòng không được để trống.");

            if (!await TableExistsAsync("RoomTypes"))
                throw new Exception("Bảng RoomTypes không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "RoomTypes";
                bool hasName = await ColumnExistsAsync(conn, tbl, "RoomTypeName");
                bool hasDefaultPrice = await ColumnExistsAsync(conn, tbl, "DefaultPrice");
                bool hasAmenities = await ColumnExistsAsync(conn, tbl, "Amenities");
                bool hasMaxCapacity = await ColumnExistsAsync(conn, tbl, "MaxCapacity");
                bool hasDescription = await ColumnExistsAsync(conn, tbl, "Description");
                bool hasIsActive = await ColumnExistsAsync(conn, tbl, "IsActive");

                var sets = new System.Collections.Generic.List<string>();
                if (hasName) sets.Add("RoomTypeName=@RoomTypeName");
                if (hasDefaultPrice) sets.Add("DefaultPrice=@DefaultPrice");
                if (hasAmenities) sets.Add("Amenities=@Amenities");
                if (hasMaxCapacity) sets.Add("MaxCapacity=@MaxCapacity");
                if (hasDescription) sets.Add("Description=@Description");
                if (hasIsActive) sets.Add("IsActive=@IsActive");

                string sql = $"UPDATE {tbl} SET {string.Join(",", sets)} WHERE RoomTypeId=@RoomTypeId";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomTypeId", roomTypeId);
                    if (hasName) cmd.Parameters.AddWithValue("@RoomTypeName", roomTypeName.Trim());
                    if (hasDefaultPrice) cmd.Parameters.AddWithValue("@DefaultPrice", (object)defaultPrice ?? DBNull.Value);
                    if (hasAmenities) cmd.Parameters.AddWithValue("@Amenities", string.IsNullOrWhiteSpace(amenities) ? (object)DBNull.Value : amenities.Trim());
                    if (hasMaxCapacity) cmd.Parameters.AddWithValue("@MaxCapacity", (object)maxCapacity ?? DBNull.Value);
                    if (hasDescription) cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());
                    if (hasIsActive) cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteRoomTypeAsync(int roomTypeId)
        {
            if (roomTypeId <= 0) throw new Exception("ID loại phòng không hợp lệ.");

            if (!await TableExistsAsync("RoomTypes"))
                throw new Exception("Bảng RoomTypes không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "RoomTypes";
                bool hasIsActive = await ColumnExistsAsync(conn, tbl, "IsActive");

                string sql = hasIsActive
                    ? $"UPDATE {tbl} SET IsActive = 0 WHERE RoomTypeId=@RoomTypeId"
                    : $"DELETE FROM {tbl} WHERE RoomTypeId=@RoomTypeId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomTypeId", roomTypeId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        #region RoomStatuses CRUD

        public async Task<int> AddRoomStatusAsync(string statusName, string description)
        {
            if (string.IsNullOrWhiteSpace(statusName)) throw new Exception("Tên trạng thái không được để trống.");

            if (!await TableExistsAsync("RoomStatuses"))
                throw new Exception("Bảng RoomStatuses không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "RoomStatuses";
                bool hasName = await ColumnExistsAsync(conn, tbl, "StatusName");
                bool hasDescription = await ColumnExistsAsync(conn, tbl, "Description");

                var cols = new System.Collections.Generic.List<string>();
                var vals = new System.Collections.Generic.List<string>();

                if (hasName) { cols.Add("StatusName"); vals.Add("@StatusName"); }
                if (hasDescription) { cols.Add("Description"); vals.Add("@Description"); }

                string sql = $"INSERT INTO {tbl} ({string.Join(",", cols)}) VALUES ({string.Join(",", vals)}); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (hasName) cmd.Parameters.AddWithValue("@StatusName", statusName.Trim());
                    if (hasDescription) cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());

                    var result = await cmd.ExecuteScalarAsync();
                    return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> UpdateRoomStatusAsync(int statusId, string statusName, string description)
        {
            if (statusId <= 0) throw new Exception("ID trạng thái không hợp lệ.");
            if (string.IsNullOrWhiteSpace(statusName)) throw new Exception("Tên trạng thái không được để trống.");

            if (!await TableExistsAsync("RoomStatuses"))
                throw new Exception("Bảng RoomStatuses không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "RoomStatuses";
                bool hasName = await ColumnExistsAsync(conn, tbl, "StatusName");
                bool hasDescription = await ColumnExistsAsync(conn, tbl, "Description");

                var sets = new System.Collections.Generic.List<string>();
                if (hasName) sets.Add("StatusName=@StatusName");
                if (hasDescription) sets.Add("Description=@Description");

                string sql = $"UPDATE {tbl} SET {string.Join(",", sets)} WHERE StatusId=@StatusId";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StatusId", statusId);
                    if (hasName) cmd.Parameters.AddWithValue("@StatusName", statusName.Trim());
                    if (hasDescription) cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());

                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteRoomStatusAsync(int statusId)
        {
            if (statusId <= 0) throw new Exception("ID trạng thái không hợp lệ.");

            if (!await TableExistsAsync("RoomStatuses"))
                throw new Exception("Bảng RoomStatuses không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string sql = "DELETE FROM RoomStatuses WHERE StatusId=@StatusId";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StatusId", statusId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        public async Task<DataTable> GetUsersByRoleAsync(int roleId)
        {
            await EnsureUsersPhoneColumnBestEffortAsync();

            return await GetTableSafeAsync(
                "Users",
                cmd => cmd.Parameters.AddWithValue("@roleId", roleId),
                @"SELECT u.UserId,
                         u.Username AS UserName,
                         u.Email,
                         u.FullName,
                         u.Phone,
                         u.RoleId,
                         u.BranchId,
                         b.BranchName,
                         u.IsActive,
                         u.CreatedDate,
                         u.UpdatedDate
                  FROM Users u
                  LEFT JOIN Branches b ON b.BranchId = u.BranchId
                  WHERE u.RoleId = @roleId
                  ORDER BY u.UserId DESC",
                // Nếu DB chưa có cột Phone, vẫn trả về cột Phone dạng NULL để UI không bị lỗi.
                @"SELECT u.UserId,
                         u.Username AS UserName,
                         u.Email,
                         u.FullName,
                         CAST(NULL AS NVARCHAR(20)) AS Phone,
                         u.RoleId,
                         u.BranchId,
                         b.BranchName,
                         u.IsActive,
                         u.CreatedDate,
                         u.UpdatedDate
                  FROM Users u
                  LEFT JOIN Branches b ON b.BranchId = u.BranchId
                  WHERE u.RoleId = @roleId
                  ORDER BY u.UserId DESC",
                @"SELECT UserId,
                         Username AS UserName,
                         Email,
                         FullName,
                         CAST(NULL AS NVARCHAR(20)) AS Phone,
                         RoleId,
                         BranchId,
                         CAST(NULL AS NVARCHAR(255)) AS BranchName,
                         IsActive,
                         CreatedDate,
                         UpdatedDate
                  FROM Users
                  WHERE RoleId = @roleId
                  ORDER BY UserId DESC",
                @"SELECT UserId,
                         UserName,
                         Email,
                         FullName,
                         CAST(NULL AS NVARCHAR(20)) AS Phone,
                         RoleId,
                         CAST(NULL AS INT) AS BranchId,
                         CAST(NULL AS NVARCHAR(255)) AS BranchName,
                         IsActive,
                         CreatedDate,
                         CAST(NULL AS DATETIME) AS UpdatedDate
                  FROM Users
                  WHERE RoleId = @roleId
                  ORDER BY UserId DESC"
            );
        }

        public async Task<DataTable> GetUsersByBranchAsync(int branchId)
        {
            await EnsureUsersPhoneColumnBestEffortAsync();

            return await GetTableSafeAsync(
                "Users",
                cmd => cmd.Parameters.AddWithValue("@BranchId", branchId),
                @"SELECT u.UserId,
                         u.Username AS UserName,
                         u.Email,
                         u.FullName,
                         u.Phone,
                         u.RoleId,
                         r.RoleName,
                         u.BranchId,
                         b.BranchName,
                         u.IsActive,
                         u.CreatedDate,
                         u.UpdatedDate
                  FROM Users u
                  LEFT JOIN Roles r ON r.RoleId = u.RoleId
                  LEFT JOIN Branches b ON b.BranchId = u.BranchId
                  WHERE u.BranchId = @BranchId AND u.RoleId <> 1
                  ORDER BY u.RoleId, u.UserId DESC",
                // Nếu DB chưa có cột Phone, vẫn trả về cột Phone dạng NULL để UI không bị lỗi.
                @"SELECT u.UserId,
                         u.Username AS UserName,
                         u.Email,
                         u.FullName,
                         CAST(NULL AS NVARCHAR(20)) AS Phone,
                         u.RoleId,
                         r.RoleName,
                         u.BranchId,
                         b.BranchName,
                         u.IsActive,
                         u.CreatedDate,
                         u.UpdatedDate
                  FROM Users u
                  LEFT JOIN Roles r ON r.RoleId = u.RoleId
                  LEFT JOIN Branches b ON b.BranchId = u.BranchId
                  WHERE u.BranchId = @BranchId AND u.RoleId <> 1
                  ORDER BY u.RoleId, u.UserId DESC",
                @"SELECT UserId,
                         Username AS UserName,
                         Email,
                         FullName,
                         CAST(NULL AS NVARCHAR(20)) AS Phone,
                         RoleId,
                         CAST(NULL AS NVARCHAR(50)) AS RoleName,
                         BranchId,
                         CAST(NULL AS NVARCHAR(255)) AS BranchName,
                         IsActive,
                         CreatedDate,
                         UpdatedDate
                  FROM Users
                  WHERE BranchId = @BranchId AND RoleId <> 1
                  ORDER BY RoleId, UserId DESC",
                @"SELECT UserId,
                         UserName,
                         Email,
                         FullName,
                         CAST(NULL AS NVARCHAR(20)) AS Phone,
                         RoleId,
                         CAST(NULL AS NVARCHAR(50)) AS RoleName,
                         CAST(NULL AS INT) AS BranchId,
                         CAST(NULL AS NVARCHAR(255)) AS BranchName,
                         IsActive,
                         CreatedDate,
                         CAST(NULL AS DATETIME) AS UpdatedDate
                  FROM Users
                  WHERE RoleId <> 1
                  ORDER BY RoleId, UserId DESC"
            );
        }

        #region CRUD Staff (RoleId = 2)

        public async Task<int> AddStaffUserAsync(string username, string password, string fullName, string email, string phone, int? branchId, bool isActive)
        {
            await EnsureUsersPhoneColumnBestEffortAsync();

            const string sqlWithPhone = @"
                INSERT INTO Users (Username, Password, Email, FullName, Phone, RoleId, BranchId, IsActive, CreatedDate, UpdatedDate)
                VALUES (@Username, @Password, @Email, @FullName, @Phone, 2, @BranchId, @IsActive, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            const string sqlNoPhone = @"
                INSERT INTO Users (Username, Password, Email, FullName, RoleId, BranchId, IsActive, CreatedDate, UpdatedDate)
                VALUES (@Username, @Password, @Email, @FullName, 2, @BranchId, @IsActive, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sqlWithPhone, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);
                    cmd.Parameters.AddWithValue("@BranchId", branchId.HasValue && branchId.Value > 0 ? (object)branchId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                    catch (SqlException ex) when (IsInvalidColumn(ex, "Phone"))
                    {
                        cmd.CommandText = sqlNoPhone;
                        cmd.Parameters.RemoveAt("@Phone");
                        var result = await cmd.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
        }

        public async Task<bool> UpdateStaffUserAsync(int userId, string fullName, string email, string phone, int? branchId, bool isActive, string newPassword = null)
        {
            await EnsureUsersPhoneColumnBestEffortAsync();

            string sql = @"
                UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    BranchId = @BranchId,
                    IsActive = @IsActive,
                    UpdatedDate = GETDATE()
                WHERE UserId = @UserId AND RoleId = 2";

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                sql = @"
                UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    BranchId = @BranchId,
                    IsActive = @IsActive,
                    Password = @Password,
                    UpdatedDate = GETDATE()
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
                    cmd.Parameters.AddWithValue("@BranchId", branchId.HasValue && branchId.Value > 0 ? (object)branchId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    if (!string.IsNullOrWhiteSpace(newPassword))
                    {
                        cmd.Parameters.AddWithValue("@Password", newPassword);
                    }

                    try
                    {
                        int affected = await cmd.ExecuteNonQueryAsync();
                        return affected > 0;
                    }
                    catch (SqlException ex) when (IsInvalidColumn(ex, "Phone"))
                    {
                        string sqlNoPhone = !string.IsNullOrWhiteSpace(newPassword)
                            ? @"
                                UPDATE Users SET
                                    FullName = @FullName,
                                    Email = @Email,
                                    BranchId = @BranchId,
                                    IsActive = @IsActive,
                                    Password = @Password,
                                    UpdatedDate = GETDATE()
                                WHERE UserId = @UserId AND RoleId = 2"
                            : @"
                                UPDATE Users SET
                                    FullName = @FullName,
                                    Email = @Email,
                                    BranchId = @BranchId,
                                    IsActive = @IsActive,
                                    UpdatedDate = GETDATE()
                                WHERE UserId = @UserId AND RoleId = 2";

                        cmd.CommandText = sqlNoPhone;
                        cmd.Parameters.RemoveAt("@Phone");
                        int affected = await cmd.ExecuteNonQueryAsync();
                        return affected > 0;
                    }
                }
            }
        }

        private async Task EnsureUsersPhoneColumnBestEffortAsync()
        {
            try
            {
                const string sql = @"
                    IF OBJECT_ID('Users', 'U') IS NOT NULL AND COL_LENGTH('Users', 'Phone') IS NULL
                    BEGIN
                        ALTER TABLE Users ADD Phone NVARCHAR(20) NULL;
                    END";

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {
                // Best-effort: nếu không có quyền ALTER TABLE, các query sẽ tự fallback không dùng Phone.
            }
        }

        private static bool IsInvalidColumn(SqlException ex, string columnName)
        {
            if (ex == null || string.IsNullOrWhiteSpace(columnName)) return false;
            string token = "Invalid column name '" + columnName + "'";
            return ex.Message != null && ex.Message.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
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
                         FrontIdPhoto,
                         BackIdPhoto,
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
                         r.RoomNumber,
                         r.BranchId,
                         CheckInDate,
                         CheckOutDate,
                         Status,
                         Notes,
                         CreatedDate
                  FROM TenantRoomHistory h
                  LEFT JOIN Rooms r ON r.RoomId = h.RoomId",
                "SELECT * FROM TenantRoomHistory"
            );
        }

        #region CRUD Dependents

        public async Task<int> AddDependentAsync(int tenantId, string fullName, string relationship, string phoneNumber)
        {
            const string sql = @"
                INSERT INTO Dependents (TenantId, FullName, Relationship, PhoneNumber, CreatedDate)
                VALUES (@TenantId, @FullName, @Relationship, @PhoneNumber, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Relationship", string.IsNullOrWhiteSpace(relationship) ? (object)DBNull.Value : relationship);
                    cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrWhiteSpace(phoneNumber) ? (object)DBNull.Value : phoneNumber);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateDependentAsync(int dependentId, int tenantId, string fullName, string relationship, string phoneNumber)
        {
            const string sql = @"
                UPDATE Dependents SET
                    TenantId = @TenantId,
                    FullName = @FullName,
                    Relationship = @Relationship,
                    PhoneNumber = @PhoneNumber
                WHERE DependentId = @DependentId";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DependentId", dependentId);
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Relationship", string.IsNullOrWhiteSpace(relationship) ? (object)DBNull.Value : relationship);
                    cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrWhiteSpace(phoneNumber) ? (object)DBNull.Value : phoneNumber);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteDependentAsync(int dependentId)
        {
            const string sql = @"DELETE FROM Dependents WHERE DependentId = @DependentId";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DependentId", dependentId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        #region CRUD TenantRoomHistory

        public async Task<int> AddTenantHistoryAsync(int tenantId, int roomId, DateTime checkInDate, DateTime? checkOutDate, string status, string notes)
        {
            const string sql = @"
                INSERT INTO TenantRoomHistory (TenantId, RoomId, CheckInDate, CheckOutDate, Status, Notes, CreatedDate)
                VALUES (@TenantId, @RoomId, @CheckInDate, @CheckOutDate, @Status, @Notes, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@CheckInDate", checkInDate.Date);
                    cmd.Parameters.AddWithValue("@CheckOutDate", checkOutDate.HasValue ? (object)checkOutDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateTenantHistoryAsync(int historyId, int roomId, DateTime checkInDate, DateTime? checkOutDate, string status, string notes)
        {
            const string sql = @"
                UPDATE TenantRoomHistory SET
                    RoomId = @RoomId,
                    CheckInDate = @CheckInDate,
                    CheckOutDate = @CheckOutDate,
                    Status = @Status,
                    Notes = @Notes
                WHERE HistoryId = @HistoryId";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@HistoryId", historyId);
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@CheckInDate", checkInDate.Date);
                    cmd.Parameters.AddWithValue("@CheckOutDate", checkOutDate.HasValue ? (object)checkOutDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteTenantHistoryAsync(int historyId)
        {
            const string sql = @"DELETE FROM TenantRoomHistory WHERE HistoryId = @HistoryId";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@HistoryId", historyId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        public Task<DataTable> GetContractsAsync()
        {
            return GetTableSafeAsync(
                "Contracts",
                null,
                @"SELECT c.ContractId,
                         c.ContractNumber,
                         c.TenantId,
                         c.RoomId,
                         r.BranchId,
                         c.SignDate,
                         c.StartDate,
                         c.EndDate,
                         c.RentalPrice,
                         c.DepositRequired,
                         c.Terms,
                         c.ContractPdfPath,
                         c.Status,
                         c.CreatedDate,
                         c.UpdatedDate
                  FROM Contracts c
                  LEFT JOIN Rooms r ON r.RoomId = c.RoomId",
                "SELECT * FROM Contracts"
            );
        }

        public Task<DataTable> GetDepositsAsync()
        {
            return GetTableSafeAsync(
                "Deposits",
                null,
                @"SELECT d.DepositId,
                         d.TenantId,
                         d.RoomId,
                         r.BranchId,
                         d.DepositAmount,
                         d.DepositDate,
                         d.DepositType,
                         d.Status,
                         d.ReturnedAmount,
                         d.ReturnedDate,
                         d.Notes,
                         d.CreatedDate
                  FROM Deposits d
                  LEFT JOIN Rooms r ON r.RoomId = d.RoomId",
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
                         r.RoomNumber,
                         r.BranchId,
                         ur.UtilityTypeId,
                         ut.UtilityName,
                         ut.UtilityCode,
                         ur.ReadingDate,
                         ur.PreviousReading,
                         ur.CurrentReading,
                         ur.UsageAmount,
                         ur.UnitPrice,
                         ur.TotalCost,
                         ur.Notes,
                         ur.CreatedDate
                  FROM UtilityReadings ur
                  LEFT JOIN Rooms r ON r.RoomId = ur.RoomId
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

        public Task<DataTable> GetUtilityReadingsAsync()
        {
            return GetTableSafeAsync(
                "UtilityReadings",
                null,
                @"SELECT ReadingId,
                         RoomId,
                         UtilityTypeId,
                         ReadingDate,
                         PreviousReading,
                         CurrentReading,
                         UsageAmount,
                         UnitPrice,
                         TotalCost,
                         Notes,
                         CreatedDate
                  FROM UtilityReadings
                  ORDER BY ReadingDate DESC",
                "SELECT * FROM UtilityReadings ORDER BY ReadingDate DESC"
            );
        }

        #region CRUD Utilities

        public async Task<int> AddUtilityTypeAsync(string utilityName, string utilityCode, string unit, bool isRecurring, decimal? defaultPrice, string description, bool isActive)
        {
            const string sql = @"
                INSERT INTO UtilityTypes (UtilityName, UtilityCode, Unit, IsRecurring, DefaultPrice, Description, IsActive, CreatedDate)
                VALUES (@UtilityName, @UtilityCode, @Unit, @IsRecurring, @DefaultPrice, @Description, @IsActive, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UtilityName", utilityName);
                    cmd.Parameters.AddWithValue("@UtilityCode", string.IsNullOrWhiteSpace(utilityCode) ? (object)DBNull.Value : utilityCode);
                    cmd.Parameters.AddWithValue("@Unit", string.IsNullOrWhiteSpace(unit) ? (object)DBNull.Value : unit);
                    cmd.Parameters.AddWithValue("@IsRecurring", isRecurring);
                    cmd.Parameters.AddWithValue("@DefaultPrice", defaultPrice.HasValue ? (object)defaultPrice.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateUtilityTypeAsync(int utilityTypeId, string utilityName, string utilityCode, string unit, bool isRecurring, decimal? defaultPrice, string description, bool isActive)
        {
            const string sql = @"
                UPDATE UtilityTypes SET
                    UtilityName = @UtilityName,
                    UtilityCode = @UtilityCode,
                    Unit = @Unit,
                    IsRecurring = @IsRecurring,
                    DefaultPrice = @DefaultPrice,
                    Description = @Description,
                    IsActive = @IsActive
                WHERE UtilityTypeId = @UtilityTypeId";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UtilityTypeId", utilityTypeId);
                    cmd.Parameters.AddWithValue("@UtilityName", utilityName);
                    cmd.Parameters.AddWithValue("@UtilityCode", string.IsNullOrWhiteSpace(utilityCode) ? (object)DBNull.Value : utilityCode);
                    cmd.Parameters.AddWithValue("@Unit", string.IsNullOrWhiteSpace(unit) ? (object)DBNull.Value : unit);
                    cmd.Parameters.AddWithValue("@IsRecurring", isRecurring);
                    cmd.Parameters.AddWithValue("@DefaultPrice", defaultPrice.HasValue ? (object)defaultPrice.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteUtilityTypeAsync(int utilityTypeId)
        {
            const string sql = @"DELETE FROM UtilityTypes WHERE UtilityTypeId = @UtilityTypeId";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UtilityTypeId", utilityTypeId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<int> AddUtilityReadingAsync(int roomId, int utilityTypeId, DateTime? readingDate, decimal? previousReading, decimal? currentReading, decimal? usageAmount, decimal? unitPrice, decimal? totalCost, string notes)
        {
            const string sql = @"
                INSERT INTO UtilityReadings (RoomId, UtilityTypeId, ReadingDate, PreviousReading, CurrentReading, UsageAmount, UnitPrice, TotalCost, Notes, CreatedDate)
                VALUES (@RoomId, @UtilityTypeId, @ReadingDate, @PreviousReading, @CurrentReading, @UsageAmount, @UnitPrice, @TotalCost, @Notes, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@UtilityTypeId", utilityTypeId);
                    cmd.Parameters.AddWithValue("@ReadingDate", readingDate.HasValue ? (object)readingDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@PreviousReading", previousReading.HasValue ? (object)previousReading.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CurrentReading", currentReading.HasValue ? (object)currentReading.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UsageAmount", usageAmount.HasValue ? (object)usageAmount.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitPrice", unitPrice.HasValue ? (object)unitPrice.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TotalCost", totalCost.HasValue ? (object)totalCost.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateUtilityReadingAsync(int readingId, int roomId, int utilityTypeId, DateTime? readingDate, decimal? previousReading, decimal? currentReading, decimal? usageAmount, decimal? unitPrice, decimal? totalCost, string notes)
        {
            const string sql = @"
                UPDATE UtilityReadings SET
                    RoomId = @RoomId,
                    UtilityTypeId = @UtilityTypeId,
                    ReadingDate = @ReadingDate,
                    PreviousReading = @PreviousReading,
                    CurrentReading = @CurrentReading,
                    UsageAmount = @UsageAmount,
                    UnitPrice = @UnitPrice,
                    TotalCost = @TotalCost,
                    Notes = @Notes
                WHERE ReadingId = @ReadingId";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ReadingId", readingId);
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@UtilityTypeId", utilityTypeId);
                    cmd.Parameters.AddWithValue("@ReadingDate", readingDate.HasValue ? (object)readingDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@PreviousReading", previousReading.HasValue ? (object)previousReading.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CurrentReading", currentReading.HasValue ? (object)currentReading.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UsageAmount", usageAmount.HasValue ? (object)usageAmount.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitPrice", unitPrice.HasValue ? (object)unitPrice.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TotalCost", totalCost.HasValue ? (object)totalCost.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteUtilityReadingAsync(int readingId)
        {
            const string sql = @"DELETE FROM UtilityReadings WHERE ReadingId = @ReadingId";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ReadingId", readingId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

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
                         TaxRate,
                         TaxAmount,
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
                @"SELECT t.TicketId,
                         t.TicketNumber,
                         t.RoomId,
                         r.RoomNumber,
                         r.BranchId,
                         RequestorType,
                         RequestorId,
                         IssueDescription,
                         Priority,
                         AssignedToUserId,
                         Status,
                         CreatedDate,
                         CompletedDate,
                         Notes
                  FROM MaintenanceTickets t
                  LEFT JOIN Rooms r ON r.RoomId = t.RoomId",
                "SELECT * FROM MaintenanceTickets"
            );
        }

        #region CRUD MaintenanceTickets

        public async Task<int> AddMaintenanceTicketAsync(string ticketNumber, int roomId, string requestorType, int? requestorId,
            string issueDescription, string priority, int? assignedToUserId, string status, DateTime? completedDate, string notes)
        {
            const string sql = @"
                INSERT INTO MaintenanceTickets
                    (TicketNumber, RoomId, RequestorType, RequestorId, IssueDescription, Priority, AssignedToUserId, Status, CreatedDate, CompletedDate, Notes)
                VALUES
                    (@TicketNumber, @RoomId, @RequestorType, @RequestorId, @IssueDescription, @Priority, @AssignedToUserId, @Status, GETDATE(), @CompletedDate, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TicketNumber", ticketNumber);
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@RequestorType", string.IsNullOrWhiteSpace(requestorType) ? (object)DBNull.Value : requestorType);
                    cmd.Parameters.AddWithValue("@RequestorId", requestorId.HasValue && requestorId.Value > 0 ? (object)requestorId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@IssueDescription", string.IsNullOrWhiteSpace(issueDescription) ? (object)DBNull.Value : issueDescription);
                    cmd.Parameters.AddWithValue("@Priority", string.IsNullOrWhiteSpace(priority) ? (object)DBNull.Value : priority);
                    cmd.Parameters.AddWithValue("@AssignedToUserId", assignedToUserId.HasValue && assignedToUserId.Value > 0 ? (object)assignedToUserId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);
                    cmd.Parameters.AddWithValue("@CompletedDate", completedDate.HasValue ? (object)completedDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateMaintenanceTicketAsync(int ticketId, int roomId, string requestorType, int? requestorId,
            string issueDescription, string priority, int? assignedToUserId, string status, DateTime? completedDate, string notes)
        {
            const string sql = @"
                UPDATE MaintenanceTickets SET
                    RoomId = @RoomId,
                    RequestorType = @RequestorType,
                    RequestorId = @RequestorId,
                    IssueDescription = @IssueDescription,
                    Priority = @Priority,
                    AssignedToUserId = @AssignedToUserId,
                    Status = @Status,
                    CompletedDate = @CompletedDate,
                    Notes = @Notes
                WHERE TicketId = @TicketId";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@RequestorType", string.IsNullOrWhiteSpace(requestorType) ? (object)DBNull.Value : requestorType);
                    cmd.Parameters.AddWithValue("@RequestorId", requestorId.HasValue && requestorId.Value > 0 ? (object)requestorId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@IssueDescription", string.IsNullOrWhiteSpace(issueDescription) ? (object)DBNull.Value : issueDescription);
                    cmd.Parameters.AddWithValue("@Priority", string.IsNullOrWhiteSpace(priority) ? (object)DBNull.Value : priority);
                    cmd.Parameters.AddWithValue("@AssignedToUserId", assignedToUserId.HasValue && assignedToUserId.Value > 0 ? (object)assignedToUserId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);
                    cmd.Parameters.AddWithValue("@CompletedDate", completedDate.HasValue ? (object)completedDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteMaintenanceTicketAsync(int ticketId)
        {
            const string sql = @"DELETE FROM MaintenanceTickets WHERE TicketId = @TicketId";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        public Task<DataTable> GetAssetsAsync()
        {
            return GetTableSafeAsync(
                "Assets",
                null,
                @"SELECT a.AssetId,
                         a.AssetCode,
                         a.AssetName,
                         a.Category,
                         a.RoomId,
                         r.RoomNumber,
                         r.BranchId,
                         a.Quantity,
                         a.Condition,
                         a.PurchaseDate,
                         a.PurchasePrice,
                         a.Description,
                         a.IsActive,
                         a.CreatedDate,
                         a.UpdatedDate
                  FROM Assets a
                  LEFT JOIN Rooms r ON r.RoomId = a.RoomId",
                "SELECT * FROM Assets"
            );
        }

        #region CRUD Assets

        public async Task<int> AddAssetAsync(string assetCode, string assetName, string category, int? roomId, int quantity, string condition,
            DateTime? purchaseDate, decimal? purchasePrice, string description, bool isActive)
        {
            const string sql = @"
                INSERT INTO Assets (AssetCode, AssetName, Category, RoomId, Quantity, Condition, PurchaseDate, PurchasePrice, Description, IsActive, CreatedDate, UpdatedDate)
                VALUES (@AssetCode, @AssetName, @Category, @RoomId, @Quantity, @Condition, @PurchaseDate, @PurchasePrice, @Description, @IsActive, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AssetCode", assetCode);
                    cmd.Parameters.AddWithValue("@AssetName", assetName);
                    cmd.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(category) ? (object)DBNull.Value : category);
                    cmd.Parameters.AddWithValue("@RoomId", roomId.HasValue && roomId.Value > 0 ? (object)roomId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@Condition", string.IsNullOrWhiteSpace(condition) ? (object)DBNull.Value : condition);
                    cmd.Parameters.AddWithValue("@PurchaseDate", purchaseDate.HasValue ? (object)purchaseDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@PurchasePrice", purchasePrice.HasValue ? (object)purchasePrice.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateAssetAsync(int assetId, string assetCode, string assetName, string category, int? roomId, int quantity, string condition,
            DateTime? purchaseDate, decimal? purchasePrice, string description, bool isActive)
        {
            const string sql = @"
                UPDATE Assets SET
                    AssetCode = @AssetCode,
                    AssetName = @AssetName,
                    Category = @Category,
                    RoomId = @RoomId,
                    Quantity = @Quantity,
                    Condition = @Condition,
                    PurchaseDate = @PurchaseDate,
                    PurchasePrice = @PurchasePrice,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdatedDate = GETDATE()
                WHERE AssetId = @AssetId";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AssetId", assetId);
                    cmd.Parameters.AddWithValue("@AssetCode", assetCode);
                    cmd.Parameters.AddWithValue("@AssetName", assetName);
                    cmd.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(category) ? (object)DBNull.Value : category);
                    cmd.Parameters.AddWithValue("@RoomId", roomId.HasValue && roomId.Value > 0 ? (object)roomId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@Condition", string.IsNullOrWhiteSpace(condition) ? (object)DBNull.Value : condition);
                    cmd.Parameters.AddWithValue("@PurchaseDate", purchaseDate.HasValue ? (object)purchaseDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@PurchasePrice", purchasePrice.HasValue ? (object)purchasePrice.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteAssetAsync(int assetId)
        {
            const string sql = @"DELETE FROM Assets WHERE AssetId = @AssetId";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AssetId", assetId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

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

        #region CRUD Notifications

        public async Task<int> AddNotificationAsync(int? userId, string title, string message, string status)
        {
            const string sql = @"
                INSERT INTO Notifications (UserId, Title, Message, Status, CreatedDate)
                VALUES (@UserId, @Title, @Message, @Status, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId.HasValue && userId.Value > 0 ? (object)userId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Message", string.IsNullOrWhiteSpace(message) ? (object)DBNull.Value : message);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateNotificationAsync(int notificationId, int? userId, string title, string message, string status)
        {
            const string sql = @"
                UPDATE Notifications SET
                    UserId = @UserId,
                    Title = @Title,
                    Message = @Message,
                    Status = @Status
                WHERE NotificationId = @NotificationId";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@NotificationId", notificationId);
                    cmd.Parameters.AddWithValue("@UserId", userId.HasValue && userId.Value > 0 ? (object)userId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Message", string.IsNullOrWhiteSpace(message) ? (object)DBNull.Value : message);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            const string sql = @"DELETE FROM Notifications WHERE NotificationId = @NotificationId";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@NotificationId", notificationId);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        public Task<DataTable> GetSystemSettingsAsync()
        {
            return GetTableSafeAsync(
                "SystemSettings",
                null,
                @"SELECT SettingKey, SettingValue, Description FROM SystemSettings",
                "SELECT * FROM SystemSettings"
            );
        }

        #region CRUD SystemSettings

        public async Task<bool> AddSystemSettingAsync(string key, string value, string description)
        {
            const string sql = @"
                INSERT INTO SystemSettings (SettingKey, SettingValue, Description)
                VALUES (@SettingKey, @SettingValue, @Description)";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SettingKey", key);
                    cmd.Parameters.AddWithValue("@SettingValue", string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> UpdateSystemSettingAsync(string key, string value, string description)
        {
            const string sql = @"
                UPDATE SystemSettings SET
                    SettingValue = @SettingValue,
                    Description = @Description
                WHERE SettingKey = @SettingKey";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SettingKey", key);
                    cmd.Parameters.AddWithValue("@SettingValue", string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteSystemSettingAsync(string key)
        {
            const string sql = @"DELETE FROM SystemSettings WHERE SettingKey = @SettingKey";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SettingKey", key);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        #endregion

        #region CRUD Tenants

        public async Task<int> AddTenantAsync(string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry, bool isActive,
            string frontIdPhoto = null, string backIdPhoto = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "Tenants";
                bool hasFullName = await ColumnExistsAsync(conn, tbl, "FullName");
                bool hasIdentity = await ColumnExistsAsync(conn, tbl, "IdentityCard");
                bool hasPhone = await ColumnExistsAsync(conn, tbl, "PhoneNumber");
                bool hasEmail = await ColumnExistsAsync(conn, tbl, "Email");
                bool hasBirth = await ColumnExistsAsync(conn, tbl, "BirthDate");
                bool hasAddress = await ColumnExistsAsync(conn, tbl, "Address");
                bool hasFront = await ColumnExistsAsync(conn, tbl, "FrontIdPhoto");
                bool hasBack = await ColumnExistsAsync(conn, tbl, "BackIdPhoto");
                bool hasTemp = await ColumnExistsAsync(conn, tbl, "TemporaryRegistration");
                bool hasTempDate = await ColumnExistsAsync(conn, tbl, "TemporaryRegistrationDate");
                bool hasTempExp = await ColumnExistsAsync(conn, tbl, "TemporaryRegistrationExpiry");
                bool hasActive = await ColumnExistsAsync(conn, tbl, "IsActive");
                bool hasCreated = await ColumnExistsAsync(conn, tbl, "CreatedDate");
                bool hasUpdated = await ColumnExistsAsync(conn, tbl, "UpdatedDate");

                if (!hasFullName)
                    throw new Exception("Schema Tenants không hợp lệ (thiếu cột FullName).");

                var cols = new System.Collections.Generic.List<string>();
                var vals = new System.Collections.Generic.List<string>();

                if (hasFullName) { cols.Add("FullName"); vals.Add("@FullName"); }
                if (hasIdentity) { cols.Add("IdentityCard"); vals.Add("@IdentityCard"); }
                if (hasPhone) { cols.Add("PhoneNumber"); vals.Add("@PhoneNumber"); }
                if (hasEmail) { cols.Add("Email"); vals.Add("@Email"); }
                if (hasBirth) { cols.Add("BirthDate"); vals.Add("@BirthDate"); }
                if (hasAddress) { cols.Add("Address"); vals.Add("@Address"); }
                if (hasFront) { cols.Add("FrontIdPhoto"); vals.Add("@FrontIdPhoto"); }
                if (hasBack) { cols.Add("BackIdPhoto"); vals.Add("@BackIdPhoto"); }
                if (hasTemp) { cols.Add("TemporaryRegistration"); vals.Add("@TempReg"); }
                if (hasTempDate) { cols.Add("TemporaryRegistrationDate"); vals.Add("@TempRegDate"); }
                if (hasTempExp) { cols.Add("TemporaryRegistrationExpiry"); vals.Add("@TempRegExpiry"); }
                if (hasActive) { cols.Add("IsActive"); vals.Add("@IsActive"); }
                if (hasCreated) { cols.Add("CreatedDate"); vals.Add("GETDATE()"); }
                if (hasUpdated) { cols.Add("UpdatedDate"); vals.Add("GETDATE()"); }

                string sql = $@"INSERT INTO {tbl} ({string.Join(", ", cols)})
                                VALUES ({string.Join(", ", vals)});
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (hasFullName) cmd.Parameters.AddWithValue("@FullName", fullName);
                    if (hasIdentity) cmd.Parameters.AddWithValue("@IdentityCard", string.IsNullOrWhiteSpace(identityCard) ? (object)DBNull.Value : identityCard);
                    if (hasPhone) cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrWhiteSpace(phoneNumber) ? (object)DBNull.Value : phoneNumber);
                    if (hasEmail) cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    if (hasBirth) cmd.Parameters.AddWithValue("@BirthDate", birthDate.HasValue ? (object)birthDate.Value : DBNull.Value);
                    if (hasAddress) cmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(address) ? (object)DBNull.Value : address);
                    if (hasFront) cmd.Parameters.AddWithValue("@FrontIdPhoto", string.IsNullOrWhiteSpace(frontIdPhoto) ? (object)DBNull.Value : frontIdPhoto);
                    if (hasBack) cmd.Parameters.AddWithValue("@BackIdPhoto", string.IsNullOrWhiteSpace(backIdPhoto) ? (object)DBNull.Value : backIdPhoto);
                    if (hasTemp) cmd.Parameters.AddWithValue("@TempReg", string.IsNullOrWhiteSpace(tempReg) ? (object)DBNull.Value : tempReg);
                    if (hasTempDate) cmd.Parameters.AddWithValue("@TempRegDate", tempRegDate.HasValue ? (object)tempRegDate.Value : DBNull.Value);
                    if (hasTempExp) cmd.Parameters.AddWithValue("@TempRegExpiry", tempRegExpiry.HasValue ? (object)tempRegExpiry.Value : DBNull.Value);
                    if (hasActive) cmd.Parameters.AddWithValue("@IsActive", isActive);

                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateTenantAsync(int tenantId, string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry, bool isActive,
            string frontIdPhoto = null, string backIdPhoto = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string tbl = "Tenants";
                bool hasFullName = await ColumnExistsAsync(conn, tbl, "FullName");
                bool hasIdentity = await ColumnExistsAsync(conn, tbl, "IdentityCard");
                bool hasPhone = await ColumnExistsAsync(conn, tbl, "PhoneNumber");
                bool hasEmail = await ColumnExistsAsync(conn, tbl, "Email");
                bool hasBirth = await ColumnExistsAsync(conn, tbl, "BirthDate");
                bool hasAddress = await ColumnExistsAsync(conn, tbl, "Address");
                bool hasFront = await ColumnExistsAsync(conn, tbl, "FrontIdPhoto");
                bool hasBack = await ColumnExistsAsync(conn, tbl, "BackIdPhoto");
                bool hasTemp = await ColumnExistsAsync(conn, tbl, "TemporaryRegistration");
                bool hasTempDate = await ColumnExistsAsync(conn, tbl, "TemporaryRegistrationDate");
                bool hasTempExp = await ColumnExistsAsync(conn, tbl, "TemporaryRegistrationExpiry");
                bool hasActive = await ColumnExistsAsync(conn, tbl, "IsActive");
                bool hasUpdated = await ColumnExistsAsync(conn, tbl, "UpdatedDate");

                if (!hasFullName)
                    throw new Exception("Schema Tenants không hợp lệ (thiếu cột FullName).");

                var sets = new System.Collections.Generic.List<string>();
                if (hasFullName) sets.Add("FullName = @FullName");
                if (hasIdentity) sets.Add("IdentityCard = @IdentityCard");
                if (hasPhone) sets.Add("PhoneNumber = @PhoneNumber");
                if (hasEmail) sets.Add("Email = @Email");
                if (hasBirth) sets.Add("BirthDate = @BirthDate");
                if (hasAddress) sets.Add("Address = @Address");
                if (hasFront && frontIdPhoto != null) sets.Add("FrontIdPhoto = @FrontIdPhoto");
                if (hasBack && backIdPhoto != null) sets.Add("BackIdPhoto = @BackIdPhoto");
                if (hasTemp) sets.Add("TemporaryRegistration = @TempReg");
                if (hasTempDate) sets.Add("TemporaryRegistrationDate = @TempRegDate");
                if (hasTempExp) sets.Add("TemporaryRegistrationExpiry = @TempRegExpiry");
                if (hasActive) sets.Add("IsActive = @IsActive");
                if (hasUpdated) sets.Add("UpdatedDate = GETDATE()");

                string sql = $@"UPDATE {tbl} SET {string.Join(", ", sets)} WHERE TenantId = @TenantId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    if (hasFullName) cmd.Parameters.AddWithValue("@FullName", fullName);
                    if (hasIdentity) cmd.Parameters.AddWithValue("@IdentityCard", string.IsNullOrWhiteSpace(identityCard) ? (object)DBNull.Value : identityCard);
                    if (hasPhone) cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrWhiteSpace(phoneNumber) ? (object)DBNull.Value : phoneNumber);
                    if (hasEmail) cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    if (hasBirth) cmd.Parameters.AddWithValue("@BirthDate", birthDate.HasValue ? (object)birthDate.Value : DBNull.Value);
                    if (hasAddress) cmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(address) ? (object)DBNull.Value : address);
                    if (hasFront && frontIdPhoto != null) cmd.Parameters.AddWithValue("@FrontIdPhoto", string.IsNullOrWhiteSpace(frontIdPhoto) ? (object)DBNull.Value : frontIdPhoto);
                    if (hasBack && backIdPhoto != null) cmd.Parameters.AddWithValue("@BackIdPhoto", string.IsNullOrWhiteSpace(backIdPhoto) ? (object)DBNull.Value : backIdPhoto);
                    if (hasTemp) cmd.Parameters.AddWithValue("@TempReg", string.IsNullOrWhiteSpace(tempReg) ? (object)DBNull.Value : tempReg);
                    if (hasTempDate) cmd.Parameters.AddWithValue("@TempRegDate", tempRegDate.HasValue ? (object)tempRegDate.Value : DBNull.Value);
                    if (hasTempExp) cmd.Parameters.AddWithValue("@TempRegExpiry", tempRegExpiry.HasValue ? (object)tempRegExpiry.Value : DBNull.Value);
                    if (hasActive) cmd.Parameters.AddWithValue("@IsActive", isActive);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteTenantAsync(int tenantId)
        {
            if (!await TableExistsAsync("Tenants"))
                throw new Exception("Bảng Tenants không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // Payments -> Invoices
                        if (await TableExistsAsync("Payments") && await TableExistsAsync("Invoices"))
                        {
                            const string sqlPayments = @"
                                DELETE FROM Payments
                                WHERE InvoiceId IN (SELECT InvoiceId FROM Invoices WHERE TenantId = @TenantId);";
                            using (var cmd = new SqlCommand(sqlPayments, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("Invoices"))
                        {
                            using (var cmd = new SqlCommand("DELETE FROM Invoices WHERE TenantId = @TenantId", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("Contracts"))
                        {
                            using (var cmd = new SqlCommand("DELETE FROM Contracts WHERE TenantId = @TenantId", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("Deposits"))
                        {
                            using (var cmd = new SqlCommand("DELETE FROM Deposits WHERE TenantId = @TenantId", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("TenantRoomHistory"))
                        {
                            using (var cmd = new SqlCommand("DELETE FROM TenantRoomHistory WHERE TenantId = @TenantId", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        if (await TableExistsAsync("Dependents"))
                        {
                            using (var cmd = new SqlCommand("DELETE FROM Dependents WHERE TenantId = @TenantId", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        int affected;
                        using (var cmd = new SqlCommand("DELETE FROM Tenants WHERE TenantId = @TenantId", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@TenantId", tenantId);
                            affected = await cmd.ExecuteNonQueryAsync();
                        }

                        tx.Commit();
                        return affected > 0;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        #endregion

        #region CRUD Contracts

        public async Task<int> AddContractAsync(
            string contractNumber,
            int tenantId,
            int roomId,
            DateTime? signDate,
            DateTime startDate,
            DateTime endDate,
            decimal? rentalPrice,
            decimal? depositRequired,
            string terms,
            string contractPdfPath,
            string status)
        {
            const string sql = @"
                INSERT INTO Contracts
                    (ContractNumber, TenantId, RoomId, SignDate, StartDate, EndDate, RentalPrice, DepositRequired, Terms, ContractPdfPath, Status, CreatedDate, UpdatedDate)
                VALUES
                    (@ContractNumber, @TenantId, @RoomId, @SignDate, @StartDate, @EndDate, @RentalPrice, @DepositRequired, @Terms, @ContractPdfPath, @Status, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ContractNumber", contractNumber);
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@SignDate", signDate.HasValue ? (object)signDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@RentalPrice", rentalPrice.HasValue ? (object)rentalPrice.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepositRequired", depositRequired.HasValue ? (object)depositRequired.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Terms", string.IsNullOrWhiteSpace(terms) ? (object)DBNull.Value : terms);
                    cmd.Parameters.AddWithValue("@ContractPdfPath", string.IsNullOrWhiteSpace(contractPdfPath) ? (object)DBNull.Value : contractPdfPath);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);

                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public async Task<bool> UpdateContractAsync(
            int contractId,
            string contractNumber,
            int tenantId,
            int roomId,
            DateTime? signDate,
            DateTime startDate,
            DateTime endDate,
            decimal? rentalPrice,
            decimal? depositRequired,
            string terms,
            string contractPdfPath,
            string status)
        {
            const string sql = @"
                UPDATE Contracts SET
                    ContractNumber = @ContractNumber,
                    TenantId = @TenantId,
                    RoomId = @RoomId,
                    SignDate = @SignDate,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    RentalPrice = @RentalPrice,
                    DepositRequired = @DepositRequired,
                    Terms = @Terms,
                    ContractPdfPath = @ContractPdfPath,
                    Status = @Status,
                    UpdatedDate = GETDATE()
                WHERE ContractId = @ContractId";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ContractId", contractId);
                    cmd.Parameters.AddWithValue("@ContractNumber", contractNumber);
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@SignDate", signDate.HasValue ? (object)signDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@RentalPrice", rentalPrice.HasValue ? (object)rentalPrice.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepositRequired", depositRequired.HasValue ? (object)depositRequired.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Terms", string.IsNullOrWhiteSpace(terms) ? (object)DBNull.Value : terms);
                    cmd.Parameters.AddWithValue("@ContractPdfPath", string.IsNullOrWhiteSpace(contractPdfPath) ? (object)DBNull.Value : contractPdfPath);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> DeleteContractAsync(int contractId)
        {
            const string sql = @"DELETE FROM Contracts WHERE ContractId = @ContractId";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ContractId", contractId);
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
