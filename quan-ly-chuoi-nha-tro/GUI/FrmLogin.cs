using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinformTiDB.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmLogin : Form
    {
        private UserBLL userBLL = new UserBLL();
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            // 1. Khóa nút để tránh bấm nhiều lần gây treo
            btnLogin.Enabled = false;
            btnLogin.Text = "Đang kiểm tra...";

            try
            {
                string user = txtUser.Text.Trim();
                string pass = txtPass.Text.Trim();

                // 2. Gọi BLL xử lý (Async để không đơ màn hình)
                string fullName = await userBLL.DangNhap(user, pass);

                // 3. Nếu chạy đến dòng này nghĩa là thành công (không bị throw Exception)
                MessageBox.Show($"Xin chào {fullName}!", "Đăng nhập thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // --- CHUYỂN SANG FORM CHÍNH ---
                FrmMain formMain = new FrmMain(); // Giả sử bạn đã có FrmMain
                this.Hide(); // Ẩn form đăng nhập
                formMain.ShowDialog(); // Hiện form chính
                this.Show(); // Khi tắt form chính thì hiện lại form đăng nhập
            }
            catch (Exception ex)
            {
                // Hiện lỗi: Sai pass hoặc Lỗi mạng
                MessageBox.Show(ex.Message, "Lỗi Đăng Nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Mở lại nút
                btnLogin.Enabled = true;
                btnLogin.Text = "Đăng Nhập";
            }
        }

        // Sự kiện nút Thoát
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Mẹo nhỏ: Bấm Enter ở ô mật khẩu thì tự gọi nút Đăng nhập
        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick();
            }
        }
    }
    
}
