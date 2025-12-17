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
            Width = 1300;
            Height = 750;
            BackColor = Color.FromArgb(245, 247, 250);
            this.DoubleBuffered = true;

            _cboSource = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
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

            _txtSearch = new TextBox { Width = 300 };
            var pnlSearch = UiKit.MakeSearchPanel(_txtSearch, 340, SearchPlaceholder, ApplyFilter);

            _btnExport = UiKit.MakeButton("📥 Xuất CSV", UiKit.Purple, (s, e) => ExportCsv(), 110);
            _btnRefresh = UiKit.MakeButton("🔄 Tải Lại", UiKit.Primary, async (s, e) => await LoadDataAsync(), 110);

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold), ForeColor = Color.FromArgb(0, 120, 215) };

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
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            _grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            _grid.RowTemplate.Height = 28;
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 255);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            var top = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(15, 10, 15, 10), BackColor = Color.White };
            top.BorderStyle = BorderStyle.FixedSingle;

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };
            actions.Controls.Add(_btnExport);
            actions.Controls.Add(_btnRefresh);

            var filter = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var lblSource = new Label { Text = "Báo Cáo:", AutoSize = true, ForeColor = Color.FromArgb(70, 70, 70), Font = new Font("Segoe UI", 10), Location = new Point(0, 12) };
            _cboSource.Location = new Point(lblSource.Right + 8, 8);
            
            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, ForeColor = Color.FromArgb(70, 70, 70), Font = new Font("Segoe UI", 10) };
            lblSearch.Location = new Point(_cboSource.Right + 20, 12);
            pnlSearch.Location = new Point(lblSearch.Right + 8, 10);

            filter.Controls.Add(lblSource);
            filter.Controls.Add(_cboSource);
            filter.Controls.Add(lblSearch);
            filter.Controls.Add(pnlSearch);
            filter.Resize += (s, e) =>
            {
                _cboSource.Location = new Point(lblSource.Right + 8, 8);
                lblSearch.Location = new Point(_cboSource.Right + 20, 12);
                pnlSearch.Location = new Point(lblSearch.Right + 8, 10);
            };

            var summary = new Panel { Dock = DockStyle.Left, Width = 180, BackColor = Color.Transparent };
            _lblCount.Location = new Point(5, 20);
            summary.Controls.Add(_lblCount);

            top.Controls.Add(filter);
            top.Controls.Add(actions);
            top.Controls.Add(summary);

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BackColor };
            gridHost.Controls.Add(_grid);

            Controls.Add(gridHost);
            Controls.Add(top);

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
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TranslateGridHeaders(DataGridView grid)
        {
            if (grid == null || grid.Columns.Count == 0) return;

            // Comprehensive translation dictionary for all reports
            var columnMap = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Invoice columns
                { "InvoiceId", "ID" },
                { "InvoiceNumber", "Số Hóa Đơn" },
                { "TenantId", "Khách ID" },
                { "TenantName", "Tên Khách" },
                { "RoomId", "Phòng ID" },
                { "RoomNumber", "Số Phòng" },
                { "InvoiceDate", "Ngày Lập" },
                { "FromDate", "Từ Ngày" },
                { "ToDate", "Đến Ngày" },
                { "RentalCost", "Tiền Thuê" },
                { "UtilityCost", "Chi Phí Tiện Ích" },
                { "OtherCost", "Chi Phí Khác" },
                { "TotalAmount", "Tổng Tiền" },
                { "PaidAmount", "Đã Thanh Toán" },
                { "RemainingAmount", "Còn Nợ" },
                { "Status", "Trạng Thái" },
                { "DueDate", "Ngày Hạn" },
                { "CreatedDate", "Ngày Tạo" },
                { "UpdatedDate", "Ngày Cập Nhập" },
                
                // Payment columns
                { "PaymentId", "ID Thanh Toán" },
                { "PaymentDate", "Ngày Thanh Toán" },
                { "PaymentAmount", "Số Tiền" },
                { "PaymentMethod", "Phương Thức" },
                { "TransactionReference", "Tham Chiếu Giao Dịch" },
                { "Notes", "Ghi Chú" },
                
                // Tenant columns
                { "FullName", "Tên Đầy Đủ" },
                { "IdentityCard", "CMND/CCCD" },
                { "PhoneNumber", "Điện Thoại" },
                { "Email", "Email" },
                { "BirthDate", "Ngày Sinh" },
                { "Address", "Địa Chỉ" },
                { "TempReg", "Nơi Tạm Trú" },
                { "TempRegDate", "Ngày Tạm Trú" },
                { "TempRegExpiry", "Hết Hạn Tạm Trú" },
                { "IsActive", "Hoạt Động" },
                { "CreatedBy", "Người Tạo" },
                
                // Room columns
                { "RoomTypeId", "Loại Phòng ID" },
                { "BranchId", "Chi Nhánh" },
                { "SectionId", "Khu Vực ID" },
                { "RoomStatusId", "Trạng Thái Phòng ID" },
                { "RoomPrice", "Giá Phòng" },
                { "Capacity", "Sức Chứa" },
                { "Occupied", "Đã Sử Dụng" },
                
                // Contract columns
                { "ContractId", "ID Hợp Đồng" },
                { "ContractNumber", "Số Hợp Đồng" },
                { "ContractType", "Loại Hợp Đồng" },
                { "SignDate", "Ngày Ký" },
                { "StartDate", "Ngày Bắt Đầu" },
                { "EndDate", "Ngày Kết Thúc" },
                { "RentalPrice", "Giá Thuê" },
                { "DepositRequired", "Cọc Yêu Cầu" },
                { "Terms", "Điều Khoản" },
                { "ContractPdfPath", "Đường Dẫn PDF" },
                
                // Deposit columns
                { "DepositId", "ID Cọc" },
                { "DepositAmount", "Tiền Cọc" },
                { "DepositDate", "Ngày Cọc" },
                { "DepositType", "Loại Cọc" },
                { "ReturnedAmount", "Số Tiền Hoàn" },
                { "ReturnedDate", "Ngày Hoàn" },
                
                // Maintenance columns
                { "MaintenanceId", "ID Bảo Trì" },
                { "RequestDate", "Ngày Yêu Cầu" },
                { "CompletionDate", "Ngày Hoàn Thành" },
                { "Cost", "Chi Phí" },
                { "Category", "Danh Mục" },
                { "Description", "Mô Tả" },
                
                // Asset columns
                { "AssetId", "ID Tài Sản" },
                { "AssetName", "Tên Tài Sản" },
                { "AssetValue", "Giá Trị" },
                { "Condition", "Tình Trạng" },
                { "PurchaseDate", "Ngày Mua" },
                
                // Notification columns
                { "NotificationId", "ID Thông Báo" },
                { "NotificationTitle", "Tiêu Đề" },
                { "NotificationContent", "Nội Dung" },
                { "NotificationDate", "Ngày Gửi" },
                { "RecipientType", "Loại Người Nhận" },
                
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

            // Auto-size columns
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (col.Width < 80)
                    col.Width = 80;
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
                case "Hóa đơn":
                    return _bll.GetInvoicesViewAsync();
                case "Thanh toán":
                    return _bll.GetPaymentsViewAsync();
                case "Khách thuê":
                    return _bll.GetTenantsAsync();
                case "Phòng":
                    return _bll.GetRoomsAsync();
                case "Hợp đồng":
                    return _bll.GetContractsAsync();
                case "Đặt cọc":
                    return _bll.GetDepositsAsync();
                case "Bảo trì":
                    return _bll.GetMaintenanceAsync();
                case "Tài sản":
                    return _bll.GetAssetsAsync();
                case "Thông báo":
                    return _bll.GetNotificationsAsync();
                case "Cấu hình hệ thống":
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
