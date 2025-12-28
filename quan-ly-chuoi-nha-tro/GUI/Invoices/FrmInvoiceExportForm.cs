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

        private Panel _previewHost;
        private Label _lblTitle;
        private Label _lblMeta;
        private Label _lblTenant;
        private Label _lblRoom;
        private Label _lblPeriod;
        private Label _lblAmounts;
        private Label _lblSummary;
        private Label _lblPayments;

        private Button _btnExportPdf;
        private Button _btnPrint;
        private Button _btnClose;

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
            Width = 900;
            Height = 720;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);

            var main = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = Color.FromArgb(240, 242, 245) };

            _previewHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
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
            var header = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.FromArgb(0, 120, 215), Padding = new Padding(16) };
            _lblTitle = new Label
            {
                Text = "HÓA ĐƠN THANH TOÁN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 40
            };
            _lblMeta = new Label
            {
                Text = "Số hóa đơn | Trạng thái | Ngày lập",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 26
            };
            header.Controls.Add(_lblMeta);
            header.Controls.Add(_lblTitle);

            var infoGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(0, 12, 0, 0)
            };
            infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _lblTenant = MakeInfoBox("Khách thuê", "—");
            _lblRoom = MakeInfoBox("Phòng", "—");
            _lblPeriod = MakeInfoBox("Kỳ hóa đơn", "—");
            _lblAmounts = MakeInfoBox("Chi tiết tiền", "—");

            infoGrid.Controls.Add(_lblTenant, 0, 0);
            infoGrid.Controls.Add(_lblRoom, 1, 0);
            infoGrid.Controls.Add(_lblPeriod, 0, 1);
            infoGrid.Controls.Add(_lblAmounts, 1, 1);

            _lblSummary = new Label
            {
                Text = "Tổng cộng",
                AutoSize = false,
                Height = 70,
                Dock = DockStyle.Top,
                Padding = new Padding(12),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 55, 90),
                BackColor = Color.FromArgb(245, 249, 255),
                Margin = new Padding(0, 10, 0, 0)
            };

            _lblPayments = new Label
            {
                Text = "Lịch sử thanh toán",
                AutoSize = false,
                Height = 120,
                Dock = DockStyle.Top,
                Padding = new Padding(12),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70),
                BackColor = Color.WhiteSmoke,
                Margin = new Padding(0, 10, 0, 0)
            };

            _previewHost.Controls.Add(_lblPayments);
            _previewHost.Controls.Add(_lblSummary);
            _previewHost.Controls.Add(infoGrid);
            _previewHost.Controls.Add(header);
        }

        private Label MakeInfoBox(string title, string value)
        {
            return new Label
            {
                Text = $"{title}:\n{value}",
                AutoSize = false,
                Height = 90,
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 60, 60),
                Margin = new Padding(0, 0, 8, 8)
            };
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

                int invoiceId = ReadInt(_invoiceRow, "InvoiceId");
                if (invoiceId > 0)
                    _payments = await _bll.GetPaymentsByInvoiceAsync(invoiceId);
            }
            catch
            {
                _tenantRow = null;
                _payments = null;
            }

            RenderPreview();
        }

        private void RenderPreview()
        {
            if (_invoiceRow == null) return;

            string invoiceNo = ReadString(_invoiceRow, "InvoiceNumber") ?? $"HD-{ReadInt(_invoiceRow, "InvoiceId")}";
            string status = ReadString(_invoiceRow, "Status") ?? "Issued";
            string invoiceDate = FormatDate(ReadString(_invoiceRow, "InvoiceDate"));
            _lblMeta.Text = $"{invoiceNo} | {status} | {invoiceDate}";

            string tenantName = ReadString(_tenantRow, "FullName") ?? ReadString(_invoiceRow, "TenantName") ?? "—";
            string phone = ReadString(_tenantRow, "PhoneNumber") ?? ReadString(_tenantRow, "Phone") ?? "—";
            string email = ReadString(_tenantRow, "Email") ?? "—";
            _lblTenant.Text = $"Khách thuê:\n{tenantName}\nSĐT: {phone}\nEmail: {email}";

            string roomNumber = ReadString(_invoiceRow, "RoomNumber") ?? ReadString(_invoiceRow, "RoomId");
            string branchId = ReadString(_invoiceRow, "BranchId") ?? "—";
            _lblRoom.Text = $"Phòng:\n{roomNumber}\nChi nhánh ID: {branchId}";

            string fromDate = FormatDate(ReadString(_invoiceRow, "FromDate"));
            string toDate = FormatDate(ReadString(_invoiceRow, "ToDate"));
            string dueDate = FormatDate(ReadString(_invoiceRow, "DueDate"));
            _lblPeriod.Text = $"Kỳ hóa đơn:\n{fromDate} → {toDate}\nHạn: {dueDate}";

            decimal rental = ReadDecimal(_invoiceRow, "RentalCost");
            decimal utility = ReadDecimal(_invoiceRow, "UtilityCost");
            decimal other = ReadDecimal(_invoiceRow, "OtherCost");
            decimal taxRate = ReadDecimal(_invoiceRow, "TaxRate");
            decimal taxAmount = ReadDecimal(_invoiceRow, "TaxAmount");
            decimal total = ReadDecimal(_invoiceRow, "TotalAmount");
            decimal paid = ReadDecimal(_invoiceRow, "PaidAmount");
            decimal remaining = ReadDecimal(_invoiceRow, "RemainingAmount");

            _lblAmounts.Text = $"Chi tiết tiền:\nTiền phòng: {FormatMoney(rental)}\nDịch vụ: {FormatMoney(utility)}\nKhác: {FormatMoney(other)}";
            _lblSummary.Text = $"Tổng: {FormatMoney(total)} | Thuế: {taxRate:N2}% ({FormatMoney(taxAmount)})\n" +
                               $"Đã thu: {FormatMoney(paid)} | Còn nợ: {FormatMoney(remaining)}";

            _lblPayments.Text = BuildPaymentsText();
        }

        private string BuildPaymentsText()
        {
            if (_payments == null || _payments.Rows.Count == 0)
                return "Lịch sử thanh toán:\nChưa có thanh toán.";

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
            if (_previewHost == null) return null;
            var size = _previewHost.DisplayRectangle.Size;
            if (size.Width < 1 || size.Height < 1) return null;

            var bmp = new Bitmap(size.Width, size.Height);
            _previewHost.DrawToBitmap(bmp, new Rectangle(Point.Empty, size));
            return bmp;
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

        private static DateTime? TryGetDate(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            return DateTime.TryParse(row[col]?.ToString(), out var dt) ? dt : (DateTime?)null;
        }
    }
}
