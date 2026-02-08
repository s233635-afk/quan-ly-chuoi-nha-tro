using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace QuanLyNhaTro.DAL
{
    public partial class DatabaseHelper
    {
        #region Room Management - Branch Filter

        public async Task<DataTable> GetRoomsByBranchAsync(int? branchId = null)
        {
            var query = "SELECT * FROM Rooms WHERE IsActive = 1";
            if (branchId.HasValue)
                query += $" AND BranchId = {branchId.Value}";
            query += " ORDER BY RoomNumber";
            return await ExecuteQueryAsync(query);
        }

        public async Task<DataTable> GetAvailableRoomsAsync(int? branchId = null)
        {
            var query = "SELECT * FROM Rooms WHERE IsActive = 1 AND CurrentStatusId = 1";
            if (branchId.HasValue)
                query += $" AND BranchId = {branchId.Value}";
            return await ExecuteQueryAsync(query);
        }

        public async Task<int> UpdateRoomStatusAsync(int roomId, int statusId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("UPDATE Rooms SET CurrentStatusId = @StatusId, UpdatedDate = GETDATE() WHERE RoomId = @RoomId", conn))
                {
                    cmd.Parameters.AddWithValue("@RoomId", roomId);
                    cmd.Parameters.AddWithValue("@StatusId", statusId);
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public Task<int> UpdateRoomOccupancyStatusAsync(int roomId, int statusId)
        {
            return UpdateRoomStatusAsync(roomId, statusId);
        }

        #endregion

        #region Tenant Management - Branch Filter

        public async Task<DataTable> GetTenantsByBranchAsync(int? branchId = null)
        {
            var query = @"
                SELECT DISTINCT t.* FROM Tenants t
                INNER JOIN TenantRoomHistory trh ON t.TenantId = trh.TenantId
                INNER JOIN Rooms r ON trh.RoomId = r.RoomId
                WHERE t.IsActive = 1";
            
            if (branchId.HasValue)
                query += $" AND r.BranchId = {branchId.Value}";
            
            query += " ORDER BY t.FullName";
            return await ExecuteQueryAsync(query);
        }

        public async Task<DataTable> GetTenantByIdAsync(int tenantId)
        {
            return await ExecuteQueryAsync($"SELECT * FROM Tenants WHERE TenantId = {tenantId}");
        }

        #endregion

        #region Contract Management - Branch Filter

        public async Task<DataTable> GetContractsByBranchAsync(int? branchId = null)
        {
            var query = @"
                SELECT c.* FROM Contracts c
                INNER JOIN Rooms r ON c.RoomId = r.RoomId
                WHERE 1=1";
            
            if (branchId.HasValue)
                query += $" AND r.BranchId = {branchId.Value}";
            
            query += " ORDER BY c.ContractNumber DESC";
            return await ExecuteQueryAsync(query);
        }

        #endregion

        #region Deposit Management - Branch Filter

        public async Task<DataTable> GetDepositsByBranchAsync(int? branchId = null)
        {
            var query = @"
                SELECT d.* FROM Deposits d
                INNER JOIN Rooms r ON d.RoomId = r.RoomId
                WHERE 1=1";
            
            if (branchId.HasValue)
                query += $" AND r.BranchId = {branchId.Value}";
            
            query += " ORDER BY d.DepositDate DESC";
            return await ExecuteQueryAsync(query);
        }

        #endregion

        #region Invoice Management - Branch Filter

        public async Task<DataTable> GetInvoicesByBranchAsync(int? branchId = null)
        {
            var query = @"
                SELECT i.* FROM Invoices i
                INNER JOIN Rooms r ON i.RoomId = r.RoomId
                WHERE 1=1";
            
            if (branchId.HasValue)
                query += $" AND r.BranchId = {branchId.Value}";
            
            query += " ORDER BY i.InvoiceDate DESC";
            return await ExecuteQueryAsync(query);
        }

        public async Task<DataTable> GetInvoiceByIdAsync(int invoiceId)
        {
            return await ExecuteQueryAsync($"SELECT * FROM Invoices WHERE InvoiceId = {invoiceId}");
        }

        public async Task<DataTable> GetOutstandingInvoicesAsync(int? branchId = null)
        {
            var query = @"
                SELECT i.* FROM Invoices i
                INNER JOIN Rooms r ON i.RoomId = r.RoomId
                WHERE i.Status IN ('Issued', 'PartialPaid', 'Overdue')";
            
            if (branchId.HasValue)
                query += $" AND r.BranchId = {branchId.Value}";
            
            query += " ORDER BY i.DueDate ASC";
            return await ExecuteQueryAsync(query);
        }

        #endregion

        #region Payment Management - Branch Filter

        public async Task<DataTable> GetPaymentsByBranchAsync(int? branchId = null)
        {
            var query = @"
                SELECT p.* FROM Payments p
                INNER JOIN Invoices i ON p.InvoiceId = i.InvoiceId
                INNER JOIN Rooms r ON i.RoomId = r.RoomId
                WHERE 1=1";
            
            if (branchId.HasValue)
                query += $" AND r.BranchId = {branchId.Value}";
            
            query += " ORDER BY p.PaymentDate DESC";
            return await ExecuteQueryAsync(query);
        }

        #endregion

        #region Utility Management

        public async Task<DataTable> GetUtilityReadingsByRoomAsync(int roomId)
        {
            return await ExecuteQueryAsync($@"
                SELECT ur.*, ut.UtilityName, ut.Unit 
                FROM UtilityReadings ur
                INNER JOIN UtilityTypes ut ON ur.UtilityTypeId = ut.UtilityTypeId
                WHERE ur.RoomId = {roomId}
                ORDER BY ur.ReadingDate DESC");
        }

        #endregion

        #region Maintenance Management - Branch Filter

        public async Task<DataTable> GetMaintenanceByBranchAsync(int? branchId = null)
        {
            var query = @"
                SELECT mt.* FROM MaintenanceTickets mt
                INNER JOIN Rooms r ON mt.RoomId = r.RoomId
                WHERE 1=1";
            
            if (branchId.HasValue)
                query += $" AND r.BranchId = {branchId.Value}";
            
            query += " ORDER BY mt.CreatedDate DESC";
            return await ExecuteQueryAsync(query);
        }

        #endregion

        #region Asset Management - Branch Filter

        public async Task<DataTable> GetAssetsByBranchAsync(int? branchId = null)
        {
            var query = @"
                SELECT a.* FROM Assets a
                WHERE a.IsActive = 1";
            
            if (branchId.HasValue)
                query += $" AND a.RoomId IN (SELECT RoomId FROM Rooms WHERE BranchId = {branchId.Value})";
            
            query += " ORDER BY a.AssetCode";
            return await ExecuteQueryAsync(query);
        }

        #endregion

        #region Room Occupancy & Revenue Reports

        public async Task<DataTable> GetRoomOccupancyAsync(int? branchId = null)
        {
            var query = @"
                SELECT 
                    b.BranchName,
                    COUNT(DISTINCT r.RoomId) as TotalRooms,
                    SUM(CASE WHEN rs.StatusName = N'Đang ở' THEN 1 ELSE 0 END) as OccupiedRooms,
                    SUM(CASE WHEN rs.StatusName = N'Trống' THEN 1 ELSE 0 END) as AvailableRooms,
                    ROUND(
                        CAST(SUM(CASE WHEN rs.StatusName = N'Đang ở' THEN 1 ELSE 0 END) AS FLOAT) /
                        COUNT(DISTINCT r.RoomId) * 100,
                        2
                    ) as OccupancyRate
                FROM Rooms r
                INNER JOIN Branches b ON r.BranchId = b.BranchId
                INNER JOIN RoomStatuses rs ON r.CurrentStatusId = rs.StatusId
                WHERE b.IsActive = 1 AND r.IsActive = 1";
            
            if (branchId.HasValue)
                query += $" AND b.BranchId = {branchId.Value}";
            
            query += @" GROUP BY b.BranchId, b.BranchName
                      ORDER BY b.BranchName";
            return await ExecuteQueryAsync(query);
        }

        public async Task<DataTable> GetRevenueReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = @"
                SELECT 
                    MONTH(i.InvoiceDate) as Month,
                    YEAR(i.InvoiceDate) as Year,
                    SUM(i.RentalCost) as TotalRental,
                    SUM(i.UtilityCost) as TotalUtility,
                    SUM(i.OtherCost) as TotalOther,
                    SUM(i.TotalAmount) as TotalRevenue,
                    SUM(i.PaidAmount) as TotalPaid,
                    SUM(i.RemainingAmount) as TotalRemaining
                FROM Invoices i
                WHERE 1=1";
            
            if (fromDate.HasValue)
                query += $" AND i.InvoiceDate >= '{fromDate.Value:yyyy-MM-dd}'";
            if (toDate.HasValue)
                query += $" AND i.InvoiceDate <= '{toDate.Value:yyyy-MM-dd}'";
            
            query += @" GROUP BY MONTH(i.InvoiceDate), YEAR(i.InvoiceDate)
                      ORDER BY YEAR(i.InvoiceDate) DESC, MONTH(i.InvoiceDate) DESC";
            return await ExecuteQueryAsync(query);
        }

        #endregion

        #region Helper Method

        private async Task<DataTable> ExecuteQueryAsync(string query)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(connectionString))
            {
                using (var adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.CommandTimeout = commandTimeoutSeconds;
                    await Task.Run(() => adapter.Fill(dt));
                }
            }
            return dt;
        }

        #endregion

        #region Tenant Room History Wrappers

        public Task<int> AddTenantRoomHistoryAsync(int tenantId, int roomId, DateTime checkInDate,
            DateTime? checkOutDate, string status, string notes)
            => AddTenantHistoryAsync(tenantId, roomId, checkInDate, checkOutDate, status, notes);

        public Task<bool> UpdateTenantRoomHistoryAsync(int historyId, int roomId, DateTime checkInDate,
            DateTime? checkOutDate, string status, string notes)
            => UpdateTenantHistoryAsync(historyId, roomId, checkInDate, checkOutDate, status, notes);

        public Task<bool> DeleteTenantRoomHistoryAsync(int historyId)
            => DeleteTenantHistoryAsync(historyId);

        #endregion
    }
}
