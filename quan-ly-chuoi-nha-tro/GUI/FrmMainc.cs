using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.Text = $"Hệ Thống Quản Lý Chuỗi Nhà Trọ - {CurrentUser}";
            this.WindowState = FormWindowState.Maximized;
            lblUserInfo.Text = $"👤 {CurrentUser} ({CurrentRole})";
            InitializeTreeMenu();
            ExpandRootNodes();
        }

        private void InitializeTreeMenu()
        {
            treeViewMenu.Nodes.Clear();
            treeViewMenu.ImageList = null;

            // ========== NHÓM 1: QUẢN LÝ (13 TÍNH NĂNG ADMIN) ==========
            TreeNode nodeQuanLy = new TreeNode("📋 QUẢN LÝ");

            TreeNode node1 = new TreeNode("1. Quản Lý Chi Nhánh");
            node1.Nodes.Add(new TreeNode("Thêm/Sửa/Xóa Chi Nhánh"));
            node1.Nodes.Add(new TreeNode("Cấu Hình Thông Tin Chi Nhánh"));
            node1.Nodes.Add(new TreeNode("Quản Lý Dãy/Khu"));

            TreeNode node2 = new TreeNode("2. Quản Lý Phòng");
            node2.Nodes.Add(new TreeNode("Tạo/Sửa/Xóa Phòng"));
            node2.Nodes.Add(new TreeNode("Cài Đặt Loại Phòng"));
            node2.Nodes.Add(new TreeNode("Quản Lý Trạng Thái Phòng"));
            node2.Nodes.Add(new TreeNode("Xem Sơ Đồ Phòng"));

            TreeNode node3 = new TreeNode("3. Quản Lý Nhân Viên");
            node3.Nodes.Add(new TreeNode("Tạo Tài Khoản Nhân Viên"));
            node3.Nodes.Add(new TreeNode("Gán Chi Nhánh & Phân Quyền"));
            node3.Nodes.Add(new TreeNode("Theo Dõi Hoạt Động"));

            TreeNode node4 = new TreeNode("4. Quản Lý Khách Thuê");
            node4.Nodes.Add(new TreeNode("Quản Lý Hồ Sơ Khách Thuê"));
            node4.Nodes.Add(new TreeNode("Quản Lý Tạm Trú - Tạm Vắng"));
            node4.Nodes.Add(new TreeNode("Check-In/Check-Out (Admin)"));
            node4.Nodes.Add(new TreeNode("Lịch Sử Phòng"));

            TreeNode node5 = new TreeNode("5. Quản Lý Hợp Đồng");
            node5.Nodes.Add(new TreeNode("Tạo Hợp Đồng (Auto-Fill)"));
            node5.Nodes.Add(new TreeNode("Xuất & Quản Lý Hợp Đồng"));
            node5.Nodes.Add(new TreeNode("Chấm Dứt Hợp Đồng"));

            TreeNode node6 = new TreeNode("6. Đặt Phòng - Đặt Cọc");
            node6.Nodes.Add(new TreeNode("Nhận Lead & Giữ Phòng"));
            node6.Nodes.Add(new TreeNode("Thu Tiền Cọc & Chuyển Trạng Thái"));
            node6.Nodes.Add(new TreeNode("Hủy Đặt Phòng"));

            TreeNode node7 = new TreeNode("7. Quản Lý Điện - Nước - Dịch Vụ");
            node7.Nodes.Add(new TreeNode("Nhập Chỉ Số & Tính Tiền (Admin)"));
            node7.Nodes.Add(new TreeNode("Quản Lý Dịch Vụ & Phí Phát Sinh"));

            TreeNode node8 = new TreeNode("8. Hóa Đơn - Thanh Toán");
            node8.Nodes.Add(new TreeNode("Tạo Hóa Đơn Tháng (Tự Động)"));
            node8.Nodes.Add(new TreeNode("Thu Tiền & Quản Lý Công Nợ"));

            TreeNode node9 = new TreeNode("9. Bảo Trì - Sự Cố - Vệ Sinh");
            node9.Nodes.Add(new TreeNode("Quản Lý Yêu Cầu & Ticket (Admin)"));
            node9.Nodes.Add(new TreeNode("Phân Công & Lịch Vệ Sinh"));

            TreeNode node10 = new TreeNode("10. Quản Lý Tài Sản - Vật Tư");
            node10.Nodes.Add(new TreeNode("Quản Lý Danh Mục Tài Sản"));
            node10.Nodes.Add(new TreeNode("Hư Hỏng - Thay Thế"));

            TreeNode node11 = new TreeNode("11. Báo Cáo - Thống Kê");
            node11.Nodes.Add(new TreeNode("Báo Cáo Tổng Hợp"));

            TreeNode node12 = new TreeNode("12. Thông Báo - Nhắc Lịch");
            node12.Nodes.Add(new TreeNode("Hệ Thống Nhắc Nhở Tự Động"));
            node12.Nodes.Add(new TreeNode("Thông Báo Nội Bộ"));

            TreeNode node13 = new TreeNode("13. Cấu Hình Hệ Thống");
            node13.Nodes.Add(new TreeNode("Thiết Lập Tham Số Mặc Định"));

            nodeQuanLy.Nodes.Add(node1);
            nodeQuanLy.Nodes.Add(node2);
            nodeQuanLy.Nodes.Add(node3);
            nodeQuanLy.Nodes.Add(node4);
            nodeQuanLy.Nodes.Add(node5);
            nodeQuanLy.Nodes.Add(node6);
            nodeQuanLy.Nodes.Add(node7);
            nodeQuanLy.Nodes.Add(node8);
            nodeQuanLy.Nodes.Add(node9);
            nodeQuanLy.Nodes.Add(node10);
            nodeQuanLy.Nodes.Add(node11);
            nodeQuanLy.Nodes.Add(node12);
            nodeQuanLy.Nodes.Add(node13);

            // ========== NHÓM 2: NHÂN VIÊN (8 TÍNH NĂNG) ==========
            TreeNode nodeNhanVien = new TreeNode("👥 NHÂN VIÊN");

            TreeNode nv1 = new TreeNode("2.1 Quản Lý Phòng");
            nv1.Nodes.Add(new TreeNode("Xem Trạng Thái Phòng"));
            nv1.Nodes.Add(new TreeNode("Cập Nhật Trạng Thái"));

            TreeNode nv2 = new TreeNode("2.2 Quản Lý Khách Thuê");
            nv2.Nodes.Add(new TreeNode("Xem Hồ Sơ Khách"));
            nv2.Nodes.Add(new TreeNode("Check-In / Check-Out"));
            nv2.Nodes.Add(new TreeNode("Cập Nhật Thông Tin Khách"));

            TreeNode nv3 = new TreeNode("2.3 Hợp Đồng");
            nv3.Nodes.Add(new TreeNode("Tạo Hợp Đồng Mới"));
            nv3.Nodes.Add(new TreeNode("Gia Hạn / Kết Thúc"));

            TreeNode nv4 = new TreeNode("2.4 Điện - Nước - Dịch Vụ");
            nv4.Nodes.Add(new TreeNode("Nhập Chỉ Số"));
            nv4.Nodes.Add(new TreeNode("Cập Nhật Dịch Vụ"));

            TreeNode nv5 = new TreeNode("2.5 Thanh Toán");
            nv5.Nodes.Add(new TreeNode("Thu Tiền & Xác Nhận"));
            nv5.Nodes.Add(new TreeNode("Gửi Hóa Đơn"));

            TreeNode nv6 = new TreeNode("2.6 Bảo Trì - Sự Cố");
            nv6.Nodes.Add(new TreeNode("Tạo Ticket Sửa Chữa"));
            nv6.Nodes.Add(new TreeNode("Cập Nhật & Xác Nhận"));

            TreeNode nv7 = new TreeNode("2.7 Tài Sản Phòng");
            nv7.Nodes.Add(new TreeNode("Kiểm Tra Tài Sản"));
            nv7.Nodes.Add(new TreeNode("Báo Cáo Hư Hỏng"));

            TreeNode nv8 = new TreeNode("2.8 Báo Cáo Chi Nhánh");
            nv8.Nodes.Add(new TreeNode("Xem Báo Cáo Nội Bộ"));

            nodeNhanVien.Nodes.Add(nv1);
            nodeNhanVien.Nodes.Add(nv2);
            nodeNhanVien.Nodes.Add(nv3);
            nodeNhanVien.Nodes.Add(nv4);
            nodeNhanVien.Nodes.Add(nv5);
            nodeNhanVien.Nodes.Add(nv6);
            nodeNhanVien.Nodes.Add(nv7);
            nodeNhanVien.Nodes.Add(nv8);

            // ========== NHÓM 3: TÀI KHOẢN ==========
            TreeNode nodeSeparator = new TreeNode("─────────────────────");
            
            TreeNode nodeTaiKhoan = new TreeNode("⚙️ CÀI ĐẶT & TÀI KHOẢN");
            nodeTaiKhoan.Nodes.Add(new TreeNode("Đổi Mật Khẩu"));
            nodeTaiKhoan.Nodes.Add(new TreeNode("Thông Tin Tài Khoản"));
            nodeTaiKhoan.Nodes.Add(new TreeNode("Đăng Xuất"));

            // Thêm tất cả vào TreeView
            treeViewMenu.Nodes.Add(nodeQuanLy);
            treeViewMenu.Nodes.Add(nodeNhanVien);
            treeViewMenu.Nodes.Add(nodeSeparator);
            treeViewMenu.Nodes.Add(nodeTaiKhoan);
        }

        private void ExpandRootNodes()
        {
            foreach (TreeNode node in treeViewMenu.Nodes)
            {
                if (node.Text != "─────────────────────")
                {
                    node.Expand();
                }
            }
        }

        private void treeViewMenu_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string nodeName = e.Node.Text;

            // ========== ADMIN: QUẢN LÝ CHI NHÁNH ==========
            if (nodeName == "Thêm/Sửa/Xóa Chi Nhánh")
                MessageBox.Show("Quản Lý Chi Nhánh - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Cấu Hình Thông Tin Chi Nhánh")
                MessageBox.Show("Cấu Hình Thông Tin - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Quản Lý Dãy/Khu")
                MessageBox.Show("Quản Lý Dãy/Khu - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: QUẢN LÝ PHÒNG ==========
            else if (nodeName == "Tạo/Sửa/Xóa Phòng")
                MessageBox.Show("Quản Lý Phòng (Admin) - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Cài Đặt Loại Phòng")
                MessageBox.Show("Cài Đặt Loại Phòng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Quản Lý Trạng Thái Phòng")
                MessageBox.Show("Quản Lý Trạng Thái Phòng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Xem Sơ Đồ Phòng")
                MessageBox.Show("Xem Sơ Đồ Phòng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: QUẢN LÝ NHÂN VIÊN ==========
            else if (nodeName == "Tạo Tài Khoản Nhân Viên")
                MessageBox.Show("Tạo Tài Khoản - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Gán Chi Nhánh & Phân Quyền")
                MessageBox.Show("Gán Chi Nhánh - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Theo Dõi Hoạt Động")
                MessageBox.Show("Theo Dõi Hoạt Động - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: QUẢN LÝ KHÁCH THUÊ ==========
            else if (nodeName == "Quản Lý Hồ Sơ Khách Thuê")
                MessageBox.Show("Quản Lý Hồ Sơ (Admin) - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Quản Lý Tạm Trú - Tạm Vắng")
                MessageBox.Show("Quản Lý Tạm Trú - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Check-In/Check-Out (Admin)")
                MessageBox.Show("Check-In/Check-Out (Admin) - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Lịch Sử Phòng")
                MessageBox.Show("Lịch Sử Phòng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: QUẢN LÝ HỢP ĐỒNG ==========
            else if (nodeName == "Tạo Hợp Đồng (Auto-Fill)")
                MessageBox.Show("Tạo Hợp Đồng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Xuất & Quản Lý Hợp Đồng")
                MessageBox.Show("Quản Lý Hợp Đồng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Chấm Dứt Hợp Đồng")
                MessageBox.Show("Chấm Dứt Hợp Đồng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: ĐẶT PHÒNG ==========
            else if (nodeName == "Nhận Lead & Giữ Phòng")
                MessageBox.Show("Nhận Lead - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Thu Tiền Cọc & Chuyển Trạng Thái")
                MessageBox.Show("Thu Tiền Cọc - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Hủy Đặt Phòng")
                MessageBox.Show("Hủy Đặt Phòng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: ĐIỆN - NƯỚC - DỊCH VỤ ==========
            else if (nodeName == "Nhập Chỉ Số & Tính Tiền (Admin)")
                MessageBox.Show("Nhập Chỉ Số (Admin) - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Quản Lý Dịch Vụ & Phí Phát Sinh")
                MessageBox.Show("Quản Lý Dịch Vụ - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: HÓA ĐƠN - THANH TOÁN ==========
            else if (nodeName == "Tạo Hóa Đơn Tháng (Tự Động)")
                MessageBox.Show("Tạo Hóa Đơn - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Thu Tiền & Quản Lý Công Nợ")
                MessageBox.Show("Quản Lý Công Nợ - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: BẢO TRÌ - SỰ CỐ ==========
            else if (nodeName == "Quản Lý Yêu Cầu & Ticket (Admin)")
                MessageBox.Show("Quản Lý Ticket (Admin) - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Phân Công & Lịch Vệ Sinh")
                MessageBox.Show("Phân Công - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: TÀI SẢN - VẬT TƯ ==========
            else if (nodeName == "Quản Lý Danh Mục Tài Sản")
                MessageBox.Show("Quản Lý Tài Sản - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Hư Hỏng - Thay Thế")
                MessageBox.Show("Hư Hỏng - Thay Thế - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: BÁO CÁO - THỐNG KÊ ==========
            else if (nodeName == "Báo Cáo Tổng Hợp")
                MessageBox.Show("Báo Cáo Tổng Hợp - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: THÔNG BÁO - NHẮC LỊCH ==========
            else if (nodeName == "Hệ Thống Nhắc Nhở Tự Động")
                MessageBox.Show("Nhắc Nhở - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Thông Báo Nội Bộ")
                MessageBox.Show("Thông Báo - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== ADMIN: CẤU HÌNH HỆ THỐNG ==========
            else if (nodeName == "Thiết Lập Tham Số Mặc Định")
                MessageBox.Show("Cấu Hình - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== NHÂN VIÊN: TÍNH NĂNG 2.1 - QUẢN LÝ PHÒNG ==========
            else if (nodeName == "Xem Trạng Thái Phòng")
                MessageBox.Show("Xem Trạng Thái Phòng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Cập Nhật Trạng Thái" && e.Node.Parent.Text == "2.1 Quản Lý Phòng")
                MessageBox.Show("Cập Nhật Trạng Thái Phòng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== NHÂN VIÊN: TÍNH NĂNG 2.2 - QUẢN LÝ KHÁCH THUÊ ==========
            else if (nodeName == "Xem Hồ Sơ Khách")
                MessageBox.Show("Xem Hồ Sơ Khách Thuê - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Check-In / Check-Out")
                MessageBox.Show("Check-In / Check-Out - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Cập Nhật Thông Tin Khách")
                MessageBox.Show("Cập Nhật Thông Tin Khách - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== NHÂN VIÊN: TÍNH NĂNG 2.3 - HỢP ĐỒNG ==========
            else if (nodeName == "Tạo Hợp Đồng Mới")
                MessageBox.Show("Tạo Hợp Đồng Mới - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Gia Hạn / Kết Thúc")
                MessageBox.Show("Gia Hạn / Kết Thúc Hợp Đồng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== NHÂN VIÊN: TÍNH NĂNG 2.4 - ĐIỆN - NƯỚC - DỊCH VỤ ==========
            else if (nodeName == "Nhập Chỉ Số")
                MessageBox.Show("Nhập Chỉ Số Điện/Nước - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Cập Nhật Dịch Vụ")
                MessageBox.Show("Cập Nhật Dịch Vụ - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== NHÂN VIÊN: TÍNH NĂNG 2.5 - THANH TOÁN ==========
            else if (nodeName == "Thu Tiền & Xác Nhận")
                MessageBox.Show("Thu Tiền & Xác Nhận - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Gửi Hóa Đơn")
                MessageBox.Show("Gửi Hóa Đơn - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== NHÂN VIÊN: TÍNH NĂNG 2.6 - BẢO TRÌ - SỰ CỐ ==========
            else if (nodeName == "Tạo Ticket Sửa Chữa")
                MessageBox.Show("Tạo Ticket Sửa Chữa - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Cập Nhật & Xác Nhận")
                MessageBox.Show("Cập Nhật & Xác Nhận - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== NHÂN VIÊN: TÍNH NĂNG 2.7 - TÀI SẢN PHÒNG ==========
            else if (nodeName == "Kiểm Tra Tài Sản")
                MessageBox.Show("Kiểm Tra Tài Sản - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Báo Cáo Hư Hỏng")
                MessageBox.Show("Báo Cáo Hư Hỏng - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== NHÂN VIÊN: TÍNH NĂNG 2.8 - BÁO CÁO CHI NHÁNH ==========
            else if (nodeName == "Xem Báo Cáo Nội Bộ")
                MessageBox.Show("Xem Báo Cáo Nội Bộ - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== MENU TÀI KHOẢN ==========
            else if (nodeName == "Đổi Mật Khẩu")
                MessageBox.Show("Đổi Mật Khẩu - Chức năng sẽ được thêm", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Thông Tin Tài Khoản")
                MessageBox.Show($"Tài Khoản: {CurrentUser}\nVai Trò: {CurrentRole}", "Thông Tin Tài Khoản", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (nodeName == "Đăng Xuất")
            {
                DialogResult result = MessageBox.Show("Bạn chắc chắn muốn đăng xuất?", "Xác nhận Đăng Xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    this.Close();
                }
            }
        }
    }
}
