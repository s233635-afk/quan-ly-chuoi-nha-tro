using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// FrmNotificationEditor - Thêm/Sửa Thông Báo
    /// </summary>
    public class FrmNotificationEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private NumericUpDown _numUserId;
        private TextBox _txtTitle;
        private ComboBox _cboStatus;
        private TextBox _txtMessage;
        private Button _btnSave, _btnCancel;

        public FrmNotificationEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            Load += (s, e) => LoadExisting();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "➕ Thêm Thông Báo Mới" : "✏️ Sửa Thông Báo";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(800, 550);
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10f);

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(0, 120, 215),
                Padding = new Padding(15, 12, 15, 12)
            };

            var lblHeader = new Label
            {
                Text = Text,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblHeader);

            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true,
                BackColor = Color.White
            };

            int labelWidth = 150;
            int inputWidth = 500;
            int top = 15;
            int lineHeight = 45;

            // UserId
            var lblUserId = new Label
            {
                Text = "👤 Nhân Viên (0 = Tất Cả)",
                Location = new Point(15, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.75f, FontStyle.Bold)
            };
            _numUserId = new NumericUpDown
            {
                Location = new Point(labelWidth + 15, top),
                Size = new Size(inputWidth, 32),
                Minimum = 0,
                Maximum = 9999999,
                DecimalPlaces = 0,
                ThousandsSeparator = true,
                Value = 0
            };
            top += lineHeight;

            // Title
            var lblTitle = new Label
            {
                Text = "📝 Tiêu Đề (*)",
                Location = new Point(15, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.75f, FontStyle.Bold)
            };
            _txtTitle = new TextBox
            {
                Location = new Point(labelWidth + 15, top),
                Size = new Size(inputWidth, 32),
                Font = new Font("Segoe UI", 10f)
            };
            top += lineHeight;

            // Status
            var lblStatus = new Label
            {
                Text = "🏷️ Trạng Thái",
                Location = new Point(15, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.75f, FontStyle.Bold)
            };
            _cboStatus = new ComboBox
            {
                Location = new Point(labelWidth + 15, top),
                Size = new Size(inputWidth, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10f)
            };
            _cboStatus.Items.AddRange(new object[] { "Chưa đọc", "Đã đọc", "Đã gửi" });
            _cboStatus.SelectedIndex = 0;
            top += lineHeight;

            // Message
            var lblMessage = new Label
            {
                Text = "💬 Nội Dung",
                Location = new Point(15, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.75f, FontStyle.Bold)
            };
            _txtMessage = new TextBox
            {
                Location = new Point(labelWidth + 15, top),
                Size = new Size(inputWidth, 150),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10f),
                WordWrap = true
            };

            pnlBody.Controls.AddRange(new Control[]
            {
                lblUserId, _numUserId,
                lblTitle, _txtTitle,
                lblStatus, _cboStatus,
                lblMessage, _txtMessage
            });

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(15, 12, 15, 12),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _btnCancel = new Button
            {
                Text = "❌ Hủy",
                Width = 120,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            _btnSave = new Button
            {
                Text = "✅ Lưu",
                Width = 120,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (s, e) => await SaveAsync();

            pnlBottom.Controls.Add(_btnCancel);
            pnlBottom.Controls.Add(_btnSave);

            _btnCancel.Location = new Point(pnlBottom.Width - _btnCancel.Width - 15, 10);
            _btnSave.Location = new Point(_btnCancel.Left - _btnSave.Width - 10, 10);

            pnlBottom.Resize += (s, e) =>
            {
                _btnCancel.Location = new Point(pnlBottom.Width - _btnCancel.Width - 15, 10);
                _btnSave.Location = new Point(_btnCancel.Left - _btnSave.Width - 10, 10);
            };

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
            Controls.Add(pnlHeader);
        }

        private void LoadExisting()
        {
            if (_existingRow == null) return;

            try
            {
                int userId = 0;
                if (_existingRow.Table.Columns.Contains("UserId") && _existingRow["UserId"] != DBNull.Value)
                {
                    userId = Convert.ToInt32(_existingRow["UserId"]);
                }
                if (userId > 0 && userId <= _numUserId.Maximum)
                {
                    _numUserId.Value = userId;
                }

                if (_existingRow.Table.Columns.Contains("Title"))
                {
                    _txtTitle.Text = _existingRow["Title"]?.ToString() ?? string.Empty;
                }

                if (_existingRow.Table.Columns.Contains("Message"))
                {
                    _txtMessage.Text = _existingRow["Message"]?.ToString() ?? string.Empty;
                }

                if (_existingRow.Table.Columns.Contains("Status"))
                {
                    string status = _existingRow["Status"]?.ToString() ?? "Chưa đọc";
                    int idx = _cboStatus.FindStringExact(status);
                    if (idx >= 0) _cboStatus.SelectedIndex = idx;
                }
            }
            catch { }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string title = _txtTitle.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("⚠️ Vui lòng nhập tiêu đề thông báo.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtTitle.Focus();
                return;
            }

            int? userId = _numUserId.Value > 0 ? (int?)Convert.ToInt32(_numUserId.Value) : null;
            string message = _txtMessage.Text.Trim();
            string status = _cboStatus.SelectedItem?.ToString() ?? "Chưa đọc";

            try
            {
                if (_existingRow == null)
                {
                    await _bll.AddNotificationAsync(userId, title, message, status);
                    MessageBox.Show("✅ Thêm thông báo thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    int id = Convert.ToInt32(_existingRow["NotificationId"]);
                    await _bll.UpdateNotificationAsync(id, userId, title, message, status);
                    MessageBox.Show("✅ Cập nhật thông báo thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

