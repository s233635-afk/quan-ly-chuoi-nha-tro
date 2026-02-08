using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Rooms
{
    /// <summary>
    /// Business Logic Layer cho quản lý Loại phòng
    /// </summary>
    public class RoomTypeBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả loại phòng
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetRoomTypesAsync();

        /// <summary>
        /// Thêm loại phòng mới
        /// </summary>
        public Task<int> AddAsync(
            string roomTypeName, 
            decimal? defaultPrice, 
            string amenities, 
            int? maxCapacity, 
            string description, 
            bool isActive)
        {
            if (string.IsNullOrWhiteSpace(roomTypeName))
                throw new Exception("Tên loại phòng không được để trống.");

            return dbHelper.AddRoomTypeAsync(
                roomTypeName.Trim(), 
                defaultPrice, 
                amenities, 
                maxCapacity, 
                description, 
                isActive);
        }

        /// <summary>
        /// Cập nhật loại phòng
        /// </summary>
        public Task<bool> UpdateAsync(
            int roomTypeId, 
            string roomTypeName, 
            decimal? defaultPrice, 
            string amenities, 
            int? maxCapacity, 
            string description, 
            bool isActive)
        {
            if (roomTypeId <= 0)
                throw new Exception("ID loại phòng không hợp lệ.");
            if (string.IsNullOrWhiteSpace(roomTypeName))
                throw new Exception("Tên loại phòng không được để trống.");

            return dbHelper.UpdateRoomTypeAsync(
                roomTypeId, 
                roomTypeName.Trim(), 
                defaultPrice, 
                amenities, 
                maxCapacity, 
                description, 
                isActive);
        }

        /// <summary>
        /// Xóa loại phòng
        /// </summary>
        public Task<bool> DeleteAsync(int roomTypeId)
        {
            if (roomTypeId <= 0)
                throw new Exception("ID loại phòng không hợp lệ.");

            return dbHelper.DeleteRoomTypeAsync(roomTypeId);
        }
    }
}
