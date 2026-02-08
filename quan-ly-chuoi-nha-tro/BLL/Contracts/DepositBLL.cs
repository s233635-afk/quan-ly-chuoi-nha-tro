using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Contracts
{
    /// <summary>
    /// Business Logic Layer cho quản lý Tiền cọc
    /// </summary>
    public class DepositBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả tiền cọc
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetDepositsAsync();

        /// <summary>
        /// Thêm tiền cọc mới
        /// </summary>
        public Task<int> AddAsync(
            int tenantId, 
            int roomId, 
            decimal depositAmount, 
            DateTime? depositDate,
            string depositType, 
            string status, 
            decimal? returnedAmount, 
            DateTime? returnedDate, 
            string notes)
        {
            if (tenantId <= 0)
                throw new Exception("Khách thuê không hợp lệ.");
            if (roomId <= 0)
                throw new Exception("Phòng không hợp lệ.");
            if (depositAmount <= 0)
                throw new Exception("Số tiền cọc phải lớn hơn 0.");

            return dbHelper.AddDepositAsync(
                tenantId, 
                roomId, 
                depositAmount, 
                depositDate,
                depositType, 
                status, 
                returnedAmount, 
                returnedDate, 
                notes);
        }

        /// <summary>
        /// Cập nhật tiền cọc
        /// </summary>
        public Task<bool> UpdateAsync(
            int depositId, 
            int tenantId, 
            int roomId, 
            decimal depositAmount, 
            DateTime? depositDate,
            string depositType, 
            string status, 
            decimal? returnedAmount, 
            DateTime? returnedDate, 
            string notes)
        {
            if (depositId <= 0)
                throw new Exception("ID tiền cọc không hợp lệ.");
            if (tenantId <= 0)
                throw new Exception("Khách thuê không hợp lệ.");
            if (roomId <= 0)
                throw new Exception("Phòng không hợp lệ.");
            if (depositAmount <= 0)
                throw new Exception("Số tiền cọc phải lớn hơn 0.");

            return dbHelper.UpdateDepositAsync(
                depositId, 
                tenantId, 
                roomId, 
                depositAmount, 
                depositDate,
                depositType, 
                status, 
                returnedAmount, 
                returnedDate, 
                notes);
        }

        /// <summary>
        /// Xóa tiền cọc
        /// </summary>
        public Task<bool> DeleteAsync(int depositId)
        {
            if (depositId <= 0)
                throw new Exception("ID tiền cọc không hợp lệ.");

            return dbHelper.DeleteDepositAsync(depositId);
        }
    }
}
