using System;
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
        private bool overviewBuilt = false;

        public FrmAdminDashboard(string username, int userId)
        {
            InitializeComponent();
            currentUser = username;
            currentUserId = userId;
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

            this.Text = $"Admin Dashboard - {currentUser}";
            this.WindowState = FormWindowState.Maximized;
            lblWelcome.Text = $"👋 Xin chào Admin: {currentUser}";
            lblUser.Text = $"Admin: {currentUser}";
            HideOverview();
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

        private void BuildOverviewMenu()
        {
            if (overviewBuilt) return;
            // Tạo các nút menu cho 13 tính năng Admin
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Clear();
            for (int i = 0; i < 7; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));
            }

            // 01. Quản lý Chi nhánh
            AddMenuButton(0, 0, "📍 Quản Lý Chi Nhánh", "Thêm/Sửa/Xóa/Cấu hình chi nhánh", btnBranch_Click);

            // 02. Quản lý Phòng
            AddMenuButton(0, 1, "🏠 Quản Lý Phòng", "Tạo/Sửa phòng, loại phòng, trạng thái", btnRoom_Click);

            // 03. Quản lý Nhân viên
            AddMenuButton(1, 0, "👤 Quản Lý Nhân Viên", "Tạo tài khoản, gán chi nhánh, theo dõi", btnStaff_Click);

            // 04. Quản lý Khách thuê
            AddMenuButton(1, 1, "👥 Quản Lý Khách Thuê", "Hồ sơ, tạm trú, check-in/out, lịch sử", btnTenant_Click);

            // 05. Quản lý Hợp đồng
            AddMenuButton(2, 0, "📄 Quản Lý Hợp Đồng", "Tạo, gia hạn, chấm dứt hợp đồng", btnContract_Click);

            // 06. Đặt phòng & Cọc
            AddMenuButton(2, 1, "💰 Đặt Phòng - Cọc", "Nhận lead, thu cọc, hủy đặt phòng", btnDeposit_Click);

            // 07. Điện - Nước - Dịch vụ
            AddMenuButton(3, 0, "⚡ Điện, Nước, Dịch Vụ", "Nhập chỉ số, quản lý dịch vụ, phí", btnUtility_Click);

            // 08. Hóa đơn & Thanh toán
            AddMenuButton(3, 1, "💳 Hóa Đơn - Thanh Toán", "Tạo hóa đơn, thu tiền, quản lý công nợ", btnInvoice_Click);

            // 09. Bảo trì & Sự cố
            AddMenuButton(4, 0, "🔧 Bảo Trì - Sự Cố", "Quản lý ticket, phân công, theo dõi", btnMaintenance_Click);

            // 10. Quản lý Tài sản
            AddMenuButton(4, 1, "📦 Quản Lý Tài Sản", "Danh mục tài sản, hư hỏng, thay thế", btnAsset_Click);

            // 11. Báo cáo & Thống kê
            AddMenuButton(5, 0, "📊 Báo Cáo - Thống Kê", "Báo cáo tổng hợp, doanh thu, công nợ", btnReport_Click);

            // 12. Thông báo & Nhắc lịch
            AddMenuButton(5, 1, "🔔 Thông Báo - Nhắc Lịch", "Nhắc nợ, hết hạn, hệ thống thông báo", btnNotification_Click);

            // 13. Cấu hình Hệ thống
            AddMenuButton(6, 0, "⚙️ Cấu Hình Hệ Thống", "Tham số mặc định, sao lưu, phục hồi", btnSettings_Click);

            overviewBuilt = true;
        }

        private void AddMenuButton(int row, int col, string title, string description, EventHandler clickHandler)
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
                BackColor = Color.FromArgb(0, 122, 204)
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Top,
                Padding = new Padding(10, 2, 0, 4)
            };

            Label lblDesc = new Label
            {
                Text = description,
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(70, 94, 120),
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 0, 0),
                AutoSize = true,
                TextAlign = System.Drawing.ContentAlignment.TopLeft
            };

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblDesc);
            pnl.Controls.Add(accent);

            pnl.Cursor = Cursors.Hand;

            // Xử lý sự kiện click
            pnl.Click += clickHandler;
            lblTitle.Click += clickHandler;
            lblDesc.Click += clickHandler;
            accent.Click += clickHandler;
            pnl.MouseEnter += (s, e) =>
            {
                pnl.BackColor = Color.FromArgb(232, 244, 255);
                accent.BackColor = Color.FromArgb(0, 105, 190);
            };
            pnl.MouseLeave += (s, e) =>
            {
                pnl.BackColor = Color.White;
                accent.BackColor = Color.FromArgb(0, 122, 204);
            };

            tableLayoutPanel1.Controls.Add(pnl, col, row);
        }

        private void ShowOverview()
        {
            BuildOverviewMenu();
            tableLayoutPanel1.Visible = true;
            lblPlaceholder.Visible = false;
        }

        private void HideOverview()
        {
            tableLayoutPanel1.Visible = false;
            lblPlaceholder.Visible = true;
        }

        // ========== EVENT HANDLERS ==========

        private void btnOverview_Click(object sender, EventArgs e)
        {
            ShowOverview();
        }

        private void btnBranch_Click(object sender, EventArgs e)
        {
            using (FrmBranch frm = new FrmBranch())
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog();
            }
        }

        private void btnRoom_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Quản Lý Phòng", () => adminDataBLL.GetRoomsAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Quản Lý Nhân Viên", () => adminDataBLL.GetStaffAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnTenant_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Quản Lý Khách Thuê", () => adminDataBLL.GetTenantsAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnContract_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Quản Lý Hợp Đồng", () => adminDataBLL.GetContractsAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnDeposit_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Đặt Phòng & Cọc", () => adminDataBLL.GetDepositsAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnUtility_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Điện - Nước - Dịch Vụ", () => adminDataBLL.GetUtilitiesAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnInvoice_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Hóa Đơn & Thanh Toán", () => adminDataBLL.GetInvoicesAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnMaintenance_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Bảo Trì & Sự Cố", () => adminDataBLL.GetMaintenanceAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnAsset_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Quản Lý Tài Sản", () => adminDataBLL.GetAssetsAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Báo Cáo & Thống Kê", () => adminDataBLL.GetInvoicesAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnNotification_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Thông Báo & Nhắc Lịch", () => adminDataBLL.GetNotificationsAsync()))
            {
                frm.ShowDialog();
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDataViewer("Cấu Hình Hệ Thống", () => adminDataBLL.GetSystemSettingsAsync()))
            {
                frm.ShowDialog();
            }
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
