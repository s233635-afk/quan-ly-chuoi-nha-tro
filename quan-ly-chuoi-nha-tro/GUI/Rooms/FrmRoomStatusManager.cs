using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmRoomStatusManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo tên/mô tả...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private DataTable _raw;

        private DataGridView _grid;
        private TextBox _txtSearch;
        private Label _lblCount;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnRefresh;

        public FrmRoomStatusManager()
        {
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            Text = "Quản lý Trạng thái phòng";
            StartPosition = FormStartPosition.CenterParent;
            Width = 900;
            Height = 600;
            BackColor = UiKit.AppBackground;

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

            _txtSearch = new TextBox { Width = 280 };
            var pnlSearch = UiKit.MakeSearchPanel(_txtSearch, 360, SearchPlaceholder, ApplyFilter);

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
            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, Location = new Point(0, 9), ForeColor = UiKit.MutedText };
            pnlSearch.Location = new Point(lblSearch.Right + 6, 10);
            filterHost.Controls.Add(lblSearch);
            filterHost.Controls.Add(pnlSearch);

            filterHost.Resize += (s, e) => pnlSearch.Location = new Point(lblSearch.Right + 6, 10);

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
                _raw = await _bll.GetRoomStatusesAsync();
                _grid.DataSource = _raw;
                ApplyGridPresentation();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải trạng thái: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void ApplyGridPresentation()
        {
            if (_grid.Columns.Contains("StatusId")) _grid.Columns["StatusId"].HeaderText = "ID";
            if (_grid.Columns.Contains("StatusName")) _grid.Columns["StatusName"].HeaderText = "Tên";
            if (_grid.Columns.Contains("Description")) _grid.Columns["Description"].HeaderText = "Mô tả";
            if (_grid.Columns.Contains("StatusId")) _grid.Columns["StatusId"].DisplayIndex = 0;
            if (_grid.Columns.Contains("StatusName")) _grid.Columns["StatusName"].DisplayIndex = 1;
            if (_grid.Columns.Contains("Description")) _grid.Columns["Description"].DisplayIndex = 2;
        }

        private void ApplyFilter()
        {
            if (_raw == null) return;

            string rawKeyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholder) rawKeyword = string.Empty;
            string keyword = rawKeyword.ToLowerInvariant();

            var rows = _raw.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rows = rows.Where(r =>
                    Contains(r, "StatusName", keyword) ||
                    Contains(r, "Description", keyword) ||
                    Contains(r, "StatusId", keyword));
            }

            var filtered = _raw.Clone();
            foreach (var r in rows) filtered.ImportRow(r);
            _grid.DataSource = filtered;
            ApplyGridPresentation();
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
        }

        private void AddNew()
        {
            using (var frm = new FrmRoomStatusEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    _ = LoadDataAsync();
            }
        }

        private void EditSelected()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một trạng thái để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmRoomStatusEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    _ = LoadDataAsync();
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một trạng thái để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = row.Table.Columns.Contains("StatusId") ? Convert.ToInt32(row["StatusId"]) : 0;
            if (id <= 0) return;

            if (MessageBox.Show("Bạn chắc chắn muốn xóa trạng thái này?\n(Lưu ý: nếu đang được dùng bởi Phòng thì sẽ không xóa được)", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteRoomStatusAsync(id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa trạng thái: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}

