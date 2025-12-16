using System;
using System.Data;
using System.Drawing;
using System.Linq;
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
                HeaderText = "Mã chi nhánh",
                DataPropertyName = "BranchCode",
                Width = 100
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BranchName",
                HeaderText = "Tên chi nhánh",
                DataPropertyName = "BranchName",
                Width = 200
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Address",
                HeaderText = "Địa chỉ",
                DataPropertyName = "Address",
                Width = 250
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Phone",
                HeaderText = "Điện thoại",
                DataPropertyName = "Phone",
                Width = 120
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Hotline",
                HeaderText = "Hotline",
                DataPropertyName = "Hotline",
                Width = 120
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OperatingHours",
                HeaderText = "Giờ hoạt động",
                DataPropertyName = "OperatingHours",
                Width = 140
            });
            dataGridViewBranches.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IsActive",
                HeaderText = "Trạng thái",
                DataPropertyName = "IsActive",
                Width = 80
            });

            dataGridViewBranches.CellFormatting += DataGridViewBranches_CellFormatting;
        }

        private async void LoadBranches()
        {
            try
            {
                var all = await branchBLL.GetAllBranchesAsync();
                TextFixer.FixDataTable(all, "BranchName", "Address", "Description");
                dtBranches = AdminBranchScope.Apply(all);
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
                AdminEvents.NotifyDataChanged();
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
                AdminEvents.NotifyDataChanged();
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
                    AdminEvents.NotifyDataChanged();
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

            string OrLike(string col)
            {
                if (dtBranches.Columns.Contains(col))
                    return $"{col} LIKE '%{keyword}%'";
                return null;
            }

            var parts = new[]
            {
                OrLike("BranchName"),
                OrLike("BranchCode"),
                OrLike("Address"),
                OrLike("Phone"),
                OrLike("Hotline"),
                OrLike("OperatingHours"),
                OrLike("Description")
            };

            string filter = string.Join(" OR ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            dv.RowFilter = string.IsNullOrWhiteSpace(filter) ? "1=1" : filter;
            dataGridViewBranches.DataSource = dv;
        }

        private void DataGridViewBranches_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewBranches.Columns[e.ColumnIndex].Name == "IsActive" && e.Value != null && e.Value != DBNull.Value)
            {
                bool isActive = TryGetBool(e.Value, out bool b) && b;
                e.Value = isActive ? "Hoạt động" : "Vô hiệu";
                e.CellStyle.ForeColor = isActive ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
                e.CellStyle.BackColor = isActive ? Color.FromArgb(232, 247, 239) : Color.FromArgb(252, 236, 238);
                e.CellStyle.SelectionBackColor = isActive ? Color.FromArgb(214, 237, 223) : Color.FromArgb(244, 214, 220);
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
            int active = dtBranches.Rows.Cast<DataRow>().Count(r => TryGetBool(r["IsActive"], out bool b) && b);
            int inactive = Math.Max(0, total - active);

            lblTotalBranches.Text = $"Tổng: {total} chi nhánh";
            lblStatTotalValue.Text = total.ToString();
            lblStatActiveValue.Text = active.ToString();
            lblStatInactiveValue.Text = inactive.ToString();
        }

        private static bool TryGetBool(object value, out bool result)
        {
            result = false;
            if (value == null || value == DBNull.Value) return false;

            if (value is bool b)
            {
                result = b;
                return true;
            }
            if (value is byte by)
            {
                result = by != 0;
                return true;
            }
            if (value is short sh)
            {
                result = sh != 0;
                return true;
            }
            if (value is int i)
            {
                result = i != 0;
                return true;
            }
            if (value is long l)
            {
                result = l != 0;
                return true;
            }

            string s = value.ToString();
            if (bool.TryParse(s, out bool parsedBool))
            {
                result = parsedBool;
                return true;
            }
            if (int.TryParse(s, out int parsedInt))
            {
                result = parsedInt != 0;
                return true;
            }
            return false;
        }
    }
}
