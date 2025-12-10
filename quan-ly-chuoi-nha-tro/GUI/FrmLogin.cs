using System;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmLogin : Form
    {
        private UserBLL userBLL = new UserBLL();

        public FrmLogin()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
            ApplyModernStyling();
        }

        private void ApplyModernStyling()
        {
            // Form styling
            this.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
            
            // Username field
            txtUser.BackColor = System.Drawing.Color.White;
            txtUser.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
            txtUser.Font = new System.Drawing.Font("Segoe UI", 11F);
            txtUser.BorderStyle = BorderStyle.FixedSingle;
            
            // Password field
            txtPass.BackColor = System.Drawing.Color.White;
            txtPass.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
            txtPass.Font = new System.Drawing.Font("Segoe UI", 11F);
            txtPass.BorderStyle = BorderStyle.FixedSingle;
            txtPass.UseSystemPasswordChar = true;

            // Login button
            btnLogin.BackColor = System.Drawing.Color.FromArgb(0, 120, 212);
            btnLogin.ForeColor = System.Drawing.Color.White;
            btnLogin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;

            // Focus on username field
            txtUser.Focus();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            btnLogin.Text = "⏳ Đang kiểm tra...";

            try
            {
                string user = txtUser.Text.Trim();
                string pass = txtPass.Text.Trim();

                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                {
                    MessageBox.Show("Vui lòng nhập tài khoản và mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get full name and role
                string fullName = await userBLL.DangNhap(user, pass);
                int roleId = await userBLL.GetUserRoleAsync(user);
                
                MessageBox.Show($"✅ Xin chào {fullName}!", "Đăng nhập thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Navigate to appropriate form based on role
                if (roleId == 1) // Admin
                {
                    FrmAdminDashboard adminForm = new FrmAdminDashboard(user, 1);
                    this.Hide();
                    adminForm.ShowDialog();
                    this.Show();
                }
                else if (roleId == 2) // Staff
                {
                    FrmMain formMain = new FrmMain(user, "Nhân Viên");
                    this.Hide();
                    formMain.ShowDialog();
                    this.Show();
                }

                // Clear fields
                txtUser.Text = "";
                txtPass.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ {ex.Message}", "Lỗi Đăng Nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "🔓 Đăng Nhập";
            }
        }

        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick();
                e.Handled = true;
            }
        }
    }
}
