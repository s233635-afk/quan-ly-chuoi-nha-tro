using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmStaffManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo tài khoản/họ tên/email/số điện thoại...";

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

        public FrmStaffManager()
        {
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            Text = "Quản lý Nhân viên";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1200;
            Height = 650;
            BackColor = Color.FromArgb(245, 247, 250);

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
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 252);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.DoubleClick += async (s, e) => await EditSelectedAsync();
            _grid.CellFormatting += Grid_CellFormatting;

            _txtSearch = new TextBox { Width = 300 };
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

            _cboBranch = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboBranch.SelectedIndexChanged += (s, e) => ApplyFilter();

            _cboActive = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboActive.Items.AddRange(new object[] { "Tất cả", "Kích hoạt", "Đã tắt" });
            _cboActive.SelectedIndex = 0;
            _cboActive.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0" };

            _btnAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), async (s, e) => await AddNewAsync());
            _btnEdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), async (s, e) => await EditSelectedAsync());
            _btnDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteSelectedAsync());
            _btnToggleActive = MakeButton("Bật/Tắt", Color.FromArgb(103, 58, 183), async (s, e) => await ToggleActiveAsync());
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
            actions.Controls.Add(_btnToggleActive);
            actions.Controls.Add(_btnRefresh);

            var filterHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var pnlSearch = new Panel
            {
                BackColor = Color.FromArgb(245, 247, 250),
                Height = 34,
                Width = 320,
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

            var lblBranch = new Label { Text = "Chi nhánh:", AutoSize = true, ForeColor = Color.FromArgb(70, 70, 70) };
            lblBranch.Location = new Point(pnlSearch.Right + 14, 9);
            _cboBranch.Location = new Point(lblBranch.Right + 6, 6);

            var lblActive = new Label { Text = "Trạng thái:", AutoSize = true, ForeColor = Color.FromArgb(70, 70, 70) };
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

            var summary = new Panel { Dock = DockStyle.Right, Width = 220, BackColor = Color.Transparent };
            _lblCount.Location = new Point(0, 18);
            _lblCount.Font = new Font("Segoe UI", 10, FontStyle.Bold);
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
                await LoadBranchLookupAsync();
                _rawTable = await _bll.GetStaffAsync();
                _grid.DataSource = _rawTable;
                ApplyGridPresentation();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async System.Threading.Tasks.Task LoadBranchLookupAsync()
        {
            _branchTable = await _bll.GetBranchesAsync();
            var branchSelect = new DataTable();
            branchSelect.Columns.Add("BranchId", typeof(int));
            branchSelect.Columns.Add("BranchDisplay", typeof(string));
            branchSelect.Rows.Add(0, "Tất cả");

            if (_branchTable != null && _branchTable.Columns.Contains("BranchId"))
            {
                foreach (DataRow r in _branchTable.Rows)
                {
                    if (!int.TryParse(r["BranchId"]?.ToString(), out var id)) continue;
                    string code = _branchTable.Columns.Contains("BranchCode") ? r["BranchCode"]?.ToString() : null;
                    string name = _branchTable.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : null;
                    string display = $"{code} - {name}".Trim(' ', '-');
                    if (string.IsNullOrWhiteSpace(display)) display = "Chi nhánh " + id;
                    branchSelect.Rows.Add(id, display);
                }
            }

            _cboBranch.DataSource = branchSelect;
            _cboBranch.DisplayMember = "BranchDisplay";
            _cboBranch.ValueMember = "BranchId";
        }

        private void ApplyGridPresentation()
        {
            SetHeader("UserId", "ID");
            SetHeader("UserName", "Tài khoản");
            SetHeader("FullName", "Họ tên");
            SetHeader("Email", "Email");
            SetHeader("Phone", "SĐT");
            SetHeader("BranchName", "Chi nhánh");
            SetHeader("IsActive", "Kích hoạt");
            SetHeader("CreatedDate", "Tạo lúc");
            SetHeader("UpdatedDate", "Cập nhật");

            HideIfExists("RoleId");
            HideIfExists("BranchId");
            HideIfExists("Password");

            FormatDateTime("CreatedDate");
            FormatDateTime("UpdatedDate");

            SetDisplayOrder(
                "UserId",
                "UserName",
                "FullName",
                "Email",
                "Phone",
                "BranchName",
                "IsActive",
                "CreatedDate",
                "UpdatedDate"
            );
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
                bool wantActive = activeChoice == 1;
                rows = rows.Where(r =>
                {
                    if (r["IsActive"] == DBNull.Value) return false;
                    try { return Convert.ToBoolean(r["IsActive"]) == wantActive; } catch { return false; }
                });
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rows = rows.Where(r =>
                    Contains(r, "UserName", keyword) ||
                    Contains(r, "Username", keyword) ||
                    Contains(r, "FullName", keyword) ||
                    Contains(r, "Email", keyword) ||
                    Contains(r, "Phone", keyword) ||
                    Contains(r, "BranchName", keyword) ||
                    Contains(r, "UserId", keyword));
            }

            var filtered = _rawTable.Clone();
            foreach (var r in rows)
                filtered.ImportRow(r);

            _grid.DataSource = filtered;
            ApplyGridPresentation();
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
        }

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            if (_grid.CurrentRow.DataBoundItem is DataRowView drv) return drv.Row;
            return null;
        }

        private async System.Threading.Tasks.Task AddNewAsync()
        {
            using (var frm = new FrmStaffEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadDataAsync();
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

            using (var frm = new FrmStaffEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadDataAsync();
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

            int id = ReadInt(row, "UserId");
            string name = ReadString(row, "FullName") ?? ReadString(row, "UserName") ?? id.ToString();
            if (id <= 0)
            {
                MessageBox.Show("Không xác định được UserId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Xóa nhân viên \"{name}\" (ID {id})?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteStaffUserAsync(id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task ToggleActiveAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một nhân viên để bật/tắt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = ReadInt(row, "UserId");
            if (id <= 0)
            {
                MessageBox.Show("Không xác định được UserId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool current = TryReadBool(row, "IsActive") ?? true;
            bool next = !current;
            string name = ReadString(row, "FullName") ?? ReadString(row, "UserName") ?? id.ToString();

            if (MessageBox.Show($"Chuyển \"{name}\" sang {(next ? "Kích hoạt" : "Đã tắt")}?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                int? branchId = TryReadIntNullable(row, "BranchId");
                string fullName = ReadString(row, "FullName");
                string email = ReadString(row, "Email");
                string phone = ReadString(row, "Phone");
                await _bll.UpdateStaffUserAsync(id, fullName, email, phone, branchId, next, null);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật trạng thái: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (grid.Columns[e.ColumnIndex].Name == "IsActive" && e.Value != null)
            {
                try
                {
                    bool act = Convert.ToBoolean(e.Value);
                    if (!act) e.CellStyle.ForeColor = Color.FromArgb(211, 47, 47);
                }
                catch { }
            }
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

        private void FormatDateTime(string columnName)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
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
    }
}
