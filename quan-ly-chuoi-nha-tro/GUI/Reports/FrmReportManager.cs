using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmReportManager : Form
    {
        private const string SearchPlaceholder = "Tìm nhanh (mọi cột)...";

        private readonly AdminDataBLL _bll;
        private readonly int? _presetBranchId;
        private readonly bool _isStaffMode;
        private System.Collections.Generic.HashSet<int> _allowedBranchIds;

        private ComboBox _cboSource;
        private TextBox _txtSearch;
        private DataGridView _grid;
        private FlowLayoutPanel _cards;
        private Label _lblCount;
        private Label _lblFooterSummary;
        private Button _btnExport;
        private Button _btnRefresh;

        private Panel _pnlTaxFilters;
        private ComboBox _cboTaxPeriodType;
        private NumericUpDown _numTaxYear;
        private NumericUpDown _numTaxPeriod;
        private NumericUpDown _numTaxRate;
        private Button _btnTaxRecalc;
        private Button _btnTaxSaveRate;
        private Button _btnTaxOpenInvoices;

        private DataTable _raw;
        private DataTable _viewTable;

        public FrmReportManager(AdminDataBLL bll, int? branchId = null, bool isStaffMode = false)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
            _presetBranchId = branchId;
            _isStaffMode = isStaffMode;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        public FrmReportManager(int? branchId, bool isStaffMode)
            : this(new AdminDataBLL(), branchId, isStaffMode)
        {
        }

        private void InitializeComponent()
        {
            Text = "Báo Cáo & Thống Kê";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1400;
            Height = 850;
            BackColor = Color.FromArgb(245, 247, 250);
            this.DoubleBuffered = true;

            // ===== HEADER PANEL WITH TITLE =====
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(0, 120, 215),
                Padding = new Padding(20, 8, 20, 8)
            };
            var lblTitle = new Label
            {
                Text = "📊",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Left
            };
            pnlHeader.Controls.Add(lblTitle);

            _cboSource = new ComboBox 
            { 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Width = 200,
                Height = 32,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            _cboSource.Items.AddRange(GetReportSources());
            _cboSource.SelectedIndex = 0;
            _cboSource.SelectedIndexChanged += async (s, e) => await LoadDataAsync();

            _txtSearch = new TextBox 
            { 
                Width = 300,
                Height = 32,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var pnlSearch = UiKit.MakeSearchPanel(_txtSearch, 340, SearchPlaceholder, ApplyFilter);

            _btnExport = UiKit.MakeButton("📥 Xuất CSV", UiKit.Purple, (s, e) => ExportCsv(), 120);
            _btnRefresh = UiKit.MakeButton("🔄 Tải Lại", UiKit.Primary, async (s, e) => await LoadDataAsync(), 120);

            _lblCount = new Label 
            { 
                AutoSize = true, 
                Text = "Tổng: 0", 
                Font = new Font("Segoe UI", 11, FontStyle.Bold), 
                ForeColor = Color.FromArgb(0, 120, 215),
                Margin = new Padding(20, 0, 0, 0)
            };

            // ===== CARD HOST =====
            _cards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = BackColor,
                Padding = new Padding(8)
            };

            // ===== TOOLBAR PANEL =====
            var pnlToolbar = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 50, 
                Padding = new Padding(15, 8, 15, 8), 
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var pnlActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            pnlActions.Controls.Add(_btnExport);
            pnlActions.Controls.Add(_btnRefresh);
            pnlActions.Controls.Add(_lblCount);

            var pnlFilters = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            
            var lblSource = new Label 
            { 
                Text = "📋 Báo Cáo:", 
                AutoSize = true, 
                ForeColor = Color.FromArgb(70, 70, 70), 
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 6, 8, 0)
            };
            var lblSearch = new Label 
            { 
                Text = "🔍 Tìm:", 
                AutoSize = true, 
                ForeColor = Color.FromArgb(70, 70, 70), 
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(15, 6, 8, 0)
            };
            
            pnlFilters.Controls.Add(lblSource);
            pnlFilters.Controls.Add(_cboSource);
            pnlFilters.Controls.Add(lblSearch);
            pnlFilters.Controls.Add(pnlSearch);

            pnlToolbar.Controls.Add(pnlFilters);
            pnlToolbar.Controls.Add(pnlActions);

            // ===== TAX FILTER PANEL (only for revenue tax report) =====
            _pnlTaxFilters = BuildTaxFilterPanel();
            _pnlTaxFilters.Dock = DockStyle.Top;
            _pnlTaxFilters.Visible = !_isStaffMode && false;

            // ===== GRID HOST =====
            var gridHost = new Panel 
            { 
                Dock = DockStyle.Fill, 
                Padding = new Padding(12), 
                BackColor = BackColor 
            };
            gridHost.Controls.Add(_cards);

            // ===== FOOTER PANEL =====
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15, 10, 15, 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            _lblFooterSummary = new Label
            {
                Text = "",
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(50, 50, 50),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlFooter.Controls.Add(_lblFooterSummary);

            Controls.Add(gridHost);
            Controls.Add(pnlFooter);
            Controls.Add(_pnlTaxFilters);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);

            Load += async (s, e) => await LoadDataAsync();
        }

        private object[] GetReportSources()
        {
            if (_isStaffMode)
            {
                return new object[]
                {
                    "Hóa Đơn",
                    "Thanh Toán",
                    "Khách Thuê",
                    "Phòng",
                    "Hợp Đồng",
                    "Đặt Cọc",
                    "Bảo Trì",
                    "Tài Sản",
                    "Thông Báo"
                };
            }

            return new object[]
            {
                "Hóa Đơn",
                "Thanh Toán",
                "Thuế Doanh Thu",
                "Khách Thuê",
                "Phòng",
                "Hợp Đồng",
                "Đặt Cọc",
                "Bảo Trì",
                "Tài Sản",
                "Thông Báo",
                "Cấu Hình Hệ Thống"
            };
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _raw = await LoadSelectedAsync();
                await EnsureAllowedBranchScopeAsync();
                if (_raw != null && _raw.Columns.Contains("BranchId"))
                    _raw = AdminBranchScope.FilterByBranchIds(_raw, _allowedBranchIds);
                ApplyTaxReportDefaults();
                _viewTable = _raw;
                RenderCards(_viewTable);
                _lblCount.Text = $"Tổng: {_viewTable?.Rows.Count ?? 0}";
                UpdateFooterSummary();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadDataAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private void UpdateFooterSummary()
        {
            if (_raw == null || _raw.Rows.Count == 0)
            {
                _lblFooterSummary.Text = "";
                return;
            }

            var summaryParts = new System.Collections.Generic.List<string>();
            var selected = _cboSource.SelectedItem?.ToString() ?? "Hóa Đơn";

            // Generate summary based on report type
            switch (selected)
            {
                case "Thuế Doanh Thu":
                    if (_raw.Columns.Contains("Revenue"))
                    {
                        decimal totalRevenue = 0;
                        foreach (DataRow row in _raw.Rows)
                        {
                            if (decimal.TryParse(row["Revenue"]?.ToString(), out decimal amt))
                                totalRevenue += amt;
                        }
                        summaryParts.Add($"💰 Doanh Thu: {totalRevenue:N0}đ");
                    }
                    if (_raw.Columns.Contains("TaxAmount"))
                    {
                        decimal totalTax = 0;
                        foreach (DataRow row in _raw.Rows)
                        {
                            if (decimal.TryParse(row["TaxAmount"]?.ToString(), out decimal amt))
                                totalTax += amt;
                        }
                        summaryParts.Add($"🧾 Thuế: {totalTax:N0}đ");
                    }
                    break;
                case "Hóa Đơn":
                    if (_raw.Columns.Contains("TotalAmount"))
                    {
                        decimal total = 0;
                        foreach (DataRow row in _raw.Rows)
                        {
                            if (decimal.TryParse(row["TotalAmount"]?.ToString(), out decimal amt))
                                total += amt;
                        }
                        summaryParts.Add($"💰 Tổng Tiền: {total:N0}đ");
                    }
                    if (_raw.Columns.Contains("TaxAmount"))
                    {
                        decimal taxTotal = 0;
                        foreach (DataRow row in _raw.Rows)
                        {
                            if (decimal.TryParse(row["TaxAmount"]?.ToString(), out decimal amt))
                                taxTotal += amt;
                        }
                        summaryParts.Add($"🧾 Thuế: {taxTotal:N0}đ");
                    }
                    if (_raw.Columns.Contains("PaidAmount"))
                    {
                        decimal paid = 0;
                        foreach (DataRow row in _raw.Rows)
                        {
                            if (decimal.TryParse(row["PaidAmount"]?.ToString(), out decimal amt))
                                paid += amt;
                        }
                        summaryParts.Add($"✓ Đã Thanh Toán: {paid:N0}đ");
                    }
                    if (_raw.Columns.Contains("RemainingAmount"))
                    {
                        decimal remaining = 0;
                        foreach (DataRow row in _raw.Rows)
                        {
                            if (decimal.TryParse(row["RemainingAmount"]?.ToString(), out decimal amt))
                                remaining += amt;
                        }
                        summaryParts.Add($"📌 Còn Nợ: {remaining:N0}đ");
                    }
                    break;

                case "Thanh Toán":
                    if (_raw.Columns.Contains("PaymentAmount"))
                    {
                        decimal total = 0;
                        foreach (DataRow row in _raw.Rows)
                        {
                            if (decimal.TryParse(row["PaymentAmount"]?.ToString(), out decimal amt))
                                total += amt;
                        }
                        summaryParts.Add($"💰 Tổng Thanh Toán: {total:N0}đ");
                    }
                    break;

                case "Đặt Cọc":
                    if (_raw.Columns.Contains("DepositAmount"))
                    {
                        decimal total = 0;
                        foreach (DataRow row in _raw.Rows)
                        {
                            if (decimal.TryParse(row["DepositAmount"]?.ToString(), out decimal amt))
                                total += amt;
                        }
                        summaryParts.Add($"💰 Tổng Cọc: {total:N0}đ");
                    }
                    break;

                case "Phòng":
                    int occupied = 0;
                    if (_raw.Columns.Contains("Occupied"))
                    {
                        foreach (DataRow row in _raw.Rows)
                        {
                            if (row["Occupied"] != null && row["Occupied"] != DBNull.Value)
                            {
                                if (bool.TryParse(row["Occupied"].ToString(), out bool isOccupied) && isOccupied)
                                    occupied++;
                            }
                        }
                        summaryParts.Add($"👥 Phòng Đã Cho Thuê: {occupied}");
                    }
                    summaryParts.Add($"🏠 Tổng Phòng: {_raw.Rows.Count}");
                    break;

                default:
                    summaryParts.Add($"📋 Tổng Bản Ghi: {_raw.Rows.Count}");
                    break;
            }

            _lblFooterSummary.Text = string.Join(" | ", summaryParts);
        }

        private void TranslateGridHeaders(DataGridView grid)
        {
            if (grid == null || grid.Columns.Count == 0) return;

            // Comprehensive translation dictionary for all reports
            var columnMap = GetColumnMap();

            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (columnMap.TryGetValue(col.Name, out var translatedName))
                {
                    col.HeaderText = translatedName;
                }
                else if (columnMap.TryGetValue(col.HeaderText, out var translatedName2))
                {
                    col.HeaderText = translatedName2;
                }

                if (string.Equals(col.Name, "TaxRate", StringComparison.OrdinalIgnoreCase))
                {
                    col.DefaultCellStyle.Format = "N2";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (string.Equals(col.Name, "TaxAmount", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(col.Name, "Revenue", StringComparison.OrdinalIgnoreCase))
                {
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            ApplyTaxGridPresentation(grid);

            // Auto-size columns with optimal width
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (col.Width < 60)
                    col.Width = 60;
                if (col.Width > 250)
                    col.Width = 250;
            }
        }

        private static System.Collections.Generic.Dictionary<string, string> GetColumnMap()
        {
            return new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Invoice columns
                { "InvoiceId", "#" },
                { "InvoiceNumber", "SốHĐ" },
                { "TenantId", "KH#" },
                { "TenantName", "Tên KH" },
                { "RoomId", "P#" },
                { "RoomNumber", "Phòng" },
                { "InvoiceDate", "Ngày" },
                { "FromDate", "Từ" },
                { "ToDate", "Đến" },
                { "RentalCost", "Thuê" },
                { "UtilityCost", "Tiện Ích" },
                { "OtherCost", "Khác" },
                { "TaxRate", "Thuế %" },
                { "TaxAmount", "Thuế" },
                { "TotalAmount", "Tổng" },
                { "PaidAmount", "Đã TT" },
                { "RemainingAmount", "Còn Nợ" },
                { "Status", "TT" },
                { "DueDate", "Hạn" },
                { "CreatedDate", "Tạo" },
                { "UpdatedDate", "Sửa" },

                // Payment columns
                { "PaymentId", "#" },
                { "PaymentDate", "Ngày TT" },
                { "PaymentAmount", "Tiền" },
                { "PaymentMethod", "PT" },
                { "TransactionReference", "Ref" },
                { "Notes", "GC" },

                // Tenant columns
                { "FullName", "Tên KH" },
                { "IdentityCard", "CMND" },
                { "PhoneNumber", "ĐT" },
                { "Email", "Email" },
                { "BirthDate", "Sinh" },
                { "Address", "ĐC" },
                { "TempReg", "Tạm Trú" },
                { "TempRegDate", "Từ TT" },
                { "TempRegExpiry", "Hết TT" },
                { "IsActive", "Hoạt" },
                { "CreatedBy", "Người TL" },

                // Room columns
                { "RoomTypeId", "LP#" },
                { "BranchId", "Chi Nhánh" },
                { "SectionId", "KV#" },
                { "RoomStatusId", "TT#" },
                { "RoomPrice", "Giá" },
                { "Capacity", "Sức" },
                { "Occupied", "Sử Dụng" },

                // Contract columns
                { "ContractId", "HĐ#" },
                { "ContractNumber", "Số HĐ" },
                { "ContractType", "Loại" },
                { "SignDate", "Ký" },
                { "StartDate", "Từ" },
                { "EndDate", "Đến" },
                { "RentalPrice", "Giá" },
                { "DepositRequired", "Cọc" },
                { "Terms", "ĐK" },
                { "ContractPdfPath", "PDF" },

                // Deposit columns
                { "DepositId", "Cọc#" },
                { "DepositAmount", "Tiền" },
                { "DepositDate", "Ngày" },
                { "DepositType", "Loại" },
                { "ReturnedAmount", "Hoàn" },
                { "ReturnedDate", "Ngày Hoàn" },

                // Maintenance columns
                { "MaintenanceId", "BT#" },
                { "RequestDate", "Yêu Cầu" },
                { "CompletionDate", "Hoàn" },
                { "Cost", "CP" },
                { "Category", "DM" },
                { "Description", "Mô Tả" },

                // Asset columns
                { "AssetId", "TS#" },
                { "AssetName", "Tên TS" },
                { "AssetValue", "Giá" },
                { "Condition", "TT" },
                { "PurchaseDate", "Mua" },

                // Notification columns
                { "NotificationId", "TB#" },
                { "NotificationTitle", "Tiêu Đề" },
                { "NotificationContent", "Nội Dung" },
                { "NotificationDate", "Gửi" },
                { "RecipientType", "Loại" },

                // System Settings columns
                { "SettingKey", "Khóa" },
                { "SettingValue", "Giá Trị" },
                { "SettingDescription", "Mô Tả" },

                // Revenue tax report columns
                { "PeriodLabel", "Kỳ" },
                { "Revenue", "Doanh Thu" },
                { "Year", "Năm" },
                { "Period", "Tháng/Quý" }
            };
        }

        private void ApplyTaxGridPresentation(DataGridView grid)
        {
            if (grid == null) return;
            if ((_cboSource.SelectedItem?.ToString() ?? string.Empty) != "Thuế Doanh Thu")
                return;

            if (grid.Columns.Contains("Year")) grid.Columns["Year"].Visible = false;
            if (grid.Columns.Contains("Period")) grid.Columns["Period"].Visible = false;

            int displayIndex = 0;
            if (grid.Columns.Contains("PeriodLabel")) grid.Columns["PeriodLabel"].DisplayIndex = displayIndex++;
            if (grid.Columns.Contains("Revenue")) grid.Columns["Revenue"].DisplayIndex = displayIndex++;
            if (grid.Columns.Contains("TaxRate")) grid.Columns["TaxRate"].DisplayIndex = displayIndex++;
            if (grid.Columns.Contains("TaxAmount")) grid.Columns["TaxAmount"].DisplayIndex = displayIndex++;

            if (grid.Columns.Contains("PeriodLabel")) grid.Columns["PeriodLabel"].Width = 90;
            if (grid.Columns.Contains("Revenue")) grid.Columns["Revenue"].Width = 140;
            if (grid.Columns.Contains("TaxRate")) grid.Columns["TaxRate"].Width = 90;
            if (grid.Columns.Contains("TaxAmount")) grid.Columns["TaxAmount"].Width = 120;
        }

        private async System.Threading.Tasks.Task EnsureAllowedBranchScopeAsync()
        {
            if (_presetBranchId.HasValue)
            {
                _allowedBranchIds = new System.Collections.Generic.HashSet<int> { _presetBranchId.Value };
                return;
            }
            if (_allowedBranchIds != null && _allowedBranchIds.Count > 0) return;
            try
            {
                var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                _allowedBranchIds = AdminBranchScope.GetAllowedBranchIds(branches);
            }
            catch
            {
                _allowedBranchIds = new System.Collections.Generic.HashSet<int>();
            }
        }

        private System.Threading.Tasks.Task<DataTable> LoadSelectedAsync()
        {
            var selected = _cboSource.SelectedItem?.ToString() ?? string.Empty;
            
            switch (selected)
            {
                case "Hóa Đơn":
                    return _bll.GetInvoicesViewAsync();
                case "Thanh Toán":
                    return _bll.GetPaymentsViewAsync();
                case "Thuế Doanh Thu":
                    if (_isStaffMode)
                        return _bll.GetInvoicesViewAsync();
                    return LoadRevenueTaxAsync();
                case "Khách Thuê":
                    return _bll.GetTenantsAsync();
                case "Phòng":
                    return _bll.GetRoomsAsync();
                case "Hợp Đồng":
                    return _bll.GetContractsAsync();
                case "Đặt Cọc":
                    return _bll.GetDepositsAsync();
                case "Bảo Trì":
                    return _bll.GetMaintenanceAsync();
                case "Tài Sản":
                    return _bll.GetAssetsAsync();
                case "Thông Báo":
                    return _bll.GetNotificationsAsync();
                case "Cấu Hình Hệ Thống":
                    if (_isStaffMode)
                        return _bll.GetInvoicesViewAsync();
                    return _bll.GetSystemSettingsAsync();
                default:
                    return _bll.GetInvoicesViewAsync();
            }
        }

        private void RenderCards(DataTable table)
        {
            if (_cards == null) return;
            _cards.SuspendLayout();
            _cards.Controls.Clear();

            if (table == null || table.Rows.Count == 0)
            {
                _cards.ResumeLayout();
                return;
            }

            var reportType = _cboSource.SelectedItem?.ToString() ?? string.Empty;
            var columnMap = GetColumnMap();
            var displayColumns = GetDisplayColumns(table, reportType);

            foreach (DataRow row in table.Rows)
            {
                _cards.Controls.Add(CreateCard(row, displayColumns, columnMap, reportType));
            }

            _cards.ResumeLayout();
        }

        private static System.Collections.Generic.List<DataColumn> GetDisplayColumns(DataTable table, string reportType)
        {
            if (table == null) return new System.Collections.Generic.List<DataColumn>();
            if (reportType == "Thuế Doanh Thu")
            {
                var names = new[] { "PeriodLabel", "Revenue", "TaxRate", "TaxAmount" };
                return table.Columns.Cast<DataColumn>()
                    .Where(c => names.Contains(c.ColumnName))
                    .ToList();
            }

            return table.Columns.Cast<DataColumn>().ToList();
        }

        private Control CreateCard(DataRow row, System.Collections.Generic.List<DataColumn> columns,
            System.Collections.Generic.Dictionary<string, string> columnMap, string reportType)
        {
            Color accentColor = GetCardAccentColor(row, reportType);

            var card = new Panel
            {
                Width = 360,
                MinimumSize = new Size(360, 60),
                AutoSize = true,
                MaximumSize = new Size(360, 0),
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(1) // Border space
            };

            // Update region when size changes
            card.SizeChanged += (s, e) => UiKit.SetRoundedRegion(card, 20);

            var container = new Panel
            {
                Dock = DockStyle.Top,
                Width = 360,
                AutoSize = true,
                BackColor = Color.White,
                Padding = new Padding(15, 12, 12, 12)
            };

            // Title
            var titleText = BuildCardTitle(row, columns, columnMap, reportType);
            var lblTitle = new Label
            {
                Text = titleText,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize = true,
                MaximumSize = new Size(240, 0)
            };

            // Badge
            Control statusBadge = null;
            if (row.Table.Columns.Contains("Status"))
            {
                var status = row["Status"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(status))
                    statusBadge = CreateStatusBadge(status);
            }

            // Header Layout (Flexible)
            var headerTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 10)
            };
            headerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            headerTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            
            headerTable.Controls.Add(lblTitle, 0, 0);
            
            if (statusBadge != null)
            {
                statusBadge.Margin = new Padding(5, 0, 0, 0);
                headerTable.Controls.Add(statusBadge, 1, 0);
            }
            
            container.Controls.Add(new Panel { Height = 1, Dock = DockStyle.Top }); // Shim for table space
            container.Controls.Add(headerTable);
            
            // Content Table
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            int rowIndex = 0;
            foreach (var col in columns)
            {
                if (col.ColumnName == "Status") continue;

                string labelText = columnMap.TryGetValue(col.ColumnName, out var mapped) ? mapped : col.ColumnName;
                string valueText = FormatValue(row, col);

                if (string.IsNullOrEmpty(valueText)) continue;

                var lblKey = new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(100, 100, 100),
                    AutoSize = true,
                    Margin = new Padding(0, 2, 0, 2)
                };

                var lblValue = new Label
                {
                    Text = valueText,
                    Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(40, 40, 40),
                    AutoSize = true,
                    MaximumSize = new Size(210, 0),
                    Margin = new Padding(0, 2, 0, 2)
                };

                // Highlight Money
                if (col.ColumnName.Contains("Amount") || col.ColumnName.Contains("Price") || col.ColumnName == "Revenue")
                {
                    lblValue.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    lblValue.ForeColor = Color.FromArgb(0, 120, 215);
                }

                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.Controls.Add(lblKey, 0, rowIndex);
                table.Controls.Add(lblValue, 1, rowIndex);
                rowIndex++;
            }

            container.Controls.Add(table);
            // headerTable is already added above via container.Controls.Add(headerTable)
            // But wait, WinForms adds controls in reverse order of display if using Dock.Top.
            // Let's re-add them clearly.
            container.Controls.Clear();
            container.Controls.Add(table);
            container.Controls.Add(headerTable);
            
            card.Controls.Add(container);

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw border
                using (var pen = new Pen(Color.FromArgb(220, 230, 240), 1.5f))
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 20;
                    var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                    path.CloseFigure();
                    
                    e.Graphics.DrawPath(pen, path);
                }

                // Draw colored strip
                using (var brush = new SolidBrush(accentColor))
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddArc(0, 0, 20, 20, 180, 90);
                    path.AddLine(10, 0, 5, 0);
                    path.AddLine(5, 0, 5, card.Height);
                    path.AddLine(5, card.Height, 10, card.Height);
                    path.AddArc(0, card.Height - 20, 20, 20, 90, 90);
                    path.CloseFigure();
                    e.Graphics.FillPath(brush, path);
                }
            };

            // Set rounded region once or on resize
            card.SizeChanged += (s, e) => UiKit.SetRoundedRegion(card, 20);
            UiKit.SetRoundedRegion(card, 20);

            // Recursively bind hover
            BindHoverRecursive(card, card, container);

            return card;
        }

        private void BindHoverRecursive(Control control, Panel card, Panel container)
        {
             control.MouseEnter += (s, e) => { 
                card.BackColor = Color.FromArgb(240, 248, 255); 
                container.BackColor = Color.FromArgb(240, 248, 255); 
             };
             control.MouseLeave += (s, e) => { 
                card.BackColor = Color.White; 
                container.BackColor = Color.White; 
             };

             foreach(Control child in control.Controls)
             {
                 BindHoverRecursive(child, card, container);
             }
        }

        private Color GetCardAccentColor(DataRow row, string reportType)
        {
            if (row.Table.Columns.Contains("Status"))
            {
                var status = row["Status"]?.ToString();
                if (new[] { "Đã TT", "Đã Thanh Toán", "Hoàn Thành", "Trống" }.Contains(status)) return Color.FromArgb(46, 125, 50);
                if (new[] { "Chưa TT", "Chưa Thanh Toán", "Còn Nợ" }.Contains(status)) return Color.FromArgb(211, 47, 47);
                if (status == "Đã Thuê") return Color.FromArgb(0, 122, 204);
            }
            return Color.FromArgb(0, 122, 204);
        }

        private Control CreateStatusBadge(string status)
        {
            var lbl = new Label
            {
                Text = status,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(8, 4, 8, 4),
                TextAlign = ContentAlignment.MiddleCenter
            };

            if (new[] { "Đã TT", "Đã Thanh Toán", "Hoàn Thành", "Trống" }.Contains(status))
            {
                lbl.BackColor = Color.FromArgb(232, 245, 233);
                lbl.ForeColor = Color.FromArgb(46, 125, 50);
            }
            else if (new[] { "Chưa TT", "Chưa Thanh Toán", "Còn Nợ" }.Contains(status))
            {
                lbl.BackColor = Color.FromArgb(255, 235, 238);
                lbl.ForeColor = Color.FromArgb(198, 40, 40);
            }
            else
            {
                lbl.BackColor = Color.FromArgb(227, 242, 253);
                lbl.ForeColor = Color.FromArgb(21, 101, 192);
            }
            
            // Round Badge
            lbl.Paint += (s, e) => UiKit.SetRoundedRegion(lbl, 12);
            
            return lbl;
        }

        private static string BuildCardTitle(DataRow row, System.Collections.Generic.List<DataColumn> columns,
            System.Collections.Generic.Dictionary<string, string> columnMap, string reportType)
        {
            if (reportType == "Thuế Doanh Thu")
            {
                var period = ReadString(row, "PeriodLabel") ?? ReadString(row, "Period");
                return string.IsNullOrWhiteSpace(period) ? "Thuế doanh thu" : $"Kỳ {period}";
            }

            var invoiceNo = ReadString(row, "InvoiceNumber");
            if (!string.IsNullOrWhiteSpace(invoiceNo))
                return $"Hóa đơn {invoiceNo}";

            var paymentId = ReadString(row, "PaymentId");
            if (!string.IsNullOrWhiteSpace(paymentId) && reportType == "Thanh Toán")
                return $"Thanh toán #{paymentId}";

            if (columns.Count > 0)
            {
                var col = columns[0];
                var label = columnMap.TryGetValue(col.ColumnName, out var mapped) ? mapped : col.ColumnName;
                var value = FormatValue(row, col);
                return $"{label}: {value}";
            }

            return "Báo cáo";
        }

        private static string FormatValue(DataRow row, DataColumn col)
        {
            if (row == null || col == null) return "—";
            var v = row[col];
            if (v == null || v == DBNull.Value) return "—";

            if (col.DataType == typeof(DateTime) || col.ColumnName.Contains("Date"))
            {
                if (DateTime.TryParse(v.ToString(), out var dt))
                    return dt.ToString("dd/MM/yyyy");
            }

            if (col.ColumnName.Equals("TaxRate", StringComparison.OrdinalIgnoreCase))
            {
                if (decimal.TryParse(v.ToString(), out var rate))
                    return rate.ToString("N2");
            }

            if (col.ColumnName.Contains("Amount") || col.ColumnName.Contains("Revenue")
                || col.ColumnName.Contains("Cost") || col.ColumnName.Contains("Price"))
            {
                if (decimal.TryParse(v.ToString(), out var money))
                    return money.ToString("N0");
            }

            return v.ToString();
        }

        private static string ReadString(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            return v == null || v == DBNull.Value ? null : v.ToString();
        }

        private async System.Threading.Tasks.Task<DataTable> LoadRevenueTaxAsync()
        {
            _pnlTaxFilters.Visible = true;
            await EnsureDefaultTaxRateAsync();

            string periodType = _cboTaxPeriodType.SelectedItem?.ToString() ?? "Tháng";
            int year = (int)_numTaxYear.Value;
            int? period = null;
            if (periodType == "Tháng" || periodType == "Quý")
                period = (int)_numTaxPeriod.Value;

            var table = await _bll.GetRevenueByPeriodAsync(periodType, year, period);
            ApplyTaxColumns(table, _numTaxRate.Value);
            return table;
        }

        private void ApplyFilter()
        {
            if (_raw == null)
                return;

            string raw = (_txtSearch.Text ?? string.Empty).Trim();
            if (raw == SearchPlaceholder) raw = string.Empty;
            string keyword = raw.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                _viewTable = _raw;
                _lblCount.Text = $"Tổng: {_raw.Rows.Count}";
                RenderCards(_viewTable);
                return;
            }

            var filtered = _raw.Clone();
            foreach (DataRow row in _raw.Rows)
            {
                if (RowContains(row, keyword))
                    filtered.ImportRow(row);
            }

            _viewTable = filtered;
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
            RenderCards(_viewTable);
        }

        private static bool RowContains(DataRow row, string keyword)
        {
            foreach (DataColumn c in row.Table.Columns)
            {
                var v = row[c];
                if (v == null || v == DBNull.Value) continue;
                if (v.ToString().ToLowerInvariant().Contains(keyword))
                    return true;
            }
            return false;
        }

        private void ExportCsv()
        {
            var dt = _viewTable ?? _raw;
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    var sb = new StringBuilder();
                    var cols = dt.Columns.Cast<DataColumn>().ToList();
                    sb.AppendLine(string.Join(",", cols.Select(c => EscapeCsv(c.ColumnName))));
                    foreach (DataRow r in dt.Rows)
                    {
                        sb.AppendLine(string.Join(",", cols.Select(c => EscapeCsv(r[c]?.ToString()))));
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                    MessageBox.Show("Đã xuất: " + sfd.FileName, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private Panel BuildTaxFilterPanel()
        {
            var panel = new Panel
            {
                Height = 54,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            _cboTaxPeriodType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120
            };
            _cboTaxPeriodType.Items.AddRange(new object[] { "Tháng", "Quý", "Năm" });
            _cboTaxPeriodType.SelectedIndex = 0;
            _cboTaxPeriodType.SelectedIndexChanged += (s, e) => UpdateTaxPeriodPicker();

            _numTaxYear = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 2100,
                Width = 90,
                Value = DateTime.Today.Year
            };

            _numTaxPeriod = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 12,
                Width = 70,
                Value = DateTime.Today.Month
            };

            _numTaxRate = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                DecimalPlaces = 2,
                Increment = 0.1m,
                Width = 90
            };
            _numTaxRate.ValueChanged += (s, e) => RecalculateTaxReport();

            _btnTaxRecalc = UiKit.MakeButton("Tính lại", UiKit.Primary, async (s, e) => await ReloadTaxReportAsync(), 100);
            _btnTaxSaveRate = UiKit.MakeButton("Lưu % thuế", UiKit.Success, async (s, e) => await SaveDefaultTaxRateAsync(), 120);
            _btnTaxOpenInvoices = UiKit.MakeButton("Sửa hóa đơn", UiKit.Warning, (s, e) => OpenInvoiceManager(), 120);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };

            flow.Controls.Add(new Label { Text = "Kỳ:", AutoSize = true, Margin = new Padding(0, 6, 6, 0) });
            flow.Controls.Add(_cboTaxPeriodType);
            flow.Controls.Add(new Label { Text = "Năm:", AutoSize = true, Margin = new Padding(10, 6, 6, 0) });
            flow.Controls.Add(_numTaxYear);
            flow.Controls.Add(new Label { Text = "Tháng/Quý:", AutoSize = true, Margin = new Padding(10, 6, 6, 0) });
            flow.Controls.Add(_numTaxPeriod);
            flow.Controls.Add(new Label { Text = "Thuế %:", AutoSize = true, Margin = new Padding(10, 6, 6, 0) });
            flow.Controls.Add(_numTaxRate);
            flow.Controls.Add(_btnTaxRecalc);
            flow.Controls.Add(_btnTaxSaveRate);
            flow.Controls.Add(_btnTaxOpenInvoices);

            panel.Controls.Add(flow);
            return panel;
        }

        private void UpdateTaxPeriodPicker()
        {
            string periodType = _cboTaxPeriodType.SelectedItem?.ToString() ?? "Tháng";
            if (periodType == "Năm")
            {
                _numTaxPeriod.Enabled = false;
            }
            else
            {
                _numTaxPeriod.Enabled = true;
                _numTaxPeriod.Maximum = periodType == "Quý" ? 4 : 12;
            }

            _ = ReloadTaxReportAsync();
        }

        private async System.Threading.Tasks.Task ReloadTaxReportAsync()
        {
            if ((_cboSource.SelectedItem?.ToString() ?? string.Empty) != "Thuế Doanh Thu")
                return;

            _raw = await LoadRevenueTaxAsync();
            _viewTable = _raw;
            _lblCount.Text = $"Tổng: {_raw?.Rows.Count ?? 0}";
            UpdateFooterSummary();
            ApplyFilter();
        }

        private void ApplyTaxReportDefaults()
        {
            bool isTaxReport = (_cboSource.SelectedItem?.ToString() ?? string.Empty) == "Thuế Doanh Thu";
            _pnlTaxFilters.Visible = !_isStaffMode && isTaxReport;
            if (!isTaxReport)
                return;

            UpdateTaxPeriodPicker();
        }

        private void ApplyTaxColumns(DataTable table, decimal taxRate)
        {
            if (table == null) return;
            if (!table.Columns.Contains("TaxRate"))
                table.Columns.Add("TaxRate", typeof(decimal));
            if (!table.Columns.Contains("TaxAmount"))
                table.Columns.Add("TaxAmount", typeof(decimal));
            if (!table.Columns.Contains("PeriodLabel"))
                table.Columns.Add("PeriodLabel", typeof(string));

            foreach (DataRow row in table.Rows)
            {
                decimal revenue = 0m;
                if (decimal.TryParse(row["Revenue"]?.ToString(), out var rev))
                    revenue = rev;

                row["TaxRate"] = taxRate;
                row["TaxAmount"] = Math.Round(revenue * taxRate / 100m, 2, MidpointRounding.AwayFromZero);

                int year = ReadInt(row, "Year");
                int period = ReadInt(row, "Period");
                row["PeriodLabel"] = BuildPeriodLabel(_cboTaxPeriodType.SelectedItem?.ToString(), year, period);
            }
        }

        private void RecalculateTaxReport()
        {
            if ((_cboSource.SelectedItem?.ToString() ?? string.Empty) != "Thuế Doanh Thu")
                return;
            ApplyTaxColumns(_raw, _numTaxRate.Value);
            RenderCards(_viewTable ?? _raw);
            UpdateFooterSummary();
        }

        private async System.Threading.Tasks.Task EnsureDefaultTaxRateAsync()
        {
            if (_numTaxRate.Value > 0) return;
            try
            {
                var settings = await _bll.GetSystemSettingsAsync();
                if (settings == null || !settings.Columns.Contains("SettingKey")) return;
                var row = settings.AsEnumerable()
                    .FirstOrDefault(r => string.Equals(r["SettingKey"]?.ToString(), "DefaultTaxRatePercent", StringComparison.OrdinalIgnoreCase));
                if (row == null) return;
                if (!decimal.TryParse(row["SettingValue"]?.ToString(), out var rate)) return;
                _numTaxRate.Value = Math.Max(_numTaxRate.Minimum, Math.Min(_numTaxRate.Maximum, rate));
            }
            catch
            {
                // ignore
            }
        }

        private async System.Threading.Tasks.Task SaveDefaultTaxRateAsync()
        {
            try
            {
                var settings = await _bll.GetSystemSettingsAsync();
                var rateText = _numTaxRate.Value.ToString();
                bool exists = settings != null && settings.AsEnumerable()
                    .Any(r => string.Equals(r["SettingKey"]?.ToString(), "DefaultTaxRatePercent", StringComparison.OrdinalIgnoreCase));
                if (exists)
                    await _bll.UpdateSystemSettingAsync("DefaultTaxRatePercent", rateText, "Mức thuế (%) áp dụng theo doanh thu hóa đơn");
                else
                    await _bll.AddSystemSettingAsync("DefaultTaxRatePercent", rateText, "Mức thuế (%) áp dụng theo doanh thu hóa đơn");

                MessageBox.Show("Đã lưu mức thuế.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu mức thuế: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenInvoiceManager()
        {
            using (var frm = new FrmInvoiceManager())
                frm.ShowDialog(this);
        }

        private static string BuildPeriodLabel(string periodType, int year, int period)
        {
            if (string.Equals(periodType, "Quý", StringComparison.OrdinalIgnoreCase))
                return $"Q{Math.Max(1, period)}/{year}";
            if (string.Equals(periodType, "Năm", StringComparison.OrdinalIgnoreCase))
                return year.ToString();
            return $"{period:00}/{year}";
        }

        private static int ReadInt(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0;
            return int.TryParse(v.ToString(), out var parsed) ? parsed : 0;
        }

        private static string EscapeCsv(string value)
        {
            value = value ?? string.Empty;
            if (value.Contains("\""))
                value = value.Replace("\"", "\"\"");
            if (value.Contains(",") || value.Contains("\n") || value.Contains("\r"))
                value = "\"" + value + "\"";
            return value;
        }
    }
}
