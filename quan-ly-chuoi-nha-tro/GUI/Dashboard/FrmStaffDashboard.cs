using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmStaffDashboard : Form
    {
        private readonly string _username;
        private readonly string _fullName;
        private readonly int? _branchId;
        private readonly int? _userId;

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private Form _currentModule;

        private Panel _sidebar;
        private Panel _header;
        private Panel _host;
        private Panel _overviewHost;
        private Label _lblHeader;
        private Label _lblUser;
        private Button _btnLogout;

        // Các button menu
        private Button _btnOverview;
        private Button _btnRoom;
        private Button _btnContract;
        private Button _btnDeposit;
        private Button _btnUtility;
        private Button _btnInvoicePayment;
        private Button _btnMaintenance;
        private Button _btnAsset;
        private Button _btnReport;
        private Button _btnStatus;
        private NotificationBell _notificationBell;

        // Room detail caches for staff view
        private FrmDataViewer _roomViewer;
        private DataTable _roomsCache;
        private DataTable _tenantHistoryCache;
        private DataTable _tenantsCache;
        private DataTable _contractsCache;

        public FrmStaffDashboard(string username, string fullName, int? branchId, int? userId = null)
        {
            _username = username;
            _fullName = fullName;
            _branchId = branchId;
            _userId = userId;
            InitializeComponent();
        }

        private async void FrmStaffDashboard_Load(object sender, EventArgs e)
        {
            if (!await CheckStaffPermissionAsync())
            {
                ToastNotification.Error("Bạn không có quyền truy cập màn Nhân viên!");
                Close();
                return;
            }

            Text = $"Staff Dashboard - {_username}";
            WindowState = FormWindowState.Maximized;
            _lblUser.Text = $"{_fullName}";
            AdminEvents.SetActor(string.IsNullOrWhiteSpace(_fullName) ? _username : _fullName, "Staff", _userId);
            AdminEvents.SetActivityScope("Tổng quan");

            // Initialize NotificationBell
            InitializeNotificationBell();

            ShowOverview();

            // Show unread notification toast
            if (_notificationBell != null && _notificationBell.UnreadCount > 0)
            {
                ToastNotification.Info($"Bạn có {_notificationBell.UnreadCount} thông báo chưa đọc");
            }
        }

        private void InitializeNotificationBell()
        {
            _notificationBell = new NotificationBell(_branchId);
            _notificationBell.Margin = new Padding(0, 12, 15, 0); // Margin để căn giữa và tạo khoảng cách
            _notificationBell.BellClicked += (s, e) =>
            {
                SetActive(_btnStatus);
                LoadModule(new FrmNotificationManager(_branchId, true), "Thông báo");
            };

            // Find rightPanel and add notification bell at the beginning
            foreach (Control ctrl in _header.Controls)
            {
                if (ctrl is FlowLayoutPanel flowPanel && flowPanel.Dock == DockStyle.Right)
                {
                    flowPanel.Controls.Add(_notificationBell);
                    flowPanel.Controls.SetChildIndex(_notificationBell, 0); // Move to first position (leftmost)
                    break;
                }
            }
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
            BackColor = Color.FromArgb(245, 247, 250); // Light Gray Background

            // --- SIDEBAR ---
            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = ModernTheme.Colors.SidebarBg
            };

            var brand = new Label
            {
                Text = "QUẢN LÝ NHÀ TRỌ",
                ForeColor = ModernTheme.Colors.Primary,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = false,
                Height = 70,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            var nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 15, 10, 10),
                BackColor = Color.Transparent
            };

            _btnOverview = MakeNavButton("🏠  Tổng quan", (s, e) => ShowOverview());
            _btnRoom = MakeNavButton("�️  Phòng", (s, e) =>
            {
                SetActive(_btnRoom);
                LoadModule(new FrmRoomManager(_bll, _branchId, true), "Phòng");
            });
<<<<<<< HEAD:quan-ly-chuoi-nha-tro/GUI/FrmStaffDashboard.cs
            _btnContract = MakeNavButton("📄 Hợp đồng", (s, e) => { SetActive(_btnContract); LoadModule(new FrmContractManager(_branchId), "📄 Hợp đồng"); });
            _btnDeposit = MakeNavButton("💰 Đặt cọc", (s, e) => { SetActive(_btnDeposit); LoadModule(new FrmDepositManager(_branchId), "💰 Đặt cọc"); });
            _btnUtility = MakeNavButton("⚡ Điện/Nước/DV", (s, e) => { SetActive(_btnUtility); LoadModule(new FrmDataViewer("Điện - Nước - Dịch vụ", LoadUtilitiesAsync), "⚡ Điện/Nước/DV"); });
            _btnInvoicePayment = MakeNavButton("💳 Hóa đơn & Thanh toán", (s, e) => { SetActive(_btnInvoicePayment); LoadModule(new FrmInvoicePaymentUnified(_branchId), "💳 Hóa đơn & Thanh toán"); });
            _btnMaintenance = MakeNavButton("🔧 Bảo trì", (s, e) => { SetActive(_btnMaintenance); LoadModule(new FrmDataViewer("Bảo trì & Sự cố", LoadMaintenanceAsync), "🔧 Bảo trì"); });
            _btnAsset = MakeNavButton("📦 Tài sản", (s, e) => { SetActive(_btnAsset); LoadModule(new FrmDataViewer("Tài sản phòng", LoadAssetsAsync), "📦 Tài sản"); });
            _btnReport = MakeNavButton("📈 Báo cáo", (s, e) => { SetActive(_btnReport); LoadModule(new FrmDataViewer("Báo cáo chi nhánh", LoadInvoicesAsync), "📈 Báo cáo"); });
            _btnStatus = MakeNavButton("🔔 Thông báo", (s, e) => { SetActive(_btnStatus); LoadModule(new FrmDataViewer("Thông báo & Sự kiện", LoadMaintenanceAsync), "🔔 Thông báo"); });
=======
            _btnContract = MakeNavButton("📄  Hợp đồng", (s, e) => { SetActive(_btnContract); LoadModule(new FrmContractManager(_branchId, true), "Hợp đồng"); });
            _btnDeposit = MakeNavButton("💰  Đặt cọc", (s, e) => { SetActive(_btnDeposit); LoadModule(new FrmDepositManager(_branchId, true), "Đặt cọc"); });
            _btnUtility = MakeNavButton("⚡  Điện/Nước/DV", (s, e) => { SetActive(_btnUtility); LoadModule(new FrmUtilityManager(_branchId, true), "Điện/Nước/DV"); });
            _btnInvoicePayment = MakeNavButton("💳  Hóa đơn & TT", (s, e) => { SetActive(_btnInvoicePayment); LoadModule(new FrmInvoicePaymentUnified(_branchId, false, true, _userId), "Hóa đơn & Thanh toán"); });
            _btnMaintenance = MakeNavButton("🔧  Bảo trì", (s, e) => { SetActive(_btnMaintenance); LoadModule(new FrmMaintenanceManager(_branchId, true), "Bảo trì"); });
            _btnAsset = MakeNavButton("📦  Tài sản", (s, e) => { SetActive(_btnAsset); LoadModule(new FrmAssetManager(_branchId, true), "Tài sản"); });
            _btnReport = MakeNavButton("📈  Báo cáo", (s, e) => { SetActive(_btnReport); LoadModule(new FrmReportManager(_branchId, true), "Báo cáo"); });
            _btnStatus = MakeNavButton("🔔  Thông báo", (s, e) => { SetActive(_btnStatus); LoadModule(new FrmNotificationManager(_branchId, true), "Thông báo"); });
>>>>>>> 12f00b2ebf1219006addf91c63b69b64eb8559ed:quan-ly-chuoi-nha-tro/GUI/Dashboard/FrmStaffDashboard.cs

            nav.Controls.Add(_btnOverview);
            nav.Controls.Add(_btnRoom);
            nav.Controls.Add(_btnContract);
            nav.Controls.Add(_btnDeposit);
            nav.Controls.Add(_btnUtility);
            nav.Controls.Add(_btnInvoicePayment);
            nav.Controls.Add(_btnMaintenance);
            nav.Controls.Add(_btnAsset);
            nav.Controls.Add(_btnReport);
            nav.Controls.Add(_btnStatus);

            _sidebar.Controls.Add(nav);
            _sidebar.Controls.Add(brand);

            // --- HEADER ---
            _header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White
            };
            _header.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(230, 230, 230)))
                    e.Graphics.DrawLine(pen, 0, _header.Height - 1, _header.Width, _header.Height - 1);
            };

            _lblHeader = new Label
            {
                Text = "Tổng quan",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            _lblUser = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.TextPrimary,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 18, 15, 0) // Top margin to center vertically with button (approx)
            };

            _btnLogout = new ModernButton
            {
                Text = "Đăng xuất",
                Width = 110,
                Height = 36,
                Parameters = new ModernButton.ButtonParameters
                {
                    BaseColor = Color.FromArgb(231, 76, 60),
                    HoverColor = ControlPaint.Dark(Color.FromArgb(231, 76, 60), 0.1f),
                    BorderRadius = 8,
                    TextFont = new Font("Segoe UI", 10f, FontStyle.Bold),
                    TextColor = Color.White
                },
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 12, 20, 0)
            };
            _btnLogout.FlatAppearance.BorderSize = 0;
            _btnLogout.Click += (s, e) =>
            {
                if (ModernConfirmDialog.Confirm("Bạn chắc chắn muốn đăng xuất?", "Xác nhận"))
                    Close();
            };

            // Container for right-aligned items
            var rightPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 400, // Initial, but AutoSize will handle it
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight, // Changed to LeftToRight for correct order
                BackColor = Color.Transparent,
                WrapContents = false
            };

            // With LeftToRight flow, add in visual order: NotificationBell, User, Logout
            // NotificationBell will be added first in InitializeNotificationBell
            rightPanel.Controls.Add(_lblUser);   // Added first (will be second visually)
            rightPanel.Controls.Add(_btnLogout); // Added second (will be third visually)

            _header.Controls.Add(rightPanel);
            _header.Controls.Add(_lblHeader);

            _host = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 247, 250) };
            _overviewHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(20) };

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
                Width = 220,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = ModernTheme.Colors.SidebarText,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 5),
                Cursor = Cursors.Hand,
                Padding = new Padding(15, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ModernTheme.Colors.SidebarHover;
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
                    {
                        if (b == active)
                        {
                            b.BackColor = ModernTheme.Colors.SidebarActive;
                            b.ForeColor = Color.White;
                            b.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                        }
                        else
                        {
                            b.BackColor = Color.Transparent;
                            b.ForeColor = ModernTheme.Colors.SidebarText;
                            b.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                        }
                    }
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
            AdminEvents.SetActivityScope(headerTitle);

            ClearCurrentModule();

            module.TopLevel = false;
            module.FormBorderStyle = FormBorderStyle.None;
            module.Dock = DockStyle.Fill;
            module.StartPosition = FormStartPosition.CenterParent;
            _currentModule = module;
            _host.Controls.Add(module);
            module.Show();

            // Ensure notification bell stays on top after module is loaded
            if (_notificationBell != null && !_notificationBell.IsDisposed)
            {
                _notificationBell.BringToFront();
            }
        }

        private void ShowOverview()
        {
            SetActive(_btnOverview);
            _lblHeader.Text = "Tổng quan";
            AdminEvents.SetActivityScope("Tổng quan");
            ClearCurrentModule();

            _overviewHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(20) };
            _overviewHost.AutoScroll = true;

            var title = new Label
            {
                Text = "Tổng quan trong ngày",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                AutoSize = true,
                Location = new Point(20, 10),
                Margin = new Padding(0, 0, 0, 20)
            };
            _overviewHost.Controls.Add(title);

            // Grid for Cards
            var cardGrid = new TableLayoutPanel
            {
                ColumnCount = 6,
                RowCount = 2, // Add extra row
                Location = new Point(10, 60),
                Width = _overviewHost.ClientSize.Width - 40,
                Height = 130,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            cardGrid.ColumnStyles.Clear();
            for (int i = 0; i < 6; i++)
                cardGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6f));

            cardGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 130f));
            cardGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // Absorb space
            _overviewHost.Controls.Add(cardGrid);

            // Ensure cardGrid resizes with parent
            _overviewHost.Resize += (s, e) =>
            {
                if (cardGrid != null && !cardGrid.IsDisposed)
                {
                    cardGrid.Width = _overviewHost.ClientSize.Width - 40;
                }
            };

            // Recent Activity Section
            var recentLabel = new Label
            {
                Text = "Hoạt động gần đây",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                AutoSize = true,
                Location = new Point(20, 260),
                Margin = new Padding(0, 20, 0, 10)
            };
            _overviewHost.Controls.Add(recentLabel);

            var recentGrid = new DataGridView
            {
                Location = new Point(20, 295),
                Width = _overviewHost.ClientSize.Width - 40,
                Height = 250,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            recentGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
            recentGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            recentGrid.ColumnHeadersHeight = 35;
            recentGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            recentGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 240, 241);
            recentGrid.DefaultCellStyle.SelectionForeColor = Color.Black;
            recentGrid.EnableHeadersVisualStyles = false;
            _overviewHost.Controls.Add(recentGrid);

            var loading = new Label { Text = "Đang tải thống kê...", AutoSize = true, ForeColor = Color.Gray, Location = new Point(20, 560) };
            _overviewHost.Controls.Add(loading);

            // Ensure recentGrid resizes with parent
            _overviewHost.Resize += (s, e) =>
            {
                if (recentGrid != null && !recentGrid.IsDisposed)
                {
                    recentGrid.Width = _overviewHost.ClientSize.Width - 40;
                }
            };

            _host.Controls.Add(_overviewHost);
            _ = LoadOverviewAsync(cardGrid, recentGrid, loading);
        }

        private async Task LoadOverviewAsync(TableLayoutPanel grid, DataGridView recentList, Label loadingLabel)
        {
            using (var loading = new SimpleLoadingOverlay(this, "Đang tải thông kê"))
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
                    grid.Controls.Add(MakeMetricCard("Phòng", $"{occupied}/{totalRooms}", "Đang sử dụng / Tổng", Color.FromArgb(26, 188, 156), (s, e) => _btnRoom.PerformClick()), 0, 0);
                    grid.Controls.Add(MakeMetricCard("Công nợ", $"{debt:N0} đ", "Tổng tiền còn nợ", Color.FromArgb(231, 76, 60), (s, e) => _btnInvoicePayment.PerformClick()), 1, 0);
                    grid.Controls.Add(MakeMetricCard("Thu tháng này", $"{collectedThisMonth:N0} đ", "Đã thu trong tháng", Color.FromArgb(46, 204, 113), (s, e) => _btnInvoicePayment.PerformClick()), 2, 0);
                    grid.Controls.Add(MakeMetricCard("Quá hạn", $"{overdueCount}", "Hóa đơn quá hạn", Color.FromArgb(243, 156, 18), (s, e) => _btnInvoicePayment.PerformClick()), 3, 0);
                    grid.Controls.Add(MakeMetricCard("Sắp hết hạn", $"{endingSoon}", "Hợp đồng (7 ngày)", Color.FromArgb(155, 89, 182), (s, e) => _btnContract.PerformClick()), 4, 0);
                    grid.Controls.Add(MakeMetricCard("Bảo trì", $"{openMaintenance}", "Yêu cầu chưa xử lý", Color.FromArgb(52, 152, 219), (s, e) => _btnMaintenance.PerformClick()), 5, 0);

                    // Populate Recent Activity (Mockup mostly using Invoice data for now as example)
                    var dtRecent = new DataTable();
                    dtRecent.Columns.Add("LOẠI", typeof(string));
                    dtRecent.Columns.Add("MÔ TẢ", typeof(string));
                    dtRecent.Columns.Add("THỜI GIAN", typeof(DateTime));
                    dtRecent.Columns.Add("TRẠNG THÁI", typeof(string));

                    if (invoices != null)
                    {
                        foreach (DataRow row in invoices.AsEnumerable().OrderByDescending(r => r["CreatedDate"]).Take(5))
                        {
                            string status = TextFixer.ToVietnameseInvoiceStatus(row["Status"]?.ToString());
                            dtRecent.Rows.Add("Hóa đơn", $"Phòng {row["RoomNumber"]} - {TryDecimal(row["TotalAmount"]):N0} đ", row["CreatedDate"], status);
                        }
                    }
                    if (maintenance != null)
                    {
                        foreach (DataRow row in maintenance.AsEnumerable().OrderByDescending(r => r["RequestDate"]).Take(5))
                        {
                            var roomNum = row.Table.Columns.Contains("RoomNumber") ? row["RoomNumber"].ToString() : "N/A";
                            string status = row["Status"]?.ToString();
                            if (string.Equals(status, "Done", StringComparison.OrdinalIgnoreCase)) status = "Hoàn tất";
                            else if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase)) status = "Đang xử lý";

                            dtRecent.Rows.Add("Bảo trì", $"Phòng {roomNum} - {row["IssueDescription"]}", row["RequestDate"], status);
                        }
                    }

                    DataView dv = dtRecent.DefaultView;
                    dv.Sort = "THỜI GIAN DESC";
                    recentList.DataSource = dv.ToTable();

                    // Handle empty state
                    if (dtRecent.Rows.Count == 0)
                    {
                        loadingLabel.Text = "Không có hoạt động gần đây";
                        loadingLabel.ForeColor = Color.FromArgb(149, 165, 166);
                    }
                    else
                    {
                        loadingLabel.Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    loadingLabel.Text = "Không thể tải thống kê: " + ex.Message;
                    loadingLabel.Visible = true;
                }
            }
        }

        private Panel MakeMetricCard(string title, string value, string subtitle, Color accent, EventHandler onClick)
        {
            // Use enhanced ModernStatCard instead of manual Panel
            var card = new ModernStatCard(title, value, subtitle, accent)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(12)
            };

            // Attach click handler
            if (onClick != null)
            {
                card.OnCardClick += onClick;
            }

            return card;
        }

        // =========================================================================================
        // ĐÃ CHỈNH SỬA: CÁC HÀM LOAD DỮ LIỆU CÓ ĐỔI TÊN CỘT SANG TIẾNG VIỆT
        // =========================================================================================

        private async Task<DataTable> LoadRoomsAsync()
        {
            var dt = await _bll.GetRoomsAsync();
            var table = FilterByBranch(dt, _branchId);
            table = LimitRoomsForStaff(table);
            AddRoomIdRawColumn(table);

            if (table != null)
            {
                // Đổi tên cột hiển thị cho Grid "Danh sách Phòng"
                if (table.Columns.Contains("RoomId")) table.Columns["RoomId"].ColumnName = "Mã Phòng";
                if (table.Columns.Contains("RoomNumber")) table.Columns["RoomNumber"].ColumnName = "Số Phòng";
                if (table.Columns.Contains("BranchId")) table.Columns["BranchId"].ColumnName = "Chi Nhánh";
                if (table.Columns.Contains("SectionId")) table.Columns["SectionId"].ColumnName = "Khu Vực";
                if (table.Columns.Contains("RoomTypeId")) table.Columns["RoomTypeId"].ColumnName = "Loại Phòng";
                if (table.Columns.Contains("RoomPrice")) table.Columns["RoomPrice"].ColumnName = "Giá Phòng";
                if (table.Columns.Contains("CurrentStatusId")) table.Columns["CurrentStatusId"].ColumnName = "Trạng Thái";
                if (table.Columns.Contains("Floor")) table.Columns["Floor"].ColumnName = "Tầng";
                if (table.Columns.Contains("Area")) table.Columns["Area"].ColumnName = "Diện Tích";
                if (table.Columns.Contains("IsActive")) table.Columns["IsActive"].ColumnName = "Hoạt Động";
                if (table.Columns.Contains("CreatedDate")) table.Columns["CreatedDate"].ColumnName = "Ngày Tạo";
                if (table.Columns.Contains("UpdatedDate")) table.Columns["UpdatedDate"].ColumnName = "Ngày Cập Nhật";
            }
            return table;
        }

        private async Task<DataTable> LoadInvoicesAsync()
        {
            var dt = await _bll.GetInvoicesViewAsync();
            var table = FilterByBranch(dt, _branchId);

            if (table != null)
            {
                // Đổi tên cột cho Grid "Báo cáo / Hóa đơn"
                if (table.Columns.Contains("InvoiceId")) table.Columns["InvoiceId"].ColumnName = "Mã HĐ";
                if (table.Columns.Contains("RoomNumber")) table.Columns["RoomNumber"].ColumnName = "Phòng";
                if (table.Columns.Contains("TotalAmount")) table.Columns["TotalAmount"].ColumnName = "Tổng Tiền";
                if (table.Columns.Contains("Status")) table.Columns["Status"].ColumnName = "Trạng Thái";
                if (table.Columns.Contains("CreatedDate")) table.Columns["CreatedDate"].ColumnName = "Ngày Lập";
                if (table.Columns.Contains("BranchId")) table.Columns["BranchId"].ColumnName = "Chi Nhánh";
            }
            return table;
        }

        private async Task<DataTable> LoadUtilitiesAsync()
        {
            var dt = await _bll.GetUtilitiesAsync();
            var table = FilterByBranch(dt, _branchId);

            if (table != null)
            {
                // Đổi tên cột cho Grid "Điện/Nước"
                if (table.Columns.Contains("RoomNumber")) table.Columns["RoomNumber"].ColumnName = "Phòng";
                if (table.Columns.Contains("ElectricityIndex")) table.Columns["ElectricityIndex"].ColumnName = "Chỉ Số Điện";
                if (table.Columns.Contains("WaterIndex")) table.Columns["WaterIndex"].ColumnName = "Chỉ Số Nước";
                if (table.Columns.Contains("RecordedDate")) table.Columns["RecordedDate"].ColumnName = "Ngày Ghi";
                if (table.Columns.Contains("BranchId")) table.Columns["BranchId"].ColumnName = "Chi Nhánh";
            }
            return table;
        }

        private async Task<DataTable> LoadMaintenanceAsync()
        {
            var dt = await _bll.GetMaintenanceAsync();
            var table = FilterByBranch(dt, _branchId);

            if (table != null)
            {
                // Đổi tên cột cho Grid "Bảo trì"
                if (table.Columns.Contains("Description")) table.Columns["Description"].ColumnName = "Mô Tả Sự Cố";
                if (table.Columns.Contains("RequestDate")) table.Columns["RequestDate"].ColumnName = "Ngày Yêu Cầu";
                if (table.Columns.Contains("Status")) table.Columns["Status"].ColumnName = "Trạng Thái";
                if (table.Columns.Contains("Cost")) table.Columns["Cost"].ColumnName = "Chi Phí";
                if (table.Columns.Contains("BranchId")) table.Columns["BranchId"].ColumnName = "Chi Nhánh";
            }
            return table;
        }

        private async Task<DataTable> LoadAssetsAsync()
        {
            var dt = await _bll.GetAssetsAsync();
            var table = FilterByBranch(dt, _branchId);

            if (table != null)
            {
                // Đổi tên cột cho Grid "Tài sản"
                if (table.Columns.Contains("AssetName")) table.Columns["AssetName"].ColumnName = "Tên Tài Sản";
                if (table.Columns.Contains("Quantity")) table.Columns["Quantity"].ColumnName = "Số Lượng";
                if (table.Columns.Contains("Status")) table.Columns["Status"].ColumnName = "Tình Trạng";
                if (table.Columns.Contains("BranchId")) table.Columns["BranchId"].ColumnName = "Chi Nhánh";
            }
            return table;
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

        private static void AddRoomIdRawColumn(DataTable table)
        {
            if (table == null) return;
            if (!table.Columns.Contains("RoomId") || table.Columns.Contains("RoomIdRaw")) return;

            table.Columns.Add("RoomIdRaw", typeof(int));
            foreach (DataRow r in table.Rows)
            {
                if (int.TryParse(r["RoomId"]?.ToString(), out var id))
                    r["RoomIdRaw"] = id;
            }
        }

        private DataTable LimitRoomsForStaff(DataTable table)
        {
            if (table == null) return table;

            var limited = table.Clone();
            var prefixes = new[] { "A", "B" };

            foreach (var prefix in prefixes)
            {
                var selection = table.AsEnumerable()
                    .Where(r => (r["RoomNumber"]?.ToString() ?? string.Empty).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r["RoomNumber"]?.ToString())
                    .Take(10);

                foreach (var row in selection)
                    limited.ImportRow(row);
            }

            return limited.Rows.Count > 0 ? limited : table;
        }

        private async Task OnRoomRowDoubleClickAsync(DataRow row)
        {
            int roomId = ExtractRoomId(row);
            if (roomId <= 0)
            {
                MessageBox.Show("Không xác định được phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await EnsureRoomCachesAsync();

            var roomRow = _roomsCache?.AsEnumerable().FirstOrDefault(r => TryReadInt(r, "RoomId") == roomId);
            if (roomRow == null)
            {
                MessageBox.Show("Không tìm thấy dữ liệu phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var activeHistory = _tenantHistoryCache?.AsEnumerable()
                .Where(r => TryReadInt(r, "RoomId") == roomId)
                .Where(IsActiveHistory)
                .OrderByDescending(r => TryReadDate(r, "CheckInDate") ?? DateTime.MinValue)
                .FirstOrDefault();

            DataRow tenantRow = null;
            if (activeHistory != null)
            {
                int tenantId = TryReadInt(activeHistory, "TenantId");
                if (tenantId > 0)
                    tenantRow = _tenantsCache?.AsEnumerable().FirstOrDefault(t => TryReadInt(t, "TenantId") == tenantId);
            }

            DataRow contractRow = null;
            if (_contractsCache != null)
            {
                var contracts = _contractsCache.AsEnumerable().Where(c => TryReadInt(c, "RoomId") == roomId);
                if (activeHistory != null)
                {
                    int tenantId = TryReadInt(activeHistory, "TenantId");
                    if (tenantId > 0) contracts = contracts.Where(c => TryReadInt(c, "TenantId") == tenantId);
                }

                contractRow = contracts
                    .OrderByDescending(c => TryReadDate(c, "StartDate") ?? DateTime.MinValue)
                    .FirstOrDefault();
            }

            using (var frm = new FrmRoomTenantQuickView(_bll, RefreshRoomsAfterEditAsync, true))
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                var tenantSummary = BuildTenantSummary(roomId);
                frm.UpdateData(roomId, roomRow, tenantRow, contractRow, tenantSummary);
                frm.ShowDialog(this);
            }
        }

        private string BuildTenantSummary(int roomId)
        {
            if (_tenantHistoryCache == null || _tenantsCache == null) return null;

            var tenantIds = _tenantHistoryCache.AsEnumerable()
                .Where(r => TryReadInt(r, "RoomId") == roomId)
                .Where(IsActiveHistory)
                .Select(r => TryReadInt(r, "TenantId"))
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (tenantIds.Count == 0) return null;

            var lines = new List<string>();
            int index = 1;
            foreach (var tenantId in tenantIds)
            {
                var tenant = _tenantsCache.AsEnumerable().FirstOrDefault(t => TryReadInt(t, "TenantId") == tenantId);
                if (tenant == null) continue;

                var name = tenant.Table.Columns.Contains("FullName") ? tenant["FullName"]?.ToString() : null;
                if (!string.IsNullOrWhiteSpace(name))
                    name = TextFixer.FixUtf8Mojibake(name) ?? name;

                var phone = tenant.Table.Columns.Contains("PhoneNumber") ? tenant["PhoneNumber"]?.ToString() : null;
                if (!string.IsNullOrWhiteSpace(phone))
                    phone = TextFixer.FixUtf8Mojibake(phone) ?? phone;

                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(phone)) continue;
                var line = string.IsNullOrWhiteSpace(phone) ? name : $"{name} - {phone}";
                lines.Add($"{index}. {line}".Trim());
                index++;
            }

            return lines.Count > 0 ? string.Join("\n", lines) : null;
        }

        private async Task EnsureRoomCachesAsync()
        {
            if (_roomsCache == null)
            {
                var rooms = FilterByBranch(await _bll.GetRoomsAsync(), _branchId);
                _roomsCache = LimitRoomsForStaff(rooms);
            }

            if (_tenantHistoryCache == null)
                _tenantHistoryCache = await _bll.GetTenantHistoryAsync();

            if (_tenantsCache == null)
                _tenantsCache = await _bll.GetTenantsAsync();

            if (_contractsCache == null)
                _contractsCache = await _bll.GetContractsAsync();
        }

        private async Task RefreshRoomsAfterEditAsync(int roomId)
        {
            _roomsCache = null;
            _tenantHistoryCache = null;
            _tenantsCache = null;
            _contractsCache = null;

            if (_roomViewer != null && !_roomViewer.IsDisposed)
                await _roomViewer.ReloadAsync();
        }

        private static bool IsActiveHistory(DataRow history)
        {
            if (history == null || history.Table == null) return false;

            var checkout = history.Table.Columns.Contains("CheckOutDate") ? history["CheckOutDate"] : null;
            bool hasCheckout = checkout != null && checkout != DBNull.Value;
            if (!hasCheckout) return true;

            if (history.Table.Columns.Contains("Status"))
            {
                var statusText = history["Status"]?.ToString() ?? string.Empty;
                return statusText.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0
                    || statusText.IndexOf("đang", StringComparison.OrdinalIgnoreCase) >= 0
                    || statusText.IndexOf("Đang", StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
        }

        private static int ExtractRoomId(DataRow row)
        {
            if (row == null || row.Table == null) return 0;
            string[] names = { "RoomIdRaw", "RoomId", "Mã Phòng" };

            foreach (var name in names)
            {
                if (row.Table.Columns.Contains(name) && int.TryParse(row[name]?.ToString(), out var value) && value > 0)
                    return value;
            }

            foreach (DataColumn col in row.Table.Columns)
            {
                if (int.TryParse(row[col]?.ToString(), out var value) && value > 0)
                    return value;
            }

            return 0;
        }

        private static int TryReadInt(DataRow row, string col)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(col)) return 0;
            return int.TryParse(row[col]?.ToString(), out var val) ? val : 0;
        }

        private static DateTime? TryReadDate(DataRow row, string col)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(col)) return null;
            return DateTime.TryParse(row[col]?.ToString(), out var dt) ? dt : (DateTime?)null;
        }

        private static decimal TryDecimal(object v)
        {
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            return decimal.TryParse(v.ToString(), out var parsed) ? parsed : 0m;
        }
    }
}
