using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmInvoiceDetailForm : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly int _invoiceId;
        private readonly DataRow _invoiceRow;

        public FrmInvoiceDetailForm(AdminDataBLL bll, int invoiceId, DataRow invoiceRow)
        {
            _bll = bll;
            _invoiceId = invoiceId;
            _invoiceRow = invoiceRow;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = $"Chi tiết hóa đơn #{_invoiceId}";
            StartPosition = FormStartPosition.CenterParent;
            Width = 900;
            Height = 700;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);

            var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20) };

            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.FromArgb(0, 120, 215), Padding = new Padding(16) };
            headerPanel.ForeColor = Color.White;

            var lblInvoiceNo = new Label
            {
                Text = $"Hóa đơn: {ReadString(_invoiceRow, "InvoiceNumber")}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false
            };

            var lblStatus = new Label
            {
                Text = $"Trạng thái: {ReadString(_invoiceRow, "Status")} | Ngày lập: {FormatDate(ReadString(_invoiceRow, "InvoiceDate"))}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 25,
                AutoSize = false
            };

            var lblAmount = new Label
            {
                Text = $"Tổng tiền: {FormatMoney(ReadDecimal(_invoiceRow, "TotalAmount"))} | Đã thu: {FormatMoney(ReadDecimal(_invoiceRow, "PaidAmount"))} | Còn nợ: {FormatMoney(ReadDecimal(_invoiceRow, "RemainingAmount"))}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false
            };

            headerPanel.Controls.Add(lblAmount);
            headerPanel.Controls.Add(lblStatus);
            headerPanel.Controls.Add(lblInvoiceNo);

            var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 10, 0, 0) };
            var tabControl = new TabControl { Dock = DockStyle.Fill, BackColor = Color.White };

            var tabTenant = new TabPage("Thông tin khách thuê") { BackColor = Color.White, Padding = new Padding(12) };
            var tenantPanel = BuildTenantInfoPanel();
            tabTenant.Controls.Add(tenantPanel);
            tabControl.TabPages.Add(tabTenant);

            var tabDetails = new TabPage("Chi tiết hóa đơn") { BackColor = Color.White, Padding = new Padding(12) };
            var detailsPanel = BuildInvoiceDetailsPanel();
            tabDetails.Controls.Add(detailsPanel);
            tabControl.TabPages.Add(tabDetails);

            var tabPayments = new TabPage("Lịch sử thanh toán") { BackColor = Color.White, Padding = new Padding(12) };
            var paymentsPanel = BuildPaymentsPanel();
            tabPayments.Controls.Add(paymentsPanel);
            tabControl.TabPages.Add(tabPayments);

            contentPanel.Controls.Add(tabControl);

            var footerPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(245, 247, 250), Padding = new Padding(12) };
            var btnClose = new Button
            {
                Text = "Đóng",
                Width = 100,
                Height = 36,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(Width - 120, 7)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();
            footerPanel.Controls.Add(btnClose);

            mainPanel.Controls.Add(contentPanel);
            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(footerPanel);

            Controls.Add(mainPanel);
        }

        private Panel BuildTenantInfoPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddRow(layout, "Tên khách thuê:", ReadString(_invoiceRow, "TenantName"));
            AddRow(layout, "Phòng:", ReadString(_invoiceRow, "RoomNumber"));
            AddRow(layout, "Chi nhánh:", ReadString(_invoiceRow, "BranchName"));
            AddRow(layout, "Từ ngày:", FormatDate(ReadString(_invoiceRow, "FromDate")));
            AddRow(layout, "Đến ngày:", FormatDate(ReadString(_invoiceRow, "ToDate")));
            AddRow(layout, "Ngày hạn:", FormatDate(ReadString(_invoiceRow, "DueDate")));

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel BuildInvoiceDetailsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddRow(layout, "Tiền phòng:", FormatMoney(ReadDecimal(_invoiceRow, "RentalCost")));
            AddRow(layout, "Tiền dịch vụ:", FormatMoney(ReadDecimal(_invoiceRow, "UtilityCost")));
            AddRow(layout, "Chi phí khác:", FormatMoney(ReadDecimal(_invoiceRow, "OtherCost")));
            AddRow(layout, "Thuế (%):", FormatPercent(ReadDecimal(_invoiceRow, "TaxRate")));
            AddRow(layout, "Tiền thuế:", FormatMoney(ReadDecimal(_invoiceRow, "TaxAmount")));
            AddRow(layout, "Tổng tiền:", FormatMoney(ReadDecimal(_invoiceRow, "TotalAmount")), true);

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel BuildPaymentsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);

            panel.Controls.Add(grid);

            Load += async (s, e) =>
            {
                try
                {
                    var payments = await _bll.GetPaymentsByInvoiceAsync(_invoiceId);
                    if (payments != null && payments.Rows.Count > 0)
                    {
                        grid.DataSource = payments;
                        if (grid.Columns.Contains("PaymentId")) grid.Columns["PaymentId"].HeaderText = "ID";
                        if (grid.Columns.Contains("PaymentDate")) grid.Columns["PaymentDate"].HeaderText = "Ngày thanh toán";
                        if (grid.Columns.Contains("PaymentAmount")) grid.Columns["PaymentAmount"].HeaderText = "Số tiền";
                        if (grid.Columns.Contains("PaymentMethod")) grid.Columns["PaymentMethod"].HeaderText = "Hình thức";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải lịch sử thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            return panel;
        }

        private void AddRow(TableLayoutPanel layout, string label, string value, bool isBold = false)
        {
            var lblLabel = new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(70, 70, 70),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Margin = new Padding(0, 6, 8, 6)
            };

            var lblValue = new Label
            {
                Text = value ?? "—",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(30, 55, 90),
                Font = new Font("Segoe UI", 10, isBold ? FontStyle.Bold : FontStyle.Regular),
                Margin = new Padding(0, 6, 0, 6)
            };

            layout.Controls.Add(lblLabel);
            layout.Controls.Add(lblValue);
        }

        private string ReadString(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return null;
            var v = r[col];
            if (v == DBNull.Value || v == null) return null;
            return v.ToString();
        }

        private decimal ReadDecimal(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return 0m;
            var v = r[col];
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            return decimal.TryParse(v.ToString(), out var parsed) ? parsed : 0m;
        }

        private string FormatMoney(decimal amount)
        {
            return amount.ToString("N0");
        }

        private string FormatPercent(decimal value)
        {
            return value.ToString("N2") + "%";
        }

        private string FormatDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return "—";
            if (DateTime.TryParse(dateStr, out var dt))
                return dt.ToString("dd/MM/yyyy");
            return dateStr;
        }
    }
}
