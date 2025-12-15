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
    /// <summary>
    /// Dashboard dành cho nhân viên với sidebar chọn module.
    /// </summary>
    public class FrmStaffDashboard : Form
    {
        private readonly string _username;
        private readonly string _fullName;
        private readonly int? _branchId;
        private readonly int _userId;

        private readonly AdminDataBLL _dataBll = new AdminDataBLL();
        private Form _currentModule;

        private Panel _sidebar;
        private Panel _header;
        private Panel _contentHost;
        private Panel _overviewPanel;

        private Label _lblHeader;
        private Label _lblUser;
        private Label _lblBranch;

        private Button _btnStaffOverview;
        private Button _btnOverview;
        private Button _btnRoom;
        private Button _btnTenant;
        private Button _btnDeposit;
        private Button _btnUtility;
        private Button _btnInvoice;
        private Button _btnPayment;
        private Button _btnMaintenance;
        private Button _btnAsset;
        private Button _btnReport;
        private Button _btnLogout;
        private Button _btnRefreshOverview;

        private Button _activeButton;
        private bool _isLoadingOverview;

        public FrmStaffDashboard(string username, string fullName, int? branchId, int userId = 0)
        {
            _username = username;
            _fullName = fullName;
            _branchId = branchId;
            _userId = userId;
            InitializeComponent();
        }

        private async Task FrmStaffDashboard_LoadAsync()
        {
            if (!await CheckStaffPermissionAsync())
            {
                MessageBox.Show("Bạn không có quyền truy cập màn Nhân viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            UpdateHeaderInfo();
            await ShowOverviewAsync();
        }

        private async Task<bool> CheckStaffPermissionAsync()
        {
            try
            {
                var userBll = new UserBLL();
                var access = await userBll.GetUserAccessAsync(_username);
                return access.RoleId == 2 || access.RoleId == 3;
            }
            catch
            {
                return false;
            }
        }

        private void InitializeComponent()
        {
            Text = "Quản lý chuỗi nhà trọ - Nhân viên";
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);

            // Sidebar
            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = Color.FromArgb(0, 95, 178),
                AutoScroll = true
            };

            var brand = new Label
            {
                Text = "Nhân viên",
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 80, 150)
            };
            _sidebar.Controls.Add(brand);

            var menu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 12, 0, 0)
            };

            _btnStaffOverview = CreateSidebarButton("Tổng quan NV");
            _btnOverview = CreateSidebarButton("Tổng quan");
            _btnRoom = CreateSidebarButton("Quản lý phòng");
            _btnTenant = CreateSidebarButton("Khách thuê");
            _btnDeposit = CreateSidebarButton("Tiền cọc");
            _btnUtility = CreateSidebarButton("Tiện ích");
            _btnInvoice = CreateSidebarButton("Hóa đơn");
            _btnPayment = CreateSidebarButton("Thanh toán");
            _btnMaintenance = CreateSidebarButton("Bảo trì");
            _btnAsset = CreateSidebarButton("Tài sản");
            _btnReport = CreateSidebarButton("Báo cáo");
            _btnLogout = CreateSidebarButton("Đăng xuất");

            menu.Controls.AddRange(new Control[]
            {
                _btnStaffOverview, _btnOverview, _btnRoom, _btnTenant, _btnDeposit, _btnUtility,
                _btnInvoice, _btnPayment, _btnMaintenance, _btnAsset, _btnReport, _btnLogout
            });

            _sidebar.Controls.Add(menu);

            // Header
            _header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(12, 12, 12, 12)
            };

            _lblHeader = new Label
            {
                Text = "Tổng quan",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.FromArgb(35, 47, 62),
                Location = new Point(12, 18)
            };

            _btnRefreshOverview = new Button
            {
                Text = "Cập nhật",
                Width = 90,
                Height = 28,
                Location = new Point(160, 21),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnRefreshOverview.FlatAppearance.BorderSize = 0;
            _btnRefreshOverview.Click += async (s, e) => await ShowOverviewAsync();

            _lblUser = new Label
            {
                Text = "Người dùng",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(600, 16)
            };

            _lblBranch = new Label
            {
                Text = "Chi nhánh",
                AutoSize = true,
                ForeColor = Color.FromArgb(90, 90, 90),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(600, 40)
            };

            _header.Controls.Add(_lblHeader);
            _header.Controls.Add(_btnRefreshOverview);
            _header.Controls.Add(_lblUser);
            _header.Controls.Add(_lblBranch);
            _header.Resize += (s, e) =>
            {
                _lblUser.Left = _header.Width - _lblUser.Width - 16;
                _lblBranch.Left = _header.Width - _lblBranch.Width - 16;
                _btnRefreshOverview.Top = _lblHeader.Top + 3;
            };

            // Content
            _overviewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(16)
            };

            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245)
            };
            _contentHost.Controls.Add(_overviewPanel);

            Controls.Add(_contentHost);
            Controls.Add(_header);
            Controls.Add(_sidebar);

            // Events
            Load += async (s, e) => await FrmStaffDashboard_LoadAsync();
            _btnStaffOverview.Click += async (s, e) => await ShowOverviewAsync();
            _btnOverview.Click += async (s, e) => await ShowOverviewAsync();
            _btnRoom.Click += (s, e) => ShowModule(new FrmRoomManager(new AdminDataBLL(), _branchId), "Quản lý phòng", _btnRoom);
            _btnTenant.Click += (s, e) => ShowModule(new FrmTenantManager(new AdminDataBLL(), _branchId), "Khách thuê", _btnTenant);
            _btnDeposit.Click += (s, e) => ShowModule(new FrmDepositManager(_branchId), "Tiền cọc", _btnDeposit);
            _btnUtility.Click += (s, e) => ShowModule(new FrmUtilityManager(), "Tiện ích", _btnUtility);
            _btnInvoice.Click += (s, e) => ShowModule(new FrmInvoiceManager(_branchId), "Hóa đơn", _btnInvoice);
            _btnPayment.Click += (s, e) => ShowModule(new FrmPaymentManager(new AdminDataBLL(), null, null, _branchId), "Thanh toán", _btnPayment);
            _btnMaintenance.Click += (s, e) => ShowModule(new FrmMaintenanceManager(), "Bảo trì", _btnMaintenance);
            _btnAsset.Click += (s, e) => ShowModule(new FrmAssetManager(), "Tài sản", _btnAsset);
            _btnReport.Click += (s, e) => ShowModule(new FrmReportManager(new AdminDataBLL()), "Báo cáo", _btnReport);
            _btnLogout.Click += BtnLogout_Click;
        }

        private Button CreateSidebarButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                Width = 210,
                Height = 46,
                Margin = new Padding(10, 6, 10, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.FromArgb(0, 118, 221),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (s, e) =>
            {
                if (btn != _activeButton)
                    btn.BackColor = Color.FromArgb(0, 136, 255);
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn != _activeButton)
                    btn.BackColor = Color.FromArgb(0, 118, 221);
            };

            return btn;
        }

        private void UpdateHeaderInfo()
        {
            _lblUser.Text = $"Người dùng: {_fullName}";
            _lblUser.Left = _header.Width - _lblUser.Width - 16;

            _lblBranch.Text = _branchId.HasValue ? $"Chi nhánh: {_branchId.Value}" : "Tất cả chi nhánh";
            _lblBranch.Left = _header.Width - _lblBranch.Width - 16;
        }

        private async Task ShowOverviewAsync()
        {
            if (_isLoadingOverview) return;
            _isLoadingOverview = true;
            SetActiveButton(_btnOverview);
            _lblHeader.Text = "Tổng quan";
            _btnRefreshOverview.Visible = true;
            _overviewPanel.Controls.Clear();

            try
            {
                var summary = await _dataBll.GetDashboardSummaryAsync();
                var roomStats = await GetRoomStatsAsync();
                var cards = BuildOverviewCards(summary, roomStats);
                _overviewPanel.Controls.Add(cards);
            }
            catch (Exception ex)
            {
                var fallback = new Label
                {
                    Text = $"Không tải được thống kê: {ex.Message}",
                    AutoSize = true,
                    ForeColor = Color.Red,
                    Location = new Point(10, 10)
                };
                _overviewPanel.Controls.Add(fallback);
            }
            finally
            {
                _isLoadingOverview = false;
            }
        }

        private async Task<(int limited, int occupied)> GetRoomStatsAsync()
        {
            var rooms = await _dataBll.GetRoomsAsync() ?? new DataTable();
            int total = rooms.Rows.Count;
            int limited = Math.Min(total, 20);

            int occupied = 0;
            if (rooms.Columns.Contains("StatusName"))
            {
                occupied = rooms.AsEnumerable()
                    .Count(r => (r["StatusName"]?.ToString() ?? string.Empty)
                        .IndexOf("đang", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            occupied = Math.Min(occupied, limited);
            return (limited, occupied);
        }

        private Control BuildOverviewCards(DataTable summary, (int limited, int occupied) roomStats)
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            var values = summary != null && summary.Rows.Count > 0 ? summary.Rows[0] : null;

            var cards = new[]
            {
                new { Title = "Doanh thu tháng", Value = ReadDecimal(values, "PaymentsThisMonth").ToString("N0"), Color = Color.FromArgb(39, 174, 96), Icon = "💵", Subtitle = (string)null, Action = (Action)(() => ShowModule(new FrmPaymentManager(new AdminDataBLL(), null, null, _branchId), "Thanh toán", _btnPayment)) },
                new { Title = "Công nợ tháng", Value = ReadDecimal(values, "OutstandingAmount").ToString("N0"), Color = Color.FromArgb(231, 76, 60), Icon = "📉", Subtitle = (string)null, Action = (Action)(() => ShowModule(new FrmInvoiceManager(_branchId), "Hóa đơn", _btnInvoice)) },
                new { Title = "Tổng phòng", Value = $"{roomStats.limited}/20", Color = Color.FromArgb(52, 168, 219), Icon = "🏠", Subtitle = $"Đã thuê: {roomStats.occupied}", Action = (Action)(() => ShowModule(new FrmRoomManager(new AdminDataBLL(), _branchId), "Quản lý phòng", _btnRoom)) },
                new { Title = "Bảo trì mở", Value = ReadInt(values, "OpenMaintenance").ToString("N0"), Color = Color.FromArgb(52, 152, 219), Icon = "🛠", Subtitle = (string)null, Action = (Action)(() => ShowModule(new FrmMaintenanceManager(), "Bảo trì", _btnMaintenance)) },
                new { Title = "Khách thuê", Value = ReadInt(values, "TotalTenants").ToString("N0"), Color = Color.FromArgb(46, 204, 113), Icon = "👥", Subtitle = (string)null, Action = (Action)(() => ShowModule(new FrmTenantManager(new AdminDataBLL(), _branchId), "Khách thuê", _btnTenant)) },
                new { Title = "Hợp đồng", Value = ReadInt(values, "TotalContracts").ToString("N0"), Color = Color.FromArgb(155, 89, 182), Icon = "📜", Subtitle = (string)null, Action = (Action)(() => ShowModule(new FrmContractManager(_branchId), "Hợp đồng", _btnReport)) },
                new { Title = "Hóa đơn", Value = ReadInt(values, "TotalInvoices").ToString("N0"), Color = Color.FromArgb(230, 126, 34), Icon = "🧾", Subtitle = (string)null, Action = (Action)(() => ShowModule(new FrmInvoiceManager(_branchId), "Hóa đơn", _btnInvoice)) },
                new { Title = "Tiền cọc", Value = ReadDecimal(values, "TotalDeposits").ToString("N0"), Color = Color.FromArgb(241, 196, 15), Icon = "🏦", Subtitle = (string)null, Action = (Action)(() => ShowModule(new FrmDepositManager(_branchId), "Tiền cọc", _btnDeposit)) }
            };

            foreach (var c in cards)
                panel.Controls.Add(CreateStatCard(c.Title, c.Value, c.Color, c.Icon, c.Action, c.Subtitle));

            return panel;
        }

        private Control CreateStatCard(string title, string value, Color color, string icon, Action onClick, string subtitle)
        {
            var card = new Panel
            {
                Width = 270,
                Height = 130,
                BackColor = Color.White,
                Margin = new Padding(10),
                Padding = new Padding(14),
                Cursor = onClick != null ? Cursors.Hand : Cursors.Default
            };

            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
                using (var bar = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(bar, 0, 0, card.Width, 4);
                }
            };

            var lblIcon = new Label
            {
                Text = icon ?? string.Empty,
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Regular),
                Location = new Point(4, 6)
            };

            var lblTitle = new Label
            {
                Text = title,
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(6, 32)
            };

            var lblValue = new Label
            {
                Text = value,
                AutoSize = true,
                ForeColor = color,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Location = new Point(6, 58)
            };

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                var lblSubtitle = new Label
                {
                    Text = subtitle,
                    AutoSize = true,
                    ForeColor = Color.FromArgb(100, 100, 100),
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    Location = new Point(6, 92)
                };
                card.Controls.Add(lblSubtitle);
            }

            if (onClick != null)
            {
                void AttachClick(Control ctl)
                {
                    ctl.Click += (s, e) => onClick();
                    ctl.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(248, 250, 252);
                    ctl.MouseLeave += (s, e) => card.BackColor = Color.White;
                }
                AttachClick(card);
                AttachClick(lblIcon);
                AttachClick(lblTitle);
                AttachClick(lblValue);
            }

            card.Controls.Add(lblIcon);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            return card;
        }

        private void ShowModule(Form module, string title, Button button)
        {
            SetActiveButton(button);
            _lblHeader.Text = title;
            _btnRefreshOverview.Visible = false;

            _currentModule?.Close();
            _overviewPanel.Controls.Clear();

            _currentModule = module;
            module.TopLevel = false;
            module.FormBorderStyle = FormBorderStyle.None;
            module.Dock = DockStyle.Fill;

            _overviewPanel.Controls.Add(module);
            module.Show();
        }

        private void SetActiveButton(Button btn)
        {
            if (_activeButton != null)
            {
                _activeButton.BackColor = Color.FromArgb(0, 118, 221);
                _activeButton.ForeColor = Color.White;
            }

            _activeButton = btn;
            if (btn != null)
            {
                btn.BackColor = Color.FromArgb(0, 95, 178);
                btn.ForeColor = Color.White;
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Đăng xuất khỏi màn hình nhân viên?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _currentModule?.Close();
                Close();
            }
        }

        private static int ReadInt(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return 0;
            var v = row[column];
            return int.TryParse(v?.ToString(), out var val) ? val : 0;
        }

        private static decimal ReadDecimal(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return 0m;
            var v = row[column];
            return decimal.TryParse(v?.ToString(), out var val) ? val : 0m;
        }
    }
}
