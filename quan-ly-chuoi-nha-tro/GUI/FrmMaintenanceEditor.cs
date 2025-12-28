using System;
using System.Data;
using System.Drawing;
using System.Linq;
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

        private string _notesMeta;

        private const string PrevStatusIdTag = "[PrevRoomStatusId=";
        private const string PrevStatusNameTag = "[PrevRoomStatusName=";

        private sealed class ComboOption
        {
            public string Value { get; }
            public string Display { get; }

            public ComboOption(string value, string display)
            {
                Value = value;
                Display = display;
            }
        }

        public FrmMaintenanceEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "➕ Thêm phiếu bảo trì" : "✎ Cập nhật phiếu bảo trì";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(900, 720);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            // ===== HEADER =====
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(229, 57, 53),
                Padding = new Padding(20, 12, 20, 12)
            };
            var lblTitle = new Label
            {
                Text = _existingRow == null ? "Thêm phiếu bảo trì mới" : "Cập nhật thông tin phiếu",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            // ===== BODY =====
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 20, 20, 10),
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250)
            };

            int labelWidth = 160;
            int inputWidth = 600;
            int top = 10;
            int left = 6;
            int line = 38;

            Label MakeLabel(string text, int y, bool required = false) => new Label
            {
                Text = required ? text + " (*)" : text,
                Location = new Point(left, y),
                Width = labelWidth,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10),
                ForeColor = required ? Color.FromArgb(229, 57, 53) : Color.FromArgb(50, 50, 50)
            };

            Control MakeInput(Control ctl, int y)
            {
                ctl.Location = new Point(left + labelWidth + 10, y);
                ctl.Width = inputWidth;
                if (ctl is TextBox tb) tb.BackColor = Color.White;
                if (ctl is ComboBox cb) cb.BackColor = Color.White;
                return ctl;
            }

            txtTicketNumber = new TextBox { ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245), ForeColor = Color.Gray };
            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRequestorType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRequestorType.DisplayMember = "Display";
            cboRequestorType.ValueMember = "Value";
            cboRequestorType.DataSource = new[]
            {
                new ComboOption("Tenant", "Khách thuê"),
                new ComboOption("Staff", "Nhân viên"),
                new ComboOption("System", "Hệ thống")
            };
            if (cboRequestorType.Items.Count > 0)
                cboRequestorType.SelectedIndex = 0;

            numRequestorId = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };

            cboPriority = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboPriority.DisplayMember = "Display";
            cboPriority.ValueMember = "Value";
            cboPriority.DataSource = new[]
            {
                new ComboOption("Low", "🟢 Thấp"),
                new ComboOption("Medium", "🟡 Trung bình"),
                new ComboOption("High", "🔴 Cao"),
                new ComboOption("Urgent", "⛔ Khẩn cấp")
            };
            if (cboPriority.Items.Count > 0)
                cboPriority.SelectedValue = "Medium";

            cboAssigned = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.DisplayMember = "Display";
            cboStatus.ValueMember = "Value";
            cboStatus.DataSource = new[]
            {
                new ComboOption("Created", "Mới"),
                new ComboOption("InProgress", "Đang xử lý"),
                new ComboOption("Completed", "Hoàn tất"),
                new ComboOption("Cancelled", "Đã hủy")
            };
            if (cboStatus.Items.Count > 0)
                cboStatus.SelectedValue = "Created";
            cboStatus.SelectedIndexChanged += (s, e) =>
            {
                var statusValue = GetComboValue(cboStatus);
                if (string.Equals(statusValue, "Completed", StringComparison.OrdinalIgnoreCase) && !dtCompleted.Checked)
                {
                    dtCompleted.Checked = true;
                    dtCompleted.Value = DateTime.Now;
                }
            };

            dtCompleted = new DateTimePicker 
            { 
                Format = DateTimePickerFormat.Custom, 
                CustomFormat = "dd/MM/yyyy HH:mm", 
                ShowCheckBox = true, 
                Checked = false, 
                Value = DateTime.Now 
            };

            txtIssue = new TextBox 
            { 
                Multiline = true, 
                Height = 100, 
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10),
                AcceptsReturn = true
            };
            
            txtNotes = new TextBox 
            { 
                Multiline = true, 
                Height = 80, 
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10),
                AcceptsReturn = true
            };

            pnlBody.Controls.Add(MakeLabel("Số phiếu", top));
            pnlBody.Controls.Add(MakeInput(txtTicketNumber, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Phòng", top, true));
            pnlBody.Controls.Add(MakeInput(cboRoom, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Nguồn yêu cầu", top));
            pnlBody.Controls.Add(MakeInput(cboRequestorType, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Nguồn ID", top));
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

            pnlBody.Controls.Add(MakeLabel("Mô tả sự cố", top, true));
            pnlBody.Controls.Add(MakeInput(txtIssue, top));
            top += 110;

            pnlBody.Controls.Add(MakeLabel("Ghi chú", top));
            pnlBody.Controls.Add(MakeInput(txtNotes, top));

            // ===== BOTTOM BUTTONS =====
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(20, 12, 20, 12),
                BackColor = Color.FromArgb(245, 247, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnCancel = new Button
            {
                Text = "❌ Hủy",
                Width = 120,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            btnSave = new Button
            {
                Text = "✓ Lưu",
                Width = 120,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand
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
            Controls.Add(pnlHeader);
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
            SelectComboValue(cboRequestorType, reqType);

            int reqId = ReadInt(_existingRow, "RequestorId");
            if (reqId > 0 && reqId <= numRequestorId.Maximum) numRequestorId.Value = reqId;

            string pri = _existingRow.Table.Columns.Contains("Priority") ? _existingRow["Priority"]?.ToString() : null;
            SelectComboValue(cboPriority, pri);

            int assignedId = ReadInt(_existingRow, "AssignedToUserId");
            if (assignedId > 0) { try { cboAssigned.SelectedValue = assignedId; } catch { } }

            string st = _existingRow.Table.Columns.Contains("Status") ? _existingRow["Status"]?.ToString() : null;
            SelectComboValue(cboStatus, st);

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
            var rawNotes = _existingRow.Table.Columns.Contains("Notes") ? _existingRow["Notes"]?.ToString() : string.Empty;
            _notesMeta = ExtractNotesMeta(rawNotes);
            txtNotes.Text = StripNotesMeta(rawNotes);
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

            string requestorType = GetComboValue(cboRequestorType);
            string priority = GetComboValue(cboPriority);
            string status = GetComboValue(cboStatus);
            DateTime? completed = dtCompleted.Checked ? (DateTime?)dtCompleted.Value : null;
            if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) && !completed.HasValue)
                completed = DateTime.Now;

            try
            {
                string notes = txtNotes.Text.Trim();
                string notesWithMeta = notes;
                if (IsOpenMaintenanceStatus(status))
                {
                    if (string.IsNullOrWhiteSpace(_notesMeta))
                    {
                        var prev = await TryGetCurrentRoomStatusAsync(roomId);
                        _notesMeta = BuildNotesMeta(prev.StatusId, prev.StatusName);
                    }
                }
                notesWithMeta = MergeNotesWithMeta(notes, _notesMeta);

                if (_existingRow == null)
                {
                    await _bll.AddMaintenanceTicketAsync(
                        ticketNumber,
                        roomId,
                        requestorType,
                        requestorId,
                        txtIssue.Text.Trim(),
                        priority,
                        assignedTo,
                        status,
                        completed,
                        notesWithMeta);
                }
                else
                {
                    int id = Convert.ToInt32(_existingRow["TicketId"]);
                    await _bll.UpdateMaintenanceTicketAsync(
                        id,
                        roomId,
                        requestorType,
                        requestorId,
                        txtIssue.Text.Trim(),
                        priority,
                        assignedTo,
                        status,
                        completed,
                        notesWithMeta);
                }

                await UpdateRoomMaintenanceStatusAsync(roomId, status, notesWithMeta);
                AdminEvents.NotifyDataChanged();
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

        private static string GetComboValue(ComboBox cbo)
        {
            try
            {
                if (cbo.SelectedValue != null)
                    return cbo.SelectedValue.ToString();
            }
            catch { }
            return cbo.Text;
        }

        private static void SelectComboValue(ComboBox cbo, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || cbo == null) return;
            try
            {
                cbo.SelectedValue = value;
                if (cbo.SelectedValue != null && string.Equals(cbo.SelectedValue.ToString(), value, StringComparison.OrdinalIgnoreCase))
                    return;
            }
            catch { }

            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (cbo.Items[i] is ComboOption opt)
                {
                    if (string.Equals(opt.Value, value, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(opt.Display, value, StringComparison.OrdinalIgnoreCase))
                    {
                        cbo.SelectedIndex = i;
                        return;
                    }
                }
            }
        }

        private static bool IsOpenMaintenanceStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            return string.Equals(status, "Created", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "InProgress", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsClosedMaintenanceStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            return string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);
        }

        private async System.Threading.Tasks.Task UpdateRoomMaintenanceStatusAsync(int roomId, string ticketStatus, string notes)
        {
            if (roomId <= 0) return;
            bool open = IsOpenMaintenanceStatus(ticketStatus);
            bool closed = IsClosedMaintenanceStatus(ticketStatus);
            if (!open && !closed) return;
            try
            {
                var statuses = await _bll.GetRoomStatusesAsync();
                if (statuses == null || !statuses.Columns.Contains("StatusId") || !statuses.Columns.Contains("StatusName")) return;

                RoomStatusCatalog.CanonicalizeColumn(statuses, "StatusName");
                int targetId = 0;
                string targetName = open ? RoomStatusCatalog.TrangThaiBaoTri : RoomStatusCatalog.TrangThaiTrong;
                if (closed)
                {
                    targetId = TryReadPrevStatusId(notes) ?? 0;
                    var targetById = targetId > 0
                        ? statuses.AsEnumerable().FirstOrDefault(r => TryReadId(r, "StatusId") == targetId)
                        : null;
                    if (targetById != null)
                        targetName = targetById["StatusName"]?.ToString() ?? targetName;
                    else
                    {
                        var prevName = TryReadPrevStatusName(notes);
                        if (!string.IsNullOrWhiteSpace(prevName))
                            targetName = prevName;
                    }
                }

                var row = statuses.AsEnumerable()
                    .FirstOrDefault(r => string.Equals(r["StatusName"]?.ToString(), targetName, StringComparison.OrdinalIgnoreCase));
                if (row == null) return;

                int statusId = 0;
                try { statusId = Convert.ToInt32(row["StatusId"]); } catch { }
                if (statusId <= 0) return;

                await _bll.UpdateRoomOccupancyStatusAsync(roomId, statusId);
            }
            catch
            {
                // ignore status sync failures
            }
        }

        private async System.Threading.Tasks.Task<(int? StatusId, string StatusName)> TryGetCurrentRoomStatusAsync(int roomId)
        {
            try
            {
                var rooms = await _bll.GetRoomsAsync();
                if (rooms == null || !rooms.Columns.Contains("RoomId")) return (null, null);
                var row = rooms.AsEnumerable().FirstOrDefault(r => TryReadId(r, "RoomId") == roomId);
                if (row == null) return (null, null);
                int? statusId = TryReadId(row, "CurrentStatusId");
                if (!statusId.HasValue) statusId = TryReadId(row, "StatusId");
                string statusName = row.Table.Columns.Contains("StatusName") ? row["StatusName"]?.ToString() : null;
                if (!string.IsNullOrWhiteSpace(statusName))
                    statusName = RoomStatusCatalog.Canonicalize(statusName);
                return (statusId, statusName);
            }
            catch
            {
                return (null, null);
            }
        }

        private static int? TryReadId(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column)) return null;
            var v = row[column];
            if (v == null || v == DBNull.Value) return null;
            if (int.TryParse(v.ToString(), out var i)) return i;
            try { return Convert.ToInt32(v); } catch { return null; }
        }

        private static string BuildNotesMeta(int? statusId, string statusName)
        {
            string meta = string.Empty;
            if (statusId.HasValue && statusId.Value > 0)
                meta += $"{PrevStatusIdTag}{statusId.Value}]";
            if (!string.IsNullOrWhiteSpace(statusName))
                meta += $"{PrevStatusNameTag}{statusName}]";
            return meta;
        }

        private static string MergeNotesWithMeta(string notes, string meta)
        {
            var cleaned = StripNotesMeta(notes);
            if (string.IsNullOrWhiteSpace(meta)) return cleaned;
            return string.IsNullOrWhiteSpace(cleaned) ? meta : $"{cleaned} {meta}";
        }

        private static string ExtractNotesMeta(string notes)
        {
            var meta = string.Empty;
            var id = TryReadPrevStatusId(notes);
            var name = TryReadPrevStatusName(notes);
            if (id.HasValue && id.Value > 0)
                meta += $"{PrevStatusIdTag}{id.Value}]";
            if (!string.IsNullOrWhiteSpace(name))
                meta += $"{PrevStatusNameTag}{name}]";
            return meta;
        }

        private static string StripNotesMeta(string notes)
        {
            if (string.IsNullOrWhiteSpace(notes)) return string.Empty;
            var cleaned = RemoveTag(notes, PrevStatusIdTag);
            cleaned = RemoveTag(cleaned, PrevStatusNameTag);
            return cleaned.Trim();
        }

        private static string RemoveTag(string text, string tag)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(tag)) return text;
            int idx = text.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                int end = text.IndexOf(']', idx);
                if (end < 0) break;
                text = text.Remove(idx, end - idx + 1);
                idx = text.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            }
            return text;
        }

        private static int? TryReadPrevStatusId(string notes)
        {
            var value = ExtractTagValue(notes, PrevStatusIdTag);
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (int.TryParse(value, out var id)) return id;
            return null;
        }

        private static string TryReadPrevStatusName(string notes)
        {
            return ExtractTagValue(notes, PrevStatusNameTag);
        }

        private static string ExtractTagValue(string notes, string tag)
        {
            if (string.IsNullOrWhiteSpace(notes) || string.IsNullOrWhiteSpace(tag)) return null;
            int idx = notes.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            int start = idx + tag.Length;
            int end = notes.IndexOf(']', start);
            if (end < 0) return null;
            return notes.Substring(start, end - start);
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
