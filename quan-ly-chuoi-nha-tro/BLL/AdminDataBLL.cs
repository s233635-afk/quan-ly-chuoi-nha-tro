using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL
{
    public class AdminDataBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public Task<DataTable> GetRoomsAsync() => dbHelper.GetRoomsAsync();
        public Task<DataTable> GetBranchesAsync() => dbHelper.GetBranchesAsync();
        public Task<DataTable> GetBranchSectionsAsync(int? branchId = null) => dbHelper.GetBranchSectionsAsync(branchId);
        public Task<DataTable> GetRoomTypesAsync() => dbHelper.GetRoomTypesAsync();
        public Task<DataTable> GetRoomStatusesAsync() => dbHelper.GetRoomStatusesAsync();
        public Task<DataTable> GetStaffAsync() => dbHelper.GetUsersByRoleAsync(2);
        public Task<DataTable> GetUsersByBranchAsync(int branchId) => dbHelper.GetUsersByBranchAsync(branchId);
        public Task<DataTable> GetTenantsAsync() => dbHelper.GetTenantsAsync();
        public Task<DataTable> GetDependentsAsync() => dbHelper.GetDependentsAsync();
        public Task<DataTable> GetTenantHistoryAsync() => dbHelper.GetTenantHistoryAsync();
        public Task<DataTable> GetContractsAsync() => dbHelper.GetContractsAsync();
        public Task<DataTable> GetDepositsAsync() => dbHelper.GetDepositsAsync();
        public Task<DataTable> GetUtilitiesAsync() => dbHelper.GetUtilitiesAsync();
        public Task<DataTable> GetUtilityTypesAsync() => dbHelper.GetUtilityTypesAsync();
        public Task<DataTable> GetInvoicesAsync() => dbHelper.GetInvoicesAsync();
        public Task<DataTable> GetPaymentsAsync() => dbHelper.GetPaymentsAsync();
        public Task<DataTable> GetInvoicesViewAsync() => dbHelper.GetInvoicesViewAsync();
        public Task<DataTable> GetPaymentsViewAsync() => dbHelper.GetPaymentsViewAsync();
        public Task<DataTable> GetPaymentsByInvoiceAsync(int invoiceId) => dbHelper.GetPaymentsByInvoiceAsync(invoiceId);
        public Task<DataTable> GetMaintenanceAsync() => dbHelper.GetMaintenanceAsync();
        public Task<DataTable> GetAssetsAsync() => dbHelper.GetAssetsAsync();
        public Task<DataTable> GetNotificationsAsync() => dbHelper.GetNotificationsAsync();
        public Task<DataTable> GetSystemSettingsAsync() => dbHelper.GetSystemSettingsAsync();
        public Task<DataTable> GetDashboardSummaryAsync() => dbHelper.GetDashboardSummaryAsync();

        public Task<int> AddStaffUserAsync(string username, string password, string fullName, string email, string phone, int? branchId, bool isActive)
            => dbHelper.AddStaffUserAsync(username, password, fullName, email, phone, branchId, isActive);

        public Task<bool> UpdateStaffUserAsync(int userId, string fullName, string email, string phone, int? branchId, bool isActive, string newPassword = null)
            => dbHelper.UpdateStaffUserAsync(userId, fullName, email, phone, branchId, isActive, newPassword);

        public Task<bool> DeleteStaffUserAsync(int userId) => dbHelper.DeleteStaffUserAsync(userId);

        public Task<int> AddTenantAsync(string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry, bool isActive)
            => dbHelper.AddTenantAsync(fullName, identityCard, phoneNumber, email, birthDate, address, tempReg, tempRegDate, tempRegExpiry, isActive);

        public Task<int> AddTenantAsync(string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry, bool isActive,
            string frontIdPhoto, string backIdPhoto)
            => dbHelper.AddTenantAsync(fullName, identityCard, phoneNumber, email, birthDate, address, tempReg, tempRegDate, tempRegExpiry, isActive, frontIdPhoto, backIdPhoto);

        public Task<bool> UpdateTenantAsync(int tenantId, string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry, bool isActive)
            => dbHelper.UpdateTenantAsync(tenantId, fullName, identityCard, phoneNumber, email, birthDate, address, tempReg, tempRegDate, tempRegExpiry, isActive);

        public Task<bool> UpdateTenantAsync(int tenantId, string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry, bool isActive,
            string frontIdPhoto, string backIdPhoto)
            => dbHelper.UpdateTenantAsync(tenantId, fullName, identityCard, phoneNumber, email, birthDate, address, tempReg, tempRegDate, tempRegExpiry, isActive, frontIdPhoto, backIdPhoto);

        public Task<bool> DeleteTenantAsync(int tenantId) => dbHelper.DeleteTenantAsync(tenantId);

        public Task<int> AddDependentAsync(int tenantId, string fullName, string relationship, string phoneNumber)
            => dbHelper.AddDependentAsync(tenantId, fullName, relationship, phoneNumber);

        public Task<bool> UpdateDependentAsync(int dependentId, int tenantId, string fullName, string relationship, string phoneNumber)
            => dbHelper.UpdateDependentAsync(dependentId, tenantId, fullName, relationship, phoneNumber);

        public Task<bool> DeleteDependentAsync(int dependentId)
            => dbHelper.DeleteDependentAsync(dependentId);

        public Task<int> AddTenantHistoryAsync(int tenantId, int roomId, DateTime checkInDate, DateTime? checkOutDate, string status, string notes)
            => dbHelper.AddTenantHistoryAsync(tenantId, roomId, checkInDate, checkOutDate, status, notes);

        public Task<bool> UpdateTenantHistoryAsync(int historyId, int roomId, DateTime checkInDate, DateTime? checkOutDate, string status, string notes)
            => dbHelper.UpdateTenantHistoryAsync(historyId, roomId, checkInDate, checkOutDate, status, notes);

        public Task<bool> DeleteTenantHistoryAsync(int historyId)
            => dbHelper.DeleteTenantHistoryAsync(historyId);

        public Task<int> AddContractAsync(
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
            => dbHelper.AddContractAsync(contractNumber, tenantId, roomId, signDate, startDate, endDate, rentalPrice, depositRequired, terms, contractPdfPath, status);

        public Task<bool> UpdateContractAsync(
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
            => dbHelper.UpdateContractAsync(contractId, contractNumber, tenantId, roomId, signDate, startDate, endDate, rentalPrice, depositRequired, terms, contractPdfPath, status);

        public Task<bool> DeleteContractAsync(int contractId) => dbHelper.DeleteContractAsync(contractId);

        public Task<int> AddDepositAsync(int tenantId, int roomId, decimal depositAmount, DateTime? depositDate,
            string depositType, string status, decimal? returnedAmount, DateTime? returnedDate, string notes)
            => dbHelper.AddDepositAsync(tenantId, roomId, depositAmount, depositDate, depositType, status, returnedAmount, returnedDate, notes);

        public Task<bool> UpdateDepositAsync(int depositId, int tenantId, int roomId, decimal depositAmount, DateTime? depositDate,
            string depositType, string status, decimal? returnedAmount, DateTime? returnedDate, string notes)
            => dbHelper.UpdateDepositAsync(depositId, tenantId, roomId, depositAmount, depositDate, depositType, status, returnedAmount, returnedDate, notes);

        public Task<bool> DeleteDepositAsync(int depositId) => dbHelper.DeleteDepositAsync(depositId);

        public Task<int> AddInvoiceAsync(
            string invoiceNumber,
            int tenantId,
            int roomId,
            DateTime invoiceDate,
            DateTime? fromDate,
            DateTime? toDate,
            decimal rentalCost,
            decimal utilityCost,
            decimal otherCost,
            DateTime? dueDate)
            => dbHelper.AddInvoiceAsync(invoiceNumber, tenantId, roomId, invoiceDate, fromDate, toDate, rentalCost, utilityCost, otherCost, dueDate);

        public Task<bool> UpdateInvoiceAsync(
            int invoiceId,
            string invoiceNumber,
            int tenantId,
            int roomId,
            DateTime invoiceDate,
            DateTime? fromDate,
            DateTime? toDate,
            decimal rentalCost,
            decimal utilityCost,
            decimal otherCost,
            DateTime? dueDate)
            => dbHelper.UpdateInvoiceAsync(invoiceId, invoiceNumber, tenantId, roomId, invoiceDate, fromDate, toDate, rentalCost, utilityCost, otherCost, dueDate);

        public Task<bool> DeleteInvoiceAsync(int invoiceId, bool deletePaymentsFirst)
            => dbHelper.DeleteInvoiceAsync(invoiceId, deletePaymentsFirst);

        public Task<int> AddPaymentAsync(int invoiceId, DateTime paymentDate, decimal paymentAmount, string paymentMethod, string transactionReference, string notes)
            => dbHelper.AddPaymentAsync(invoiceId, paymentDate, paymentAmount, paymentMethod, transactionReference, notes);

        public Task<bool> UpdatePaymentAsync(int paymentId, DateTime paymentDate, decimal paymentAmount, string paymentMethod, string transactionReference, string notes)
            => dbHelper.UpdatePaymentAsync(paymentId, paymentDate, paymentAmount, paymentMethod, transactionReference, notes);

        public Task<bool> DeletePaymentAsync(int paymentId) => dbHelper.DeletePaymentAsync(paymentId);

        public Task<int> GenerateMonthlyInvoicesAsync(int year, int month, DateTime? invoiceDate = null, int? dueDay = null)
            => dbHelper.GenerateMonthlyInvoicesAsync(year, month, invoiceDate, dueDay);

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
            if (string.IsNullOrWhiteSpace(roomNumber))
                throw new Exception("Số phòng không được để trống.");
            if (branchId <= 0)
                throw new Exception("Chi nhánh không hợp lệ.");
            if (!sectionId.HasValue || sectionId.Value <= 0)
                throw new Exception("Vui lòng chọn Khu/Dãy.");
            if (!roomTypeId.HasValue || roomTypeId.Value <= 0)
                throw new Exception("Vui lòng chọn Loại phòng.");

            if (!currentStatusId.HasValue || currentStatusId.Value <= 0)
                currentStatusId = 1;

            return await dbHelper.AddRoomAsync(roomNumber.Trim(), branchId, sectionId, roomTypeId, roomPrice, currentStatusId, floor, area, isActive);
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
            if (roomId <= 0)
                throw new Exception("ID phòng không hợp lệ.");
            if (string.IsNullOrWhiteSpace(roomNumber))
                throw new Exception("Số phòng không được để trống.");
            if (branchId <= 0)
                throw new Exception("Chi nhánh không hợp lệ.");
            if (!sectionId.HasValue || sectionId.Value <= 0)
                throw new Exception("Vui lòng chọn Khu/Dãy.");
            if (!roomTypeId.HasValue || roomTypeId.Value <= 0)
                throw new Exception("Vui lòng chọn Loại phòng.");

            if (!currentStatusId.HasValue || currentStatusId.Value <= 0)
                currentStatusId = 1;

            return await dbHelper.UpdateRoomAsync(roomId, roomNumber.Trim(), branchId, sectionId, roomTypeId, roomPrice, currentStatusId, floor, area, isActive, occupants);
        }

        public Task<bool> DeleteRoomAsync(int roomId) => dbHelper.DeleteRoomAsync(roomId);

        public Task<int> AddUtilityTypeAsync(string utilityName, string utilityCode, string unit, bool isRecurring, decimal? defaultPrice, string description, bool isActive)
            => dbHelper.AddUtilityTypeAsync(utilityName, utilityCode, unit, isRecurring, defaultPrice, description, isActive);

        public Task<bool> UpdateUtilityTypeAsync(int utilityTypeId, string utilityName, string utilityCode, string unit, bool isRecurring, decimal? defaultPrice, string description, bool isActive)
            => dbHelper.UpdateUtilityTypeAsync(utilityTypeId, utilityName, utilityCode, unit, isRecurring, defaultPrice, description, isActive);

        public Task<bool> DeleteUtilityTypeAsync(int utilityTypeId)
            => dbHelper.DeleteUtilityTypeAsync(utilityTypeId);

        public Task<int> AddUtilityReadingAsync(int roomId, int utilityTypeId, DateTime? readingDate, decimal? previousReading, decimal? currentReading, decimal? usageAmount, decimal? unitPrice, decimal? totalCost, string notes)
            => dbHelper.AddUtilityReadingAsync(roomId, utilityTypeId, readingDate, previousReading, currentReading, usageAmount, unitPrice, totalCost, notes);

        public Task<bool> UpdateUtilityReadingAsync(int readingId, int roomId, int utilityTypeId, DateTime? readingDate, decimal? previousReading, decimal? currentReading, decimal? usageAmount, decimal? unitPrice, decimal? totalCost, string notes)
            => dbHelper.UpdateUtilityReadingAsync(readingId, roomId, utilityTypeId, readingDate, previousReading, currentReading, usageAmount, unitPrice, totalCost, notes);

        public Task<bool> DeleteUtilityReadingAsync(int readingId)
            => dbHelper.DeleteUtilityReadingAsync(readingId);

        public Task<int> AddMaintenanceTicketAsync(string ticketNumber, int roomId, string requestorType, int? requestorId,
            string issueDescription, string priority, int? assignedToUserId, string status, DateTime? completedDate, string notes)
            => dbHelper.AddMaintenanceTicketAsync(ticketNumber, roomId, requestorType, requestorId, issueDescription, priority, assignedToUserId, status, completedDate, notes);

        public Task<bool> UpdateMaintenanceTicketAsync(int ticketId, int roomId, string requestorType, int? requestorId,
            string issueDescription, string priority, int? assignedToUserId, string status, DateTime? completedDate, string notes)
            => dbHelper.UpdateMaintenanceTicketAsync(ticketId, roomId, requestorType, requestorId, issueDescription, priority, assignedToUserId, status, completedDate, notes);

        public Task<bool> DeleteMaintenanceTicketAsync(int ticketId)
            => dbHelper.DeleteMaintenanceTicketAsync(ticketId);

        public Task<int> AddAssetAsync(string assetCode, string assetName, string category, int? roomId, int quantity, string condition,
            DateTime? purchaseDate, decimal? purchasePrice, string description, bool isActive)
            => dbHelper.AddAssetAsync(assetCode, assetName, category, roomId, quantity, condition, purchaseDate, purchasePrice, description, isActive);

        public Task<bool> UpdateAssetAsync(int assetId, string assetCode, string assetName, string category, int? roomId, int quantity, string condition,
            DateTime? purchaseDate, decimal? purchasePrice, string description, bool isActive)
            => dbHelper.UpdateAssetAsync(assetId, assetCode, assetName, category, roomId, quantity, condition, purchaseDate, purchasePrice, description, isActive);

        public Task<bool> DeleteAssetAsync(int assetId)
            => dbHelper.DeleteAssetAsync(assetId);

        public Task<int> AddNotificationAsync(int? userId, string title, string message, string status)
            => dbHelper.AddNotificationAsync(userId, title, message, status);

        public Task<bool> UpdateNotificationAsync(int notificationId, int? userId, string title, string message, string status)
            => dbHelper.UpdateNotificationAsync(notificationId, userId, title, message, status);

        public Task<bool> DeleteNotificationAsync(int notificationId)
            => dbHelper.DeleteNotificationAsync(notificationId);

        public Task<bool> AddSystemSettingAsync(string key, string value, string description)
            => dbHelper.AddSystemSettingAsync(key, value, description);

        public Task<bool> UpdateSystemSettingAsync(string key, string value, string description)
            => dbHelper.UpdateSystemSettingAsync(key, value, description);

        public Task<bool> DeleteSystemSettingAsync(string key)
            => dbHelper.DeleteSystemSettingAsync(key);

        #region Lookups (BranchSections / RoomTypes / RoomStatuses)

        public Task<int> AddBranchSectionAsync(int branchId, string sectionCode, string sectionName, string description, bool isActive)
            => dbHelper.AddBranchSectionAsync(branchId, sectionCode, sectionName, description, isActive);

        public Task<bool> UpdateBranchSectionAsync(int sectionId, int branchId, string sectionCode, string sectionName, string description, bool isActive)
            => dbHelper.UpdateBranchSectionAsync(sectionId, branchId, sectionCode, sectionName, description, isActive);

        public Task<bool> DeleteBranchSectionAsync(int sectionId)
            => dbHelper.DeleteBranchSectionAsync(sectionId);

        public Task<int> AddRoomTypeAsync(string roomTypeName, decimal? defaultPrice, string amenities, int? maxCapacity, string description, bool isActive)
            => dbHelper.AddRoomTypeAsync(roomTypeName, defaultPrice, amenities, maxCapacity, description, isActive);

        public Task<bool> UpdateRoomTypeAsync(int roomTypeId, string roomTypeName, decimal? defaultPrice, string amenities, int? maxCapacity, string description, bool isActive)
            => dbHelper.UpdateRoomTypeAsync(roomTypeId, roomTypeName, defaultPrice, amenities, maxCapacity, description, isActive);

        public Task<bool> DeleteRoomTypeAsync(int roomTypeId)
            => dbHelper.DeleteRoomTypeAsync(roomTypeId);

        public Task<int> AddRoomStatusAsync(string statusName, string description)
            => dbHelper.AddRoomStatusAsync(statusName, description);

        public Task<bool> UpdateRoomStatusAsync(int statusId, string statusName, string description)
            => dbHelper.UpdateRoomStatusAsync(statusId, statusName, description);

        public Task<bool> DeleteRoomStatusAsync(int statusId)
            => dbHelper.DeleteRoomStatusAsync(statusId);

        #endregion
    }
}

