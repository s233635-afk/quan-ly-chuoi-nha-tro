using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    // ================================================================
    // PHẦN 1: MÀN HÌNH QUẢN LÝ HỢP ĐỒNG (DẠNG THẺ - CARD VIEW)
    // ================================================================
    public class FrmContractManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo số HĐ/khách/phòng...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly int? _branchId;
        private HashSet<int> _allowedBranchIds;
        private DataTable _rawTable;

        // Thay GridView bằng FlowLayoutPanel để hiển thị dạng thẻ
        private FlowLayoutPanel _flowPanel;
        private TextBox _txtSearch;
        private ComboBox _cboStatus;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private Label _lblCount;

        // Biến để theo dõi thẻ và dữ liệu đang được chọn
        private (Panel Card, DataRow Row)? _selectedItem;

        public FrmContractManager() : this(null) { }

        public FrmContractManager(int? branchId)
        {
            _branchId = branchId;
            InitializeComponent();
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
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(15) };
            topPanel.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.LightGray }); // Đường kẻ dưới

            // 2. TableLayoutPanel để chia Header thành 2 hàng (Actions và Filters)
            var tableLayout = new TableLayoutPanel
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
            var btnDelete = UiKit.MakeButton("Xóa Hợp đồng", Color.IndianRed, async (s, e) => await DeleteSelectedAsync());
            var btnRefresh = UiKit.MakeButton("Tải lại", Color.Gray, async (s, e) => await LoadDataAsync());
            pnlActions.Controls.AddRange(new Control[] { btnAdd, btnDelete, btnRefresh });

            // 4. Panel chứa các bộ lọc (Hàng 2)
            var pnlFilters = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = Color.Transparent,
                ColumnCount = 9, // 1 search, 6 filter controls, 1 label count, 1 spacer
                RowCount = 1
            };
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F)); // Search
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // Label "Trạng thái"
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F)); // ComboBox Status
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F)); // ComboBox Status
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // Label "Từ"
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F)); // DateTimePicker From
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // Label "Đến"
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F)); // DateTimePicker To
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // Label Count
            pnlFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Cột trống để đẩy các control về bên trái
            
            // Tạo các control cho bộ lọc
            _txtSearch = new TextBox { Width = 220, Font = new Font("Segoe UI", 10) };
            var pnlSearch = UiKit.MakeSearchPanel(_txtSearch, 230, SearchPlaceholder, ApplyFilter);

            _cboStatus = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.None };
            _cboStatus = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.None };
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Active", "Extended", "Terminated", "Expired" });
            _cboStatus.SelectedIndex = 0;

            _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 110, Font = new Font("Segoe UI", 10), Anchor = AnchorStyles.None };
            _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 110, Font = new Font("Segoe UI", 10), Anchor = AnchorStyles.None };
            
            _lblCount = new Label { Anchor = AnchorStyles.None, AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(10, 0, 0, 0), ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft };

            // Gán sự kiện
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            _dtFrom.ValueChanged += (s, e) => ApplyFilter();
            _dtTo.ValueChanged += (s, e) => ApplyFilter();

            // Thêm các control vào các cột tương ứng của TableLayoutPanel
            pnlFilters.Controls.Add(pnlSearch, 0, 0);
            pnlFilters.Controls.Add(CreateFilterLabel("Trạng thái:"), 1, 0);
            pnlFilters.Controls.Add(_cboStatus, 2, 0);
            pnlFilters.Controls.Add(CreateFilterLabel("Từ:"), 3, 0);
            pnlFilters.Controls.Add(_dtFrom, 4, 0);
            pnlFilters.Controls.Add(CreateFilterLabel("Đến:"), 5, 0);
            pnlFilters.Controls.Add(_dtTo, 6, 0);
            pnlFilters.Controls.Add(_lblCount, 7, 0);

            // 5. Thêm các panel Actions và Filters vào TableLayoutPanel
            tableLayout.Controls.Add(pnlActions, 0, 0); // Thêm vào hàng 0, cột 0
            tableLayout.Controls.Add(pnlFilters, 0, 1); // Thêm vào hàng 1, cột 0
            topPanel.Controls.Add(tableLayout);

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
                Anchor = AnchorStyles.Left, // Neo vào bên trái cột
                AutoSize = true, 
                Margin = new Padding(10, 0, 3, 0), // Căn lề
                TextAlign = ContentAlignment.MiddleLeft // Căn chữ ở giữa theo chiều dọc
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
                TextFixer.FixDataTable(_rawTable, "ContractNumber", "TenantName", "RoomNumber", "BranchName", "Status");
                await EnrichContractsAsync(_rawTable);
                
                ApplyFilter(); // Hàm này sẽ gọi RenderCards
                _selectedItem = null; // Bỏ chọn khi tải lại
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void ApplyFilter()
        {
            if (_rawTable == null) return;
            string keyword = _txtSearch.Text.Trim().ToLowerInvariant();
            if (keyword == SearchPlaceholder.ToLowerInvariant()) keyword = "";
            string status = _cboStatus.SelectedItem?.ToString();
            DateTime? from = _dtFrom.Checked ? (DateTime?)_dtFrom.Value.Date : null;
            DateTime? to = _dtTo.Checked ? (DateTime?)_dtTo.Value.Date : null;

            var query = _rawTable.AsEnumerable();
            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
                query = query.Where(r => string.Equals(r["Status"]?.ToString(), status, StringComparison.OrdinalIgnoreCase));
            if (from.HasValue) query = query.Where(r => DateTime.TryParse(r["StartDate"]?.ToString(), out var d) && d.Date >= from.Value);
            if (to.HasValue) query = query.Where(r => DateTime.TryParse(r["EndDate"]?.ToString(), out var d) && d.Date <= to.Value);
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(r => Contains(r, "ContractNumber", keyword) || Contains(r, "TenantName", keyword) || Contains(r, "RoomNumber", keyword));

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
                string status = row["Status"]?.ToString();
                Color statusColor = status == "Active" ? Color.SeaGreen : (status == "Expired" ? Color.Firebrick : Color.Gray);
                
                var statusStrip = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = statusColor };
                
                // 2. Nội dung bên trong Card
                var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

                // Header: Số Hợp Đồng + Trạng thái
                var lblContractNo = new Label 
                { 
                    Text = row["ContractNumber"]?.ToString(), 
                    Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                    ForeColor = Color.FromArgb(0, 100, 200),
                    AutoSize = true, 
                    Location = new Point(10, 10) 
                };

                var lblStatus = new Label
                {
                    Text = status,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = statusColor,
                    AutoSize = true,
                    Location = new Point(280, 12)
                };

                // Body: Tên khách, Phòng, Giá
                var lblTenant = new Label 
                { 
                    Text = "Khách: " + row["TenantName"]?.ToString(), 
                    Font = new Font("Segoe UI", 10, FontStyle.Regular), 
                    Location = new Point(10, 45), 
                    AutoSize = true 
                };

                var lblRoom = new Label 
                { 
                    Text = "Phòng: " + row["RoomNumber"]?.ToString(), 
                    Font = new Font("Segoe UI", 10, FontStyle.Regular), 
                    Location = new Point(10, 70), 
                    AutoSize = true 
                };
                
                decimal price = 0;
                decimal.TryParse(row["RentalPrice"]?.ToString(), out price);
                var lblPrice = new Label 
                { 
                    Text = "Giá: " + price.ToString("N0") + " đ", 
                    Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                    ForeColor = Color.DarkSlateGray,
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
                    Text = row["BranchName"]?.ToString(),
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.DimGray,
                    Location = new Point(10, 142),
                    AutoSize = true
                };

                // Thêm controls vào card
                content.Controls.AddRange(new Control[] { lblContractNo, lblStatus, lblTenant, lblRoom, lblPrice, lblDate, line, lblBranch });
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
            using (var frm = new FrmContractEditor(_bll, null)) { if (frm.ShowDialog(this) == DialogResult.OK) await LoadDataAsync(); }
        }

        private async System.Threading.Tasks.Task DeleteSelectedAsync()
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một hợp đồng để xóa.", "Chưa chọn hợp đồng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var contractNumber = _selectedItem.Value.Row["ContractNumber"]?.ToString();
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa hợp đồng '{contractNumber}' không? Hành động này không thể hoàn tác.", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int contractId = Convert.ToInt32(_selectedItem.Value.Row["ContractId"]);
                    // Giả sử bạn có phương thức DeleteContractAsync trong BLL
                    await _bll.DeleteContractAsync(contractId); 
                    MessageBox.Show("Đã xóa hợp đồng thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync(); // Tải lại danh sách
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa hợp đồng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            try
            {
                var tenants = await _bll.GetTenantsAsync();
                var rooms = await _bll.GetRoomsAsync();
                var branches = await _bll.GetBranchesAsync();
                
                var tenantMap = new Dictionary<int, string>();
                foreach (DataRow r in tenants.Rows) if (int.TryParse(r["TenantId"]?.ToString(), out int id)) tenantMap[id] = r["FullName"]?.ToString();

                var roomMap = new Dictionary<int, string>();
                foreach (DataRow r in rooms.Rows) if (int.TryParse(r["RoomId"]?.ToString(), out int id)) roomMap[id] = r["RoomNumber"]?.ToString();

                var branchMap = new Dictionary<int, string>();
                foreach (DataRow r in branches.Rows) if (int.TryParse(r["BranchId"]?.ToString(), out int id)) branchMap[id] = r["BranchName"]?.ToString();

                if (!contracts.Columns.Contains("TenantName")) contracts.Columns.Add("TenantName");
                if (!contracts.Columns.Contains("RoomNumber")) contracts.Columns.Add("RoomNumber");
                if (!contracts.Columns.Contains("BranchName")) contracts.Columns.Add("BranchName");

                foreach (DataRow r in contracts.Rows)
                {
                    if (int.TryParse(r["TenantId"]?.ToString(), out int tid) && tenantMap.TryGetValue(tid, out var tname)) r["TenantName"] = tname;
                    if (int.TryParse(r["RoomId"]?.ToString(), out int rid) && roomMap.TryGetValue(rid, out var rnum)) r["RoomNumber"] = rnum;
                    if (int.TryParse(r["BranchId"]?.ToString(), out int bid) && branchMap.TryGetValue(bid, out var bname)) r["BranchName"] = bname;
                }
            }
            catch { }
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
            var btnEdit = UiKit.MakeButton("Sửa", Color.Orange, (s, e) => EditContract());
            var btnClose = UiKit.MakeButton("Đóng", Color.Gray, (s, e) => Close());

            // 1. Neo các nút vào góc trên bên phải của panel header
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // 2. Đặt vị trí của chúng dựa trên kích thước của panel, tính từ phải qua trái
            int paddingRight = 20;
            int buttonSpacing = 10;
            btnClose.Location = new Point(pnlHeader.ClientSize.Width - btnClose.Width - paddingRight, 30);
            btnEdit.Location = new Point(btnClose.Left - btnEdit.Width - buttonSpacing, 30);

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub, btnEdit, btnClose });
            pnlHeader.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.LightGray });

            // 2. Tab Control (Tổng quan, Dịch vụ, Thanh toán...)
            var tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            
            // Tab 1: Tổng quan
            var tabOverview = new TabPage("Tổng quan") { BackColor = Color.White, Padding = new Padding(20) };
            tabOverview.Controls.Add(CreateLabel("Ngày bắt đầu:", Convert.ToDateTime(_row["StartDate"]).ToString("dd/MM/yyyy"), 20, 20));
            tabOverview.Controls.Add(CreateLabel("Ngày kết thúc:", Convert.ToDateTime(_row["EndDate"]).ToString("dd/MM/yyyy"), 20, 60));
            tabOverview.Controls.Add(CreateLabel("Giá thuê:", string.Format("{0:N0} đ", _row["RentalPrice"]), 20, 100));
            tabOverview.Controls.Add(CreateLabel("Tiền cọc:", string.Format("{0:N0} đ", _row["DepositRequired"]), 20, 140));
            tabOverview.Controls.Add(CreateLabel("Trạng thái:", _row["Status"]?.ToString(), 20, 180));
            
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
    }
}