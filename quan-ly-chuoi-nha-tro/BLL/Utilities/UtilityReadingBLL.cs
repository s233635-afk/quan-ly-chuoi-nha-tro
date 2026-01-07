using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Utilities
{
    /// <summary>
    /// Business Logic Layer cho quản lý Chỉ số tiện ích (điện, nước)
    /// </summary>
    public class UtilityReadingBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả chỉ số tiện ích
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetUtilitiesAsync();

        /// <summary>
        /// Thêm chỉ số tiện ích mới
        /// </summary>
        public Task<int> AddAsync(
            int roomId, 
            int utilityTypeId, 
            DateTime? readingDate, 
            decimal? previousReading, 
            decimal? currentReading, 
            decimal? usageAmount, 
            decimal? unitPrice, 
            decimal? totalCost, 
            string notes)
        {
            if (roomId <= 0)
                throw new Exception("Phòng không hợp lệ.");
            if (utilityTypeId <= 0)
                throw new Exception("Loại tiện ích không hợp lệ.");
            if (currentReading.HasValue && previousReading.HasValue && currentReading < previousReading)
                throw new Exception("Chỉ số hiện tại không được nhỏ hơn chỉ số trước.");

            return dbHelper.AddUtilityReadingAsync(
                roomId, 
                utilityTypeId, 
                readingDate, 
                previousReading, 
                currentReading, 
                usageAmount, 
                unitPrice, 
                totalCost, 
                notes);
        }

        /// <summary>
        /// Cập nhật chỉ số tiện ích
        /// </summary>
        public Task<bool> UpdateAsync(
            int readingId, 
            int roomId, 
            int utilityTypeId, 
            DateTime? readingDate, 
            decimal? previousReading, 
            decimal? currentReading, 
            decimal? usageAmount, 
            decimal? unitPrice, 
            decimal? totalCost, 
            string notes)
        {
            if (readingId <= 0)
                throw new Exception("ID chỉ số không hợp lệ.");
            if (roomId <= 0)
                throw new Exception("Phòng không hợp lệ.");
            if (utilityTypeId <= 0)
                throw new Exception("Loại tiện ích không hợp lệ.");
            if (currentReading.HasValue && previousReading.HasValue && currentReading < previousReading)
                throw new Exception("Chỉ số hiện tại không được nhỏ hơn chỉ số trước.");

            return dbHelper.UpdateUtilityReadingAsync(
                readingId, 
                roomId, 
                utilityTypeId, 
                readingDate, 
                previousReading, 
                currentReading, 
                usageAmount, 
                unitPrice, 
                totalCost, 
                notes);
        }

        /// <summary>
        /// Xóa chỉ số tiện ích
        /// </summary>
        public Task<bool> DeleteAsync(int readingId)
        {
            if (readingId <= 0)
                throw new Exception("ID chỉ số không hợp lệ.");

            return dbHelper.DeleteUtilityReadingAsync(readingId);
        }
    }
}
