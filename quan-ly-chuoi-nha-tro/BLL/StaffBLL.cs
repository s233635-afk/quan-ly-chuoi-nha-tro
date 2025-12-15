using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL
{
    /// <summary>
    /// Business Logic Layer cho Nhân Viên
    /// Xử lý tất cả logic liên quan đến công việc của Nhân Viên
    /// </summary>
    public class StaffBLL
    {
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();

        #region Room Management (Quản Lý Phòng)
        
        public Task<DataTable> GetRoomsByBranchAsync(int? branchId = null)
            => _dbHelper.GetRoomsByBranchAsync(branchId);

        public Task<DataTable> GetRoomStatusesAsync()
            => _dbHelper.GetRoomStatusesAsync();

        public Task<DataTable> GetRoomTypesAsync()
            => _dbHelper.GetRoomTypesAsync();

        public Task<int> UpdateRoomStatusAsync(int roomId, int statusId)
            => _dbHelper.UpdateRoomStatusAsync(roomId, statusId);

        public Task<DataTable> GetAvailableRoomsAsync(int? branchId = null)
            => _dbHelper.GetAvailableRoomsAsync(branchId);

        #endregion

        #region Tenant Management (Quản Lý Khách Thuê)

        public Task<DataTable> GetTenantsAsync()
            => _dbHelper.GetTenantsAsync();

        public Task<DataTable> GetTenantsByBranchAsync(int? branchId = null)
            => _dbHelper.GetTenantsByBranchAsync(branchId);

        public Task<DataTable> GetTenantByIdAsync(int tenantId)
            => _dbHelper.GetTenantByIdAsync(tenantId);

        public Task<int> AddTenantAsync(string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry)
            => _dbHelper.AddTenantAsync(fullName, identityCard, phoneNumber, email, birthDate, address, tempReg, tempRegDate, tempRegExpiry, true);

        public Task<bool> UpdateTenantAsync(int tenantId, string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry)
            => _dbHelper.UpdateTenantAsync(tenantId, fullName, identityCard, phoneNumber, email, birthDate, address, tempReg, tempRegDate, tempRegExpiry, true);

        public Task<bool> DeleteTenantAsync(int tenantId)
            => _dbHelper.DeleteTenantAsync(tenantId);

        public Task<DataTable> GetDependentsAsync()
            => _dbHelper.GetDependentsAsync();

        public Task<int> AddDependentAsync(int tenantId, string fullName, string relationship, string phoneNumber)
            => _dbHelper.AddDependentAsync(tenantId, fullName, relationship, phoneNumber);

        public Task<bool> UpdateDependentAsync(int dependentId, int tenantId, string fullName, string relationship, string phoneNumber)
            => _dbHelper.UpdateDependentAsync(dependentId, tenantId, fullName, relationship, phoneNumber);

        public Task<bool> DeleteDependentAsync(int dependentId)
            => _dbHelper.DeleteDependentAsync(dependentId);

        #endregion

        #region Contract Management (Quản Lý Hợp Đồng)

        public Task<DataTable> GetContractsAsync()
            => _dbHelper.GetContractsAsync();

        public Task<DataTable> GetContractsByBranchAsync(int? branchId = null)
            => _dbHelper.GetContractsByBranchAsync(branchId);

        public Task<DataTable> GetContractByIdAsync(int contractId)
            => _dbHelper.GetContractByIdAsync(contractId);

        public Task<int> AddContractAsync(string contractNumber, int tenantId, int roomId, DateTime? signDate,
            DateTime startDate, DateTime endDate, decimal? rentalPrice, decimal? depositRequired, string terms, string contractPdfPath)
            => _dbHelper.AddContractAsync(contractNumber, tenantId, roomId, signDate, startDate, endDate, rentalPrice, depositRequired, terms, contractPdfPath);

        public Task<bool> UpdateContractAsync(int contractId, DateTime? signDate, DateTime startDate, DateTime endDate,
            decimal? rentalPrice, decimal? depositRequired, string terms, string contractPdfPath, string status)
            => _dbHelper.UpdateContractAsync(contractId, signDate, startDate, endDate, rentalPrice, depositRequired, terms, contractPdfPath, status);

        public Task<bool> DeleteContractAsync(int contractId)
            => _dbHelper.DeleteContractAsync(contractId);

        #endregion

        #region Deposit Management (Quản Lý Tiền Cọc)

        public Task<DataTable> GetDepositsAsync()
            => _dbHelper.GetDepositsAsync();

        public Task<DataTable> GetDepositsByBranchAsync(int? branchId = null)
            => _dbHelper.GetDepositsByBranchAsync(branchId);

        public Task<int> AddDepositAsync(int tenantId, int roomId, decimal depositAmount, DateTime depositDate,
            string depositType, string status)
            => _dbHelper.AddDepositAsync(tenantId, roomId, depositAmount, depositDate, depositType, status);

        public Task<bool> UpdateDepositAsync(int depositId, decimal? returnedAmount, DateTime? returnedDate, string status, string notes)
            => _dbHelper.UpdateDepositAsync(depositId, returnedAmount, returnedDate, status, notes);

        public Task<bool> DeleteDepositAsync(int depositId)
            => _dbHelper.DeleteDepositAsync(depositId);

        #endregion

        #region Invoice Management (Quản Lý Hóa Đơn)

        public Task<DataTable> GetInvoicesAsync()
            => _dbHelper.GetInvoicesAsync();

        public Task<DataTable> GetInvoicesByBranchAsync(int? branchId = null)
            => _dbHelper.GetInvoicesByBranchAsync(branchId);

        public Task<DataTable> GetInvoiceByIdAsync(int invoiceId)
            => _dbHelper.GetInvoiceByIdAsync(invoiceId);

        public Task<int> AddInvoiceAsync(string invoiceNumber, int tenantId, int roomId, DateTime invoiceDate,
            DateTime? fromDate, DateTime? toDate, decimal rentalCost, decimal utilityCost, decimal otherCost,
            decimal totalAmount, DateTime dueDate, string status)
            => _dbHelper.AddInvoiceAsync(invoiceNumber, tenantId, roomId, invoiceDate, fromDate, toDate, rentalCost, utilityCost, otherCost, totalAmount, dueDate, status);

        public Task<bool> UpdateInvoiceAsync(int invoiceId, DateTime? fromDate, DateTime? toDate, decimal rentalCost,
            decimal utilityCost, decimal otherCost, decimal totalAmount, DateTime dueDate, string status)
            => _dbHelper.UpdateInvoiceAsync(invoiceId, fromDate, toDate, rentalCost, utilityCost, otherCost, totalAmount, dueDate, status);

        public Task<bool> DeleteInvoiceAsync(int invoiceId)
            => _dbHelper.DeleteInvoiceAsync(invoiceId);

        public Task<DataTable> GetOutstandingInvoicesAsync(int? branchId = null)
            => _dbHelper.GetOutstandingInvoicesAsync(branchId);

        #endregion

        #region Payment Management (Quản Lý Thanh Toán)

        public Task<DataTable> GetPaymentsAsync()
            => _dbHelper.GetPaymentsAsync();

        public Task<DataTable> GetPaymentsByInvoiceAsync(int invoiceId)
            => _dbHelper.GetPaymentsByInvoiceAsync(invoiceId);

        public Task<int> AddPaymentAsync(int invoiceId, DateTime paymentDate, decimal paymentAmount,
            string paymentMethod, string transactionReference, string notes)
            => _dbHelper.AddPaymentAsync(invoiceId, paymentDate, paymentAmount, paymentMethod, transactionReference, notes);

        public Task<bool> DeletePaymentAsync(int paymentId)
            => _dbHelper.DeletePaymentAsync(paymentId);

        public Task<DataTable> GetPaymentsByBranchAsync(int? branchId = null)
            => _dbHelper.GetPaymentsByBranchAsync(branchId);

        #endregion

        #region Utility Management (Quản Lý Tiện Ích)

        public Task<DataTable> GetUtilityTypesAsync()
            => _dbHelper.GetUtilityTypesAsync();

        public Task<DataTable> GetUtilityReadingsAsync()
            => _dbHelper.GetUtilityReadingsAsync();

        public Task<DataTable> GetUtilityReadingsByRoomAsync(int roomId)
            => _dbHelper.GetUtilityReadingsByRoomAsync(roomId);

        public Task<int> AddUtilityReadingAsync(int roomId, int utilityTypeId, DateTime readingDate,
            decimal previousReading, decimal currentReading, decimal usageAmount, decimal unitPrice, decimal totalCost, string notes)
            => _dbHelper.AddUtilityReadingAsync(roomId, utilityTypeId, readingDate, previousReading, currentReading, usageAmount, unitPrice, totalCost, notes);

        public Task<bool> UpdateUtilityReadingAsync(int readingId, decimal previousReading, decimal currentReading,
            decimal usageAmount, decimal unitPrice, decimal totalCost, string notes)
            => _dbHelper.UpdateUtilityReadingAsync(readingId, previousReading, currentReading, usageAmount, unitPrice, totalCost, notes);

        public Task<bool> DeleteUtilityReadingAsync(int readingId)
            => _dbHelper.DeleteUtilityReadingAsync(readingId);

        #endregion

        #region Maintenance Management (Quản Lý Bảo Trì)

        public Task<DataTable> GetMaintenanceTicketsAsync()
            => _dbHelper.GetMaintenanceTicketsAsync();

        public Task<DataTable> GetMaintenanceByBranchAsync(int? branchId = null)
            => _dbHelper.GetMaintenanceByBranchAsync(branchId);

        public Task<int> AddMaintenanceTicketAsync(string ticketNumber, int roomId, string requestorType,
            int? requestorId, string issueDescription, string priority, int? assignedToUserId, string status)
            => _dbHelper.AddMaintenanceTicketAsync(ticketNumber, roomId, requestorType, requestorId, issueDescription, priority, assignedToUserId, status);

        public Task<bool> UpdateMaintenanceTicketAsync(int ticketId, string issueDescription, string priority,
            int? assignedToUserId, string status, DateTime? completedDate, string notes)
            => _dbHelper.UpdateMaintenanceTicketAsync(ticketId, issueDescription, priority, assignedToUserId, status, completedDate, notes);

        public Task<bool> DeleteMaintenanceTicketAsync(int ticketId)
            => _dbHelper.DeleteMaintenanceTicketAsync(ticketId);

        #endregion

        #region Asset Management (Quản Lý Tài Sản)

        public Task<DataTable> GetAssetsAsync()
            => _dbHelper.GetAssetsAsync();

        public Task<DataTable> GetAssetsByBranchAsync(int? branchId = null)
            => _dbHelper.GetAssetsByBranchAsync(branchId);

        public Task<int> AddAssetAsync(string assetCode, string assetName, string category, int? roomId,
            int quantity, string condition, DateTime? purchaseDate, decimal? purchasePrice, string description)
            => _dbHelper.AddAssetAsync(assetCode, assetName, category, roomId, quantity, condition, purchaseDate, purchasePrice, description);

        public Task<bool> UpdateAssetAsync(int assetId, string assetName, string category, int? roomId,
            int quantity, string condition, DateTime? purchaseDate, decimal? purchasePrice, string description)
            => _dbHelper.UpdateAssetAsync(assetId, assetName, category, roomId, quantity, condition, purchaseDate, purchasePrice, description);

        public Task<bool> DeleteAssetAsync(int assetId)
            => _dbHelper.DeleteAssetAsync(assetId);

        #endregion

        #region Reports & Statistics (Báo Cáo & Thống Kê)

        public Task<DataTable> GetDashboardSummaryAsync()
            => _dbHelper.GetDashboardSummaryAsync();

        public Task<DataTable> GetRoomOccupancyAsync(int? branchId = null)
            => _dbHelper.GetRoomOccupancyAsync(branchId);

        public Task<DataTable> GetRevenueReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
            => _dbHelper.GetRevenueReportAsync(fromDate, toDate);

        public Task<DataTable> GetTenantHistoryAsync()
            => _dbHelper.GetTenantHistoryAsync();

        #endregion

        #region Tenant Room History (Lịch Sử Phòng)

        public Task<DataTable> GetTenantRoomHistoryAsync()
            => _dbHelper.GetTenantRoomHistoryAsync();

        public Task<int> AddTenantRoomHistoryAsync(int tenantId, int roomId, DateTime checkInDate,
            DateTime? checkOutDate, string status, string notes)
            => _dbHelper.AddTenantRoomHistoryAsync(tenantId, roomId, checkInDate, checkOutDate, status, notes);

        public Task<bool> UpdateTenantRoomHistoryAsync(int historyId, int roomId, DateTime checkInDate,
            DateTime? checkOutDate, string status, string notes)
            => _dbHelper.UpdateTenantRoomHistoryAsync(historyId, roomId, checkInDate, checkOutDate, status, notes);

        public Task<bool> DeleteTenantRoomHistoryAsync(int historyId)
            => _dbHelper.DeleteTenantRoomHistoryAsync(historyId);

        #endregion

        #region Branches (Chi Nhánh)

        public Task<DataTable> GetBranchesAsync()
            => _dbHelper.GetBranchesAsync();

        public Task<DataTable> GetBranchSectionsAsync(int branchId)
            => _dbHelper.GetBranchSectionsAsync(branchId);

        #endregion
    }
}
