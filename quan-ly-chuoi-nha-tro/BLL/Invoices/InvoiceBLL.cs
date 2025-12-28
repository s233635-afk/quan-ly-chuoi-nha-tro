using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Invoices
{
    /// <summary>
    /// Business Logic Layer cho quản lý Hóa đơn
    /// </summary>
    public class InvoiceBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả hóa đơn
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetInvoicesAsync();

        /// <summary>
        /// Lấy view hóa đơn (có thông tin chi tiết)
        /// </summary>
        public Task<DataTable> GetInvoicesViewAsync() => dbHelper.GetInvoicesViewAsync();

        /// <summary>
        /// Thêm hóa đơn mới
        /// </summary>
        public Task<int> AddAsync(
            string invoiceNumber,
            int tenantId,
            int roomId,
            DateTime invoiceDate,
            DateTime? fromDate,
            DateTime? toDate,
            decimal rentalCost,
            decimal utilityCost,
            decimal otherCost,
            DateTime? dueDate,
            decimal? taxRate = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                throw new Exception("Số hóa đơn không được để trống.");
            if (tenantId <= 0)
                throw new Exception("Khách thuê không hợp lệ.");
            if (roomId <= 0)
                throw new Exception("Phòng không hợp lệ.");
            if (rentalCost < 0 || utilityCost < 0 || otherCost < 0)
                throw new Exception("Số tiền không được âm.");

            return dbHelper.AddInvoiceAsync(
                invoiceNumber.Trim(),
                tenantId,
                roomId,
                invoiceDate,
                fromDate,
                toDate,
                rentalCost,
                utilityCost,
                otherCost,
                dueDate,
                taxRate);
        }

        /// <summary>
        /// Cập nhật hóa đơn
        /// </summary>
        public Task<bool> UpdateAsync(
            int invoiceId,
            string invoiceNumber,
            int tenantId,
            int roomId,
            DateTime invoiceDate,
            DateTime? fromDate,
            DateTime? toDate,
            decimal rentalCost,
            decimal utilityCost,
            decimal otherCost,
            DateTime? dueDate,
            decimal? taxRate = null)
        {
            if (invoiceId <= 0)
                throw new Exception("ID hóa đơn không hợp lệ.");
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                throw new Exception("Số hóa đơn không được để trống.");
            if (tenantId <= 0)
                throw new Exception("Khách thuê không hợp lệ.");
            if (roomId <= 0)
                throw new Exception("Phòng không hợp lệ.");
            if (rentalCost < 0 || utilityCost < 0 || otherCost < 0)
                throw new Exception("Số tiền không được âm.");

            return dbHelper.UpdateInvoiceAsync(
                invoiceId,
                invoiceNumber.Trim(),
                tenantId,
                roomId,
                invoiceDate,
                fromDate,
                toDate,
                rentalCost,
                utilityCost,
                otherCost,
                dueDate,
                taxRate);
        }

        /// <summary>
        /// Xóa hóa đơn
        /// </summary>
        public Task<bool> DeleteAsync(int invoiceId, bool deletePaymentsFirst = true)
        {
            if (invoiceId <= 0)
                throw new Exception("ID hóa đơn không hợp lệ.");

            return dbHelper.DeleteInvoiceAsync(invoiceId, deletePaymentsFirst);
        }

        /// <summary>
        /// Tạo hóa đơn tháng tự động
        /// </summary>
        public Task<int> GenerateMonthlyInvoicesAsync(
            int year, 
            int month, 
            DateTime? invoiceDate = null, 
            int? dueDay = null, 
            decimal? taxRateOverride = null)
        {
            if (year < 2000 || year > 2100)
                throw new Exception("Năm không hợp lệ.");
            if (month < 1 || month > 12)
                throw new Exception("Tháng không hợp lệ.");

            return dbHelper.GenerateMonthlyInvoicesAsync(year, month, invoiceDate, dueDay, taxRateOverride);
        }
    }
}
