using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.SystemSettings
{
    /// <summary>
    /// Business Logic Layer cho quản lý Cài đặt hệ thống
    /// </summary>
    public class SystemSettingBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả cài đặt
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetSystemSettingsAsync();

        /// <summary>
        /// Thêm cài đặt mới
        /// </summary>
        public Task<bool> AddAsync(string key, string value, string description)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("Key không được để trống.");

            return dbHelper.AddSystemSettingAsync(key.Trim(), value, description);
        }

        /// <summary>
        /// Cập nhật cài đặt
        /// </summary>
        public Task<bool> UpdateAsync(string key, string value, string description)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("Key không được để trống.");

            return dbHelper.UpdateSystemSettingAsync(key.Trim(), value, description);
        }

        /// <summary>
        /// Xóa cài đặt
        /// </summary>
        public Task<bool> DeleteAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("Key không được để trống.");

            return dbHelper.DeleteSystemSettingAsync(key.Trim());
        }
    }
}
