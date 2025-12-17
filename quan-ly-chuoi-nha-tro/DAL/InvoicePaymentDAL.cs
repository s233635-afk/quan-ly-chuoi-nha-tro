using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace QuanLyNhaTro.DAL
{
    public partial class DatabaseHelper
    {
        #region Invoices & Payments (Full module)

        public Task<DataTable> GetInvoicesViewAsync()
        {
            return GetTableSafeAsync(
                "Invoices",
                null,
                @"SELECT i.InvoiceId,
                         i.InvoiceNumber,
                         i.TenantId,
                         t.FullName AS TenantName,
                         i.RoomId,
                         r.RoomNumber,
                         r.BranchId,
                         i.InvoiceDate,
                         i.FromDate,
                         i.ToDate,
                         i.RentalCost,
                         i.UtilityCost,
                         i.OtherCost,
                         i.TotalAmount,
                         i.PaidAmount,
                         i.RemainingAmount,
                         CASE
                            WHEN ISNULL(i.RemainingAmount, (ISNULL(i.TotalAmount,0) - ISNULL(i.PaidAmount,0))) <= 0 THEN N'Paid'
                            WHEN i.DueDate IS NOT NULL AND i.DueDate < CAST(GETDATE() AS DATE) THEN N'Overdue'
                            WHEN ISNULL(i.PaidAmount,0) > 0 THEN N'PartialPaid'
                            ELSE ISNULL(NULLIF(i.Status, ''), N'Issued')
                         END AS Status,
                         i.DueDate,
                         i.CreatedDate,
                         i.UpdatedDate
                  FROM Invoices i
                  LEFT JOIN Tenants t ON t.TenantId = i.TenantId
                  LEFT JOIN Rooms r ON r.RoomId = i.RoomId
                  ORDER BY i.InvoiceDate DESC, i.InvoiceId DESC",
                "SELECT * FROM Invoices"
            );
        }

        public Task<DataTable> GetPaymentsViewAsync()
        {
            return GetTableSafeAsync(
                "Payments",
                null,
                @"SELECT p.PaymentId,
                         p.InvoiceId,
                         i.InvoiceNumber,
                         i.TenantId,
                         t.FullName AS TenantName,
                         i.RoomId,
                         r.RoomNumber,
                         r.BranchId,
                         p.PaymentDate,
                         p.PaymentAmount,
                         p.PaymentMethod,
                         p.TransactionReference,
                         p.Notes,
                         p.CreatedDate
                  FROM Payments p
                  LEFT JOIN Invoices i ON i.InvoiceId = p.InvoiceId
                  LEFT JOIN Tenants t ON t.TenantId = i.TenantId
                  LEFT JOIN Rooms r ON r.RoomId = i.RoomId
                  ORDER BY p.PaymentDate DESC, p.PaymentId DESC",
                "SELECT * FROM Payments"
            );
        }

        public Task<DataTable> GetPaymentsByInvoiceAsync(int invoiceId)
        {
            return GetTableSafeAsync(
                "Payments",
                cmd => cmd.Parameters.AddWithValue("@InvoiceId", invoiceId),
                @"SELECT PaymentId,
                         InvoiceId,
                         PaymentDate,
                         PaymentAmount,
                         PaymentMethod,
                         TransactionReference,
                         Notes,
                         CreatedDate
                  FROM Payments
                  WHERE InvoiceId = @InvoiceId
                  ORDER BY PaymentDate DESC, PaymentId DESC",
                "SELECT * FROM Payments WHERE InvoiceId = @InvoiceId"
            );
        }

        public async Task<int> AddInvoiceAsync(
            string invoiceNumber,
            int tenantId,
            int roomId,
            DateTime invoiceDate,
            DateTime? fromDate,
            DateTime? toDate,
            decimal rentalCost,
            decimal utilityCost,
            decimal otherCost,
            DateTime? dueDate)
        {
            if (!await TableExistsAsync("Invoices"))
                throw new Exception("Bảng Invoices không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        string invNo = string.IsNullOrWhiteSpace(invoiceNumber)
                            ? GenerateInvoiceNumber(invoiceDate)
                            : invoiceNumber.Trim();

                        var total = rentalCost + utilityCost + otherCost;
                        var paid = 0m;
                        var remaining = total - paid;
                        var status = ComputeInvoiceStatus(dueDate, paid, remaining);

                        const string sql = @"
                            INSERT INTO Invoices
                                (InvoiceNumber, TenantId, RoomId, InvoiceDate, FromDate, ToDate,
                                 RentalCost, UtilityCost, OtherCost, TotalAmount, PaidAmount, RemainingAmount, Status, DueDate, CreatedDate, UpdatedDate)
                            VALUES
                                (@InvoiceNumber, @TenantId, @RoomId, @InvoiceDate, @FromDate, @ToDate,
                                 @RentalCost, @UtilityCost, @OtherCost, @TotalAmount, @PaidAmount, @RemainingAmount, @Status, @DueDate, GETDATE(), GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using (var cmd = new SqlCommand(sql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceNumber", invNo);
                            cmd.Parameters.AddWithValue("@TenantId", tenantId);
                            cmd.Parameters.AddWithValue("@RoomId", roomId);
                            cmd.Parameters.AddWithValue("@InvoiceDate", invoiceDate.Date);
                            cmd.Parameters.AddWithValue("@FromDate", fromDate.HasValue ? (object)fromDate.Value.Date : DBNull.Value);
                            cmd.Parameters.AddWithValue("@ToDate", toDate.HasValue ? (object)toDate.Value.Date : DBNull.Value);
                            cmd.Parameters.AddWithValue("@RentalCost", rentalCost);
                            cmd.Parameters.AddWithValue("@UtilityCost", utilityCost);
                            cmd.Parameters.AddWithValue("@OtherCost", otherCost);
                            cmd.Parameters.AddWithValue("@TotalAmount", total);
                            cmd.Parameters.AddWithValue("@PaidAmount", paid);
                            cmd.Parameters.AddWithValue("@RemainingAmount", remaining);
                            cmd.Parameters.AddWithValue("@Status", status);
                            cmd.Parameters.AddWithValue("@DueDate", dueDate.HasValue ? (object)dueDate.Value.Date : DBNull.Value);

                            var result = await cmd.ExecuteScalarAsync();
                            tx.Commit();
                            return result != null ? Convert.ToInt32(result) : 0;
                        }
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> UpdateInvoiceAsync(
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
            DateTime? dueDate)
        {
            if (!await TableExistsAsync("Invoices"))
                throw new Exception("Bảng Invoices không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        decimal paid = await GetInvoicePaidAmountAsync(conn, tx, invoiceId);
                        var total = rentalCost + utilityCost + otherCost;
                        var remaining = total - paid;
                        var status = ComputeInvoiceStatus(dueDate, paid, remaining);

                        const string sql = @"
                            UPDATE Invoices SET
                                InvoiceNumber = @InvoiceNumber,
                                TenantId = @TenantId,
                                RoomId = @RoomId,
                                InvoiceDate = @InvoiceDate,
                                FromDate = @FromDate,
                                ToDate = @ToDate,
                                RentalCost = @RentalCost,
                                UtilityCost = @UtilityCost,
                                OtherCost = @OtherCost,
                                TotalAmount = @TotalAmount,
                                PaidAmount = @PaidAmount,
                                RemainingAmount = @RemainingAmount,
                                Status = @Status,
                                DueDate = @DueDate,
                                UpdatedDate = GETDATE()
                            WHERE InvoiceId = @InvoiceId;";

                        using (var cmd = new SqlCommand(sql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                            cmd.Parameters.AddWithValue("@InvoiceNumber", string.IsNullOrWhiteSpace(invoiceNumber) ? (object)DBNull.Value : invoiceNumber.Trim());
                            cmd.Parameters.AddWithValue("@TenantId", tenantId);
                            cmd.Parameters.AddWithValue("@RoomId", roomId);
                            cmd.Parameters.AddWithValue("@InvoiceDate", invoiceDate.Date);
                            cmd.Parameters.AddWithValue("@FromDate", fromDate.HasValue ? (object)fromDate.Value.Date : DBNull.Value);
                            cmd.Parameters.AddWithValue("@ToDate", toDate.HasValue ? (object)toDate.Value.Date : DBNull.Value);
                            cmd.Parameters.AddWithValue("@RentalCost", rentalCost);
                            cmd.Parameters.AddWithValue("@UtilityCost", utilityCost);
                            cmd.Parameters.AddWithValue("@OtherCost", otherCost);
                            cmd.Parameters.AddWithValue("@TotalAmount", total);
                            cmd.Parameters.AddWithValue("@PaidAmount", paid);
                            cmd.Parameters.AddWithValue("@RemainingAmount", remaining);
                            cmd.Parameters.AddWithValue("@Status", status);
                            cmd.Parameters.AddWithValue("@DueDate", dueDate.HasValue ? (object)dueDate.Value.Date : DBNull.Value);

                            int affected = await cmd.ExecuteNonQueryAsync();
                            tx.Commit();
                            return affected > 0;
                        }
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> DeleteInvoiceAsync(int invoiceId, bool deletePaymentsFirst)
        {
            if (!await TableExistsAsync("Invoices"))
                throw new Exception("Bảng Invoices không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        if (deletePaymentsFirst && await TableExistsAsync("Payments"))
                        {
                            using (var cmdDelPay = new SqlCommand("DELETE FROM Payments WHERE InvoiceId = @InvoiceId", conn, tx))
                            {
                                cmdDelPay.Parameters.AddWithValue("@InvoiceId", invoiceId);
                                await cmdDelPay.ExecuteNonQueryAsync();
                            }
                        }

                        using (var cmd = new SqlCommand("DELETE FROM Invoices WHERE InvoiceId = @InvoiceId", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                            int affected = await cmd.ExecuteNonQueryAsync();
                            tx.Commit();
                            return affected > 0;
                        }
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<int> AddPaymentAsync(
            int invoiceId,
            DateTime paymentDate,
            decimal paymentAmount,
            string paymentMethod,
            string transactionReference,
            string notes)
        {
            if (!await TableExistsAsync("Payments"))
                throw new Exception("Bảng Payments không tồn tại.");
            if (paymentAmount <= 0) throw new Exception("Số tiền thanh toán phải > 0.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        var inv = await GetInvoiceAmountsAsync(conn, tx, invoiceId);
                        decimal total = inv.total;
                        decimal paid = inv.paid;
                        decimal remaining = inv.remaining;
                        DateTime? dueDate = inv.dueDate;

                        var newPaid = paid + paymentAmount;
                        var newRemaining = total - newPaid;
                        var status = ComputeInvoiceStatus(dueDate, newPaid, newRemaining);

                        const string sqlPay = @"
                            INSERT INTO Payments
                                (InvoiceId, PaymentDate, PaymentAmount, PaymentMethod, TransactionReference, Notes, CreatedDate)
                            VALUES
                                (@InvoiceId, @PaymentDate, @PaymentAmount, @PaymentMethod, @TransactionReference, @Notes, GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int paymentId;
                        using (var cmd = new SqlCommand(sqlPay, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                            cmd.Parameters.AddWithValue("@PaymentDate", paymentDate.Date);
                            cmd.Parameters.AddWithValue("@PaymentAmount", paymentAmount);
                            cmd.Parameters.AddWithValue("@PaymentMethod", string.IsNullOrWhiteSpace(paymentMethod) ? (object)DBNull.Value : paymentMethod.Trim());
                            cmd.Parameters.AddWithValue("@TransactionReference", string.IsNullOrWhiteSpace(transactionReference) ? (object)DBNull.Value : transactionReference.Trim());
                            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes.Trim());
                            var result = await cmd.ExecuteScalarAsync();
                            paymentId = result != null ? Convert.ToInt32(result) : 0;
                        }

                        const string sqlInv = @"
                            UPDATE Invoices SET
                                PaidAmount = @PaidAmount,
                                RemainingAmount = @RemainingAmount,
                                Status = @Status,
                                UpdatedDate = GETDATE()
                            WHERE InvoiceId = @InvoiceId;";

                        using (var cmd = new SqlCommand(sqlInv, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                            cmd.Parameters.AddWithValue("@PaidAmount", newPaid);
                            cmd.Parameters.AddWithValue("@RemainingAmount", newRemaining);
                            cmd.Parameters.AddWithValue("@Status", status);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        tx.Commit();
                        return paymentId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> UpdatePaymentAsync(
            int paymentId,
            DateTime paymentDate,
            decimal paymentAmount,
            string paymentMethod,
            string transactionReference,
            string notes)
        {
            if (!await TableExistsAsync("Payments"))
                throw new Exception("Bảng Payments không tồn tại.");
            if (paymentAmount <= 0) throw new Exception("Số tiền thanh toán phải > 0.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int invoiceId;
                        using (var cmd = new SqlCommand("SELECT InvoiceId FROM Payments WHERE PaymentId = @PaymentId", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                            var result = await cmd.ExecuteScalarAsync();
                            if (result == null || result == DBNull.Value)
                                throw new Exception("Không tìm thấy thanh toán.");
                            invoiceId = Convert.ToInt32(result);
                        }

                        const string sql = @"
                            UPDATE Payments SET
                                PaymentDate = @PaymentDate,
                                PaymentAmount = @PaymentAmount,
                                PaymentMethod = @PaymentMethod,
                                TransactionReference = @TransactionReference,
                                Notes = @Notes
                            WHERE PaymentId = @PaymentId;";

                        using (var cmd = new SqlCommand(sql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                            cmd.Parameters.AddWithValue("@PaymentDate", paymentDate.Date);
                            cmd.Parameters.AddWithValue("@PaymentAmount", paymentAmount);
                            cmd.Parameters.AddWithValue("@PaymentMethod", string.IsNullOrWhiteSpace(paymentMethod) ? (object)DBNull.Value : paymentMethod.Trim());
                            cmd.Parameters.AddWithValue("@TransactionReference", string.IsNullOrWhiteSpace(transactionReference) ? (object)DBNull.Value : transactionReference.Trim());
                            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes.Trim());
                            int affected = await cmd.ExecuteNonQueryAsync();
                            if (affected <= 0)
                                throw new Exception("Không thể cập nhật thanh toán.");
                        }

                        await RecalculateInvoiceFromPaymentsAsync(conn, tx, invoiceId);

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            if (!await TableExistsAsync("Payments"))
                throw new Exception("Bảng Payments không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int invoiceId;
                        using (var cmd = new SqlCommand("SELECT InvoiceId FROM Payments WHERE PaymentId = @PaymentId", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                            var result = await cmd.ExecuteScalarAsync();
                            if (result == null || result == DBNull.Value)
                                throw new Exception("Không tìm thấy thanh toán.");
                            invoiceId = Convert.ToInt32(result);
                        }

                        using (var cmd = new SqlCommand("DELETE FROM Payments WHERE PaymentId = @PaymentId", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                            int affected = await cmd.ExecuteNonQueryAsync();
                            if (affected <= 0)
                                throw new Exception("Không thể xóa thanh toán.");
                        }

                        await RecalculateInvoiceFromPaymentsAsync(conn, tx, invoiceId);

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status)
        {
            if (!await TableExistsAsync("Payments"))
                throw new Exception("Bảng Payments không tồn tại.");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int invoiceId;
                        using (var cmd = new SqlCommand("SELECT InvoiceId FROM Payments WHERE PaymentId = @PaymentId", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                            var result = await cmd.ExecuteScalarAsync();
                            if (result == null || result == DBNull.Value)
                                throw new Exception("Không tìm thấy thanh toán.");
                            invoiceId = Convert.ToInt32(result);
                        }

                        using (var cmd = new SqlCommand("UPDATE Payments SET Status = @Status, UpdatedDate = GETDATE() WHERE PaymentId = @PaymentId", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                            cmd.Parameters.AddWithValue("@Status", status ?? "");
                            int affected = await cmd.ExecuteNonQueryAsync();
                            if (affected <= 0)
                                throw new Exception("Không thể cập nhật trạng thái thanh toán.");
                        }

                        await RecalculateInvoiceFromPaymentsAsync(conn, tx, invoiceId);

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<int> GenerateMonthlyInvoicesAsync(int year, int month, DateTime? invoiceDate = null, int? dueDay = null)
        {
            if (!await TableExistsAsync("Contracts"))
                throw new Exception("Bảng Contracts không tồn tại.");
            if (!await TableExistsAsync("Invoices"))
                throw new Exception("Bảng Invoices không tồn tại.");

            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var invDate = (invoiceDate ?? DateTime.Today).Date;

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int dueDayValue = dueDay ?? await GetInvoiceDueDayAsync(conn, tx) ?? 10;
                        int safeDueDay = Math.Max(1, Math.Min(DateTime.DaysInMonth(year, month), dueDayValue));
                        var dueDate = new DateTime(year, month, safeDueDay);

                        var utilityByRoom = new Dictionary<int, decimal>();
                        if (await TableExistsAsync("UtilityReadings"))
                        {
                            const string sqlUtil = @"
                                SELECT RoomId, CAST(ISNULL(SUM(ISNULL(TotalCost,0)),0) AS DECIMAL(18,2)) AS UtilityCost
                                FROM UtilityReadings
                                WHERE ReadingDate >= @FromDate AND ReadingDate <= @ToDate
                                GROUP BY RoomId;";
                            using (var cmd = new SqlCommand(sqlUtil, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@FromDate", firstDay);
                                cmd.Parameters.AddWithValue("@ToDate", lastDay);
                                using (var reader = await cmd.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        int roomId = reader.GetInt32(0);
                                        decimal cost = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
                                        utilityByRoom[roomId] = cost;
                                    }
                                }
                            }
                        }

                        const string sqlContracts = @"
                            SELECT c.TenantId,
                                   c.RoomId,
                                   CAST(COALESCE(c.RentalPrice, r.RoomPrice, rt.DefaultPrice, 0) AS DECIMAL(18,2)) AS RentalCost
                            FROM Contracts c
                            LEFT JOIN Rooms r ON r.RoomId = c.RoomId
                            LEFT JOIN RoomTypes rt ON rt.RoomTypeId = r.RoomTypeId
                            WHERE ISNULL(NULLIF(c.Status,''),'Active') IN ('Active','Extended')
                              AND c.StartDate <= @ToDate
                              AND c.EndDate >= @FromDate;";

                        int created = 0;
                        using (var cmd = new SqlCommand(sqlContracts, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@FromDate", firstDay);
                            cmd.Parameters.AddWithValue("@ToDate", lastDay);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    int tenantId = reader.GetInt32(0);
                                    int roomId = reader.GetInt32(1);
                                    decimal rentalCost = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);

                                    if (await InvoiceExistsAsync(conn, tx, tenantId, roomId, firstDay, lastDay))
                                        continue;

                                    decimal utilityCost = utilityByRoom.TryGetValue(roomId, out var u) ? u : 0m;
                                    decimal otherCost = 0m;
                                    decimal total = rentalCost + utilityCost + otherCost;

                                    string invoiceNumber = await GenerateMonthlyInvoiceNumberAsync(conn, tx, year, month);
                                    string status = ComputeInvoiceStatus(dueDate, 0m, total);

                                    const string sqlInsert = @"
                                        INSERT INTO Invoices
                                            (InvoiceNumber, TenantId, RoomId, InvoiceDate, FromDate, ToDate,
                                             RentalCost, UtilityCost, OtherCost, TotalAmount, PaidAmount, RemainingAmount, Status, DueDate, CreatedDate, UpdatedDate)
                                        VALUES
                                            (@InvoiceNumber, @TenantId, @RoomId, @InvoiceDate, @FromDate, @ToDate,
                                             @RentalCost, @UtilityCost, @OtherCost, @TotalAmount, 0, @RemainingAmount, @Status, @DueDate, GETDATE(), GETDATE());";

                                    using (var cmdIns = new SqlCommand(sqlInsert, conn, tx))
                                    {
                                        cmdIns.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                                        cmdIns.Parameters.AddWithValue("@TenantId", tenantId);
                                        cmdIns.Parameters.AddWithValue("@RoomId", roomId);
                                        cmdIns.Parameters.AddWithValue("@InvoiceDate", invDate);
                                        cmdIns.Parameters.AddWithValue("@FromDate", firstDay);
                                        cmdIns.Parameters.AddWithValue("@ToDate", lastDay);
                                        cmdIns.Parameters.AddWithValue("@RentalCost", rentalCost);
                                        cmdIns.Parameters.AddWithValue("@UtilityCost", utilityCost);
                                        cmdIns.Parameters.AddWithValue("@OtherCost", otherCost);
                                        cmdIns.Parameters.AddWithValue("@TotalAmount", total);
                                        cmdIns.Parameters.AddWithValue("@RemainingAmount", total);
                                        cmdIns.Parameters.AddWithValue("@Status", status);
                                        cmdIns.Parameters.AddWithValue("@DueDate", dueDate);
                                        await cmdIns.ExecuteNonQueryAsync();
                                        created++;
                                    }
                                }
                            }
                        }

                        tx.Commit();
                        return created;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private static string ComputeInvoiceStatus(DateTime? dueDate, decimal paidAmount, decimal remainingAmount)
        {
            if (remainingAmount <= 0) return "Paid";
            if (dueDate.HasValue && dueDate.Value.Date < DateTime.Today) return "Overdue";
            if (paidAmount > 0) return "PartialPaid";
            return "Issued";
        }

        private static string GenerateInvoiceNumber(DateTime invoiceDate)
        {
            return $"HD-{invoiceDate:yyyyMMdd}-{DateTime.Now:HHmmss}";
        }

        private async Task<decimal> GetInvoicePaidAmountAsync(SqlConnection conn, SqlTransaction tx, int invoiceId)
        {
            using (var cmd = new SqlCommand("SELECT ISNULL(PaidAmount,0) FROM Invoices WHERE InvoiceId = @InvoiceId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                var result = await cmd.ExecuteScalarAsync();
                return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
            }
        }

        private async Task<(decimal total, decimal paid, decimal remaining, DateTime? dueDate)> GetInvoiceAmountsAsync(
            SqlConnection conn,
            SqlTransaction tx,
            int invoiceId)
        {
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(TotalAmount,0), ISNULL(PaidAmount,0), ISNULL(RemainingAmount,0), DueDate
                FROM Invoices WHERE InvoiceId = @InvoiceId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        throw new Exception("Không tìm thấy hóa đơn.");
                    decimal total = reader.IsDBNull(0) ? 0m : reader.GetDecimal(0);
                    decimal paid = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
                    decimal remaining = reader.IsDBNull(2) ? (total - paid) : reader.GetDecimal(2);
                    DateTime? dueDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3);
                    return (total, paid, remaining, dueDate);
                }
            }
        }

        private async Task RecalculateInvoiceFromPaymentsAsync(SqlConnection conn, SqlTransaction tx, int invoiceId)
        {
            decimal total;
            DateTime? dueDate;

            using (var cmd = new SqlCommand("SELECT ISNULL(TotalAmount,0), DueDate FROM Invoices WHERE InvoiceId = @InvoiceId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        throw new Exception("Không tìm thấy hóa đơn.");
                    total = reader.IsDBNull(0) ? 0m : reader.GetDecimal(0);
                    dueDate = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
                }
            }

            decimal paid = 0m;
            using (var cmd = new SqlCommand("SELECT ISNULL(SUM(PaymentAmount),0) FROM Payments WHERE InvoiceId = @InvoiceId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                var result = await cmd.ExecuteScalarAsync();
                paid = result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
            }

            decimal remaining = total - paid;
            string status = ComputeInvoiceStatus(dueDate, paid, remaining);

            using (var cmd = new SqlCommand(@"
                UPDATE Invoices SET
                    PaidAmount = @PaidAmount,
                    RemainingAmount = @RemainingAmount,
                    Status = @Status,
                    UpdatedDate = GETDATE()
                WHERE InvoiceId = @InvoiceId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                cmd.Parameters.AddWithValue("@PaidAmount", paid);
                cmd.Parameters.AddWithValue("@RemainingAmount", remaining);
                cmd.Parameters.AddWithValue("@Status", status);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task<bool> InvoiceExistsAsync(SqlConnection conn, SqlTransaction tx, int tenantId, int roomId, DateTime fromDate, DateTime toDate)
        {
            const string sql = @"
                SELECT COUNT(*) FROM Invoices
                WHERE TenantId = @TenantId AND RoomId = @RoomId AND FromDate = @FromDate AND ToDate = @ToDate;";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@RoomId", roomId);
                cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }

        private async Task<string> GenerateMonthlyInvoiceNumberAsync(SqlConnection conn, SqlTransaction tx, int year, int month)
        {
            string prefix = $"HD-{year}{month:00}-";
            const string sql = @"
                SELECT ISNULL(MAX(TRY_CONVERT(INT, RIGHT(InvoiceNumber, 4))), 0)
                FROM Invoices
                WHERE InvoiceNumber LIKE @Prefix + '%';";

            int maxSeq = 0;
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@Prefix", prefix);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    maxSeq = Convert.ToInt32(result);
            }

            return prefix + (maxSeq + 1).ToString("0000");
        }

        private async Task<int?> GetInvoiceDueDayAsync(SqlConnection conn, SqlTransaction tx)
        {
            if (!await TableExistsAsync("SystemSettings"))
                return null;

            using (var cmd = new SqlCommand("SELECT SettingValue FROM SystemSettings WHERE SettingKey = @Key", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Key", "InvoiceDueDay");
                var result = await cmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value) return null;
                if (int.TryParse(result.ToString(), out var v)) return v;
                return null;
            }
        }

        #endregion
    }
}
