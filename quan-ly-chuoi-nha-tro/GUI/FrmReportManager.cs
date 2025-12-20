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
        private System.Collections.Generic.HashSet<int> _allowedBranchIds;

        private ComboBox _cboSource;
        private TextBox _txtSearch;
        private DataGridView _grid;
        private Label _lblCount;
        private Label _lblFooterSummary;
        private Button _btnExport;
        private Button _btnRefresh;

        private DataTable _raw;

        public FrmReportManager(AdminDataBLL bll)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
            InitializeComponent();
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
            _cboSource.Items.AddRange(new object[]
            {
                "Hóa Đơn",
                "Thanh Toán",
                "Khách Thuê",
                "Phòng",
                "Hợp Đồng",
                "Đặt Cọc",
                "Bảo Trì",
                "Tài Sản",
                "Thông Báo",
                "Cấu Hình Hệ Thống"
            });
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

            // ===== GRID =====
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowTemplate = { Height = 28 }
            };
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            _grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _grid.DefaultCellStyle.ForeColor = Color.FromArgb(50, 50, 50);
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            _grid.GridColor = Color.FromArgb(220, 230, 240);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(179, 211, 247);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;

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

            // ===== GRID HOST =====
            var gridHost = new Panel 
            { 
                Dock = DockStyle.Fill, 
                Padding = new Padding(12), 
                BackColor = BackColor 
            };
            gridHost.Controls.Add(_grid);

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
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);

            Load += async (s, e) => await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _raw = await LoadSelectedAsync();
                await EnsureAllowedBranchScopeAsync();
                if (_raw != null && _raw.Columns.Contains("BranchId"))
                    _raw = AdminBranchScope.FilterByBranchIds(_raw, _allowedBranchIds);
                _grid.DataSource = _raw;
                TranslateGridHeaders(_grid);
                _lblCount.Text = $"Tổng: {_raw?.Rows.Count ?? 0}";
                UpdateFooterSummary();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var columnMap = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
                { "SettingDescription", "Mô Tả" }
            };

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
            }

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
                    return _bll.GetSystemSettingsAsync();
                default:
                    return _bll.GetInvoicesViewAsync();
            }
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
                _grid.DataSource = _raw;
                _lblCount.Text = $"Tổng: {_raw.Rows.Count}";
                return;
            }

            var filtered = _raw.Clone();
            foreach (DataRow row in _raw.Rows)
            {
                if (RowContains(row, keyword))
                    filtered.ImportRow(row);
            }

            _grid.DataSource = filtered;
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
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
            var dt = _grid.DataSource as DataTable;
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
