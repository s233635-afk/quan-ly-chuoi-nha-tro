using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Maintenance
{
    /// <summary>
    /// Business Logic Layer cho quản lý Bảo trì/Sửa chữa
    /// </summary>
    public class MaintenanceBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả tickets bảo trì
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetMaintenanceAsync();

        /// <summary>
        /// Thêm ticket bảo trì mới
        /// </summary>
        public Task<int> AddAsync(
            string ticketNumber, 
            int roomId, 
            string requestorType, 
            int? requestorId,
            string issueDescription, 
            string priority, 
            int? assignedToUserId, 
            string status, 
            DateTime? completedDate, 
            string notes)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber))
                throw new Exception("Số ticket không được để trống.");
            if (roomId <= 0)
                throw new Exception("Phòng không hợp lệ.");
            if (string.IsNullOrWhiteSpace(issueDescription))
                throw new Exception("Mô tả sự cố không được để trống.");

            return dbHelper.AddMaintenanceTicketAsync(
                ticketNumber.Trim(), 
                roomId, 
                requestorType, 
                requestorId,
                issueDescription.Trim(), 
                priority, 
                assignedToUserId, 
                status, 
                completedDate, 
                notes);
        }

        /// <summary>
        /// Cập nhật ticket bảo trì
        /// </summary>
        public Task<bool> UpdateAsync(
            int ticketId, 
            int roomId, 
            string requestorType, 
            int? requestorId,
            string issueDescription, 
            string priority, 
            int? assignedToUserId, 
            string status, 
            DateTime? completedDate, 
            string notes)
        {
            if (ticketId <= 0)
                throw new Exception("ID ticket không hợp lệ.");
            if (roomId <= 0)
                throw new Exception("Phòng không hợp lệ.");
            if (string.IsNullOrWhiteSpace(issueDescription))
                throw new Exception("Mô tả sự cố không được để trống.");

            return dbHelper.UpdateMaintenanceTicketAsync(
                ticketId, 
                roomId, 
                requestorType, 
                requestorId,
                issueDescription.Trim(), 
                priority, 
                assignedToUserId, 
                status, 
                completedDate, 
                notes);
        }

        /// <summary>
        /// Xóa ticket bảo trì
        /// </summary>
        public Task<bool> DeleteAsync(int ticketId)
        {
            if (ticketId <= 0)
                throw new Exception("ID ticket không hợp lệ.");

            return dbHelper.DeleteMaintenanceTicketAsync(ticketId);
        }
    }
}
