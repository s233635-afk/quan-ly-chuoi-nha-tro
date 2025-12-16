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

        // Dùng FlowLayoutPanel để hiện thẻ thay vì GridView
        private FlowLayoutPanel _flowPanel;
        private TextBox _txtSearch;
        private ComboBox _cboStatus;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private Label _lblCount;

        public FrmContractManager() : this(null) { }

        public FrmContractManager(int? branchId)
        {
            _branchId = branchId;
            // Gọi hàm khởi tạo giao diện thủ công, bỏ qua Designer cũ
            InitializeComponentManual();
        }

        // Đổi tên hàm này để tránh xung đột với file Designer.cs (nếu có)
        private void InitializeComponentManual()
        {
            Text = "Quản lý Hợp đồng";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1280;
            Height = 760;
            BackColor = Color.FromArgb(245, 247, 250); // Nền xám nhạt

            // --- HEADER: Tìm kiếm và Bộ lọc ---
            var top = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(15) };
            top.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.LightGray });

            // Dòng nút bấm (Thêm, Tải lại)
            var pnlActions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent };

            // Nếu bạn dùng UiKit
            var btnAdd = UiKit.MakeButton("Thêm Mới", UiKit.Primary, async (s, e) => await AddNewAsync());
            var btnRefresh = UiKit.MakeButton("Tải lại", Color.Gray, async (s, e) => await LoadDataAsync());

            pnlActions.Controls.AddRange(new Control[] { btnAdd, btnRefresh });

            // Dòng bộ lọc
            var pnlFilters = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 35, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent };

            _txtSearch = new TextBox { Width = 220, Font = new Font("Segoe UI", 10) };
            // Giả sử UiKit.MakeSearchPanel trả về Panel
            var pnlSearch = UiKit.MakeSearchPanel(_txtSearch, 230, SearchPlaceholder, ApplyFilter);

            _cboStatus = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Active", "Terminated", "Expired" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 110, Font = new Font("Segoe UI", 10) };
            _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 110, Font = new Font("Segoe UI", 10) };
            _dtFrom.ValueChanged += (s, e) => ApplyFilter();
            _dtTo.ValueChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(10, 8, 0, 0), ForeColor = Color.DimGray };

            pnlFilters.Controls.Add(pnlSearch);
            pnlFilters.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Margin = new Padding(15, 8, 5, 0) });
            pnlFilters.Controls.Add(_cboStatus);
            pnlFilters.Controls.Add(new Label { Text = "Từ:", AutoSize = true, Margin = new Padding(15, 8, 5, 0) });
            pnlFilters.Controls.Add(_dtFrom);
            pnlFilters.Controls.Add(new Label { Text = "Đến:", AutoSize = true, Margin = new Padding(5, 8, 5, 0) });
            pnlFilters.Controls.Add(_dtTo);
            pnlFilters.Controls.Add(_lblCount);

            top.Controls.Add(pnlFilters);
            top.Controls.Add(pnlActions);

            // --- BODY: Nơi chứa các thẻ (Cards) ---
            _flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(15)
            };

            Controls.Add(_flowPanel);
            Controls.Add(top);

            Load += async (s, e) => await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _flowPanel.Controls.Clear();
                var loading = new Label { Text = "Đang tải dữ liệu...", AutoSize = true, Font = new Font("Segoe UI", 12), ForeColor = Color.Gray, Padding = new Padding(20) };
                _flowPanel.Controls.Add(loading);

                await EnsureAllowedBranchScopeAsync();
                _rawTable = await _bll.GetContractsAsync();
                _rawTable = _branchId.HasValue ? FilterByBranch(_rawTable, _branchId) : AdminBranchScope.FilterByBranchIds(_rawTable, _allowedBranchIds);
                TextFixer.FixDataTable(_rawTable, "ContractNumber", "TenantName", "RoomNumber", "BranchName", "Status");
                await EnrichContractsAsync(_rawTable);

                ApplyFilter();
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

            RenderCards(resultTable);
        }

        // --- HÀM VẼ THẺ (CARD) ---
        private void RenderCards(DataTable dt)
        {
            _flowPanel.Controls.Clear();
            if (dt == null || dt.Rows.Count == 0) return;

            foreach (DataRow row in dt.Rows)
            {
                // 1. Khung thẻ
                var card = new Panel
                {
                    Width = 380,
                    Height = 170,
                    BackColor = Color.White,
                    Margin = new Padding(10),
                    Cursor = Cursors.Hand
                };

                // Màu trạng thái
                string status = row["Status"]?.ToString();
                Color statusColor = status == "Active" ? Color.SeaGreen : (status == "Expired" ? Color.Firebrick : Color.Gray);

                // Dải màu bên trái
                var strip = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = statusColor };

                // Nội dung
                var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

                var lblNo = new Label { Text = row["ContractNumber"]?.ToString(), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(0, 100, 200), AutoSize = true, Location = new Point(10, 10) };
                var lblStt = new Label { Text = status, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = statusColor, AutoSize = true, Location = new Point(280, 12) };

                var lblTenant = new Label { Text = "Khách: " + row["TenantName"]?.ToString(), Font = new Font("Segoe UI", 10), Location = new Point(10, 45), AutoSize = true };
                var lblRoom = new Label { Text = "Phòng: " + row["RoomNumber"]?.ToString(), Font = new Font("Segoe UI", 10), Location = new Point(10, 70), AutoSize = true };

                decimal price = 0; decimal.TryParse(row["RentalPrice"]?.ToString(), out price);
                var lblPrice = new Label { Text = "Giá: " + price.ToString("N0") + " đ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.DarkSlateGray, Location = new Point(200, 70), AutoSize = true };

                var lblDate = new Label { Text = $"Hạn: {Convert.ToDateTime(row["StartDate"]):dd/MM} - {Convert.ToDateTime(row["EndDate"]):dd/MM/yyyy}", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.Gray, Location = new Point(10, 110), AutoSize = true };

                var line = new Panel { Height = 1, BackColor = Color.LightGray, Width = 340, Location = new Point(10, 135) };
                var lblBr = new Label { Text = row["BranchName"]?.ToString(), Font = new Font("Segoe UI", 8), ForeColor = Color.DimGray, Location = new Point(10, 142), AutoSize = true };

                content.Controls.AddRange(new Control[] { lblNo, lblStt, lblTenant, lblRoom, lblPrice, lblDate, line, lblBr });
                card.Controls.Add(content);
                card.Controls.Add(strip);

                // Sự kiện click mở chi tiết
                card.Click += (s, e) => ShowDetail(row);
                foreach (Control c in content.Controls) c.Click += (s, e) => ShowDetail(row);

                _flowPanel.Controls.Add(card);
            }
        }

        private void ShowDetail(DataRow row)
        {
            using (var frm = new FrmContractDetail(row, _bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK) LoadDataAsync();
            }
        }

        private async System.Threading.Tasks.Task AddNewAsync()
        {
            using (var frm = new FrmContractEditor(_bll, null)) { if (frm.ShowDialog(this) == DialogResult.OK) await LoadDataAsync(); }
        }

        // --- Hàm hỗ trợ dữ liệu ---
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
    // PHẦN 2: FORM CHI TIẾT HỢP ĐỒNG (CÓ TAB - GIỐNG ẢNH BẠN GỬI)
    // ================================================================
    public class FrmContractDetail : Form
    {
        private DataRow _row;
        private AdminDataBLL _bll;

        public FrmContractDetail(DataRow row, AdminDataBLL bll)
        {
            _row = row;
            _bll = bll;
            InitializeComponentManual();
        }

        private void InitializeComponentManual()
        {
            Text = "Chi tiết hợp đồng";
            Width = 1000;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            // 1. Header
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(20) };

            var lblTitle = new Label { Text = _row["ContractNumber"]?.ToString() + " - " + _row["TenantName"]?.ToString(), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(0, 122, 204), AutoSize = true, Location = new Point(20, 20) };
            var lblSub = new Label { Text = $"Phòng: {_row["RoomNumber"]} | Chi nhánh: {_row["BranchName"]}", Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, 55) };

            var btnEdit = UiKit.MakeButton("Sửa Hợp Đồng", Color.Orange, (s, e) => EditContract());
            btnEdit.Location = new Point(800, 30);

            var btnClose = UiKit.MakeButton("Đóng", Color.Gray, (s, e) => Close());
            btnClose.Location = new Point(900, 30);

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub, btnEdit, btnClose });
            pnlHeader.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.LightGray });

            // 2. Tabs
            var tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

            // Tab Tổng quan
            var tabOverview = new TabPage("Tổng quan") { BackColor = Color.White, Padding = new Padding(20) };
            tabOverview.Controls.Add(MakeRow("Ngày bắt đầu:", Convert.ToDateTime(_row["StartDate"]).ToString("dd/MM/yyyy"), 20, 20));
            tabOverview.Controls.Add(MakeRow("Ngày kết thúc:", Convert.ToDateTime(_row["EndDate"]).ToString("dd/MM/yyyy"), 20, 60));
            tabOverview.Controls.Add(MakeRow("Giá thuê:", string.Format("{0:N0} đ", _row["RentalPrice"]), 20, 100));
            tabOverview.Controls.Add(MakeRow("Tiền cọc:", string.Format("{0:N0} đ", _row["DepositRequired"]), 20, 140));
            tabOverview.Controls.Add(MakeRow("Trạng thái:", _row["Status"]?.ToString(), 20, 180));

            // Tab Hóa đơn
            var tabBill = new TabPage("Hóa đơn") { BackColor = Color.WhiteSmoke };
            tabBill.Controls.Add(new Label { Text = "Danh sách hóa đơn của hợp đồng này...", Location = new Point(20, 20), AutoSize = true });

            // Tab Điều khoản
            var tabTerm = new TabPage("Điều khoản") { BackColor = Color.WhiteSmoke };
            tabTerm.Controls.Add(new Label { Text = _row["Terms"]?.ToString(), Location = new Point(20, 20), AutoSize = true, MaximumSize = new Size(900, 0) });

            tabControl.TabPages.Add(tabOverview);
            tabControl.TabPages.Add(tabBill);
            tabControl.TabPages.Add(tabTerm);

            Controls.Add(tabControl);
            Controls.Add(pnlHeader);
        }

        private Control MakeRow(string title, string value, int x, int y)
        {
            var pnl = new Panel { Location = new Point(x, y), Size = new Size(500, 30) };
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), Width = 150, ForeColor = Color.DimGray });
            pnl.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 10), Location = new Point(160, 0), AutoSize = true });
            return pnl;
        }

        private void EditContract()
        {
            using (var frm = new FrmContractEditor(_bll, _row)) { if (frm.ShowDialog(this) == DialogResult.OK) { DialogResult = DialogResult.OK; Close(); } }
        }
    }
}