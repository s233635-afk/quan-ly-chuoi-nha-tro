using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

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

        private DataTable _branchTable;

        private TextBox txtUsername;
        private TextBox txtFullName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private ComboBox cboBranch;
        private CheckBox chkActive;
        private TextBox txtPassword;
        private TextBox txtConfirm;

        private Button btnSave;
        private Button btnCancel;

        public FrmStaffEditor(AdminDataBLL bll, DataRow row = null)
        {
            _bll = bll;
            _row = row;
            _isEdit = row != null;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = _isEdit ? "Sửa nhân viên" : "Thêm nhân viên";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(680, 460);
            BackColor = Color.White;

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White
            };

            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 18, 18, 10),
                AutoScroll = true,
                BackColor = Color.White
            };

            int labelWidth = 190;
            int inputWidth = 420;
            int top = 10;
            int left = 6;
            int line = 34;

            Label MakeLabel(string text, int y) => new Label
            {
                Text = text,
                Location = new Point(left, y),
                Width = labelWidth,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Control MakeInput(Control ctl, int y)
            {
                ctl.Location = new Point(left + labelWidth, y);
                ctl.Width = inputWidth;
                return ctl;
            }

            txtUsername = new TextBox();
            txtFullName = new TextBox();
            txtEmail = new TextBox();
            txtPhone = new TextBox();
            cboBranch = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            chkActive = new CheckBox { Text = "Kích hoạt tài khoản", AutoSize = true, Checked = true };
            txtPassword = new TextBox { UseSystemPasswordChar = true };
            txtConfirm = new TextBox { UseSystemPasswordChar = true };

            pnlBody.Controls.Add(MakeLabel("Tên đăng nhập (*)", top));
            pnlBody.Controls.Add(MakeInput(txtUsername, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Họ tên (*)", top));
            pnlBody.Controls.Add(MakeInput(txtFullName, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Email", top));
            pnlBody.Controls.Add(MakeInput(txtEmail, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Số điện thoại", top));
            pnlBody.Controls.Add(MakeInput(txtPhone, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Chi nhánh", top));
            pnlBody.Controls.Add(MakeInput(cboBranch, top));
            top += line;

            var pnlActive = new Panel { Location = new Point(left + labelWidth, top), Width = inputWidth, Height = 26, BackColor = Color.Transparent };
            chkActive.Parent = pnlActive;
            chkActive.Location = new Point(0, 3);
            pnlBody.Controls.Add(MakeLabel("Trạng thái", top));
            pnlBody.Controls.Add(pnlActive);
            top += line;

            pnlBody.Controls.Add(MakeLabel(_isEdit ? "Mật khẩu mới (tùy chọn)" : "Mật khẩu (*)", top));
            pnlBody.Controls.Add(MakeInput(txtPassword, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel(_isEdit ? "Xác nhận (nếu đổi)" : "Xác nhận mật khẩu (*)", top));
            pnlBody.Controls.Add(MakeInput(txtConfirm, top));

            btnCancel = new ModernButton
            {
                Text = "Hủy",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BaseColor = Color.White,
                BackColor = Color.Transparent,
                ForeColor = Color.Black
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            btnSave = new ModernButton
            {
                Text = "Lưu",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BaseColor = Color.FromArgb(0, 122, 204),
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveAsync();

            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSave);
            btnCancel.Location = new Point(pnlBottom.Width - btnCancel.Width - 12, 12);
            btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            pnlBottom.Resize += (s, e) =>
            {
                btnCancel.Location = new Point(pnlBottom.Width - btnCancel.Width - 12, 12);
                btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            };

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);

            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Load += async (s, e) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            await LoadBranchesAsync();

            if (_isEdit)
                LoadDataFromRow();
        }

        private async System.Threading.Tasks.Task LoadBranchesAsync()
        {
            try
            {
                var dt = await _bll.GetBranchesAsync();
                _branchTable = new DataTable();
                _branchTable.Columns.Add("BranchId", typeof(int));
                _branchTable.Columns.Add("BranchName", typeof(string));
                _branchTable.Rows.Add(0, "Không chọn");

                if (dt != null && dt.Columns.Contains("BranchId") && dt.Columns.Contains("BranchName"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int id = 0;
                        try { id = Convert.ToInt32(r["BranchId"]); } catch { }
                        string name = r["BranchName"]?.ToString();
                        _branchTable.Rows.Add(id, name);
                    }
                }

                cboBranch.DataSource = _branchTable;
                cboBranch.DisplayMember = "BranchName";
                cboBranch.ValueMember = "BranchId";
                cboBranch.SelectedValue = 0;
            }
            catch
            {
                cboBranch.Items.Clear();
                cboBranch.Items.Add("Không chọn");
                cboBranch.SelectedIndex = 0;
            }
        }

        private void LoadDataFromRow()
        {
            txtUsername.Text = ReadString(_row, "UserName", "Username", "User");
            txtUsername.ReadOnly = true;
            txtFullName.Text = ReadString(_row, "FullName");
            txtEmail.Text = ReadString(_row, "Email");
            txtPhone.Text = ReadString(_row, "Phone");

            chkActive.Checked = ReadBool(_row, "IsActive") ?? true;

            int? branchId = TryReadIntNullable(_row, "BranchId");
            if (branchId.HasValue)
            {
                try { cboBranch.SelectedValue = branchId.Value; } catch { }
            }
            else
            {
                try { cboBranch.SelectedValue = 0; } catch { }
            }
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
            int? branchId = null;
            try
            {
                if (cboBranch.SelectedValue != null && int.TryParse(cboBranch.SelectedValue.ToString(), out var bid) && bid > 0)
                    branchId = bid;
            }
            catch { }

            if (string.IsNullOrWhiteSpace(username))
            {
                ToastNotification.Warning("Tên đăng nhập không được trống");
                return;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ToastNotification.Warning("Họ tên không được trống");
                return;
            }

            if (!_isEdit || !string.IsNullOrWhiteSpace(password) || !string.IsNullOrWhiteSpace(confirm))
            {
                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    ToastNotification.Warning("Mật khẩu phải có ít nhất 6 ký tự");
                    return;
                }

                if (password != confirm)
                {
                    ToastNotification.Warning("Xác nhận mật khẩu không khớp");
                    return;
                }
            }

            try
            {
                if (_isEdit)
                {
                    int id = Convert.ToInt32(_row["UserId"]);
                    string newPassword = string.IsNullOrWhiteSpace(password) ? null : password;
                    await _bll.UpdateStaffUserAsync(id, fullName, email, phone, branchId, isActive, newPassword);
                }
                else
                {
                    await _bll.AddStaffUserAsync(username, password, fullName, email, phone, branchId, isActive);
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "SaveStaff", "Lỗi lưu nhân viên");
            }
        }

        private static string ReadString(DataRow row, params string[] cols)
        {
            if (row?.Table == null) return null;
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v != null && v != DBNull.Value) return v.ToString();
                }
            }
            return null;
        }

        private static int ReadInt(DataRow row, params string[] cols)
        {
            if (row?.Table == null) return 0;
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (int.TryParse(v.ToString(), out var i)) return i;
                    try { return Convert.ToInt32(v); } catch { }
                }
            }
            return 0;
        }

        private static int? TryReadIntNullable(DataRow row, params string[] cols)
        {
            int i = ReadInt(row, cols);
            return i > 0 ? (int?)i : null;
        }

        private static bool? ReadBool(DataRow row, params string[] cols)
        {
            if (row?.Table == null) return null;
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (bool.TryParse(v.ToString(), out var b)) return b;
                    try { return Convert.ToBoolean(v); } catch { }
                }
            }
            return null;
        }
    }
}
