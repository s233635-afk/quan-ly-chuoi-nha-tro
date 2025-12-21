using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Unified form for managing both Invoices and Payments
    /// Combines the features of FrmInvoiceManager and FrmPaymentManager
    /// </summary>
    public class FrmInvoicePaymentUnified : Form
    {
        private const string SearchPlaceholder = "Tìm theo hóa đơn/khách/phòng...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly int? _branchId;
        private HashSet<int> _allowedBranchIds;
        private DataTable _invoiceTable;
        private DataTable _paymentTable;

        // Tab Control
        private TabControl _tabControl;
        private TabPage _tabInvoices;
        private TabPage _tabPayments;

        // Invoice Controls
        private DataGridView _gridInvoices;
        private TextBox _txtSearchInvoice;
        private ComboBox _cboInvoiceStatus;
        private Label _lblInvoiceCount;
        private Label _lblInvoiceSummary;
        private Button _btnAddInvoice, _btnEditInvoice, _btnDeleteInvoice, _btnGenerateInvoices, _btnExportInvoices, _btnRefreshInvoices;

        // Payment Controls
        private DataGridView _gridPayments;
        private TextBox _txtSearchPayment;
        private ComboBox _cboPaymentMethod;
        private DateTimePicker _dtFromDate;
        private DateTimePicker _dtToDate;
        private Label _lblPaymentCount;
        private Label _lblPaymentTotal;
        private Button _btnAddPayment, _btnEditPayment, _btnDeletePayment, _btnExportPayments, _btnRefreshPayments;
        private Button _btnRoomPayment;
        private Button _btnBulkPayment;

        public FrmInvoicePaymentUnified() : this(null)
        {
        }

        public FrmInvoicePaymentUnified(int? branchId)
        {
            _branchId = branchId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản Lý Hóa Đơn & Thanh Toán";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1400;
            Height = 800;
            BackColor = UiKit.AppBackground;
            Font = new Font("Segoe UI", 10F);

            // Initialize Tab Control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(6, 6)
            };

            _tabInvoices = new TabPage("📋 Hóa Đơn");
            _tabPayments = new TabPage("💳 Thanh Toán");

            // Initialize Invoice Tab
            InitializeInvoicesTab();

            // Initialize Payment Tab
            InitializePaymentsTab();

            _tabControl.TabPages.Add(_tabInvoices);
            _tabControl.TabPages.Add(_tabPayments);

            Controls.Add(_tabControl);
            Load += async (s, e) => await LoadAllDataAsync();
        }

        private void InitializeInvoicesTab()
        {
            _gridInvoices = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeColumns = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _gridInvoices.DoubleClick += (s, e) => EditSelectedInvoice();
            _gridInvoices.CellDoubleClick += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && _gridInvoices.Columns[e.ColumnIndex].Name == "InvoiceNumber")
                    ShowInvoiceDetail();
            };
            _gridInvoices.SelectionChanged += (s, e) => UpdateInvoiceSummary();
            UiKit.StyleGrid(_gridInvoices);

            _txtSearchInvoice = new TextBox { Width = 280 };
            _txtSearchInvoice.TextChanged += (s, e) => ApplyInvoiceFilter();
            _txtSearchInvoice.GotFocus += (s, e) =>
            {
                if (_txtSearchInvoice.Text == SearchPlaceholder)
                    _txtSearchInvoice.Text = string.Empty;
            };
            _txtSearchInvoice.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearchInvoice.Text))
                    _txtSearchInvoice.Text = SearchPlaceholder;
            };
            _txtSearchInvoice.Text = SearchPlaceholder;

            _cboInvoiceStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            _cboInvoiceStatus.Items.AddRange(new object[] { "Tất cả", "Issued", "PartialPaid", "Paid", "Overdue" });
            _cboInvoiceStatus.SelectedIndex = 0;
            _cboInvoiceStatus.SelectedIndexChanged += (s, e) => ApplyInvoiceFilter();

            _lblInvoiceCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblInvoiceSummary = new Label { AutoSize = true, Text = "Tổng tiền: 0 | Đã thu: 0 | Còn nợ: 0", ForeColor = UiKit.MutedText };

            _btnAddInvoice = UiKit.MakeButton("➕ Thêm", UiKit.Primary, (s, e) => AddNewInvoice(), 100);
            _btnEditInvoice = UiKit.MakeButton("✏️ Sửa", UiKit.Primary, (s, e) => EditSelectedInvoice(), 100);
            _btnDeleteInvoice = UiKit.MakeButton("🗑️ Xóa", UiKit.Danger, async (s, e) => await DeleteSelectedInvoiceAsync(), 100);
            _btnGenerateInvoices = UiKit.MakeButton("📅 Tạo tháng", UiKit.Warning, async (s, e) => await GenerateMonthlyAsync(), 140);
            _btnExportInvoices = UiKit.MakeButton("📊 Xuất CSV", UiKit.Purple, (s, e) => ExportInvoicesCsv(), 110);
            _btnRefreshInvoices = UiKit.MakeButton("🔄 Tải lại", UiKit.Primary, async (s, e) => await LoadInvoicesAsync(), 110);

            // Top Panel with Controls
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(12), BackColor = Color.White };
            topPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.DrawLine(pen, 0, topPanel.Height - 1, topPanel.Width, topPanel.Height - 1);
                }
            };

            var actionsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Height = 40
            };
            actionsPanel.Controls.Add(_btnAddInvoice);
            actionsPanel.Controls.Add(_btnEditInvoice);
            actionsPanel.Controls.Add(_btnDeleteInvoice);
            actionsPanel.Controls.Add(_btnGenerateInvoices);
            actionsPanel.Controls.Add(_btnExportInvoices);
            actionsPanel.Controls.Add(_btnRefreshInvoices);

            var searchPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, ForeColor = UiKit.MutedText };
            _txtSearchInvoice.Location = new Point(lblSearch.Width + 8, 8);

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, ForeColor = UiKit.MutedText };
            lblStatus.Location = new Point(_txtSearchInvoice.Right + 20, 8);
            _cboInvoiceStatus.Location = new Point(lblStatus.Right + 8, 5);

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(_txtSearchInvoice);
            searchPanel.Controls.Add(lblStatus);
            searchPanel.Controls.Add(_cboInvoiceStatus);

            topPanel.Controls.Add(searchPanel);
            topPanel.Controls.Add(actionsPanel);

            // Bottom Panel for Summary
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(12, 10, 12, 10), BackColor = Color.White };
            bottomPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.DrawLine(pen, 0, 0, bottomPanel.Width, 0);
                }
            };
            _lblInvoiceCount.Location = new Point(0, 6);
            _lblInvoiceSummary.Location = new Point(0, 28);
            bottomPanel.Controls.Add(_lblInvoiceCount);
            bottomPanel.Controls.Add(_lblInvoiceSummary);

            var gridPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BackColor };
            gridPanel.Controls.Add(_gridInvoices);

            _tabInvoices.Controls.Add(gridPanel);
            _tabInvoices.Controls.Add(bottomPanel);
            _tabInvoices.Controls.Add(topPanel);
        }

        private void InitializePaymentsTab()
        {
            _gridPayments = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeColumns = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _gridPayments.DoubleClick += (s, e) => EditSelectedPayment();
            _gridPayments.SelectionChanged += (s, e) => UpdatePaymentSummary();
            UiKit.StyleGrid(_gridPayments);

            _txtSearchPayment = new TextBox { Width = 280 };
            _txtSearchPayment.TextChanged += (s, e) => ApplyPaymentFilter();
            _txtSearchPayment.GotFocus += (s, e) =>
            {
                if (_txtSearchPayment.Text == SearchPlaceholder)
                    _txtSearchPayment.Text = string.Empty;
            };
            _txtSearchPayment.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearchPayment.Text))
                    _txtSearchPayment.Text = SearchPlaceholder;
            };
            _txtSearchPayment.Text = SearchPlaceholder;

            _cboPaymentMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            _cboPaymentMethod.Items.AddRange(new object[] { "Tất cả", "Cash", "Transfer", "Check", "Card" });
            _cboPaymentMethod.SelectedIndex = 0;
            _cboPaymentMethod.SelectedIndexChanged += (s, e) => ApplyPaymentFilter();

            _dtFromDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-1) };
            _dtFromDate.ValueChanged += (s, e) => ApplyPaymentFilter();

            _dtToDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            _dtToDate.ValueChanged += (s, e) => ApplyPaymentFilter();

            _lblPaymentCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblPaymentTotal = new Label { AutoSize = true, Text = "Tổng thu: 0", ForeColor = UiKit.MutedText };

            _btnAddPayment = UiKit.MakeButton("➕ Thêm", UiKit.Primary, (s, e) => AddNewPayment(), 100);
            _btnEditPayment = UiKit.MakeButton("✏️ Sửa", UiKit.Primary, (s, e) => EditSelectedPayment(), 100);
            _btnDeletePayment = UiKit.MakeButton("🗑️ Xóa", UiKit.Danger, async (s, e) => await DeleteSelectedPaymentAsync(), 100);
            _btnExportPayments = UiKit.MakeButton("📊 Xuất CSV", UiKit.Purple, (s, e) => ExportPaymentsCsv(), 110);
            _btnRefreshPayments = UiKit.MakeButton("🔄 Tải lại", UiKit.Primary, async (s, e) => await LoadPaymentsAsync(), 110);
            _btnRoomPayment = UiKit.MakeButton("🏠 Thu tiền phòng", Color.FromArgb(0, 123, 255), (s, e) => OpenRoomPaymentSelector(), 140);
            _btnBulkPayment = UiKit.MakeButton("🧾 Thu tiền tất cả", UiKit.Success, async (s, e) => await OpenBulkPaymentRunnerAsync(), 150);

            // Top Panel
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(12), BackColor = Color.White };
            topPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.DrawLine(pen, 0, topPanel.Height - 1, topPanel.Width, topPanel.Height - 1);
                }
            };

            var actionsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Height = 40
            };
            actionsPanel.Controls.Add(_btnAddPayment);
            actionsPanel.Controls.Add(_btnEditPayment);
            actionsPanel.Controls.Add(_btnDeletePayment);
            actionsPanel.Controls.Add(_btnRoomPayment);
            actionsPanel.Controls.Add(_btnBulkPayment);
            actionsPanel.Controls.Add(_btnExportPayments);
            actionsPanel.Controls.Add(_btnRefreshPayments);

            var filterPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new Point(0, 8) };
            _txtSearchPayment.Location = new Point(lblSearch.Width + 8, 8);

            var lblMethod = new Label { Text = "Hình thức:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new Point(_txtSearchPayment.Right + 20, 8) };
            _cboPaymentMethod.Location = new Point(lblMethod.Right + 8, 5);

            var lblFrom = new Label { Text = "Từ:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new Point(_cboPaymentMethod.Right + 20, 8) };
            _dtFromDate.Location = new Point(lblFrom.Right + 8, 5);
            _dtFromDate.Width = 120;

            var lblTo = new Label { Text = "Đến:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new Point(_dtFromDate.Right + 20, 8) };
            _dtToDate.Location = new Point(lblTo.Right + 8, 5);
            _dtToDate.Width = 120;

            filterPanel.Controls.Add(lblSearch);
            filterPanel.Controls.Add(_txtSearchPayment);
            filterPanel.Controls.Add(lblMethod);
            filterPanel.Controls.Add(_cboPaymentMethod);
            filterPanel.Controls.Add(lblFrom);
            filterPanel.Controls.Add(_dtFromDate);
            filterPanel.Controls.Add(lblTo);
            filterPanel.Controls.Add(_dtToDate);

            topPanel.Controls.Add(filterPanel);
            topPanel.Controls.Add(actionsPanel);

            // Bottom Panel
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(12, 10, 12, 10), BackColor = Color.White };
            bottomPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.DrawLine(pen, 0, 0, bottomPanel.Width, 0);
                }
            };
            _lblPaymentCount.Location = new Point(0, 6);
            _lblPaymentTotal.Location = new Point(0, 28);
            bottomPanel.Controls.Add(_lblPaymentCount);
            bottomPanel.Controls.Add(_lblPaymentTotal);

            var gridPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BackColor };
            gridPanel.Controls.Add(_gridPayments);

            _tabPayments.Controls.Add(gridPanel);
            _tabPayments.Controls.Add(bottomPanel);
            _tabPayments.Controls.Add(topPanel);
        }

        private async System.Threading.Tasks.Task LoadAllDataAsync()
        {
            try
            {
                await LoadInvoicesAsync();
                await LoadPaymentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadInvoicesAsync()
        {
            try
            {
                _invoiceTable = await _bll.GetInvoicesViewAsync();
                if (_branchId.HasValue)
                {
                    _invoiceTable = FilterByBranch(_invoiceTable, _branchId);
                }
                else
                {
                    await EnsureAllowedBranchScopeAsync();
                    _invoiceTable = AdminBranchScope.FilterByBranchIds(_invoiceTable, _allowedBranchIds);
                }
                _gridInvoices.DataSource = _invoiceTable;
                ApplyInvoiceFilter();
                FormatInvoiceGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadPaymentsAsync()
        {
            try
            {
                _paymentTable = await _bll.GetPaymentsViewAsync();
                if (_branchId.HasValue)
                {
                    _paymentTable = FilterByBranch(_paymentTable, _branchId);
                }
                else
                {
                    await EnsureAllowedBranchScopeAsync();
                    _paymentTable = AdminBranchScope.FilterByBranchIds(_paymentTable, _allowedBranchIds);
                }
                _gridPayments.DataSource = _paymentTable;
                ApplyPaymentFilter();
                FormatPaymentGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static DataTable FilterByBranch(DataTable dt, int? branchId)
        {
            if (dt == null || !branchId.HasValue || !dt.Columns.Contains("BranchId")) return dt;
            var filtered = dt.Clone();
            foreach (DataRow r in dt.Rows)
            {
                if (int.TryParse(r["BranchId"]?.ToString(), out var b) && b == branchId.Value)
                    filtered.ImportRow(r);
            }
            return filtered;
        }

        private void FormatInvoiceGrid()
        {
            var columnMapping = new Dictionary<string, string>
            {
                { "InvoiceId", "Mã hóa đơn" },
                { "InvoiceNumber", "Số hóa đơn" },
                { "TenantName", "Khách thuê" },
                { "RoomNumber", "Phòng" },
                { "InvoiceDate", "Ngày lập" },
                { "FromDate", "Từ" },
                { "ToDate", "Đến" },
                { "RentalCost", "Tiền phòng" },
                { "UtilityCost", "Tiền dịch vụ" },
                { "OtherCost", "Chi phí khác" },
                { "TotalAmount", "Tổng tiền" },
                { "PaidAmount", "Đã thu" },
                { "RemainingAmount", "Còn nợ" },
                { "Status", "Trạng thái" },
                { "DueDate", "Hạn thanh toán" }
            };

            foreach (DataGridViewColumn col in _gridInvoices.Columns)
            {
                if (columnMapping.ContainsKey(col.Name))
                    col.HeaderText = columnMapping[col.Name];

                if (col.Name.EndsWith("Cost") || col.Name.EndsWith("Amount"))
                {
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                if (col.Name.Contains("Date"))
                {
                    col.DefaultCellStyle.Format = "dd/MM/yyyy";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        private void FormatPaymentGrid()
        {
            var columnMapping = new Dictionary<string, string>
            {
                { "PaymentId", "Mã TT" },
                { "InvoiceNumber", "Hóa đơn" },
                { "TenantName", "Khách" },
                { "RoomNumber", "Phòng" },
                { "PaymentDate", "Ngày TT" },
                { "Amount", "Số tiền" },
                { "Method", "Hình thức" },
                { "Reference", "Tham chiếu" },
                { "Notes", "Ghi chú" }
            };

            foreach (DataGridViewColumn col in _gridPayments.Columns)
            {
                if (columnMapping.ContainsKey(col.Name))
                    col.HeaderText = columnMapping[col.Name];

                if (col.Name == "Amount")
                {
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                if (col.Name == "PaymentDate")
                {
                    col.DefaultCellStyle.Format = "dd/MM/yyyy";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        private void ApplyInvoiceFilter()
        {
            if (_invoiceTable == null) return;
            var keyword = _txtSearchInvoice.Text == SearchPlaceholder ? string.Empty : _txtSearchInvoice.Text.Trim();
            keyword = keyword.Replace("'", "''");
            var status = _cboInvoiceStatus.SelectedItem?.ToString();

            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kwFilters = new List<string>();
                if (_invoiceTable.Columns.Contains("InvoiceNumber")) kwFilters.Add($"InvoiceNumber LIKE '%{keyword}%'");
                if (_invoiceTable.Columns.Contains("TenantName")) kwFilters.Add($"TenantName LIKE '%{keyword}%'");
                if (_invoiceTable.Columns.Contains("RoomNumber")) kwFilters.Add($"RoomNumber LIKE '%{keyword}%'");
                if (kwFilters.Count > 0) filters.Add("(" + string.Join(" OR ", kwFilters) + ")");
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "Tất cả" && _invoiceTable.Columns.Contains("Status"))
                filters.Add($"Status = '{status.Replace("'", "''")}'");

            _invoiceTable.DefaultView.RowFilter = filters.Count > 0 ? string.Join(" AND ", filters) : string.Empty;
            _lblInvoiceCount.Text = $"Tổng: {_invoiceTable.DefaultView.Count}";
            UpdateInvoiceSummary();
        }

        private void ApplyPaymentFilter()
        {
            if (_paymentTable == null) return;
            var keyword = _txtSearchPayment.Text == SearchPlaceholder ? string.Empty : _txtSearchPayment.Text.Trim();
            keyword = keyword.Replace("'", "''");
            var method = _cboPaymentMethod.SelectedItem?.ToString();

            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kwFilters = new List<string>();
                if (_paymentTable.Columns.Contains("InvoiceNumber")) kwFilters.Add($"InvoiceNumber LIKE '%{keyword}%'");
                if (_paymentTable.Columns.Contains("TenantName")) kwFilters.Add($"TenantName LIKE '%{keyword}%'");
                if (_paymentTable.Columns.Contains("RoomNumber")) kwFilters.Add($"RoomNumber LIKE '%{keyword}%'");
                if (kwFilters.Count > 0) filters.Add("(" + string.Join(" OR ", kwFilters) + ")");
            }

            if (!string.IsNullOrWhiteSpace(method) && method != "Tất cả" && _paymentTable.Columns.Contains("Method"))
                filters.Add($"Method = '{method.Replace("'", "''")}'");

            if (_paymentTable.Columns.Contains("PaymentDate"))
            {
                filters.Add($"PaymentDate >= #{_dtFromDate.Value:yyyy-MM-dd}#");
                filters.Add($"PaymentDate <= #{_dtToDate.Value:yyyy-MM-dd}#");
            }

            _paymentTable.DefaultView.RowFilter = filters.Count > 0 ? string.Join(" AND ", filters) : string.Empty;
            _lblPaymentCount.Text = $"Tổng: {_paymentTable.DefaultView.Count}";
            UpdatePaymentSummary();
        }

        private void UpdateInvoiceSummary()
        {
            if (_invoiceTable == null) return;
            decimal total = 0m, paid = 0m, remaining = 0m;
            foreach (DataRowView view in _invoiceTable.DefaultView)
            {
                var row = view.Row;
                total += ReadDecimal(row, "TotalAmount");
                paid += ReadDecimal(row, "PaidAmount");
                remaining += ReadDecimal(row, "RemainingAmount");
            }
            _lblInvoiceSummary.Text = $"Tổng tiền: {total:N0} | Đã thu: {paid:N0} | Còn nợ: {remaining:N0}";
        }

        private void UpdatePaymentSummary()
        {
            if (_paymentTable == null) return;
            decimal total = 0m;
            foreach (DataRowView view in _paymentTable.DefaultView)
            {
                total += ReadDecimal(view.Row, "Amount");
            }
            _lblPaymentTotal.Text = $"Tổng thu: {total:N0}";
        }

        private static decimal ReadDecimal(DataRow row, string col)
        {
            if (row == null || !row.Table.Columns.Contains(col)) return 0m;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
            if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed)) return parsed;
            return 0m;
        }

        private DataRow GetCurrentInvoiceRow()
        {
            if (_gridInvoices.CurrentRow?.DataBoundItem is DataRowView drv) return drv.Row;
            return null;
        }

        private DataRow GetCurrentPaymentRow()
        {
            if (_gridPayments.CurrentRow?.DataBoundItem is DataRowView drv) return drv.Row;
            return null;
        }

        // Invoice Actions
        private void AddNewInvoice()
        {
            using (var frm = new FrmInvoiceEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadInvoicesAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private void EditSelectedInvoice()
        {
            var row = GetCurrentInvoiceRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một hóa đơn để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmInvoiceEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadInvoicesAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedInvoiceAsync()
        {
            var row = GetCurrentInvoiceRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một hóa đơn để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int invoiceId = Convert.ToInt32(row["InvoiceId"]);
            decimal paid = ReadDecimal(row, "PaidAmount");
            bool hasPayment = paid > 0;

            string msg = hasPayment
                ? $"Hóa đơn ID {invoiceId} đã có thanh toán. Xóa cả lịch sử thanh toán?"
                : $"Xóa hóa đơn ID {invoiceId}?";

            if (MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                await _bll.DeleteInvoiceAsync(invoiceId, deletePaymentsFirst: hasPayment);
                await LoadInvoicesAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PaySelectedInvoice()
        {
            var row = GetCurrentInvoiceRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một hóa đơn để thu tiền.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal remaining = ReadDecimal(row, "RemainingAmount");
            if (remaining <= 0)
            {
                MessageBox.Show("Hóa đơn đã thanh toán đủ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show tenant info and payment form
            using (var frm = new FrmPaymentWithTenantInfo(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadAllDataAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private void ShowInvoiceDetail()
        {
            var row = GetCurrentInvoiceRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một hóa đơn để xem chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmInvoiceEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadInvoicesAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task GenerateMonthlyAsync()
        {
            using (var dlg = new MonthlyInvoiceDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    int created = await _bll.GenerateMonthlyInvoicesAsync(dlg.SelectedYear, dlg.SelectedMonth, DateTime.Today, dlg.DueDay);
                    await LoadInvoicesAsync();
                    AdminEvents.NotifyDataChanged();
                    MessageBox.Show($"Đã tạo {created} hóa đơn cho {dlg.SelectedMonth:00}/{dlg.SelectedYear}.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tạo hóa đơn tháng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportInvoicesCsv()
        {
            if (_invoiceTable == null || _invoiceTable.DefaultView.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = $"Invoices_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                OverwritePrompt = true
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    WriteCsvFromView(_invoiceTable.DefaultView, sfd.FileName);
                    MessageBox.Show("Đã xuất CSV: " + sfd.FileName, "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Payment Actions
        private void AddNewPayment()
        {
            MessageBox.Show("Vui lòng chọn hóa đơn từ tab 'Hóa Đơn' rồi ấn nút 'Thu tiền'.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenRoomPaymentSelector()
        {
            using (var frm = new FrmRoomPaymentSelector(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadAllDataAsync();
                }
            }
        }

        private async Task OpenBulkPaymentRunnerAsync()
        {
            using (var frm = new FrmBulkPaymentRunner(_bll))
            {
                frm.ShowDialog(this);
            }
            await LoadAllDataAsync();
        }

        private void EditSelectedPayment()
        {
            var row = GetCurrentPaymentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một thanh toán để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get invoice info
            int paymentId = Convert.ToInt32(row["PaymentId"]);
            MessageBox.Show("Tính năng sửa thanh toán sẽ được cập nhật sớm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async System.Threading.Tasks.Task DeleteSelectedPaymentAsync()
        {
            var row = GetCurrentPaymentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một thanh toán để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int paymentId = Convert.ToInt32(row["PaymentId"]);
            if (MessageBox.Show($"Xóa thanh toán ID {paymentId}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                await _bll.DeletePaymentAsync(paymentId);
                await LoadPaymentsAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportPaymentsCsv()
        {
            if (_paymentTable == null || _paymentTable.DefaultView.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = $"Payments_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                OverwritePrompt = true
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    WriteCsvFromView(_paymentTable.DefaultView, sfd.FileName);
                    MessageBox.Show("Đã xuất CSV: " + sfd.FileName, "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void WriteCsvFromView(DataView view, string path)
        {
            var sb = new StringBuilder();
            var cols = view.Table.Columns.Cast<DataColumn>().ToArray();

            sb.AppendLine(string.Join(",", cols.Select(c => EscapeCsv(c.ColumnName))));
            foreach (DataRowView rv in view)
            {
                sb.AppendLine(string.Join(",", cols.Select(c => EscapeCsv(rv.Row[c]))));
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static string EscapeCsv(object value)
        {
            string s = value == null || value == DBNull.Value ? "" : value.ToString();
            bool needQuote = s.Contains(",") || s.Contains("\"") || s.Contains("\n") || s.Contains("\r");
            s = s.Replace("\"", "\"\"");
            return needQuote ? "\"" + s + "\"" : s;
        }

        private async System.Threading.Tasks.Task EnsureAllowedBranchScopeAsync()
        {
            if (_allowedBranchIds != null && _allowedBranchIds.Count > 0) return;
            try
            {
                var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                _allowedBranchIds = AdminBranchScope.GetAllowedBranchIds(branches);
            }
            catch
            {
                _allowedBranchIds = new HashSet<int>();
            }
        }

        private class MonthlyInvoiceDialog : Form
        {
            private NumericUpDown _numYear;
            private NumericUpDown _numMonth;
            private NumericUpDown _numDueDay;
            private Button _btnOk;
            private Button _btnCancel;

            public int SelectedYear => (int)_numYear.Value;
            public int SelectedMonth => (int)_numMonth.Value;
            public int? DueDay => _numDueDay.Value > 0 ? (int?)_numDueDay.Value : null;

            public MonthlyInvoiceDialog()
            {
                Text = "Tạo hóa đơn tháng";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(360, 180);

                var now = DateTime.Today;
                _numYear = new NumericUpDown { Minimum = 2000, Maximum = 2100, Value = now.Year, Location = new Point(130, 20), Width = 180 };
                _numMonth = new NumericUpDown { Minimum = 1, Maximum = 12, Value = now.Month, Location = new Point(130, 55), Width = 180 };
                _numDueDay = new NumericUpDown { Minimum = 0, Maximum = 31, Value = 10, Location = new Point(130, 90), Width = 180 };

                Controls.Add(new Label { Text = "Năm", AutoSize = true, Location = new Point(20, 24) });
                Controls.Add(_numYear);
                Controls.Add(new Label { Text = "Tháng", AutoSize = true, Location = new Point(20, 59) });
                Controls.Add(_numMonth);
                Controls.Add(new Label { Text = "Hạn (ngày, 0=auto)", AutoSize = true, Location = new Point(20, 94) });
                Controls.Add(_numDueDay);

                _btnOk = new Button { Text = "Tạo", Width = 100, Location = new Point(130, 130) };
                _btnCancel = new Button { Text = "Hủy", Width = 100, Location = new Point(240, 130) };
                _btnOk.Click += (s, e) => DialogResult = DialogResult.OK;
                _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
                Controls.Add(_btnOk);
                Controls.Add(_btnCancel);
            }
        }
    }
}
