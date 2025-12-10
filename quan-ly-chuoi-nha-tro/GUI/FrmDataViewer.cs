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
        private readonly string _title;

        private DataGridView dgv;
        private Button btnRefresh;
        private Label lblCount;

        public FrmDataViewer(string title, Func<Task<DataTable>> loadFunc)
        {
            _title = title;
            _loadFunc = loadFunc;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = _title;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 1000;
            this.Height = 600;

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
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 252);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            btnRefresh = new Button
            {
                Text = "Tải lại",
                Width = 80,
                Height = 28,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            lblCount = new Label
            {
                Text = "Tổng: 0",
                AutoSize = true,
                Dock = DockStyle.Left
            };

            Panel top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38
            };
            top.Controls.Add(lblCount);
            top.Controls.Add(btnRefresh);
            btnRefresh.Location = new System.Drawing.Point(900, 6);

            this.Controls.Add(dgv);
            this.Controls.Add(top);
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var data = await _loadFunc.Invoke();
                dgv.DataSource = data;
                lblCount.Text = $"Tổng: {data.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
