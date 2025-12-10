using System;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmBranchDetail : Form
    {
        private BranchBLL branchBLL = new BranchBLL();
        private int branchId = 0;

        public FrmBranchDetail()
        {
            InitializeComponent();
            this.branchId = 0;
        }

        public FrmBranchDetail(int id)
        {
            InitializeComponent();
            this.branchId = id;
        }

        private async void FrmBranchDetail_Load(object sender, EventArgs e)
        {
            if (branchId > 0)
            {
                this.Text = "Sửa Chi Nhánh";
                await LoadBranchData();
            }
            else
            {
                this.Text = "Thêm Chi Nhánh Mới";
                btnDelete.Visible = false;
            }
        }

        private async System.Threading.Tasks.Task LoadBranchData()
        {
            try
            {
                var dt = await branchBLL.GetBranchByIdAsync(branchId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    txtCode.Text = row["BranchCode"].ToString();
                    txtName.Text = row["BranchName"].ToString();
                    txtAddress.Text = row["Address"].ToString();
                    txtPhone.Text = row["Phone"].ToString();
                    txtManager.Text = row["ManagerName"] == DBNull.Value ? string.Empty : row["ManagerName"].ToString();
                    chkActive.Checked = row["IsActive"] != DBNull.Value && Convert.ToInt32(row["IsActive"]) == 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                if (branchId > 0)
                {
                    // Update
                    await branchBLL.UpdateBranchAsync(
                        branchId,
                        txtCode.Text.Trim(),
                        txtName.Text.Trim(),
                        txtAddress.Text.Trim(),
                        txtPhone.Text.Trim(),
                        txtManager.Text.Trim(),
                        chkActive.Checked
                    );
                    MessageBox.Show("Cập nhật chi nhánh thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Insert
                    await branchBLL.AddBranchAsync(
                        txtCode.Text.Trim(),
                        txtName.Text.Trim(),
                        txtAddress.Text.Trim(),
                        txtPhone.Text.Trim(),
                        txtManager.Text.Trim(),
                        chkActive.Checked
                    );
                    MessageBox.Show("Thêm chi nhánh mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (branchId <= 0)
                return;

            if (MessageBox.Show("Bạn chắc chắn muốn xóa chi nhánh này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            try
            {
                await branchBLL.DeleteBranchAsync(branchId);
                MessageBox.Show("Xóa chi nhánh thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("Mã chi nhánh không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Tên chi nhánh không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Địa chỉ không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAddress.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Số điện thoại không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }

            if (txtCode.Text.Length > 20)
            {
                MessageBox.Show("Mã chi nhánh không được vượt quá 20 ký tự!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (txtName.Text.Length > 100)
            {
                MessageBox.Show("Tên chi nhánh không được vượt quá 100 ký tự!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
