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
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

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
        private readonly bool _showPayments;
        private readonly bool _isStaffMode;
        private HashSet<int> _allowedBranchIds;
        private DataTable _invoiceTable;
        private DataTable _paymentTable;

        // Tab Control
        private TabControl _tabControl;
        private TabPage _tabInvoices;
        private TabPage _tabPayments;

        // Invoice Controls
        private FlowLayoutPanel _invoiceCards;
        private TextBox _txtSearchInvoice;
        private ComboBox _cboInvoiceStatus;
        private Label _lblInvoiceCount;
        private Label _lblInvoiceSummary;
        private Button _btnAddInvoice, _btnEditInvoice, _btnDeleteInvoice, _btnPayInvoice, _btnExportInvoice, _btnGenerateInvoices, _btnExportInvoices, _btnRefreshInvoices;
        private (Panel Card, DataRow Row)? _selectedInvoice;

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

        public FrmInvoicePaymentUnified() : this(null, false, false)
        {
        }

        public FrmInvoicePaymentUnified(int? branchId, bool showPayments = false, bool isStaffMode = false)
        {
            _branchId = branchId;
            _showPayments = showPayments;
            _isStaffMode = isStaffMode;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
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

            // Initialize Invoice Tab
            InitializeInvoicesTab();

            _tabControl.TabPages.Add(_tabInvoices);
            if (_showPayments)
            {
                _tabPayments = new TabPage("💳 Thanh Toán");
                InitializePaymentsTab();
                _tabControl.TabPages.Add(_tabPayments);
            }

            Controls.Add(_tabControl);
            Load += async (s, e) => await LoadAllDataAsync();
        }

        private void InitializeInvoicesTab()
        {
            _invoiceCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = BackColor,
                Padding = new Padding(8)
            };

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
            _cboInvoiceStatus.Items.AddRange(new object[] { "Tất cả", "Chưa thanh toán", "Thanh toán một phần", "Đã thanh toán", "Quá hạn" });
            _cboInvoiceStatus.SelectedIndex = 0;
            _cboInvoiceStatus.SelectedIndexChanged += (s, e) => ApplyInvoiceFilter();

            _lblInvoiceCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblInvoiceSummary = new Label { AutoSize = true, Text = "Tổng tiền: 0 | Đã thu: 0 | Còn nợ: 0", ForeColor = UiKit.MutedText };

            _btnAddInvoice = UiKit.MakeButton("➕ Thêm", UiKit.Primary, (s, e) => AddNewInvoice(), 100);
            _btnEditInvoice = UiKit.MakeButton("✏️ Sửa", UiKit.Primary, (s, e) => EditSelectedInvoice(), 100);
            _btnDeleteInvoice = UiKit.MakeButton("🗑️ Xóa", UiKit.Danger, async (s, e) => await DeleteSelectedInvoiceAsync(), 100);
            _btnPayInvoice = UiKit.MakeButton("💳 Thu tiền", UiKit.Success, async (s, e) => await PaySelectedInvoiceAsync(), 120);
            _btnExportInvoice = UiKit.MakeButton("📄 Xuất hóa đơn", UiKit.Purple, (s, e) => ExportSelectedInvoice(), 140);
            _btnPayInvoice.Enabled = false;
            _btnExportInvoice.Enabled = false;
            _btnGenerateInvoices = UiKit.MakeButton("📅 Tạo tháng", UiKit.Warning, async (s, e) => await GenerateMonthlyAsync(), 140);
            _btnExportInvoices = UiKit.MakeButton("📊 Xuất CSV", UiKit.Purple, (s, e) => ExportInvoicesCsv(), 110);
            _btnRefreshInvoices = UiKit.MakeButton("🔄 Tải lại", UiKit.Primary, async (s, e) => await LoadInvoicesAsync(), 110);
            _btnDeleteInvoice.Visible = !_isStaffMode;
            _btnDeleteInvoice.Enabled = !_isStaffMode;

            // Top Panel with Controls
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 135, Padding = new Padding(15, 12, 15, 0), BackColor = Color.White };
            topPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(225, 230, 235), 1))
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
                Height = 50,
                Padding = new Padding(0, 5, 0, 10)
            };
            actionsPanel.Controls.Add(_btnAddInvoice);
            actionsPanel.Controls.Add(_btnEditInvoice);
            actionsPanel.Controls.Add(_btnDeleteInvoice);
            actionsPanel.Controls.Add(_btnPayInvoice);
            actionsPanel.Controls.Add(_btnExportInvoice);
            actionsPanel.Controls.Add(_btnGenerateInvoices);
            actionsPanel.Controls.Add(_btnExportInvoices);
            actionsPanel.Controls.Add(_btnRefreshInvoices);

            var searchPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 0, 0, 5) };
            var lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new Point(0, 12) };
            _txtSearchInvoice.Location = new Point(lblSearch.Right + 8, 11);

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new Point(_txtSearchInvoice.Right + 25, 12) };
            _cboInvoiceStatus.Location = new Point(lblStatus.Right + 8, 8);

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
            gridPanel.Controls.Add(_invoiceCards);

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
            _cboPaymentMethod.Items.AddRange(new object[] { "Tất cả", "Tiền mặt", "Chuyển khoản", "Séc", "Thẻ" });
            _cboPaymentMethod.SelectedIndex = 0;
            _cboPaymentMethod.SelectedIndexChanged += (s, e) => ApplyPaymentFilter();
            _gridPayments.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                var col = _gridPayments.Columns[e.ColumnIndex];
                if (col.Name == "PaymentMethod" && e.Value != null)
                {
                    e.Value = ToVietnamesePaymentMethod(e.Value.ToString());
                    e.FormattingApplied = true;
                }
            };

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
            _btnDeletePayment.Visible = !_isStaffMode;
            _btnDeletePayment.Enabled = !_isStaffMode;

            // Top Panel
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 135, Padding = new Padding(15, 12, 15, 0), BackColor = Color.White };
            topPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(225, 230, 235), 1))
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
                Height = 50,
                Padding = new Padding(0, 5, 0, 10)
            };
            actionsPanel.Controls.Add(_btnAddPayment);
            actionsPanel.Controls.Add(_btnEditPayment);
            actionsPanel.Controls.Add(_btnDeletePayment);
            actionsPanel.Controls.Add(_btnRoomPayment);
            actionsPanel.Controls.Add(_btnBulkPayment);
            actionsPanel.Controls.Add(_btnExportPayments);
            actionsPanel.Controls.Add(_btnRefreshPayments);

            var filterPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 0, 0, 5) };

            var lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new Point(0, 12) };
            _txtSearchPayment.Location = new Point(lblSearch.Right + 8, 11);

            var lblMethod = new Label { Text = "Hình thức:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new Point(_txtSearchPayment.Right + 25, 12) };
            _cboPaymentMethod.Location = new Point(lblMethod.Right + 8, 8);

            var lblFrom = new Label { Text = "Từ:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new Point(_cboPaymentMethod.Right + 25, 12) };
            _dtFromDate.Location = new Point(lblFrom.Right + 8, 8);
            _dtFromDate.Width = 120;

            var lblTo = new Label { Text = "Đến:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new Point(_dtFromDate.Right + 25, 12) };
            _dtToDate.Location = new Point(lblTo.Right + 8, 8);
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
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 65, Padding = new Padding(15, 12, 15, 12), BackColor = Color.White };
            bottomPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(225, 230, 235), 1))
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
                if (_showPayments)
                    await LoadPaymentsAsync();
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi tải dữ liệu: " + ex.Message);
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
                ApplyInvoiceFilter();
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi tải hóa đơn: " + ex.Message);
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
                ModernDialog.Error("Lỗi tải thanh toán: " + ex.Message);
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

        private void FormatPaymentGrid()
        {
            var columnMapping = new Dictionary<string, string>
            {
                { "PaymentId", "Mã TT" },
                { "InvoiceNumber", "Hóa đơn" },
                { "TenantName", "Khách" },
                { "RoomNumber", "Phòng" },
                { "PaymentDate", "Ngày TT" },
                { "PaymentAmount", "Số tiền" },
                { "PaymentMethod", "Hình thức" },
                { "TransactionReference", "Tham chiếu" },
                { "Notes", "Ghi chú" }
            };

            foreach (DataGridViewColumn col in _gridPayments.Columns)
            {
                if (columnMapping.ContainsKey(col.Name))
                    col.HeaderText = columnMapping[col.Name];

                if (col.Name == "PaymentAmount")
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
            var status = MapStatusFilter(_cboInvoiceStatus.SelectedItem?.ToString());

            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kwFilters = new List<string>();
                if (_invoiceTable.Columns.Contains("InvoiceNumber")) kwFilters.Add($"InvoiceNumber LIKE '%{keyword}%'");
                if (_invoiceTable.Columns.Contains("TenantName")) kwFilters.Add($"TenantName LIKE '%{keyword}%'");
                if (_invoiceTable.Columns.Contains("RoomNumber")) kwFilters.Add($"RoomNumber LIKE '%{keyword}%'");
                if (kwFilters.Count > 0) filters.Add("(" + string.Join(" OR ", kwFilters) + ")");
            }

            if (!string.IsNullOrWhiteSpace(status) && _invoiceTable.Columns.Contains("Status"))
                filters.Add($"Status = '{status.Replace("'", "''")}'");

            _invoiceTable.DefaultView.RowFilter = filters.Count > 0 ? string.Join(" AND ", filters) : string.Empty;
            _lblInvoiceCount.Text = $"Tổng: {_invoiceTable.DefaultView.Count}";
            UpdateInvoiceSummary();
            UpdateInvoiceActionState();
            RenderInvoiceCards();
        }

        private void RenderInvoiceCards()
        {
            if (_invoiceCards == null) return;
            _invoiceCards.SuspendLayout();
            _invoiceCards.Controls.Clear();
            _selectedInvoice = null;

            if (_invoiceTable == null || _invoiceTable.DefaultView.Count == 0)
            {
                _invoiceCards.ResumeLayout();
                return;
            }

            foreach (DataRowView view in _invoiceTable.DefaultView)
            {
                var row = view.Row;
                _invoiceCards.Controls.Add(CreateInvoiceCard(row));
            }

            if (_invoiceCards.Controls.Count > 0 && _invoiceCards.Controls[0] is Panel first && first.Tag is DataRow firstRow)
                SelectInvoiceCard(first, firstRow);

            _invoiceCards.ResumeLayout();
            UpdateInvoiceActionState();
        }

        private Control CreateInvoiceCard(DataRow row)
        {
            // 1. Get Data
            string invoiceNumber = ReadString(row, "InvoiceNumber") ?? ReadString(row, "InvoiceId") ?? "—";
            string tenant = ReadString(row, "TenantName") ?? ReadString(row, "TenantId") ?? "—";
            string room = ReadString(row, "RoomNumber") ?? ReadString(row, "RoomId") ?? "—";
            string date = TryReadDate(row, "InvoiceDate")?.ToString("dd/MM/yyyy") ?? "—";
            string statusKey = ReadString(row, "Status");
            string statusText = ToVietnameseStatus(statusKey);

            decimal total = ReadDecimal(row, "TotalAmount");
            decimal paid = ReadDecimal(row, "PaidAmount");
            decimal remaining = ReadDecimal(row, "RemainingAmount");

            Color statusColor = GetStatusColor(statusKey);

            // 2. Main Card Container
            var card = new Panel
            {
                Width = 380,
                Height = 200,
                BackColor = ModernTheme.Colors.Background,
                Margin = new Padding(8),
                Padding = new Padding(1), // Border width
                Cursor = Cursors.Hand,
                Tag = row
            };

            // 3. Selection Highlighting (Border with rounded corners)
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                bool isSelected = IsSelectedCard(card);
                Color borderColor = isSelected ? ModernTheme.Colors.Primary : ModernTheme.Colors.Border;
                int borderWidth = isSelected ? 2 : 1;
                
                Rectangle rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                
                // Draw background with rounded corners
                using (var path = UiKit.GetRoundPath(rect, 12))
                using (var bgBrush = new SolidBrush(ModernTheme.Colors.Background))
                {
                    e.Graphics.FillPath(bgBrush, path);
                }
                
                // Draw border with rounded corners
                using (var path = UiKit.GetRoundPath(rect, 12))
                using (var pen = new Pen(borderColor, borderWidth))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // Update region for rounded corners
            EventHandler updateRegion = (s, e) => UiKit.SetRoundedRegion(card, 12);
            card.Resize += updateRegion;
            updateRegion(null, null);

            // 4. Inner Content Panel
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.Colors.Background,
                Padding = new Padding(12)
            };

            // --- Header Section: Invoice # and Status Badge ---
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 32 };
            
            var lblTitle = new Label
            {
                Text = $"Hóa đơn: {invoiceNumber}",
                Font = ModernTheme.Fonts.Bold(11),
                ForeColor = ModernTheme.Colors.Primary,
                AutoSize = true,
                Location = new Point(0, 4)
            };

            var lblStatusBadge = UIHelper.CreateBadge(statusText, statusColor);
            lblStatusBadge.Left = content.Width - lblStatusBadge.Width - 25; // Align right (approx)
            lblStatusBadge.Top = 2;
            // Fix badge auto-positioning logic purely for this absolute layout if needed, 
            // but FlowLayout/Dock is safer. Let's use Dock Right container for badge.
            var pnlBadgeContainer = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Right, 
                AutoSize = true, 
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 2, 0, 0)
            };
            pnlBadgeContainer.Controls.Add(lblStatusBadge);

            pnlHeader.Controls.Add(pnlBadgeContainer);
            pnlHeader.Controls.Add(lblTitle);

            // --- Body Section: Tenant and Room ---
            var pnlBody = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(0, 8, 0, 0) };
            
            var lblTenant = new Label
            {
                Text = $"👤 {tenant}",
                Font = ModernTheme.Fonts.Regular(10),
                ForeColor = ModernTheme.Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 8)
            };
            
            var lblRoom = new Label
            {
                Text = $"🏠 {room}   📅 {date}",
                Font = ModernTheme.Fonts.Regular(9.5f),
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 32)
            };

            pnlBody.Controls.Add(lblRoom);
            pnlBody.Controls.Add(lblTenant);

            // --- Divider ---
            var divider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ModernTheme.Colors.Divider, Margin = new Padding(0, 4, 0, 4) };

            // --- Footer Section: Financials ---
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 50 };

            var lblTotalLabel = new Label
            {
                Text = "Tổng cộng",
                Font = ModernTheme.Fonts.Regular(8.5f),
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 6)
            };
            var lblTotalValue = new Label
            {
                Text = $"{total:N0}đ",
                Font = ModernTheme.Fonts.Bold(12),
                ForeColor = ModernTheme.Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 22)
            };

            var pnlDebt = new Panel { Dock = DockStyle.Right, Width = 150 };
            var lblDebtLabel = new Label
            {
                Text = remaining > 0 ? "Còn nợ" : "Đã thanh toán",
                Font = ModernTheme.Fonts.Regular(8.5f),
                ForeColor = remaining > 0 ? ModernTheme.Colors.Error : ModernTheme.Colors.Success,
                AutoSize = false,
                TextAlign = ContentAlignment.TopRight,
                Dock = DockStyle.Top,
                Height = 18
            };
            var lblDebtValue = new Label
            {
                Text = remaining > 0 ? $"{remaining:N0}đ" : "✓",
                Font = ModernTheme.Fonts.Bold(12),
                ForeColor = remaining > 0 ? ModernTheme.Colors.Error : ModernTheme.Colors.Success,
                AutoSize = false,
                TextAlign = ContentAlignment.TopRight,
                Dock = DockStyle.Fill
            };
            pnlDebt.Controls.Add(lblDebtValue);
            pnlDebt.Controls.Add(lblDebtLabel);

            pnlFooter.Controls.Add(lblTotalValue);
            pnlFooter.Controls.Add(lblTotalLabel);
            pnlFooter.Controls.Add(pnlDebt);


            // Assemble
            content.Controls.Add(pnlFooter);
            content.Controls.Add(divider);
            content.Controls.Add(pnlBody);
            content.Controls.Add(pnlHeader);
            
            card.Controls.Add(content);

            // --- Event Wiring ---
            void SelectAction() => SelectInvoiceCard(card, row);
            
            card.Click += (s, e) => SelectAction();
            // Rekursively add click handlers to all children
            void AddClickRecursively(Control c)
            {
                c.Click += (s, e) => SelectAction();
                if (c.HasChildren) foreach (Control child in c.Controls) AddClickRecursively(child);
            }
            AddClickRecursively(content);

            // Double click to open details
            void DoubleClickAction()
            {
                SelectAction();
                ShowInvoiceDetail();
            }
            card.DoubleClick += (s, e) => DoubleClickAction();
            void AddDoubleClickRecursively(Control c)
            {
                c.DoubleClick += (s, e) => DoubleClickAction();
                if (c.HasChildren) foreach (Control child in c.Controls) AddDoubleClickRecursively(child);
            }
            AddDoubleClickRecursively(content);

            return card;
        }

        private void SelectInvoiceCard(Panel card, DataRow row)
        {
            if (_selectedInvoice.HasValue && _selectedInvoice.Value.Card != null)
                _selectedInvoice.Value.Card.Invalidate();

            _selectedInvoice = (card, row);
            card.Invalidate();
            UpdateInvoiceActionState();
        }

        private bool IsSelectedCard(Panel card)
        {
            return _selectedInvoice.HasValue && ReferenceEquals(_selectedInvoice.Value.Card, card);
        }

        private static string MapStatusFilter(string selected)
        {
            if (string.IsNullOrWhiteSpace(selected) || selected == "Tất cả") return null;
            switch (selected)
            {
                case "Chưa thanh toán":
                    return "Issued";
                case "Thanh toán một phần":
                    return "PartialPaid";
                case "Đã thanh toán":
                    return "Paid";
                case "Quá hạn":
                    return "Overdue";
                default:
                    return selected;
            }
        }



        private static string MapPaymentMethod(string selected)
        {
            if (string.IsNullOrWhiteSpace(selected) || selected == "Tất cả") return null;
            switch (selected)
            {
                case "Tiền mặt": return "Cash";
                case "Chuyển khoản": return "Transfer";
                case "Séc": return "Check";
                case "Thẻ": return "Card";
                default: return selected;
            }
        }

        private static string ToVietnamesePaymentMethod(string method)
        {
            switch (method)
            {
                case "Cash": return "Tiền mặt";
                case "Transfer": return "Chuyển khoản";
                case "Check": return "Séc";
                case "Card": return "Thẻ";
                default: return method;
            }
        }

        private static string ToVietnameseStatus(string status)
        {
            return TextFixer.ToVietnameseInvoiceStatus(status);
        }

        private static Color GetStatusColor(string status)
        {
            switch (status)
            {
                case "Paid":
                    return Color.SeaGreen;
                case "PartialPaid":
                    return Color.DarkOrange;
                case "Overdue":
                    return Color.Firebrick;
                default:
                    return Color.Gray;
            }
        }

        private static string ReadString(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            return v == null || v == DBNull.Value ? null : v.ToString();
        }

        private static DateTime? TryReadDate(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            if (DateTime.TryParse(row[col]?.ToString(), out var dt)) return dt.Date;
            return null;
        }

        private void ApplyPaymentFilter()
        {
            if (_paymentTable == null) return;
            var keyword = _txtSearchPayment.Text == SearchPlaceholder ? string.Empty : _txtSearchPayment.Text.Trim();
            keyword = keyword.Replace("'", "''");
            var method = MapPaymentMethod(_cboPaymentMethod.SelectedItem?.ToString());

            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kwFilters = new List<string>();
                if (_paymentTable.Columns.Contains("InvoiceNumber")) kwFilters.Add($"InvoiceNumber LIKE '%{keyword}%'");
                if (_paymentTable.Columns.Contains("TenantName")) kwFilters.Add($"TenantName LIKE '%{keyword}%'");
                if (_paymentTable.Columns.Contains("RoomNumber")) kwFilters.Add($"RoomNumber LIKE '%{keyword}%'");
                if (kwFilters.Count > 0) filters.Add("(" + string.Join(" OR ", kwFilters) + ")");
            }

            if (!string.IsNullOrWhiteSpace(method) && method != "Tất cả" && _paymentTable.Columns.Contains("PaymentMethod"))
                filters.Add($"PaymentMethod = '{method.Replace("'", "''")}'");

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

        private void UpdateInvoiceActionState()
        {
            var row = GetCurrentInvoiceRow();
            if (row == null)
            {
                if (_btnPayInvoice != null) _btnPayInvoice.Enabled = false;
                if (_btnExportInvoice != null) _btnExportInvoice.Enabled = false;
                return;
            }

            decimal remaining = ReadDecimal(row, "RemainingAmount");
            if (_btnPayInvoice != null) _btnPayInvoice.Enabled = remaining > 0;
            if (_btnExportInvoice != null) _btnExportInvoice.Enabled = remaining <= 0;
        }

        private void UpdatePaymentSummary()
        {
            if (_paymentTable == null) return;
            decimal total = 0m;
            foreach (DataRowView view in _paymentTable.DefaultView)
            {
                total += ReadDecimal(view.Row, "PaymentAmount");
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
            return _selectedInvoice?.Row;
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
                ToastNotification.Warning("Chọn một hóa đơn để sửa");
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
            if (_isStaffMode) return;
            var row = GetCurrentInvoiceRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một hóa đơn để xóa");
                return;
            }

            int invoiceId = Convert.ToInt32(row["InvoiceId"]);
            decimal paid = ReadDecimal(row, "PaidAmount");
            bool hasPayment = paid > 0;

            string msg = hasPayment
                ? $"Hóa đơn ID {invoiceId} đã có thanh toán. Xóa cả lịch sử thanh toán?"
                : $"Xóa hóa đơn ID {invoiceId}?";

            if (!ModernConfirmDialog.ConfirmDanger(msg)) return;

            try
            {
                await _bll.DeleteInvoiceAsync(invoiceId, deletePaymentsFirst: hasPayment);
                await LoadInvoicesAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi xóa hóa đơn: " + ex.Message);
            }
        }

        private async Task PaySelectedInvoiceAsync()
        {
            var row = GetCurrentInvoiceRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một hóa đơn để thu tiền");
                return;
            }

            decimal remaining = ReadDecimal(row, "RemainingAmount");
            if (remaining <= 0)
            {
                ToastNotification.Info("Hóa đơn đã thanh toán đủ");
                return;
            }

            int invoiceId = Convert.ToInt32(row["InvoiceId"]);
            // Show tenant info and payment form
            using (var frm = new FrmPaymentWithTenantInfo(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadAllDataAsync();
                    SelectInvoiceRow(invoiceId);
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private void ExportSelectedInvoice()
        {
            var row = GetCurrentInvoiceRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một hóa đơn để xuất");
                return;
            }

            decimal remaining = ReadDecimal(row, "RemainingAmount");
            if (remaining > 0)
            {
                ToastNotification.Warning("Hóa đơn chỉ có thể xuất sau khi thanh toán đủ");
                return;
            }

            using (var frm = new FrmInvoiceExportForm(_bll, row))
            {
                frm.ShowDialog(this);
            }
        }

        private void ShowInvoiceDetail()
        {
            var row = GetCurrentInvoiceRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một hóa đơn để xem chi tiết");
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
                    int created = await _bll.GenerateMonthlyInvoicesAsync(dlg.SelectedYear, dlg.SelectedMonth, DateTime.Today, dlg.DueDay, dlg.TaxRateOverride);
                    await LoadInvoicesAsync();
                    AdminEvents.NotifyDataChanged();
                    ToastNotification.Success($"Đã tạo {created} hóa đơn cho {dlg.SelectedMonth:00}/{dlg.SelectedYear}");
                }
                catch (Exception ex)
                {
                    ModernDialog.Error("Lỗi tạo hóa đơn tháng: " + ex.Message);
                }
            }
        }

        private void ExportInvoicesCsv()
        {
            if (_invoiceTable == null || _invoiceTable.DefaultView.Count == 0)
            {
                ToastNotification.Info("Không có dữ liệu để xuất");
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
            _ = AddNewPaymentAsync();
        }

        private void SelectInvoiceRow(int invoiceId)
        {
            if (_invoiceCards == null || _invoiceCards.Controls.Count == 0) return;
            foreach (Control control in _invoiceCards.Controls)
            {
                if (control is Panel panel && panel.Tag is DataRow row &&
                    Convert.ToInt32(row["InvoiceId"]) == invoiceId)
                {
                    SelectInvoiceCard(panel, row);
                    return;
                }
            }
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

            _ = EditSelectedPaymentAsync(row);
        }

        private async System.Threading.Tasks.Task DeleteSelectedPaymentAsync()
        {
            if (_isStaffMode) return;
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

        private async Task AddNewPaymentAsync()
        {
            try
            {
                var invoiceRow = await PickInvoiceForPaymentAsync();
                if (invoiceRow == null)
                {
                    MessageBox.Show("Không chọn được hóa đơn để thu tiền.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var frm = new FrmPaymentEditor(_bll, invoiceRow))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        await LoadAllDataAsync();
                        AdminEvents.NotifyDataChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thu tiền: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadAllDataAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private async Task EditSelectedPaymentAsync(DataRow row)
        {
            try
            {
                if (!row.Table.Columns.Contains("PaymentId"))
                {
                    MessageBox.Show("Không xác định được PaymentId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int paymentId = Convert.ToInt32(row["PaymentId"]);
                var rawRow = FindPaymentById(paymentId);
                if (rawRow == null || !rawRow.Table.Columns.Contains("InvoiceId"))
                {
                    MessageBox.Show("Không tìm thấy dữ liệu gốc để sửa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int invoiceId = Convert.ToInt32(rawRow["InvoiceId"]);
                if (_invoiceTable == null || _invoiceTable.Rows.Count == 0)
                    await LoadInvoicesAsync();

                var invoiceRow = _invoiceTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["InvoiceId"]) == invoiceId);
                if (invoiceRow == null)
                {
                    MessageBox.Show("Không tìm thấy hóa đơn để hiển thị.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (var frm = new FrmPaymentEditor(_bll, invoiceRow, rawRow))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        await LoadAllDataAsync();
                        AdminEvents.NotifyDataChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sửa thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataRow FindPaymentById(int paymentId)
        {
            if (_paymentTable == null || !_paymentTable.Columns.Contains("PaymentId")) return null;
            return _paymentTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["PaymentId"]) == paymentId);
        }

        private async Task<DataRow> PickInvoiceForPaymentAsync()
        {
            if (_invoiceTable == null || _invoiceTable.Rows.Count == 0)
                await LoadInvoicesAsync();
            if (_invoiceTable == null || _invoiceTable.Rows.Count == 0) return null;

            if (!_invoiceTable.Columns.Contains("InvoiceDisplay"))
                _invoiceTable.Columns.Add("InvoiceDisplay", typeof(string));

            foreach (DataRow r in _invoiceTable.Rows)
            {
                decimal remaining = ReadDecimal(r, "RemainingAmount");
                string invNo = _invoiceTable.Columns.Contains("InvoiceNumber") ? r["InvoiceNumber"]?.ToString() : r["InvoiceId"]?.ToString();
                string tenant = _invoiceTable.Columns.Contains("TenantName") ? r["TenantName"]?.ToString() : r["TenantId"]?.ToString();
                string room = _invoiceTable.Columns.Contains("RoomNumber") ? r["RoomNumber"]?.ToString() : r["RoomId"]?.ToString();
                r["InvoiceDisplay"] = $"{invNo} | {tenant} | Phòng {room} | Còn: {remaining:N0}";
            }

            var selectable = _invoiceTable.AsEnumerable().ToList();

            if (_branchId.HasValue && _invoiceTable.Columns.Contains("BranchId"))
                selectable = selectable.Where(r => int.TryParse(r["BranchId"]?.ToString(), out var b) && b == _branchId.Value).ToList();

            if (selectable.Count == 0) return null;

            using (var dlg = new PaymentInvoicePickerDialog(selectable.CopyToDataTable()))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return null;
                return dlg.SelectedRow;
            }
        }

        private class PaymentInvoicePickerDialog : Form
        {
            private readonly DataTable _table;
            private ComboBox _cbo;
            private Button _btnOk;
            private Button _btnCancel;
            public DataRow SelectedRow { get; private set; }

            public PaymentInvoicePickerDialog(DataTable table)
            {
                _table = table;
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                Text = "Chọn hóa đơn để thu tiền";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(640, 170);
                BackColor = Color.White;

                _cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 600, Location = new Point(20, 20) };
                _cbo.DataSource = _table;
                _cbo.DisplayMember = _table.Columns.Contains("InvoiceDisplay") ? "InvoiceDisplay" : _table.Columns[0].ColumnName;
                _cbo.ValueMember = _table.Columns.Contains("InvoiceId") ? "InvoiceId" : _table.Columns[0].ColumnName;

                _btnOk = new Button { Text = "Chọn", Width = 110, Height = 34, Location = new Point(390, 100) };
                _btnCancel = new Button { Text = "Hủy", Width = 110, Height = 34, Location = new Point(510, 100) };

                _btnOk.FlatStyle = FlatStyle.Flat;
                _btnOk.FlatAppearance.BorderSize = 0;
                _btnOk.BackColor = Color.FromArgb(0, 122, 204);
                _btnOk.ForeColor = Color.White;
                _btnCancel.FlatStyle = FlatStyle.Flat;
                _btnCancel.FlatAppearance.BorderSize = 1;

                _btnOk.Click += (s, e) =>
                {
                    if (_cbo.SelectedItem is DataRowView drv)
                        SelectedRow = drv.Row;
                    DialogResult = SelectedRow != null ? DialogResult.OK : DialogResult.Cancel;
                };
                _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

                Controls.Add(_cbo);
                Controls.Add(_btnOk);
                Controls.Add(_btnCancel);

                AcceptButton = _btnOk;
                CancelButton = _btnCancel;
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
            private NumericUpDown _numTaxRate;
            private Button _btnOk;
            private Button _btnCancel;

            public int SelectedYear => (int)_numYear.Value;
            public int SelectedMonth => (int)_numMonth.Value;
            public int? DueDay => _numDueDay.Value > 0 ? (int?)_numDueDay.Value : null;
            public decimal? TaxRateOverride => _numTaxRate.Value > 0 ? (decimal?)_numTaxRate.Value : null;

            public MonthlyInvoiceDialog()
            {
                Text = "Tạo hóa đơn tháng";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(360, 220);

                var now = DateTime.Today;
                _numYear = new NumericUpDown { Minimum = 2000, Maximum = 2100, Value = now.Year, Location = new Point(130, 20), Width = 180 };
                _numMonth = new NumericUpDown { Minimum = 1, Maximum = 12, Value = now.Month, Location = new Point(130, 55), Width = 180 };
                _numDueDay = new NumericUpDown { Minimum = 0, Maximum = 31, Value = 10, Location = new Point(130, 90), Width = 180 };
                _numTaxRate = new NumericUpDown { Minimum = 0, Maximum = 100, DecimalPlaces = 2, Increment = 0.1m, Value = 0, Location = new Point(130, 125), Width = 180 };

                Controls.Add(new Label { Text = "Năm", AutoSize = true, Location = new Point(20, 24) });
                Controls.Add(_numYear);
                Controls.Add(new Label { Text = "Tháng", AutoSize = true, Location = new Point(20, 59) });
                Controls.Add(_numMonth);
                Controls.Add(new Label { Text = "Hạn (ngày, 0=auto)", AutoSize = true, Location = new Point(20, 94) });
                Controls.Add(_numDueDay);
                Controls.Add(new Label { Text = "Thuế (%), 0=auto", AutoSize = true, Location = new Point(20, 129) });
                Controls.Add(_numTaxRate);

                _btnOk = new Button { Text = "Tạo", Width = 100, Location = new Point(130, 165) };
                _btnCancel = new Button { Text = "Hủy", Width = 100, Location = new Point(240, 165) };
                _btnOk.Click += (s, e) => DialogResult = DialogResult.OK;
                _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
                Controls.Add(_btnOk);
                Controls.Add(_btnCancel);
            }
        }
    }
}
