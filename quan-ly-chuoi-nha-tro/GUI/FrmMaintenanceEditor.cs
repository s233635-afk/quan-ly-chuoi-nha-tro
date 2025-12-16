using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmMaintenanceEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private DataTable _roomTable;
        private DataTable _staffTable;

        private TextBox txtTicketNumber;
        private ComboBox cboRoom;
        private ComboBox cboRequestorType;
        private NumericUpDown numRequestorId;
        private ComboBox cboPriority;
        private ComboBox cboAssigned;
        private ComboBox cboStatus;
        private DateTimePicker dtCompleted;
        private TextBox txtIssue;
        private TextBox txtNotes;

        private Button btnSave;
        private Button btnCancel;

        public FrmMaintenanceEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm phiếu bảo trì" : "Cập nhật phiếu bảo trì";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(820, 560);
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

            int labelWidth = 200;
            int inputWidth = 520;
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

            txtTicketNumber = new TextBox { ReadOnly = true };
            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRequestorType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRequestorType.Items.AddRange(new object[] { "Tenant", "Staff", "System" });
            cboRequestorType.SelectedIndex = 0;

            numRequestorId = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };

            cboPriority = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboPriority.Items.AddRange(new object[] { "Low", "Medium", "High", "Urgent" });
            cboPriority.SelectedIndex = 1;

            cboAssigned = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Created", "InProgress", "Completed", "Cancelled" });
            cboStatus.SelectedIndex = 0;
            cboStatus.SelectedIndexChanged += (s, e) =>
            {
                if (string.Equals(cboStatus.Text, "Completed", StringComparison.OrdinalIgnoreCase) && !dtCompleted.Checked)
                {
                    dtCompleted.Checked = true;
                    dtCompleted.Value = DateTime.Now;
                }
            };

            dtCompleted = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", ShowCheckBox = true, Checked = false, Value = DateTime.Now };

            txtIssue = new TextBox { Multiline = true, Height = 80, ScrollBars = ScrollBars.Vertical };
            txtNotes = new TextBox { Multiline = true, Height = 80, ScrollBars = ScrollBars.Vertical };

            pnlBody.Controls.Add(MakeLabel("Số phiếu", top));
            pnlBody.Controls.Add(MakeInput(txtTicketNumber, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Phòng (*)", top));
            pnlBody.Controls.Add(MakeInput(cboRoom, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Nguồn yêu cầu", top));
            pnlBody.Controls.Add(MakeInput(cboRequestorType, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Nguồn ID (tùy chọn)", top));
            pnlBody.Controls.Add(MakeInput(numRequestorId, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ưu tiên", top));
            pnlBody.Controls.Add(MakeInput(cboPriority, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Nhân viên xử lý", top));
            pnlBody.Controls.Add(MakeInput(cboAssigned, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Trạng thái", top));
            pnlBody.Controls.Add(MakeInput(cboStatus, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Hoàn tất lúc", top));
            pnlBody.Controls.Add(MakeInput(dtCompleted, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Mô tả sự cố", top));
            pnlBody.Controls.Add(MakeInput(txtIssue, top));
            top += 90;

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
            await LoadStaffAsync();

            if (_existingRow == null)
            {
                txtTicketNumber.Text = GenerateTicketNumber();
                return;
            }

            txtTicketNumber.Text = _existingRow.Table.Columns.Contains("TicketNumber") ? _existingRow["TicketNumber"]?.ToString() : GenerateTicketNumber();
            int roomId = ReadInt(_existingRow, "RoomId");
            if (roomId > 0) { try { cboRoom.SelectedValue = roomId; } catch { } }

            string reqType = _existingRow.Table.Columns.Contains("RequestorType") ? _existingRow["RequestorType"]?.ToString() : null;
            if (!string.IsNullOrWhiteSpace(reqType))
            {
                int idx = cboRequestorType.FindStringExact(reqType);
                if (idx >= 0) cboRequestorType.SelectedIndex = idx;
            }

            int reqId = ReadInt(_existingRow, "RequestorId");
            if (reqId > 0 && reqId <= numRequestorId.Maximum) numRequestorId.Value = reqId;

            string pri = _existingRow.Table.Columns.Contains("Priority") ? _existingRow["Priority"]?.ToString() : null;
            if (!string.IsNullOrWhiteSpace(pri))
            {
                int idx = cboPriority.FindStringExact(pri);
                if (idx >= 0) cboPriority.SelectedIndex = idx;
            }

            int assignedId = ReadInt(_existingRow, "AssignedToUserId");
            if (assignedId > 0) { try { cboAssigned.SelectedValue = assignedId; } catch { } }

            string st = _existingRow.Table.Columns.Contains("Status") ? _existingRow["Status"]?.ToString() : null;
            if (!string.IsNullOrWhiteSpace(st))
            {
                int idx = cboStatus.FindStringExact(st);
                if (idx >= 0) cboStatus.SelectedIndex = idx;
            }

            if (DateTime.TryParse(_existingRow["CompletedDate"]?.ToString(), out var cd))
            {
                dtCompleted.Checked = true;
                dtCompleted.Value = cd;
            }
            else
            {
                dtCompleted.Checked = false;
            }

            txtIssue.Text = _existingRow.Table.Columns.Contains("IssueDescription") ? _existingRow["IssueDescription"]?.ToString() : string.Empty;
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
                        string display = roomNo;
                        if (!string.IsNullOrWhiteSpace(branch)) display = $"{roomNo} - {branch}";
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

        private async System.Threading.Tasks.Task LoadStaffAsync()
        {
            try
            {
                var dt = await _bll.GetStaffAsync();
                _staffTable = new DataTable();
                _staffTable.Columns.Add("UserId", typeof(int));
                _staffTable.Columns.Add("UserDisplay", typeof(string));
                _staffTable.Rows.Add(0, "— Không chọn —");

                if (dt != null && dt.Columns.Contains("UserId"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int id = 0;
                        try { id = Convert.ToInt32(r["UserId"]); } catch { }
                        string user = r.Table.Columns.Contains("UserName") ? r["UserName"]?.ToString() : null;
                        string name = r.Table.Columns.Contains("FullName") ? r["FullName"]?.ToString() : null;
                        string display = string.IsNullOrWhiteSpace(name) ? user : $"{name} ({user})";
                        if (string.IsNullOrWhiteSpace(display)) display = "NV " + id;
                        _staffTable.Rows.Add(id, display);
                    }
                }

                cboAssigned.DataSource = _staffTable;
                cboAssigned.DisplayMember = "UserDisplay";
                cboAssigned.ValueMember = "UserId";
                cboAssigned.SelectedValue = 0;
            }
            catch
            {
                cboAssigned.Items.Clear();
                cboAssigned.Items.Add("— Không chọn —");
                cboAssigned.SelectedIndex = 0;
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            int roomId = GetSelectedId(cboRoom);
            if (roomId <= 0)
            {
                MessageBox.Show("Vui lòng chọn phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ticketNumber = txtTicketNumber.Text.Trim();
            if (string.IsNullOrWhiteSpace(ticketNumber))
                ticketNumber = GenerateTicketNumber();

            int? requestorId = numRequestorId.Value > 0 ? (int?)Convert.ToInt32(numRequestorId.Value) : null;
            int? assignedTo = GetSelectedId(cboAssigned) > 0 ? (int?)GetSelectedId(cboAssigned) : null;

            string status = cboStatus.Text;
            DateTime? completed = dtCompleted.Checked ? (DateTime?)dtCompleted.Value : null;
            if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) && !completed.HasValue)
                completed = DateTime.Now;

            try
            {
                if (_existingRow == null)
                {
                    await _bll.AddMaintenanceTicketAsync(
                        ticketNumber,
                        roomId,
                        cboRequestorType.Text,
                        requestorId,
                        txtIssue.Text.Trim(),
                        cboPriority.Text,
                        assignedTo,
                        status,
                        completed,
                        txtNotes.Text.Trim());
                }
                else
                {
                    int id = Convert.ToInt32(_existingRow["TicketId"]);
                    await _bll.UpdateMaintenanceTicketAsync(
                        id,
                        roomId,
                        cboRequestorType.Text,
                        requestorId,
                        txtIssue.Text.Trim(),
                        cboPriority.Text,
                        assignedTo,
                        status,
                        completed,
                        txtNotes.Text.Trim());
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu phiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GenerateTicketNumber()
        {
            return "BT-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        private static int GetSelectedId(ComboBox cbo)
        {
            try
            {
                if (cbo.SelectedValue != null && int.TryParse(cbo.SelectedValue.ToString(), out var id))
                    return id;
            }
            catch { }
            return 0;
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
