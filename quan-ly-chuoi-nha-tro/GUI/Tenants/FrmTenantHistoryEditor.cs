using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmTenantHistoryEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly int _tenantId;
        private readonly DataRow _existingRow;

        private DataTable _roomTable;

        private ComboBox cboRoom;
        private DateTimePicker dtCheckIn;
        private DateTimePicker dtCheckOut;
        private ComboBox cboStatus;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        public FrmTenantHistoryEditor(AdminDataBLL bll, int tenantId, DataRow existingRow = null)
        {
            _bll = bll;
            _tenantId = tenantId;
            _existingRow = existingRow;
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm lịch sử phòng" : "Cập nhật lịch sử phòng";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 420);
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

            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            dtCheckIn = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            dtCheckOut = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Value = DateTime.Today };
            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Active", "Completed", "Cancelled" });
            cboStatus.SelectedIndex = 0;
            txtNotes = new TextBox { Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };

            pnlBody.Controls.Add(MakeLabel("Phòng (*)", top));
            pnlBody.Controls.Add(MakeInput(cboRoom, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ngày vào (*)", top));
            pnlBody.Controls.Add(MakeInput(dtCheckIn, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ngày ra", top));
            pnlBody.Controls.Add(MakeInput(dtCheckOut, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Trạng thái", top));
            pnlBody.Controls.Add(MakeInput(cboStatus, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ghi chú", top));
            pnlBody.Controls.Add(MakeInput(txtNotes, top));

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

        private async System.Threading.Tasks.Task LoadAsync()
        {
            await LoadRoomsAsync();

            if (_existingRow == null) return;

            int roomId = ReadInt(_existingRow, "RoomId");
            if (roomId > 0)
            {
                try { cboRoom.SelectedValue = roomId; } catch { }
            }

            if (DateTime.TryParse(_existingRow["CheckInDate"]?.ToString(), out var ci))
                dtCheckIn.Value = ci.Date;

            if (DateTime.TryParse(_existingRow["CheckOutDate"]?.ToString(), out var co))
            {
                dtCheckOut.Checked = true;
                dtCheckOut.Value = co.Date;
            }
            else
            {
                dtCheckOut.Checked = false;
            }

            string st = _existingRow.Table.Columns.Contains("Status") ? _existingRow["Status"]?.ToString() : null;
            if (!string.IsNullOrWhiteSpace(st))
            {
                int idx = cboStatus.FindStringExact(st);
                if (idx >= 0) cboStatus.SelectedIndex = idx;
            }

            txtNotes.Text = _existingRow.Table.Columns.Contains("Notes") ? _existingRow["Notes"]?.ToString() : string.Empty;
        }

        private async System.Threading.Tasks.Task LoadRoomsAsync()
        {
            try
            {
                var dt = await _bll.GetRoomsAsync();

                var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                var allowedIds = AdminBranchScope.GetAllowedBranchIds(branches);
                dt = AdminBranchScope.FilterByBranchIds(dt, allowedIds);
                TextFixer.FixDataTable(dt, "RoomNumber", "BranchName", "StatusName");

                _roomTable = new DataTable();
                _roomTable.Columns.Add("RoomId", typeof(int));
                _roomTable.Columns.Add("RoomDisplay", typeof(string));
                _roomTable.Rows.Add(0, "— Chọn phòng —");

                if (dt != null && dt.Columns.Contains("RoomId"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int id = 0;
                        try { id = Convert.ToInt32(r["RoomId"]); } catch { }

                        string roomNo = r.Table.Columns.Contains("RoomNumber") ? r["RoomNumber"]?.ToString() : null;
                        string branch = r.Table.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : null;
                        string status = r.Table.Columns.Contains("StatusName") ? r["StatusName"]?.ToString() : null;
                        string display = roomNo;
                        if (!string.IsNullOrWhiteSpace(branch)) display = $"{roomNo} - {branch}";
                        if (!string.IsNullOrWhiteSpace(status)) display = $"{display} ({status})";
                        if (string.IsNullOrWhiteSpace(display)) display = "Phòng " + id;

                        _roomTable.Rows.Add(id, display);
                    }
                }

                cboRoom.DataSource = _roomTable;
                cboRoom.DisplayMember = "RoomDisplay";
                cboRoom.ValueMember = "RoomId";
                cboRoom.SelectedValue = 0;
            }
            catch
            {
                cboRoom.Items.Clear();
                cboRoom.Items.Add("— Chọn phòng —");
                cboRoom.SelectedIndex = 0;
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            int roomId = 0;
            try
            {
                if (cboRoom.SelectedValue != null)
                    int.TryParse(cboRoom.SelectedValue.ToString(), out roomId);
            }
            catch { }

            if (roomId <= 0)
            {
                MessageBox.Show("Vui lòng chọn phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime checkIn = dtCheckIn.Value.Date;
            DateTime? checkOut = dtCheckOut.Checked ? (DateTime?)dtCheckOut.Value.Date : null;
            if (checkOut.HasValue && checkOut.Value.Date < checkIn.Date)
            {
                MessageBox.Show("Ngày ra phải >= ngày vào.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string status = cboStatus.Text;

            try
            {
                if (_existingRow == null)
                {
                    await _bll.AddTenantHistoryAsync(_tenantId, roomId, checkIn, checkOut, status, txtNotes.Text.Trim());
                }
                else
                {
                    int historyId = Convert.ToInt32(_existingRow["HistoryId"]);
                    await _bll.UpdateTenantHistoryAsync(historyId, roomId, checkIn, checkOut, status, txtNotes.Text.Trim());
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu lịch sử phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static int ReadInt(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0;
            if (int.TryParse(v.ToString(), out var i)) return i;
            try { return Convert.ToInt32(v); } catch { return 0; }
        }
    }
}
