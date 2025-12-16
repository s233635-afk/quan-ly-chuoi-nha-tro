using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Màn hình quản lý khách thuê cho nhân viên.
    /// </summary>
    public class FrmTenantManager : Form
    {
        private const string SearchPlaceholder = "Tìm tên/CCCD/SĐT...";

        private readonly AdminDataBLL _bll;
        private readonly int? _branchId;

        private DataTable _tenants;
        private DataTable _dependents;
        private DataTable _history;
        private DataTable _rooms;

        private DataGridView _dgvTenants;
        private DataGridView _dgvDependents;
        private DataGridView _dgvHistory;

        private TextBox _txtSearch;
        private Label _lblTotal;

        public FrmTenantManager(AdminDataBLL bll, int? branchId = null)
        {
            _bll = bll ?? new AdminDataBLL();
            _branchId = branchId;
            InitializeComponent();
        }

        public FrmTenantManager() : this(new AdminDataBLL(), null)
        {
        }

        private void InitializeComponent()
        {
            Text = "Quản lý khách thuê";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Width = 1100;
            Height = 650;

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White
            };

            var searchLabel = new Label { Text = "Tìm:", AutoSize = true, Margin = new Padding(0, 8, 6, 0) };
            _txtSearch = new TextBox { Width = 260, Text = SearchPlaceholder, ForeColor = Color.Gray, Margin = new Padding(0, 4, 12, 0) };
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
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) ApplySearch(); };

            var btnSearch = MakeButton("Tìm", Color.FromArgb(0, 122, 204), (s, e) => ApplySearch());
            var btnAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), (s, e) => AddTenant());
            var btnEdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), (s, e) => EditTenant());
            var btnDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), (s, e) => DeleteTenant());
            var btnRefresh = MakeButton("Làm mới", Color.FromArgb(40, 167, 69), async (s, e) => await LoadDataAsync());

            _lblTotal = new Label { Text = "Tổng: 0", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(0, 122, 204), Margin = new Padding(12, 8, 0, 0) };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            actions.Controls.Add(searchLabel);
            actions.Controls.Add(_txtSearch);
            actions.Controls.Add(btnSearch);
            actions.Controls.Add(btnAdd);
            actions.Controls.Add(btnEdit);
            actions.Controls.Add(btnDelete);
            actions.Controls.Add(btnRefresh);
            actions.Controls.Add(_lblTotal);

            toolbar.Controls.Add(actions);

            var tab = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Tenants tab
            _dgvTenants = CreateGrid();
            var tabTenants = new TabPage("Khách thuê") { BackColor = Color.FromArgb(240, 242, 245), Padding = new Padding(10) };
            tabTenants.Controls.Add(_dgvTenants);
            tab.TabPages.Add(tabTenants);

            // Dependents tab
            _dgvDependents = CreateGrid();
            var tabDependents = new TabPage("Người ở cùng") { BackColor = Color.FromArgb(240, 242, 245), Padding = new Padding(10) };
            tabDependents.Controls.Add(_dgvDependents);
            tab.TabPages.Add(tabDependents);

            // History tab
            _dgvHistory = CreateGrid();
            var tabHistory = new TabPage("Lịch sử phòng") { BackColor = Color.FromArgb(240, 242, 245), Padding = new Padding(10) };
            tabHistory.Controls.Add(_dgvHistory);
            tab.TabPages.Add(tabHistory);

            Controls.Add(tab);
            Controls.Add(toolbar);

            Load += async (s, e) => await LoadDataAsync();
        }

        private DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            return grid;
        }

        private Button MakeButton(string text, Color backColor, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Width = 90,
                Height = 32,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 8, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                _tenants = await _bll.GetTenantsAsync() ?? new DataTable();
                _dependents = await _bll.GetDependentsAsync() ?? new DataTable();
                _history = await _bll.GetTenantHistoryAsync() ?? new DataTable();
                _rooms = await _bll.GetRoomsAsync() ?? new DataTable();

                if (_branchId.HasValue && _tenants.Columns.Contains("BranchId"))
                {
                    var filtered = _tenants.AsEnumerable()
                        .Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == _branchId.Value);
                    _tenants = filtered.Any() ? filtered.CopyToDataTable() : _tenants.Clone();
                }

                PopulateRoomNumbers();

                _dgvTenants.DataSource = _tenants;
                _dgvDependents.DataSource = _dependents;
                _dgvHistory.DataSource = _history;

                if (_dgvTenants.Columns.Contains("TenantId"))
                    _dgvTenants.Columns["TenantId"].Visible = false;
                if (_dgvDependents.Columns.Contains("TenantId"))
                    _dgvDependents.Columns["TenantId"].Visible = false;
                if (_dgvHistory.Columns.Contains("TenantId"))
                    _dgvHistory.Columns["TenantId"].Visible = false;
                if (_dgvHistory.Columns.Contains("RoomId"))
                    _dgvHistory.Columns["RoomId"].Visible = false;

                _lblTotal.Text = $"Tổng: {_tenants.Rows.Count}";
                ApplySearch();
                ConfigureDependentsGrid();
                ConfigureHistoryGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu khách thuê: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ApplySearch()
        {
            if (_tenants == null) return;

            string keyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (keyword == SearchPlaceholder) keyword = string.Empty;

            var view = new DataView(_tenants);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var escaped = keyword.Replace("'", "''");
                view.RowFilter = $"Convert(FullName, 'System.String') LIKE '%{escaped}%' OR Convert(IdentityCard, 'System.String') LIKE '%{escaped}%' OR Convert(PhoneNumber, 'System.String') LIKE '%{escaped}%'";
            }
            else
            {
                view.RowFilter = string.Empty;
            }

            _dgvTenants.DataSource = view;
            _lblTotal.Text = $"Tổng: {view.Count}";
            ConfigureTenantGrid();
        }

        private DataRowView GetSelectedTenant()
        {
            return _dgvTenants.CurrentRow?.DataBoundItem as DataRowView;
        }

        private async void AddTenant()
        {
            using (var frm = new FrmTenantEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadDataAsync();
            }
        }

        private async void EditTenant()
        {
            var row = GetSelectedTenant();
            if (row == null)
            {
                MessageBox.Show("Chọn khách thuê trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmTenantEditor(_bll, row.Row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadDataAsync();
            }
        }

        private async void DeleteTenant()
        {
            var row = GetSelectedTenant();
            if (row == null)
            {
                MessageBox.Show("Chọn khách thuê trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int tenantId = int.TryParse(row["TenantId"]?.ToString(), out var id) ? id : 0;
            string name = row["FullName"]?.ToString() ?? tenantId.ToString();
            if (tenantId <= 0)
            {
                MessageBox.Show("Không xác định được TenantId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Xóa khách thuê \"{name}\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteTenantAsync(tenantId);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa khách thuê: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateRoomNumbers()
        {
            if (_tenants == null) return;

            if (!_tenants.Columns.Contains("RoomNumber"))
                _tenants.Columns.Add("RoomNumber", typeof(string));

            var roomLookup = _rooms?.AsEnumerable()
                .Where(r => _rooms.Columns.Contains("RoomId") && r["RoomId"] != DBNull.Value)
                .ToDictionary(r => Convert.ToInt32(r["RoomId"]), r => r["RoomNumber"]?.ToString() ?? string.Empty)
                ?? new System.Collections.Generic.Dictionary<int, string>();

            foreach (DataRow tenant in _tenants.Rows)
            {
                int tenantId = int.TryParse(tenant["TenantId"]?.ToString(), out var id) ? id : 0;
                if (tenantId <= 0) continue;

                var historyRows = _history?.AsEnumerable()
                    .Where(r => int.TryParse(r["TenantId"]?.ToString(), out var tid) && tid == tenantId)
                    .ToList();
                if (historyRows == null || historyRows.Count == 0) continue;

                DataRow latest = historyRows
                    .Where(r => string.IsNullOrWhiteSpace(r["CheckOutDate"]?.ToString()))
                    .OrderByDescending(r => ParseDate(r["CheckInDate"]))
                    .FirstOrDefault()
                    ?? historyRows.OrderByDescending(r => ParseDate(r["CheckInDate"])).FirstOrDefault();

                if (latest != null && int.TryParse(latest["RoomId"]?.ToString(), out var rid) && roomLookup.TryGetValue(rid, out var roomNo))
                {
                    tenant["RoomNumber"] = roomNo;
                }
            }
        }

        private DateTime ParseDate(object value)
        {
            if (value == null) return DateTime.MinValue;
            return DateTime.TryParse(value.ToString(), out var dt) ? dt : DateTime.MinValue;
        }

        private void ConfigureTenantGrid()
        {
            if (_dgvTenants?.Columns == null) return;

            SetHeader(_dgvTenants, "FullName", "Họ tên", 0);
            SetHeader(_dgvTenants, "IdentityCard", "CCCD/CMND", 1);
            SetHeader(_dgvTenants, "PhoneNumber", "SĐT", 2);
            SetHeader(_dgvTenants, "Email", "Email", 3);
            SetHeader(_dgvTenants, "BirthDate", "Ngày sinh", 4);
            SetHeader(_dgvTenants, "Address", "Địa chỉ", 5);
            SetHeader(_dgvTenants, "RoomNumber", "Số phòng", 6);
            SetHeader(_dgvTenants, "TemporaryRegistration", "Tạm trú", 7);
            SetHeader(_dgvTenants, "TemporaryRegistrationDate", "Tạm trú từ", 8);
            SetHeader(_dgvTenants, "TemporaryRegistrationExpiry", "Tạm trú đến", 9);
            SetHeader(_dgvTenants, "IsActive", "Kích hoạt", 10);
            SetHeader(_dgvTenants, "CreatedDate", "Ngày tạo", 11);
            SetHeader(_dgvTenants, "UpdatedDate", "Cập nhật", 12);
            if (_dgvTenants.Columns.Contains("BranchId")) _dgvTenants.Columns["BranchId"].Visible = false;
        }

        private void ConfigureDependentsGrid()
        {
            if (_dgvDependents?.Columns == null) return;
            if (_dgvDependents.Columns.Contains("DependentId")) _dgvDependents.Columns["DependentId"].Visible = false;
            SetHeader(_dgvDependents, "FullName", "Họ tên");
            SetHeader(_dgvDependents, "Relationship", "Quan hệ");
            SetHeader(_dgvDependents, "PhoneNumber", "SĐT");
            SetHeader(_dgvDependents, "CreatedDate", "Ngày tạo");
        }

        private void ConfigureHistoryGrid()
        {
            if (_dgvHistory?.Columns == null) return;
            if (_dgvHistory.Columns.Contains("HistoryId")) _dgvHistory.Columns["HistoryId"].Visible = false;
            SetHeader(_dgvHistory, "RoomId", "Mã phòng", visible: false);
            SetHeader(_dgvHistory, "CheckInDate", "Ngày vào");
            SetHeader(_dgvHistory, "CheckOutDate", "Ngày ra");
            SetHeader(_dgvHistory, "Status", "Trạng thái");
            SetHeader(_dgvHistory, "Notes", "Ghi chú");
            SetHeader(_dgvHistory, "CreatedDate", "Ngày tạo");
        }

        private void SetHeader(DataGridView grid, string columnName, string header, int? displayIndex = null, bool visible = true)
        {
            if (!grid.Columns.Contains(columnName)) return;
            var col = grid.Columns[columnName];
            col.HeaderText = header;
            col.Visible = visible;
            if (displayIndex.HasValue) col.DisplayIndex = displayIndex.Value;
        }
    }
}
