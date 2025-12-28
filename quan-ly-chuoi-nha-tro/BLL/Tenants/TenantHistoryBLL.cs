using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Tenants
{
    /// <summary>
    /// Business Logic Layer cho quản lý Lịch sử thuê phòng của khách
    /// </summary>
    public class TenantHistoryBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả lịch sử thuê phòng
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetTenantHistoryAsync();

        /// <summary>
        /// Thêm lịch sử thuê phòng mới (Check-in)
        /// </summary>
        public Task<int> AddAsync(
            int tenantId, 
            int roomId, 
            DateTime checkInDate, 
            DateTime? checkOutDate, 
            string status, 
            string notes)
        {
            if (tenantId <= 0)
                throw new Exception("ID khách thuê không hợp lệ.");
            if (roomId <= 0)
                throw new Exception("ID phòng không hợp lệ.");

            return dbHelper.AddTenantHistoryAsync(tenantId, roomId, checkInDate, checkOutDate, status, notes);
        }

        /// <summary>
        /// Cập nhật lịch sử thuê phòng (Check-out)
        /// </summary>
        public Task<bool> UpdateAsync(
            int historyId, 
            int roomId, 
            DateTime checkInDate, 
            DateTime? checkOutDate, 
            string status, 
            string notes)
        {
            if (historyId <= 0)
                throw new Exception("ID lịch sử không hợp lệ.");
            if (roomId <= 0)
                throw new Exception("ID phòng không hợp lệ.");

            return dbHelper.UpdateTenantHistoryAsync(historyId, roomId, checkInDate, checkOutDate, status, notes);
        }

        /// <summary>
        /// Xóa lịch sử thuê phòng
        /// </summary>
        public Task<bool> DeleteAsync(int historyId)
        {
            if (historyId <= 0)
                throw new Exception("ID lịch sử không hợp lệ.");

            return dbHelper.DeleteTenantHistoryAsync(historyId);
        }
    }
}
