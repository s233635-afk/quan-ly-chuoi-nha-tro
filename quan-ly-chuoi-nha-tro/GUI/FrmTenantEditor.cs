using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmTenantEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private TextBox txtFullName;
        private TextBox txtIdentity;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private DateTimePicker dtBirth;
        private TextBox txtAddress;
        private TextBox txtTempReg;
        private DateTimePicker dtTempFrom;
        private DateTimePicker dtTempTo;
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnCancel;

        public int? SavedTenantId { get; private set; }

        public FrmTenantEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            LoadExisting();
        }

        private void InitializeComponent()
        {
            this.Text = _existingRow == null ? "Thêm khách thuê" : "Cập nhật khách thuê";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(520, 430);

            int labelWidth = 140;
            int inputWidth = 320;
            int top = 20;
            int left = 20;
            int lineHeight = 30;

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

            txtFullName = new TextBox();
            txtIdentity = new TextBox();
            txtPhone = new TextBox();
            txtEmail = new TextBox();
            dtBirth = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            txtAddress = new TextBox { Multiline = true, Height = 60, ScrollBars = ScrollBars.Vertical };
            txtTempReg = new TextBox();
            dtTempFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            dtTempTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            chkActive = new CheckBox { Text = "Đang hoạt động", Checked = true, AutoSize = true };

            this.Controls.Add(MakeLabel("Họ tên (*)", top));
            this.Controls.Add(MakeInput(txtFullName, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("CMND/CCCD", top));
            this.Controls.Add(MakeInput(txtIdentity, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Điện thoại", top));
            this.Controls.Add(MakeInput(txtPhone, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Email", top));
            this.Controls.Add(MakeInput(txtEmail, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Ngày sinh", top));
            this.Controls.Add(MakeInput(dtBirth, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Địa chỉ", top));
            this.Controls.Add(MakeInput(txtAddress, top));
            top += 70;

            this.Controls.Add(MakeLabel("Tạm trú tại", top));
            this.Controls.Add(MakeInput(txtTempReg, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Ngày tạm trú", top));
            this.Controls.Add(MakeInput(dtTempFrom, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Hết hạn tạm trú", top));
            this.Controls.Add(MakeInput(dtTempTo, top));
            top += lineHeight;

            chkActive.Location = new Point(left + labelWidth, top);
            this.Controls.Add(chkActive);
            top += lineHeight + 10;

            btnSave = new Button
            {
                Text = "Lưu",
                Width = 100,
                Height = 32,
                Location = new Point(this.ClientSize.Width - 220, this.ClientSize.Height - 50),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnSave.Click += async (s, e) => await SaveAsync();

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 100,
                Height = 32,
                Location = new Point(this.ClientSize.Width - 110, this.ClientSize.Height - 50),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void LoadExisting()
        {
            if (_existingRow == null) return;

            txtFullName.Text = _existingRow["FullName"]?.ToString();
            txtIdentity.Text = _existingRow["IdentityCard"]?.ToString();
            txtPhone.Text = _existingRow["PhoneNumber"]?.ToString();
            txtEmail.Text = _existingRow["Email"]?.ToString();
            if (DateTime.TryParse(_existingRow["BirthDate"]?.ToString(), out var b))
            {
                dtBirth.Value = b;
                dtBirth.Checked = true;
            }
            else dtBirth.Checked = false;

            txtAddress.Text = _existingRow["Address"]?.ToString();
            txtTempReg.Text = _existingRow["TemporaryRegistration"]?.ToString();
            if (DateTime.TryParse(_existingRow["TemporaryRegistrationDate"]?.ToString(), out var t1))
            {
                dtTempFrom.Value = t1;
                dtTempFrom.Checked = true;
            }
            else dtTempFrom.Checked = false;

            if (DateTime.TryParse(_existingRow["TemporaryRegistrationExpiry"]?.ToString(), out var t2))
            {
                dtTempTo.Value = t2;
                dtTempTo.Checked = true;
            }
            else dtTempTo.Checked = false;

            if (bool.TryParse(_existingRow["IsActive"]?.ToString(), out var act))
            {
                chkActive.Checked = act;
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập Họ tên", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_existingRow == null)
                {
                    var newId = await _bll.AddTenantAsync(
                        txtFullName.Text.Trim(),
                        txtIdentity.Text.Trim(),
                        txtPhone.Text.Trim(),
                        txtEmail.Text.Trim(),
                        dtBirth.Checked ? (DateTime?)dtBirth.Value.Date : null,
                        txtAddress.Text.Trim(),
                        txtTempReg.Text.Trim(),
                        dtTempFrom.Checked ? (DateTime?)dtTempFrom.Value.Date : null,
                        dtTempTo.Checked ? (DateTime?)dtTempTo.Value.Date : null,
                        chkActive.Checked
                    );
                    SavedTenantId = newId;
                }
                else
                {
                    int id = Convert.ToInt32(_existingRow["TenantId"]);
                    await _bll.UpdateTenantAsync(
                        id,
                        txtFullName.Text.Trim(),
                        txtIdentity.Text.Trim(),
                        txtPhone.Text.Trim(),
                        txtEmail.Text.Trim(),
                        dtBirth.Checked ? (DateTime?)dtBirth.Value.Date : null,
                        txtAddress.Text.Trim(),
                        txtTempReg.Text.Trim(),
                        dtTempFrom.Checked ? (DateTime?)dtTempFrom.Value.Date : null,
                        dtTempTo.Checked ? (DateTime?)dtTempTo.Value.Date : null,
                        chkActive.Checked
                    );
                    SavedTenantId = id;
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu khách thuê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
