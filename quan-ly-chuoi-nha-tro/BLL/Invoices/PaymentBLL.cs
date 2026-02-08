using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Invoices
{
    /// <summary>
    /// Business Logic Layer cho quản lý Thanh toán
    /// </summary>
    public class PaymentBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả thanh toán
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetPaymentsAsync();

        /// <summary>
        /// Lấy view thanh toán (có thông tin chi tiết)
        /// </summary>
        public Task<DataTable> GetPaymentsViewAsync() => dbHelper.GetPaymentsViewAsync();

        /// <summary>
        /// Lấy thanh toán theo hóa đơn
        /// </summary>
        public Task<DataTable> GetByInvoiceAsync(int invoiceId)
        {
            if (invoiceId <= 0)
                throw new Exception("ID hóa đơn không hợp lệ.");

            return dbHelper.GetPaymentsByInvoiceAsync(invoiceId);
        }

        /// <summary>
        /// Thêm thanh toán mới
        /// </summary>
        public Task<int> AddAsync(
            int invoiceId, 
            DateTime paymentDate, 
            decimal paymentAmount, 
            string paymentMethod, 
            string transactionReference, 
            string notes)
        {
            if (invoiceId <= 0)
                throw new Exception("ID hóa đơn không hợp lệ.");
            if (paymentAmount <= 0)
                throw new Exception("Số tiền thanh toán phải lớn hơn 0.");

            return dbHelper.AddPaymentAsync(
                invoiceId, 
                paymentDate, 
                paymentAmount, 
                paymentMethod, 
                transactionReference, 
                notes);
        }

        /// <summary>
        /// Cập nhật thanh toán
        /// </summary>
        public Task<bool> UpdateAsync(
            int paymentId, 
            DateTime paymentDate, 
            decimal paymentAmount, 
            string paymentMethod, 
            string transactionReference, 
            string notes)
        {
            if (paymentId <= 0)
                throw new Exception("ID thanh toán không hợp lệ.");
            if (paymentAmount <= 0)
                throw new Exception("Số tiền thanh toán phải lớn hơn 0.");

            return dbHelper.UpdatePaymentAsync(
                paymentId, 
                paymentDate, 
                paymentAmount, 
                paymentMethod, 
                transactionReference, 
                notes);
        }

        /// <summary>
        /// Xóa thanh toán
        /// </summary>
        public Task<bool> DeleteAsync(int paymentId)
        {
            if (paymentId <= 0)
                throw new Exception("ID thanh toán không hợp lệ.");

            return dbHelper.DeletePaymentAsync(paymentId);
        }


    }
}
