using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmNotificationEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private NumericUpDown numUserId;
        private TextBox txtTitle;
        private ComboBox cboStatus;
        private TextBox txtMessage;
        private Button btnSave;
        private Button btnCancel;

        public FrmNotificationEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            Load += (s, e) => LoadExisting();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm thông báo" : "Cập nhật thông báo";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(760, 480);
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

            numUserId = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            txtTitle = new TextBox();
            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Unread", "Read", "Sent" });
            cboStatus.SelectedIndex = 0;
            txtMessage = new TextBox { Multiline = true, Height = 200, ScrollBars = ScrollBars.Vertical };

            pnlBody.Controls.Add(MakeLabel("UserId (0 = tất cả)", top));
            pnlBody.Controls.Add(MakeInput(numUserId, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Tiêu đề (*)", top));
            pnlBody.Controls.Add(MakeInput(txtTitle, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Trạng thái", top));
            pnlBody.Controls.Add(MakeInput(cboStatus, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Nội dung", top));
            pnlBody.Controls.Add(MakeInput(txtMessage, top));

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

            int userId = 0;
            if (_existingRow.Table.Columns.Contains("UserId"))
            {
                try { userId = Convert.ToInt32(_existingRow["UserId"]); } catch { userId = 0; }
            }
            if (userId > 0 && userId <= numUserId.Maximum) numUserId.Value = userId;

            txtTitle.Text = _existingRow.Table.Columns.Contains("Title") ? _existingRow["Title"]?.ToString() : string.Empty;
            txtMessage.Text = _existingRow.Table.Columns.Contains("Message") ? _existingRow["Message"]?.ToString() : string.Empty;
            string st = _existingRow.Table.Columns.Contains("Status") ? _existingRow["Status"]?.ToString() : null;
            if (!string.IsNullOrWhiteSpace(st))
            {
                int idx = cboStatus.FindStringExact(st);
                if (idx >= 0) cboStatus.SelectedIndex = idx;
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string title = txtTitle.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Vui lòng nhập tiêu đề.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? userId = numUserId.Value > 0 ? (int?)Convert.ToInt32(numUserId.Value) : null;

            try
            {
                if (_existingRow == null)
                {
                    await _bll.AddNotificationAsync(userId, title, txtMessage.Text.Trim(), cboStatus.Text);
                }
                else
                {
                    int id = Convert.ToInt32(_existingRow["NotificationId"]);
                    await _bll.UpdateNotificationAsync(id, userId, title, txtMessage.Text.Trim(), cboStatus.Text);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu thông báo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

