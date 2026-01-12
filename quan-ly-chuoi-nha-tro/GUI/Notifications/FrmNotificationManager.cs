using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmNotificationManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo tiêu đề/nội dung...";
        private const string BranchTagPrefix = "[CN:";
        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly int? _presetBranchId;
        private readonly bool _isStaffMode;

        // Tab Control
        private TabControl _tabControl;

        // Tab 1: Quản Lý Thông Báo
        private DataTable _rawTable;
        private DataGridView _grid;
        private ModernSearchBox _txtSearch;
        private ComboBox _cboStatus;
        private Label _lblCount;
        private Button _btnAdd, _btnEdit, _btnDelete, _btnMarkRead, _btnRefresh;
        private readonly Color _panelBorder = Color.FromArgb(225, 230, 236);

        // Tab 2: Nhắc Lịch Tự Động
        private Label _lblOverdueCount, _lblExpiredCount, _lblIncompleteCount;
        private Button _btnCreateReminder, _btnViewDetails, _btnSendNew;

        public FrmNotificationManager() : this(null, false)
        {
        }

        public FrmNotificationManager(int? branchId) : this(branchId, false)
        {
        }

        public FrmNotificationManager(int? branchId, bool isStaffMode)
        {
            _presetBranchId = branchId;
            _isStaffMode = isStaffMode;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            Text = "Thông báo & Nhắc lịch";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1400;
            Height = 800;
            BackColor = Color.FromArgb(245, 247, 250);

            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                ItemSize = new System.Drawing.Size(180, 32),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var tab1 = new TabPage { Text = "Quản Lý Thông Báo", BackColor = Color.FromArgb(245, 247, 250) };
            InitializeTab1(tab1);
            _tabControl.TabPages.Add(tab1);

            var tab2 = new TabPage { Text = "Nhắc Lịch Tự Động", BackColor = Color.FromArgb(245, 247, 250) };
            InitializeTab2(tab2);
            _tabControl.TabPages.Add(tab2);

            Controls.Add(_tabControl);
            Load += async (s, e) => await LoadAsync();
        }

        #region TAB 1 - Quản Lý Thông Báo
        private void InitializeTab1(TabPage tab)
        {
            _grid = MakeGrid();
            _grid.Dock = DockStyle.Fill;
            _grid.DoubleClick += async (s, e) => await EditSelectedAsync();
            _grid.CellFormatting += Grid_CellFormatting;

            _txtSearch = new ModernSearchBox
            {
                Width = 320,
                PlaceholderText = SearchPlaceholder,
                Margin = new Padding(0, 0, 12, 0) // Add right margin
            };
            _txtSearch.SearchTriggered += (s, e) => ApplyFilter();
            _cboStatus = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboStatus.Items.AddRange(new object[]
            {
                new StatusOption("Tất cả", null),
                new StatusOption("Chưa đọc", "Unread"),
                new StatusOption("Đã đọc", "Read"),
                new StatusOption("Đã gửi", "Sent")
            });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            _btnAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), async (s, e) => await AddNewAsync());
            _btnEdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), async (s, e) => await EditSelectedAsync());
            _btnDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteSelectedAsync());
            _btnMarkRead = MakeButton("Đã đọc", Color.FromArgb(46, 125, 50), async (s, e) => await MarkReadAsync());
            _btnRefresh = MakeButton("Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadAsync());
            _btnAdd.Visible = !_isStaffMode;
            _btnAdd.Enabled = !_isStaffMode;
            _btnEdit.Visible = !_isStaffMode;
            _btnEdit.Enabled = !_isStaffMode;
            _btnDelete.Visible = !_isStaffMode;
            _btnDelete.Enabled = !_isStaffMode;

            var top = new Panel { Dock = DockStyle.Top, Height = 104, Padding = new Padding(16, 10, 16, 10), BackColor = Color.White };
            top.Paint += (s, e) =>
            {
                using (var pen = new Pen(_panelBorder))
                    e.Graphics.DrawLine(pen, 0, top.Height - 1, top.Width, top.Height - 1);
            };
            
            // Use TableLayoutPanel with 2 rows
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F)); // Row 1: Buttons
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F)); // Row 2: Search
            
            // Row 1: Action Buttons
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            actions.Controls.Add(_btnAdd);
            actions.Controls.Add(_btnEdit);
            actions.Controls.Add(_btnDelete);
            actions.Controls.Add(_btnMarkRead);
            actions.Controls.Add(_btnRefresh);

            // Row 2: Search and Filters
            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            filters.Controls.Add(new Label { Text = "Tìm:", AutoSize = true, Margin = new Padding(0, 8, 6, 0), Font = new Font("Segoe UI", 9) });
            filters.Controls.Add(_txtSearch);
            filters.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Margin = new Padding(12, 8, 6, 0), Font = new Font("Segoe UI", 9) });
            _cboStatus.Margin = new Padding(0, 0, 12, 0);
            filters.Controls.Add(_cboStatus);
            filters.Controls.Add(new Label { Text = "  " });
            _lblCount.Margin = new Padding(8, 8, 0, 0);
            filters.Controls.Add(_lblCount);

            tableLayout.Controls.Add(actions, 0, 0);
            tableLayout.Controls.Add(filters, 0, 1);
            top.Controls.Add(tableLayout);

            tab.Controls.Add(_grid);
            tab.Controls.Add(top);
        }
        #endregion

        #region TAB 2 - Nhắc Lịch Tự Động
        private void InitializeTab2(TabPage tab)
        {
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White, Padding = new Padding(12) };
            var lblTitle = new Label { Text = "Tình trạng hệ thống - Nhắc nhở tự động", AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0, 0, 0) };
            pnlHeader.Controls.Add(lblTitle);

            var pnlCards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 150, BackColor = Color.FromArgb(245, 247, 250), Padding = new Padding(12), WrapContents = true, FlowDirection = FlowDirection.LeftToRight, AutoScroll = false };

            var card1 = CreateStatCard("⚠ Công Nợ Quá Hạn", "0", Color.FromArgb(211, 47, 47));
            _lblOverdueCount = card1.Item2;
            pnlCards.Controls.Add(card1.Item1);

            var card2 = CreateStatCard("⏱ Hợp Động Hết Hạn", "0", Color.FromArgb(255, 152, 0));
            _lblExpiredCount = card2.Item2;
            pnlCards.Controls.Add(card2.Item1);

            var card3 = CreateStatCard("📊 Chưa Nhập Chỉ Số", "0", Color.FromArgb(0, 150, 200));
            _lblIncompleteCount = card3.Item2;
            pnlCards.Controls.Add(card3.Item1);

            var pnlButtons = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(245, 247, 250), Padding = new Padding(12) };
            _btnCreateReminder = MakeButton("Tạo Nhắc Nhở", Color.FromArgb(46, 125, 50), async (s, e) => await CreateReminderAsync());
            _btnCreateReminder.Width = 150;
            _btnViewDetails = MakeButton("Xem Chi Tiết", Color.FromArgb(0, 122, 204), async (s, e) => await ViewDetailsAsync());
            _btnViewDetails.Width = 150;
            _btnSendNew = MakeButton("Gửi Thông Báo Mới", Color.FromArgb(0, 150, 136), async (s, e) => await OpenSendNotificationAsync());
            _btnSendNew.Width = 160;
            _btnCreateReminder.Visible = !_isStaffMode;
            _btnCreateReminder.Enabled = !_isStaffMode;
            _btnSendNew.Visible = !_isStaffMode;
            _btnSendNew.Enabled = !_isStaffMode;

            var btnContainer = new FlowLayoutPanel { Dock = DockStyle.Left, AutoSize = true, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent };
            btnContainer.Controls.Add(_btnCreateReminder);
            btnContainer.Controls.Add(_btnViewDetails);
            btnContainer.Controls.Add(_btnSendNew);
            pnlButtons.Controls.Add(btnContainer);

            tab.Controls.Add(pnlButtons);
            tab.Controls.Add(pnlCards);
            tab.Controls.Add(pnlHeader);
        }

        private (Panel, Label) CreateStatCard(string title, string count, Color cardColor)
        {
            var pnl = new Panel { Width = 280, Height = 120, BackColor = Color.White, BorderStyle = BorderStyle.None, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(12) };
            var pnlColor = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = cardColor };
            var lblTitle = new Label { Dock = DockStyle.Top, Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = cardColor, AutoSize = false, Height = 24 };
            var lblCount = new Label { Dock = DockStyle.Fill, Text = count, Font = new Font("Segoe UI", 32, FontStyle.Bold), ForeColor = cardColor, TextAlign = ContentAlignment.MiddleLeft };
            pnl.Controls.Add(lblCount);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(pnlColor);
            return (pnl, lblCount);
        }
        #endregion

        #region Load & Events
        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                _rawTable = await _bll.GetNotificationsAsync();
                _rawTable = FilterByBranch(_rawTable);
                _grid.DataSource = _rawTable;
                ApplyGridPresentation();
                ApplyFilter();
                await LoadAutomationStatsAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "LoadNotifications", "Không thể tải thông báo");
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

        private async System.Threading.Tasks.Task LoadAutomationStatsAsync()
        {
            try
            {
                var invoices = await _bll.GetInvoicesAsync();
                invoices = FilterByBranch(invoices);
                int overdueCount = invoices?.AsEnumerable().Where(r => r["Status"]?.ToString() == "Chưa thanh toán").Count() ?? 0;
                _lblOverdueCount.Text = overdueCount.ToString();

                var contracts = await _bll.GetContractsAsync();
                contracts = FilterByBranch(contracts);
                int expiredCount = contracts?.AsEnumerable().Where(r => { if (r["EndDate"] is DateTime endDate) return endDate < DateTime.Today; return false; }).Count() ?? 0;
                _lblExpiredCount.Text = expiredCount.ToString();

                var utilities = await _bll.GetUtilitiesAsync();
                utilities = FilterByBranch(utilities);
                int incompleteCount = utilities?.AsEnumerable().Where(r => r["Value"] == null || r["Value"] == DBNull.Value || string.IsNullOrEmpty(r["Value"]?.ToString())).Count() ?? 0;
                _lblIncompleteCount.Text = incompleteCount.ToString();
            }
            catch { }
        }

        private DataTable FilterByBranch(DataTable table)
        {
            if (!_presetBranchId.HasValue || table == null)
                return table;

            if (table.Columns.Contains("BranchId"))
            {
                var filtered = table.Clone();
                foreach (DataRow row in table.Rows)
                {
                    if (!int.TryParse(row["BranchId"]?.ToString(), out var bid)) continue;
                    if (bid == _presetBranchId.Value)
                        filtered.ImportRow(row);
                }
                return filtered;
            }

            string tag = BuildBranchTag(_presetBranchId.Value);
            var tagged = table.Clone();
            foreach (DataRow row in table.Rows)
            {
                var title = ReadString(row, "Title");
                var message = ReadString(row, "Message");
                if (ContainsBranchTag(title, tag) || ContainsBranchTag(message, tag))
                    tagged.ImportRow(row);
            }
            return tagged;
        }
        #endregion

        #region Tab 1 Methods
        private void ApplyGridPresentation()
        {
            SetHeader("NotificationId", "ID");
            SetHeader("UserId", "Người dùng");
            SetHeader("Title", "Tiêu đề");
            SetHeader("Status", "Trạng thái");
            SetHeader("CreatedDate", "Ngày tạo");
            SetHeader("Message", "Nội dung");
            FormatDateTime("CreatedDate");
            SetDisplayOrder("NotificationId", "Title", "Status", "CreatedDate", "UserId", "Message");
            if (_grid.Columns.Contains("Message")) _grid.Columns["Message"].FillWeight = 220;
        }

        private void ApplyFilter()
        {
            if (_rawTable == null) return;
            string keyword = (_txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            string status = (_cboStatus.SelectedItem as StatusOption)?.Value;
            var rows = _rawTable.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(status) && _rawTable.Columns.Contains("Status"))
                rows = rows.Where(r => string.Equals(r["Status"]?.ToString(), status, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(keyword))
                rows = rows.Where(r => Contains(r, "Title", keyword) || Contains(r, "Message", keyword));
            var filtered = rows.Any() ? rows.CopyToDataTable() : _rawTable.Clone();
            _grid.DataSource = filtered;
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
        }

        private async System.Threading.Tasks.Task AddNewAsync()
        {
            using (var frm = new FrmNotificationEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK) await LoadAsync();
            }
        }

        private async System.Threading.Tasks.Task EditSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một dòng để sửa");
                return;
            }
            using (var frm = new FrmNotificationEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK) await LoadAsync();
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một dòng để xóa");
                return;
            }
            int id = ReadInt(row, "NotificationId");
            string title = ReadString(row, "Title") ?? id.ToString();
            if (!ModernConfirmDialog.ConfirmDanger($"Xóa thông báo \"{title}\"?")) return;
            try
            {
                await _bll.DeleteNotificationAsync(id);
                ToastNotification.Success("Xóa thông báo thành công");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "DeleteNotification");
            }
        }

        private async System.Threading.Tasks.Task MarkReadAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một dòng để đánh dấu đã đọc");
                return;
            }
            int id = ReadInt(row, "NotificationId");
            string current = ReadString(row, "Status");
            if (string.Equals(current, "Read", StringComparison.OrdinalIgnoreCase))
            {
                ToastNotification.Info("Đã ở trạng thái đã đọc");
                return;
            }
            try
            {
                int? userId = TryReadIntNullable(row, "UserId");
                await _bll.UpdateNotificationAsync(id, userId, ReadString(row, "Title"), ReadString(row, "Message"), "Read");
                ToastNotification.Success("Đánh dấu đã đọc thành công");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "MarkRead");
            }
        }

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            var drv = _grid.CurrentRow.DataBoundItem as DataRowView;
            return drv?.Row;
        }
        #endregion

        #region Tab 2 & 3 Methods
        private async System.Threading.Tasks.Task CreateReminderAsync()
        {
            try
            {
                string msg = $"Gửi nhắc nhở về:\n• Công nợ quá hạn: {_lblOverdueCount.Text}\n• HĐ hết hạn: {_lblExpiredCount.Text}\n• Chưa nhập chỉ số: {_lblIncompleteCount.Text}";
                if (!ModernConfirmDialog.Confirm(msg, "Tạo nhắc nhở?")) return;
                await _bll.AddNotificationAsync(null, "Nhắc nhở từ hệ thống", msg, "Sent");
                ToastNotification.Success("Gửi nhắc nhở thành công!");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "CreateReminder");
            }
        }

        private async System.Threading.Tasks.Task ViewDetailsAsync()
        {
            await System.Threading.Tasks.Task.Delay(0);
            ToastNotification.Info("Tính năng đang phát triển");
        }

        private async System.Threading.Tasks.Task OpenSendNotificationAsync()
        {
            using (var frm = new FrmSendNotification(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadAsync();
            }
        }
        #endregion

        #region Helper Methods
        private static DataGridView MakeGrid()
        {
            var g = new DataGridView { ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None };
            UiKit.StyleGrid(g);
            g.ColumnHeadersHeight = 34;
            return g;
        }

        private static Button MakeButton(string text, Color backColor, EventHandler onClick)
        {
            var b = new ModernButton { Text = text, Width = 96, Height = 34, FlatStyle = FlatStyle.Flat, BaseColor = backColor, BackColor = Color.Transparent, ForeColor = Color.White, Margin = new Padding(0, 0, 8, 0), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name != "Status") return;
            if (e.Value == null) return;
            e.Value = ToVietnameseStatus(e.Value.ToString());
            e.FormattingApplied = true;
        }


        private void SetHeader(string columnName, string headerText)
        {
            if (_grid.Columns.Contains(columnName)) _grid.Columns[columnName].HeaderText = headerText;
        }

        private void FormatDateTime(string columnName)
        {
            if (_grid.Columns.Contains(columnName)) _grid.Columns[columnName].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        private void SetDisplayOrder(params string[] order)
        {
            int idx = 0;
            foreach (var name in order)
            {
                if (_grid.Columns.Contains(name)) { _grid.Columns[name].DisplayIndex = idx; idx++; }
            }
        }

        private static bool Contains(DataRow row, string column, string keywordLower)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column)) return false;
            var v = row[column];
            if (v == null || v == DBNull.Value) return false;
            return v.ToString().ToLowerInvariant().Contains(keywordLower);
        }

        private static string BuildBranchTag(int branchId)
        {
            return $"{BranchTagPrefix}{branchId}]";
        }

        private static bool ContainsBranchTag(string text, string tag)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(tag)) return false;
            return text.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0;
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

        private static string ToVietnameseStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "Chưa xác định";
            switch (status.Trim().ToLowerInvariant())
            {
                case "unread":
                    return "Chưa đọc";
                case "read":
                    return "Đã đọc";
                case "sent":
                    return "Đã gửi";
                default:
                    return status;
            }
        }

        private sealed class StatusOption
        {
            public StatusOption(string text, string value)
            {
                Text = text;
                Value = value;
            }

            public string Text { get; }
            public string Value { get; }

            public override string ToString() => Text;
        }
        #endregion
    }

    public class FrmSendNotification : Form
    {
        private AdminDataBLL _bll;
        private TextBox txtTitle, txtMessage;
        private ComboBox cboRecipient;
        private Label lblCount;
        private Button btnSend, btnCancel;

        public FrmSendNotification(AdminDataBLL bll = null)
        {
            _bll = bll ?? new AdminDataBLL();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Gửi Thông Báo Mới";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(700, 550);
            BackColor = Color.White;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(0, 122, 204), Padding = new Padding(20) };
            var lblHeader = new Label { Text = "Gửi thông báo cho toàn bộ người dùng", AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            pnlTop.Controls.Add(lblHeader);

            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), AutoScroll = true, BackColor = Color.White };

            int top = 10;
            var lbl1 = new Label { Text = "Tiêu đề (*)", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(0, top), Width = 120, Height = 30, TextAlign = ContentAlignment.MiddleLeft };
            txtTitle = new TextBox { Location = new Point(130, top), Width = 550, Height = 32, Font = new Font("Segoe UI", 10) };
            pnlBody.Controls.Add(lbl1);
            pnlBody.Controls.Add(txtTitle);

            top += 50;
            var lbl2 = new Label { Text = "Gửi tới (*)", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(0, top), Width = 120, Height = 30, TextAlign = ContentAlignment.MiddleLeft };
            cboRecipient = new ComboBox { Location = new Point(130, top), Width = 550, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cboRecipient.Items.AddRange(new object[] { "Tất cả người dùng", "Chỉ Admin", "Chỉ Staff" });
            cboRecipient.SelectedIndex = 0;
            pnlBody.Controls.Add(lbl2);
            pnlBody.Controls.Add(cboRecipient);

            top += 50;
            var lbl3 = new Label { Text = "Người nhận", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(0, top), Width = 120, Height = 30, TextAlign = ContentAlignment.MiddleLeft };
            lblCount = new Label { Text = "0 người", Font = new Font("Segoe UI", 10), Location = new Point(130, top), Width = 550, Height = 30, TextAlign = ContentAlignment.MiddleLeft };
            pnlBody.Controls.Add(lbl3);
            pnlBody.Controls.Add(lblCount);

            top += 50;
            var lbl4 = new Label { Text = "Nội dung (*)", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(0, top), Width = 120, Height = 30, TextAlign = ContentAlignment.TopLeft };
            txtMessage = new TextBox { Location = new Point(130, top), Width = 550, Height = 200, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = true, Font = new Font("Segoe UI", 10) };
            pnlBody.Controls.Add(lbl4);
            pnlBody.Controls.Add(txtMessage);

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = Color.FromArgb(245, 247, 250), Padding = new Padding(20) };
            btnSend = new ModernButton { Text = "Gửi Thông Báo", Width = 140, Height = 40, BaseColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Anchor = AnchorStyles.Right, BackColor = Color.Transparent };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += async (s, e) => await SendAsync();

            btnCancel = new ModernButton { Text = "Hủy", Width = 140, Height = 40, BaseColor = Color.White, ForeColor = Color.FromArgb(64, 64, 64), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Anchor = AnchorStyles.Right, BackColor = Color.Transparent };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSend);
            btnCancel.Location = new Point(pnlBottom.Width - btnCancel.Width - 12, 15);
            btnSend.Location = new Point(btnCancel.Left - btnSend.Width - 12, 15);

            AcceptButton = btnSend;
            CancelButton = btnCancel;

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);

            Load += async (s, e) => { await System.Threading.Tasks.Task.Delay(0); lblCount.Text = "2 người"; };
        }

        private async System.Threading.Tasks.Task SendAsync()
        {
            string title = txtTitle.Text.Trim();
            string message = txtMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                ToastNotification.Warning("Vui lòng nhập tiêu đề");
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                ToastNotification.Warning("Vui lòng nhập nội dung");
                return;
            }

            try
            {
                await _bll.AddNotificationAsync(null, title, message, "Sent");
                ToastNotification.Success("Gửi thông báo thành công!");
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi: " + ex.Message);
            }
        }
    }
}
