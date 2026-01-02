using System;
using System.Data;
using System.Drawing;
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

        public FrmAdminDashboard(string username, int userId)
        {
            InitializeComponent();
            currentUser = username;
            currentUserId = userId;
            AdminEvents.DataChanged += HandleAdminDataChanged;
            ApplyNavStyling();
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

            this.Text = $"Bảng điều khiển Admin - {currentUser}";
            this.WindowState = FormWindowState.Maximized;
            lblWelcome.Text = $"Xin chào Admin: {currentUser}";
            lblUser.Text = $"Admin: {currentUser}";
            lblPlaceholder.Text = defaultPlaceholderText;
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
            _notificationBell.BellClicked += (s, e) => {
                SetActiveNav(btnNavNotification);
                btnNotification_Click(s, e);
            };
            pnlHeader.Controls.Add(_notificationBell);
            _notificationBell.BringToFront();
            
            // Update position when panel resizes
            pnlHeader.Resize += (s, e) => {
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
            StyleNavButton(btnNavTenant);
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
        }

        private void StyleNavButton(Button btn)
        {
            if (btn == null) return;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 73, 94);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(41, 128, 185);
            btn.Cursor = Cursors.Hand;
        }

        private void SetActiveNav(Button btn)
        {
            if (btn == null) return;

            if (_activeNavButton != null && !_activeNavButton.IsDisposed)
            {
                _activeNavButton.BackColor = Color.Transparent;
                _activeNavButton.ForeColor = Color.FromArgb(189, 195, 199);
                _activeNavButton.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }

            _activeNavButton = btn;
            _activeNavButton.BackColor = Color.FromArgb(52, 152, 219); // Active Blue
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
            tableLayoutPanel1.Padding = new Padding(20, 10, 20, 20);
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
            int totalTenants = GetInt("TotalTenants");
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
            tableLayoutPanel1.RowCount = rowCount;
            tableLayoutPanel1.RowStyles.Clear();
            for (int i = 0; i < rowCount; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rowCount));
            }

            for (int i = 0; i < metrics.Length; i++)
            {
                int row = i / colCount;
                int col = i % colCount;
                AddStatCard(row, col, metrics[i]);
            }
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
                Margin = new Padding(16),
                MinimumSize = new Size(0, 160),
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
                    tableLayoutPanel1.Visible = false;
                    pnlModuleHost.Visible = false;

                    var summary = await adminDataBLL.GetDashboardSummaryAsync();
                    BuildOverviewStats(summary);

                    tableLayoutPanel1.Visible = true;
                    lblPlaceholder.Visible = false;
                    pnlModuleHost.Visible = false;
                    lblWelcome.Text = $"Xin chào Admin: {currentUser}";
                    _overviewDirty = false;
                }
                catch (Exception ex)
                {
                    tableLayoutPanel1.Visible = false;
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
            if (tableLayoutPanel1.Visible && IsHandleCreated)
            {
                BeginInvoke(new Action(() => { _ = ShowOverviewAsync(); }));
            }
        }

        private void HideOverview()
        {
            tableLayoutPanel1.Visible = false;
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
            if (!_overviewDirty && tableLayoutPanel1.Visible && currentModule == null)
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
            LoadModuleSafe(() => new FrmInvoicePaymentUnified(), "💳 Hóa Đơn & Thanh Toán");
        }

        private void btnPayment_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavInvoice);
            LoadModuleSafe(() => new FrmInvoicePaymentUnified(), "💳 Hóa Đơn & Thanh Toán");
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
