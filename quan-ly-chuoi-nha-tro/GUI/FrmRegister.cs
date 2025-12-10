using System;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmRegister : Form
    {
        private UserBLL userBLL = new UserBLL();

        public FrmRegister()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void FrmRegister_Load(object sender, EventArgs e)
        {
            ApplyModernStyling();
        }

        private void ApplyModernStyling()
        {
            // Form styling
            this.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);

            // Text fields
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is TextBox textBox)
                {
                    textBox.BackColor = System.Drawing.Color.White;
                    textBox.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
                    textBox.Font = new System.Drawing.Font("Segoe UI", 11F);
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            // Register button
            if (Controls.Contains(btnRegister))
            {
                btnRegister.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
                btnRegister.ForeColor = System.Drawing.Color.White;
                btnRegister.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
                btnRegister.FlatStyle = FlatStyle.Flat;
                btnRegister.FlatAppearance.BorderSize = 0;
                btnRegister.Cursor = System.Windows.Forms.Cursors.Hand;
            }

            // Focus on username
            if (Controls.Contains(txtUser))
                txtUser.Focus();
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            btnRegister.Enabled = false;
            btnRegister.Text = "⏳ Đang xử lý...";

            try
            {
                string user = txtUser.Text.Trim();
                string pass = txtPass.Text.Trim();
                string name = txtName.Text.Trim();

                // Validation
                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("⚠️ Vui lòng điền đầy đủ thông tin!", "Lỗi Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (user.Length < 3)
                {
                    MessageBox.Show("⚠️ Tên tài khoản phải có ít nhất 3 ký tự!", "Lỗi Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (pass.Length < 6)
                {
                    MessageBox.Show("⚠️ Mật khẩu phải có ít nhất 6 ký tự!", "Lỗi Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (name.Length < 2)
                {
                    MessageBox.Show("⚠️ Tên người dùng phải có ít nhất 2 ký tự!", "Lỗi Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Register
                bool isSuccess = await userBLL.DangKy(user, pass, name);

                if (isSuccess)
                {
                    MessageBox.Show("✅ Đăng ký thành công!\nBạn có thể đăng nhập ngay bây giờ.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Clear and close
                    txtUser.Text = "";
                    txtPass.Text = "";
                    txtName.Text = "";
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi Đăng Ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text = "✅ Đăng Ký";
            }
        }

        private void lblLogin_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtPass_TextChanged(object sender, EventArgs e)
        {
            // Optional: Show password strength indicator
            if (Controls.Contains(lblPasswordStrength))
            {
                string password = txtPass.Text;
                if (password.Length == 0)
                {
                    lblPasswordStrength.Text = "";
                    lblPasswordStrength.ForeColor = System.Drawing.Color.Gray;
                }
                else if (password.Length < 6)
                {
                    lblPasswordStrength.Text = "⚠️ Yếu";
                    lblPasswordStrength.ForeColor = System.Drawing.Color.Red;
                }
                else if (password.Length < 10)
                {
                    lblPasswordStrength.Text = "⚡ Trung bình";
                    lblPasswordStrength.ForeColor = System.Drawing.Color.Orange;
                }
                else
                {
                    lblPasswordStrength.Text = "✓ Mạnh";
                    lblPasswordStrength.ForeColor = System.Drawing.Color.Green;
                }
            }
        }
    }
}
