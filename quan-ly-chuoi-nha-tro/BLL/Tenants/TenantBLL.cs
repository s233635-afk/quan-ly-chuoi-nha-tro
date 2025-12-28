using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Tenants
{
    /// <summary>
    /// Business Logic Layer cho quản lý Khách thuê
    /// </summary>
    public class TenantBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả khách thuê
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetTenantsAsync();

        /// <summary>
        /// Thêm khách thuê mới (phiên bản đầy đủ với ảnh CCCD)
        /// </summary>
        public Task<int> AddAsync(
            string fullName, 
            string identityCard, 
            string phoneNumber, 
            string email,
            DateTime? birthDate, 
            string address, 
            string tempReg, 
            DateTime? tempRegDate, 
            DateTime? tempRegExpiry, 
            bool isActive,
            string frontIdPhoto, 
            string backIdPhoto)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("Tên khách thuê không được để trống.");
            
            return dbHelper.AddTenantAsync(
                fullName.Trim(), 
                identityCard, 
                phoneNumber, 
                email,
                birthDate, 
                address, 
                tempReg, 
                tempRegDate, 
                tempRegExpiry, 
                isActive,
                frontIdPhoto, 
                backIdPhoto);
        }

        /// <summary>
        /// Thêm khách thuê mới (phiên bản đơn giản không có ảnh)
        /// </summary>
        public Task<int> AddAsync(
            string fullName, 
            string identityCard, 
            string phoneNumber, 
            string email,
            DateTime? birthDate, 
            string address, 
            string tempReg, 
            DateTime? tempRegDate, 
            DateTime? tempRegExpiry, 
            bool isActive)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("Tên khách thuê không được để trống.");

            return dbHelper.AddTenantAsync(
                fullName.Trim(), 
                identityCard, 
                phoneNumber, 
                email,
                birthDate, 
                address, 
                tempReg, 
                tempRegDate, 
                tempRegExpiry, 
                isActive);
        }

        /// <summary>
        /// Cập nhật thông tin khách thuê (có ảnh CCCD)
        /// </summary>
        public Task<bool> UpdateAsync(
            int tenantId, 
            string fullName, 
            string identityCard, 
            string phoneNumber, 
            string email,
            DateTime? birthDate, 
            string address, 
            string tempReg, 
            DateTime? tempRegDate, 
            DateTime? tempRegExpiry, 
            bool isActive,
            string frontIdPhoto, 
            string backIdPhoto)
        {
            if (tenantId <= 0)
                throw new Exception("ID khách thuê không hợp lệ.");
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("Tên khách thuê không được để trống.");

            return dbHelper.UpdateTenantAsync(
                tenantId, 
                fullName.Trim(), 
                identityCard, 
                phoneNumber, 
                email,
                birthDate, 
                address, 
                tempReg, 
                tempRegDate, 
                tempRegExpiry, 
                isActive,
                frontIdPhoto, 
                backIdPhoto);
        }

        /// <summary>
        /// Cập nhật thông tin khách thuê (không có ảnh)
        /// </summary>
        public Task<bool> UpdateAsync(
            int tenantId, 
            string fullName, 
            string identityCard, 
            string phoneNumber, 
            string email,
            DateTime? birthDate, 
            string address, 
            string tempReg, 
            DateTime? tempRegDate, 
            DateTime? tempRegExpiry, 
            bool isActive)
        {
            if (tenantId <= 0)
                throw new Exception("ID khách thuê không hợp lệ.");
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("Tên khách thuê không được để trống.");

            return dbHelper.UpdateTenantAsync(
                tenantId, 
                fullName.Trim(), 
                identityCard, 
                phoneNumber, 
                email,
                birthDate, 
                address, 
                tempReg, 
                tempRegDate, 
                tempRegExpiry, 
                isActive);
        }

        /// <summary>
        /// Xóa khách thuê
        /// </summary>
        public Task<bool> DeleteAsync(int tenantId)
        {
            if (tenantId <= 0)
                throw new Exception("ID khách thuê không hợp lệ.");

            return dbHelper.DeleteTenantAsync(tenantId);
        }
    }
}
