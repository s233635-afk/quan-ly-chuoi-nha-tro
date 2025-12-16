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
        }

        private void InitializeComponent()
        {
            this.Text = "Đặt Phòng & Cọc";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 1200;
            this.Height = 650;
            this.BackColor = Color.FromArgb(245, 247, 250);

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
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 252);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
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
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Pending", "Confirmed", "Returned", "Cancelled" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0" };
            _lblTotal = new Label { AutoSize = true, Text = "Tổng cọc: 0", ForeColor = Color.FromArgb(70, 70, 70) };

            _btnAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), (s, e) => AddNew());
            _btnEdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), (s, e) => EditSelected());
            _btnDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteSelectedAsync());
            _btnConfirm = MakeButton("Xác nhận", Color.FromArgb(46, 125, 50), async (s, e) => await MarkStatusAsync("Confirmed"));
            _btnReturn = MakeButton("Hoàn cọc", Color.FromArgb(121, 85, 72), async (s, e) => await MarkReturnedAsync());
            _btnRefresh = MakeButton("Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadDataAsync());

            var top = new Panel { Dock = DockStyle.Top, Height = 64, Padding = new Padding(12, 10, 12, 10), BackColor = Color.White };

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

            var summary = new Panel { Dock = DockStyle.Right, Width = 260, BackColor = Color.Transparent };
            _lblCount.Location = new Point(0, 6);
            _lblCount.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _lblTotal.Location = new Point(0, 28);
            summary.Controls.Add(_lblCount);
            summary.Controls.Add(_lblTotal);

            top.Controls.Add(searchHost);
            top.Controls.Add(summary);
            top.Controls.Add(actions);

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = this.BackColor };
            gridHost.Controls.Add(_grid);

            this.Controls.Add(gridHost);
            this.Controls.Add(top);
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
        }

        private void ApplyFilter()
        {
            if (_table == null) return;

            string rawKeyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholder) rawKeyword = string.Empty;
            string keyword = rawKeyword.Replace("'", "''");
            string status = _cboStatus?.SelectedItem?.ToString();

            string statusFilter = (!string.IsNullOrWhiteSpace(status) && status != "Tất cả")
                ? $"Status = '{status}'"
                : null;

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
            foreach (DataRowView r in _table.DefaultView)
            {
                if (_table.Columns.Contains("DepositAmount") && decimal.TryParse(r["DepositAmount"]?.ToString(), out var v))
                    total += v;
            }
            _lblTotal.Text = $"Tổng cọc: {total:N0}";
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
            if (MessageBox.Show($"Cập nhật trạng thái cọc ID {id} -> {newStatus}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

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

                if (newStatus == "Confirmed" && !depositDate.HasValue)
                    depositDate = DateTime.Today;

                await _bll.UpdateDepositAsync(id, tenantId, roomId, amount, depositDate, type, newStatus, returnedAmount, returnedDate, notes);
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
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
            if (MessageBox.Show($"Hoàn cọc cho ID {id} (Status = Returned)?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

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

                if (!returnedAmount.HasValue || returnedAmount.Value <= 0)
                    returnedAmount = amount;
                returnedDate = DateTime.Today;

                await _bll.UpdateDepositAsync(id, tenantId, roomId, amount, depositDate, type, "Returned", returnedAmount, returnedDate, notes);
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hoàn cọc: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Width = 96,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 8, 0)
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

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name != "Status" || e.Value == null) return;

            string status = e.Value.ToString();
            if (string.Equals(status, "Confirmed", StringComparison.OrdinalIgnoreCase))
                e.CellStyle.ForeColor = Color.FromArgb(46, 125, 50);
            else if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
                e.CellStyle.ForeColor = Color.FromArgb(245, 124, 0);
            else if (string.Equals(status, "Returned", StringComparison.OrdinalIgnoreCase))
                e.CellStyle.ForeColor = Color.FromArgb(33, 150, 243);
            else if (string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                e.CellStyle.ForeColor = Color.FromArgb(211, 47, 47);
        }
    }
}
