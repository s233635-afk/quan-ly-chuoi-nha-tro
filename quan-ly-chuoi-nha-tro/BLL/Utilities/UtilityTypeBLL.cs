using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Utilities
{
    /// <summary>
    /// Business Logic Layer cho quản lý Loại tiện ích (Điện, Nước, Internet, v.v.)
    /// </summary>
    public class UtilityTypeBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả loại tiện ích
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetUtilityTypesAsync();

        /// <summary>
        /// Thêm loại tiện ích mới
        /// </summary>
        public Task<int> AddAsync(
            string utilityName, 
            string utilityCode, 
            string unit, 
            bool isRecurring, 
            decimal? defaultPrice, 
            string description, 
            bool isActive)
        {
            if (string.IsNullOrWhiteSpace(utilityName))
                throw new Exception("Tên tiện ích không được để trống.");
            if (string.IsNullOrWhiteSpace(utilityCode))
                throw new Exception("Mã tiện ích không được để trống.");

            return dbHelper.AddUtilityTypeAsync(
                utilityName.Trim(), 
                utilityCode.Trim(), 
                unit, 
                isRecurring, 
                defaultPrice, 
                description, 
                isActive);
        }

        /// <summary>
        /// Cập nhật loại tiện ích
        /// </summary>
        public Task<bool> UpdateAsync(
            int utilityTypeId, 
            string utilityName, 
            string utilityCode, 
            string unit, 
            bool isRecurring, 
            decimal? defaultPrice, 
            string description, 
            bool isActive)
        {
            if (utilityTypeId <= 0)
                throw new Exception("ID loại tiện ích không hợp lệ.");
            if (string.IsNullOrWhiteSpace(utilityName))
                throw new Exception("Tên tiện ích không được để trống.");
            if (string.IsNullOrWhiteSpace(utilityCode))
                throw new Exception("Mã tiện ích không được để trống.");

            return dbHelper.UpdateUtilityTypeAsync(
                utilityTypeId, 
                utilityName.Trim(), 
                utilityCode.Trim(), 
                unit, 
                isRecurring, 
                defaultPrice, 
                description, 
                isActive);
        }

        /// <summary>
        /// Xóa loại tiện ích
        /// </summary>
        public Task<bool> DeleteAsync(int utilityTypeId)
        {
            if (utilityTypeId <= 0)
                throw new Exception("ID loại tiện ích không hợp lệ.");

            return dbHelper.DeleteUtilityTypeAsync(utilityTypeId);
        }
    }
}
