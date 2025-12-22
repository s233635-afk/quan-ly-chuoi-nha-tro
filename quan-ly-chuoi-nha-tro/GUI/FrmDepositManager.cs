using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmDepositManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo khách/phòng/trạng thái...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly int? _branchId;
        private System.Collections.Generic.HashSet<int> _allowedBranchIds;
        private DataTable _table;
        private DataGridView _grid;
        private TextBox _txtSearch;
        private ComboBox _cboStatus;
        private Label _lblCount;
        private Label _lblTotal;
        private Button _btnAdd, _btnEdit, _btnDelete, _btnRefresh;
        private Button _btnConfirm, _btnReturn;

        public FrmDepositManager() : this(null)
        {
        }

        public FrmDepositManager(int? branchId)
        {
            _branchId = branchId;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            this.Text = "Đặt cọc";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 1280;
            this.Height = 720;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _grid.DoubleClick += (s, e) => EditSelected();
            _grid.MouseDown += (s, e) => HandleGridMouseDown(e);
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(12, 99, 166);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.75f, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            _grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 255);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 252);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _grid.ColumnHeadersHeight = 38;
            _grid.RowTemplate.Height = 34;
            _grid.GridColor = Color.FromArgb(225, 232, 240);
            _grid.CellFormatting += Grid_CellFormatting;

            _txtSearch = new TextBox { Width = 260 };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _txtSearch.GotFocus += (s, e) =>
            {
                if (_txtSearch.Text == SearchPlaceholder)
                {
                    _txtSearch.Text = string.Empty;
                    _txtSearch.ForeColor = Color.Black;
                }
            };
            _txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _txtSearch.Text = SearchPlaceholder;
                    _txtSearch.ForeColor = Color.Gray;
                }
            };

            _cboStatus = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Chờ xử lý", "Đã xác nhận", "Hoàn cọc", "Hủy" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblTotal = new Label { AutoSize = true, Text = "Tổng cọc: 0", ForeColor = Color.FromArgb(70, 70, 70) };

            _btnAdd = MakeButton("➕ Thêm", Color.FromArgb(0, 122, 204), (s, e) => AddNew());
            _btnEdit = MakeButton("✏️ Sửa", Color.FromArgb(0, 122, 204), (s, e) => EditSelected());
            _btnDelete = MakeButton("🗑 Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteSelectedAsync());
            _btnConfirm = MakeButton("✔ Xác nhận cọc", Color.FromArgb(46, 125, 50), async (s, e) => await MarkStatusAsync("Confirmed"));
            _btnReturn = MakeButton("💸 Hoàn cọc", Color.FromArgb(121, 85, 72), async (s, e) => await MarkReturnedAsync());
            _btnRefresh = MakeButton("🔄 Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadDataAsync());

            var header = new Panel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(16, 10, 16, 10), BackColor = Color.White };
            var lblTitle = new Label
            {
                Text = "Đặt cọc",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159)
            };

            var summary = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                BackColor = Color.Transparent
            };
            _lblCount.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _lblTotal.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            summary.Controls.Add(_lblCount);
            summary.Controls.Add(_lblTotal);

            header.Controls.Add(summary);
            header.Controls.Add(lblTitle);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 72, Padding = new Padding(16, 14, 16, 14), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            actions.Controls.Add(_btnAdd);
            actions.Controls.Add(_btnEdit);
            actions.Controls.Add(_btnDelete);
            actions.Controls.Add(_btnConfirm);
            actions.Controls.Add(_btnReturn);
            actions.Controls.Add(_btnRefresh);

            var searchHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var pnlSearch = new Panel
            {
                BackColor = Color.FromArgb(245, 247, 250),
                Height = 34,
                Width = 280,
                Padding = new Padding(10, 7, 10, 7)
            };
            _txtSearch.BorderStyle = BorderStyle.None;
            _txtSearch.Parent = pnlSearch;
            _txtSearch.Location = new Point(2, 6);
            _txtSearch.Width = pnlSearch.Width - 16;
            pnlSearch.Resize += (s, e) => _txtSearch.Width = pnlSearch.Width - 16;

            _txtSearch.Text = SearchPlaceholder;
            _txtSearch.ForeColor = Color.Gray;

            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, Location = new Point(0, 9), ForeColor = Color.FromArgb(70, 70, 70) };
            pnlSearch.Location = new Point(lblSearch.Right + 6, 10);

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, ForeColor = Color.FromArgb(70, 70, 70) };
            lblStatus.Location = new Point(pnlSearch.Right + 14, 9);
            _cboStatus.Location = new Point(lblStatus.Right + 6, 6);

            searchHost.Controls.Add(lblSearch);
            searchHost.Controls.Add(pnlSearch);
            searchHost.Controls.Add(lblStatus);
            searchHost.Controls.Add(_cboStatus);
            searchHost.Resize += (s, e) =>
            {
                pnlSearch.Location = new Point(lblSearch.Right + 6, 10);
                lblStatus.Location = new Point(pnlSearch.Right + 14, 9);
                _cboStatus.Location = new Point(lblStatus.Right + 6, 6);
            };

            toolbar.Controls.Add(searchHost);
            toolbar.Controls.Add(actions);

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = this.BackColor };
            gridHost.Controls.Add(_grid);

            this.Controls.Add(gridHost);
            this.Controls.Add(toolbar);
            this.Controls.Add(header);
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                await EnsureAllowedBranchScopeAsync();
                _table = await _bll.GetDepositsAsync();
                _table = _branchId.HasValue ? FilterByBranch(_table, _branchId) : AdminBranchScope.FilterByBranchIds(_table, _allowedBranchIds);
                await EnrichDepositsAsync(_table);
                _grid.DataSource = _table;
                ApplyGridPresentation();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải đặt phòng/cọc: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadDataAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private async System.Threading.Tasks.Task EnrichDepositsAsync(DataTable deposits)
        {
            if (deposits == null) return;

            DataTable tenants = null;
            DataTable rooms = null;
            try
            {
                tenants = await _bll.GetTenantsAsync();
                rooms = await _bll.GetRoomsAsync();
                rooms = _branchId.HasValue ? FilterByBranch(rooms, _branchId) : AdminBranchScope.FilterByBranchIds(rooms, _allowedBranchIds);
            }
            catch
            {
                return;
            }

            var tenantMap = tenants?.Columns.Contains("TenantId") == true
                ? tenants.AsEnumerable()
                    .Where(r => int.TryParse(r["TenantId"]?.ToString(), out _))
                    .GroupBy(r => Convert.ToInt32(r["TenantId"]))
                    .ToDictionary(g => g.Key, g => tenants.Columns.Contains("FullName") ? g.First()["FullName"]?.ToString() : ("Tenant " + g.Key))
                : null;

            var roomMap = rooms?.Columns.Contains("RoomId") == true
                ? rooms.AsEnumerable()
                    .Where(r => int.TryParse(r["RoomId"]?.ToString(), out _))
                    .GroupBy(r => Convert.ToInt32(r["RoomId"]))
                    .ToDictionary(g => g.Key, g => rooms.Columns.Contains("RoomNumber") ? g.First()["RoomNumber"]?.ToString() : ("Phòng " + g.Key))
                : null;

            if (!deposits.Columns.Contains("TenantName"))
                deposits.Columns.Add("TenantName", typeof(string));
            if (!deposits.Columns.Contains("RoomNumber"))
                deposits.Columns.Add("RoomNumber", typeof(string));

            foreach (DataRow r in deposits.Rows)
            {
                if (deposits.Columns.Contains("TenantId") && tenantMap != null && int.TryParse(r["TenantId"]?.ToString(), out var tid) && tenantMap.TryGetValue(tid, out var tname))
                    r["TenantName"] = tname;
                if (deposits.Columns.Contains("RoomId") && roomMap != null && int.TryParse(r["RoomId"]?.ToString(), out var rid) && roomMap.TryGetValue(rid, out var rnum))
                    r["RoomNumber"] = rnum;
            }
        }

        private void ApplyGridPresentation()
        {
            SetHeader("DepositId", "ID");
            SetHeader("TenantName", "Khách thuê");
            SetHeader("RoomNumber", "Phòng");
            SetHeader("DepositAmount", "Tiền cọc");
            SetHeader("DepositDate", "Ngày cọc");
            SetHeader("DepositType", "Loại");
            SetHeader("Status", "Trạng thái");
            SetHeader("ReturnedAmount", "Tiền hoàn");
            SetHeader("ReturnedDate", "Ngày hoàn");
            SetHeader("Notes", "Ghi chú");
            SetHeader("CreatedDate", "Tạo lúc");

            HideIfExists("TenantId");
            HideIfExists("RoomId");
            HideIfExists("BranchId");

            FormatMoney("DepositAmount");
            FormatMoney("ReturnedAmount");
            FormatDate("DepositDate");
            FormatDate("ReturnedDate");
            FormatDate("CreatedDate");

            SetDisplayOrder(
                "DepositId",
                "TenantName",
                "RoomNumber",
                "DepositAmount",
                "DepositDate",
                "DepositType",
                "Status",
                "ReturnedAmount",
                "ReturnedDate",
                "Notes",
                "CreatedDate"
            );

            if (_grid.Columns.Contains("DepositAmount"))
                _grid.Columns["DepositAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            if (_grid.Columns.Contains("ReturnedAmount"))
                _grid.Columns["ReturnedAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            if (_grid.Columns.Contains("Status"))
                _grid.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            if (_grid.Columns.Contains("DepositId"))
                _grid.Columns["DepositId"].Width = 70;
            if (_grid.Columns.Contains("RoomNumber"))
                _grid.Columns["RoomNumber"].Width = 90;
            if (_grid.Columns.Contains("DepositDate"))
                _grid.Columns["DepositDate"].Width = 110;
            if (_grid.Columns.Contains("ReturnedDate"))
                _grid.Columns["ReturnedDate"].Width = 110;
        }

        private void ApplyFilter()
        {
            if (_table == null) return;

            string rawKeyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholder) rawKeyword = string.Empty;
            string keyword = rawKeyword.Replace("'", "''");
            string statusCombo = _cboStatus?.SelectedItem?.ToString();

            string statusFilter = null;
            if (!string.IsNullOrWhiteSpace(statusCombo) && statusCombo != "Tất cả")
            {
                // Convert Vietnamese status to English for database query
                string dbStatus = statusCombo;
                if (statusCombo == "Chờ xử lý") dbStatus = "Pending";
                else if (statusCombo == "Đã xác nhận") dbStatus = "Confirmed";
                else if (statusCombo == "Hoàn cọc") dbStatus = "Returned";
                else if (statusCombo == "Hủy") dbStatus = "Cancelled";

                statusFilter = $"(Status = '{dbStatus}' OR Status = '{statusCombo}')";
            }

            string keywordFilter = !string.IsNullOrWhiteSpace(keyword)
                ? $"(Convert(DepositId, 'System.String') LIKE '%{keyword}%' " +
                  $"OR Convert(TenantId, 'System.String') LIKE '%{keyword}%' " +
                  $"OR Convert(RoomId, 'System.String') LIKE '%{keyword}%' " +
                  $"OR TenantName LIKE '%{keyword}%' " +
                  $"OR RoomNumber LIKE '%{keyword}%' " +
                  $"OR DepositType LIKE '%{keyword}%' " +
                  $"OR Status LIKE '%{keyword}%' " +
                  $"OR Notes LIKE '%{keyword}%')"
                : null;

            if (statusFilter != null && keywordFilter != null)
                _table.DefaultView.RowFilter = statusFilter + " AND " + keywordFilter;
            else if (statusFilter != null)
                _table.DefaultView.RowFilter = statusFilter;
            else if (keywordFilter != null)
                _table.DefaultView.RowFilter = keywordFilter;
            else
                _table.DefaultView.RowFilter = string.Empty;

            _lblCount.Text = $"Tổng: {_table.DefaultView.Count}";

            decimal total = 0m;
            decimal returned = 0m;
            foreach (DataRowView r in _table.DefaultView)
            {
                if (_table.Columns.Contains("DepositAmount") && decimal.TryParse(r["DepositAmount"]?.ToString(), out var v))
                    total += v;
                if (_table.Columns.Contains("ReturnedAmount") && decimal.TryParse(r["ReturnedAmount"]?.ToString(), out var ret))
                    returned += ret;
            }
            var net = total - returned;
            _lblTotal.Text = $"Tổng cọc: {net:N0} (Hoàn: {returned:N0})";
        }

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            var drv = _grid.CurrentRow.DataBoundItem as DataRowView;
            return drv?.Row;
        }

        private void AddNew()
        {
            using (var frm = new FrmDepositEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadDataAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private void EditSelected()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmDepositEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadDataAsync();
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

            int id = Convert.ToInt32(row["DepositId"]);
            if (MessageBox.Show($"Xóa đặt phòng/cọc ID {id}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    await _bll.DeleteDepositAsync(id);
                    await LoadDataAsync();
                    AdminEvents.NotifyDataChanged();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async System.Threading.Tasks.Task MarkStatusAsync(string newStatus)
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để cập nhật trạng thái.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(row["DepositId"]);
            string tenantName = row.Table.Columns.Contains("TenantName") ? row["TenantName"]?.ToString() : null;
            string roomNumber = row.Table.Columns.Contains("RoomNumber") ? row["RoomNumber"]?.ToString() : null;

            try
            {
                int tenantId = Convert.ToInt32(row["TenantId"]);
                int roomId = Convert.ToInt32(row["RoomId"]);
                decimal amount = decimal.TryParse(row["DepositAmount"]?.ToString(), out var a) ? a : 0m;
                DateTime? depositDate = DateTime.TryParse(row["DepositDate"]?.ToString(), out var dd) ? (DateTime?)dd.Date : null;
                string type = row["DepositType"]?.ToString();
                decimal? returnedAmount = decimal.TryParse(row["ReturnedAmount"]?.ToString(), out var ra) ? (decimal?)ra : null;
                DateTime? returnedDate = DateTime.TryParse(row["ReturnedDate"]?.ToString(), out var rd) ? (DateTime?)rd.Date : null;
                string notes = row["Notes"]?.ToString();
                string paymentMethod = null;

                if (newStatus == "Confirmed")
                {
                    using (var dlg = new DepositActionDialog(
                        DepositActionKind.Confirm,
                        tenantName,
                        roomNumber,
                        amount,
                        depositDate,
                        notes))
                    {
                        if (dlg.ShowDialog(this) != DialogResult.OK) return;
                        depositDate = dlg.DepositDate ?? DateTime.Today;
                        notes = dlg.Notes;
                        paymentMethod = dlg.PaymentMethod;
                    }
                }

                if (newStatus == "Confirmed" && !HasLinkedPayment(notes))
                {
                    var actionDate = depositDate ?? DateTime.Today;
                    var result = await _bll.CreateDepositPaymentAsync(
                        tenantId,
                        roomId,
                        amount,
                        actionDate,
                        "Deposit",
                        paymentMethod,
                        $"Xác nhận cọc - DepositId: {id}");
                    notes = AppendPaymentNote(notes, result.Item1, result.Item2, "Deposit");
                }

                await _bll.UpdateDepositAsync(id, tenantId, roomId, amount, depositDate, type, newStatus, returnedAmount, returnedDate, notes);
                await AddDepositNotificationAsync("Xác nhận cọc", tenantName, roomNumber, amount, paymentMethod);
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
                DataSyncManager.NotifyInvoicesChanged();
                DataSyncManager.NotifyPaymentsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật trạng thái: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task MarkReturnedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để hoàn cọc.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(row["DepositId"]);
            string tenantName = row.Table.Columns.Contains("TenantName") ? row["TenantName"]?.ToString() : null;
            string roomNumber = row.Table.Columns.Contains("RoomNumber") ? row["RoomNumber"]?.ToString() : null;

            try
            {
                int tenantId = Convert.ToInt32(row["TenantId"]);
                int roomId = Convert.ToInt32(row["RoomId"]);
                decimal amount = decimal.TryParse(row["DepositAmount"]?.ToString(), out var a) ? a : 0m;
                DateTime? depositDate = DateTime.TryParse(row["DepositDate"]?.ToString(), out var dd) ? (DateTime?)dd.Date : null;
                string type = row["DepositType"]?.ToString();
                decimal? returnedAmount = decimal.TryParse(row["ReturnedAmount"]?.ToString(), out var ra) ? (decimal?)ra : null;
                DateTime? returnedDate = DateTime.TryParse(row["ReturnedDate"]?.ToString(), out var rd) ? (DateTime?)rd.Date : null;
                string notes = row["Notes"]?.ToString();
                string paymentMethod = null;

                using (var dlg = new DepositActionDialog(
                    DepositActionKind.Return,
                    tenantName,
                    roomNumber,
                    amount,
                    returnedAmount,
                    returnedDate,
                    notes))
                {
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;
                    returnedAmount = dlg.ReturnedAmount ?? amount;
                    returnedDate = dlg.ReturnedDate ?? DateTime.Today;
                    notes = dlg.Notes;
                    paymentMethod = dlg.PaymentMethod;
                }

                if (!HasLinkedPayment(notes))
                {
                    var actionDate = returnedDate ?? DateTime.Today;
                    var refundAmount = returnedAmount ?? amount;
                    var result = await _bll.CreateDepositPaymentAsync(
                        tenantId,
                        roomId,
                        refundAmount,
                        actionDate,
                        "Refund",
                        paymentMethod,
                        $"Hoàn cọc - DepositId: {id}");
                    notes = AppendPaymentNote(notes, result.Item1, result.Item2, "Refund");
                }

                await _bll.UpdateDepositAsync(id, tenantId, roomId, amount, depositDate, type, "Returned", returnedAmount, returnedDate, notes);
                await AddDepositNotificationAsync("Hoàn cọc", tenantName, roomNumber, returnedAmount ?? amount, paymentMethod);
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
                DataSyncManager.NotifyInvoicesChanged();
                DataSyncManager.NotifyPaymentsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hoàn cọc: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool HasLinkedPayment(string notes)
        {
            return !string.IsNullOrWhiteSpace(notes) && notes.Contains("[INV:");
        }

        private static string AppendPaymentNote(string notes, int invoiceId, int paymentId, string actionType)
        {
            if (invoiceId <= 0 || paymentId <= 0)
                return notes;

            string tag = $"[INV:{invoiceId}|PAY:{paymentId}|{actionType}]";
            if (string.IsNullOrWhiteSpace(notes)) return tag;
            if (notes.Contains(tag)) return notes;
            return notes.TrimEnd() + " " + tag;
        }

        private async System.Threading.Tasks.Task AddDepositNotificationAsync(string actionTitle, string tenantName, string roomNumber, decimal amount, string paymentMethod)
        {
            try
            {
                string methodText = paymentMethod == "Card" ? "Thẻ" : "Tiền mặt";
                string message = $"{actionTitle}: {tenantName ?? "—"} | Phòng: {roomNumber ?? "—"} | Số tiền: {amount:N0} | Hình thức: {methodText}";
                await _bll.AddNotificationAsync(null, actionTitle, message, "Unread");
            }
            catch
            {
                // ignore notification errors
            }
        }

        private async System.Threading.Tasks.Task EnsureAllowedBranchScopeAsync()
        {
            if (_branchId.HasValue) return;
            if (_allowedBranchIds != null && _allowedBranchIds.Count > 0) return;

            try
            {
                var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                _allowedBranchIds = AdminBranchScope.GetAllowedBranchIds(branches);
            }
            catch
            {
                _allowedBranchIds = new System.Collections.Generic.HashSet<int>();
            }
        }

        private static DataTable FilterByBranch(DataTable dt, int? branchId)
        {
            if (dt == null) return dt;
            if (!branchId.HasValue) return dt;
            if (!dt.Columns.Contains("BranchId")) return dt;

            var filtered = dt.Clone();
            foreach (DataRow r in dt.Rows)
            {
                if (int.TryParse(r["BranchId"]?.ToString(), out var b) && b == branchId.Value)
                    filtered.ImportRow(r);
            }
            return filtered;
        }

        private static Button MakeButton(string text, Color backColor, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(10, 0, 10, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        private void SetHeader(string columnName, string headerText)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].HeaderText = headerText;
        }

        private void HideIfExists(string columnName)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].Visible = false;
        }

        private void FormatMoney(string columnName)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].DefaultCellStyle.Format = "N0";
        }

        private void FormatDate(string columnName)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private void SetDisplayOrder(params string[] order)
        {
            int index = 0;
            foreach (var name in order)
            {
                if (_grid.Columns.Contains(name))
                {
                    _grid.Columns[name].DisplayIndex = index;
                    index++;
                }
            }
        }

        private enum DepositActionKind
        {
            Confirm,
            Return
        }

        private sealed class DepositActionDialog : Form
        {
            private readonly DepositActionKind _kind;
            private readonly decimal _depositAmount;
            private readonly DateTimePicker _dtDeposit;
            private readonly NumericUpDown _numReturn;
            private readonly DateTimePicker _dtReturn;
            private readonly ComboBox _cboMethod;
            private readonly TextBox _txtNotes;

            public DateTime? DepositDate { get; private set; }
            public decimal? ReturnedAmount { get; private set; }
            public DateTime? ReturnedDate { get; private set; }
            public string Notes { get; private set; }
            public string PaymentMethod { get; private set; }

            public DepositActionDialog(
                DepositActionKind kind,
                string tenantName,
                string roomNumber,
                decimal depositAmount,
                DateTime? depositDate,
                string notes)
                : this(kind, tenantName, roomNumber, depositAmount, notes)
            {
                _dtDeposit.Value = depositDate ?? DateTime.Today;
            }

            public DepositActionDialog(
                DepositActionKind kind,
                string tenantName,
                string roomNumber,
                decimal depositAmount,
                decimal? returnedAmount,
                DateTime? returnedDate,
                string notes)
                : this(kind, tenantName, roomNumber, depositAmount, notes)
            {
                _numReturn.Value = returnedAmount.HasValue && returnedAmount.Value > 0 ? returnedAmount.Value : depositAmount;
                _dtReturn.Value = returnedDate ?? DateTime.Today;
            }

            private DepositActionDialog(
                DepositActionKind kind,
                string tenantName,
                string roomNumber,
                decimal depositAmount,
                string notes)
            {
                _kind = kind;
                _depositAmount = depositAmount;

                Text = kind == DepositActionKind.Confirm ? "Xác nhận cọc" : "Hoàn cọc";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(520, 350);
                BackColor = Color.White;
                Font = new Font("Segoe UI", 10F);

                var lblTitle = new Label
                {
                    Text = kind == DepositActionKind.Confirm ? "Xác nhận đặt cọc" : "Hoàn cọc cho khách thuê",
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(16, 14)
                };

                var infoText = $"Khách thuê: {(string.IsNullOrWhiteSpace(tenantName) ? "—" : tenantName)}\n" +
                               $"Phòng: {(string.IsNullOrWhiteSpace(roomNumber) ? "—" : roomNumber)}\n" +
                               $"Tiền cọc: {depositAmount:N0}";
                var lblInfo = new Label
                {
                    Text = infoText,
                    AutoSize = true,
                    Location = new Point(16, 44),
                    ForeColor = Color.FromArgb(70, 70, 70)
                };

                var panel = new Panel
                {
                    Location = new Point(16, 108),
                    Size = new Size(488, 140)
                };

                int leftLabel = 0;
                int leftInput = 140;
                int top = 4;
                int line = 32;

                _dtDeposit = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 140 };
                _numReturn = new NumericUpDown
                {
                    DecimalPlaces = 0,
                    Maximum = Math.Max(1000000000m, depositAmount),
                    Minimum = 0,
                    Increment = 100000,
                    ThousandsSeparator = true,
                    Width = 160
                };
                _dtReturn = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 140 };
                _cboMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
                _cboMethod.Items.AddRange(new object[] { "Tiền mặt", "Thẻ" });
                _cboMethod.SelectedIndex = 0;
                _txtNotes = new TextBox { Width = 320, Height = 52, Multiline = true, ScrollBars = ScrollBars.Vertical };

                if (kind == DepositActionKind.Confirm)
                {
                    var lblDate = new Label { Text = "Ngày cọc:", AutoSize = true, Location = new Point(leftLabel, top + 4) };
                    _dtDeposit.Location = new Point(leftInput, top);
                    panel.Controls.Add(lblDate);
                    panel.Controls.Add(_dtDeposit);
                    top += line;
                }
                else
                {
                    var lblAmount = new Label { Text = "Tiền hoàn:", AutoSize = true, Location = new Point(leftLabel, top + 4) };
                    _numReturn.Location = new Point(leftInput, top);
                    panel.Controls.Add(lblAmount);
                    panel.Controls.Add(_numReturn);
                    top += line;

                    var lblReturnDate = new Label { Text = "Ngày hoàn:", AutoSize = true, Location = new Point(leftLabel, top + 4) };
                    _dtReturn.Location = new Point(leftInput, top);
                    panel.Controls.Add(lblReturnDate);
                    panel.Controls.Add(_dtReturn);
                    top += line;
                }

                var lblMethod = new Label { Text = _kind == DepositActionKind.Confirm ? "Hình thức thu:" : "Hình thức hoàn:", AutoSize = true, Location = new Point(leftLabel, top + 4) };
                _cboMethod.Location = new Point(leftInput, top);
                panel.Controls.Add(lblMethod);
                panel.Controls.Add(_cboMethod);
                top += line;

                var lblNotes = new Label { Text = "Ghi chú:", AutoSize = true, Location = new Point(leftLabel, top + 4) };
                _txtNotes.Location = new Point(leftInput, top);
                _txtNotes.Text = notes ?? string.Empty;
                panel.Controls.Add(lblNotes);
                panel.Controls.Add(_txtNotes);

                var btnOk = new Button
                {
                    Text = "Xác nhận",
                    Width = 110,
                    Height = 32,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(274, 292)
                };
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Click += (s, e) => HandleSave();

                var btnCancel = new Button
                {
                    Text = "Hủy",
                    Width = 90,
                    Height = 32,
                    BackColor = Color.FromArgb(200, 200, 200),
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(394, 292),
                    DialogResult = DialogResult.Cancel
                };
                btnCancel.FlatAppearance.BorderSize = 0;

                Controls.Add(lblTitle);
                Controls.Add(lblInfo);
                Controls.Add(panel);
                Controls.Add(btnOk);
                Controls.Add(btnCancel);

                AcceptButton = btnOk;
                CancelButton = btnCancel;
            }

            private void HandleSave()
            {
                if (_cboMethod.SelectedIndex < 0)
                {
                    MessageBox.Show("Vui lòng chọn hình thức thanh toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_kind == DepositActionKind.Return)
                {
                    if (_numReturn.Value <= 0)
                    {
                        MessageBox.Show("Tiền hoàn phải lớn hơn 0.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (_numReturn.Value > _depositAmount)
                    {
                        MessageBox.Show("Tiền hoàn không được vượt quá tiền cọc.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    ReturnedAmount = _numReturn.Value;
                    ReturnedDate = _dtReturn.Value.Date;
                }
                else
                {
                    DepositDate = _dtDeposit.Value.Date;
                }

                PaymentMethod = _cboMethod.SelectedItem?.ToString() == "Thẻ" ? "Card" : "Cash";
                Notes = _txtNotes.Text?.Trim();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name == "DepositType" && e.Value != null)
            {
                string type = e.Value.ToString();
                if (type == "Booking") e.Value = "Đặt chỗ";
                else if (type == "Official") e.Value = "Chính thức";
            }

            if (_grid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Pending") e.Value = "Chờ xử lý";
                else if (status == "Confirmed") 
                {
                    e.Value = "Đã xác nhận";
                    e.CellStyle.ForeColor = Color.FromArgb(46, 125, 50); // Green
                }
                else if (status == "Returned") 
                {
                    e.Value = "Hoàn cọc";
                    e.CellStyle.ForeColor = Color.FromArgb(33, 150, 243); // Blue
                }
                else if (status == "Cancelled") 
                {
                    e.Value = "Hủy";
                    e.CellStyle.ForeColor = Color.FromArgb(211, 47, 47); // Red
                }
            }
        }

        private void HandleGridMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = _grid.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0)
                {
                    _grid.ClearSelection();
                    _grid.Rows[hitTest.RowIndex].Selected = true;
                    ShowContextMenu(e.X, e.Y);
                }
            }
        }

        private void ShowContextMenu(int x, int y)
        {
            var row = GetCurrentRow();
            if (row == null) return;

            ContextMenuStrip menu = new ContextMenuStrip();
            
            menu.Items.Add("Xem chi tiết khách", null, (s, e) =>
            {
                try
                {
                    if (!int.TryParse(row["TenantId"]?.ToString(), out var tenantId)) return;
                    using (var frm = new FrmTenantQuickInfo(_bll, tenantId))
                    {
                        var result = frm.ShowDialog(this);
                        if (result == DialogResult.OK)
                        {
                            _ = LoadDataAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            menu.Items.Add("-"); // Separator
            menu.Items.Add("Sửa", null, (s, e) => EditSelected());
            menu.Items.Add("Xóa", null, async (s, e) => await DeleteSelectedAsync());
            
            _grid.ContextMenuStrip = menu;
            menu.Show(_grid, x, y);
        }

    }
}

