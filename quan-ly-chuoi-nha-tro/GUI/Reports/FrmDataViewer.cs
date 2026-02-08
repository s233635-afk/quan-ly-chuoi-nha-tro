using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Form dùng chung để xem dữ liệu nhanh cho các module Admin (tạm thời).
    /// </summary>
    public class FrmDataViewer : Form
    {
        private readonly Func<Task<DataTable>> _loadFunc;
        private readonly Func<DataRow, Task> _onRowDoubleClick;
        private readonly string _title;

        private DataTable _currentData;

        private DataGridView dgv;
        private Button btnRefresh;
        private Label lblCount;

        public FrmDataViewer(string title, Func<Task<DataTable>> loadFunc, Func<DataRow, Task> onRowDoubleClick = null)
        {
            _title = title;
            _loadFunc = loadFunc;
            _onRowDoubleClick = onRowDoubleClick;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = _title;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 1000;
            this.Height = 600;
            this.BackColor = UiKit.AppBackground;

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            UiKit.StyleGrid(dgv);

            btnRefresh = UiKit.MakeButton("Tải lại", UiKit.Primary, async (s, e) => await LoadDataAsync(), 92);

            lblCount = new Label
            {
                Text = "Tổng: 0",
                AutoSize = true,
                Dock = DockStyle.Left
            };

            Panel top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White
            };
            top.Controls.Add(lblCount);
            top.Controls.Add(btnRefresh);
            btnRefresh.Dock = DockStyle.Right;

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BackColor };
            gridHost.Controls.Add(dgv);

            this.Controls.Add(gridHost);
            this.Controls.Add(top);
            this.Load += async (s, e) => await LoadDataAsync();
            dgv.CellDoubleClick += async (s, e) => await HandleDoubleClickAsync(e.RowIndex);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var data = await _loadFunc.Invoke();
                _currentData = data ?? new DataTable();
                dgv.DataSource = _currentData;
                lblCount.Text = $"Tổng: {_currentData.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public Task ReloadAsync() => LoadDataAsync();

        private async Task HandleDoubleClickAsync(int rowIndex)
        {
            if (_onRowDoubleClick == null) return;
            if (rowIndex < 0 || rowIndex >= dgv.Rows.Count) return;

            DataRow row = null;
            if (dgv.Rows[rowIndex].DataBoundItem is DataRowView drv)
                row = drv.Row;
            else if (_currentData != null && rowIndex < _currentData.Rows.Count)
                row = _currentData.Rows[rowIndex];

            if (row == null) return;

            try
            {
                await _onRowDoubleClick(row);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xem chi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
