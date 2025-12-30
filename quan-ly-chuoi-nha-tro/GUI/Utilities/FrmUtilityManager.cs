using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmUtilityManager : Form
    {
        private const string SearchPlaceholderReadings = "Tìm theo phòng/loại...";
        private const string SearchPlaceholderTypes = "Tìm theo tên/mã...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly int? _presetBranchId;
        private readonly bool _isStaffMode;

        private DataTable _readingTable;
        private DataTable _readingTableRaw;
        private DataTable _typeTable;
        private DataTable _typeTableRaw;
        private DataTable _branchTable;
        private System.Collections.Generic.HashSet<int> _allowedBranchIds;

        private TabControl _tabs;
        private DataGridView _gridReadings;
        private DataGridView _gridTypes;

        private TextBox _txtSearchReadings;
        private ComboBox _cboBranch;
        private Label _lblReadingsCount;

        private TextBox _txtSearchTypes;
        private Label _lblTypesCount;

        private Button _btnRAdd;
        private Button _btnREdit;
        private Button _btnRDelete;
        private Button _btnRRefresh;

        private Button _btnTAdd;
        private Button _btnTEdit;
        private Button _btnTDelete;
        private Button _btnTRefresh;

        public FrmUtilityManager() : this(null, false)
        {
        }

        public FrmUtilityManager(int? branchId) : this(branchId, false)
        {
        }

        public FrmUtilityManager(int? branchId, bool isStaffMode)
        {
            _presetBranchId = branchId;
            _isStaffMode = isStaffMode;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            Text = "Điện - Nước - Dịch vụ";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1280;
            Height = 720;
            BackColor = Color.FromArgb(245, 247, 250);

            _tabs = new TabControl { Dock = DockStyle.Fill };
            var tabReadings = new TabPage("Chỉ số") { Padding = new Padding(0), BackColor = Color.White };
            var tabTypes = new TabPage("Loại dịch vụ") { Padding = new Padding(0), BackColor = Color.White };
            _tabs.TabPages.Add(tabReadings);
            _tabs.TabPages.Add(tabTypes);

            _gridReadings = MakeGrid();
            _gridReadings.Dock = DockStyle.Fill;
            _gridReadings.DoubleClick += async (s, e) => await EditReadingAsync();

            _gridTypes = MakeGrid();
            _gridTypes.Dock = DockStyle.Fill;
            _gridTypes.DoubleClick += async (s, e) => await EditTypeAsync();

            _btnRAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), async (s, e) => await AddReadingAsync());
            _btnREdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), async (s, e) => await EditReadingAsync());
            _btnRDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteReadingAsync());
            _btnRRefresh = MakeButton("Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadReadingsAsync());

            _btnTAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), async (s, e) => await AddTypeAsync());
            _btnTEdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), async (s, e) => await EditTypeAsync());
            _btnTDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteTypeAsync());
            _btnTRefresh = MakeButton("Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadTypesAsync());
            _btnRDelete.Visible = !_isStaffMode;
            _btnRDelete.Enabled = !_isStaffMode;
            _btnTDelete.Visible = !_isStaffMode;
            _btnTDelete.Enabled = !_isStaffMode;

            _txtSearchReadings = MakeSearchBox(SearchPlaceholderReadings, () => ApplyReadingsFilter());
            _cboBranch = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboBranch.SelectedIndexChanged += (s, e) => ApplyReadingsFilter();
            _lblReadingsCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            _txtSearchTypes = MakeSearchBox(SearchPlaceholderTypes, () => ApplyTypesFilter());
            _lblTypesCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            tabReadings.Controls.Add(_gridReadings);
            tabReadings.Controls.Add(MakeTopBarReadings());

            tabTypes.Controls.Add(_gridTypes);
            tabTypes.Controls.Add(MakeTopBarTypes());

            Controls.Add(_tabs);
            Load += async (s, e) => await LoadAllAsync();
        }

        private Panel MakeTopBarReadings()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 64, Padding = new Padding(12, 10, 12, 10), BackColor = Color.White };
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            actions.Controls.Add(_btnRAdd);
            actions.Controls.Add(_btnREdit);
            actions.Controls.Add(_btnRDelete);
            actions.Controls.Add(_btnRRefresh);

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
            filters.Controls.Add(_txtSearchReadings);
            filters.Controls.Add(new Label { Text = "Chi nhánh:", AutoSize = true, Margin = new Padding(12, 6, 6, 0) });
            filters.Controls.Add(_cboBranch);
            filters.Controls.Add(new Label { Text = "  ", AutoSize = true });
            filters.Controls.Add(_lblReadingsCount);

            top.Controls.Add(actions);
            top.Controls.Add(filters);
            return top;
        }

        private Panel MakeTopBarTypes()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 64, Padding = new Padding(12, 10, 12, 10), BackColor = Color.White };
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            actions.Controls.Add(_btnTAdd);
            actions.Controls.Add(_btnTEdit);
            actions.Controls.Add(_btnTDelete);
            actions.Controls.Add(_btnTRefresh);

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
            filters.Controls.Add(_txtSearchTypes);
            filters.Controls.Add(new Label { Text = "  ", AutoSize = true });
            filters.Controls.Add(_lblTypesCount);

            top.Controls.Add(actions);
            top.Controls.Add(filters);
            return top;
        }

        private async System.Threading.Tasks.Task LoadAllAsync()
        {
            await LoadBranchesAsync();
            await LoadReadingsAsync();
            await LoadTypesAsync();
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadAllAsync();
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

        private async System.Threading.Tasks.Task LoadReadingsAsync()
        {
            try
            {
                _readingTableRaw = await _bll.GetUtilitiesAsync();
                TextFixer.ForceFixDataTable(_readingTableRaw, "RoomNumber", "UtilityName", "UtilityCode", "Notes");
                _readingTableRaw = AdminBranchScope.FilterByBranchIds(_readingTableRaw, _allowedBranchIds);
                _readingTable = BuildReadingsDisplayTable(_readingTableRaw);
                _gridReadings.DataSource = _readingTable;
                ApplyReadingsGridPresentation();
                ApplyReadingsFilter();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "LoadReadings", "Lỗi tải chỉ số");
            }
        }

        private async System.Threading.Tasks.Task LoadTypesAsync()
        {
            try
            {
                _typeTableRaw = await _bll.GetUtilityTypesAsync();
                TextFixer.ForceFixDataTable(_typeTableRaw, "UtilityName", "UtilityCode", "Unit", "Description");
                _typeTable = BuildTypesDisplayTable(_typeTableRaw);
                _gridTypes.DataSource = _typeTable;
                ApplyTypesGridPresentation();
                ApplyTypesFilter();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "LoadTypes", "Lỗi tải loại dịch vụ");
            }
        }

        private void ApplyReadingsGridPresentation()
        {
            SetHeader(_gridReadings, "ReadingId", "ID");
            SetHeader(_gridReadings, "RoomNumber", "Phòng");
            SetHeader(_gridReadings, "UtilityName", "Loại");
            SetHeader(_gridReadings, "UtilityCode", "Mã");
            SetHeader(_gridReadings, "ReadingDate", "Ngày");
            SetHeader(_gridReadings, "PreviousReading", "Chỉ số cũ");
            SetHeader(_gridReadings, "CurrentReading", "Chỉ số mới");
            SetHeader(_gridReadings, "UsageAmount", "Tiêu thụ");
            SetHeader(_gridReadings, "UnitPrice", "Đơn giá");
            SetHeader(_gridReadings, "TotalCost", "Thành tiền");
            SetHeader(_gridReadings, "Notes", "Ghi chú");
            SetHeader(_gridReadings, "CreatedDate", "Tạo lúc");

            HideIfExists(_gridReadings, "RoomId");
            HideIfExists(_gridReadings, "BranchId");
            HideIfExists(_gridReadings, "UtilityTypeId");

            FormatDate(_gridReadings, "ReadingDate");
            FormatDateTime(_gridReadings, "CreatedDate");

            SetDisplayOrder(_gridReadings,
                "ReadingId", "RoomNumber", "UtilityName", "ReadingDate",
                "PreviousReading", "CurrentReading", "UsageAmount", "UnitPrice", "TotalCost",
                "Notes", "CreatedDate");
        }

        private void ApplyTypesGridPresentation()
        {
            SetHeader(_gridTypes, "UtilityTypeId", "ID");
            SetHeader(_gridTypes, "UtilityName", "Tên");
            SetHeader(_gridTypes, "UtilityCode", "Mã");
            SetHeader(_gridTypes, "Unit", "Đơn vị");
            SetHeader(_gridTypes, "IsRecurring", "Định kỳ");
            SetHeader(_gridTypes, "DefaultPrice", "Đơn giá");
            SetHeader(_gridTypes, "IsActive", "Kích hoạt");
            SetHeader(_gridTypes, "Description", "Mô tả");
            SetHeader(_gridTypes, "CreatedDate", "Tạo lúc");

            FormatDateTime(_gridTypes, "CreatedDate");

            SetDisplayOrder(_gridTypes,
                "UtilityTypeId", "UtilityName", "UtilityCode", "Unit",
                "DefaultPrice", "IsRecurring", "IsActive", "Description", "CreatedDate");

            if (_gridTypes.Columns.Contains("Description"))
                _gridTypes.Columns["Description"].FillWeight = 180;
        }

        private void ApplyReadingsFilter()
        {
            if (_readingTable == null) return;

            string rawKeyword = (_txtSearchReadings.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholderReadings) rawKeyword = string.Empty;
            string keyword = rawKeyword.ToLowerInvariant();

            int branchId = _cboBranch.SelectedValue is int b ? b : 0;
            var rows = _readingTable.AsEnumerable();

            if (branchId > 0 && _readingTable.Columns.Contains("BranchId"))
                rows = rows.Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == branchId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rows = rows.Where(r =>
                    Contains(r, "RoomNumber", keyword) ||
                    Contains(r, "UtilityName", keyword) ||
                    Contains(r, "UtilityCode", keyword));
            }

            var filtered = rows.Any() ? rows.CopyToDataTable() : _readingTable.Clone();
            TextFixer.ForceFixDataTable(filtered, "RoomNumber", "UtilityName", "UtilityCode", "Notes");
            _gridReadings.DataSource = filtered;
            _lblReadingsCount.Text = $"Tổng: {filtered.Rows.Count}";
        }

        private void ApplyTypesFilter()
        {
            if (_typeTable == null) return;

            string rawKeyword = (_txtSearchTypes.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholderTypes) rawKeyword = string.Empty;
            string keyword = rawKeyword.ToLowerInvariant();

            var rows = _typeTable.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rows = rows.Where(r =>
                    Contains(r, "UtilityName", keyword) ||
                    Contains(r, "UtilityCode", keyword) ||
                    Contains(r, "Unit", keyword));
            }

            var filtered = rows.Any() ? rows.CopyToDataTable() : _typeTable.Clone();
            _gridTypes.DataSource = filtered;
            _lblTypesCount.Text = $"Tổng: {filtered.Rows.Count}";
        }

        private async System.Threading.Tasks.Task AddReadingAsync()
        {
            using (var frm = new FrmUtilityReadingEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadReadingsAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task EditReadingAsync()
        {
            var row = GetCurrentRow(_gridReadings);
            if (row == null)
            {
                ToastNotification.Warning("Chọn một dòng để sửa");
                return;
            }

            using (var frm = new FrmUtilityReadingEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadReadingsAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task DeleteReadingAsync()
        {
            if (_isStaffMode) return;
            var row = GetCurrentRow(_gridReadings);
            if (row == null)
            {
                ToastNotification.Warning("Chọn một dòng để xóa");
                return;
            }

            int id = ReadInt(row, "ReadingId");
            if (!ModernConfirmDialog.ConfirmDanger($"Xóa chỉ số ID {id}?")) return;

            try
            {
                await _bll.DeleteUtilityReadingAsync(id);
                ToastNotification.Success("Xóa thành công");
                await LoadReadingsAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "DeleteReading", "Lỗi xóa chỉ số");
            }
        }

        private async System.Threading.Tasks.Task AddTypeAsync()
        {
            using (var frm = new FrmUtilityTypeEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadTypesAsync();
            }
        }

        private async System.Threading.Tasks.Task EditTypeAsync()
        {
            var row = GetSelectedTypeRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một dòng để sửa");
                return;
            }

            using (var frm = new FrmUtilityTypeEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadTypesAsync();
            }
        }

        private async System.Threading.Tasks.Task DeleteTypeAsync()
        {
            if (_isStaffMode) return;
            var row = GetSelectedTypeRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một dòng để xóa");
                return;
            }

            int id = ReadInt(row, "UtilityTypeId");
            string name = ReadString(row, "UtilityName") ?? id.ToString();

            if (!ModernConfirmDialog.ConfirmDanger($"Xóa loại \"{name}\"?\n(Nếu đã có chỉ số sử dụng thì có thể xóa thất bại)")) return;

            try
            {
                await _bll.DeleteUtilityTypeAsync(id);
                ToastNotification.Success("Xóa thành công");
                await LoadTypesAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "DeleteType", "Lỗi xóa loại dịch vụ");
            }
        }

        private static TextBox MakeSearchBox(string placeholder, Action onChanged)
        {
            var tb = new TextBox { Width = 320, ForeColor = Color.Gray, Text = placeholder };
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

        private static Button MakeButton(string text, Color backColor, EventHandler onClick)
        {
            var b = new ModernButton
            {
                Text = text,
                Width = 96,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BaseColor = backColor,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        private static DataRow GetCurrentRow(DataGridView grid)
        {
            if (grid?.CurrentRow == null || grid.CurrentRow.DataBoundItem == null) return null;
            if (grid.CurrentRow.DataBoundItem is DataRowView drv) return drv.Row;
            return null;
        }

        private DataRow GetSelectedTypeRow()
        {
            var row = GetCurrentRow(_gridTypes);
            if (row == null) return null;
            int id = ReadInt(row, "UtilityTypeId");
            if (_typeTableRaw == null || id <= 0) return row;
            var matches = _typeTableRaw.Select($"UtilityTypeId = {id}");
            return matches.Length > 0 ? matches[0] : row;
        }

        private static DataTable BuildReadingsDisplayTable(DataTable source)
        {
            if (source == null) return null;
            var table = source.Clone();

            foreach (DataRow r in source.Rows)
            {
                string code = ReadString(r, "UtilityCode");
                string name = ReadString(r, "UtilityName");
                if (IsElectricOrWater(code, name))
                {
                    var newRow = table.NewRow();
                    newRow.ItemArray = r.ItemArray.Clone() as object[];
                    FixTextColumns(newRow, "RoomNumber", "UtilityName", "UtilityCode", "Notes");
                    if (table.Columns.Contains("UtilityName"))
                        newRow["UtilityName"] = "Điện/Nước";
                    table.Rows.Add(newRow);
                }
                else if (IsInternet(code, name))
                {
                    var newRow = table.NewRow();
                    newRow.ItemArray = r.ItemArray.Clone() as object[];
                    FixTextColumns(newRow, "RoomNumber", "UtilityName", "UtilityCode", "Notes");
                    if (table.Columns.Contains("UtilityName"))
                        newRow["UtilityName"] = "Internet";
                    table.Rows.Add(newRow);
                }
            }

            return table;
        }

        private static DataTable BuildTypesDisplayTable(DataTable source)
        {
            if (source == null) return null;
            var table = source.Clone();
            if (!source.Columns.Contains("UtilityTypeId")) return table;

            DataRow elecRow = null;
            DataRow waterRow = null;
            DataRow internetRow = null;

            foreach (DataRow r in source.Rows)
            {
                string code = ReadString(r, "UtilityCode");
                string name = ReadString(r, "UtilityName");
                if (internetRow == null && IsInternet(code, name)) internetRow = r;
                if (elecRow == null && IsElectric(code, name)) elecRow = r;
                if (waterRow == null && IsWater(code, name)) waterRow = r;
            }

            var mainRow = elecRow ?? waterRow;
            if (mainRow != null)
            {
                var newRow = table.NewRow();
                newRow.ItemArray = mainRow.ItemArray.Clone() as object[];
                if (table.Columns.Contains("UtilityName"))
                    newRow["UtilityName"] = "Điện/Nước";
                if (table.Columns.Contains("UtilityCode"))
                    newRow["UtilityCode"] = "ELEC/WATER";
                table.Rows.Add(newRow);
            }

            if (internetRow != null)
            {
                var newRow = table.NewRow();
                newRow.ItemArray = internetRow.ItemArray.Clone() as object[];
                if (table.Columns.Contains("UtilityName"))
                    newRow["UtilityName"] = "Internet";
                table.Rows.Add(newRow);
            }

            if (table.Rows.Count == 0)
            {
                foreach (DataRow r in source.Rows)
                {
                    var newRow = table.NewRow();
                    newRow.ItemArray = r.ItemArray.Clone() as object[];
                    table.Rows.Add(newRow);
                    if (table.Rows.Count >= 2) break;
                }
            }

            return table;
        }

        private static bool IsInternet(string code, string name)
        {
            string codeUpper = (code ?? string.Empty).ToUpperInvariant();
            string nameLower = (name ?? string.Empty).ToLowerInvariant();
            return codeUpper.Contains("INTERNET") || codeUpper == "NET" || nameLower.Contains("internet");
        }

        private static bool IsElectric(string code, string name)
        {
            string codeUpper = (code ?? string.Empty).ToUpperInvariant();
            string nameLower = (name ?? string.Empty).ToLowerInvariant();
            return codeUpper.Contains("ELEC") || nameLower.Contains("dien") || nameLower.Contains("điện");
        }

        private static bool IsWater(string code, string name)
        {
            string codeUpper = (code ?? string.Empty).ToUpperInvariant();
            string nameLower = (name ?? string.Empty).ToLowerInvariant();
            return codeUpper.Contains("WATER") || nameLower.Contains("nuoc") || nameLower.Contains("nước");
        }

        private static bool IsElectricOrWater(string code, string name)
        {
            return IsElectric(code, name) || IsWater(code, name);
        }

        private static void FixTextColumns(DataRow row, params string[] columns)
        {
            if (row?.Table == null || columns == null || columns.Length == 0) return;
            foreach (var column in columns)
            {
                if (string.IsNullOrWhiteSpace(column)) continue;
                if (!row.Table.Columns.Contains(column)) continue;
                var value = row[column];
                if (value == null || value == DBNull.Value) continue;

                string text = value.ToString();
                string fixedText = FixTextRepeat(text);
                if (!string.Equals(text, fixedText, StringComparison.Ordinal))
                    row[column] = fixedText;
            }
        }

        private static string FixTextRepeat(string input)
        {
            string current = input;
            for (int i = 0; i < 2; i++)
            {
                string next = TextFixer.ForceFixUtf8Mojibake(current);
                if (string.Equals(current, next, StringComparison.Ordinal))
                    return current;
                current = next;
            }
            return current;
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
    }
}
