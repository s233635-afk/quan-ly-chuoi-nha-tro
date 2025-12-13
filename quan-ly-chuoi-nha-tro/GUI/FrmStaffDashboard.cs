using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmStaffDashboard : Form
    {
        private readonly string _username;
        private readonly string _fullName;
        private readonly int? _branchId;

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private Form _currentModule;

        private Panel _sidebar;
        private Panel _header;
        private Panel _host;
        private Panel _overviewHost;
        private Label _lblHeader;
        private Label _lblUser;
        private Button _btnLogout;

        private Button _btnOverview;
        private Button _btnRoom;
        private Button _btnTenant;
        private Button _btnContract;
        private Button _btnDeposit;
        private Button _btnUtility;
        private Button _btnInvoice;
        private Button _btnPayment;
        private Button _btnMaintenance;
        private Button _btnAsset;
        private Button _btnReport;

        public FrmStaffDashboard(string username, string fullName, int? branchId)
        {
            _username = username;
            _fullName = fullName;
            _branchId = branchId;
            InitializeComponent();
        }

        private async void FrmStaffDashboard_Load(object sender, EventArgs e)
        {
            if (!await CheckStaffPermissionAsync())
            {
                MessageBox.Show("Bạn không có quyền truy cập màn Nhân viên!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            Text = $"Staff Dashboard - {_username}";
            WindowState = FormWindowState.Maximized;
            _lblUser.Text = $"👤 {_fullName} ({_username})" + (_branchId.HasValue ? $" | Chi nhánh: {_branchId.Value}" : "");
            ShowOverview();
        }

        private async Task<bool> CheckStaffPermissionAsync()
        {
            try
            {
                var bll = new UserBLL();
                var access = await bll.GetUserAccessAsync(_username);
                return access.RoleId == 2;
            }
            catch
            {
                return false;
            }
        }

        private void InitializeComponent()
        {
            BackColor = Color.FromArgb(245, 247, 250);

            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 210,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            var brand = new Label
            {
                Text = "Quản Lý Nhà Trọ",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = false,
                Height = 60,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };

            var nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 8, 10, 10),
                BackColor = Color.Transparent
            };

            _btnOverview = MakeNavButton("🏠 Tổng quan", (s, e) => ShowOverview());
            _btnRoom = MakeNavButton("🏠 Phòng", (s, e) => { SetActive(_btnRoom); LoadModule(new FrmDataViewer("Danh sách Phòng", LoadRoomsAsync), "🏠 Phòng"); });
            _btnTenant = MakeNavButton("👥 Khách thuê", (s, e) => { SetActive(_btnTenant); LoadModule(new FrmTenantManager(), "👥 Khách thuê"); });
            _btnContract = MakeNavButton("📄 Hợp đồng", (s, e) => { SetActive(_btnContract); LoadModule(new FrmContractManager(_branchId), "📄 Hợp đồng"); });
            _btnDeposit = MakeNavButton("💰 Đặt cọc", (s, e) => { SetActive(_btnDeposit); LoadModule(new FrmDepositManager(_branchId), "💰 Đặt cọc"); });
            _btnUtility = MakeNavButton("⚡ Điện/Nước/DV", (s, e) => { SetActive(_btnUtility); LoadModule(new FrmDataViewer("Điện - Nước - Dịch vụ", LoadUtilitiesAsync), "⚡ Điện/Nước/DV"); });
            _btnInvoice = MakeNavButton("🧾 Hóa đơn", (s, e) => { SetActive(_btnInvoice); LoadModule(new FrmInvoiceManager(_branchId), "🧾 Hóa đơn"); });
            _btnPayment = MakeNavButton("💳 Thanh toán", (s, e) => { SetActive(_btnPayment); LoadModule(new FrmPaymentManager(_bll, null, null, _branchId), "💳 Thanh toán"); });
            _btnMaintenance = MakeNavButton("🔧 Bảo trì", (s, e) => { SetActive(_btnMaintenance); LoadModule(new FrmDataViewer("Bảo trì & Sự cố", LoadMaintenanceAsync), "🔧 Bảo trì"); });
            _btnAsset = MakeNavButton("📦 Tài sản", (s, e) => { SetActive(_btnAsset); LoadModule(new FrmDataViewer("Tài sản phòng", LoadAssetsAsync), "📦 Tài sản"); });
            _btnReport = MakeNavButton("📊 Báo cáo", (s, e) => { SetActive(_btnReport); LoadModule(new FrmDataViewer("Báo cáo chi nhánh", LoadInvoicesAsync), "📊 Báo cáo"); });

            nav.Controls.Add(_btnOverview);
            nav.Controls.Add(_btnRoom);
            nav.Controls.Add(_btnTenant);
            nav.Controls.Add(_btnContract);
            nav.Controls.Add(_btnDeposit);
            nav.Controls.Add(_btnUtility);
            nav.Controls.Add(_btnInvoice);
            nav.Controls.Add(_btnPayment);
            nav.Controls.Add(_btnMaintenance);
            nav.Controls.Add(_btnAsset);
            nav.Controls.Add(_btnReport);

            _sidebar.Controls.Add(nav);
            _sidebar.Controls.Add(brand);

            _header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = Color.White
            };

            _lblHeader = new Label
            {
                Text = "🏠 Tổng quan",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                AutoSize = true,
                Location = new Point(18, 16)
            };

            _lblUser = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(90, 90, 90),
                Location = new Point(22, 42)
            };

            _btnLogout = new Button
            {
                Text = "Đăng xuất",
                Width = 110,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnLogout.FlatAppearance.BorderSize = 0;
            _btnLogout.Location = new Point(Width - 150, 14);
            _btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Bạn chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Close();
            };
            _header.Resize += (s, e) => _btnLogout.Location = new Point(_header.ClientSize.Width - _btnLogout.Width - 18, 14);

            _header.Controls.Add(_lblHeader);
            _header.Controls.Add(_lblUser);
            _header.Controls.Add(_btnLogout);

            _host = new Panel { Dock = DockStyle.Fill, BackColor = BackColor };
            _overviewHost = new Panel { Dock = DockStyle.Fill, BackColor = BackColor, Padding = new Padding(18) };

            Controls.Add(_host);
            Controls.Add(_header);
            Controls.Add(_sidebar);

            Load += FrmStaffDashboard_Load;
        }

        private Button MakeNavButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Width = 180,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        private void SetActive(Button active)
        {
            foreach (Control c in _sidebar.Controls)
            {
                if (!(c is FlowLayoutPanel flp)) continue;
                foreach (Control child in flp.Controls)
                {
                    if (child is Button b)
                        b.BackColor = b == active ? Color.FromArgb(0, 90, 160) : Color.FromArgb(0, 122, 204);
                }
            }
        }

        private void ClearCurrentModule()
        {
            if (_currentModule != null)
            {
                _currentModule.Close();
                _currentModule.Dispose();
                _currentModule = null;
            }
            _host.Controls.Clear();
        }

        private void LoadModule(Form module, string headerTitle)
        {
            _lblHeader.Text = headerTitle;

            ClearCurrentModule();

            module.TopLevel = false;
            module.FormBorderStyle = FormBorderStyle.None;
            module.Dock = DockStyle.Fill;
            module.StartPosition = FormStartPosition.CenterParent;
            _currentModule = module;
            _host.Controls.Add(module);
            module.Show();
        }

        private void ShowOverview()
        {
            SetActive(_btnOverview);
            _lblHeader.Text = "🏠 Tổng quan";
            ClearCurrentModule();

            _overviewHost = new Panel { Dock = DockStyle.Fill, BackColor = BackColor, Padding = new Padding(18) };
            var title = new Label
            {
                Text = "Công việc hôm nay",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            _overviewHost.Controls.Add(title);

            var grid = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 2,
                Location = new Point(0, 46),
                Width = 980,
                Height = 260,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            var loading = new Label { Text = "Đang tải thống kê...", AutoSize = true, ForeColor = Color.Gray, Location = new Point(0, 320) };
            _overviewHost.Controls.Add(grid);
            _overviewHost.Controls.Add(loading);

            _host.Controls.Add(_overviewHost);
            _ = LoadOverviewAsync(grid, loading);
        }

        private async Task LoadOverviewAsync(TableLayoutPanel grid, Label loadingLabel)
        {
            try
            {
                var rooms = await _bll.GetRoomsAsync();
                var invoices = await _bll.GetInvoicesViewAsync();
                var payments = await _bll.GetPaymentsViewAsync();
                var contracts = await _bll.GetContractsAsync();
                var maintenance = await _bll.GetMaintenanceAsync();

                rooms = FilterByBranch(rooms, _branchId);
                invoices = FilterByBranch(invoices, _branchId);
                payments = FilterByBranch(payments, _branchId);
                contracts = FilterByBranch(contracts, _branchId);
                maintenance = FilterByBranch(maintenance, _branchId);

                int totalRooms = rooms?.Rows.Count ?? 0;
                int occupied = rooms?.AsEnumerable().Count(r => r.Table.Columns.Contains("CurrentStatusId") && int.TryParse(r["CurrentStatusId"]?.ToString(), out var s) && s != 1) ?? 0;

                int overdueCount = invoices?.AsEnumerable().Count(r => string.Equals(r["Status"]?.ToString(), "Overdue", StringComparison.OrdinalIgnoreCase)) ?? 0;
                decimal debt = invoices?.AsEnumerable()
                                  .Where(r => r.Table.Columns.Contains("RemainingAmount"))
                                  .Sum(r => TryDecimal(r["RemainingAmount"])) ?? 0m;

                decimal collectedThisMonth = payments?.AsEnumerable()
                    .Where(r => r.Table.Columns.Contains("PaymentDate") && DateTime.TryParse(r["PaymentDate"]?.ToString(), out var d) && d.Year == DateTime.Today.Year && d.Month == DateTime.Today.Month)
                    .Sum(r => TryDecimal(r["PaymentAmount"])) ?? 0m;

                int endingSoon = contracts?.AsEnumerable().Count(r => r.Table.Columns.Contains("EndDate") && DateTime.TryParse(r["EndDate"]?.ToString(), out var d) && d.Date >= DateTime.Today && d.Date <= DateTime.Today.AddDays(7)) ?? 0;

                int openMaintenance = maintenance?.AsEnumerable().Count(r => !string.Equals(r["Status"]?.ToString(), "Done", StringComparison.OrdinalIgnoreCase)
                                                                            && !string.Equals(r["Status"]?.ToString(), "Hoàn tất", StringComparison.OrdinalIgnoreCase)
                                                                            && !string.Equals(r["Status"]?.ToString(), "Hoan tat", StringComparison.OrdinalIgnoreCase)) ?? 0;

                grid.Controls.Clear();
                grid.Controls.Add(MakeMetricCard("🏠 Phòng", $"{occupied:N0}/{totalRooms:N0}", "Đang sử dụng / Tổng phòng", Color.FromArgb(0, 150, 136), (s, e) => _btnRoom.PerformClick()), 0, 0);
                grid.Controls.Add(MakeMetricCard("🧾 Công nợ", $"{debt:N0}", "Tổng tiền còn nợ", Color.FromArgb(244, 67, 54), (s, e) => _btnInvoice.PerformClick()), 1, 0);
                grid.Controls.Add(MakeMetricCard("⏰ Quá hạn", $"{overdueCount:N0}", "Hóa đơn overdue", Color.FromArgb(255, 152, 0), (s, e) => _btnInvoice.PerformClick()), 2, 0);
                grid.Controls.Add(MakeMetricCard("💳 Thu tháng này", $"{collectedThisMonth:N0}", "Tổng tiền đã thu", Color.FromArgb(76, 175, 80), (s, e) => _btnPayment.PerformClick()), 0, 1);
                grid.Controls.Add(MakeMetricCard("📄 Sắp hết hạn", $"{endingSoon:N0}", "Hợp đồng trong 7 ngày", Color.FromArgb(103, 58, 183), (s, e) => _btnContract.PerformClick()), 1, 1);
                grid.Controls.Add(MakeMetricCard("🔧 Bảo trì mở", $"{openMaintenance:N0}", "Ticket chưa hoàn thành", Color.FromArgb(156, 39, 176), (s, e) => _btnMaintenance.PerformClick()), 2, 1);

                loadingLabel.Visible = false;
            }
            catch (Exception ex)
            {
                loadingLabel.Text = "Không thể tải thống kê: " + ex.Message;
                loadingLabel.Visible = true;
            }
        }

        private Panel MakeMetricCard(string title, string value, string subtitle, Color accent, EventHandler onClick)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            var bar = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accent };
            var lblTitle = new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(14, 16) };
            var lblValue = new Label { Text = value, AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = accent, Location = new Point(14, 44) };
            var lblSub = new Label { Text = subtitle, AutoSize = true, ForeColor = Color.Gray, Location = new Point(14, 86) };

            card.Controls.Add(bar);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblSub);

            card.Click += onClick;
            foreach (Control c in card.Controls) c.Click += onClick;
            return card;
        }

        private async Task<DataTable> LoadRoomsAsync()
        {
            var dt = await _bll.GetRoomsAsync();
            return FilterByBranch(dt, _branchId);
        }

        private async Task<DataTable> LoadInvoicesAsync()
        {
            var dt = await _bll.GetInvoicesViewAsync();
            return FilterByBranch(dt, _branchId);
        }

        private async Task<DataTable> LoadUtilitiesAsync()
        {
            var dt = await _bll.GetUtilitiesAsync();
            return FilterByBranch(dt, _branchId);
        }

        private async Task<DataTable> LoadMaintenanceAsync()
        {
            var dt = await _bll.GetMaintenanceAsync();
            return FilterByBranch(dt, _branchId);
        }

        private async Task<DataTable> LoadAssetsAsync()
        {
            var dt = await _bll.GetAssetsAsync();
            return FilterByBranch(dt, _branchId);
        }

        private static DataTable FilterByBranch(DataTable dt, int? branchId)
        {
            if (dt == null) return dt;
            if (!branchId.HasValue) return dt;
            if (!dt.Columns.Contains("BranchId")) return dt;

            var filtered = dt.Clone();
            foreach (DataRow r in dt.Rows)
            {
                if (int.TryParse(r["BranchId"]?.ToString(), out var b) && b == branchId.Value)
                    filtered.ImportRow(r);
            }
            return filtered;
        }

        private static decimal TryDecimal(object v)
        {
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            return decimal.TryParse(v.ToString(), out var parsed) ? parsed : 0m;
        }
    }
}
