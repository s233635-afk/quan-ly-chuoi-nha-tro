using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Form thêm/sửa nhân viên và tạo tài khoản đăng nhập (RoleId = 2).
    /// </summary>
    public class FrmStaffEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _row;
        private readonly bool _isEdit;

        private TextBox txtUsername, txtFullName, txtEmail, txtPhone, txtPassword, txtConfirm;
        private CheckBox chkActive;
        private Label lblPassword, lblConfirm;
        private Button btnSave, btnCancel;

        public FrmStaffEditor(AdminDataBLL bll, DataRow row = null)
        {
            _bll = bll;
            _row = row;
            _isEdit = row != null;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = _isEdit ? "Sửa nhân viên" : "Thêm nhân viên";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(420, 360);

            Label lblUsername = new Label { Text = "Tên đăng nhập", AutoSize = true, Location = new Point(20, 20) };
            txtUsername = new TextBox { Location = new Point(150, 16), Width = 230 };

            Label lblFullName = new Label { Text = "Họ tên", AutoSize = true, Location = new Point(20, 60) };
            txtFullName = new TextBox { Location = new Point(150, 56), Width = 230 };

            Label lblEmail = new Label { Text = "Email", AutoSize = true, Location = new Point(20, 100) };
            txtEmail = new TextBox { Location = new Point(150, 96), Width = 230 };

            Label lblPhone = new Label { Text = "SĐT", AutoSize = true, Location = new Point(20, 140) };
            txtPhone = new TextBox { Location = new Point(150, 136), Width = 230 };

            lblPassword = new Label { Text = _isEdit ? "Mật khẩu mới (tùy chọn)" : "Mật khẩu", AutoSize = true, Location = new Point(20, 180) };
            txtPassword = new TextBox { Location = new Point(150, 176), Width = 230, UseSystemPasswordChar = true };

            lblConfirm = new Label { Text = _isEdit ? "Xác nhận (nếu đổi)" : "Xác nhận mật khẩu", AutoSize = true, Location = new Point(20, 220) };
            txtConfirm = new TextBox { Location = new Point(150, 216), Width = 230, UseSystemPasswordChar = true };

            chkActive = new CheckBox { Text = "Kích hoạt tài khoản", Location = new Point(150, 248), AutoSize = true, Checked = true };

            btnSave = new Button { Text = "Lưu", Width = 90, Location = new Point(150, 290) };
            btnCancel = new Button { Text = "Hủy", Width = 90, Location = new Point(250, 290) };

            btnSave.Click += async (s, e) => await SaveAsync();
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblFullName);
            this.Controls.Add(txtFullName);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblPhone);
            this.Controls.Add(txtPhone);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblConfirm);
            this.Controls.Add(txtConfirm);
            this.Controls.Add(chkActive);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            if (_isEdit)
            {
                LoadDataFromRow();
            }
        }

        private void LoadDataFromRow()
        {
            txtUsername.Text = _row["UserName"]?.ToString();
            txtUsername.ReadOnly = true;
            txtFullName.Text = _row["FullName"]?.ToString();
            txtEmail.Text = _row["Email"]?.ToString();
            txtPhone.Text = _row["Phone"]?.ToString();

            bool isActive = false;
            bool.TryParse(_row["IsActive"]?.ToString(), out isActive);
            chkActive.Checked = isActive;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string username = txtUsername.Text.Trim();
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirm.Text;
            bool isActive = chkActive.Checked;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Tên đăng nhập không được trống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Họ tên không được trống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_isEdit || !string.IsNullOrWhiteSpace(password) || !string.IsNullOrWhiteSpace(confirm))
            {
                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (password != confirm)
                {
                    MessageBox.Show("Xác nhận mật khẩu không khớp.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                if (_isEdit)
                {
                    int id = Convert.ToInt32(_row["UserId"]);
                    string newPassword = string.IsNullOrWhiteSpace(password) ? null : password;
                    await _bll.UpdateStaffUserAsync(id, fullName, email, phone, isActive, newPassword);
                }
                else
                {
                    await _bll.AddStaffUserAsync(username, password, fullName, email, phone, isActive);
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
