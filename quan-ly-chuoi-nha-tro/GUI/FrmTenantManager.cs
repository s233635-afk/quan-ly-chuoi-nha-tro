using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmTenantManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo họ tên/cccd/sđt/email...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();

        private DataTable _tenantTable;
        private DataTable _dependentTable;
        private DataTable _historyTable;

        private SplitContainer _split;
        private bool _splitInitialized;

        private DataGridView _gridTenants;
        private DataGridView _gridDependents;
        private DataGridView _gridHistory;

        private TextBox _txtSearch;
        private ComboBox _cboActive;
        private Label _lblCount;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnToggleActive;
        private Button _btnRefresh;

        private Button _btnDepAdd;
        private Button _btnDepEdit;
        private Button _btnDepDelete;

        private Button _btnHisAdd;
        private Button _btnHisEdit;
        private Button _btnHisDelete;
        private Button _btnHisCheckout;

        public FrmTenantManager()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý Khách thuê";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1280;
            Height = 720;
            BackColor = Color.FromArgb(245, 247, 250);

            _gridTenants = MakeGrid();
            _gridTenants.Dock = DockStyle.Fill;
            _gridTenants.DoubleClick += async (s, e) => await EditSelectedTenantAsync();
            _gridTenants.SelectionChanged += (s, e) => ApplyTenantDetailsFilter();

            _gridDependents = MakeGrid();
            _gridDependents.Dock = DockStyle.Fill;
            _gridDependents.DoubleClick += async (s, e) => await EditSelectedDependentAsync();

            _gridHistory = MakeGrid();
            _gridHistory.Dock = DockStyle.Fill;
            _gridHistory.DoubleClick += async (s, e) => await EditSelectedHistoryAsync();

            _txtSearch = new TextBox { Width = 320, ForeColor = Color.Gray, Text = SearchPlaceholder };
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
            _txtSearch.TextChanged += (s, e) => ApplyTenantFilter();

            _cboActive = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboActive.Items.AddRange(new object[] { "Tất cả", "Đang hoạt động", "Đã tắt" });
            _cboActive.SelectedIndex = 0;
            _cboActive.SelectedIndexChanged += (s, e) => ApplyTenantFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            _btnAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), async (s, e) => await AddTenantAsync());
            _btnEdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), async (s, e) => await EditSelectedTenantAsync());
            _btnDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteSelectedTenantAsync());
            _btnToggleActive = MakeButton("Bật/Tắt", Color.FromArgb(103, 58, 183), async (s, e) => await ToggleTenantActiveAsync());
            _btnRefresh = MakeButton("Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadAllAsync());

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
            actions.Controls.Add(_btnToggleActive);
            actions.Controls.Add(_btnRefresh);

            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 0)
            };
            filters.Controls.Add(new Label { Text = "Tìm:", AutoSize = true, Margin = new Padding(0, 6, 6, 0) });
            filters.Controls.Add(_txtSearch);
            filters.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Margin = new Padding(12, 6, 6, 0) });
            filters.Controls.Add(_cboActive);
            filters.Controls.Add(new Label { Text = "  ", AutoSize = true });
            filters.Controls.Add(_lblCount);

            top.Controls.Add(actions);
            top.Controls.Add(filters);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(new TabPage("Người ở chung") { Padding = new Padding(0) });
            tabs.TabPages.Add(new TabPage("Lịch sử phòng") { Padding = new Padding(0) });

            var depTop = MakeSubTop(out _btnDepAdd, out _btnDepEdit, out _btnDepDelete,
                async (s, e) => await AddDependentAsync(),
                async (s, e) => await EditSelectedDependentAsync(),
                async (s, e) => await DeleteSelectedDependentAsync());
            tabs.TabPages[0].Controls.Add(_gridDependents);
            tabs.TabPages[0].Controls.Add(depTop);

            var hisTop = MakeSubTop(out _btnHisAdd, out _btnHisEdit, out _btnHisDelete,
                async (s, e) => await AddHistoryAsync(),
                async (s, e) => await EditSelectedHistoryAsync(),
                async (s, e) => await DeleteSelectedHistoryAsync());
            _btnHisCheckout = MakeSmallButton("Check-out", Color.FromArgb(255, 152, 0), async (s, e) => await CheckoutSelectedHistoryAsync());
            hisTop.Controls.Add(_btnHisCheckout);
            _btnHisCheckout.Location = new Point(hisTop.Width - _btnHisCheckout.Width - 12, 10);
            hisTop.Resize += (s, e) => _btnHisCheckout.Location = new Point(hisTop.Width - _btnHisCheckout.Width - 12, 10);
            tabs.TabPages[1].Controls.Add(_gridHistory);
            tabs.TabPages[1].Controls.Add(hisTop);

            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = Color.FromArgb(245, 247, 250),
                // Không set MinSize ở đây vì lúc khởi tạo control có thể chưa có Width -> dễ throw SplitterDistance invalid.
                Panel1MinSize = 0,
                Panel2MinSize = 0
            };
            _split.Panel1.Padding = new Padding(12, 0, 6, 12);
            _split.Panel2.Padding = new Padding(6, 0, 12, 12);

            var pnlTenants = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlTenants.Controls.Add(_gridTenants);
            _split.Panel1.Controls.Add(pnlTenants);

            var pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlRight.Controls.Add(tabs);
            _split.Panel2.Controls.Add(pnlRight);

            Controls.Add(_split);
            Controls.Add(top);

            Load += async (s, e) => await LoadAllAsync();
            Layout += (s, e) => EnsureSplitterInitialized();
            Resize += (s, e) => ClampSplitterDistance();
        }

        private void EnsureSplitterInitialized()
        {
            if (_splitInitialized) return;
            if (_split == null || _split.IsDisposed) return;
            if (_split.Width <= 0) return;

            // Set min sizes sau khi control đã được layout để tránh lỗi SplitterDistance invalid.
            SafeSetMinSizes(420, 320);

            // ưu tiên panel trái ~60%, nhưng luôn clamp để không lỗi.
            int desired = (int)Math.Round(_split.Width * 0.60);
            SafeSetSplitterDistance(desired);
            _splitInitialized = true;
        }

        private void ClampSplitterDistance()
        {
            if (_split == null || _split.IsDisposed) return;
            if (_split.Width <= 0) return;
            SafeSetSplitterDistance(_split.SplitterDistance);
        }

        private void SafeSetMinSizes(int panel1Min, int panel2Min)
        {
            if (_split == null || _split.IsDisposed) return;
            try
            {
                _split.Panel1MinSize = Math.Max(0, panel1Min);
                _split.Panel2MinSize = Math.Max(0, panel2Min);
            }
            catch
            {
                try
                {
                    _split.Panel1MinSize = 0;
                    _split.Panel2MinSize = 0;
                }
                catch
                {
                    // ignore
                }
            }
        }

        private void SafeSetSplitterDistance(int desired)
        {
            if (_split == null || _split.IsDisposed) return;

            // Nếu width quá nhỏ để thỏa min size, nới min size để UI vẫn hiển thị thay vì crash.
            int min = _split.Panel1MinSize;
            int max = _split.Width - _split.Panel2MinSize;
            if (max < min)
            {
                _split.Panel1MinSize = 0;
                _split.Panel2MinSize = 0;
                min = 0;
                max = Math.Max(0, _split.Width - 1);
            }

            int clamped = Math.Max(min, Math.Min(max, desired));
            try
            {
                if (_split.SplitterDistance != clamped)
                    _split.SplitterDistance = clamped;
            }
            catch
            {
                // ignore: chỉ để tránh crash khi WinForms đang layout
            }
        }

        private static DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
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

            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            g.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 252);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;
            return g;
        }

        private static Panel MakeSubTop(out Button add, out Button edit, out Button del,
            EventHandler onAdd, EventHandler onEdit, EventHandler onDelete)
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(12, 9, 12, 9), BackColor = Color.White };
            add = MakeSmallButton("Thêm", Color.FromArgb(0, 122, 204), onAdd);
            edit = MakeSmallButton("Sửa", Color.FromArgb(0, 122, 204), onEdit);
            del = MakeSmallButton("Xóa", Color.FromArgb(211, 47, 47), onDelete);

            top.Controls.Add(add);
            top.Controls.Add(edit);
            top.Controls.Add(del);
            add.Location = new Point(12, 10);
            edit.Location = new Point(add.Right + 8, 10);
            del.Location = new Point(edit.Right + 8, 10);
            return top;
        }

        private async System.Threading.Tasks.Task LoadAllAsync()
        {
            try
            {
                _tenantTable = await _bll.GetTenantsAsync();
                _dependentTable = await _bll.GetDependentsAsync();
                _historyTable = await _bll.GetTenantHistoryAsync();

                _gridTenants.DataSource = _tenantTable;
                _gridDependents.DataSource = _dependentTable;
                _gridHistory.DataSource = _historyTable;

                ApplyTenantGridPresentation();
                ApplyDependentGridPresentation();
                ApplyHistoryGridPresentation();

                ApplyTenantFilter();
                ApplyTenantDetailsFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu khách thuê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTenantGridPresentation()
        {
            SetHeader(_gridTenants, "TenantId", "ID");
            SetHeader(_gridTenants, "FullName", "Họ tên");
            SetHeader(_gridTenants, "IdentityCard", "CCCD");
            SetHeader(_gridTenants, "PhoneNumber", "SĐT");
            SetHeader(_gridTenants, "Email", "Email");
            SetHeader(_gridTenants, "BirthDate", "Ngày sinh");
            SetHeader(_gridTenants, "Address", "Địa chỉ");
            SetHeader(_gridTenants, "TemporaryRegistration", "Tạm trú");
            SetHeader(_gridTenants, "TemporaryRegistrationDate", "Ngày đăng ký");
            SetHeader(_gridTenants, "TemporaryRegistrationExpiry", "Hết hạn");
            SetHeader(_gridTenants, "IsActive", "Kích hoạt");
            SetHeader(_gridTenants, "CreatedDate", "Tạo lúc");
            SetHeader(_gridTenants, "UpdatedDate", "Cập nhật");

            FormatDate(_gridTenants, "BirthDate");
            FormatDate(_gridTenants, "TemporaryRegistrationDate");
            FormatDate(_gridTenants, "TemporaryRegistrationExpiry");
            FormatDateTime(_gridTenants, "CreatedDate");
            FormatDateTime(_gridTenants, "UpdatedDate");

            SetDisplayOrder(_gridTenants,
                "TenantId", "FullName", "IdentityCard", "PhoneNumber", "Email",
                "BirthDate", "TemporaryRegistration", "TemporaryRegistrationDate", "TemporaryRegistrationExpiry",
                "IsActive", "CreatedDate", "UpdatedDate", "Address");

            if (_gridTenants.Columns.Contains("Address"))
                _gridTenants.Columns["Address"].FillWeight = 170;
        }

        private void ApplyDependentGridPresentation()
        {
            SetHeader(_gridDependents, "DependentId", "ID");
            SetHeader(_gridDependents, "TenantId", "TenantId");
            SetHeader(_gridDependents, "FullName", "Họ tên");
            SetHeader(_gridDependents, "Relationship", "Quan hệ");
            SetHeader(_gridDependents, "PhoneNumber", "SĐT");
            SetHeader(_gridDependents, "CreatedDate", "Tạo lúc");

            HideIfExists(_gridDependents, "TenantId");
            FormatDateTime(_gridDependents, "CreatedDate");

            SetDisplayOrder(_gridDependents, "DependentId", "FullName", "Relationship", "PhoneNumber", "CreatedDate");
        }

        private void ApplyHistoryGridPresentation()
        {
            SetHeader(_gridHistory, "HistoryId", "ID");
            SetHeader(_gridHistory, "TenantId", "TenantId");
            SetHeader(_gridHistory, "RoomId", "RoomId");
            SetHeader(_gridHistory, "RoomNumber", "Phòng");
            SetHeader(_gridHistory, "CheckInDate", "Ngày vào");
            SetHeader(_gridHistory, "CheckOutDate", "Ngày ra");
            SetHeader(_gridHistory, "Status", "Trạng thái");
            SetHeader(_gridHistory, "Notes", "Ghi chú");
            SetHeader(_gridHistory, "CreatedDate", "Tạo lúc");

            HideIfExists(_gridHistory, "TenantId");
            HideIfExists(_gridHistory, "RoomId");
            FormatDate(_gridHistory, "CheckInDate");
            FormatDate(_gridHistory, "CheckOutDate");
            FormatDateTime(_gridHistory, "CreatedDate");

            SetDisplayOrder(_gridHistory, "HistoryId", "RoomNumber", "CheckInDate", "CheckOutDate", "Status", "Notes", "CreatedDate");
        }

        private void ApplyTenantFilter()
        {
            if (_tenantTable == null) return;

            string rawKeyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholder) rawKeyword = string.Empty;
            string keyword = rawKeyword.ToLowerInvariant();

            int activeChoice = _cboActive.SelectedIndex; // 0 all, 1 active, 2 inactive
            var rows = _tenantTable.AsEnumerable();

            if (activeChoice != 0 && _tenantTable.Columns.Contains("IsActive"))
            {
                bool want = activeChoice == 1;
                rows = rows.Where(r =>
                {
                    if (r["IsActive"] == DBNull.Value) return false;
                    try { return Convert.ToBoolean(r["IsActive"]) == want; } catch { return false; }
                });
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rows = rows.Where(r =>
                    Contains(r, "FullName", keyword) ||
                    Contains(r, "IdentityCard", keyword) ||
                    Contains(r, "PhoneNumber", keyword) ||
                    Contains(r, "Email", keyword));
            }

            var filtered = rows.Any() ? rows.CopyToDataTable() : _tenantTable.Clone();
            _gridTenants.DataSource = filtered;
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
        }

        private void ApplyTenantDetailsFilter()
        {
            int tenantId = GetSelectedTenantId();
            bool hasTenant = tenantId > 0;

            SetEnabledDependentButtons(hasTenant);
            SetEnabledHistoryButtons(hasTenant);

            if (_dependentTable != null)
            {
                var view = _dependentTable.DefaultView;
                view.RowFilter = hasTenant ? $"TenantId = {tenantId}" : "1=0";
                _gridDependents.DataSource = view;
            }

            if (_historyTable != null)
            {
                var view = _historyTable.DefaultView;
                view.RowFilter = hasTenant ? $"TenantId = {tenantId}" : "1=0";
                _gridHistory.DataSource = view;
            }
        }

        private void SetEnabledDependentButtons(bool enabled)
        {
            _btnDepAdd.Enabled = enabled;
            _btnDepEdit.Enabled = enabled;
            _btnDepDelete.Enabled = enabled;
        }

        private void SetEnabledHistoryButtons(bool enabled)
        {
            _btnHisAdd.Enabled = enabled;
            _btnHisEdit.Enabled = enabled;
            _btnHisDelete.Enabled = enabled;
            _btnHisCheckout.Enabled = enabled;
        }

        private int GetSelectedTenantId()
        {
            var row = GetCurrentRow(_gridTenants);
            if (row == null) return 0;
            return ReadInt(row, "TenantId");
        }

        private async System.Threading.Tasks.Task AddTenantAsync()
        {
            using (var frm = new FrmTenantEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadAllAsync();
            }
        }

        private async System.Threading.Tasks.Task EditSelectedTenantAsync()
        {
            var row = GetCurrentRow(_gridTenants);
            if (row == null)
            {
                MessageBox.Show("Chọn một khách thuê để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmTenantEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadAllAsync();
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedTenantAsync()
        {
            var row = GetCurrentRow(_gridTenants);
            if (row == null)
            {
                MessageBox.Show("Chọn một khách thuê để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = ReadInt(row, "TenantId");
            string name = ReadString(row, "FullName") ?? id.ToString();

            if (MessageBox.Show($"Xóa khách thuê \"{name}\"?\n(Phụ thuộc/hợp đồng liên quan có thể khiến xóa thất bại)", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteTenantAsync(id);
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa khách thuê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task ToggleTenantActiveAsync()
        {
            var row = GetCurrentRow(_gridTenants);
            if (row == null)
            {
                MessageBox.Show("Chọn một khách thuê để bật/tắt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = ReadInt(row, "TenantId");
            string name = ReadString(row, "FullName") ?? id.ToString();
            bool current = TryReadBool(row, "IsActive") ?? true;
            bool next = !current;

            if (MessageBox.Show($"Chuyển \"{name}\" sang {(next ? "Đang hoạt động" : "Đã tắt")}?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.UpdateTenantAsync(
                    id,
                    ReadString(row, "FullName"),
                    ReadString(row, "IdentityCard"),
                    ReadString(row, "PhoneNumber"),
                    ReadString(row, "Email"),
                    TryReadDate(row, "BirthDate"),
                    ReadString(row, "Address"),
                    ReadString(row, "TemporaryRegistration"),
                    TryReadDate(row, "TemporaryRegistrationDate"),
                    TryReadDate(row, "TemporaryRegistrationExpiry"),
                    next);

                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật trạng thái: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task AddDependentAsync()
        {
            int tenantId = GetSelectedTenantId();
            if (tenantId <= 0) return;

            using (var frm = new FrmDependentEditor(_bll, tenantId))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadAllAsync();
            }
        }

        private async System.Threading.Tasks.Task EditSelectedDependentAsync()
        {
            int tenantId = GetSelectedTenantId();
            if (tenantId <= 0) return;

            var row = GetCurrentRow(_gridDependents);
            if (row == null)
            {
                MessageBox.Show("Chọn một người ở chung để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmDependentEditor(_bll, tenantId, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadAllAsync();
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedDependentAsync()
        {
            var row = GetCurrentRow(_gridDependents);
            if (row == null)
            {
                MessageBox.Show("Chọn một người ở chung để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = ReadInt(row, "DependentId");
            string name = ReadString(row, "FullName") ?? id.ToString();

            if (MessageBox.Show($"Xóa người ở chung \"{name}\"?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteDependentAsync(id);
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa người ở chung: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task AddHistoryAsync()
        {
            int tenantId = GetSelectedTenantId();
            if (tenantId <= 0) return;

            using (var frm = new FrmTenantHistoryEditor(_bll, tenantId))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadAllAsync();
            }
        }

        private async System.Threading.Tasks.Task EditSelectedHistoryAsync()
        {
            int tenantId = GetSelectedTenantId();
            if (tenantId <= 0) return;

            var row = GetCurrentRow(_gridHistory);
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng lịch sử để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmTenantHistoryEditor(_bll, tenantId, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadAllAsync();
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedHistoryAsync()
        {
            var row = GetCurrentRow(_gridHistory);
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng lịch sử để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = ReadInt(row, "HistoryId");
            if (MessageBox.Show($"Xóa lịch sử ID {id}?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteTenantHistoryAsync(id);
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa lịch sử: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task CheckoutSelectedHistoryAsync()
        {
            var row = GetCurrentRow(_gridHistory);
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng lịch sử để check-out.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int historyId = ReadInt(row, "HistoryId");
            int roomId = ReadInt(row, "RoomId");
            DateTime? checkIn = TryReadDate(row, "CheckInDate");
            DateTime? checkOut = TryReadDate(row, "CheckOutDate");

            if (checkIn == null)
            {
                MessageBox.Show("Dòng lịch sử thiếu ngày vào.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (checkOut != null)
            {
                MessageBox.Show("Dòng lịch sử này đã có ngày ra.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Xác nhận check-out (ngày ra = hôm nay, trạng thái = Completed)?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.UpdateTenantHistoryAsync(
                    historyId,
                    roomId,
                    checkIn.Value,
                    DateTime.Today,
                    "Completed",
                    ReadString(row, "Notes"));

                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi check-out: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static DataRow GetCurrentRow(DataGridView grid)
        {
            if (grid?.CurrentRow == null || grid.CurrentRow.DataBoundItem == null) return null;
            if (grid.CurrentRow.DataBoundItem is DataRowView drv) return drv.Row;
            return null;
        }

        private static void SetHeader(DataGridView grid, string columnName, string headerText)
        {
            if (grid.Columns.Contains(columnName))
                grid.Columns[columnName].HeaderText = headerText;
        }

        private static void HideIfExists(DataGridView grid, string columnName)
        {
            if (grid.Columns.Contains(columnName))
                grid.Columns[columnName].Visible = false;
        }

        private static void FormatDate(DataGridView grid, string columnName)
        {
            if (grid.Columns.Contains(columnName))
                grid.Columns[columnName].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private static void FormatDateTime(DataGridView grid, string columnName)
        {
            if (grid.Columns.Contains(columnName))
                grid.Columns[columnName].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        private static void SetDisplayOrder(DataGridView grid, params string[] order)
        {
            int index = 0;
            foreach (var name in order)
            {
                if (grid.Columns.Contains(name))
                {
                    grid.Columns[name].DisplayIndex = index;
                    index++;
                }
            }
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

        private static Button MakeSmallButton(string text, Color backColor, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                Width = 92,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
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

        private static bool? TryReadBool(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (bool.TryParse(v.ToString(), out var b)) return b;
                    try { return Convert.ToBoolean(v); } catch { }
                }
            }
            return null;
        }

        private static DateTime? TryReadDate(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (DateTime.TryParse(v.ToString(), out var d)) return d.Date;
                    try
                    {
                        if (v is DateTime dt) return dt.Date;
                    }
                    catch { }
                }
            }
            return null;
        }
    }
}
