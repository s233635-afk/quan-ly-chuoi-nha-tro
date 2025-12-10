using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmBranch : Form
    {
        private BranchBLL branchBLL = new BranchBLL();
        private DataTable dtBranches;

        public FrmBranch()
        {
            InitializeComponent();
        }

        private void FrmBranch_Load(object sender, EventArgs e)
        {
            InitializeUI();
            LoadBranches();
        }

        private void InitializeUI()
        {
            // Thiết lập DataGridView
            dataGridViewBranches.AutoGenerateColumns = false;
            dataGridViewBranches.Columns.Clear();
            dataGridViewBranches.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewBranches.MultiSelect = false;
            dataGridViewBranches.AllowUserToAddRows = false;
            dataGridViewBranches.RowHeadersVisible = false;
            dataGridViewBranches.BackgroundColor = Color.White;
            dataGridViewBranches.BorderStyle = BorderStyle.None;
            dataGridViewBranches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewBranches.EnableHeadersVisualStyles = false;
            dataGridViewBranches.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            dataGridViewBranches.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewBranches.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridViewBranches.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dataGridViewBranches.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            dataGridViewBranches.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 252);
            dataGridViewBranches.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridViewBranches.RowTemplate.Height = 32;
            dataGridViewBranches.GridColor = Color.FromArgb(235, 240, 245);

            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BranchId",
                HeaderText = "ID",
                DataPropertyName = "BranchId",
                Visible = false
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BranchCode",
                HeaderText = "Mã Chi Nhánh",
                DataPropertyName = "BranchCode",
                Width = 100
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BranchName",
                HeaderText = "Tên Chi Nhánh",
                DataPropertyName = "BranchName",
                Width = 200
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Address",
                HeaderText = "Địa Chỉ",
                DataPropertyName = "Address",
                Width = 250
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Phone",
                HeaderText = "Điện Thoại",
                DataPropertyName = "Phone",
                Width = 120
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ManagerName",
                HeaderText = "Quản Lý",
                DataPropertyName = "ManagerName",
                Width = 160
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IsActive",
                HeaderText = "Trạng Thái",
                DataPropertyName = "IsActive",
                Width = 80
            });

            dataGridViewBranches.CellFormatting += DataGridViewBranches_CellFormatting;
        }

        private async void LoadBranches()
        {
            try
            {
                dtBranches = await branchBLL.GetAllBranchesAsync();
                dataGridViewBranches.DataSource = dtBranches;
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            FrmBranchDetail frm = new FrmBranchDetail();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadBranches();
            }
        }

        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewBranches.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn chi nhánh cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int branchId = (int)dataGridViewBranches.SelectedRows[0].Cells["BranchId"].Value;
            FrmBranchDetail frm = new FrmBranchDetail(branchId);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadBranches();
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewBranches.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn chi nhánh cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn chắc chắn muốn xóa chi nhánh này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            try
            {
                int branchId = (int)dataGridViewBranches.SelectedRows[0].Cells["BranchId"].Value;
                bool result = await branchBLL.DeleteBranchAsync(branchId);
                
                if (result)
                {
                    MessageBox.Show("Xóa chi nhánh thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadBranches();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadBranches();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            if (dtBranches == null) return;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                dataGridViewBranches.DataSource = dtBranches;
                return;
            }

            keyword = keyword.Replace("'", "''");
            DataView dv = new DataView(dtBranches);
            dv.RowFilter = $"BranchName LIKE '%{keyword}%' OR BranchCode LIKE '%{keyword}%' OR ManagerName LIKE '%{keyword}%'";
            dataGridViewBranches.DataSource = dv;
        }

        private void DataGridViewBranches_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewBranches.Columns[e.ColumnIndex].Name == "IsActive" && e.Value != null && e.Value != DBNull.Value)
            {
                int val = 0;
                int.TryParse(e.Value.ToString(), out val);
                e.Value = val == 1 ? "Hoạt động" : "Vô hiệu";
                e.CellStyle.ForeColor = val == 1 ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
                e.CellStyle.BackColor = val == 1 ? Color.FromArgb(232, 247, 239) : Color.FromArgb(252, 236, 238);
                e.CellStyle.SelectionBackColor = val == 1 ? Color.FromArgb(214, 237, 223) : Color.FromArgb(244, 214, 220);
                e.FormattingApplied = true;
            }
        }

        private void UpdateStats()
        {
            if (dtBranches == null)
            {
                lblTotalBranches.Text = "Tổng: 0 chi nhánh";
                lblStatTotalValue.Text = "0";
                lblStatActiveValue.Text = "0";
                lblStatInactiveValue.Text = "0";
                return;
            }

            int total = dtBranches.Rows.Count;
            int active = dtBranches.Select("IsActive = 1").Length;
            int inactive = total - active;

            lblTotalBranches.Text = $"Tổng: {total} chi nhánh";
            lblStatTotalValue.Text = total.ToString();
            lblStatActiveValue.Text = active.ToString();
            lblStatInactiveValue.Text = inactive.ToString();
        }
    }
}
