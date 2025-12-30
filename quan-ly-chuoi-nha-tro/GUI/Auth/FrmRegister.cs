using System;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

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

            foreach (Control ctrl in GetAllControls(this))
            {
                if (ctrl is TextBox textBox)
                {
                    textBox.BackColor = System.Drawing.Color.White;
                    textBox.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
                    textBox.Font = new System.Drawing.Font("Segoe UI", 11F);
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            if (btnRegister != null)
            {
                btnRegister.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
                btnRegister.ForeColor = System.Drawing.Color.White;
                btnRegister.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
                btnRegister.FlatStyle = FlatStyle.Flat;
                btnRegister.FlatAppearance.BorderSize = 0;
                btnRegister.Cursor = Cursors.Hand;
            }

            if (chkShowPassword != null)
            {
                chkShowPassword.Checked = false;
                chkShowPassword.CheckedChanged += (s, e) =>
                {
                    bool show = chkShowPassword.Checked;
                    if (txtPass != null) txtPass.UseSystemPasswordChar = !show;
                    if (txtConfirmPass != null) txtConfirmPass.UseSystemPasswordChar = !show;
                };
            }

            if (txtUser != null)
                txtUser.Focus();
        }

        private static Control[] GetAllControls(Control root)
        {
            if (root == null) return Array.Empty<Control>();
            return root.Controls.Cast<Control>()
                .SelectMany(c => GetAllControls(c).Prepend(c))
                .ToArray();
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            btnRegister.Enabled = false;
            btnRegister.Text = "Đang xử lý...";

            try
            {
                string user = (txtUser.Text ?? string.Empty).Trim();
                string pass = (txtPass.Text ?? string.Empty).Trim();
                string confirm = (txtConfirmPass.Text ?? string.Empty).Trim();
                string name = (txtName.Text ?? string.Empty).Trim();
                string email = (txtEmail.Text ?? string.Empty).Trim();
                string phone = (txtPhone.Text ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(confirm) || string.IsNullOrWhiteSpace(name))
                {
                    ToastNotification.Warning("Vui lòng điền đầy đủ thông tin!");
                    return;
                }

                if (user.Length < 3)
                {
                    ToastNotification.Warning("Tên đăng nhập phải có ít nhất 3 ký tự!");
                    return;
                }

                if (pass.Length < 6)
                {
                    ToastNotification.Warning("Mật khẩu phải có ít nhất 6 ký tự!");
                    return;
                }

                if (!string.Equals(pass, confirm, StringComparison.Ordinal))
                {
                    ToastNotification.Warning("Mật khẩu nhập lại không khớp!");
                    return;
                }

                if (name.Length < 2)
                {
                    ToastNotification.Warning("Họ tên phải có ít nhất 2 ký tự!");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(email) && (!email.Contains("@") || !email.Contains(".")))
                {
                    ToastNotification.Warning("Email không hợp lệ!");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    string digits = new string(phone.Where(char.IsDigit).ToArray());
                    if (digits.Length < 8 || digits.Length > 15)
                    {
                        ToastNotification.Warning("Số điện thoại không hợp lệ!");
                        return;
                    }
                }

                bool isSuccess = await userBLL.DangKy(user, pass, name, email, phone);
                if (isSuccess)
                {
                    ToastNotification.Success("Đăng ký thành công! Bạn có thể đăng nhập ngay.");
                    txtUser.Text = "";
                    txtPass.Text = "";
                    txtConfirmPass.Text = "";
                    txtName.Text = "";
                    txtEmail.Text = "";
                    txtPhone.Text = "";
                    Close();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "Register", "Lỗi đăng ký");
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
