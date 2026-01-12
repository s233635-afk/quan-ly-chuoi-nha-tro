using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    // ================================================================
    // PHẦN 1: MÀN HÌNH QUẢN LÝ HỢP ĐỒNG (DẠNG THẺ - CARD VIEW)
    // ================================================================
    public class FrmContractManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo số HĐ/khách/phòng...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly StaffBLL _staffBll = new StaffBLL();
        private readonly int? _branchId;
        private readonly bool _isStaffMode;
        private HashSet<int> _allowedBranchIds;
        private DataTable _rawTable;

        // Thay GridView bằng FlowLayoutPanel để hiển thị dạng thẻ
        private FlowLayoutPanel _flowPanel;
        private ModernSearchBox _txtSearch;
        private ComboBox _cboStatus;
        private ComboBox _cboSort;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private Label _lblCount;
        private Button _btnAdd, _btnDelete, _btnRefresh;

        // Biến để theo dõi thẻ và dữ liệu đang được chọn
        private (Panel Card, DataRow Row)? _selectedItem;

        public FrmContractManager() : this(null, false) { }

        public FrmContractManager(int? branchId) : this(branchId, false) { }

        public FrmContractManager(int? branchId, bool isStaffMode)
        {
            _branchId = branchId;
            _isStaffMode = isStaffMode;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            // --- Cấu hình Form chính ---
            Text = "Quản lý Hợp đồng";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1280;
            Height = 760;
            BackColor = Color.FromArgb(245, 247, 250);

            // =========================================================================
            // PHẦN HEADER: SỬ DỤNG TABLELAYOUTPANEL ĐỂ BỐ CỤC CHUẨN HƠN
            // =========================================================================

            // 1. Panel Header chính
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80, // Reduced from 130 to 80 since we're combining into one row
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            topPanel.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.LightGray }); // Đường kẻ dưới

            // 2. Single FlowLayoutPanel containing both buttons and filters in one row
            var mainFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // Hàng 1 cao 40px
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Hàng 2 chiếm phần còn lại

            // 3. Panel chứa các nút chức năng (Hàng 1)
            var pnlActions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent };
            var btnAdd = UiKit.MakeButton("Thêm Hợp đồng", UiKit.Primary, async (s, e) => await AddNewAsync());
            var btnRefresh = UiKit.MakeButton("Tải lại", Color.Gray, async (s, e) => await LoadDataAsync());
            pnlActions.Controls.AddRange(new Control[] { btnAdd, btnRefresh });

            // 4. Panel chứa các bộ lọc (Hàng 2)
            var pnlFilters = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = Color.Transparent,
                ColumnCount = 11, // 1 search, 6 filter controls, 1 label count, 1 spacer
                RowCount = 1
            };
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F)); // Search
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Status label
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F)); // Status
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Sort label
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F)); // Sort
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // From label
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F)); // From
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // To label
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F)); // To
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Count
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Spacer
            
            // Tạo các control cho bộ lọc
            _txtSearch = new TextBox { Width = 220, Font = new Font("Segoe UI", 10) };
            var pnlSearch = UiKit.MakeSearchPanel(_txtSearch, 230, SearchPlaceholder, ApplyFilter);

            _cboStatus = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.None };
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Active", "Extended", "Terminated", "Expired" });
            _cboStatus.SelectedIndex = 0;

            _cboSort = new ComboBox { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.None };
            _cboSort.Items.AddRange(new object[] { "H\u1ee3p \u0111\u1ed3ng m\u1edbi nh\u1ea5t", "H\u1ee3p \u0111\u1ed3ng c\u0169 nh\u1ea5t" });
            _cboSort.SelectedIndex = 0;

            _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 110, Font = new Font("Segoe UI", 10), Anchor = AnchorStyles.None };
            _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 110, Font = new Font("Segoe UI", 10), Anchor = AnchorStyles.None };
            
            _lblCount = new Label { Anchor = AnchorStyles.None, AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(10, 0, 0, 0), ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft };

            // Gán sự kiện
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cboSort.SelectedIndexChanged += (s, e) => ApplyFilter();
                WrapContents = false, // Keep everything on one line
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // 3. Create buttons
            _btnAdd = UiKit.MakeButton("Thêm Hợp đồng", UiKit.Primary, async (s, e) => await AddNewAsync(), 130);
            _btnDelete = UiKit.MakeButton("Xóa Hợp đồng", UiKit.Danger, async (s, e) => await DeleteSelectedAsync(), 120);
            _btnRefresh = UiKit.MakeButton("Tải lại", UiKit.Primary, async (s, e) => await LoadDataAsync(), 92);
            _btnDelete.Visible = !_isStaffMode;
            _btnDelete.Enabled = !_isStaffMode;
            
            // Add buttons to flow
            mainFlow.Controls.Add(_btnAdd);
            mainFlow.Controls.Add(_btnDelete);
            mainFlow.Controls.Add(_btnRefresh);

            // 4. Create a panel for filters that will sit next to buttons
            var pnlFilters = new FlowLayoutPanel
            {
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(20, 0, 0, 0) // Left margin to separate from buttons
            };
            
            // Create filter controls
            _txtSearch = new ModernSearchBox
            {
                Width = 220,
                PlaceholderText = SearchPlaceholder,
                Margin = new Padding(0, 0, 10, 0)
            };
            _txtSearch.SearchTriggered += (s, e) => ApplyFilter();

            _cboStatus = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 10, 0) };
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Đang hiệu lực", "Gia hạn", "Đã chấm dứt", "Hết hạn" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 110, Font = new Font("Segoe UI", 10), Margin = new Padding(0, 0, 10, 0) };
            _dtFrom.ValueChanged += (s, e) => ApplyFilter();
            
            _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 110, Font = new Font("Segoe UI", 10), Margin = new Padding(0, 0, 10, 0) };
            _dtTo.ValueChanged += (s, e) => ApplyFilter();
            
            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(10, 0, 0, 0), ForeColor = Color.DimGray };

            // Thêm các control vào các cột tương ứng của TableLayoutPanel
            pnlFilters.Controls.Add(pnlSearch, 0, 0);
            pnlFilters.Controls.Add(CreateFilterLabel("Tr\u1ea1ng th\u00e1i:"), 1, 0);
            pnlFilters.Controls.Add(_cboStatus, 2, 0);
            pnlFilters.Controls.Add(CreateFilterLabel("S\u1eafp x\u1ebfp:"), 3, 0);
            pnlFilters.Controls.Add(_cboSort, 4, 0);
            pnlFilters.Controls.Add(CreateFilterLabel("T\u1eeb:"), 5, 0);
            pnlFilters.Controls.Add(_dtFrom, 6, 0);
            pnlFilters.Controls.Add(CreateFilterLabel("\u0110\u1ebfn:"), 7, 0);
            pnlFilters.Controls.Add(_dtTo, 8, 0);
            pnlFilters.Controls.Add(_lblCount, 9, 0);
            // Add filter controls to pnlFilters
            pnlFilters.Controls.Add(_txtSearch);
            pnlFilters.Controls.Add(CreateFilterLabel("Trạng thái:"));
            pnlFilters.Controls.Add(_cboStatus);
            pnlFilters.Controls.Add(CreateFilterLabel("Từ:"));
            pnlFilters.Controls.Add(_dtFrom);
            pnlFilters.Controls.Add(CreateFilterLabel("Đến:"));
            pnlFilters.Controls.Add(_dtTo);
            pnlFilters.Controls.Add(_lblCount);

            // Add filters panel to main flow
            mainFlow.Controls.Add(pnlFilters);

            // Add main flow to top panel
            topPanel.Controls.Add(mainFlow);

            // =========================================================================
            // PHẦN BODY: DANH SÁCH CÁC THẺ HỢP ĐỒNG
            // =========================================================================
            _flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(15) // Khoảng cách viền
            };

            // Thêm các panel chính vào Form
            Controls.Add(_flowPanel);
            Controls.Add(topPanel);

            Load += async (s, e) => await LoadDataAsync();
        }

        // Hàm tiện ích để tạo các nhãn trong bộ lọc cho đồng bộ
        private Label CreateFilterLabel(string text)
        {
            return new Label {
                Text = text,
                AutoSize = true,
                Margin = new Padding(5, 0, 5, 0), // Simplified margin
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                // Hiển thị trạng thái đang tải (Optional)
                _flowPanel.Controls.Clear();
                var loading = new Label { Text = "Đang tải dữ liệu...", AutoSize = true, Font = new Font("Segoe UI", 12), ForeColor = Color.Gray, Padding = new Padding(20) };
                _flowPanel.Controls.Add(loading);

                await EnsureAllowedBranchScopeAsync();
                _rawTable = await _bll.GetContractsAsync();
                _rawTable = _branchId.HasValue ? FilterByBranch(_rawTable, _branchId) : AdminBranchScope.FilterByBranchIds(_rawTable, _allowedBranchIds);
                _rawTable = await FilterContractsByRentedRoomsAsync(_rawTable);
                TextFixer.FixDataTable(_rawTable, "ContractNumber", "TenantName", "TenantPhone", "RoomNumber", "BranchName", "Status");
                await EnrichContractsAsync(_rawTable);
                await EnrichContractsAsync(_rawTable); // Enrich FIRST to add TenantName, RoomNumber, BranchName columns
                TextFixer.FixDataTable(_rawTable, "ContractNumber", "TenantName", "RoomNumber", "BranchName", "Status");
                
                ApplyFilter(); // Hàm này sẽ gọi RenderCards
                _selectedItem = null; // Bỏ chọn khi tải lại
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "LoadContracts", "Không thể tải hợp đồng");
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

        private void ApplyFilter()
        {
            if (_rawTable == null) return;
            string keyword = (_txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            string status = _cboStatus.SelectedItem?.ToString();
            DateTime? from = _dtFrom.Checked ? (DateTime?)_dtFrom.Value.Date : null;
            DateTime? to = _dtTo.Checked ? (DateTime?)_dtTo.Value.Date : null;

            var query = _rawTable.AsEnumerable();
            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                string dbStatus = FrmContractDetail.FromVietnameseStatus(status);
                query = query.Where(r => string.Equals(r["Status"]?.ToString(), dbStatus, StringComparison.OrdinalIgnoreCase));
            }
            if (from.HasValue) query = query.Where(r => DateTime.TryParse(r["StartDate"]?.ToString(), out var d) && d.Date >= from.Value);
            if (to.HasValue) query = query.Where(r => DateTime.TryParse(r["EndDate"]?.ToString(), out var d) && d.Date <= to.Value);
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(r => Contains(r, "ContractNumber", keyword) || Contains(r, "TenantName", keyword) || Contains(r, "RoomNumber", keyword) || Contains(r, "TenantPhone", keyword));

            if (_cboSort != null && _cboSort.SelectedIndex == 1)
                query = query.OrderBy(r => GetSortDate(r)).ThenBy(r => GetSortId(r));
            else
                query = query.OrderByDescending(r => GetSortDate(r)).ThenByDescending(r => GetSortId(r));

            var resultTable = query.Any() ? query.CopyToDataTable() : null;
            _lblCount.Text = resultTable != null ? $"Tổng: {resultTable.Rows.Count}" : "Tổng: 0";
            
            // Render Cards thay vì bind vào Grid
            RenderCards(resultTable);
        }

        // =========================================================================
        // HÀM QUAN TRỌNG: VẼ CÁC THẺ (CARDS) GIỐNG NHƯ ẢNH BẠN MUỐN
        // =========================================================================
        private void RenderCards(DataTable dt)
        {
            _flowPanel.Controls.Clear();
            if (dt == null || dt.Rows.Count == 0) return;

            foreach (DataRow row in dt.Rows)
            {
                // 1. Tạo Panel chính (Card)
                var card = new Panel
                {
                    Width = 380,
                    Height = 170,
                    BackColor = Color.White,
                    Margin = new Padding(10), 
                    Padding = new Padding(1), // Để tạo hiệu ứng viền khi chọn
                    Cursor = Cursors.Hand // Để người dùng biết có thể click
                };

                // Hiệu ứng viền trái thể hiện trạng thái (Xanh = Active, Đỏ = Expired)
                string status = SafeReadString(row, "Status");
                Color statusColor = status == "Active" ? Color.SeaGreen : (status == "Expired" ? Color.Firebrick : Color.Gray);
                
                var statusStrip = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = statusColor };
                
                // 2. Nội dung bên trong Card
                var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

                // Header: Số Hợp Đồng + Trạng thái
                var lblContractNo = new Label 
                { 
                    Text = SafeReadString(row, "ContractNumber"), 
                    Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                    ForeColor = Color.FromArgb(0, 100, 200),
                    AutoSize = true, 
                    Location = new Point(10, 10) 
                };

                var lblStatus = new Label
                {
                    Text = FrmContractDetail.ToVietnameseStatus(status),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = statusColor,
                    AutoSize = true,
                    Location = new Point(280, 12)
                };

                // Body: tenant, room, phone, price
                var tenantName = row["TenantName"]?.ToString() ?? string.Empty;
                var roomNumber = row["RoomNumber"]?.ToString() ?? string.Empty;
                var tenantPhone = string.Empty;
                if (row.Table.Columns.Contains("TenantPhone"))
                    tenantPhone = row["TenantPhone"]?.ToString();
                else if (row.Table.Columns.Contains("PhoneNumber"))
                    tenantPhone = row["PhoneNumber"]?.ToString();

                var lblTenant = new Label
                {
                    Text = "Kh\u00e1ch: " + tenantName,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    Location = new Point(10, 45),
                    AutoSize = true
                };

                var lblRoom = new Label
                {
                    Text = "Ph\u00f2ng: " + roomNumber,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    Location = new Point(10, 70),
                    AutoSize = true
                // Body: Tên khách, Phòng, Giá
                var lblTenant = new Label 
                { 
                    Text = "Khách: " + (SafeReadString(row, "TenantName") ?? "—"), 
                    Font = new Font("Segoe UI", 10, FontStyle.Regular), 
                    Location = new Point(10, 45), 
                    AutoSize = true 
                };

                var lblRoom = new Label 
                { 
                    Text = "Phòng: " + (SafeReadString(row, "RoomNumber") ?? "—"), 
                    Font = new Font("Segoe UI", 10, FontStyle.Regular), 
                    Location = new Point(10, 70), 
                    AutoSize = true 
                };

                var lblPhone = new Label
                {
                    Text = "S\u0110T: " + tenantPhone,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    Location = new Point(10, 95),
                    AutoSize = true
                };

                decimal price = 0;
                decimal.TryParse(row["RentalPrice"]?.ToString(), out price);
                var lblPrice = new Label
                {
                    Text = "Gi\u00e1: " + price.ToString("N0") + " \u0111",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.DarkSlateGray,
                    Location = new Point(200, 70),
                    Location = new Point(200, 70), 
                    AutoSize = true 
                };

                // Footer: Thời gian
                var lblDate = new Label
                {
                    Text = $"Thời hạn: {Convert.ToDateTime(row["StartDate"]):dd/MM/yyyy} - {Convert.ToDateTime(row["EndDate"]):dd/MM/yyyy}",
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Location = new Point(10, 110),
                    AutoSize = true
                };
                
                var line = new Panel { Height = 1, BackColor = Color.LightGray, Width = 340, Location = new Point(10, 135) };
                
                var lblBranch = new Label
                {
                    Text = SafeReadString(row, "BranchName") ?? "—",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.DimGray,
                    Location = new Point(10, 142),
                    AutoSize = true
                };

                // ThA?m controls vA?o card
                content.Controls.AddRange(new Control[] { lblContractNo, lblStatus, lblTenant, lblRoom, lblPhone, lblPrice });
                card.Controls.Add(content);
                card.Controls.Add(statusStrip);

                // 3. Sự kiện Click và DoubleClick
                // Click: Chọn thẻ. DoubleClick: Mở chi tiết
                Action<object, EventArgs> selectAction = (s, e) => SelectCard(card, row);
                card.Click += new EventHandler(selectAction);
                foreach (Control c in content.Controls) c.Click += new EventHandler(selectAction);
                card.DoubleClick += (s, e) => ShowDetail(row);
                foreach (Control c in content.Controls) c.DoubleClick += (s, e) => ShowDetail(row);
                _flowPanel.Controls.Add(card);
            }
        }

        private void ShowDetail(DataRow row)
        {
            // Mở form chi tiết giống như ảnh bạn gửi
            using (var frm = new FrmContractDetail(row, _bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadDataAsync(); // Load lại nếu có sửa đổi
                }
            }
        }

        private async System.Threading.Tasks.Task AddNewAsync()
        {
            using (var frm = new FrmContractEditor(_bll, null))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    if (_isStaffMode)
                        await SyncStaffRoomAfterContractAsync(frm);
                    else
                        await SyncAdminRoomAfterContractAsync(frm);
                    await LoadDataAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task SyncAdminRoomAfterContractAsync(FrmContractEditor frm)
        {
            if (frm == null) return;

            int roomId = frm.RoomId;
            int tenantId = frm.TenantId;
            if (roomId <= 0 || tenantId <= 0) return;

            try
            {
                await EnsureTenantHistoryAdminAsync(tenantId, roomId, frm.StartDate);
                await EnsureRoomOccupiedStatusAdminAsync(roomId);

                DataSyncManager.NotifyRoomsChanged();
                DataSyncManager.NotifyTenantsChanged();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "SyncAdminRoomAfterContract", "Không thể cập nhật trạng thái phòng và khách thuê");
            }
        }

        private async System.Threading.Tasks.Task EnsureTenantHistoryAdminAsync(int tenantId, int roomId, DateTime checkInDate)
        {
            var history = await _bll.GetTenantHistoryAsync();
            if (history == null || !history.Columns.Contains("RoomId"))
            {
                await _bll.AddTenantHistoryAsync(tenantId, roomId, checkInDate, null, "Active", "Tạo từ hợp đồng");
                return;
            }

            var activeRows = history.AsEnumerable()
                .Where(r => TryGetInt(r, "RoomId") == roomId)
                .Where(IsHistoryActive)
                .ToList();

            bool hasActiveForTenant = activeRows.Any(r => TryGetInt(r, "TenantId") == tenantId);

            foreach (var row in activeRows)
            {
                int existingTenantId = TryGetInt(row, "TenantId");
                if (existingTenantId == tenantId) continue;

                int historyId = TryGetInt(row, "HistoryId");
                if (historyId <= 0) continue;

                DateTime existingCheckIn = TryGetDate(row, "CheckInDate") ?? checkInDate;
                DateTime checkOut = checkInDate < existingCheckIn ? existingCheckIn : checkInDate;
                string notes = row.Table.Columns.Contains("Notes") ? row["Notes"]?.ToString() : null;

                await _bll.UpdateTenantHistoryAsync(historyId, roomId, existingCheckIn, checkOut, "Inactive", notes);
            }

            if (!hasActiveForTenant)
            {
                await _bll.AddTenantHistoryAsync(tenantId, roomId, checkInDate, null, "Active", "Tạo từ hợp đồng");
            }
        }

        private async System.Threading.Tasks.Task EnsureRoomOccupiedStatusAdminAsync(int roomId)
        {
            var statuses = await _bll.GetRoomStatusesAsync();
            if (statuses == null || !statuses.Columns.Contains("StatusId")) return;

            int occupiedStatusId = 0;
            foreach (DataRow row in statuses.Rows)
            {
                var name = row.Table.Columns.Contains("StatusName") ? row["StatusName"]?.ToString() : null;
                var canonical = RoomStatusCatalog.Canonicalize(name);
                var key = NormalizeStatusKey(name);
                if (string.Equals(canonical, RoomStatusCatalog.TrangThaiDangO, StringComparison.OrdinalIgnoreCase)
                    || key.Contains("dang o")
                    || key.Contains("dang thue")
                    || key.Contains("occupied")
                    || key.Contains("rent")
                    || key.Contains("in use")
                    || key.Contains("inuse"))
                {
                    occupiedStatusId = TryGetInt(row, "StatusId");
                    break;
                }
            }

            if (occupiedStatusId > 0)
                await _bll.UpdateRoomOccupancyStatusAsync(roomId, occupiedStatusId);
        }

        private async System.Threading.Tasks.Task SyncStaffRoomAfterContractAsync(FrmContractEditor frm)
        {
            if (frm == null) return;

            int roomId = frm.RoomId;
            int tenantId = frm.TenantId;
            if (roomId <= 0 || tenantId <= 0) return;

            try
            {
                await EnsureTenantHistoryAsync(tenantId, roomId, frm.StartDate);
                await EnsureRoomOccupiedStatusAsync(roomId);

                DataSyncManager.NotifyRoomsChanged();
                DataSyncManager.NotifyTenantsChanged();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "SyncStaffRoomAfterContract", "Không thể cập nhật trạng thái phòng và khách thuê");
            }
        }

        private async System.Threading.Tasks.Task EnsureTenantHistoryAsync(int tenantId, int roomId, DateTime checkInDate)
        {
            var history = await _staffBll.GetTenantHistoryAsync();
            if (history == null || !history.Columns.Contains("RoomId"))
            {
                await _staffBll.AddTenantRoomHistoryAsync(tenantId, roomId, checkInDate, null, "Active", "Tạo từ hợp đồng");
                return;
            }

            var activeRows = history.AsEnumerable()
                .Where(r => TryGetInt(r, "RoomId") == roomId)
                .Where(IsHistoryActive)
                .ToList();

            bool hasActiveForTenant = activeRows.Any(r => TryGetInt(r, "TenantId") == tenantId);

            foreach (var row in activeRows)
            {
                int existingTenantId = TryGetInt(row, "TenantId");
                if (existingTenantId == tenantId) continue;

                int historyId = TryGetInt(row, "HistoryId");
                if (historyId <= 0) continue;

                DateTime existingCheckIn = TryGetDate(row, "CheckInDate") ?? checkInDate;
                DateTime checkOut = checkInDate < existingCheckIn ? existingCheckIn : checkInDate;
                string notes = row.Table.Columns.Contains("Notes") ? row["Notes"]?.ToString() : null;

                await _staffBll.UpdateTenantRoomHistoryAsync(historyId, roomId, existingCheckIn, checkOut, "Inactive", notes);
            }

            if (!hasActiveForTenant)
            {
                await _staffBll.AddTenantRoomHistoryAsync(tenantId, roomId, checkInDate, null, "Active", "Tạo từ hợp đồng");
            }
        }

        private async System.Threading.Tasks.Task EnsureRoomOccupiedStatusAsync(int roomId)
        {
            var statuses = await _staffBll.GetRoomStatusesAsync();
            if (statuses == null || !statuses.Columns.Contains("StatusId")) return;

            int occupiedStatusId = 0;
            foreach (DataRow row in statuses.Rows)
            {
                var name = row.Table.Columns.Contains("StatusName") ? row["StatusName"]?.ToString() : null;
                var canonical = RoomStatusCatalog.Canonicalize(name);
                var key = NormalizeStatusKey(name);
                if (string.Equals(canonical, RoomStatusCatalog.TrangThaiDangO, StringComparison.OrdinalIgnoreCase)
                    || key.Contains("dang o")
                    || key.Contains("dang thue")
                    || key.Contains("occupied")
                    || key.Contains("rent")
                    || key.Contains("in use")
                    || key.Contains("inuse"))
                {
                    occupiedStatusId = TryGetInt(row, "StatusId");
                    break;
                }
            }

            if (occupiedStatusId > 0)
                await _staffBll.UpdateRoomStatusAsync(roomId, occupiedStatusId);
        }

        private static bool IsHistoryActive(DataRow history)
        {
            if (history == null || history.Table == null) return false;

            var checkout = history.Table.Columns.Contains("CheckOutDate") ? history["CheckOutDate"] : null;
            bool hasCheckout = checkout != null && checkout != DBNull.Value;
            if (!hasCheckout) return true;

            if (history.Table.Columns.Contains("Status"))
            {
                var statusText = history["Status"]?.ToString() ?? string.Empty;
                return statusText.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0
                    || statusText.IndexOf("đang", StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
        }

        private static int TryGetInt(DataRow row, string column)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column)) return 0;
            return int.TryParse(row[column]?.ToString(), out var value) ? value : 0;
        }

        private static DateTime? TryGetDate(DataRow row, string column)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column)) return null;
            return DateTime.TryParse(row[column]?.ToString(), out var value) ? value : (DateTime?)null;
        }

        private static string NormalizeStatusKey(string statusName)
        {
            var fixedName = TextFixer.ForceFixUtf8Mojibake(statusName ?? string.Empty) ?? string.Empty;
            return RemoveDiacritics(fixedName).ToLowerInvariant();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        private async System.Threading.Tasks.Task DeleteSelectedAsync()
        {
            if (_selectedItem == null)
            {
                ToastNotification.Warning("Vui lòng chọn một hợp đồng để xóa");
                return;
            }

            var contractNumber = _selectedItem.Value.Row["ContractNumber"]?.ToString();
            if (!ModernConfirmDialog.ConfirmDanger($"Xóa hợp đồng '{contractNumber}'?\nHành động này không thể hoàn tác.")) return;

            try
            {
                int contractId = Convert.ToInt32(_selectedItem.Value.Row["ContractId"]);
                await _bll.DeleteContractAsync(contractId);
                ToastNotification.Success("Xóa hợp đồng thành công");
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "DeleteContract", "Không thể xóa hợp đồng");
            }
        }

        private void SelectCard(Panel card, DataRow row)
        {
            // Bỏ chọn thẻ cũ (nếu có)
            if (_selectedItem.HasValue)
            {
                _selectedItem.Value.Card.BackColor = Color.White;
            }
            // Chọn thẻ mới
            card.BackColor = Color.AliceBlue; // Tô màu nền để thể hiện thẻ được chọn
            _selectedItem = (card, row);
        }

        // --- Các hàm hỗ trợ cũ giữ nguyên ---
        private async System.Threading.Tasks.Task EnrichContractsAsync(DataTable contracts)
        {
            if (contracts == null) return;
            if (!contracts.Columns.Contains("TenantName")) contracts.Columns.Add("TenantName");
            if (!contracts.Columns.Contains("RoomNumber")) contracts.Columns.Add("RoomNumber");
            if (!contracts.Columns.Contains("BranchName")) contracts.Columns.Add("BranchName");
            try
            {
                var tenants = await _bll.GetTenantsAsync();
                var rooms = await _bll.GetRoomsAsync();
                var branches = await _bll.GetBranchesAsync();
                
                var tenantMap = new Dictionary<int, string>();
                foreach (DataRow r in tenants.Rows) if (int.TryParse(r["TenantId"]?.ToString(), out int id)) tenantMap[id] = r["FullName"]?.ToString();

                var phoneMap = new Dictionary<int, string>();
                bool hasPhoneNumber = tenants.Columns.Contains("PhoneNumber");
                bool hasPhone = tenants.Columns.Contains("Phone");
                foreach (DataRow r in tenants.Rows)
                {
                    if (!int.TryParse(r["TenantId"]?.ToString(), out int id)) continue;
                    string phone = null;
                    if (hasPhoneNumber) phone = r["PhoneNumber"]?.ToString();
                    else if (hasPhone) phone = r["Phone"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(phone)) phoneMap[id] = phone;
                }

                var roomMap = new Dictionary<int, string>();
                foreach (DataRow r in rooms.Rows) if (int.TryParse(r["RoomId"]?.ToString(), out int id)) roomMap[id] = r["RoomNumber"]?.ToString();

                var branchMap = new Dictionary<int, string>();
                foreach (DataRow r in branches.Rows) if (int.TryParse(r["BranchId"]?.ToString(), out int id)) branchMap[id] = r["BranchName"]?.ToString();

                if (!contracts.Columns.Contains("TenantName")) contracts.Columns.Add("TenantName");
                if (!contracts.Columns.Contains("TenantPhone")) contracts.Columns.Add("TenantPhone");
                if (!contracts.Columns.Contains("RoomNumber")) contracts.Columns.Add("RoomNumber");
                if (!contracts.Columns.Contains("BranchName")) contracts.Columns.Add("BranchName");

                foreach (DataRow r in contracts.Rows)
                {
                    if (int.TryParse(r["TenantId"]?.ToString(), out int tid) && tenantMap.TryGetValue(tid, out var tname)) r["TenantName"] = tname;
                    if (int.TryParse(r["TenantId"]?.ToString(), out int tpid) && phoneMap.TryGetValue(tpid, out var phone)) r["TenantPhone"] = phone;
                    if (int.TryParse(r["RoomId"]?.ToString(), out int rid) && roomMap.TryGetValue(rid, out var rnum)) r["RoomNumber"] = rnum;
                    if (int.TryParse(r["BranchId"]?.ToString(), out int bid) && branchMap.TryGetValue(bid, out var bname)) r["BranchName"] = bname;
                }
            }
            catch { }
        }
        private async System.Threading.Tasks.Task<DataTable> FilterContractsByRentedRoomsAsync(DataTable contracts)
        {
            if (contracts == null) return contracts;

            var roomIds = new HashSet<int>();
            foreach (DataRow row in contracts.Rows)
            {
                if (int.TryParse(row["RoomId"]?.ToString(), out var rid))
                    roomIds.Add(rid);
            }

            if (roomIds.Count == 0) return contracts;

            DataTable rooms = null;
            try
            {
                rooms = await _bll.GetRoomsAsync();
            }
            catch
            {
                return contracts;
            }

            if (rooms == null || rooms.Rows.Count == 0) return contracts;

            var rentedRoomIds = new HashSet<int>();
            foreach (DataRow row in rooms.Rows)
            {
                if (!int.TryParse(row["RoomId"]?.ToString(), out var rid)) continue;
                if (!roomIds.Contains(rid)) continue;

                string status = row.Table.Columns.Contains("StatusName")
                    ? row["StatusName"]?.ToString()
                    : row.Table.Columns.Contains("Status") ? row["Status"]?.ToString() : null;

                if (IsRentedRoomStatus(status))
                    rentedRoomIds.Add(rid);
            }

            return SelectLatestContractsByRoom(contracts, rentedRoomIds);
        }

        private static DataTable SelectLatestContractsByRoom(DataTable contracts, HashSet<int> rentedRoomIds)
        {
            var result = contracts.Clone();
            if (rentedRoomIds == null || rentedRoomIds.Count == 0) return result;

            var latestByRoom = new Dictionary<int, DataRow>();
            foreach (DataRow row in contracts.Rows)
            {
                if (!int.TryParse(row["RoomId"]?.ToString(), out var rid)) continue;
                if (!rentedRoomIds.Contains(rid)) continue;

                if (!latestByRoom.TryGetValue(rid, out var existing) || CompareContractRow(row, existing) > 0)
                    latestByRoom[rid] = row;
            }

            foreach (var row in latestByRoom.Values)
                result.ImportRow(row);

            return result;
        }

        private static int CompareContractRow(DataRow left, DataRow right)
        {
            var leftDate = GetSortDate(left);
            var rightDate = GetSortDate(right);
            int cmp = leftDate.CompareTo(rightDate);
            if (cmp != 0) return cmp;
            return GetSortId(left).CompareTo(GetSortId(right));
        }

        private static DateTime GetSortDate(DataRow row)
        {
            if (TryReadDate(row, "StartDate", out var dt)) return dt;
            if (TryReadDate(row, "SignDate", out dt)) return dt;
            if (TryReadDate(row, "CreatedDate", out dt)) return dt;
            return DateTime.MinValue;
        }

        private static int GetSortId(DataRow row)
        {
            return int.TryParse(row["ContractId"]?.ToString(), out var id) ? id : 0;
        }

        private static bool TryReadDate(DataRow row, string col, out DateTime value)
        {
            value = DateTime.MinValue;
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return false;
            return DateTime.TryParse(row[col]?.ToString(), out value);
        }

        private static bool IsRentedRoomStatus(string statusName)
        {
            var key = NormalizeStatusKey(statusName);
            return key.Contains("dang o") || key.Contains("dang thue") || key.Contains("da thue");
        }

        private static string NormalizeStatusKey(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName)) return string.Empty;
            var fixedName = TextFixer.FixUtf8Mojibake(statusName) ?? statusName;
            var normalized = fixedName.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (char ch in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        private static string SafeReadString(DataRow row, string col)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(col)) return null;
            var value = row[col];
            return value == null || value == DBNull.Value ? null : value.ToString();
        }
        private async System.Threading.Tasks.Task EnsureAllowedBranchScopeAsync()
        {
            if (!_branchId.HasValue && (_allowedBranchIds == null || _allowedBranchIds.Count == 0))
            {
                try { var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync()); _allowedBranchIds = AdminBranchScope.GetAllowedBranchIds(branches); } catch { _allowedBranchIds = new HashSet<int>(); }
            }
        }
        private static DataTable FilterByBranch(DataTable dt, int? branchId)
        {
            if (dt == null || !branchId.HasValue) return dt;
            var clone = dt.Clone();
            foreach (DataRow r in dt.Rows) if (r["BranchId"] != DBNull.Value && Convert.ToInt32(r["BranchId"]) == branchId.Value) clone.ImportRow(r);
            return clone;
        }
        private static bool Contains(DataRow row, string col, string key) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value && row[col].ToString().ToLower().Contains(key);
    }

    // ================================================================
    // PHẦN 2: FORM CHI TIẾT HỢP ĐỒNG (DẠNG TAB - GIỐNG ẢNH BẠN GỬI)
    // ================================================================
    public class FrmContractDetail : Form
    {
        private DataRow _row;
        private AdminDataBLL _bll;
        private Button _btnEdit, _btnClose;

        public FrmContractDetail(DataRow row, AdminDataBLL bll)
        {
            _row = row;
            _bll = bll;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Cấu hình Form chính
            Text = "Chi tiết hợp đồng";
            Width = 1000;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            // 1. Header (Thông tin tóm tắt ở trên cùng)
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(20) };
            
            var lblTitle = new Label 
            { 
                Text = _row["ContractNumber"]?.ToString() + " - " + _row["TenantName"]?.ToString(),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var lblSub = new Label 
            { 
                Text = $"Phòng: {_row["RoomNumber"]} | Chi nhánh: {_row["BranchName"]}",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(20, 55)
            };

            // --- Sửa lỗi các nút bị chồng lên nhau ---
            _btnEdit = UiKit.MakeButton("Sửa Hợp đồng", UiKit.Warning, (s, e) => EditContract(), 130);
            _btnClose = UiKit.MakeButton("Đóng", Color.FromArgb(108, 117, 125), (s, e) => Close(), 90);

            // 1. Neo các nút vào góc trên bên phải của panel header
            _btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // 2. Đặt vị trí của chúng dựa trên kích thước của panel, tính từ phải qua trái
            int paddingRight = 20;
            int buttonSpacing = 10;
            _btnClose.Location = new Point(pnlHeader.ClientSize.Width - _btnClose.Width - paddingRight, 30);
            _btnEdit.Location = new Point(_btnClose.Left - _btnEdit.Width - buttonSpacing, 30);

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub, _btnEdit, _btnClose });
            pnlHeader.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.LightGray });

            // 2. Tab Control (Tổng quan, Dịch vụ, Thanh toán...)
            var tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            
            // Tab 1: Tổng quan
            var tabOverview = new TabPage("Tổng quan") { BackColor = Color.White, Padding = new Padding(20) };
            tabOverview.Controls.Add(CreateLabel("Ngày bắt đầu:", Convert.ToDateTime(_row["StartDate"]).ToString("dd/MM/yyyy"), 20, 20));
            tabOverview.Controls.Add(CreateLabel("Ngày kết thúc:", Convert.ToDateTime(_row["EndDate"]).ToString("dd/MM/yyyy"), 20, 60));
            tabOverview.Controls.Add(CreateLabel("Giá thuê:", string.Format("{0:N0} đ", _row["RentalPrice"]), 20, 100));
            tabOverview.Controls.Add(CreateLabel("Tiền cọc:", string.Format("{0:N0} đ", _row["DepositRequired"]), 20, 140));
            tabOverview.Controls.Add(CreateLabel("Trạng thái:", ToVietnameseStatus(_row["Status"]?.ToString()), 20, 180));
            
            // Tab 2: Dịch vụ (Minh họa)
            var tabServices = new TabPage("Dịch vụ") { BackColor = Color.WhiteSmoke };
            tabServices.Controls.Add(new Label { Text = "Danh sách dịch vụ đang sử dụng...", Location = new Point(20,20), AutoSize = true });

            // Tab 3: Lịch sử thanh toán (Minh họa)
            var tabPayment = new TabPage("Thanh toán") { BackColor = Color.WhiteSmoke };
            tabPayment.Controls.Add(new Label { Text = "Lịch sử hóa đơn...", Location = new Point(20, 20), AutoSize = true });

            tabControl.TabPages.Add(tabOverview);
            tabControl.TabPages.Add(tabServices);
            tabControl.TabPages.Add(tabPayment);

            Controls.Add(tabControl);
            Controls.Add(pnlHeader);
        }

        private Control CreateLabel(string title, string value, int x, int y)
        {
            var pnl = new Panel { Location = new Point(x, y), Size = new Size(400, 30) };
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), Width = 120, ForeColor = Color.DimGray });
            pnl.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 10), Location = new Point(130, 0), AutoSize = true });
            return pnl;
        }

        private void EditContract()
        {
            // Gọi lại form sửa cũ
            using (var frm = new FrmContractEditor(_bll, _row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        public static string ToVietnameseStatus(string status)
        {
            switch (status)
            {
                case "Active": return "Đang hiệu lực";
                case "Extended": return "Gia hạn";
                case "Terminated": return "Đã chấm dứt";
                case "Expired": return "Hết hạn";
                default: return status;
            }
        }

        public static string FromVietnameseStatus(string status)
        {
            switch (status)
            {
                case "Đang hiệu lực": return "Active";
                case "Gia hạn": return "Extended";
                case "Đã chấm dứt": return "Terminated";
                case "Hết hạn": return "Expired";
                default: return status;
            }
        }
    }
}
