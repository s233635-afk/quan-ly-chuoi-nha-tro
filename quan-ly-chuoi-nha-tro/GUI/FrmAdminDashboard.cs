using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

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
                MessageBox.Show("Bạn không có quyền truy cập Admin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            StyleNavButton(btnNavPayment);
            StyleNavButton(btnNavMaintenance);
            StyleNavButton(btnNavAsset);
            StyleNavButton(btnNavNotification);
            StyleNavButton(btnNavSettings);

            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(btnLogout.BackColor);
        }

        private void StyleNavButton(Button btn)
        {
            if (btn == null) return;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(btn.BackColor);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.DarkDark(btn.BackColor);
            btn.Cursor = Cursors.Hand;
        }

        private void SetActiveNav(Button btn)
        {
            if (btn == null) return;

            if (_activeNavButton != null && !_activeNavButton.IsDisposed)
            {
                _activeNavButton.BackColor = Color.FromArgb(0, 122, 204);
                _activeNavButton.ForeColor = Color.White;
            }

            _activeNavButton = btn;
            _activeNavButton.BackColor = Color.FromArgb(0, 90, 170);
            _activeNavButton.ForeColor = Color.White;
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
                new StatMetric("Chi nhánh", totalBranches.ToString("N0"), "Tổng số chi nhánh", Color.FromArgb(0, 122, 204), (EventHandler)btnBranch_Click),
                new StatMetric("Phòng", totalRooms.ToString("N0"), "Tổng số phòng", Color.FromArgb(0, 150, 136), (EventHandler)btnRoom_Click),
                new StatMetric("Khách thuê", totalTenants.ToString("N0"), "Tổng khách thuê", Color.FromArgb(63, 81, 181), (EventHandler)btnTenant_Click),
                new StatMetric("Hợp đồng", totalContracts.ToString("N0"), "Tổng hợp đồng", Color.FromArgb(103, 58, 183), (EventHandler)btnContract_Click),
                new StatMetric("Hóa đơn", totalInvoices.ToString("N0"), $"Còn nợ: {outstandingCount:N0}", Color.FromArgb(255, 152, 0), (EventHandler)btnInvoice_Click),
                new StatMetric("Công nợ", outstandingAmount.ToString("N0"), "Tổng tiền còn nợ", Color.FromArgb(244, 67, 54), (EventHandler)btnInvoice_Click),
                new StatMetric("Đặt cọc", depositAmount.ToString("N0"), $"Phiếu cọc: {totalDeposits:N0}", Color.FromArgb(33, 150, 243), (EventHandler)btnDeposit_Click),
                new StatMetric("Thu tháng này", paymentsThisMonth.ToString("N0"), "Tổng tiền đã thu", Color.FromArgb(76, 175, 80), (EventHandler)btnPayment_Click),
                new StatMetric("Bảo trì", openMaintenance.ToString("N0"), "Yêu cầu đang mở", Color.FromArgb(156, 39, 176), (EventHandler)btnMaintenance_Click),
            };

            int colCount = tableLayoutPanel1.ColumnCount;
            int rowCount = (int)Math.Ceiling(metrics.Length / (double)colCount);
            tableLayoutPanel1.RowCount = rowCount;
            tableLayoutPanel1.RowStyles.Clear();
            for (int i = 0; i < rowCount; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
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
            public StatMetric(string title, string valueText, string subText, Color accentColor, EventHandler clickHandler)
            {
                Title = title;
                ValueText = valueText;
                SubText = subText;
                AccentColor = accentColor;
                ClickHandler = clickHandler;
            }

            public string Title { get; }
            public string ValueText { get; }
            public string SubText { get; }
            public Color AccentColor { get; }
            public EventHandler ClickHandler { get; }
        }

        private void AddStatCard(int row, int col, StatMetric metric)
        {
            Panel pnl = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                Padding = new Padding(14, 12, 14, 12),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            Panel accent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = metric.AccentColor
            };

            Label lblTitle = new Label
            {
                Text = metric.Title,
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Top,
                Padding = new Padding(10, 2, 0, 4)
            };

            Label lblValue = new Label
            {
                Text = metric.ValueText,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 55, 90),
                Dock = DockStyle.Top,
                Height = 48,
                Padding = new Padding(10, 0, 0, 0)
            };

            Label lblSub = new Label
            {
                Text = metric.SubText,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(70, 94, 120),
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 4, 0, 0),
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };

            // Add theo thứ tự để Dock layout đúng (Fill trước, Top sau)
            pnl.Controls.Add(accent);
            pnl.Controls.Add(lblSub);
            pnl.Controls.Add(lblValue);
            pnl.Controls.Add(lblTitle);

            pnl.Cursor = Cursors.Hand;

            // Xử lý sự kiện click
            if (metric.ClickHandler != null)
            {
                pnl.Click += metric.ClickHandler;
                lblTitle.Click += metric.ClickHandler;
                lblValue.Click += metric.ClickHandler;
                lblSub.Click += metric.ClickHandler;
                accent.Click += metric.ClickHandler;
            }
            pnl.MouseEnter += (s, e) =>
            {
                pnl.BackColor = Color.FromArgb(232, 244, 255);
                accent.BackColor = ControlPaint.Dark(metric.AccentColor);
            };
            pnl.MouseLeave += (s, e) =>
            {
                pnl.BackColor = Color.White;
                accent.BackColor = metric.AccentColor;
            };

            tableLayoutPanel1.Controls.Add(pnl, col, row);
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

        private void LoadModule(Form module, string headerTitle)
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

            lblWelcome.Text = headerTitle;
            module.Show();
        }

        private async System.Threading.Tasks.Task ShowOverviewAsync()
        {
            ClearCurrentModule();

            try
            {
                // Có thể hiển thị trạng thái tải nhanh (nếu muốn)
                lblPlaceholder.Text = "Đang tải thống kê...";
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
                lblWelcome.Text = $"Xin chào Admin: {currentUser}";
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

        private void LoadModuleSafe(Func<Form> create, string headerTitle)
        {
            try
            {
                LoadModule(create(), headerTitle);
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
            LoadModuleSafe(() => new FrmBranch(), "Quản lý chi nhánh");
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
            LoadModuleSafe(() => new FrmDepositManager(), "Đặt phòng & đặt cọc");
        }

        private void btnPayment_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavPayment);
            LoadModuleSafe(() => new FrmPaymentManager(adminDataBLL), "Thanh toán");
        }

        private void btnUtility_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavUtility);
            LoadModuleSafe(() => new FrmUtilityManager(), "Điện - Nước - Dịch vụ");
        }

        private void btnInvoice_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavInvoice);
            LoadModuleSafe(() => new FrmInvoiceManager(), "Hóa đơn & thanh toán");
        }

        private void btnMaintenance_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavMaintenance);
            LoadModuleSafe(() => new FrmMaintenanceManager(), "Bảo trì & sự cố");
        }

        private void btnAsset_Click(object sender, EventArgs e)
        {
            SetActiveNav(btnNavAsset);
            LoadModuleSafe(() => new FrmAssetManager(), "Quản lý tài sản");
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
