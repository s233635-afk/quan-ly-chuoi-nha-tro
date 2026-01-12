using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Text;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmPaymentEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _invoiceRow;
        private readonly DataRow _existingPayment;

        private Label lblInvoice;
        private Label lblAmounts;
        private DateTimePicker dtPaymentDate;
        private NumericUpDown numAmount;
        private Button btnPayFull;
        private ComboBox cboMethod;
        private TextBox txtReference;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        public FrmPaymentEditor(AdminDataBLL bll, DataRow invoiceRow, DataRow existingPayment = null)
        {
            _bll = bll;
            _invoiceRow = invoiceRow;
            _existingPayment = existingPayment;
            InitializeComponent();
            Load += (s, e) => LoadInfo();
        }

        private void InitializeComponent()
        {
            Text = _existingPayment == null ? "Thu tiền / Ghi nhận thanh toán" : "Cập nhật thanh toán";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(640, 420);
            BackColor = Color.White;

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White
            };

            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 18, 18, 10),
                AutoScroll = true,
                BackColor = Color.White
            };

            int labelWidth = 160;
            int inputWidth = 420;
            int top = 10;
            int left = 6;
            int line = 34;

            Label MakeLabel(string text, int y) => new Label
            {
                Text = text,
                Location = new Point(left, y),
                Width = labelWidth,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Control MakeInput(Control ctl, int y)
            {
                ctl.Location = new Point(left + labelWidth, y);
                ctl.Width = inputWidth;
                return ctl;
            }

            lblInvoice = new Label { AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Location = new Point(left, top) };
            lblAmounts = new Label { AutoSize = true, ForeColor = Color.DimGray, Location = new Point(left, top + 28) };
            pnlBody.Controls.Add(lblInvoice);
            pnlBody.Controls.Add(lblAmounts);

            top += 68;

            dtPaymentDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            numAmount = new NumericUpDown { Minimum = 0, Maximum = 100000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            btnPayFull = new Button { Text = "Thu đủ", Width = 90, Height = 28 };
            btnPayFull.Click += (s, e) =>
            {
                if (numAmount.Maximum > 0)
                    numAmount.Value = numAmount.Maximum;
            };
            btnPayFull.FlatStyle = FlatStyle.Flat;
            btnPayFull.FlatAppearance.BorderSize = 1;

            cboMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboMethod.Items.AddRange(new object[] { "Cash", "Transfer", "QR", "Check" });
            if (cboMethod.Items.Count > 0) cboMethod.SelectedIndex = 0;

            txtReference = new TextBox();
            txtNotes = new TextBox { Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };

            pnlBody.Controls.Add(MakeLabel("Ngày thanh toán (*)", top));
            pnlBody.Controls.Add(MakeInput(dtPaymentDate, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Số tiền (*)", top));
            var pnlAmount = new Panel { Location = new Point(left + labelWidth, top), Width = inputWidth, Height = 30 };
            numAmount.Parent = pnlAmount;
            numAmount.Location = new Point(0, 0);
            numAmount.Width = inputWidth - btnPayFull.Width - 10;
            btnPayFull.Parent = pnlAmount;
            btnPayFull.Location = new Point(inputWidth - btnPayFull.Width, 1);
            pnlAmount.Resize += (s, e) =>
            {
                numAmount.Width = pnlAmount.Width - btnPayFull.Width - 10;
                btnPayFull.Location = new Point(pnlAmount.Width - btnPayFull.Width, 1);
            };
            pnlBody.Controls.Add(pnlAmount);
            top += line;

            pnlBody.Controls.Add(MakeLabel("Hình thức", top));
            pnlBody.Controls.Add(MakeInput(cboMethod, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Mã giao dịch", top));
            pnlBody.Controls.Add(MakeInput(txtReference, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ghi chú", top));
            pnlBody.Controls.Add(MakeInput(txtNotes, top));

            btnSave = new Button
            {
                Text = _existingPayment == null ? "Lưu" : "Cập nhật",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnSave.Click += async (s, e) => await SaveAsync();
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.BackColor = Color.FromArgb(0, 122, 204);
            btnSave.ForeColor = Color.White;

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 1;

            btnCancel.Location = new Point(pnlBottom.ClientSize.Width - btnCancel.Width, 12);
            btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            pnlBottom.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Resize += (s, e) =>
            {
                btnCancel.Location = new Point(pnlBottom.ClientSize.Width - btnCancel.Width, 12);
                btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            };

            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }

        private void LoadInfo()
        {
            if (_invoiceRow == null) return;

            int invoiceId = Convert.ToInt32(_invoiceRow["InvoiceId"]);
            string invoiceNo = _invoiceRow.Table.Columns.Contains("InvoiceNumber") ? _invoiceRow["InvoiceNumber"]?.ToString() : invoiceId.ToString();
            string tenant = _invoiceRow.Table.Columns.Contains("TenantName") ? _invoiceRow["TenantName"]?.ToString() : null;
            string room = _invoiceRow.Table.Columns.Contains("RoomNumber") ? _invoiceRow["RoomNumber"]?.ToString() : null;

            decimal total = ReadDecimal(_invoiceRow, "TotalAmount");
            decimal paid = ReadDecimal(_invoiceRow, "PaidAmount");
            decimal remaining = ReadDecimal(_invoiceRow, "RemainingAmount");

            lblInvoice.Text = $"Hóa đơn: {invoiceNo} | {tenant} | Phòng {room}";
            lblAmounts.Text = $"Tổng: {total:N0} | Đã thu: {paid:N0} | Còn nợ: {remaining:N0}";

            decimal oldAmount = 0m;
            if (_existingPayment != null && _existingPayment.Table.Columns.Contains("PaymentAmount"))
                oldAmount = ReadDecimal(_existingPayment, "PaymentAmount");

            decimal maxAllowed = _existingPayment == null ? remaining : (remaining + oldAmount);
            if (maxAllowed > 0)
            {
                numAmount.Maximum = Math.Max(1, maxAllowed);
                numAmount.Value = Math.Min(numAmount.Maximum, _existingPayment == null ? maxAllowed : oldAmount);
            }

            if (_existingPayment != null)
            {
                if (_existingPayment.Table.Columns.Contains("PaymentDate") &&
                    DateTime.TryParse(_existingPayment["PaymentDate"]?.ToString(), out var pd))
                {
                    dtPaymentDate.Value = pd.Date;
                }

                string method = _existingPayment.Table.Columns.Contains("PaymentMethod") ? _existingPayment["PaymentMethod"]?.ToString() : null;
                if (!string.IsNullOrWhiteSpace(method))
                {
                    int idx = cboMethod.FindStringExact(method);
                    if (idx >= 0) cboMethod.SelectedIndex = idx;
                }

                txtReference.Text = _existingPayment.Table.Columns.Contains("TransactionReference") ? _existingPayment["TransactionReference"]?.ToString() : string.Empty;
                txtNotes.Text = _existingPayment.Table.Columns.Contains("Notes") ? _existingPayment["Notes"]?.ToString() : string.Empty;
            }
        }

        private static decimal ReadDecimal(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0m;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
            if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed)) return parsed;
            return 0m;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (_invoiceRow == null)
            {
                MessageBox.Show("Không xác định được hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal amount = numAmount.Value;
            if (amount <= 0)
            {
                MessageBox.Show("Số tiền thanh toán phải > 0.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_existingPayment == null)
                {
                    int invoiceId = Convert.ToInt32(_invoiceRow["InvoiceId"]);
                    await _bll.AddPaymentAsync(
                    int newPaymentId = await _bll.AddPaymentAsync(
                        invoiceId,
                        dtPaymentDate.Value.Date,
                        amount,
                        cboMethod.Text,
                        txtReference.Text.Trim(),
                        txtNotes.Text.Trim());
                }
                else
                {
                    int paymentId = Convert.ToInt32(_existingPayment["PaymentId"]);
                    await _bll.UpdatePaymentAsync(
                        paymentId,
                        dtPaymentDate.Value.Date,
                        amount,
                        cboMethod.Text,
                        txtReference.Text.Trim(),
                        txtNotes.Text.Trim());
                }

                if (MessageBox.Show("Ghi nhận thanh toán thành công!\nBạn có muốn in biên nhận không?", "Thành công", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Giả sử ID thanh toán mới là `newPaymentId`
                    // Nếu là sửa, chúng ta cần ID cũ
                    int paymentIdForBill = _existingPayment != null ? Convert.ToInt32(_existingPayment["PaymentId"]) : newPaymentId;
                    PrintReceipt(paymentIdForBill, amount);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi ghi nhận thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintReceipt(int paymentId, decimal paidAmount)
        {
            try
            {
                string tenantName = ReadString(_invoiceRow, "TenantName");
                string roomNumber = ReadString(_invoiceRow, "RoomNumber");
                string invoiceNumber = ReadString(_invoiceRow, "InvoiceNumber");
                decimal totalAmount = ReadDecimal(_invoiceRow, "TotalAmount");
                decimal oldPaidAmount = ReadDecimal(_invoiceRow, "PaidAmount");
                decimal oldRemaining = ReadDecimal(_invoiceRow, "RemainingAmount");

                // Tính toán số tiền còn lại MỚI sau khi thanh toán
                decimal newRemaining = oldRemaining - paidAmount;

                var sb = new StringBuilder();
                sb.AppendLine("".PadRight(40, '='));
                sb.AppendLine("PHIẾU THU TIỀN".PadLeft(28));
                sb.AppendLine("".PadRight(40, '='));
                sb.AppendLine($"Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}");
                sb.AppendLine($"Số phiếu: PT{paymentId:D6}");
                sb.AppendLine($"Hóa đơn: {invoiceNumber}");
                sb.AppendLine("".PadRight(40, '-'));
                sb.AppendLine($"Khách hàng: {tenantName}");
                sb.AppendLine($"Phòng: {roomNumber}");
                sb.AppendLine("".PadRight(40, '-'));
                sb.AppendLine("NỘI DUNG THANH TOÁN");
                sb.AppendLine($"Thanh toán tiền phòng: {paidAmount:N0} VND");
                sb.AppendLine($"Bằng chữ: {NumberToText(paidAmount)} đồng.");
                sb.AppendLine("".PadRight(40, '-'));
                sb.AppendLine($"Tổng tiền HĐ: {totalAmount:N0} VND");
                sb.AppendLine($"Số tiền đã trả: {(oldPaidAmount + paidAmount):N0} VND");
                sb.AppendLine($"Số tiền còn lại: {newRemaining:N0} VND");
                sb.AppendLine("".PadRight(40, '='));
                sb.AppendLine("Người nộp tiền".PadLeft(15) + "Người lập phiếu".PadLeft(24));
                sb.AppendLine("(Ký, ghi rõ họ tên)".PadLeft(20) + "(Ký, ghi rõ họ tên)".PadLeft(20));

                using (var frmBill = new FrmBillViewer(sb.ToString()))
                {
                    frmBill.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo biên nhận: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Hàm chuyển số thành chữ (đã có trong dự án của bạn)
        private static string NumberToText(decimal number)
        {
            // Tạm thời để trống, bạn có thể tích hợp thư viện hoặc code đã có
            return "Một triệu hai trăm nghìn";
        }
    }
}

