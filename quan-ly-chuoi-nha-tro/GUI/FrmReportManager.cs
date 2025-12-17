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
            Font = new Font("Times New Roman", 11);

            // Header panel
            var header = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 80, 
                BackColor = Color.White, 
                Padding = new Padding(16, 12, 16, 12) 
            };

            var lblTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Times New Roman", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Text = "📊 Báo Cáo & Thống Kê",
                Dock = DockStyle.Top,
                Margin = new Padding(0)
            };

            var lblSub = new Label
            {
                AutoSize = true,
                Font = new Font("Times New Roman", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(90, 90, 90),
                Text = "Xem và xuất dữ liệu từ các module",
                Dock = DockStyle.Top,
                Margin = new Padding(0, 4, 0, 0)
            };

            _btnExport = UiKit.MakeButton("📥 Xuất CSV", UiKit.Purple, (s, e) => ExportCsv(), 110);
            _btnRefresh = UiKit.MakeButton("🔄 Tải Lại", UiKit.Primary, async (s, e) => await LoadDataAsync(), 110);

            var headerButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0, 6, 0, 0)
            };
            _btnExport.Margin = new Padding(0, 0, 10, 0);
            _btnRefresh.Margin = new Padding(0, 0, 0, 0);
            headerButtons.Controls.Add(_btnExport);
            headerButtons.Controls.Add(_btnRefresh);

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var headerText = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            headerText.Controls.Add(lblSub);
            headerText.Controls.Add(lblTitle);

            headerLayout.Controls.Add(headerText, 0, 0);
            headerLayout.Controls.Add(headerButtons, 1, 0);

            header.Controls.Add(headerLayout);
            header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 3, BackColor = Color.FromArgb(0, 120, 215) });

            // Filter toolbar
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

            _lblCount = new Label 
            { 
                AutoSize = true, 
                Text = "Tổng: 0", 
                Font = new Font("Times New Roman", 11, FontStyle.Bold), 
                ForeColor = Color.FromArgb(0, 120, 215) 
            };

            var toolbar = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 70, 
                Padding = new Padding(15, 10, 15, 10), 
                BackColor = Color.White 
            };
            toolbar.BorderStyle = BorderStyle.FixedSingle;

            var filter = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var lblSource = new Label 
            { 
                Text = "Báo Cáo:", 
                AutoSize = true, 
                ForeColor = Color.FromArgb(70, 70, 70), 
                Font = new Font("Times New Roman", 11), 
                Location = new Point(0, 12) 
            };
            _cboSource.Location = new Point(lblSource.Right + 8, 8);
            
            var lblSearch = new Label 
            { 
                Text = "Tìm:", 
                AutoSize = true, 
                ForeColor = Color.FromArgb(70, 70, 70), 
                Font = new Font("Times New Roman", 11) 
            };
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

            var summary = new Panel { Dock = DockStyle.Left, Width = 280, BackColor = Color.Transparent };
            _lblCount.Location = new Point(5, 20);
            summary.Controls.Add(_lblCount);

            toolbar.Controls.Add(filter);
            toolbar.Controls.Add(summary);

            // Grid
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
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 11, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grid.DefaultCellStyle.Font = new Font("Times New Roman", 11);
            _grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            _grid.RowTemplate.Height = 28;
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 252);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BackColor };
            gridHost.Controls.Add(_grid);

            Controls.Add(gridHost);
            Controls.Add(toolbar);
            Controls.Add(header);

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
                ApplyMoneyFormatting(_grid);
                UpdateSummaryStats(_raw);
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
                { "TransactionReference", "Mã GD" },
                { "Notes", "Ghi Chú" },
                { "PaymentStatus", "Trạng Thái Thanh Toán" },
                { "RemainingAmount", "Còn Nợ" },
                { "FromDate", "Từ Ngày" },
                { "ToDate", "Đến Ngày" },
                
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

        private void ApplyMoneyFormatting(DataGridView grid)
        {
            if (grid == null) return;
            
            // Money columns that need formatting
            var moneyColumns = new[] 
            { 
                "PaymentAmount", "TotalAmount", "PaidAmount", "RemainingAmount",
                "RentalCost", "UtilityCost", "OtherCost", "RoomPrice",
                "DepositAmount", "DepositRequired", "ReturnedAmount",
                "AssetValue", "Cost", "RentalPrice"
            };
            
            // Date columns that need formatting
            var dateColumns = new[]
            {
                "PaymentDate", "InvoiceDate", "FromDate", "ToDate", "DueDate",
                "BirthDate", "TempRegDate", "TempRegExpiry", "DepositDate",
                "ReturnedDate", "RequestDate", "CompletionDate", "SignDate",
                "StartDate", "EndDate", "PurchaseDate", "NotificationDate",
                "CreatedDate", "UpdatedDate"
            };

            // Wide columns for notes and descriptions
            var wideColumns = new[] { "Notes", "Description", "NotificationContent", "SettingValue", "SettingDescription" };

            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (moneyColumns.Contains(col.Name, StringComparer.OrdinalIgnoreCase))
                {
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (dateColumns.Contains(col.Name, StringComparer.OrdinalIgnoreCase))
                {
                    col.DefaultCellStyle.Format = "dd/MM/yyyy";
                }
                else if (wideColumns.Contains(col.Name, StringComparer.OrdinalIgnoreCase))
                {
                    col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    grid.AutoResizeRowsHeightForHeader = true;
                }
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

        private async System.Threading.Tasks.Task<DataTable> LoadSelectedAsync()
        {
            var selected = _cboSource.SelectedItem?.ToString() ?? string.Empty;
            DataTable result = null;
            
            switch (selected)
            {
                case "Hóa đơn":
                    result = await _bll.GetInvoicesViewAsync();
                    break;
                case "Thanh toán":
                    result = await _bll.GetPaymentsViewAsync();
                    result = EnrichPaymentData(result);
                    break;
                case "Khách thuê":
                    result = await _bll.GetTenantsAsync();
                    break;
                case "Phòng":
                    result = await _bll.GetRoomsAsync();
                    break;
                case "Hợp đồng":
                    result = await _bll.GetContractsAsync();
                    break;
                case "Đặt cọc":
                    result = await _bll.GetDepositsAsync();
                    break;
                case "Bảo trì":
                    result = await _bll.GetMaintenanceAsync();
                    break;
                case "Tài sản":
                    result = await _bll.GetAssetsAsync();
                    break;
                case "Thông báo":
                    result = await _bll.GetNotificationsAsync();
                    break;
                case "Cấu hình hệ thống":
                    result = await _bll.GetSystemSettingsAsync();
                    break;
                default:
                    result = await _bll.GetInvoicesViewAsync();
                    break;
            }
            
            return result;
        }

        private DataTable EnrichPaymentData(DataTable payments)
        {
            if (payments == null) return payments;

            // Thêm cột ghi chú nếu chưa có
            if (!payments.Columns.Contains("Notes"))
                payments.Columns.Add("Notes", typeof(string));
            if (!payments.Columns.Contains("PaymentStatus"))
                payments.Columns.Add("PaymentStatus", typeof(string));
            if (!payments.Columns.Contains("RemainingAmount"))
                payments.Columns.Add("RemainingAmount", typeof(decimal));
            if (!payments.Columns.Contains("FromDate"))
                payments.Columns.Add("FromDate", typeof(DateTime));
            if (!payments.Columns.Contains("ToDate"))
                payments.Columns.Add("ToDate", typeof(DateTime));

            foreach (DataRow row in payments.Rows)
            {
                // Ghi chú thanh toán thành công
                var paymentAmount = ReadDecimal(row, "PaymentAmount");
                var remainingAmount = ReadDecimal(row, "RemainingAmount");
                var notes = row["Notes"]?.ToString() ?? "";

                if (!string.IsNullOrWhiteSpace(notes))
                    notes += " | ";

                notes += $"✓ Thanh toán thành công: {paymentAmount:N0}";

                if (remainingAmount > 0)
                    notes += $" | Còn công nợ: {remainingAmount:N0}";

                // Thêm khoảng thời gian nếu có
                if (row.Table.Columns.Contains("FromDate") && row["FromDate"] != DBNull.Value &&
                    row.Table.Columns.Contains("ToDate") && row["ToDate"] != DBNull.Value)
                {
                    var fromDate = Convert.ToDateTime(row["FromDate"]);
                    var toDate = Convert.ToDateTime(row["ToDate"]);
                    notes += $" | Kỳ: {fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}";
                }

                row["Notes"] = notes;
                row["PaymentStatus"] = remainingAmount > 0 ? "Thanh toán một phần" : "Đã thanh toán";
            }

            return payments;
        }

        private static decimal ReadDecimal(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0m;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            if (decimal.TryParse(v.ToString(), out var parsed)) return parsed;
            return 0m;
        }

        private void ApplyFilter()
        {
            if (_raw == null)
                return;

            string raw = (_txtSearch.Text ?? string.Empty).Trim();
            if (raw == SearchPlaceholder) raw = string.Empty;
            string keyword = raw.ToLowerInvariant();

            DataTable displayed;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                displayed = _raw;
            }
            else
            {
                displayed = _raw.Clone();
                foreach (DataRow row in _raw.Rows)
                {
                    if (RowContains(row, keyword))
                        displayed.ImportRow(row);
                }
            }

            _grid.DataSource = displayed;
            _lblCount.Text = $"Tổng: {displayed.Rows.Count}";
            UpdateSummaryStats(displayed);
        }

        private void UpdateSummaryStats(DataTable displayed)
        {
            var selectedReport = _cboSource.SelectedItem?.ToString() ?? string.Empty;
            
            // Only calculate totals for specific reports
            if (selectedReport == "Thanh toán")
            {
                decimal totalPaid = 0m;
                decimal totalRemaining = 0m;
                int successfulPayments = 0;
                int partialPayments = 0;

                if (displayed != null)
                {
                    foreach (DataRow r in displayed.Rows)
                    {
                        if (decimal.TryParse(r["PaymentAmount"]?.ToString(), out var paid))
                            totalPaid += paid;
                        if (decimal.TryParse(r["RemainingAmount"]?.ToString(), out var remaining))
                            totalRemaining += remaining;

                        var status = r["PaymentStatus"]?.ToString() ?? "";
                        if (status.Contains("Đã thanh toán"))
                            successfulPayments++;
                        else if (status.Contains("một phần"))
                            partialPayments++;
                    }
                }

                _lblCount.Text = $"Tổng: {displayed?.Rows.Count ?? 0} | Đã thu: {totalPaid:N0} | Còn nợ: {totalRemaining:N0} | ✓ Toàn bộ: {successfulPayments} | ◐ Một phần: {partialPayments}";
            }
            else if (selectedReport == "Hóa đơn")
            {
                decimal totalAmount = 0m;
                decimal remainingAmount = 0m;
                if (displayed != null)
                {
                    if (displayed.Columns.Contains("TotalAmount"))
                    {
                        foreach (DataRow r in displayed.Rows)
                        {
                            if (decimal.TryParse(r["TotalAmount"]?.ToString(), out var v))
                                totalAmount += v;
                        }
                    }
                    if (displayed.Columns.Contains("RemainingAmount"))
                    {
                        foreach (DataRow r in displayed.Rows)
                        {
                            if (decimal.TryParse(r["RemainingAmount"]?.ToString(), out var v))
                                remainingAmount += v;
                        }
                    }
                }
                _lblCount.Text = $"Tổng: {displayed?.Rows.Count ?? 0} | Tổng tiền: {totalAmount:N0} | Còn nợ: {remainingAmount:N0}";
            }
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
