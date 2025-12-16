using System;
using System.Data;
using System.Drawing;
using System.Linq;
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
        private TextBox txtFrontIdPhoto;
        private TextBox txtBackIdPhoto;
        private Button btnBrowseFront;
        private Button btnBrowseBack;
        private TextBox txtTempReg;
        private DateTimePicker dtTempFrom;
        private DateTimePicker dtTempTo;
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnCancel;
        private ComboBox cboRoom;
        private DataTable _rooms;

        public int? SavedTenantId { get; private set; }

        public FrmTenantEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            Load += async (s, e) =>
            {
                await LoadRoomsAsync();
                LoadExisting();
            };
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm khách thuê" : "Cập nhật khách thuê";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 520);
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
            int inputWidth = 440;
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
            txtIdentity = new TextBox();
            txtPhone = new TextBox();
            txtEmail = new TextBox();
            dtBirth = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            txtAddress = new TextBox { Multiline = true, Height = 70, ScrollBars = ScrollBars.Vertical };
            txtFrontIdPhoto = new TextBox();
            txtBackIdPhoto = new TextBox();
            btnBrowseFront = new Button { Text = "Chọn...", Width = 80, Height = 26 };
            btnBrowseBack = new Button { Text = "Chọn...", Width = 80, Height = 26 };
            btnBrowseFront.Click += (s, e) => BrowseFileToTextBox(txtFrontIdPhoto, "Chọn ảnh CCCD mặt trước");
            btnBrowseBack.Click += (s, e) => BrowseFileToTextBox(txtBackIdPhoto, "Chọn ảnh CCCD mặt sau");
            txtTempReg = new TextBox();
            dtTempFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            dtTempTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            chkActive = new CheckBox { Text = "Đang hoạt động", Checked = true, AutoSize = true };
            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

            pnlBody.Controls.Add(MakeLabel("Họ tên (*)", top));
            pnlBody.Controls.Add(MakeInput(txtFullName, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("CMND/CCCD", top));
            pnlBody.Controls.Add(MakeInput(txtIdentity, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("SĐT", top));
            pnlBody.Controls.Add(MakeInput(txtPhone, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Email", top));
            pnlBody.Controls.Add(MakeInput(txtEmail, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ngày sinh", top));
            pnlBody.Controls.Add(MakeInput(dtBirth, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Địa chỉ", top));
            pnlBody.Controls.Add(MakeInput(txtAddress, top));
            top += 80;

            pnlBody.Controls.Add(MakeLabel("Ảnh CCCD (mặt trước)", top));
            pnlBody.Controls.Add(MakeBrowseRow(txtFrontIdPhoto, btnBrowseFront, left + labelWidth, top, inputWidth));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ảnh CCCD (mặt sau)", top));
            pnlBody.Controls.Add(MakeBrowseRow(txtBackIdPhoto, btnBrowseBack, left + labelWidth, top, inputWidth));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Tạm trú tại", top));
            pnlBody.Controls.Add(MakeInput(txtTempReg, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ngày đăng ký", top));
            pnlBody.Controls.Add(MakeInput(dtTempFrom, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Hết hạn tạm trú", top));
            pnlBody.Controls.Add(MakeInput(dtTempTo, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Phòng đang ở", top));
            pnlBody.Controls.Add(MakeInput(cboRoom, top));
            top += line;

            var pnlActive = new Panel { Location = new Point(left + labelWidth, top), Width = inputWidth, Height = 26, BackColor = Color.Transparent };
            chkActive.Parent = pnlActive;
            chkActive.Location = new Point(0, 3);
            pnlBody.Controls.Add(MakeLabel("Trạng thái", top));
            pnlBody.Controls.Add(pnlActive);

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

            txtFullName.Text = _existingRow["FullName"]?.ToString();
            txtIdentity.Text = _existingRow.Table.Columns.Contains("IdentityCard") ? _existingRow["IdentityCard"]?.ToString() : string.Empty;
            txtPhone.Text = _existingRow.Table.Columns.Contains("PhoneNumber") ? _existingRow["PhoneNumber"]?.ToString() : string.Empty;
            txtEmail.Text = _existingRow.Table.Columns.Contains("Email") ? _existingRow["Email"]?.ToString() : string.Empty;

            if (_existingRow.Table.Columns.Contains("BirthDate") && DateTime.TryParse(_existingRow["BirthDate"]?.ToString(), out var b))
            {
                dtBirth.Value = b;
                dtBirth.Checked = true;
            }
            else dtBirth.Checked = false;

            txtAddress.Text = _existingRow.Table.Columns.Contains("Address") ? _existingRow["Address"]?.ToString() : string.Empty;
            txtFrontIdPhoto.Text = _existingRow.Table.Columns.Contains("FrontIdPhoto") ? _existingRow["FrontIdPhoto"]?.ToString() : string.Empty;
            txtBackIdPhoto.Text = _existingRow.Table.Columns.Contains("BackIdPhoto") ? _existingRow["BackIdPhoto"]?.ToString() : string.Empty;
            txtTempReg.Text = _existingRow.Table.Columns.Contains("TemporaryRegistration") ? _existingRow["TemporaryRegistration"]?.ToString() : string.Empty;

            if (_existingRow.Table.Columns.Contains("CurrentRoomId") && int.TryParse(_existingRow["CurrentRoomId"]?.ToString(), out var rid))
            {
                if (cboRoom.Items.Count > 0)
                    cboRoom.SelectedValue = rid;
            }
            else if (_existingRow.Table.Columns.Contains("RoomNumber"))
            {
                var roomNo = _existingRow["RoomNumber"]?.ToString();
                if (!string.IsNullOrWhiteSpace(roomNo) && _rooms != null && _rooms.Columns.Contains("RoomNumber"))
                {
                    var row = _rooms.AsEnumerable().FirstOrDefault(r => string.Equals(r["RoomNumber"]?.ToString(), roomNo, StringComparison.OrdinalIgnoreCase));
                    if (row != null) cboRoom.SelectedValue = Convert.ToInt32(row["RoomId"]);
                }
            }

            if (_existingRow.Table.Columns.Contains("TemporaryRegistrationDate") && DateTime.TryParse(_existingRow["TemporaryRegistrationDate"]?.ToString(), out var t1))
            {
                dtTempFrom.Value = t1;
                dtTempFrom.Checked = true;
            }
            else dtTempFrom.Checked = false;

            if (_existingRow.Table.Columns.Contains("TemporaryRegistrationExpiry") && DateTime.TryParse(_existingRow["TemporaryRegistrationExpiry"]?.ToString(), out var t2))
            {
                dtTempTo.Value = t2;
                dtTempTo.Checked = true;
            }
            else dtTempTo.Checked = false;

            if (_existingRow.Table.Columns.Contains("IsActive") && bool.TryParse(_existingRow["IsActive"]?.ToString(), out var act))
                chkActive.Checked = act;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập Họ tên.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int? selectedRoomId = cboRoom.SelectedValue is int v && v > 0 ? v : (int?)null;
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
                        chkActive.Checked,
                        txtFrontIdPhoto.Text.Trim(),
                        txtBackIdPhoto.Text.Trim()
                    );
                    SavedTenantId = newId;
                    await UpdateTenantRoomAsync(newId, selectedRoomId);
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
                        chkActive.Checked,
                        txtFrontIdPhoto.Text.Trim(),
                        txtBackIdPhoto.Text.Trim()
                    );
                    SavedTenantId = id;
                    await UpdateTenantRoomAsync(id, selectedRoomId);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu khách thuê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Panel MakeBrowseRow(TextBox textBox, Button button, int x, int y, int width)
        {
            var panel = new Panel { Location = new Point(x, y), Width = width, Height = 26, BackColor = Color.Transparent };
            textBox.Parent = panel;
            textBox.Location = new Point(0, 0);
            textBox.Width = Math.Max(120, width - button.Width - 10);
            textBox.Height = 26;
            button.Parent = panel;
            button.Location = new Point(textBox.Right + 10, 0);
            button.Height = 26;
            return panel;
        }

        private static void BrowseFileToTextBox(TextBox target, string title)
        {
            if (target == null) return;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = title ?? "Chọn file";
                ofd.Filter = "Tất cả file (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                    target.Text = ofd.FileName;
            }
        }

        private async System.Threading.Tasks.Task LoadRoomsAsync()
        {
            try
            {
                _rooms = await _bll.GetRoomsAsync() ?? new DataTable();
                TextFixer.ForceFixDataTable(_rooms, "RoomNumber", "BranchName", "SectionName", "StatusName");

                var filteredRows = _rooms.AsEnumerable()
                    .Where(r =>
                    {
                        string status = r.Table.Columns.Contains("StatusName") ? r["StatusName"]?.ToString() : null;
                        return string.IsNullOrWhiteSpace(status) || status.IndexOf("đang ở", StringComparison.OrdinalIgnoreCase) < 0;
                    })
                    .OrderBy(r => r["RoomNumber"]?.ToString())
                    .Take(200);

                var filtered = CopyRowsToTable(filteredRows, _rooms);

                cboRoom.DataSource = filtered.Rows.Count > 0 ? filtered : _rooms;
                cboRoom.DisplayMember = "RoomNumber";
                cboRoom.ValueMember = "RoomId";
                cboRoom.SelectedIndex = -1;
            }
            catch
            {
                cboRoom.DataSource = null;
            }
        }

        private static System.Threading.Tasks.Task UpdateTenantRoomAsync(int? tenantId, int? roomId)
        {
            // TODO: Wire up tenant-room assignment when backend API is available.
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private static DataTable CopyRowsToTable(System.Collections.Generic.IEnumerable<DataRow> rows, DataTable template)
        {
            var table = template?.Clone() ?? new DataTable();
            if (rows == null) return table;

            foreach (var r in rows)
            {
                try { table.ImportRow(r); }
                catch { }
            }

            return table;
        }
    }
}
