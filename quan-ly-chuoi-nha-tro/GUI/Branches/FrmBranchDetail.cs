using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmBranchDetail : Form
    {
        private readonly BranchBLL _bll = new BranchBLL();
        private readonly int _branchId;
        private readonly ValidationHelper _validator = new ValidationHelper();
        private readonly LoadingOverlay _loadingOverlay = new LoadingOverlay();

        public FrmBranchDetail() : this(0) { }

        public FrmBranchDetail(int id)
        {
            _branchId = id;
            InitializeComponent();
            SetupValidation();
            Controls.Add(_loadingOverlay);
        }

        private void SetupValidation()
        {
            _validator
                .Required(txtCode, "Mã chi nhánh là bắt buộc")
                .MaxLength(txtCode, 20)
                .Required(txtName, "Tên chi nhánh là bắt buộc")
                .MaxLength(txtName, 255)
                .MaxLength(txtPhone, 20)
                .MaxLength(txtHotline, 20)
                .MaxLength(txtHours, 100)
                .MaxLength(txtDescription, 500);
        }

        private async void FrmBranchDetail_Load(object sender, EventArgs e)
        {
            if (_branchId > 0)
            {
                Text = "Sửa chi nhánh";
                await LoadBranchDataAsync();
            }
            else
            {
                Text = "Thêm chi nhánh mới";
                btnDelete.Visible = false;
            }
        }

        private async System.Threading.Tasks.Task LoadBranchDataAsync()
        {
            try
            {
                var dt = await _loadingOverlay.ExecuteWithLoadingAsync(
                    () => _bll.GetBranchByIdAsync(_branchId),
                    "Đang tải dữ liệu...");

                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    txtCode.Text = row["BranchCode"].ToString();
                    txtName.Text = TextFixer.FixUtf8Mojibake(row["BranchName"].ToString());
                    txtAddress.Text = TextFixer.FixUtf8Mojibake(row["Address"].ToString());
                    txtPhone.Text = row["Phone"].ToString();
                    txtHotline.Text = GetValue(row, "Hotline");
                    txtHours.Text = GetValue(row, "OperatingHours");
                    txtDescription.Text = TextFixer.FixUtf8Mojibake(GetValue(row, "Description"));
                    chkActive.Checked = row["IsActive"] != DBNull.Value && Convert.ToInt32(row["IsActive"]) == 1;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "LoadBranch", "Không thể tải thông tin chi nhánh");
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (!_validator.ValidateAll())
                return;

            try
            {
                await _loadingOverlay.ExecuteWithLoadingAsync(async () =>
                {
                    if (_branchId > 0)
                    {
                        await _bll.UpdateBranchAsync(
                            _branchId,
                            txtCode.Text.Trim(),
                            txtName.Text.Trim(),
                            txtAddress.Text.Trim(),
                            txtPhone.Text.Trim(),
                            txtHotline.Text.Trim(),
                            txtHours.Text.Trim(),
                            txtDescription.Text.Trim(),
                            chkActive.Checked);
                    }
                    else
                    {
                        await _bll.AddBranchAsync(
                            txtCode.Text.Trim(),
                            txtName.Text.Trim(),
                            txtAddress.Text.Trim(),
                            txtPhone.Text.Trim(),
                            txtHotline.Text.Trim(),
                            txtHours.Text.Trim(),
                            txtDescription.Text.Trim(),
                            chkActive.Checked);
                    }
                    return true;
                }, "Đang lưu...");

                ToastNotification.Success(_branchId > 0 
                    ? "Cập nhật chi nhánh thành công!" 
                    : "Thêm chi nhánh mới thành công!");

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "SaveBranch", "Không thể lưu chi nhánh");
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (_branchId <= 0) return;

            if (!ModernConfirmDialog.ConfirmDanger($"Xóa chi nhánh \"{txtName.Text}\"?\n\nHành động này không thể hoàn tác."))
                return;

            try
            {
                await _loadingOverlay.ExecuteWithLoadingAsync(
                    () => _bll.DeleteBranchAsync(_branchId),
                    "Đang xóa...");

                ToastNotification.Success("Xóa chi nhánh thành công!");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "DeleteBranch", "Không thể xóa chi nhánh");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private static string GetValue(System.Data.DataRow row, string column)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column)) return string.Empty;
            var v = row[column];
            return v == null || v == DBNull.Value ? string.Empty : v.ToString();
        }
    }
}
