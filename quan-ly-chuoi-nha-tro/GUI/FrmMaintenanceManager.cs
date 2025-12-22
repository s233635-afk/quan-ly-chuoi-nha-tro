using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmMaintenanceManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo số phiếu/phòng/nội dung...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly int? _presetBranchId;

        private DataTable _rawTable;
        private DataTable _branchTable;
        private System.Collections.Generic.HashSet<int> _allowedBranchIds;

        private FlowLayoutPanel _cardsHost;
        private TextBox _txtSearch;
        private ComboBox _cboBranch;
        private ComboBox _cboStatus;
        private Label _lblCount;
        private (Panel Card, DataRow Row)? _selectedItem;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnDone;
        private Button _btnRefresh;

        public FrmMaintenanceManager() : this(null)
        {
        }

        public FrmMaintenanceManager(int? branchId)
        {
            _presetBranchId = branchId;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            Text = "Bảo trì & sự cố";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1400;
            Height = 800;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);

            // ===== TOOLBAR PANEL WITH TITLE =====
            var pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.White,
                Padding = new Padding(0),
                BorderStyle = BorderStyle.None
            };

            // Actions bar
            var pnlActionBar = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 40,
                BackColor = Color.White,
                Padding = new Padding(12, 5, 12, 5),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Buttons
            _btnAdd = MakeButton("➕ Thêm", Color.FromArgb(0, 122, 204), async (s, e) => await AddNewAsync());
            _btnEdit = MakeButton("✎ Sửa", Color.FromArgb(0, 122, 204), async (s, e) => await EditSelectedAsync());
            _btnDelete = MakeButton("🗑 Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteSelectedAsync());
            _btnDone = MakeButton("✓ Hoàn tất", Color.FromArgb(46, 125, 50), async (s, e) => await MarkDoneAsync());
            _btnRefresh = MakeButton("⟳ Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadAsync());
            _btnEdit.Enabled = false;
            _btnDelete.Enabled = false;
            _btnDone.Enabled = false;

            var pnlActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            pnlActions.Controls.Add(_btnAdd);
            pnlActions.Controls.Add(_btnEdit);
            pnlActions.Controls.Add(_btnDelete);
            pnlActions.Controls.Add(_btnDone);
            pnlActions.Controls.Add(_btnRefresh);

            // Search and Filters
            _txtSearch = MakeSearchBox(SearchPlaceholder, () => ApplyFilter());
            _cboBranch = new ComboBox
            {
                Width = 180,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9)
            };
            _cboBranch.SelectedIndexChanged += (s, e) => ApplyFilter();

            _cboStatus = new ComboBox
            {
                Width = 160,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9)
            };
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Created", "InProgress", "Completed", "Cancelled" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblCount = new Label
            {
                AutoSize = true,
                Text = "Tổng: 0",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(229, 57, 53),
                Margin = new Padding(15, 4, 0, 0)
            };

            var pnlFilters = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 3, 0, 0)
            };
            pnlFilters.Controls.Add(new Label { Text = "🔍 Tìm:", AutoSize = true, Margin = new Padding(0, 4, 6, 0), Font = new Font("Segoe UI", 9) });
            pnlFilters.Controls.Add(_txtSearch);
            pnlFilters.Controls.Add(new Label { Text = "Chi nhánh:", AutoSize = true, Margin = new Padding(15, 4, 6, 0), Font = new Font("Segoe UI", 9) });
            pnlFilters.Controls.Add(_cboBranch);
            pnlFilters.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Margin = new Padding(15, 4, 6, 0), Font = new Font("Segoe UI", 9) });
            pnlFilters.Controls.Add(_cboStatus);
            pnlFilters.Controls.Add(_lblCount);

            pnlActionBar.Controls.Add(pnlActions);
            pnlActionBar.Controls.Add(pnlFilters);
            pnlToolbar.Controls.Add(pnlActionBar);

            _cardsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = BackColor,
                Padding = new Padding(12)
            };

            // Add all controls
            Controls.Add(_cardsHost);
            Controls.Add(pnlToolbar);

            Load += async (s, e) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                await LoadBranchesAsync();
                _rawTable = await _bll.GetMaintenanceAsync();
                TextFixer.FixDataTable(_rawTable, "RoomNumber", "IssueDescription", "Priority", "Status", "Notes");
                _rawTable = AdminBranchScope.FilterByBranchIds(_rawTable, _allowedBranchIds);
                _selectedItem = null;
                ApplyFilter();
                UpdateActionState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải bảo trì: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private async System.Threading.Tasks.Task LoadBranchesAsync()
        {
            try
            {
                var dt = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                TextFixer.FixDataTable(dt, "BranchCode", "BranchName");
                _allowedBranchIds = AdminBranchScope.GetAllowedBranchIds(dt);
                _branchTable = new DataTable();
                _branchTable.Columns.Add("BranchId", typeof(int));
                _branchTable.Columns.Add("BranchDisplay", typeof(string));
                _branchTable.Rows.Add(0, "Tất cả");

                if (dt != null && dt.Columns.Contains("BranchId") && dt.Columns.Contains("BranchName"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int id = 0;
                        try { id = Convert.ToInt32(r["BranchId"]); } catch { }
                        string code = r.Table.Columns.Contains("BranchCode") ? r["BranchCode"]?.ToString() : null;
                        string name = r["BranchName"]?.ToString();
                        string display = $"{code} - {name}".Trim(' ', '-');
                        if (string.IsNullOrWhiteSpace(display)) display = "Chi nhánh " + id;
                        _branchTable.Rows.Add(id, display);
                    }
                }

                _cboBranch.DataSource = _branchTable;
                _cboBranch.DisplayMember = "BranchDisplay";
                _cboBranch.ValueMember = "BranchId";
                if (_presetBranchId.HasValue)
                {
                    _cboBranch.SelectedValue = _presetBranchId.Value;
                    _cboBranch.Enabled = false;
                }
            }
            catch
            {
                _cboBranch.Items.Clear();
                _cboBranch.Items.Add("Tất cả");
                _cboBranch.SelectedIndex = 0;
            }
        }

        private void RenderCards(DataTable table)
        {
            if (_cardsHost == null) return;
            _cardsHost.SuspendLayout();
            _cardsHost.Controls.Clear();

            if (table == null || table.Rows.Count == 0)
            {
                _cardsHost.ResumeLayout();
                return;
            }

            foreach (DataRow row in table.Rows)
            {
                _cardsHost.Controls.Add(CreateCard(row));
            }

            _cardsHost.ResumeLayout();
        }

        private Control CreateCard(DataRow row)
        {
            string ticketNo = ReadString(row, "TicketNumber") ?? ReadString(row, "TicketId") ?? "—";
            string room = ReadString(row, "RoomNumber") ?? ReadString(row, "RoomId") ?? "—";
            string status = ReadString(row, "Status") ?? "Created";
            string priority = ReadString(row, "Priority") ?? "Normal";
            string issue = ReadString(row, "IssueDescription") ?? "—";
            string notes = ReadString(row, "Notes") ?? string.Empty;
            string created = FormatDate(ReadString(row, "CreatedDate"));
            string completed = FormatDate(ReadString(row, "CompletedDate"));
            string assignee = ReadString(row, "AssignedToUserId") ?? "—";

            var card = new Panel
            {
                Width = 340,
                Height = 170,
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(1),
                Cursor = Cursors.Hand,
                Tag = row
            };

            var statusStrip = new Panel
            {
                Dock = DockStyle.Left,
                Width = 6,
                BackColor = GetStatusColor(status)
            };

            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 8) };

            var lblTitle = new Label
            {
                Text = $"{ticketNo} • Phòng {room}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                AutoSize = false,
                Width = 300,
                Height = 22,
                Location = new Point(0, 0)
            };

            var lblStatus = new Label
            {
                Text = ToVietnameseStatus(status),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = GetStatusColor(status),
                AutoSize = false,
                Width = 120,
                Height = 20,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(190, 0)
            };

            var lblPriority = new Label
            {
                Text = $"Ưu tiên: {ToVietnamesePriority(priority)}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = GetPriorityColor(priority),
                AutoSize = false,
                Width = 300,
                Height = 18,
                Location = new Point(0, 26)
            };

            var lblDates = new Label
            {
                Text = $"Tạo: {created} | Hoàn tất: {completed}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.DimGray,
                AutoSize = false,
                Width = 300,
                Height = 18,
                Location = new Point(0, 46)
            };

            var lblIssue = new Label
            {
                Text = $"Sự cố: {issue}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(50, 50, 50),
                AutoSize = false,
                Width = 300,
                Height = 36,
                Location = new Point(0, 66),
                AutoEllipsis = true
            };

            var lblNotes = new Label
            {
                Text = $"Ghi chú: {notes}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(90, 90, 90),
                AutoSize = false,
                Width = 300,
                Height = 18,
                Location = new Point(0, 106),
                AutoEllipsis = true
            };

            var lblAssignee = new Label
            {
                Text = $"NV xử lý: {assignee}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(90, 90, 90),
                AutoSize = false,
                Width = 300,
                Height = 18,
                Location = new Point(0, 126),
                AutoEllipsis = true
            };

            content.Controls.Add(lblTitle);
            content.Controls.Add(lblStatus);
            content.Controls.Add(lblPriority);
            content.Controls.Add(lblDates);
            content.Controls.Add(lblIssue);
            content.Controls.Add(lblNotes);
            content.Controls.Add(lblAssignee);

            var border = new Panel { Dock = DockStyle.Fill };
            border.Paint += (s, e) =>
            {
                using (var pen = new Pen(IsSelected(card) ? Color.FromArgb(0, 122, 204) : Color.FromArgb(220, 230, 240), 1.4f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            border.Controls.Add(content);
            card.Controls.Add(border);
            card.Controls.Add(statusStrip);

            void SelectAction()
            {
                SelectCard(card, row);
            }

            card.Click += (s, e) => SelectAction();
            foreach (Control c in content.Controls) c.Click += (s, e) => SelectAction();
            card.DoubleClick += async (s, e) => await EditSelectedAsync();
            foreach (Control c in content.Controls) c.DoubleClick += async (s, e) => await EditSelectedAsync();

            return card;
        }

        private void SelectCard(Panel card, DataRow row)
        {
            if (_selectedItem.HasValue && _selectedItem.Value.Card != null)
                _selectedItem.Value.Card.Invalidate();
            _selectedItem = (card, row);
            UpdateActionState();
            card.Invalidate();
        }

        private bool IsSelected(Panel card)
        {
            return _selectedItem.HasValue && ReferenceEquals(_selectedItem.Value.Card, card);
        }

        private void UpdateActionState()
        {
            bool has = _selectedItem.HasValue;
            _btnEdit.Enabled = has;
            _btnDelete.Enabled = has;
            _btnDone.Enabled = has;
        }

        private static string FormatDate(string raw)
        {
            if (DateTime.TryParse(raw, out var dt))
                return dt.ToString("dd/MM/yyyy");
            return "—";
        }

        private static string ToVietnameseStatus(string status)
        {
            switch ((status ?? string.Empty).Trim())
            {
                case "Created":
                    return "Mới";
                case "InProgress":
                    return "Đang xử lý";
                case "Completed":
                    return "Hoàn tất";
                case "Cancelled":
                    return "Đã hủy";
                default:
                    return status;
            }
        }

        private static string ToVietnamesePriority(string priority)
        {
            switch ((priority ?? string.Empty).Trim())
            {
                case "Low":
                    return "Thấp";
                case "High":
                    return "Cao";
                case "Urgent":
                    return "Khẩn cấp";
                default:
                    return string.IsNullOrWhiteSpace(priority) ? "Bình thường" : priority;
            }
        }

        private static Color GetStatusColor(string status)
        {
            switch ((status ?? string.Empty).Trim())
            {
                case "Completed":
                    return Color.SeaGreen;
                case "InProgress":
                    return Color.DarkOrange;
                case "Cancelled":
                    return Color.Firebrick;
                default:
                    return Color.Gray;
            }
        }

        private static Color GetPriorityColor(string priority)
        {
            switch ((priority ?? string.Empty).Trim())
            {
                case "High":
                    return Color.Firebrick;
                case "Urgent":
                    return Color.DarkRed;
                case "Low":
                    return Color.DimGray;
                default:
                    return Color.FromArgb(40, 40, 40);
            }
        }

        private void ApplyFilter()
        {
            if (_rawTable == null) return;

            string rawKeyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholder) rawKeyword = string.Empty;
            string keyword = rawKeyword.ToLowerInvariant();

            int branchId = _cboBranch.SelectedValue is int b ? b : 0;
            string status = _cboStatus.SelectedIndex > 0 ? _cboStatus.Text : null;

            var rows = _rawTable.AsEnumerable();

            if (branchId > 0 && _rawTable.Columns.Contains("BranchId"))
                rows = rows.Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == branchId);

            if (!string.IsNullOrWhiteSpace(status) && _rawTable.Columns.Contains("Status"))
                rows = rows.Where(r => string.Equals(r["Status"]?.ToString(), status, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rows = rows.Where(r =>
                    Contains(r, "TicketNumber", keyword) ||
                    Contains(r, "RoomNumber", keyword) ||
                    Contains(r, "IssueDescription", keyword));
            }

            var filtered = rows.Any() ? rows.CopyToDataTable() : _rawTable.Clone();
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
            RenderCards(filtered);
            _selectedItem = null;
            UpdateActionState();
        }

        private async System.Threading.Tasks.Task AddNewAsync()
        {
            using (var frm = new FrmMaintenanceEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task EditSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmMaintenanceEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = ReadInt(row, "TicketId");
            string no = ReadString(row, "TicketNumber") ?? id.ToString();

            if (MessageBox.Show($"Xóa phiếu \"{no}\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteMaintenanceTicketAsync(id);
                await LoadAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa phiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task MarkDoneAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để hoàn tất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = ReadInt(row, "TicketId");
            string no = ReadString(row, "TicketNumber") ?? id.ToString();
            string currentStatus = ReadString(row, "Status");

            if (string.Equals(currentStatus, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Phiếu đã ở trạng thái Completed.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Chuyển phiếu \"{no}\" sang Completed?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.UpdateMaintenanceTicketAsync(
                    id,
                    ReadInt(row, "RoomId"),
                    ReadString(row, "RequestorType"),
                    TryReadIntNullable(row, "RequestorId"),
                    ReadString(row, "IssueDescription"),
                    ReadString(row, "Priority"),
                    TryReadIntNullable(row, "AssignedToUserId"),
                    "Completed",
                    DateTime.Now,
                    ReadString(row, "Notes"));

                await LoadAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật trạng thái: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataRow GetCurrentRow()
        {
            return _selectedItem?.Row;
        }

        private static Button MakeButton(string text, Color backColor, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                Width = 110,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 6, 0),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ColorAdjust(backColor, 10);
            b.Click += onClick;
            return b;
        }

        private static Color ColorAdjust(Color c, int delta)
        {
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, c.R + delta)),
                Math.Max(0, Math.Min(255, c.G + delta)),
                Math.Max(0, Math.Min(255, c.B + delta))
            );
        }

        private static TextBox MakeSearchBox(string placeholder, Action onChanged)
        {
            var tb = new TextBox
            {
                Width = 280,
                Height = 32,
                ForeColor = Color.Gray,
                Text = placeholder,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };
            tb.GotFocus += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = string.Empty;
                    tb.ForeColor = Color.Black;
                }
            };
            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.ForeColor = Color.Gray;
                }
            };
            tb.TextChanged += (s, e) => onChanged?.Invoke();
            return tb;
        }


        private static bool Contains(DataRow row, string column, string keywordLower)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column)) return false;
            var v = row[column];
            if (v == null || v == DBNull.Value) return false;
            return v.ToString().ToLowerInvariant().Contains(keywordLower);
        }

        private static string ReadString(DataRow row, params string[] cols)
        {
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
    }
}
