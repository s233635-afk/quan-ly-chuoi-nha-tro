using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// FrmAdminDashboard - Form chính cho Admin
    /// Chứa 13 tính năng Admin
    /// </summary>
    public partial class FrmAdminDashboard : Form
    {
        private string currentUser;
        private int currentUserId;
        private AdminDataBLL adminDataBLL = new AdminDataBLL();
        private Form currentModule;
        private readonly string defaultPlaceholderText = "Chọn chức năng ở thanh bên hoặc nhấn \"Tổng quan\" để xem thống kê nhanh.";
        private bool _overviewDirty = true;
        private Button _activeNavButton;
        private NotificationBell _notificationBell;
        private Panel _overviewHost;
        private Panel _recentPanel;
        private DataGridView _recentGrid;
        private Label _recentEmptyLabel;
        private Button _recentRefreshButton;
        private Button _recentViewAllButton;
        private ModernSearchBox _recentSearchBox;
        private ComboBox _recentGroupCombo;
        private bool _recentGroupByDay = true;
        private string _recentKeyword;
        private DataTable _recentSource;

        public FrmAdminDashboard(string username, int userId)
        {
            InitializeComponent();
            currentUser = username;
            currentUserId = userId;
            AdminEvents.DataChanged += HandleAdminDataChanged;
            ApplyNavStyling();
            InitializeOverviewLayout();
        }

        private async void FrmAdminDashboard_Load(object sender, EventArgs e)
        {
            // Kiểm tra quyền Admin
            if (!await CheckAdminPermissionAsync())
            {
                ToastNotification.Error("Bạn không có quyền truy cập Admin!");
                this.Close();
                return;
            }

            // Chế độ Admin-only: ẩn module quản lý nhân viên & các màn Staff
            if (btnNavStaff != null)
                btnNavStaff.Visible = false;
            if (btnNavTenant != null)
            {
                btnNavTenant.Visible = false;
                btnNavTenant.Enabled = false;
                btnNavTenant.TabStop = false;
            }

            this.Text = $"Bảng điều khiển Admin - {currentUser}";
            this.WindowState = FormWindowState.Maximized;
            lblWelcome.Text = $"Xin chào Admin: {currentUser}";
            lblUser.Text = $"Admin: {currentUser}";
            lblPlaceholder.Text = defaultPlaceholderText;
            AdminEvents.SetActor(currentUser, "Admin", currentUserId);
            SetActiveNav(btnNavOverview);
            await ShowOverviewAsync();

            // Initialize NotificationBell
            InitializeNotificationBell();

            // Show unread notification toast
            if (_notificationBell != null && _notificationBell.UnreadCount > 0)
            {
                ToastNotification.Info($"Bạn có {_notificationBell.UnreadCount} thông báo chưa đọc", "Thông báo");
            }
        }

        private void InitializeNotificationBell()
        {
            _notificationBell = new NotificationBell();
            _notificationBell.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Calculate position: left of lblUser if it exists, otherwise left of btnLogout
            int xPosition;
            if (lblUser != null && lblUser.Visible)
            {
                xPosition = lblUser.Left - _notificationBell.Width - 15;
            }
            else
            {
                xPosition = btnLogout.Left - _notificationBell.Width - 15;
            }

            _notificationBell.Location = new Point(xPosition, 18);
            _notificationBell.BellClicked += (s, e) =>
            {
                SetActiveNav(btnNavNotification);
                btnNotification_Click(s, e);
            };
            pnlHeader.Controls.Add(_notificationBell);
            _notificationBell.BringToFront();

            // Update position when panel resizes
            pnlHeader.Resize += (s, e) =>
            {
                if (_notificationBell != null && !_notificationBell.IsDisposed)
                {
                    int newX;
                    if (lblUser != null && lblUser.Visible)
                    {
                        newX = lblUser.Left - _notificationBell.Width - 15;
                    }
                    else
                    {
                        newX = btnLogout.Left - _notificationBell.Width - 15;
                    }
                    _notificationBell.Location = new Point(newX, 18);
                    _notificationBell.BringToFront();
                }
            };
        }

        private void ApplyNavStyling()
        {
            StyleNavButton(btnNavOverview);
            StyleNavButton(btnNavBranch);
            StyleNavButton(btnNavRoom);
            StyleNavButton(btnNavStaff);
            StyleNavButton(btnNavContract);
            StyleNavButton(btnNavDeposit);
            StyleNavButton(btnNavUtility);
            StyleNavButton(btnNavInvoice);
            StyleNavButton(btnNavReport);
            StyleNavButton(btnNavMaintenance);
            StyleNavButton(btnNavAsset);
            StyleNavButton(btnNavNotification);
            StyleNavButton(btnNavSettings);

            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(btnLogout.BackColor);

            // Add rounded corners to logout button
            UiKit.SetRoundedRegion(btnLogout, 8);
            btnLogout.Resize += (s, e) => UiKit.SetRoundedRegion(btnLogout, 8);

            // Header styling
            pnlHeader.BackColor = Color.White;
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(230, 230, 230)))
                    e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };
            lblWelcome.ForeColor = ModernTheme.Colors.Primary;
        }

        private void StyleNavButton(Button btn)
        {
            if (btn == null) return;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ModernTheme.Colors.SidebarHover;
            btn.FlatAppearance.MouseDownBackColor = ModernTheme.Colors.SidebarActive;
            btn.ForeColor = ModernTheme.Colors.SidebarText;
            btn.Cursor = Cursors.Hand;
        }

        private void SetActiveNav(Button btn)
        {
            if (btn == null) return;

            if (_activeNavButton != null && !_activeNavButton.IsDisposed)
            {
                _activeNavButton.BackColor = Color.Transparent;
                _activeNavButton.ForeColor = ModernTheme.Colors.SidebarText;
                _activeNavButton.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }

            _activeNavButton = btn;
            _activeNavButton.BackColor = ModernTheme.Colors.SidebarActive;
            _activeNavButton.ForeColor = Color.White;
            _activeNavButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        /// <summary>
        /// Kiểm tra quyền Admin từ Database
        /// RoleId = 1: Admin
        /// RoleId = 2: Staff
        /// </summary>
        private async System.Threading.Tasks.Task<bool> CheckAdminPermissionAsync()
        {
            try
            {
                UserBLL userBLL = new UserBLL();
                int roleId = await userBLL.GetUserRoleAsync(currentUser);

                // RoleId 1 = Admin
                return roleId == 1;
            }
            catch
            {
                return false;
            }
        }

        private string BuildBulletText(params string[] items)
        {
            return "• " + string.Join("\n• ", items);
        }

        private void BuildOverviewStats(DataTable summary)
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            tableLayoutPanel1.Padding = new Padding(24);
            tableLayoutPanel1.BackColor = Color.FromArgb(248, 250, 252);
            tableLayoutPanel1.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;

            int GetInt(string col)
            {
                if (summary == null || summary.Rows.Count == 0) return 0;
                var v = summary.Rows[0][col];
                if (v == null || v == DBNull.Value) return 0;
                return Convert.ToInt32(v);
            }

            decimal GetDecimal(string col)
            {
                if (summary == null || summary.Rows.Count == 0) return 0m;
                var v = summary.Rows[0][col];
                if (v == null || v == DBNull.Value) return 0m;
                return Convert.ToDecimal(v);
            }

            int totalBranches = GetInt("TotalBranches");
            int totalRooms = GetInt("TotalRooms");
            int totalContracts = GetInt("TotalContracts");
            int totalInvoices = GetInt("TotalInvoices");
            int outstandingCount = GetInt("OutstandingInvoiceCount");
            decimal outstandingAmount = GetDecimal("OutstandingAmount");
            int totalDeposits = GetInt("TotalDeposits");
            decimal depositAmount = GetDecimal("DepositAmount");
            decimal paymentsThisMonth = GetDecimal("PaymentsThisMonth");
            int openMaintenance = GetInt("OpenMaintenance");

            var metrics = new[]
            {
                new StatMetric("Chi nhánh", totalBranches.ToString("N0"), "Tổng số chi nhánh", Color.FromArgb(0, 122, 204), (EventHandler)btnBranch_Click),
                new StatMetric("Phòng", totalRooms.ToString("N0"), "Tổng số phòng", Color.FromArgb(0, 150, 136), (EventHandler)btnRoom_Click),
                new StatMetric("Hợp đồng", totalContracts.ToString("N0"), "Tổng hợp đồng", Color.FromArgb(103, 58, 183), (EventHandler)btnContract_Click),
                new StatMetric("Hóa đơn", totalInvoices.ToString("N0"), $"Còn nợ: {outstandingCount:N0}", Color.FromArgb(255, 152, 0), (EventHandler)btnInvoice_Click),
                new StatMetric("Công nợ", outstandingAmount.ToString("N0"), "Tổng tiền còn nợ", Color.FromArgb(244, 67, 54), (EventHandler)btnInvoice_Click),
                new StatMetric("Đặt cọc", depositAmount.ToString("N0"), $"Phiếu cọc: {totalDeposits:N0}", Color.FromArgb(33, 150, 243), (EventHandler)btnDeposit_Click),
                new StatMetric("Thu tháng này", paymentsThisMonth.ToString("N0"), "Tổng tiền đã thu", Color.FromArgb(76, 175, 80), (EventHandler)btnPayment_Click),
                new StatMetric("Bảo trì", openMaintenance.ToString("N0"), "Yêu cầu đang mở", Color.FromArgb(156, 39, 176), (EventHandler)btnMaintenance_Click),
                new StatMetric("Chi nhánh", totalBranches.ToString("N0"), "Tổng số chi nhánh", Color.FromArgb(0, 122, 204), "🏢", (EventHandler)btnBranch_Click),
                new StatMetric("Phòng", totalRooms.ToString("N0"), "Tổng số phòng", Color.FromArgb(0, 150, 136), "🏠", (EventHandler)btnRoom_Click),
                new StatMetric("Khách thuê", totalTenants.ToString("N0"), "Tổng khách thuê", Color.FromArgb(63, 81, 181), "👥", (EventHandler)btnTenant_Click),
                new StatMetric("Hợp đồng", totalContracts.ToString("N0"), "Tổng hợp đồng", Color.FromArgb(103, 58, 183), "📋", (EventHandler)btnContract_Click),
                new StatMetric("Hóa đơn", totalInvoices.ToString("N0"), $"Còn nợ: {outstandingCount:N0}", Color.FromArgb(255, 152, 0), "🧾", (EventHandler)btnInvoice_Click),
                new StatMetric("Công nợ", outstandingAmount.ToString("N0"), "Tổng tiền còn nợ", Color.FromArgb(244, 67, 54), "💰", (EventHandler)btnInvoice_Click),
                new StatMetric("Đặt cọc", depositAmount.ToString("N0"), $"Phiếu cọc: {totalDeposits:N0}", Color.FromArgb(33, 150, 243), "💵", (EventHandler)btnDeposit_Click),
                new StatMetric("Thu tháng này", paymentsThisMonth.ToString("N0"), "Tổng tiền đã thu", Color.FromArgb(76, 175, 80), "📈", (EventHandler)btnPayment_Click),
                new StatMetric("Bảo trì", openMaintenance.ToString("N0"), "Yêu cầu đang mở", Color.FromArgb(156, 39, 176), "🔧", (EventHandler)btnMaintenance_Click),
            };

            int colCount = tableLayoutPanel1.ColumnCount;
            int rowCount = (int)Math.Ceiling(metrics.Length / (double)colCount);
            tableLayoutPanel1.RowCount = rowCount + 1; // Add one extra row to absorb space
            tableLayoutPanel1.RowStyles.Clear();
            for (int i = 0; i < rowCount; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 130f)); // Slightly smaller height
            }
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // This row takes all remaining space

            for (int i = 0; i < metrics.Length; i++)
            {
                int row = i / colCount;
                int col = i % colCount;
                AddStatCard(row, col, metrics[i]);
            }
        }

        private void InitializeOverviewLayout()
        {
            _overviewHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            pnlContent.Controls.Remove(tableLayoutPanel1);

            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.Margin = new Padding(0, 0, 0, 12);

            _recentPanel = BuildRecentActivityPanel();
            _overviewHost.Controls.Add(_recentPanel);
            _overviewHost.Controls.Add(tableLayoutPanel1);
            pnlContent.Controls.Add(_overviewHost);
            _overviewHost.Visible = false;
        }

        private Panel BuildRecentActivityPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 320,
                BackColor = Color.White,
                Padding = new Padding(18),
                Margin = new Padding(24, 0, 24, 24)
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.Transparent
            };

            var title = new Label
            {
                Text = "Hoạt động gần đây",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(0, 8)
            };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            _recentViewAllButton = UiKit.MakeButton("Xem tất cả", UiKit.Primary, (s, e) => btnNotification_Click(s, e), 110);
            _recentRefreshButton = UiKit.MakeButton("Tải lại", UiKit.Success, async (s, e) => await LoadRecentActivityAsync(), 90);
            actions.Controls.Add(_recentViewAllButton);
            actions.Controls.Add(_recentRefreshButton);

            var filterRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 6)
            };

            _recentSearchBox = new ModernSearchBox
            {
                Width = 320,
                PlaceholderText = "Tìm theo nội dung/nhân viên...",
                Margin = new Padding(0, 0, 12, 0)
            };
            _recentSearchBox.SearchTriggered += (s, e) =>
            {
                _recentKeyword = _recentSearchBox.Text;
                BindRecentActivity();
            };
            _recentSearchBox.TextChanged += (s, e) =>
            {
                _recentKeyword = _recentSearchBox.Text;
                BindRecentActivity();
            };

            var groupLabel = new Label
            {
                Text = "Hiển thị:",
                AutoSize = true,
                Margin = new Padding(0, 6, 6, 0),
                ForeColor = Color.FromArgb(90, 90, 90),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            _recentGroupCombo = new ComboBox
            {
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 2, 12, 0)
            };
            _recentGroupCombo.Items.AddRange(new object[] { "Theo ngày", "Danh sách" });
            _recentGroupCombo.SelectedIndex = _recentGroupByDay ? 0 : 1;
            _recentGroupCombo.SelectedIndexChanged += (s, e) =>
            {
                _recentGroupByDay = _recentGroupCombo.SelectedIndex == 0;
                BindRecentActivity();
            };

            filterRow.Controls.Add(_recentSearchBox);
            filterRow.Controls.Add(groupLabel);
            filterRow.Controls.Add(_recentGroupCombo);

            _recentGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UiKit.StyleGrid(_recentGrid);
            _recentGrid.ColumnHeadersHeight = 34;
            _recentGrid.CellFormatting += RecentGrid_CellFormatting;
            _recentGrid.RowPrePaint += RecentGrid_RowPrePaint;
            panel.Controls.Add(_recentGrid);

            _recentEmptyLabel = new Label
            {
                Text = "Chưa có hoạt động gần đây",
                AutoSize = true,
                ForeColor = Color.FromArgb(149, 165, 166),
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                BackColor = Color.White,
                Visible = false
            };
            panel.Controls.Add(_recentEmptyLabel);
            _recentEmptyLabel.BringToFront();
            panel.Resize += (s, e) =>
            {
                if (_recentEmptyLabel != null)
                {
                    _recentEmptyLabel.Location = new Point(
                        (panel.Width - _recentEmptyLabel.Width) / 2,
                        (panel.Height - _recentEmptyLabel.Height) / 2);
                }
            };

            panel.Controls.Add(filterRow);
            panel.Controls.Add(header);

            return panel;
        }

        private async System.Threading.Tasks.Task LoadRecentActivityAsync()
        {
            if (_recentGrid == null) return;

            try
            {
                _recentSource = await adminDataBLL.GetNotificationsAsync();
                BindRecentActivity();
            }
            catch
            {
                _recentEmptyLabel.Text = "Không thể tải hoạt động gần đây";
                _recentEmptyLabel.Visible = true;
            }
        }

        private void BindRecentActivity()
        {
            var table = BuildRecentActivityTable(_recentSource, _recentGroupByDay, _recentKeyword);
            _recentGrid.DataSource = table;

            if (_recentGrid.Columns.Contains("THỜI GIAN"))
                _recentGrid.Columns["THỜI GIAN"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

            if (_recentGrid.Columns.Contains("NỘI DUNG"))
                _recentGrid.Columns["NỘI DUNG"].FillWeight = 200;

            if (_recentGrid.Columns.Contains("LOẠI"))
                _recentGrid.Columns["LOẠI"].FillWeight = 120;
            if (_recentGrid.Columns.Contains("THỜI GIAN"))
                _recentGrid.Columns["THỜI GIAN"].FillWeight = 110;
            if (_recentGrid.Columns.Contains("TRẠNG THÁI"))
                _recentGrid.Columns["TRẠNG THÁI"].FillWeight = 80;

            if (_recentGrid.Columns.Contains("IS_GROUP"))
                _recentGrid.Columns["IS_GROUP"].Visible = false;

            foreach (DataGridViewColumn col in _recentGrid.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            _recentEmptyLabel.Visible = table == null || table.Rows.Count == 0;
        }

        private static DataTable BuildRecentActivityTable(DataTable notifications, bool groupByDay, string keyword)
        {
            var dt = new DataTable();
            dt.Columns.Add("LOẠI", typeof(string));
            dt.Columns.Add("NỘI DUNG", typeof(string));
            dt.Columns.Add("THỜI GIAN", typeof(DateTime));
            dt.Columns.Add("TRẠNG THÁI", typeof(string));
            dt.Columns.Add("IS_GROUP", typeof(bool));

            if (notifications == null || notifications.Rows.Count == 0) return dt;

            var rows = notifications.AsEnumerable()
                .Where(r =>
                {
                    var title = r.Table.Columns.Contains("Title") ? r["Title"]?.ToString() : null;
                    return !string.IsNullOrWhiteSpace(title)
                        && title.IndexOf("Hoạt động nhân viên", StringComparison.OrdinalIgnoreCase) >= 0;
                })
                .Where(r =>
                {
                    if (string.IsNullOrWhiteSpace(keyword)) return true;
                    var lower = keyword.Trim().ToLowerInvariant();
                    var title = r.Table.Columns.Contains("Title") ? r["Title"]?.ToString() : string.Empty;
                    var message = r.Table.Columns.Contains("Message") ? r["Message"]?.ToString() : string.Empty;
                    return (title ?? string.Empty).ToLowerInvariant().Contains(lower)
                        || (message ?? string.Empty).ToLowerInvariant().Contains(lower);
                })
                .Select(r => new
                {
                    Title = r["Title"]?.ToString(),
                    Message = r.Table.Columns.Contains("Message") ? r["Message"]?.ToString() : null,
                    Status = ToVietnameseStatus(r.Table.Columns.Contains("Status") ? r["Status"]?.ToString() : null),
                    Created = TryReadDateTime(r, "CreatedDate")
                })
                .Where(x => x.Created.HasValue)
                .OrderByDescending(x => x.Created.Value)
                .Take(20)
                .ToList();

            if (!groupByDay)
            {
            foreach (var row in rows)
            {
                dt.Rows.Add(row.Title, row.Message, row.Created.Value, row.Status, false);
            }
                return dt;
            }

            var grouped = rows
                .GroupBy(r => r.Created.Value.Date)
                .OrderByDescending(g => g.Key);

            foreach (var group in grouped)
            {
                var label = BuildGroupLabel(group.Key);
                dt.Rows.Add(label, string.Empty, group.Key, string.Empty, true);

                foreach (var row in group.OrderByDescending(r => r.Created.Value))
                {
                    dt.Rows.Add(row.Title, row.Message, row.Created.Value, row.Status, false);
                }
            }

            return dt;
        }

        private static string BuildGroupLabel(DateTime date)
        {
            var today = DateTime.Today;
            if (date == today) return $"Hôm nay - {date:dd/MM/yyyy}";
            if (date == today.AddDays(-1)) return $"Hôm qua - {date:dd/MM/yyyy}";
            return date.ToString("dddd, dd/MM/yyyy", new CultureInfo("vi-VN"));
        }

        private static DateTime? TryReadDateTime(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            if (v == null || v == DBNull.Value) return null;
            if (v is DateTime dt) return dt;
            return DateTime.TryParse(v.ToString(), out var parsed) ? parsed : (DateTime?)null;
        }

        private static string ToVietnameseStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "Chưa xác định";
            var key = status.Trim();
            switch (key.ToLowerInvariant())
            {
                case "unread":
                    return "Chưa đọc";
                case "read":
                    return "Đã đọc";
                case "sent":
                    return "Đã gửi";
                default:
                    return status;
            }
        }

        private void RecentGrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = _recentGrid.Rows[e.RowIndex];
            if (!IsGroupRow(row)) return;
            row.DefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
            row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 248, 252);
            row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(52, 73, 94);
            row.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            row.Height = 30;
            row.ReadOnly = true;
        }

        private void RecentGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = _recentGrid.Rows[e.RowIndex];
            if (!IsGroupRow(row)) return;

            if (_recentGrid.Columns[e.ColumnIndex].Name == "LOẠI")
            {
                e.CellStyle.ForeColor = Color.FromArgb(52, 73, 94);
                return;
            }

            e.Value = string.Empty;
            e.FormattingApplied = true;
        }

        private static bool IsGroupRow(DataGridViewRow row)
        {
            if (row?.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("IS_GROUP"))
            {
                var v = drv.Row["IS_GROUP"];
                return v != null && v != DBNull.Value && (bool)v;
            }
            return false;
        }

        private sealed class StatMetric
        {
            public StatMetric(string title, string valueText, string subText, Color accentColor, string icon, EventHandler clickHandler)
            {
                Title = title;
                ValueText = valueText;
                SubText = subText;
                AccentColor = accentColor;
                Icon = icon;
                ClickHandler = clickHandler;
            }

            public string Title { get; }
            public string ValueText { get; }
            public string SubText { get; }
            public Color AccentColor { get; }
            public string Icon { get; }
            public EventHandler ClickHandler { get; }
        }

        private void AddStatCard(int row, int col, StatMetric metric)
        {
            // Use enhanced ModernStatCard instead of manual Panel
            var card = new ModernStatCard(
                metric.Title,
                metric.ValueText,
                metric.SubText,
                metric.AccentColor
            )
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10), // Slightly smaller margin
                Icon = metric.Icon
            };

            // Attach click handler
            if (metric.ClickHandler != null)
            {
                card.OnCardClick += metric.ClickHandler;
            }

            tableLayoutPanel1.Controls.Add(card, col, row);
        }

        private void ClearCurrentModule()
        {
            if (currentModule != null)
            {
                currentModule.Close();
                currentModule.Dispose();
                currentModule = null;
            }

            pnlModuleHost.Controls.Clear();
            pnlModuleHost.Visible = false;
        }

        private void LoadModule(Form module, string headerTitle, bool showHeaderTitle = true)
        {
            HideOverview();
            lblPlaceholder.Visible = false;

            ClearCurrentModule();

            module.TopLevel = false;
            module.FormBorderStyle = FormBorderStyle.None;
            module.Dock = DockStyle.Fill;
            module.StartPosition = FormStartPosition.CenterParent;

            currentModule = module;
            pnlModuleHost.Controls.Add(module);
            pnlModuleHost.Visible = true;
            pnlModuleHost.BringToFront();

            // Luôn hiển thị tiêu đề của module
            lblWelcome.Text = headerTitle;
            module.Show();

            // Ensure notification bell stays on top after module is loaded
            if (_notificationBell != null && !_notificationBell.IsDisposed)
            {
                _notificationBell.BringToFront();
            }
        }

        private async System.Threading.Tasks.Task ShowOverviewAsync()
        {
            ClearCurrentModule();

            using (var loading = new SimpleLoadingOverlay(this, "Đang tải thông kê"))
            {
                try
                {
                    lblPlaceholder.Visible = true;
                    if (_overviewHost != null) _overviewHost.Visible = false;
                    pnlModuleHost.Visible = false;

                    var summary = await adminDataBLL.GetDashboardSummaryAsync();
                    BuildOverviewStats(summary);
                    await LoadRecentActivityAsync();

                    if (_overviewHost != null) _overviewHost.Visible = true;
                    lblPlaceholder.Visible = false;
                    pnlModuleHost.Visible = false;
                    lblWelcome.Text = $"Xin chào Admin: {currentUser}";
                    _overviewDirty = false;
                }
                catch (Exception ex)
                {
                    if (_overviewHost != null) _overviewHost.Visible = false;
                    pnlModuleHost.Visible = false;
                    lblPlaceholder.Text = "Không thể tải thống kê tổng quan.\n\n" + ex.Message;
                    lblPlaceholder.Visible = true;
                    lblWelcome.Text = $"Xin chào : {currentUser}";
                }
            }
        }

        private void HandleAdminDataChanged()
        {
            _overviewDirty = true;
            if (_overviewHost != null && _overviewHost.Visible && IsHandleCreated)
            {
                BeginInvoke(new Action(() => { _ = ShowOverviewAsync(); }));
            }
        }

        private void HideOverview()
        {
            if (_overviewHost != null) _overviewHost.Visible = false;
            lblPlaceholder.Visible = false;
        }

        private void ShowSectionInfo(string title, params string[] features)
        {
            ClearCurrentModule();
            HideOverview();
            lblPlaceholder.Text = $"{title}\n\n{BuildBulletText(features)}";
            lblPlaceholder.Visible = true;
            pnlModuleHost.Visible = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AdminEvents.DataChanged -= HandleAdminDataChanged;
            ClearCurrentModule();
            base.OnFormClosing(e);
        }

        // ========== EVENT HANDLERS ==========

        private async void btnOverview_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavOverview);
            if (!_overviewDirty && _overviewHost != null && _overviewHost.Visible && currentModule == null)
                return;
            lblPlaceholder.Text = defaultPlaceholderText;
            await ShowOverviewAsync();
        }

        private void LoadModuleSafe(Func<Form> create, string headerTitle, bool showHeaderTitle = true)
        {
            try
            {
                LoadModule(create(), headerTitle, showHeaderTitle);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở chức năng này.\n\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _ = ShowOverviewAsync();
            }
        }

        private void btnBranch_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavBranch);
            LoadModuleSafe(() => new FrmBranchCards(), "Quản lý chi nhánh");
        }

        private void btnRoom_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavRoom);
            LoadModuleSafe(() => new FrmRoomManager(), "Quản lý phòng");
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavStaff);
            LoadModuleSafe(() => new FrmStaffManager(), "Quản lý nhân viên");
        }

        private void btnTenant_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavTenant);
            LoadModuleSafe(() => new FrmTenantManager(), "Quản lý khách thuê");
        }

        private void btnContract_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavContract);
            LoadModuleSafe(() => new FrmContractManager(), "Quản lý hợp đồng");
        }

        private void btnDeposit_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavDeposit);
            LoadModuleSafe(() => new FrmDepositManager(), "Đặt cọc");
        }

        private void btnUtility_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavUtility);
            LoadModuleSafe(() => new FrmUtilityManager(), "Điện - Nước - Dịch vụ");
        }

        private void btnInvoice_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavInvoice);
            // MERGED: Mở FrmInvoicePaymentUnified (2 tabs: Hóa Đơn + Thanh Toán)
            LoadModuleSafe(() => new FrmInvoicePaymentUnified(null, false, false, currentUserId), "💳 Hóa Đơn & Thanh Toán");
        }

        private void btnPayment_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavInvoice);
            LoadModuleSafe(() => new FrmInvoicePaymentUnified(null, false, false, currentUserId), "💳 Hóa Đơn & Thanh Toán");
        }

        private void btnMaintenance_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavMaintenance);
            LoadModuleSafe(() => new FrmMaintenanceManager(), "Bảo trì & sự cố");
        }

        private void btnAsset_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavAsset);
            LoadModuleSafe(() => new FrmAssetManager(), "Tài sản");
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavReport);
            LoadModuleSafe(() => new FrmReportManager(adminDataBLL), "Báo cáo & thống kê");
        }

        private void btnNotification_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavNotification);
            LoadModuleSafe(() => new FrmNotificationManager(), "Thông báo & nhắc lịch");
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavSettings);
            LoadModuleSafe(() => new FrmSystemSettingsManager(), "Cấu hình hệ thống");
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
