using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmDepositManager : Form
    {
        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private DataTable _table;
        private DataGridView _grid;
        private TextBox _txtSearch;
        private Label _lblCount;
        private Button _btnAdd, _btnEdit, _btnDelete, _btnRefresh;

        public FrmDepositManager()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Đặt Phòng & Cọc";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 1200;
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

            _txtSearch = new TextBox { Width = 260 };
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
            _btnAdd.Location = new Point(10, 10);
            _btnEdit.Location = new Point(100, 10);
            _btnDelete.Location = new Point(190, 10);
            _btnRefresh.Location = new Point(280, 10);
            _txtSearch.Location = new Point(380, 10);
            _lblCount.Location = new Point(660, 14);

            top.Controls.Add(_btnAdd);
            top.Controls.Add(_btnEdit);
            top.Controls.Add(_btnDelete);
            top.Controls.Add(_btnRefresh);
            top.Controls.Add(_txtSearch);
            top.Controls.Add(_lblCount);

            this.Controls.Add(_grid);
            this.Controls.Add(top);
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _table = await _bll.GetDepositsAsync();
                _grid.DataSource = _table;
                _lblCount.Text = $"Tổng: {_table.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải đặt phòng/cọc: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    $"Convert(TenantId, 'System.String') LIKE '%{keyword}%' OR Convert(RoomId, 'System.String') LIKE '%{keyword}%' OR Status LIKE '%{keyword}%'";
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
            using (var frm = new FrmDepositEditor(_bll))
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

            using (var frm = new FrmDepositEditor(_bll, row))
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

            int id = Convert.ToInt32(row["DepositId"]);
            if (MessageBox.Show($"Xóa đặt phòng/cọc ID {id}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    await _bll.DeleteDepositAsync(id);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
