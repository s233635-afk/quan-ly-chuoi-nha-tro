using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Payment form with full tenant information display and editing
    /// </summary>
    public class FrmPaymentWithTenantInfo : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _invoiceRow;
        private DataRow _tenantRow;
        private DataRow _existingPayment;

        // Tenant Info Controls
        private Label lblTenantInfo;
        private Label lblRoomInfo;
        private Label lblPhoneInfo;
        private Label lblEmailInfo;
        private Button _btnEditTenant;

        // Invoice Info Controls
        private Label lblInvoice;
        private Label lblAmounts;

        // Payment Controls
        private DateTimePicker dtPaymentDate;
        private NumericUpDown numAmount;
        private Button btnPayFull;
        private ComboBox cboMethod;
        private TextBox txtReference;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;
        private Button btnExportInvoice;
        private bool _exportAllowed;

        public FrmPaymentWithTenantInfo(AdminDataBLL bll, DataRow invoiceRow, DataRow existingPayment = null)
        {
            _bll = bll;
            _invoiceRow = invoiceRow;
            _existingPayment = existingPayment;
            InitializeComponent();
            Load += async (s, e) => await LoadTenantInfoAsync();
        }

        private void InitializeComponent()
        {
            Text = _existingPayment == null ? "Thu tiền & Thông tin khách" : "Cập nhật thanh toán";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(800, 650);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            // Bottom panel with buttons
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.FromArgb(240, 242, 245)
            };

            btnSave = new Button
            {
                Text = "Lưu",
                Width = 100,
                Height = 36,
                DialogResult = DialogResult.None,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += async (s, e) => await SavePaymentAsync();

            btnExportInvoice = new Button
            {
                Text = "📄 Xuất hóa đơn",
                Width = 120,
                Height = 36,
                BackColor = Color.FromArgb(107, 105, 123),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExportInvoice.Click += (s, e) => HandleInvoiceExport();
            btnExportInvoice.Enabled = _existingPayment != null;
            _exportAllowed = btnExportInvoice.Enabled;

            btnCancel = new Button
            {
                Text = "Đóng",
                Width = 100,
                Height = 36,
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };

            pnlBottom.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnExportInvoice);
            pnlBottom.Controls.Add(btnCancel);

            btnSave.Location = new Point(pnlBottom.Width - 320, 10);
            btnExportInvoice.Location = new Point(pnlBottom.Width - 210, 10);
            btnCancel.Location = new Point(pnlBottom.Width - 110, 10);

            // Main body with scroll
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 18, 18, 10),
                AutoScroll = true,
                BackColor = Color.White
            };

            int labelWidth = 160;
            int inputWidth = 500;
            int top = 10;
            int left = 6;
            int line = 34;

            // ===== TENANT INFO SECTION =====
            var lblTenantSection = new Label
            {
                Text = "👤 THÔNG TIN KHÁCH THUÊ",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(left, top),
                AutoSize = true
            };
            pnlBody.Controls.Add(lblTenantSection);
            top += 30;

            lblTenantInfo = new Label
            {
                Location = new Point(left, top),
                Width = 600,
                Height = 60,
                AutoSize = false,
                Text = "Đang tải...",
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8),
                BackColor = Color.FromArgb(240, 248, 255)
            };
            pnlBody.Controls.Add(lblTenantInfo);
            top += 70;

            lblRoomInfo = new Label { Location = new Point(left, top), Width = 600, AutoSize = true, Text = "Phòng: ..." };
            pnlBody.Controls.Add(lblRoomInfo);
            top += 28;

            lblPhoneInfo = new Label { Location = new Point(left, top), Width = 600, AutoSize = true, Text = "Điện thoại: ..." };
            pnlBody.Controls.Add(lblPhoneInfo);
            top += 28;

            lblEmailInfo = new Label { Location = new Point(left, top), Width = 600, AutoSize = true, Text = "Email: ..." };
            pnlBody.Controls.Add(lblEmailInfo);
            top += 28;

            _btnEditTenant = new Button { Text = "✏️ Chỉnh sửa thông tin khách", Width = 200, Height = 32, Location = new Point(left, top) };
            _btnEditTenant.Click += (s, e) => EditTenantInfo();
            pnlBody.Controls.Add(_btnEditTenant);
            top += 42;

            // Separator
            var pnlSep1 = new Panel { Location = new Point(left, top), Width = 600, Height = 1, BackColor = Color.LightGray };
            pnlBody.Controls.Add(pnlSep1);
            top += 20;

            // ===== INVOICE INFO SECTION =====
            var lblInvoiceSection = new Label
            {
                Text = "📋 THÔNG TIN HÓA ĐƠN",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(left, top),
                AutoSize = true
            };
            pnlBody.Controls.Add(lblInvoiceSection);
            top += 30;

            lblInvoice = new Label
            {
                Location = new Point(left, top),
                Width = 600,
                Height = 40,
                AutoSize = false,
                Text = "Đang tải...",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            pnlBody.Controls.Add(lblInvoice);
            top += 50;

            lblAmounts = new Label
            {
                Location = new Point(left, top),
                Width = 600,
                Height = 60,
                AutoSize = false,
                Text = "Tổng tiền: ...",
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8),
                BackColor = Color.FromArgb(240, 255, 240)
            };
            pnlBody.Controls.Add(lblAmounts);
            top += 70;

            // Separator
            var pnlSep2 = new Panel { Location = new Point(left, top), Width = 600, Height = 1, BackColor = Color.LightGray };
            pnlBody.Controls.Add(pnlSep2);
            top += 20;

            // ===== PAYMENT INFO SECTION =====
            var lblPaymentSection = new Label
            {
                Text = "💳 THÔNG TIN THANH TOÁN",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(left, top),
                AutoSize = true
            };
            pnlBody.Controls.Add(lblPaymentSection);
            top += 30;

            // Payment Date
            var lblDate = new Label { Text = "Ngày thanh toán:", Location = new Point(left, top), Width = labelWidth, TextAlign = ContentAlignment.MiddleLeft };
            dtPaymentDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Location = new Point(left + labelWidth, top), Width = 200 };
            pnlBody.Controls.Add(lblDate);
            pnlBody.Controls.Add(dtPaymentDate);
            top += line;

            // Amount
            var lblAmount = new Label { Text = "Số tiền:", Location = new Point(left, top), Width = labelWidth, TextAlign = ContentAlignment.MiddleLeft };
            numAmount = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100000000000,
                DecimalPlaces = 0,
                ThousandsSeparator = true,
                Location = new Point(left + labelWidth, top),
                Width = 200
            };
            btnPayFull = new Button { Text = "Thu đủ", Width = 90, Height = 28, Location = new Point(left + labelWidth + 210, top) };
            btnPayFull.Click += (s, e) =>
            {
                if (numAmount.Maximum > 0)
                    numAmount.Value = numAmount.Maximum;
            };
            btnPayFull.FlatStyle = FlatStyle.Flat;
            btnPayFull.FlatAppearance.BorderSize = 1;
            pnlBody.Controls.Add(lblAmount);
            pnlBody.Controls.Add(numAmount);
            pnlBody.Controls.Add(btnPayFull);
            top += line;

            // Payment Method
            var lblMethod = new Label { Text = "Hình thức:", Location = new Point(left, top), Width = labelWidth, TextAlign = ContentAlignment.MiddleLeft };
            cboMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(left + labelWidth, top), Width = 200 };
            cboMethod.Items.AddRange(new object[] { "Cash", "Transfer", "Check", "Card" });
            cboMethod.SelectedIndex = 0;
            pnlBody.Controls.Add(lblMethod);
            pnlBody.Controls.Add(cboMethod);
            top += line;

            // Reference
            var lblRef = new Label { Text = "Tham chiếu:", Location = new Point(left, top), Width = labelWidth, TextAlign = ContentAlignment.MiddleLeft };
            txtReference = new TextBox { Location = new Point(left + labelWidth, top), Width = 300 };
            pnlBody.Controls.Add(lblRef);
            pnlBody.Controls.Add(txtReference);
            top += line;

            // Notes
            var lblNotes = new Label { Text = "Ghi chú:", Location = new Point(left, top), Width = labelWidth, TextAlign = ContentAlignment.TopLeft };
            txtNotes = new TextBox
            {
                Multiline = true,
                Location = new Point(left + labelWidth, top),
                Width = inputWidth,
                Height = 80
            };
            pnlBody.Controls.Add(lblNotes);
            pnlBody.Controls.Add(txtNotes);

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }

        private async System.Threading.Tasks.Task LoadTenantInfoAsync()
        {
            try
            {
                // Get tenant ID from invoice
                if (_invoiceRow == null || !_invoiceRow.Table.Columns.Contains("TenantId"))
                {
                    MessageBox.Show("Không tìm thấy thông tin khách thuê.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int tenantId = Convert.ToInt32(_invoiceRow["TenantId"]);
                var tenantTable = await _bll.GetTenantsAsync();
                if (tenantTable == null || tenantTable.Rows.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu khách thuê.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Find tenant row
                foreach (DataRow r in tenantTable.Rows)
                {
                    if (Convert.ToInt32(r["TenantId"]) == tenantId)
                    {
                        _tenantRow = r;
                        break;
                    }
                }

                if (_tenantRow == null)
                {
                    MessageBox.Show($"Không tìm thấy khách thuê ID {tenantId}.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Display tenant info
                DisplayTenantInfo();
                DisplayInvoiceInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayTenantInfo()
        {
            if (_tenantRow == null) return;

            string tenantName = _tenantRow["TenantName"]?.ToString() ?? "N/A";
            string tenantId = _tenantRow["TenantId"]?.ToString() ?? "N/A";
            string phone = _tenantRow["Phone"]?.ToString() ?? "N/A";
            string email = _tenantRow["Email"]?.ToString() ?? "N/A";
            string address = _tenantRow["Address"]?.ToString() ?? "N/A";
            string idCard = _tenantRow["IdCard"]?.ToString() ?? "N/A";

            lblTenantInfo.Text = $"ID: {tenantId}\n{tenantName}\nĐC: {address}\nCMND: {idCard}";
            lblPhoneInfo.Text = $"Điện thoại: {phone}";
            lblEmailInfo.Text = $"Email: {email}";

            // Get room info
            if (_invoiceRow.Table.Columns.Contains("RoomNumber"))
            {
                string roomNumber = _invoiceRow["RoomNumber"]?.ToString() ?? "N/A";
                lblRoomInfo.Text = $"Phòng: {roomNumber}";
            }
        }

        private void DisplayInvoiceInfo()
        {
            if (_invoiceRow == null) return;

            string invoiceNumber = _invoiceRow["InvoiceNumber"]?.ToString() ?? "N/A";
            string invoiceDate = _invoiceRow["InvoiceDate"]?.ToString() ?? "N/A";
            string dueDate = _invoiceRow["DueDate"]?.ToString() ?? "N/A";

            lblInvoice.Text = $"Hóa đơn: {invoiceNumber} | Ngày lập: {invoiceDate} | Hạn: {dueDate}";

            // Calculate amounts
            decimal totalAmount = ReadDecimal(_invoiceRow, "TotalAmount");
            decimal paidAmount = ReadDecimal(_invoiceRow, "PaidAmount");
            decimal remainingAmount = ReadDecimal(_invoiceRow, "RemainingAmount");

            string rentalCost = ReadDecimal(_invoiceRow, "RentalCost").ToString("N0");
            string utilityCost = ReadDecimal(_invoiceRow, "UtilityCost").ToString("N0");
            string otherCost = ReadDecimal(_invoiceRow, "OtherCost").ToString("N0");

            lblAmounts.Text = $"Tiền phòng: {rentalCost}\nTiền dịch vụ: {utilityCost}\nChí phí khác: {otherCost}\n" +
                $"Tổng tiền: {totalAmount:N0} | Đã thu: {paidAmount:N0} | Còn nợ: {remainingAmount:N0}";

            numAmount.Maximum = (decimal)remainingAmount;
            numAmount.Value = (decimal)remainingAmount;
        }

        private decimal ReadDecimal(DataRow row, string col)
        {
            if (row == null || !row.Table.Columns.Contains(col)) return 0m;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            if (decimal.TryParse(v.ToString(), out var parsed)) return parsed;
            return 0m;
        }

        private void EditTenantInfo()
        {
            if (_tenantRow == null) return;

            using (var frm = new FrmTenantEditor(_bll, _tenantRow))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadTenantInfoAsync();
                }
            }
        }

        private async Task SavePaymentAsync()
        {
            try
            {
                if (_invoiceRow == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int invoiceId = Convert.ToInt32(_invoiceRow["InvoiceId"]);
                decimal amount = numAmount.Value;

                if (amount <= 0)
                {
                    MessageBox.Show("Số tiền phải lớn hơn 0.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create payment record
                await _bll.InsertPaymentAsync(
                    invoiceId: invoiceId,
                    paymentDate: dtPaymentDate.Value,
                    amount: amount,
                    method: cboMethod.SelectedItem?.ToString() ?? "Cash",
                    reference: txtReference.Text,
                    notes: txtNotes.Text
                );

                // Enable invoice export and mark as sent
                SetExportAllowed(true);
                await NotifyTenantInvoiceSentAsync();
                if (MessageBox.Show(
                    "Đã lưu thanh toán thành công!\nBạn có muốn in hóa đơn ngay?",
                    "Hoàn tất",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    ExportInvoicePdf();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetExportAllowed(bool allowed)
        {
            _exportAllowed = allowed;
            btnExportInvoice.Enabled = allowed;
        }

        private async Task NotifyTenantInvoiceSentAsync()
        {
            await Task.Delay(1);
            string tenantName = _tenantRow?["TenantName"]?.ToString() ?? _invoiceRow?["TenantName"]?.ToString() ?? "khách thuê";
            string tenantEmail = _tenantRow?["Email"]?.ToString() ?? _invoiceRow?["TenantEmail"]?.ToString() ?? "chưa cung cấp";
            MessageBox.Show($"Hóa đơn đã được gửi đến {tenantName} ({tenantEmail}).", "Đã gửi hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void HandleInvoiceExport()
        {
            if (!_exportAllowed)
            {
                MessageBox.Show("Hóa đơn chỉ có thể xuất sau khi thanh toán xong.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ExportInvoicePdf();
        }

        private void ExportInvoicePdf()
        {
            if (_invoiceRow == null)
            {
                MessageBox.Show("Không tìm thấy thông tin hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string invoiceNumber = _invoiceRow["InvoiceNumber"]?.ToString() ?? "Invoice";
                string fileName = $"{invoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                using (var sfd = new SaveFileDialog
                {
                    Filter = "Text (*.txt)|*.txt|PDF (*.pdf)|*.pdf",
                    FileName = fileName,
                    DefaultExt = "txt"
                })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;

                    // For now, export as text
                    // TODO: Integrate with PDF library (iTextSharp, PdfSharp, etc.)
                    GenerateInvoiceText(_invoiceRow, _tenantRow, sfd.FileName);

                    MessageBox.Show($"Đã xuất hóa đơn: {sfd.FileName}", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateInvoiceText(DataRow invoice, DataRow tenant, string filePath)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                      HÓA ĐƠN THANH TOÁN                        ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            sb.AppendLine("━━━━━━ THÔNG TIN HÓA ĐƠN ━━━━━━");
            sb.AppendLine($"Số hóa đơn  : {invoice["InvoiceNumber"]}");
            sb.AppendLine($"Ngày lập   : {invoice["InvoiceDate"]}");
            sb.AppendLine($"Hạn thanh  : {invoice["DueDate"]}");
            sb.AppendLine();

            sb.AppendLine("━━━━━━ THÔNG TIN KHÁCH THUÊ ━━━━━━");
            sb.AppendLine($"Tên khách  : {tenant["TenantName"]}");
            sb.AppendLine($"CMND       : {tenant["IdCard"]}");
            sb.AppendLine($"Địa chỉ    : {tenant["Address"]}");
            sb.AppendLine($"Điện thoại : {tenant["Phone"]}");
            sb.AppendLine($"Email      : {tenant["Email"]}");
            sb.AppendLine();

            sb.AppendLine("━━━━━━ CHI TIẾT TIỀN ━━━━━━");
            decimal rental = ReadDecimal(invoice, "RentalCost");
            decimal utility = ReadDecimal(invoice, "UtilityCost");
            decimal other = ReadDecimal(invoice, "OtherCost");
            decimal total = ReadDecimal(invoice, "TotalAmount");
            decimal paid = ReadDecimal(invoice, "PaidAmount");
            decimal remaining = ReadDecimal(invoice, "RemainingAmount");

            sb.AppendLine($"Tiền phòng      : {rental:N0} đ");
            sb.AppendLine($"Tiền dịch vụ    : {utility:N0} đ");
            sb.AppendLine($"Chi phí khác    : {other:N0} đ");
            sb.AppendLine("─────────────────────────");
            sb.AppendLine($"Tổng tiền       : {total:N0} đ");
            sb.AppendLine($"Đã thanh toán   : {paid:N0} đ");
            sb.AppendLine($"Còn nợ          : {remaining:N0} đ");
            sb.AppendLine();

            sb.AppendLine("Ngày xuất: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────────────────────────────");
            sb.AppendLine("Cảm ơn quý khách đã thanh toán. Hóa đơn này là bằng chứng");
            sb.AppendLine("thanh toán của quý khách.");
            sb.AppendLine("─────────────────────────────────────────────────────────────");

            System.IO.File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
        }
    }
}
