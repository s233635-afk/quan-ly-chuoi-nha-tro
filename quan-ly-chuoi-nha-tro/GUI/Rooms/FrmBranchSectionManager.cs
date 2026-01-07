using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmBranchSectionManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo mã/tên khu/dãy...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly int? _presetBranchId;
        private DataTable _raw;
        private DataTable _branches;

        private DataGridView _grid;
        private TextBox _txtSearch;
        private ComboBox _cboBranch;
        private ComboBox _cboActive;
        private Label _lblCount;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnRefresh;

        public FrmBranchSectionManager() : this(null)
        {
        }

        public FrmBranchSectionManager(int? branchId)
        {
            _presetBranchId = branchId;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            Text = "Quản lý Khu/Dãy";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1100;
            Height = 650;
            BackColor = UiKit.AppBackground;
            Font = new Font("Segoe UI", 9, FontStyle.Regular); // Đảm bảo font hỗ trợ tiếng Việt

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
            UiKit.StyleGrid(_grid);
            _grid.DoubleClick += (s, e) => EditSelected();
            _grid.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                {
                    e.Handled = true;
                    _ = DeleteSelectedAsync();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    EditSelected();
                }
            };

            _txtSearch = new TextBox { Width = 280 };
            var pnlSearch = UiKit.MakeSearchPanel(_txtSearch, 320, SearchPlaceholder, ApplyFilter);

            _cboBranch = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboBranch.SelectedIndexChanged += (s, e) => ApplyFilter();

            _cboActive = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboActive.Items.AddRange(new object[] { "Tất cả", "Đang hoạt động", "Đã tắt" });
            _cboActive.SelectedIndex = 0;
            _cboActive.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            _btnAdd = UiKit.MakeButton("Thêm", UiKit.Primary, (s, e) => AddNew(), 92);
            _btnEdit = UiKit.MakeButton("Sửa", UiKit.Primary, (s, e) => EditSelected(), 92);
            _btnDelete = UiKit.MakeButton("Xóa", UiKit.Danger, async (s, e) => await DeleteSelectedAsync(), 92);
            _btnRefresh = UiKit.MakeButton("Tải lại", UiKit.Primary, async (s, e) => await LoadDataAsync(), 92);

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
            actions.Controls.Add(_btnRefresh);

            var filterHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, Location = new Point(0, 9), ForeColor = UiKit.MutedText, Font = new Font("Segoe UI", 9, FontStyle.Regular) };
            pnlSearch.Location = new Point(lblSearch.Right + 6, 10);

            var lblBranch = new Label { Text = "Chi nhánh:", AutoSize = true, ForeColor = UiKit.MutedText, Font = new Font("Segoe UI", 9, FontStyle.Regular) };
            lblBranch.Location = new Point(pnlSearch.Right + 14, 9);
            _cboBranch.Location = new Point(lblBranch.Right + 6, 6);

            var lblActive = new Label { Text = "Kích hoạt:", AutoSize = true, ForeColor = UiKit.MutedText, Font = new Font("Segoe UI", 9, FontStyle.Regular) };
            lblActive.Location = new Point(_cboBranch.Right + 14, 9);
            _cboActive.Location = new Point(lblActive.Right + 6, 6);

            filterHost.Controls.Add(lblSearch);
            filterHost.Controls.Add(pnlSearch);
            filterHost.Controls.Add(lblBranch);
            filterHost.Controls.Add(_cboBranch);
            filterHost.Controls.Add(lblActive);
            filterHost.Controls.Add(_cboActive);

            filterHost.Resize += (s, e) =>
            {
                pnlSearch.Location = new Point(lblSearch.Right + 6, 10);
                lblBranch.Location = new Point(pnlSearch.Right + 14, 9);
                _cboBranch.Location = new Point(lblBranch.Right + 6, 6);
                lblActive.Location = new Point(_cboBranch.Right + 14, 9);
                _cboActive.Location = new Point(lblActive.Right + 6, 6);
            };

            var summary = new Panel { Dock = DockStyle.Right, Width = 160, BackColor = Color.Transparent };
            _lblCount.Location = new Point(0, 16);
            summary.Controls.Add(_lblCount);

            top.Controls.Add(filterHost);
            top.Controls.Add(summary);
            top.Controls.Add(actions);

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BackColor };
            gridHost.Controls.Add(_grid);

            Controls.Add(gridHost);
            Controls.Add(top);

            Load += async (s, e) => await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                TextFixer.FixDataTable(_branches, "BranchCode", "BranchName");

                _raw = await _bll.GetBranchSectionsAsync();
                TextFixer.FixDataTable(_raw, "SectionCode", "SectionName", "Description");

                var allowedIds = AdminBranchScope.GetAllowedBranchIds(_branches);
                _raw = AdminBranchScope.FilterByBranchIds(_raw, allowedIds);
                EnrichBranchName(_raw, _branches);
                BindBranchFilter();
                _grid.DataSource = _raw;
                ApplyGridPresentation();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "LoadSections", "Lỗi tải khu/dãy");
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

        private void BindBranchFilter()
        {
            var dt = new DataTable();
            dt.Columns.Add("BranchId", typeof(int));
            dt.Columns.Add("BranchDisplay", typeof(string));
            dt.Rows.Add(0, "Tất cả");

            if (_branches != null && _branches.Columns.Contains("BranchId"))
            {
                foreach (DataRow r in _branches.Rows)
                {
                    if (!int.TryParse(r["BranchId"]?.ToString(), out var bid)) continue;
                    string code = _branches.Columns.Contains("BranchCode") ? r["BranchCode"]?.ToString() : null;
                    string name = _branches.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : null;
                    string display = string.IsNullOrWhiteSpace(code) ? name : $"{code} - {name}";
                    dt.Rows.Add(bid, display);
                }
            }

            _cboBranch.DataSource = dt;
            _cboBranch.DisplayMember = "BranchDisplay";
            _cboBranch.ValueMember = "BranchId";
            if (_presetBranchId.HasValue)
            {
                _cboBranch.SelectedValue = _presetBranchId.Value;
                _cboBranch.Enabled = false;
            }
        }

        private static void EnrichBranchName(DataTable sections, DataTable branches)
        {
            if (sections == null || branches == null) return;
            if (!sections.Columns.Contains("BranchId")) return;
            if (!sections.Columns.Contains("BranchName"))
                sections.Columns.Add("BranchName", typeof(string));

            var map = branches.AsEnumerable()
                .Where(r => branches.Columns.Contains("BranchId"))
                .Select(r =>
                {
                    int.TryParse(r["BranchId"]?.ToString(), out var bid);
                    string name = branches.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : null;
                    return new { bid, name };
                })
                .Where(x => x.bid > 0)
                .GroupBy(x => x.bid)
                .ToDictionary(g => g.Key, g => g.First().name);

            foreach (DataRow r in sections.Rows)
            {
                if (int.TryParse(r["BranchId"]?.ToString(), out var bid) && map.TryGetValue(bid, out var name))
                    r["BranchName"] = name;
            }
        }

        private void ApplyGridPresentation()
        {
            SetHeader("SectionId", "ID");
            SetHeader("BranchName", "Chi nhánh");
            SetHeader("SectionCode", "Mã");
            SetHeader("SectionName", "Tên");
            SetHeader("IsActive", "Kích hoạt");
            HideIfExists("BranchId");
            FormatBool("IsActive");
            SetDisplayOrder("SectionId", "BranchName", "SectionCode", "SectionName", "IsActive");
        }

        private void ApplyFilter()
        {
            if (_raw == null) return;

            string rawKeyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholder) rawKeyword = string.Empty;
            string keyword = rawKeyword.ToLowerInvariant();

            int branchId = _cboBranch.SelectedValue is int b ? b : 0;
            int activeChoice = _cboActive.SelectedIndex; // 0 all, 1 active, 2 inactive

            var rows = _raw.AsEnumerable();
            if (branchId > 0 && _raw.Columns.Contains("BranchId"))
                rows = rows.Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == branchId);

            if (activeChoice != 0 && _raw.Columns.Contains("IsActive"))
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
                    Contains(r, "SectionCode", keyword) ||
                    Contains(r, "SectionName", keyword) ||
                    Contains(r, "BranchName", keyword) ||
                    Contains(r, "SectionId", keyword));
            }

            var filtered = _raw.Clone();
            foreach (var r in rows) filtered.ImportRow(r);
            _grid.DataSource = filtered;
            ApplyGridPresentation();
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
        }

        private void AddNew()
        {
            int? preset = _cboBranch.SelectedValue is int b && b > 0 ? (int?)b : null;
            using (var frm = new FrmBranchSectionEditor(_bll, null, preset))
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
                ToastNotification.Info("Chọn một khu/dãy để sửa");
                return;
            }

            using (var frm = new FrmBranchSectionEditor(_bll, row))
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
                ToastNotification.Info("Chọn một khu/dãy để xóa");
                return;
            }

            int id = row.Table.Columns.Contains("SectionId") ? Convert.ToInt32(row["SectionId"]) : 0;
            if (id <= 0) return;

            if (!await ModernConfirmDialog.ShowAsync("Bạn chắc chắn muốn xóa (tắt) khu/dãy này?", "Xác nhận"))
                return;

            try
            {
                await _bll.DeleteBranchSectionAsync(id);
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "DeleteSection", "Lỗi xóa khu/dãy");
            }
        }

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            return (_grid.CurrentRow.DataBoundItem as DataRowView)?.Row;
        }

        private static bool Contains(DataRow row, string col, string keyword)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return false;
            var v = row[col];
            if (v == null || v == DBNull.Value) return false;
            return v.ToString().ToLowerInvariant().Contains(keyword);
        }

        private void SetHeader(string col, string header)
        {
            if (_grid.Columns.Contains(col))
                _grid.Columns[col].HeaderText = header;
        }

        private void HideIfExists(string col)
        {
            if (_grid.Columns.Contains(col))
                _grid.Columns[col].Visible = false;
        }

        private void SetDisplayOrder(params string[] cols)
        {
            int i = 0;
            foreach (var c in cols)
            {
                if (_grid.Columns.Contains(c))
                    _grid.Columns[c].DisplayIndex = i++;
            }
        }

        private void FormatBool(string col)
        {
            if (_grid.Columns.Contains(col))
            {
                _grid.Columns[col].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }
    }
}
