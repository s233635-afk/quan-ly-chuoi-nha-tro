using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmForgotPassword : Form
    {
        private readonly UserBLL _userBll = new UserBLL();

        public FrmForgotPassword()
        {
            InitializeComponent();
        }

        private void FrmForgotPassword_Load(object sender, EventArgs e)
        {
            ApplyModernStyling();
        }

        private void ApplyModernStyling()
        {
            BackColor = Color.FromArgb(245, 245, 245);

            foreach (Control ctrl in GetAllControls(this))
            {
                if (ctrl is TextBox textBox)
                {
                    textBox.BackColor = Color.White;
                    textBox.ForeColor = Color.FromArgb(33, 37, 41);
                    textBox.Font = new Font("Segoe UI", 11F);
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            btnReset.Cursor = Cursors.Hand;
            btnCancel.Cursor = Cursors.Hand;

            chkShowPassword.Checked = false;
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                bool show = chkShowPassword.Checked;
                txtNewPass.UseSystemPasswordChar = !show;
                txtConfirm.UseSystemPasswordChar = !show;
            };

            txtUser.Focus();
        }

        private static Control[] GetAllControls(Control root)
        {
            if (root == null) return Array.Empty<Control>();
            return root.Controls.Cast<Control>()
                .SelectMany(c => GetAllControls(c).Prepend(c))
                .ToArray();
        }

        private async void btnReset_Click(object sender, EventArgs e)
        {
            btnReset.Enabled = false;
            btnCancel.Enabled = false;

            try
            {
                string username = (txtUser.Text ?? string.Empty).Trim();
                string fullName = (txtFullName.Text ?? string.Empty).Trim();
                string email = (txtEmail.Text ?? string.Empty).Trim();
                string phone = (txtPhone.Text ?? string.Empty).Trim();
                string newPass = (txtNewPass.Text ?? string.Empty).Trim();
                string confirm = (txtConfirm.Text ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirm))
                {
                    MessageBox.Show("Vui lòng nhập Tên đăng nhập, Mật khẩu mới và Nhập lại mật khẩu.", "Thiếu thông tin",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!string.Equals(newPass, confirm, StringComparison.Ordinal))
                {
                    MessageBox.Show("Mật khẩu nhập lại không khớp.", "Không hợp lệ",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await _userBll.ResetPasswordAsync(username, fullName, email, phone, newPass);

                MessageBox.Show("Đặt lại mật khẩu thành công! Bạn có thể đăng nhập lại.", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể đặt lại mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnReset.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

