using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmRoomManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo số phòng/chi nhánh/trạng thái...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();

        private DataTable _rawTable;

        private DataGridView _grid;
        private TextBox _txtSearch;
        private ComboBox _cboBranch;
        private ComboBox _cboStatus;
        private ComboBox _cboActive;
        private Label _lblCount;
        private Label _lblSummary;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnToggleActive;
        private Button _btnSetStatus;
        private Button _btnRefresh;
        private Button _btnCatalog;
        private ContextMenuStrip _catalogMenu;

        private DataTable _branchTable;
        private DataTable _statusTable;

        public FrmRoomManager()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý Phòng";
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
            _grid.CellFormatting += Grid_CellFormatting;
            _grid.DoubleClick += async (s, e) => await EditSelectedAsync();

            _txtSearch = new TextBox { Width = 280 };
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

            _cboBranch = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboBranch.SelectedIndexChanged += (s, e) => ApplyFilter();

            _cboStatus = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _cboActive = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboActive.Items.AddRange(new object[] { "Tất cả", "Đang hoạt động", "Đã tắt" });
            _cboActive.SelectedIndex = 0;
            _cboActive.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0" };
            _lblSummary = new Label { AutoSize = true, Text = "Trống: 0 | Đang ở: 0 | Bảo trì: 0", ForeColor = Color.FromArgb(70, 70, 70) };

            _btnAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), async (s, e) => await AddNewAsync());
            _btnEdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), async (s, e) => await EditSelectedAsync());
            _btnDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteSelectedAsync());
            _btnSetStatus = MakeButton("Đổi trạng thái", Color.FromArgb(255, 152, 0), async (s, e) => await ChangeStatusAsync());
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
            actions.Controls.Add(_btnSetStatus);
            actions.Controls.Add(_btnToggleActive);
            actions.Controls.Add(_btnRefresh);

            _catalogMenu = BuildCatalogMenu();
            _btnCatalog = MakeButton("Danh mục ▾", Color.FromArgb(103, 58, 183), (s, e) =>
            {
                _catalogMenu.Show(_btnCatalog, new Point(0, _btnCatalog.Height));
            });
            actions.Controls.Add(_btnCatalog);

            var filterHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var pnlSearch = new Panel
            {
                BackColor = Color.FromArgb(245, 247, 250),
                Height = 34,
                Width = 300,
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

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, ForeColor = Color.FromArgb(70, 70, 70) };
            lblStatus.Location = new Point(_cboBranch.Right + 14, 9);
            _cboStatus.Location = new Point(lblStatus.Right + 6, 6);

            var lblActive = new Label { Text = "Kích hoạt:", AutoSize = true, ForeColor = Color.FromArgb(70, 70, 70) };
            lblActive.Location = new Point(_cboStatus.Right + 14, 9);
            _cboActive.Location = new Point(lblActive.Right + 6, 6);

            filterHost.Controls.Add(lblSearch);
            filterHost.Controls.Add(pnlSearch);
            filterHost.Controls.Add(lblBranch);
            filterHost.Controls.Add(_cboBranch);
            filterHost.Controls.Add(lblStatus);
            filterHost.Controls.Add(_cboStatus);
            filterHost.Controls.Add(lblActive);
            filterHost.Controls.Add(_cboActive);

            filterHost.Resize += (s, e) =>
            {
                pnlSearch.Location = new Point(lblSearch.Right + 6, 10);
                lblBranch.Location = new Point(pnlSearch.Right + 14, 9);
                _cboBranch.Location = new Point(lblBranch.Right + 6, 6);
                lblStatus.Location = new Point(_cboBranch.Right + 14, 9);
                _cboStatus.Location = new Point(lblStatus.Right + 6, 6);
                lblActive.Location = new Point(_cboStatus.Right + 14, 9);
                _cboActive.Location = new Point(lblActive.Right + 6, 6);
            };

            var summary = new Panel { Dock = DockStyle.Right, Width = 320, BackColor = Color.Transparent };
            _lblCount.Location = new Point(0, 6);
            _lblCount.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _lblSummary.Location = new Point(0, 28);
            summary.Controls.Add(_lblCount);
            summary.Controls.Add(_lblSummary);

            top.Controls.Add(filterHost);
            top.Controls.Add(summary);
            top.Controls.Add(actions);

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BackColor };
            gridHost.Controls.Add(_grid);

            Controls.Add(gridHost);
            Controls.Add(top);

            Load += async (s, e) => await LoadDataAsync();
        }

        private ContextMenuStrip BuildCatalogMenu()
        {
            var menu = new ContextMenuStrip();

            var miSection = new ToolStripMenuItem("Khu/Dãy (BranchSections)");
            miSection.Click += (s, e) =>
            {
                using (var frm = new FrmBranchSectionManager())
                    frm.ShowDialog(this);
            };

            var miRoomType = new ToolStripMenuItem("Loại phòng (RoomTypes)");
            miRoomType.Click += (s, e) =>
            {
                using (var frm = new FrmRoomTypeManager())
                    frm.ShowDialog(this);
            };

            var miStatus = new ToolStripMenuItem("Trạng thái phòng (RoomStatuses)");
            miStatus.Click += (s, e) =>
            {
                using (var frm = new FrmRoomStatusManager())
                    frm.ShowDialog(this);
            };

            menu.Items.Add(miSection);
            menu.Items.Add(miRoomType);
            menu.Items.Add(miStatus);
            return menu;
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                await LoadLookupsAsync();
                _rawTable = await _bll.GetRoomsAsync();
                _grid.DataSource = _rawTable;
                ApplyGridPresentation();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadLookupsAsync()
        {
            _branchTable = await _bll.GetBranchesAsync();
            _statusTable = await _bll.GetRoomStatusesAsync();

            var branches = _branchTable?.Copy();
            if (branches != null && !branches.Columns.Contains("BranchDisplay"))
                branches.Columns.Add("BranchDisplay", typeof(string));
            if (branches != null)
            {
                foreach (DataRow r in branches.Rows)
                {
                    string code = branches.Columns.Contains("BranchCode") ? r["BranchCode"]?.ToString() : null;
                    string name = branches.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : null;
                    string id = branches.Columns.Contains("BranchId") ? r["BranchId"]?.ToString() : null;
                    r["BranchDisplay"] = $"{code} - {name}".Trim(' ', '-');
                    if (string.IsNullOrWhiteSpace(r["BranchDisplay"]?.ToString()))
                        r["BranchDisplay"] = "Chi nhánh " + id;
                }
            }

            var branchSelect = new DataTable();
            branchSelect.Columns.Add("BranchId", typeof(int));
            branchSelect.Columns.Add("BranchDisplay", typeof(string));
            branchSelect.Rows.Add(0, "Tất cả");
            if (branches != null && branches.Columns.Contains("BranchId"))
            {
                foreach (DataRow r in branches.Rows)
                {
                    if (!int.TryParse(r["BranchId"]?.ToString(), out var bid)) continue;
                    branchSelect.Rows.Add(bid, r["BranchDisplay"]?.ToString());
                }
            }
            _cboBranch.DataSource = branchSelect;
            _cboBranch.DisplayMember = "BranchDisplay";
            _cboBranch.ValueMember = "BranchId";

            var statusSelect = new DataTable();
            statusSelect.Columns.Add("StatusId", typeof(int));
            statusSelect.Columns.Add("StatusName", typeof(string));
            statusSelect.Rows.Add(0, "Tất cả");
            if (_statusTable != null && _statusTable.Columns.Contains("StatusId"))
            {
                foreach (DataRow r in _statusTable.Rows)
                {
                    if (!int.TryParse(r["StatusId"]?.ToString(), out var sid)) continue;
                    statusSelect.Rows.Add(sid, r["StatusName"]?.ToString());
                }
            }
            _cboStatus.DataSource = statusSelect;
            _cboStatus.DisplayMember = "StatusName";
            _cboStatus.ValueMember = "StatusId";
        }

        private void ApplyGridPresentation()
        {
            SetHeader("RoomId", "ID");
            SetHeader("RoomNumber", "Số phòng");
            SetHeader("BranchName", "Chi nhánh");
            SetHeader("SectionName", "Khu/Dãy");
            SetHeader("RoomTypeName", "Loại phòng");
            SetHeader("RoomPrice", "Giá phòng");
            SetHeader("StatusName", "Trạng thái");
            SetHeader("IsActive", "Kích hoạt");
            SetHeader("CreatedDate", "Tạo lúc");
            SetHeader("UpdatedDate", "Cập nhật");

            HideIfExists("BranchId");
            HideIfExists("SectionId");
            HideIfExists("RoomTypeId");
            HideIfExists("CurrentStatusId");
            HideIfExists("Floor");
            HideIfExists("Area");

            FormatMoney("RoomPrice");
            FormatDateTime("CreatedDate");
            FormatDateTime("UpdatedDate");

            SetDisplayOrder(
                "RoomId",
                "RoomNumber",
                "BranchName",
                "SectionName",
                "RoomTypeName",
                "RoomPrice",
                "StatusName",
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
            int statusId = _cboStatus.SelectedValue is int s ? s : 0;
            int activeChoice = _cboActive.SelectedIndex; // 0 all, 1 active, 2 inactive

            var rows = _rawTable.AsEnumerable();

            if (branchId > 0 && _rawTable.Columns.Contains("BranchId"))
                rows = rows.Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == branchId);

            if (statusId > 0 && _rawTable.Columns.Contains("CurrentStatusId"))
                rows = rows.Where(r => int.TryParse(r["CurrentStatusId"]?.ToString(), out var sid) && sid == statusId);

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
                    Contains(r, "RoomNumber", keyword) ||
                    Contains(r, "BranchName", keyword) ||
                    Contains(r, "SectionName", keyword) ||
                    Contains(r, "RoomTypeName", keyword) ||
                    Contains(r, "StatusName", keyword) ||
                    Contains(r, "RoomId", keyword));
            }

            var filtered = _rawTable.Clone();
            foreach (var r in rows)
                filtered.ImportRow(r);

            _grid.DataSource = filtered;
            ApplyGridPresentation();

            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
            UpdateSummary(filtered);
        }

        private void UpdateSummary(DataTable table)
        {
            int vacant = 0, occupied = 0, maintenance = 0;
            foreach (DataRow r in table.Rows)
            {
                string statusName = table.Columns.Contains("StatusName") ? r["StatusName"]?.ToString() : null;
                int statusId = table.Columns.Contains("CurrentStatusId") && int.TryParse(r["CurrentStatusId"]?.ToString(), out var sid) ? sid : 0;

                if (!string.IsNullOrWhiteSpace(statusName))
                {
                    var s = statusName.ToLowerInvariant();
                    if (s.Contains("trống") || s.Contains("trong") || s.Contains("vacant")) vacant++;
                    else if (s.Contains("bảo trì") || s.Contains("bao tri") || s.Contains("maint")) maintenance++;
                    else occupied++;
                }
                else
                {
                    if (statusId == 1) vacant++;
                    else if (statusId == 4) maintenance++;
                    else if (statusId > 0) occupied++;
                }
            }

            _lblSummary.Text = $"Trống: {vacant:N0} | Đang ở: {occupied:N0} | Bảo trì: {maintenance:N0}";
        }

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            if (_grid.CurrentRow.DataBoundItem is DataRowView drv) return drv.Row;
            return null;
        }

        private async System.Threading.Tasks.Task AddNewAsync()
        {
            using (var frm = new FrmRoomEditor(_bll))
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

            using (var frm = new FrmRoomEditor(_bll, row))
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

            int id = ReadInt(row, "RoomId", "RoomID", "Id");
            string number = ReadString(row, "RoomNumber") ?? id.ToString();
            if (id <= 0)
            {
                MessageBox.Show("Không xác định được RoomId để xóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Xóa/Vô hiệu hóa phòng \"{number}\" (ID {id})?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteRoomAsync(id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task ToggleActiveAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một phòng để bật/tắt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = ReadInt(row, "RoomId");
            if (id <= 0)
            {
                MessageBox.Show("Không xác định được RoomId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool isActive = TryReadBool(row, "IsActive") ?? true;
            bool next = !isActive;

            string roomNumber = ReadString(row, "RoomNumber") ?? id.ToString();
            if (MessageBox.Show($"Chuyển phòng \"{roomNumber}\" sang {(next ? "Đang hoạt động" : "Đã tắt")}?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await UpdateRoomFromRowAsync(row, isActive: next);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task ChangeStatusAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một phòng để đổi trạng thái.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = ReadInt(row, "RoomId");
            if (id <= 0)
            {
                MessageBox.Show("Không xác định được RoomId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dlg = new StatusPickerDialog(_statusTable);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            int newStatusId = dlg.SelectedStatusId;
            if (newStatusId <= 0) return;

            try
            {
                await UpdateRoomFromRowAsync(row, statusId: newStatusId);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi trạng thái: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task UpdateRoomFromRowAsync(DataRow row, int? statusId = null, bool? isActive = null)
        {
            int id = ReadInt(row, "RoomId");
            string roomNumber = ReadString(row, "RoomNumber");
            int branchId = ReadInt(row, "BranchId");

            int? sectionId = TryReadIntNullable(row, "SectionId");
            int? roomTypeId = TryReadIntNullable(row, "RoomTypeId");
            int? currentStatusId = statusId ?? TryReadIntNullable(row, "CurrentStatusId");
            int? floor = TryReadIntNullable(row, "Floor");
            decimal? area = TryReadDecimalNullable(row, "Area");
            decimal? price = TryReadDecimalNullable(row, "RoomPrice");
            bool? active = isActive ?? TryReadBool(row, "IsActive");

            await _bll.UpdateRoomAsync(id, roomNumber, branchId, sectionId, roomTypeId, price, currentStatusId, floor, area, active);
        }

        private static void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var col = grid.Columns[e.ColumnIndex];
            if (col == null) return;

            if (col.Name == "StatusName" && e.Value != null)
            {
                string s = e.Value.ToString().ToLowerInvariant();
                if (s.Contains("trống") || s.Contains("trong") || s.Contains("vacant"))
                    e.CellStyle.ForeColor = Color.FromArgb(46, 125, 50);
                else if (s.Contains("bảo trì") || s.Contains("bao tri") || s.Contains("maint"))
                    e.CellStyle.ForeColor = Color.FromArgb(156, 39, 176);
                else if (s.Contains("cọc") || s.Contains("coc"))
                    e.CellStyle.ForeColor = Color.FromArgb(255, 152, 0);
                else
                    e.CellStyle.ForeColor = Color.FromArgb(33, 150, 243);
            }

            if (col.Name == "IsActive" && e.Value != null)
            {
                try
                {
                    bool act = Convert.ToBoolean(e.Value);
                    if (!act)
                        e.CellStyle.ForeColor = Color.FromArgb(211, 47, 47);
                }
                catch { }
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

        private class StatusPickerDialog : Form
        {
            private ComboBox _cbo;
            private Button _btnOk;
            private Button _btnCancel;
            public int SelectedStatusId { get; private set; }

            public StatusPickerDialog(DataTable statusTable)
            {
                InitializeComponent();

                var src = new DataTable();
                src.Columns.Add("StatusId", typeof(int));
                src.Columns.Add("StatusName", typeof(string));
                if (statusTable != null && statusTable.Columns.Contains("StatusId"))
                {
                    foreach (DataRow r in statusTable.Rows)
                    {
                        if (!int.TryParse(r["StatusId"]?.ToString(), out var id)) continue;
                        src.Rows.Add(id, r["StatusName"]?.ToString());
                    }
                }

                _cbo.DataSource = src;
                _cbo.DisplayMember = "StatusName";
                _cbo.ValueMember = "StatusId";
            }

            private void InitializeComponent()
            {
                Text = "Đổi trạng thái phòng";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(520, 150);
                BackColor = Color.White;

                var lbl = new Label { Text = "Chọn trạng thái:", AutoSize = true, Location = new Point(18, 22) };
                _cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 470, Location = new Point(18, 48) };

                _btnOk = new Button { Text = "Cập nhật", Width = 110, Height = 32, Location = new Point(280, 96) };
                _btnCancel = new Button { Text = "Hủy", Width = 90, Height = 32, Location = new Point(400, 96) };

                _btnOk.FlatStyle = FlatStyle.Flat;
                _btnOk.FlatAppearance.BorderSize = 0;
                _btnOk.BackColor = Color.FromArgb(0, 122, 204);
                _btnOk.ForeColor = Color.White;
                _btnCancel.FlatStyle = FlatStyle.Flat;
                _btnCancel.FlatAppearance.BorderSize = 1;

                _btnOk.Click += (s, e) =>
                {
                    if (_cbo.SelectedValue is int id && id > 0)
                        SelectedStatusId = id;
                    DialogResult = SelectedStatusId > 0 ? DialogResult.OK : DialogResult.Cancel;
                };
                _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

                Controls.Add(lbl);
                Controls.Add(_cbo);
                Controls.Add(_btnOk);
                Controls.Add(_btnCancel);

                AcceptButton = _btnOk;
                CancelButton = _btnCancel;
            }
        }
    }
}
