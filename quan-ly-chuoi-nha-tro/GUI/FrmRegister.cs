using System;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmRegister : Form
    {
        private readonly UserBLL userBLL = new UserBLL();

        public FrmRegister()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
        }

        private void FrmRegister_Load(object sender, EventArgs e)
        {
            ApplyModernStyling();
        }

        private void ApplyModernStyling()
        {
            BackColor = System.Drawing.Color.FromArgb(245, 245, 245);

            foreach (Control ctrl in Controls)
            {
                if (ctrl is TextBox textBox)
                {
                    textBox.BackColor = System.Drawing.Color.White;
                    textBox.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
                    textBox.Font = new System.Drawing.Font("Segoe UI", 11F);
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            if (Controls.Contains(btnRegister))
            {
                btnRegister.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
                btnRegister.ForeColor = System.Drawing.Color.White;
                btnRegister.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
                btnRegister.FlatStyle = FlatStyle.Flat;
                btnRegister.FlatAppearance.BorderSize = 0;
                btnRegister.Cursor = Cursors.Hand;
            }

            if (Controls.Contains(txtUser))
                txtUser.Focus();
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            btnRegister.Enabled = false;
            btnRegister.Text = "Đang xử lý...";

            try
            {
                string user = txtUser.Text.Trim();
                string pass = txtPass.Text.Trim();
                string name = txtName.Text.Trim();

                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (user.Length < 3)
                {
                    MessageBox.Show("Tên đăng nhập phải có ít nhất 3 ký tự!", "Không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (pass.Length < 6)
                {
                    MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (name.Length < 2)
                {
                    MessageBox.Show("Họ tên phải có ít nhất 2 ký tự!", "Không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool isSuccess = await userBLL.DangKy(user, pass, name);
                if (isSuccess)
                {
                    MessageBox.Show("Đăng ký thành công! Bạn có thể đăng nhập ngay.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtUser.Text = "";
                    txtPass.Text = "";
                    txtName.Text = "";
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text = "Đăng ký";
            }
        }

        private void lblLogin_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtPass_TextChanged(object sender, EventArgs e)
        {
            // Optional: bạn có thể thêm chỉ báo độ mạnh mật khẩu tại đây.
        }
    }
}

