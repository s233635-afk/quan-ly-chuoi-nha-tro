using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Notifications
{
    /// <summary>
    /// Business Logic Layer cho quản lý Thông báo
    /// </summary>
    public class NotificationBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả thông báo
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetNotificationsAsync();

        /// <summary>
        /// Thêm thông báo mới
        /// </summary>
        public Task<int> AddAsync(int? userId, string title, string message, string status)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new Exception("Tiêu đề thông báo không được để trống.");

            return dbHelper.AddNotificationAsync(userId, title.Trim(), message, status);
        }

        /// <summary>
        /// Cập nhật thông báo
        /// </summary>
        public Task<bool> UpdateAsync(int notificationId, int? userId, string title, string message, string status)
        {
            if (notificationId <= 0)
                throw new Exception("ID thông báo không hợp lệ.");
            if (string.IsNullOrWhiteSpace(title))
                throw new Exception("Tiêu đề thông báo không được để trống.");

            return dbHelper.UpdateNotificationAsync(notificationId, userId, title.Trim(), message, status);
        }

        /// <summary>
        /// Xóa thông báo
        /// </summary>
        public Task<bool> DeleteAsync(int notificationId)
        {
            if (notificationId <= 0)
                throw new Exception("ID thông báo không hợp lệ.");

            return dbHelper.DeleteNotificationAsync(notificationId);
        }
    }
}
