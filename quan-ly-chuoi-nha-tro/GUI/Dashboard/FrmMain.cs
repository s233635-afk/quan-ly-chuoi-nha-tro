using System;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmMain : Form
    {
        public string CurrentUser { get; set; }
        public string CurrentRole { get; set; }

        public FrmMain()
        {
            InitializeComponent();
        }

        public FrmMain(string username, string role) : this()
        {
            CurrentUser = username;
            CurrentRole = role;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            ApplyModernStyling();
            this.Text = $"Quản Lý Chuỗi Nhà Trọ - {CurrentUser} ({CurrentRole})";
            this.WindowState = FormWindowState.Maximized;
            
            if (Controls.Contains(lblUserInfo))
                lblUserInfo.Text = $"👤 {CurrentUser} | Vai trò: {CurrentRole}";
            
            InitializeTreeMenu();
            ExpandRootNodes();
        }

        private void ApplyModernStyling()
        {
            // Form background
            this.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);

            // TreeView styling
            if (Controls.Contains(treeViewMenu))
            {
                treeViewMenu.BackColor = System.Drawing.Color.White;
                treeViewMenu.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
                treeViewMenu.Font = new System.Drawing.Font("Segoe UI", 10F);
            }

            // User info label
            if (Controls.Contains(lblUserInfo))
            {
                lblUserInfo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
                lblUserInfo.ForeColor = System.Drawing.Color.FromArgb(0, 120, 212);
            }
        }

        private void InitializeTreeMenu()
        {
            if (!Controls.Contains(treeViewMenu)) return;

            treeViewMenu.Nodes.Clear();

            // ========== ADMIN FEATURES ONLY ==========
            TreeNode nodeAdmin = new TreeNode("📋 QUẢN LÝ (Admin)");

            TreeNode n2 = new TreeNode("🚪 Quản Lý Phòng");
            n2.Nodes.Add("➕ Tạo/Sửa/Xóa");
            n2.Nodes.Add("📊 Trạng thái");

            TreeNode n3 = new TreeNode("👥 Quản Lý Nhân Viên");
            n3.Nodes.Add("➕ Tạo tài khoản");
            n3.Nodes.Add("🔐 Phân quyền");

            TreeNode n4 = new TreeNode("🧑 Quản Lý Khách Thuê");
            n4.Nodes.Add("📋 Hồ sơ");
            n4.Nodes.Add("🏠 Check-in/out");

            TreeNode n5 = new TreeNode("📜 Hợp Đồng");
            n5.Nodes.Add("✏️ Tạo mới");
            n5.Nodes.Add("📄 Xuất PDF");

            TreeNode n6 = new TreeNode("💰 Ký Cược");
            n6.Nodes.Add("💵 Thu tiền");
            n6.Nodes.Add("↩️ Hoàn cọc");

            TreeNode n7 = new TreeNode("💡 Tiện Ích");
            n7.Nodes.Add("📊 Điện/Nước");
            n7.Nodes.Add("🔧 Dịch vụ");

            // ===== ADMIN - HÓA ĐƠN & THANH TOÁN (FULL) =====
            TreeNode n8 = new TreeNode("💳 Hóa Đơn & Thanh Toán");
            n8.Nodes.Add("📋 Xem danh sách");
            n8.Nodes.Add("➕ Thêm");
            n8.Nodes.Add("✏️ Sửa");
            n8.Nodes.Add("🗑️ Xóa");
            n8.Nodes.Add("💵 Thu tiền");
            n8.Nodes.Add("📅 Tạo tháng");
            n8.Nodes.Add("📊 Xuất CSV");
            n8.Nodes.Add("🔄 Tải lại");

            TreeNode n9 = new TreeNode("🔧 Bảo Trì");
            n9.Nodes.Add("🛠️ Sửa chữa");
            n9.Nodes.Add("🧹 Vệ sinh");

            TreeNode n10 = new TreeNode("🏠 Tài Sản");
            n10.Nodes.Add("📦 Danh mục");
            n10.Nodes.Add("❌ Hư hỏng");

            TreeNode n11 = new TreeNode("📊 Báo Cáo");
            n11.Nodes.Add("📈 Thống kê");
            n11.Nodes.Add("💰 Doanh thu");

            TreeNode n12 = new TreeNode("📢 Thông Báo");
            n12.Nodes.Add("🔔 Nhắc lịch");
            n12.Nodes.Add("📮 Nội bộ");

            TreeNode n13 = new TreeNode("⚙️ Cài Đặt");
            n13.Nodes.Add("🔧 Tham số");
            n13.Nodes.Add("👨‍💼 Quản trị");

            nodeAdmin.Nodes.AddRange(new[] { n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13 });

            // ========== ACCOUNT ==========
            TreeNode nodeAccount = new TreeNode("⚙️ TÀI KHOẢN");
            nodeAccount.Nodes.Add("🔑 Đổi mật khẩu");
            nodeAccount.Nodes.Add("👤 Thông tin");
            nodeAccount.Nodes.Add("🚪 Đăng xuất");

            // Add to tree - ONLY ADMIN + ACCOUNT (NO STAFF)
            treeViewMenu.Nodes.Add(nodeAdmin);
            treeViewMenu.Nodes.Add(nodeAccount);
        }

        private void ExpandRootNodes()
        {
            if (Controls.Contains(treeViewMenu))
            {
                foreach (TreeNode node in treeViewMenu.Nodes)
                {
                    node.Expand();
                }
            }
        }

        private void treeViewMenu_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string nodeName = e.Node.Text;

            // Account actions
            if (nodeName == "🔑 Đổi mật khẩu")
                MessageBox.Show("Tính năng Đổi Mật Khẩu sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "👤 Thông tin")
                MessageBox.Show($"👤 Tài Khoản: {CurrentUser}\n💼 Vai trò: {CurrentRole}", "Thông Tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "🚪 Đăng xuất")
            {
                DialogResult result = MessageBox.Show("Bạn chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    this.Close();
            }
            // ===== HÓA ĐƠN & THANH TOÁN - FULL ADMIN =====
            else if (nodeName == "💳 Hóa Đơn & Thanh Toán" || nodeName == "📋 Xem danh sách")
            {
                // Mở form Hóa Đơn & Thanh Toán đầy đủ với 2 tabs
                using (var frm = new FrmInvoicePaymentUnified())
                    frm.ShowDialog(this);
            }
            else if (nodeName == "➕ Thêm")
            {
                // Mở form Hóa Đơn & Thanh Toán để thêm hóa đơn
                using (var frm = new FrmInvoicePaymentUnified())
                    frm.ShowDialog(this);
            }
            else if (nodeName == "✏️ Sửa")
            {
                // Mở form Hóa Đơn & Thanh Toán để sửa hóa đơn
                using (var frm = new FrmInvoicePaymentUnified())
                    frm.ShowDialog(this);
            }
            else if (nodeName == "🗑️ Xóa")
            {
                // Mở form Hóa Đơn & Thanh Toán để xóa hóa đơn
                using (var frm = new FrmInvoicePaymentUnified())
                    frm.ShowDialog(this);
            }
            else if (nodeName == "💵 Thu tiền")
            {
                // Mở form Hóa Đơn & Thanh Toán để ghi nhận thanh toán
                using (var frm = new FrmInvoicePaymentUnified())
                    frm.ShowDialog(this);
            }
            else if (nodeName == "📅 Tạo tháng")
            {
                // Mở form Hóa Đơn & Thanh Toán để tạo hóa đơn tháng
                using (var frm = new FrmInvoicePaymentUnified())
                    frm.ShowDialog(this);
            }
            else if (nodeName == "📊 Xuất CSV")
            {
                // Mở form Hóa Đơn & Thanh Toán để xuất CSV
                using (var frm = new FrmInvoicePaymentUnified())
                    frm.ShowDialog(this);
            }
            else if (nodeName == "🔄 Tải lại")
            {
                // Mở form Hóa Đơn & Thanh Toán để tải lại dữ liệu
                using (var frm = new FrmInvoicePaymentUnified())
                    frm.ShowDialog(this);
            }
            else
            {
                MessageBox.Show($"📋 {nodeName}\n\nTính năng này sẽ được triển khai sớm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
