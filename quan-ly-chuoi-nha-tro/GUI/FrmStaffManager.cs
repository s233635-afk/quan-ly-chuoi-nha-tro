using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Quản lý nhân viên: xem danh sách, thêm/sửa/xóa tài khoản (role = staff/manager), không tạo admin.
    /// </summary>
    public class FrmStaffManager : Form
    {
        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private DataTable _table;
        private DataGridView _grid;
        private TextBox _txtSearch;
        private Label _lblCount;
        private Button _btnAdd, _btnEdit, _btnDelete, _btnRefresh;

        public FrmStaffManager()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Nhân Viên";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 1100;
            this.Height = 650;

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false
            };
            _grid.DoubleClick += (s, e) => EditSelected();

            _txtSearch = new TextBox { Width = 240 };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0" };

            _btnAdd = new Button { Text = "Thêm", Width = 80 };
            _btnEdit = new Button { Text = "Sửa", Width = 80 };
            _btnDelete = new Button { Text = "Xóa", Width = 80 };
            _btnRefresh = new Button { Text = "Tải lại", Width = 80 };

            _btnAdd.Click += (s, e) => AddNew();
            _btnEdit.Click += (s, e) => EditSelected();
            _btnDelete.Click += async (s, e) => await DeleteSelectedAsync();
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();

            var top = new Panel { Dock = DockStyle.Top, Height = 44 };
            top.Controls.Add(_btnAdd);
            top.Controls.Add(_btnEdit);
            top.Controls.Add(_btnDelete);
            top.Controls.Add(_btnRefresh);
            top.Controls.Add(_txtSearch);
            top.Controls.Add(_lblCount);

            // Position controls
            _btnAdd.Location = new Point(10, 10);
            _btnEdit.Location = new Point(100, 10);
            _btnDelete.Location = new Point(190, 10);
            _btnRefresh.Location = new Point(280, 10);
            _txtSearch.Location = new Point(380, 10);
            _lblCount.Location = new Point(640, 14);

            this.Controls.Add(_grid);
            this.Controls.Add(top);
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _table = await _bll.GetStaffAsync();
                _grid.DataSource = _table;
                _lblCount.Text = $"Tổng: {_table.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            if (_table == null) return;
            string keyword = _txtSearch.Text.Trim().Replace("'", "''");
            if (string.IsNullOrEmpty(keyword))
            {
                _table.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                _table.DefaultView.RowFilter =
                    $"UserName LIKE '%{keyword}%' OR FullName LIKE '%{keyword}%' OR Phone LIKE '%{keyword}%' OR Email LIKE '%{keyword}%'";
            }
            _lblCount.Text = $"Tổng: {_table.DefaultView.Count}";
        }

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            var drv = _grid.CurrentRow.DataBoundItem as DataRowView;
            return drv?.Row;
        }

        private void AddNew()
        {
            using (var frm = new FrmStaffEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadDataAsync();
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

            using (var frm = new FrmStaffEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadDataAsync();
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

            int id = Convert.ToInt32(row["UserId"]);
            string name = row["FullName"]?.ToString();
            if (MessageBox.Show($"Xóa nhân viên \"{name}\" (ID {id})?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
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
        }
    }
}
