using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Contracts
{
    /// <summary>
    /// Business Logic Layer cho quản lý Hợp đồng thuê
    /// </summary>
    public class ContractBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả hợp đồng
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetContractsAsync();

        /// <summary>
        /// Lấy hợp đồng theo ID
        /// </summary>
        public async Task<DataRow> GetByIdAsync(int contractId)
        {
            if (contractId <= 0)
                throw new Exception("ID hợp đồng không hợp lệ.");

            DataTable dt = await dbHelper.GetContractByIdAsync(contractId);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0];
            }
            return null;
        }

        /// <summary>
        /// Thêm hợp đồng mới
        /// </summary>
        public Task<int> AddAsync(
            string contractNumber,
            int tenantId,
            int roomId,
            DateTime? signDate,
            DateTime startDate,
            DateTime endDate,
            decimal? rentalPrice,
            decimal? depositRequired,
            string terms,
            string contractPdfPath,
            string status)
        {
            if (string.IsNullOrWhiteSpace(contractNumber))
                throw new Exception("Số hợp đồng không được để trống.");
            if (tenantId <= 0)
                throw new Exception("Khách thuê không hợp lệ.");
            if (roomId <= 0)
                throw new Exception("Phòng không hợp lệ.");
            if (startDate >= endDate)
                throw new Exception("Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");

            return dbHelper.AddContractAsync(
                contractNumber.Trim(),
                tenantId,
                roomId,
                signDate,
                startDate,
                endDate,
                rentalPrice,
                depositRequired,
                terms,
                contractPdfPath,
                status);
        }

        /// <summary>
        /// Cập nhật hợp đồng
        /// </summary>
        public Task<bool> UpdateAsync(
            int contractId,
            string contractNumber,
            int tenantId,
            int roomId,
            DateTime? signDate,
            DateTime startDate,
            DateTime endDate,
            decimal? rentalPrice,
            decimal? depositRequired,
            string terms,
            string contractPdfPath,
            string status)
        {
            if (contractId <= 0)
                throw new Exception("ID hợp đồng không hợp lệ.");
            if (string.IsNullOrWhiteSpace(contractNumber))
                throw new Exception("Số hợp đồng không được để trống.");
            if (tenantId <= 0)
                throw new Exception("Khách thuê không hợp lệ.");
            if (roomId <= 0)
                throw new Exception("Phòng không hợp lệ.");
            if (startDate >= endDate)
                throw new Exception("Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");

            return dbHelper.UpdateContractAsync(
                contractId,
                contractNumber.Trim(),
                tenantId,
                roomId,
                signDate,
                startDate,
                endDate,
                rentalPrice,
                depositRequired,
                terms,
                contractPdfPath,
                status);
        }

        /// <summary>
        /// Xóa hợp đồng
        /// </summary>
        public Task<bool> DeleteAsync(int contractId)
        {
            if (contractId <= 0)
                throw new Exception("ID hợp đồng không hợp lệ.");

            return dbHelper.DeleteContractAsync(contractId);
        }
    }
}
