using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmDependentEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly int _tenantId;
        private readonly DataRow _existingRow;

        private TextBox txtFullName;
        private TextBox txtRelationship;
        private TextBox txtPhone;
        private Button btnSave;
        private Button btnCancel;

        public FrmDependentEditor(AdminDataBLL bll, int tenantId, DataRow existingRow = null)
        {
            _bll = bll;
            _tenantId = tenantId;
            _existingRow = existingRow;
            InitializeComponent();
            Load += (s, e) => LoadExisting();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm người ở chung" : "Cập nhật người ở chung";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(640, 260);
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

            int labelWidth = 170;
            int inputWidth = 390;
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

            txtFullName = new TextBox();
            txtRelationship = new TextBox();
            txtPhone = new TextBox();

            pnlBody.Controls.Add(MakeLabel("Họ tên (*)", top));
            pnlBody.Controls.Add(MakeInput(txtFullName, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Quan hệ", top));
            pnlBody.Controls.Add(MakeInput(txtRelationship, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("SĐT", top));
            pnlBody.Controls.Add(MakeInput(txtPhone, top));

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            btnSave = new Button
            {
                Text = "Lưu",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
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

            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }

        private void LoadExisting()
        {
            if (_existingRow == null) return;
            txtFullName.Text = _existingRow.Table.Columns.Contains("FullName") ? _existingRow["FullName"]?.ToString() : string.Empty;
            txtRelationship.Text = _existingRow.Table.Columns.Contains("Relationship") ? _existingRow["Relationship"]?.ToString() : string.Empty;
            txtPhone.Text = _existingRow.Table.Columns.Contains("PhoneNumber") ? _existingRow["PhoneNumber"]?.ToString() : string.Empty;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string fullName = txtFullName.Text.Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Vui lòng nhập Họ tên.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_existingRow == null)
                {
                    await _bll.AddDependentAsync(_tenantId, fullName, txtRelationship.Text.Trim(), txtPhone.Text.Trim());
                }
                else
                {
                    int id = Convert.ToInt32(_existingRow["DependentId"]);
                    await _bll.UpdateDependentAsync(id, _tenantId, fullName, txtRelationship.Text.Trim(), txtPhone.Text.Trim());
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu người ở chung: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

