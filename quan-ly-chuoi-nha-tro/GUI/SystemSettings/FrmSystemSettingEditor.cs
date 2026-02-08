using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmSystemSettingEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private TextBox txtKey;
        private TextBox txtValue;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;

        public FrmSystemSettingEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            Load += (s, e) => LoadExisting();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm cấu hình" : "Cập nhật cấu hình";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(760, 360);
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
            int inputWidth = 460;
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

            txtKey = new TextBox();
            txtValue = new TextBox();
            txtDescription = new TextBox { Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };

            pnlBody.Controls.Add(MakeLabel("SettingKey (*)", top));
            pnlBody.Controls.Add(MakeInput(txtKey, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("SettingValue", top));
            pnlBody.Controls.Add(MakeInput(txtValue, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Description", top));
            pnlBody.Controls.Add(MakeInput(txtDescription, top));

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
            txtKey.Text = _existingRow.Table.Columns.Contains("SettingKey") ? _existingRow["SettingKey"]?.ToString() : string.Empty;
            txtKey.ReadOnly = true;
            txtValue.Text = _existingRow.Table.Columns.Contains("SettingValue") ? _existingRow["SettingValue"]?.ToString() : string.Empty;
            txtDescription.Text = _existingRow.Table.Columns.Contains("Description") ? _existingRow["Description"]?.ToString() : string.Empty;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string key = txtKey.Text.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                ToastNotification.Warning("Vui lòng nhập SettingKey");
                return;
            }

            try
            {
                if (_existingRow == null)
                {
                    await _bll.AddSystemSettingAsync(key, txtValue.Text.Trim(), txtDescription.Text.Trim());
                }
                else
                {
                    await _bll.UpdateSystemSettingAsync(key, txtValue.Text.Trim(), txtDescription.Text.Trim());
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "SaveSetting", "Lỗi lưu cấu hình");
            }
        }
    }
}

