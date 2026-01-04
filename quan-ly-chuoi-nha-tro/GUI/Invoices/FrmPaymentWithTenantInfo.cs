using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Payment form with full tenant information display and editing
    /// </summary>
    public class FrmPaymentWithTenantInfo : Form
    {
        private readonly AdminDataBLL _bll;
        private DataRow _invoiceRow;
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

        // Bank & QR Controls
        private Panel pnlBankInfo;
        private PictureBox picQrCode;
        private Label lblBankDetails;

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

            btnSave = new ModernButton
            {
                Text = "Lưu",
                Width = 100,
                Height = 36,
                DialogResult = DialogResult.None,
                BaseColor = Color.FromArgb(0, 120, 215),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += async (s, e) => await SavePaymentAsync();

            btnExportInvoice = new ModernButton
            {
                Text = "📄 Xuất PDF/In",
                Width = 140,
                Height = 36,
                BaseColor = Color.FromArgb(107, 105, 123),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExportInvoice.Click += (s, e) => HandleInvoiceExport();
            btnExportInvoice.Enabled = false;
            _exportAllowed = false;

            btnCancel = new ModernButton
            {
                Text = "Đóng",
                Width = 100,
                Height = 36,
                DialogResult = DialogResult.Cancel,
                BaseColor = Color.FromArgb(200, 200, 200),
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };

            pnlBottom.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnExportInvoice);
            pnlBottom.Controls.Add(btnCancel);

            btnSave.Location = new Point(pnlBottom.Width - 350, 10);
            btnExportInvoice.Location = new Point(pnlBottom.Width - 240, 10);
            btnCancel.Location = new Point(pnlBottom.Width - 110, 10);

            pnlBottom.Resize += (s, e) =>
            {
                btnSave.Location = new Point(pnlBottom.Width - 350, 10);
                btnExportInvoice.Location = new Point(pnlBottom.Width - 240, 10);
                btnCancel.Location = new Point(pnlBottom.Width - 110, 10);
            };

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

            _btnEditTenant = new ModernButton { Text = "✏️ Chỉnh sửa thông tin khách", Width = 200, Height = 32, Location = new Point(left, top), BaseColor = Color.FromArgb(240, 240, 240), BackColor = Color.Transparent, ForeColor = Color.Black };
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
            top += 35;

            // Use a FlowLayoutPanel for payment fields to handle dynamic visibility of Bank Info
            var flowPayment = new FlowLayoutPanel
            {
                Location = new Point(left, top),
                Width = 620,
                Height = 500, // Will be adjusted or scrollable
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            pnlBody.Controls.Add(flowPayment);

            // Helper to create row panel for FlowLayoutPanel
            Panel CreateRow(Control lbl, Control input, Control extra = null)
            {
                var p = new Panel { Width = 600, Height = 35 };
                lbl.Location = new Point(0, 5);
                input.Location = new Point(labelWidth, 2);
                p.Controls.Add(lbl);
                p.Controls.Add(input);
                if (extra != null)
                {
                    extra.Location = new Point(labelWidth + input.Width + 10, 2);
                    p.Controls.Add(extra);
                }
                return p;
            }

            // Payment Date
            var lblDate = new Label { Text = "Ngày thanh toán:", Width = labelWidth, TextAlign = ContentAlignment.MiddleLeft };
            dtPaymentDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 200 };
            flowPayment.Controls.Add(CreateRow(lblDate, dtPaymentDate));

            // Amount
            var lblAmount = new Label { Text = "Số tiền:", Width = labelWidth, TextAlign = ContentAlignment.MiddleLeft };
            numAmount = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100000000000,
                DecimalPlaces = 0,
                ThousandsSeparator = true,
                Width = 200
            };
            btnPayFull = new ModernButton { Text = "Thu đủ", Width = 90, Height = 28, BaseColor = Color.FromArgb(240, 240, 240), BackColor = Color.Transparent, ForeColor = Color.Black };
            btnPayFull.Click += (s, e) =>
            {
                if (numAmount.Maximum > 0)
                    numAmount.Value = numAmount.Maximum;
            };
            btnPayFull.FlatStyle = FlatStyle.Flat;
            btnPayFull.FlatAppearance.BorderSize = 1;
            flowPayment.Controls.Add(CreateRow(lblAmount, numAmount, btnPayFull));

            // Payment Method
            var lblMethod = new Label { Text = "Hình thức:", Width = labelWidth, TextAlign = ContentAlignment.MiddleLeft };
            cboMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
            cboMethod.Items.AddRange(new object[] { "Tiền mặt", "Chuyển khoản", "Thẻ" });
            cboMethod.SelectedIndex = 0;
            cboMethod.SelectedIndexChanged += (s, e) => UpdateBankInfoVisibility();
            flowPayment.Controls.Add(CreateRow(lblMethod, cboMethod));

            // Bank & QR Panel
            pnlBankInfo = new Panel
            {
                Width = 600,
                Height = 220,
                Visible = false,
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(labelWidth, 5, 0, 10)
            };

            picQrCode = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(200, 200),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlBankInfo.Controls.Add(picQrCode);

            lblBankDetails = new Label
            {
                Location = new Point(220, 10),
                Size = new Size(360, 160),
                Font = new Font("Segoe UI", 10F),
                Text = "Đang tải thông tin ngân hàng..."
            };
            pnlBankInfo.Controls.Add(lblBankDetails);

            var btnCopyBank = new ModernButton
            {
                Text = "📋 Sao chép STK",
                Location = new Point(220, 175),
                Width = 140,
                Height = 30,
                BaseColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black
            };
            btnCopyBank.Click += (s, e) =>
            {
                var stk = lblBankDetails.Text.Split('\n').FirstOrDefault(l => l.Contains("SỐ TÀI KHOẢN:"))?.Replace("SỐ TÀI KHOẢN:", "").Trim();
                if (!string.IsNullOrEmpty(stk))
                {
                    Clipboard.SetText(stk);
                    ToastNotification.Success("Đã sao chép số tài khoản");
                }
            };
            pnlBankInfo.Controls.Add(btnCopyBank);

            flowPayment.Controls.Add(pnlBankInfo);

            // Reference
            var lblRef = new Label { Text = "Tham chiếu:", Width = labelWidth, TextAlign = ContentAlignment.MiddleLeft };
            txtReference = new TextBox { Width = 300 };
            flowPayment.Controls.Add(CreateRow(lblRef, txtReference));

            // Notes
            var lblNotes = new Label { Text = "Ghi chú:", Width = labelWidth, TextAlign = ContentAlignment.TopLeft };
            txtNotes = new TextBox
            {
                Multiline = true,
                Width = 400,
                Height = 80
            };
            var pnlNotes = new Panel { Width = 600, Height = 90 };
            lblNotes.Location = new Point(0, 5);
            txtNotes.Location = new Point(labelWidth, 2);
            pnlNotes.Controls.Add(lblNotes);
            pnlNotes.Controls.Add(txtNotes);
            flowPayment.Controls.Add(pnlNotes);

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
                    ModernDialog.Error("Không tìm thấy thông tin khách thuê");
                    return;
                }

                int tenantId = Convert.ToInt32(_invoiceRow["TenantId"]);
                var tenantTable = await _bll.GetTenantsAsync();
                if (tenantTable == null || tenantTable.Rows.Count == 0)
                {
                    ModernDialog.Error("Không tìm thấy dữ liệu khách thuê");
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
                    ModernDialog.Error($"Không tìm thấy khách thuê ID {tenantId}");
                }
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi tải thông tin: " + ex.Message);
            }
            finally
            {
                DisplayTenantInfo();
                DisplayInvoiceInfo();
                if (_existingPayment != null && _existingPayment.Table.Columns.Contains("PaymentMethod"))
                {
                    var method = _existingPayment["PaymentMethod"]?.ToString();
                    cboMethod.SelectedItem = MapPaymentMethodToDisplay(method);
                }
            }
        }

        private void DisplayTenantInfo()
        {
            string tenantName = ReadTenantValue(_tenantRow, "TenantName", "FullName") ?? ReadInvoiceValue("TenantName");
            string tenantId = ReadTenantValue(_tenantRow, "TenantId") ?? ReadInvoiceValue("TenantId");
            string phone = ReadTenantValue(_tenantRow, "Phone", "PhoneNumber") ?? ReadInvoiceValue("Phone");
            string email = ReadTenantValue(_tenantRow, "Email") ?? ReadInvoiceValue("TenantEmail");
            string address = ReadTenantValue(_tenantRow, "Address") ?? ReadInvoiceValue("TenantAddress");
            string idCard = ReadTenantValue(_tenantRow, "IdCard", "IdentityCard") ?? ReadInvoiceValue("IdentityCard");

            if (string.IsNullOrWhiteSpace(tenantName)) tenantName = "N/A";
            if (string.IsNullOrWhiteSpace(tenantId)) tenantId = "N/A";
            if (string.IsNullOrWhiteSpace(phone)) phone = "N/A";
            if (string.IsNullOrWhiteSpace(email)) email = "N/A";
            if (string.IsNullOrWhiteSpace(address)) address = "N/A";
            if (string.IsNullOrWhiteSpace(idCard)) idCard = "N/A";

            lblTenantInfo.Text = $"ID: {tenantId}\n{tenantName}\nĐC: {address}\nCMND: {idCard}";
            lblPhoneInfo.Text = $"Điện thoại: {phone}";
            lblEmailInfo.Text = $"Email: {email}";

            // Get room info
            if (_invoiceRow != null && _invoiceRow.Table.Columns.Contains("RoomNumber"))
            {
                string roomNumber = _invoiceRow["RoomNumber"]?.ToString() ?? "N/A";
                lblRoomInfo.Text = $"Phòng: {roomNumber}";
            }
            else
            {
                lblRoomInfo.Text = "Phòng: N/A";
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
            SetExportAllowed(true); // Luôn cho phép xuất hóa đơn để gửi khách
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
                    ModernDialog.Error("Không tìm thấy thông tin hóa đơn");
                    return;
                }

                int invoiceId = Convert.ToInt32(_invoiceRow["InvoiceId"]);
                decimal amount = numAmount.Value;

                if (amount <= 0)
                {
                    ToastNotification.Warning("Số tiền phải lớn hơn 0");
                    return;
                }

                // Create payment record
                await _bll.InsertPaymentAsync(
                    invoiceId: invoiceId,
                    paymentDate: dtPaymentDate.Value,
                    amount: amount,
                    method: MapPaymentMethodToStored(cboMethod.SelectedItem?.ToString()),
                    reference: txtReference.Text,
                    notes: txtNotes.Text
                );

                await ReloadInvoiceRowAsync();
                DisplayInvoiceInfo();

                decimal remaining = ReadDecimal(_invoiceRow, "RemainingAmount");
                SetExportAllowed(remaining <= 0);
                await NotifyTenantInvoiceSentAsync();

                AdminEvents.NotifyDataChanged();
                DataSyncManager.NotifyInvoicesChanged();
                DataSyncManager.NotifyPaymentsChanged();
                DataSyncManager.NotifyRoomsChanged();

                if (_exportAllowed && remaining <= 0 && ModernConfirmDialog.Confirm(
                    "Đã thanh toán đủ.\nBạn có muốn xuất/in hóa đơn ngay?",
                    "Hoàn tất"))
                {
                    OpenExportForm();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi lưu thanh toán: " + ex.Message);
            }
        }

        private void SetExportAllowed(bool allowed)
        {
            _exportAllowed = true; // Luôn cho phép xuất hóa đơn
            btnExportInvoice.Enabled = true;
        }

        private async void UpdateBankInfoVisibility()
        {
            bool isTransfer = cboMethod.SelectedItem?.ToString() == "Chuyển khoản";
            pnlBankInfo.Visible = isTransfer;

            if (isTransfer)
            {
                await LoadAndGenerateQrAsync();
            }
        }

        private async Task LoadAndGenerateQrAsync()
        {
            try
            {
                string bankId = await _bll.GetSystemSettingValueAsync("BankId") ?? "ICB"; // Default VietinBank
                string accountNo = await _bll.GetSystemSettingValueAsync("BankAccountNumber") ?? "0000000000";
                string accountName = await _bll.GetSystemSettingValueAsync("BankAccountName") ?? "CHUA CAU HINH";
                string template = await _bll.GetSystemSettingValueAsync("BankTemplate") ?? "compact";

                decimal amount = numAmount.Value;
                string description = $"THANH TOAN HOA DON {ReadInvoiceValue("InvoiceNumber")}";

                // VietQR API URL
                // https://img.vietqr.io/image/<BANK_ID>-<ACCOUNT_NO>-<TEMPLATE>.png?amount=<AMOUNT>&addInfo=<CONTENT>&accountName=<NAME>
                string qrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png" +
                               $"?amount={amount:0}" +
                               $"&addInfo={Uri.EscapeDataString(description)}" +
                               $"&accountName={Uri.EscapeDataString(accountName)}";

                lblBankDetails.Text = $"NGÂN HÀNG: {bankId}\n" +
                                     $"SỐ TÀI KHOẢN: {accountNo}\n" +
                                     $"CHỦ TÀI KHOẢN: {accountName}\n\n" +
                                     $"NỘI DUNG: {description}\n" +
                                     $"SỐ TIỀN: {amount:N0} VNĐ";

                picQrCode.ImageLocation = qrUrl;
            }
            catch (Exception ex)
            {
                lblBankDetails.Text = "Lỗi tải thông tin ngân hàng: " + ex.Message;
            }
        }

        private static string MapPaymentMethodToStored(string displayValue)
        {
            switch ((displayValue ?? string.Empty).Trim())
            {
                case "Tiền mặt":
                    return "Cash";
                case "Chuyển khoản":
                    return "Transfer";
                case "Séc":
                    return "Check";
                case "Thẻ":
                    return "Card";
                default:
                    return "Cash";
            }
        }

        private static string MapPaymentMethodToDisplay(string storedValue)
        {
            switch ((storedValue ?? string.Empty).Trim())
            {
                case "Cash":
                    return "Tiền mặt";
                case "Transfer":
                    return "Chuyển khoản";
                case "Check":
                    return "Séc";
                case "Card":
                    return "Thẻ";
                default:
                    return "Tiền mặt";
            }
        }

        private async Task NotifyTenantInvoiceSentAsync()
        {
            await Task.Delay(1);
            string tenantName = ReadTenantValue(_tenantRow, "TenantName", "FullName")
                ?? ReadInvoiceValue("TenantName")
                ?? "khách thuê";
            string tenantEmail = _tenantRow?["Email"]?.ToString() ?? _invoiceRow?["TenantEmail"]?.ToString() ?? "chưa cung cấp";
            ToastNotification.Success($"Hóa đơn đã được gửi đến {tenantName} ({tenantEmail})");
        }

        private static string ReadTenantValue(DataRow row, params string[] cols)
        {
            if (row == null || row.Table == null || cols == null) return null;
            foreach (var col in cols)
            {
                if (row.Table.Columns.Contains(col))
                {
                    var v = row[col];
                    if (v != null && v != DBNull.Value)
                        return v.ToString();
                }
            }
            return null;
        }

        private string ReadInvoiceValue(string col)
        {
            if (_invoiceRow == null || _invoiceRow.Table == null || !_invoiceRow.Table.Columns.Contains(col)) return null;
            var v = _invoiceRow[col];
            return v == null || v == DBNull.Value ? null : v.ToString();
        }

        private async void HandleInvoiceExport()
        {
            // Đã bỏ chặn xuất hóa đơn trước khi thanh toán
            await ReloadInvoiceRowAsync();
            DisplayInvoiceInfo();
            OpenExportForm();
        }

        private async Task ReloadInvoiceRowAsync()
        {
            if (_invoiceRow == null)
                return;
            try
            {
                int invoiceId = Convert.ToInt32(_invoiceRow["InvoiceId"]);
                var table = await _bll.GetInvoicesViewAsync();
                var updated = table?.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["InvoiceId"]) == invoiceId);
                if (updated != null)
                    _invoiceRow = updated;
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi tải lại hóa đơn: " + ex.Message);
            }
        }

        private void OpenExportForm()
        {
            using (var frm = new FrmInvoiceExportForm(_bll, _invoiceRow))
            {
                frm.ShowDialog(this);
            }
        }
    }
}
