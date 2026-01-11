using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmInvoiceExportForm : Form
    {
        private readonly AdminDataBLL _bll;
        private DataRow _invoiceRow;
        private DataRow _tenantRow;
        private DataTable _payments;
        private DataTable _utilityReadings;
        private DataTable _utilityTypes;

        private Panel _previewHost;
        private Panel _pnlContent;
        private Label _lblBranchInfo;
        private Label _lblInvoiceTitle;
        private Label _lblInvoiceMeta;
        private Label _lblTenantInfo;
        private TableLayoutPanel _tableCosts;
        private Label _lblSummary;
        private Label _lblPayments;
        private Label _lblStaffSignature;

        // Bank & QR
        private Panel _pnlBankInfo;
        private PictureBox _picQrCode;
        private Label _lblBankDetails;

        private Button _btnExportPdf;
        private Button _btnPrint;
        private Button _btnClose;

        private DataRow _branchRow;

        public FrmInvoiceExportForm(AdminDataBLL bll, DataRow invoiceRow)
        {
            _bll = bll;
            _invoiceRow = invoiceRow;
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            Text = "Xuất hóa đơn";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1000;
            Height = 850;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);

            var main = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = Color.FromArgb(240, 242, 245) };

            _previewHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(224, 224, 224),
                AutoScroll = true
            };

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                Padding = new Padding(12, 8, 12, 8),
                BackColor = Color.White
            };

            _btnExportPdf = new Button
            {
                Text = "Xuất PDF",
                Width = 110,
                Height = 34,
                BackColor = Color.FromArgb(107, 105, 123),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnExportPdf.FlatAppearance.BorderSize = 0;
            _btnExportPdf.Click += (s, e) => ExportPdf();

            _btnPrint = new Button
            {
                Text = "In hóa đơn",
                Width = 110,
                Height = 34,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnPrint.FlatAppearance.BorderSize = 0;
            _btnPrint.Click += (s, e) => PrintInvoice();

            _btnClose = new Button
            {
                Text = "Đóng",
                Width = 90,
                Height = 34,
                BackColor = Color.FromArgb(230, 230, 230),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            _btnClose.FlatAppearance.BorderSize = 1;
            _btnClose.Click += (s, e) => Close();

            footer.Controls.Add(_btnExportPdf);
            footer.Controls.Add(_btnPrint);
            footer.Controls.Add(_btnClose);

            _btnClose.Location = new Point(footer.Width - _btnClose.Width - 12, 10);
            _btnPrint.Location = new Point(_btnClose.Left - _btnPrint.Width - 8, 10);
            _btnExportPdf.Location = new Point(_btnPrint.Left - _btnExportPdf.Width - 8, 10);
            footer.Resize += (s, e) =>
            {
                _btnClose.Location = new Point(footer.Width - _btnClose.Width - 12, 10);
                _btnPrint.Location = new Point(_btnClose.Left - _btnPrint.Width - 8, 10);
                _btnExportPdf.Location = new Point(_btnPrint.Left - _btnExportPdf.Width - 8, 10);
            };

            BuildPreviewLayout();

            main.Controls.Add(_previewHost);
            main.Controls.Add(footer);
            Controls.Add(main);
        }

        private void BuildPreviewLayout()
        {
            _previewHost.Controls.Clear();

            _pnlContent = new Panel
            {
                Width = 800,
                MinimumSize = new Size(800, 500),
                MaximumSize = new Size(800, 10000),
                BackColor = Color.White,
                Padding = new Padding(40),
                Location = new Point(Math.Max(0, (_previewHost.Width - 800) / 2), 20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            _previewHost.Controls.Add(_pnlContent);

            _previewHost.Resize += (s, e) =>
            {
                _pnlContent.Location = new Point(Math.Max(0, (_previewHost.Width - 800) / 2), 20);
            };

            // 1. Header Section (Branch Info & Title)
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 120 };

            _lblBranchInfo = new Label
            {
                Text = "HỆ THỐNG NHÀ TRỌ\nĐịa chỉ: ...\nHotline: ...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Left,
                Width = 400,
                TextAlign = ContentAlignment.TopLeft
            };

            var pnlTitleContainer = new Panel { Dock = DockStyle.Fill };
            _lblInvoiceTitle = new Label
            {
                Text = "HÓA ĐƠN DỊCH VỤ",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.TopRight
            };

            _lblInvoiceMeta = new Label
            {
                Text = "Số: HD-000001\nNgày: 01/01/2024",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.TopRight
            };
            pnlTitleContainer.Controls.Add(_lblInvoiceMeta);
            pnlTitleContainer.Controls.Add(_lblInvoiceTitle);

            pnlHeader.Controls.Add(pnlTitleContainer);
            pnlHeader.Controls.Add(_lblBranchInfo);

            var line1 = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = Color.FromArgb(0, 102, 204), Margin = new Padding(0, 10, 0, 10) };

            // 2. Tenant Info Section
            _lblTenantInfo = new Label
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(0, 10, 0, 10),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Text = "Khách hàng: ...\nPhòng: ...\nKỳ thanh toán: ..."
            };

            // 3. Table Section
            var pnlTableContainer = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 10, 0, 10) };
            _tableCosts = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                BackColor = Color.White
            };
            _tableCosts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Nội dung
            _tableCosts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20)); // Đơn vị
            _tableCosts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20)); // Thành tiền

            pnlTableContainer.Controls.Add(_tableCosts);

            // 4. Summary Section
            _lblSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.TopRight,
                Text = "TỔNG CỘNG: 0đ\nĐã thu: 0đ\nCÒN LẠI: 0đ",
                BackColor = Color.FromArgb(245, 249, 255)
            };

            // 5. Payments History
            _lblPayments = new Label
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(5),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Text = "Lịch sử thanh toán: ..."
            };

            // 6. Bank & QR Section
            _pnlBankInfo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 170,
                Padding = new Padding(15),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _picQrCode = new PictureBox
            {
                Size = new Size(140, 140),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Left
            };

            _lblBankDetails = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Text = "Thông tin chuyển khoản..."
            };

            _pnlBankInfo.Controls.Add(_lblBankDetails);
            _pnlBankInfo.Controls.Add(_picQrCode);

            // 7. Footer (Signatures)
            var pnlFooter = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(0, 20, 0, 0) };
            var lblSignTenant = new Label { Text = "Khách hàng\n(Ký và ghi rõ họ tên)", Dock = DockStyle.Left, Width = 300, TextAlign = ContentAlignment.TopCenter, Font = new Font("Segoe UI", 9, FontStyle.Italic) };

            _lblStaffSignature = new Label
            {
                Text = "Người lập phiếu\n(Ký và ghi rõ họ tên)\n\n\nNHÂN VIÊN QUẢN LÝ",
                Dock = DockStyle.Right,
                Width = 300,
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            pnlFooter.Controls.Add(lblSignTenant);
            pnlFooter.Controls.Add(_lblStaffSignature);

            // Add all to content panel (Order matters for Dock.Top: last added is top)
            _pnlContent.Controls.Add(pnlFooter);
            _pnlContent.Controls.Add(_pnlBankInfo);
            _pnlContent.Controls.Add(_lblPayments);
            _pnlContent.Controls.Add(_lblSummary);
            _pnlContent.Controls.Add(pnlTableContainer);
            _pnlContent.Controls.Add(_lblTenantInfo);
            _pnlContent.Controls.Add(line1);
            _pnlContent.Controls.Add(pnlHeader);
        }

        private void AddTableRow(string content, string detail, string amount, bool isHeader = false)
        {
            var font = isHeader ? new Font("Segoe UI", 10, FontStyle.Bold) : new Font("Segoe UI", 9, FontStyle.Regular);
            var backColor = isHeader ? Color.FromArgb(230, 235, 245) : Color.White;
            var foreColor = isHeader ? Color.FromArgb(30, 55, 90) : Color.Black;

            var lblContent = new Label { Text = content, Font = font, ForeColor = foreColor, BackColor = backColor, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 4, 4, 4), AutoSize = false, Height = 36 };
            var lblDetail = new Label { Text = detail, Font = font, ForeColor = foreColor, BackColor = backColor, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(4), AutoSize = false, Height = 36 };
            var lblAmount = new Label { Text = amount, Font = font, ForeColor = foreColor, BackColor = backColor, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(4, 4, 8, 4), AutoSize = false, Height = 36 };

            int row = _tableCosts.RowCount++;
            _tableCosts.Controls.Add(lblContent, 0, row);
            _tableCosts.Controls.Add(lblDetail, 1, row);
            _tableCosts.Controls.Add(lblAmount, 2, row);
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            if (_invoiceRow == null) return;
            try
            {
                int tenantId = ReadInt(_invoiceRow, "TenantId");
                if (tenantId > 0)
                {
                    var tenants = await _bll.GetTenantsAsync();
                    _tenantRow = tenants?.AsEnumerable().FirstOrDefault(r => ReadInt(r, "TenantId") == tenantId);
                }

                int branchId = ReadInt(_invoiceRow, "BranchId");
                if (branchId > 0)
                {
                    var branches = await _bll.GetBranchesAsync();
                    _branchRow = branches?.AsEnumerable().FirstOrDefault(r => ReadInt(r, "BranchId") == branchId);
                }

                int invoiceId = ReadInt(_invoiceRow, "InvoiceId");
                if (invoiceId > 0)
                    _payments = await _bll.GetPaymentsByInvoiceAsync(invoiceId);

                _utilityTypes = await _bll.GetUtilityTypesAsync();
                _utilityReadings = await _bll.GetUtilityReadingsAsync();
            }
            catch
            {
                _tenantRow = null;
                _payments = null;
                _branchRow = null;
                _utilityReadings = null;
                _utilityTypes = null;
            }

            await RenderPreviewAsync();
        }

        private async System.Threading.Tasks.Task RenderPreviewAsync()
        {
            if (_invoiceRow == null) return;

            // 1. Branch Info
            string branchName = ReadString(_branchRow, "BranchName") ?? "HỆ THỐNG NHÀ TRỌ";
            string branchAddr = ReadString(_branchRow, "Address") ?? "...";
            string branchPhone = ReadString(_branchRow, "Hotline") ?? ReadString(_branchRow, "Phone") ?? "...";
            _lblBranchInfo.Text = $"{branchName.ToUpper()}\nĐịa chỉ: {branchAddr}\nHotline: {branchPhone}";

            // 2. Invoice Meta
            string invoiceNo = ReadString(_invoiceRow, "InvoiceNumber") ?? $"HD-{ReadInt(_invoiceRow, "InvoiceId")}";
            string invoiceDate = FormatDate(ReadString(_invoiceRow, "InvoiceDate"));
            _lblInvoiceMeta.Text = $"Số: {invoiceNo}\nNgày: {invoiceDate}";

            // 3. Tenant Info
            string tenantName = ReadString(_tenantRow, "FullName") ?? ReadString(_invoiceRow, "TenantName") ?? "—";
            string roomNumber = ReadString(_invoiceRow, "RoomNumber") ?? "—";
            string fromDate = FormatDate(ReadString(_invoiceRow, "FromDate"));
            string toDate = FormatDate(ReadString(_invoiceRow, "ToDate"));
            _lblTenantInfo.Text = $"Khách hàng: {tenantName}\nPhòng: {roomNumber}\nKỳ thanh toán: {fromDate} đến {toDate}";

            // 4. Table Costs
            _tableCosts.Controls.Clear();
            _tableCosts.RowCount = 0;
            AddTableRow("NỘI DUNG", "CHI TIẾT / ĐƠN GIÁ", "THÀNH TIỀN", true);

            decimal rental = ReadDecimal(_invoiceRow, "RentalCost");
            if (rental > 0) AddTableRow("Tiền thuê phòng", "Tháng", FormatMoney(rental));

            // Detailed Utilities
            DateTime? fromDateDt = TryGetDate(_invoiceRow, "FromDate");
            DateTime? toDateDt = TryGetDate(_invoiceRow, "ToDate");
            int roomId = ReadInt(_invoiceRow, "RoomId");

            if (_utilityReadings != null && fromDateDt.HasValue && toDateDt.HasValue)
            {
                var readings = _utilityReadings.AsEnumerable()
                    .Where(r => ReadInt(r, "RoomId") == roomId &&
                                TryGetDate(r, "ReadingDate") >= fromDateDt &&
                                TryGetDate(r, "ReadingDate") <= toDateDt)
                    .ToList();

                foreach (var r in readings)
                {
                    int typeId = ReadInt(r, "UtilityTypeId");
                    string typeName = _utilityTypes?.AsEnumerable()
                        .FirstOrDefault(t => ReadInt(t, "UtilityTypeId") == typeId)?["UtilityName"]?.ToString() ?? "Dịch vụ";

                    decimal oldIdx = ReadDecimal(r, "PreviousReading");
                    decimal newIdx = ReadDecimal(r, "CurrentReading");
                    decimal usage = ReadDecimal(r, "UsageAmount");
                    decimal price = ReadDecimal(r, "UnitPrice");
                    decimal cost = ReadDecimal(r, "TotalCost");

                    string detail = $"{typeName} (Chỉ số: {oldIdx} ➔ {newIdx} = {usage})";
                    AddTableRow(detail, $"{FormatMoney(price)}", FormatMoney(cost));
                }
            }
            else
            {
                decimal utility = ReadDecimal(_invoiceRow, "UtilityCost");
                if (utility > 0) AddTableRow("Tiền điện & nước (Tổng hợp)", "Gói", FormatMoney(utility));
            }

            decimal other = ReadDecimal(_invoiceRow, "OtherCost");
            if (other > 0) AddTableRow("Chi phí khác / Bảo trì", "Lần", FormatMoney(other));

            decimal taxAmount = ReadDecimal(_invoiceRow, "TaxAmount");
            if (taxAmount > 0)
            {
                decimal taxRate = ReadDecimal(_invoiceRow, "TaxRate");
                AddTableRow($"Thuế GTGT ({taxRate}%)", "", FormatMoney(taxAmount));
            }

            // 5. Summary
            decimal total = ReadDecimal(_invoiceRow, "TotalAmount");
            decimal paid = ReadDecimal(_invoiceRow, "PaidAmount");
            decimal remaining = ReadDecimal(_invoiceRow, "RemainingAmount");

            _lblSummary.Text = $"TỔNG CỘNG: {FormatMoney(total)}\n" +
                               $"Đã thanh toán: {FormatMoney(paid)}\n" +
                               $"CÒN LẠI: {FormatMoney(remaining)}";

            string paymentsText = BuildPaymentsText();
            _lblPayments.Text = paymentsText;
            _lblPayments.Visible = !string.IsNullOrEmpty(paymentsText);

            // 6. Bank & QR Logic
            try
            {
                string bankId = null;
                string accountNo = null;
                string accountName = null;
                string template = null;

                int branchId = ReadInt(_branchRow, "BranchId");
                if (branchId > 0)
                {
                    var branchSetting = await _bll.GetBranchBankSettingsAsync(branchId);
                    if (branchSetting.HasValue)
                    {
                        bankId = branchSetting.Value.BankId;
                        accountNo = branchSetting.Value.AccountNumber;
                        accountName = branchSetting.Value.AccountName;
                        template = branchSetting.Value.Template;
                    }
                }

                if (string.IsNullOrWhiteSpace(bankId))
                    bankId = await _bll.GetSystemSettingValueAsync("BankId") ?? "VietinBank";
                if (string.IsNullOrWhiteSpace(accountNo))
                    accountNo = await _bll.GetSystemSettingValueAsync("BankAccountNumber") ?? "0338352423";
                if (string.IsNullOrWhiteSpace(accountName))
                    accountName = await _bll.GetSystemSettingValueAsync("BankAccountName") ?? "NGUYEN TRUNG KIEN";
                if (string.IsNullOrWhiteSpace(template))
                    template = await _bll.GetSystemSettingValueAsync("BankTemplate") ?? "compact";

                string description = $"THANH TOAN HOA DON {invoiceNo}";

                string qrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png" +
                               $"?amount={remaining:0}" +
                               $"&addInfo={Uri.EscapeDataString(description)}" +
                               $"&accountName={Uri.EscapeDataString(accountName)}";

                _lblBankDetails.Text = $"NGÂN HÀNG: {bankId}\n" +
                                      $"SỐ TÀI KHOẢN: {accountNo}\n" +
                                      $"CHỦ TÀI KHOẢN: {accountName}\n\n" +
                                      $"NỘI DUNG: {description}\n" +
                                      $"SỐ TIỀN: {remaining:N0} VNĐ";

                _picQrCode.ImageLocation = qrUrl;
            }
            catch (Exception ex)
            {
                _lblBankDetails.Text = "Không thể tải thông tin ngân hàng: " + ex.Message;
            }

            // 7. Staff Info
            _lblStaffSignature.Text = $"Người lập phiếu\n(Ký và ghi rõ họ tên)\n\n\n\nNHÂN VIÊN QUẢN LÝ";

            _pnlContent.PerformLayout();
        }

        private string BuildPaymentsText()
        {
            if (_payments == null || _payments.Rows.Count == 0) return "";

            var lines = _payments.AsEnumerable()
                .OrderByDescending(r => TryGetDate(r, "PaymentDate") ?? DateTime.MinValue)
                .Select(r =>
                {
                    string date = FormatDate(ReadString(r, "PaymentDate"));
                    string amount = FormatMoney(ReadDecimal(r, "PaymentAmount"));
                    string method = ReadString(r, "PaymentMethod") ?? "—";
                    string reference = ReadString(r, "TransactionReference") ?? "";
                    return $"{date} | {amount} | {method} {reference}".Trim();
                })
                .ToList();

            return "Lịch sử thanh toán:\n" + string.Join("\n", lines);
        }

        private void ExportPdf()
        {
            if (_invoiceRow == null) return;
            string invoiceNo = ReadString(_invoiceRow, "InvoiceNumber") ?? $"HD-{ReadInt(_invoiceRow, "InvoiceId")}";

            using (var sfd = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"{invoiceNo}.pdf",
                OverwritePrompt = true
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                PrintToPdfFile(sfd.FileName);
            }
        }

        private void PrintToPdfFile(string filePath)
        {
            var printer = new PrinterSettings
            {
                PrinterName = "Microsoft Print to PDF",
                PrintToFile = true,
                PrintFileName = filePath
            };

            if (!printer.IsValid)
            {
                MessageBox.Show("Không tìm thấy máy in PDF (Microsoft Print to PDF).", "Thiếu máy in", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var doc = BuildPrintDocument(printer))
            {
                doc.PrintController = new StandardPrintController();
                doc.Print();
            }
        }

        private void PrintInvoice()
        {
            using (var dlg = new PrintDialog())
            {
                var doc = BuildPrintDocument(null);
                dlg.Document = doc;
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                doc.PrinterSettings = dlg.PrinterSettings;
                doc.Print();
            }
        }

        private PrintDocument BuildPrintDocument(PrinterSettings printerSettings)
        {
            var doc = new PrintDocument();
            if (printerSettings != null)
                doc.PrinterSettings = printerSettings;

            doc.PrintPage += (s, e) =>
            {
                using (var bmp = RenderPreviewBitmap())
                {
                    if (bmp == null) return;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                    var page = e.MarginBounds;
                    float scale = Math.Min((float)page.Width / bmp.Width, (float)page.Height / bmp.Height);
                    int w = (int)(bmp.Width * scale);
                    int h = (int)(bmp.Height * scale);
                    var rect = new Rectangle(page.Left, page.Top, w, h);
                    e.Graphics.DrawImage(bmp, rect);
                }
            };

            return doc;
        }

        private Bitmap RenderPreviewBitmap()
        {
            if (_pnlContent == null) return null;

            // We capture the _pnlContent which is fixed at 800px width
            int width = _pnlContent.Width;
            int height = _pnlContent.Height;

            // Force layout update to ensure everything is sized correctly
            _pnlContent.PerformLayout();

            Bitmap bmp = new Bitmap(width, height);
            _pnlContent.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));

            return bmp;
        }

        private static DateTime? TryGetDate(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            if (v == null || v == DBNull.Value) return null;
            if (v is DateTime dt) return dt;
            return DateTime.TryParse(v.ToString(), out var val) ? val : (DateTime?)null;
        }

        private static string FormatDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return "—";
            return DateTime.TryParse(dateStr, out var dt) ? dt.ToString("dd/MM/yyyy") : dateStr;
        }

        private static string FormatMoney(decimal value)
        {
            return value.ToString("N0") + "đ";
        }

        private static string ReadString(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            return v == null || v == DBNull.Value ? null : v.ToString();
        }

        private static int ReadInt(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0;
            return int.TryParse(row[col]?.ToString(), out var val) ? val : 0;
        }

        private static decimal ReadDecimal(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0m;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            return decimal.TryParse(v.ToString(), out var val) ? val : 0m;
        }
    }
}
