using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Rooms
{
    /// <summary>
    /// Business Logic Layer cho quản lý Trạng thái phòng
    /// </summary>
    public class RoomStatusBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả trạng thái phòng
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetRoomStatusesAsync();

        /// <summary>
        /// Thêm trạng thái phòng mới
        /// </summary>
        public Task<int> AddAsync(string statusName, string description)
        {
            if (string.IsNullOrWhiteSpace(statusName))
                throw new Exception("Tên trạng thái không được để trống.");

            return dbHelper.AddRoomStatusAsync(statusName.Trim(), description);
        }

        /// <summary>
        /// Cập nhật trạng thái phòng
        /// </summary>
        public Task<bool> UpdateAsync(int statusId, string statusName, string description)
        {
            if (statusId <= 0)
                throw new Exception("ID trạng thái không hợp lệ.");
            if (string.IsNullOrWhiteSpace(statusName))
                throw new Exception("Tên trạng thái không được để trống.");

            return dbHelper.UpdateRoomStatusAsync(statusId, statusName.Trim(), description);
        }

        /// <summary>
        /// Xóa trạng thái phòng
        /// </summary>
        public Task<bool> DeleteAsync(int statusId)
        {
            if (statusId <= 0)
                throw new Exception("ID trạng thái không hợp lệ.");

            return dbHelper.DeleteRoomStatusAsync(statusId);
        }
    }
}
