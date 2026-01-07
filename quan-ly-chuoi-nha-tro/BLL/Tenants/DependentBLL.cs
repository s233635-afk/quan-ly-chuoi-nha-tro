using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Tenants
{
    /// <summary>
    /// Business Logic Layer cho quản lý Người ở chung/Người phụ thuộc
    /// </summary>
    public class DependentBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả người ở chung
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetDependentsAsync();

        /// <summary>
        /// Thêm người ở chung mới
        /// </summary>
        public Task<int> AddAsync(int tenantId, string fullName, string relationship, string phoneNumber)
        {
            if (tenantId <= 0)
                throw new Exception("ID khách thuê không hợp lệ.");
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("Tên người ở chung không được để trống.");

            return dbHelper.AddDependentAsync(tenantId, fullName.Trim(), relationship, phoneNumber);
        }

        /// <summary>
        /// Cập nhật thông tin người ở chung
        /// </summary>
        public Task<bool> UpdateAsync(int dependentId, int tenantId, string fullName, string relationship, string phoneNumber)
        {
            if (dependentId <= 0)
                throw new Exception("ID người ở chung không hợp lệ.");
            if (tenantId <= 0)
                throw new Exception("ID khách thuê không hợp lệ.");
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("Tên người ở chung không được để trống.");

            return dbHelper.UpdateDependentAsync(dependentId, tenantId, fullName.Trim(), relationship, phoneNumber);
        }

        /// <summary>
        /// Xóa người ở chung
        /// </summary>
        public Task<bool> DeleteAsync(int dependentId)
        {
            if (dependentId <= 0)
                throw new Exception("ID người ở chung không hợp lệ.");

            return dbHelper.DeleteDependentAsync(dependentId);
        }
    }
}
