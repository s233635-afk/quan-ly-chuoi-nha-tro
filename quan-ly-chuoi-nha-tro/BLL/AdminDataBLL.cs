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
        public Task<DataTable> GetStaffAsync() => dbHelper.GetUsersByRoleAsync(2);
        public Task<DataTable> GetTenantsAsync() => dbHelper.GetTenantsAsync();
        public Task<DataTable> GetDependentsAsync() => dbHelper.GetDependentsAsync();
        public Task<DataTable> GetTenantHistoryAsync() => dbHelper.GetTenantHistoryAsync();
        public Task<DataTable> GetContractsAsync() => dbHelper.GetContractsAsync();
        public Task<DataTable> GetDepositsAsync() => dbHelper.GetDepositsAsync();
        public Task<DataTable> GetUtilitiesAsync() => dbHelper.GetUtilitiesAsync();
        public Task<DataTable> GetUtilityTypesAsync() => dbHelper.GetUtilityTypesAsync();
        public Task<DataTable> GetInvoicesAsync() => dbHelper.GetInvoicesAsync();
        public Task<DataTable> GetPaymentsAsync() => dbHelper.GetPaymentsAsync();
        public Task<DataTable> GetMaintenanceAsync() => dbHelper.GetMaintenanceAsync();
        public Task<DataTable> GetAssetsAsync() => dbHelper.GetAssetsAsync();
        public Task<DataTable> GetNotificationsAsync() => dbHelper.GetNotificationsAsync();
        public Task<DataTable> GetSystemSettingsAsync() => dbHelper.GetSystemSettingsAsync();
        public Task<DataTable> GetDashboardSummaryAsync() => dbHelper.GetDashboardSummaryAsync();

        public Task<int> AddStaffUserAsync(string username, string password, string fullName, string email, string phone, bool isActive)
            => dbHelper.AddStaffUserAsync(username, password, fullName, email, phone, isActive);

        public Task<bool> UpdateStaffUserAsync(int userId, string fullName, string email, string phone, bool isActive, string newPassword = null)
            => dbHelper.UpdateStaffUserAsync(userId, fullName, email, phone, isActive, newPassword);

        public Task<bool> DeleteStaffUserAsync(int userId) => dbHelper.DeleteStaffUserAsync(userId);

        public Task<int> AddTenantAsync(string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry, bool isActive)
            => dbHelper.AddTenantAsync(fullName, identityCard, phoneNumber, email, birthDate, address, tempReg, tempRegDate, tempRegExpiry, isActive);

        public Task<bool> UpdateTenantAsync(int tenantId, string fullName, string identityCard, string phoneNumber, string email,
            DateTime? birthDate, string address, string tempReg, DateTime? tempRegDate, DateTime? tempRegExpiry, bool isActive)
            => dbHelper.UpdateTenantAsync(tenantId, fullName, identityCard, phoneNumber, email, birthDate, address, tempReg, tempRegDate, tempRegExpiry, isActive);

        public Task<bool> DeleteTenantAsync(int tenantId) => dbHelper.DeleteTenantAsync(tenantId);

        public Task<int> AddDepositAsync(int tenantId, int roomId, decimal depositAmount, DateTime? depositDate,
            string depositType, string status, decimal? returnedAmount, DateTime? returnedDate, string notes)
            => dbHelper.AddDepositAsync(tenantId, roomId, depositAmount, depositDate, depositType, status, returnedAmount, returnedDate, notes);

        public Task<bool> UpdateDepositAsync(int depositId, int tenantId, int roomId, decimal depositAmount, DateTime? depositDate,
            string depositType, string status, decimal? returnedAmount, DateTime? returnedDate, string notes)
            => dbHelper.UpdateDepositAsync(depositId, tenantId, roomId, depositAmount, depositDate, depositType, status, returnedAmount, returnedDate, notes);

        public Task<bool> DeleteDepositAsync(int depositId) => dbHelper.DeleteDepositAsync(depositId);

        public Task<int> AddRoomAsync(string roomNumber, int branchId, int? sectionId, int? roomTypeId, decimal? roomPrice,
            int? currentStatusId, int? floor, decimal? area, bool? isActive)
            => dbHelper.AddRoomAsync(roomNumber, branchId, sectionId, roomTypeId, roomPrice, currentStatusId, floor, area, isActive);

        public Task<bool> UpdateRoomAsync(int roomId, string roomNumber, int branchId, int? sectionId, int? roomTypeId, decimal? roomPrice,
            int? currentStatusId, int? floor, decimal? area, bool? isActive)
            => dbHelper.UpdateRoomAsync(roomId, roomNumber, branchId, sectionId, roomTypeId, roomPrice, currentStatusId, floor, area, isActive);

        public Task<bool> DeleteRoomAsync(int roomId) => dbHelper.DeleteRoomAsync(roomId);
    }
}
