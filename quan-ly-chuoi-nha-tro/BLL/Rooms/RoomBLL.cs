using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Rooms
{
    /// <summary>
    /// Business Logic Layer cho quản lý Phòng
    /// Extracted từ AdminDataBLL để dễ maintain
    /// </summary>
    public class RoomBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả phòng
        /// </summary>
        public Task<DataTable> GetRoomsAsync() => dbHelper.GetRoomsAsync();

        /// <summary>
        /// Thêm phòng mới
        /// </summary>
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
            // Validation
            if (string.IsNullOrWhiteSpace(roomNumber))
                throw new Exception("Số phòng không được để trống.");
            if (branchId <= 0)
                throw new Exception("Chi nhánh không hợp lệ.");
            if (!sectionId.HasValue || sectionId.Value <= 0)
                throw new Exception("Vui lòng chọn Khu/Dãy.");
            if (!roomTypeId.HasValue || roomTypeId.Value <= 0)
                throw new Exception("Vui lòng chọn Loại phòng.");

            if (!currentStatusId.HasValue || currentStatusId.Value <= 0)
                currentStatusId = 1; // Default: Trống

            return await dbHelper.AddRoomAsync(
                roomNumber.Trim(), 
                branchId, 
                sectionId, 
                roomTypeId, 
                roomPrice, 
                currentStatusId, 
                floor, 
                area, 
                isActive);
        }

        /// <summary>
        /// Cập nhật thông tin phòng
        /// </summary>
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
            // Validation
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
                currentStatusId = 1; // Default: Trống

            return await dbHelper.UpdateRoomAsync(
                roomId, 
                roomNumber.Trim(), 
                branchId, 
                sectionId, 
                roomTypeId, 
                roomPrice, 
                currentStatusId, 
                floor, 
                area, 
                isActive, 
                occupants);
        }

        /// <summary>
        /// Cập nhật trạng thái phòng (Trống/Đang ở/Bảo trì/...)
        /// </summary>
        public Task<int> UpdateRoomOccupancyStatusAsync(int roomId, int statusId)
            => dbHelper.UpdateRoomOccupancyStatusAsync(roomId, statusId);

        /// <summary>
        /// Xóa phòng (hard delete - xóa cả related data)
        /// </summary>
        public Task<bool> DeleteRoomAsync(int roomId) 
            => dbHelper.DeleteRoomAsync(roomId);
    }
}
