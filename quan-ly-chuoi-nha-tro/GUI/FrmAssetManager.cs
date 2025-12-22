using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmAssetManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo mã/tên/phòng/nhóm...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();

        private DataTable _rawTable;
        private DataTable _branchTable;

        private DataGridView _grid;
        private TextBox _txtSearch;
        private ComboBox _cboBranch;
        private ComboBox _cboActive;
        private Label _lblCount;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnToggleActive;
        private Button _btnRefresh;

        public FrmAssetManager()
        {
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            Text = "Tài Sản";
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
            _btnToggleActive = MakeButton("⚙ Bật/Tắt", Color.FromArgb(103, 58, 183), async (s, e) => await ToggleActiveAsync());
            _btnRefresh = MakeButton("⟳ Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadAsync());

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
            pnlActions.Controls.Add(_btnToggleActive);
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

            _cboActive = new ComboBox
            {
                Width = 140,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9)
            };
            _cboActive.Items.AddRange(new object[] { "Tất cả", "🟢 Kích hoạt", "🔴 Đã tắt" });
            _cboActive.SelectedIndex = 0;
            _cboActive.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblCount = new Label
            {
                AutoSize = true,
                Text = "Tổng: 0",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
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
            pnlFilters.Controls.Add(_cboActive);
            pnlFilters.Controls.Add(_lblCount);

            pnlActionBar.Controls.Add(pnlActions);
            pnlActionBar.Controls.Add(pnlFilters);
            pnlToolbar.Controls.Add(pnlActionBar);

            // ===== GRID =====
            _grid = MakeGrid();
            _grid.Dock = DockStyle.Fill;
            _grid.DoubleClick += async (s, e) => await EditSelectedAsync();

            // Add all controls
            Controls.Add(_grid);
            Controls.Add(pnlToolbar);

            Load += async (s, e) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                await LoadBranchesAsync();
                _rawTable = await _bll.GetAssetsAsync();
                ApplyAdminBranchScopeToAssets();
                TextFixer.FixDataTable(_rawTable, "AssetName", "Category", "Condition", "Description", "RoomNumber");
                _grid.DataSource = _rawTable;
                ApplyGridPresentation();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải tài sản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                TextFixer.FixDataTable(dt, "BranchName");
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
            }
            catch
            {
                _cboBranch.Items.Clear();
                _cboBranch.Items.Add("Tất cả");
                _cboBranch.SelectedIndex = 0;
            }
        }

        private void ApplyAdminBranchScopeToAssets()
        {
            if (!AdminBranchScope.IsEnabled) return;
            if (_rawTable == null || !_rawTable.Columns.Contains("BranchId")) return;
            if (_branchTable == null || !_branchTable.Columns.Contains("BranchId")) return;

            var allowed = _branchTable.AsEnumerable()
                .Select(r => r["BranchId"]?.ToString())
                .Where(s => int.TryParse(s, out var id) && id > 0)
                .Select(int.Parse)
                .ToHashSet();

            if (allowed.Count == 0) return;

            var filtered = _rawTable.Clone();
            foreach (DataRow r in _rawTable.Rows)
            {
                if (!int.TryParse(r["BranchId"]?.ToString(), out var bid)) continue;
                if (!allowed.Contains(bid)) continue;
                filtered.ImportRow(r);
            }
            _rawTable = filtered;
        }

        private void ApplyGridPresentation()
        {
            SetHeader("AssetId", "ID");
            SetHeader("AssetCode", "Mã");
            SetHeader("AssetName", "Tên tài sản");
            SetHeader("Category", "Nhóm");
            SetHeader("RoomNumber", "Phòng");
            SetHeader("Quantity", "SL");
            SetHeader("Condition", "Tình trạng");
            SetHeader("PurchaseDate", "Ngày mua");
            SetHeader("PurchasePrice", "Giá mua");
            SetHeader("Description", "Mô tả");
            SetHeader("IsActive", "Kích hoạt");
            SetHeader("CreatedDate", "Tạo lúc");
            SetHeader("UpdatedDate", "Cập nhật");

            HideIfExists("RoomId");
            HideIfExists("BranchId");

            FormatDate("PurchaseDate");
            FormatDateTime("CreatedDate");
            FormatDateTime("UpdatedDate");

            SetDisplayOrder(
                "AssetId",
                "AssetCode",
                "AssetName",
                "Category",
                "RoomNumber",
                "Quantity",
                "Condition",
                "PurchaseDate",
                "PurchasePrice",
                "IsActive",
                "CreatedDate",
                "UpdatedDate",
                "Description"
            );

            if (_grid.Columns.Contains("Description"))
                _grid.Columns["Description"].FillWeight = 180;
        }

        private void ApplyFilter()
        {
            if (_rawTable == null) return;

            string rawKeyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholder) rawKeyword = string.Empty;
            string keyword = rawKeyword.ToLowerInvariant();

            int branchId = _cboBranch.SelectedValue is int b ? b : 0;
            int activeChoice = _cboActive.SelectedIndex; // 0 all, 1 active, 2 inactive

            var rows = _rawTable.AsEnumerable();

            if (branchId > 0 && _rawTable.Columns.Contains("BranchId"))
                rows = rows.Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == branchId);

            if (activeChoice != 0 && _rawTable.Columns.Contains("IsActive"))
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
                    Contains(r, "AssetCode", keyword) ||
                    Contains(r, "AssetName", keyword) ||
                    Contains(r, "Category", keyword) ||
                    Contains(r, "RoomNumber", keyword));
            }

            var filtered = rows.Any() ? rows.CopyToDataTable() : _rawTable.Clone();
            _grid.DataSource = filtered;
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
        }

        private async System.Threading.Tasks.Task AddNewAsync()
        {
            using (var frm = new FrmAssetEditor(_bll))
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

            using (var frm = new FrmAssetEditor(_bll, row))
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

            int id = ReadInt(row, "AssetId");
            string code = ReadString(row, "AssetCode") ?? id.ToString();

            if (MessageBox.Show($"Xóa tài sản \"{code}\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteAssetAsync(id);
                await LoadAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa tài sản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task ToggleActiveAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để bật/tắt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = ReadInt(row, "AssetId");
            string name = ReadString(row, "AssetName") ?? id.ToString();
            bool current = TryReadBool(row, "IsActive") ?? true;
            bool next = !current;

            if (MessageBox.Show($"Chuyển \"{name}\" sang {(next ? "Kích hoạt" : "Đã tắt")}?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.UpdateAssetAsync(
                    id,
                    ReadString(row, "AssetCode"),
                    ReadString(row, "AssetName"),
                    ReadString(row, "Category"),
                    TryReadIntNullable(row, "RoomId"),
                    ReadInt(row, "Quantity"),
                    ReadString(row, "Condition"),
                    TryReadDate(row, "PurchaseDate"),
                    TryReadDecimalNullable(row, "PurchasePrice"),
                    ReadString(row, "Description"),
                    next);

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
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            var drv = _grid.CurrentRow.DataBoundItem as DataRowView;
            return drv?.Row;
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
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 28 }
            };
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            g.DefaultCellStyle.ForeColor = Color.FromArgb(50, 50, 50);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(179, 211, 247);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;
            g.GridColor = Color.FromArgb(220, 230, 240);
            return g;
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

        private void FormatDate(string columnName)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private void FormatDateTime(string columnName)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        private void SetDisplayOrder(params string[] order)
        {
            int idx = 0;
            foreach (var name in order)
            {
                if (_grid.Columns.Contains(name))
                {
                    _grid.Columns[name].DisplayIndex = idx;
                    idx++;
                }
            }
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

        private static decimal? TryReadDecimalNullable(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (decimal.TryParse(v.ToString(), out var d)) return d;
                    try { return Convert.ToDecimal(v); } catch { }
                }
            }
            return null;
        }
    }
}
