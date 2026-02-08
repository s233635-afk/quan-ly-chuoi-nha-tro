using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL
{
    public class BranchBLL
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả chi nhánh
        /// </summary>
        public async Task<DataTable> GetAllBranchesAsync()
        {
            try
            {
                return await dbHelper.GetAllBranchesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy danh sách chi nhánh: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy chi tiết 1 chi nhánh
        /// </summary>
        public async Task<DataTable> GetBranchByIdAsync(int branchId)
        {
            if (branchId <= 0)
                throw new Exception("ID Chi nhánh không hợp lệ!");

            try
            {
                return await dbHelper.GetBranchByIdAsync(branchId);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy chi tiết chi nhánh: " + ex.Message);
            }
        }

        /// <summary>
        /// Thêm chi nhánh mới
        /// </summary>
        public async Task<bool> AddBranchAsync(
            string branchCode,
            string branchName,
            string address,
            string phone,
            string hotline,
            string operatingHours,
            string description,
            bool isActive)
        {
            // Kiểm tra dữ liệu
            if (string.IsNullOrWhiteSpace(branchCode) || string.IsNullOrWhiteSpace(branchName))
                throw new Exception("Vui lòng nhập Mã chi nhánh và Tên chi nhánh!");

            if (branchCode.Length < 2)
                throw new Exception("Mã chi nhánh phải từ 2 ký tự trở lên!");

            try
            {
                return await dbHelper.AddBranchAsync(branchCode, branchName, address, phone, hotline, operatingHours, description, isActive);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("2601"))
                    throw new Exception("Mã chi nhánh đã tồn tại rồi!");
                throw new Exception("Lỗi thêm chi nhánh: " + ex.Message);
            }
        }

        /// <summary>
        /// Sửa thông tin chi nhánh
        /// </summary>
        public async Task<bool> UpdateBranchAsync(
            int branchId,
            string branchCode,
            string branchName,
            string address,
            string phone,
            string hotline,
            string operatingHours,
            string description,
            bool isActive)
        {
            if (branchId <= 0)
                throw new Exception("ID Chi nhánh không hợp lệ!");

            if (string.IsNullOrWhiteSpace(branchName))
                throw new Exception("Vui lòng nhập Tên chi nhánh!");

            try
            {
                return await dbHelper.UpdateBranchAsync(branchId, branchCode, branchName, address, phone, hotline, operatingHours, description, isActive);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi cập nhật chi nhánh: " + ex.Message);
            }
        }

        /// <summary>
        /// Xóa chi nhánh
        /// </summary>
        public async Task<bool> DeleteBranchAsync(int branchId)
        {
            if (branchId <= 0)
                throw new Exception("ID Chi nhánh không hợp lệ!");

            try
            {
                return await dbHelper.DeleteBranchAsync(branchId);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                    throw new Exception("Chi nhánh này có dữ liệu liên quan, không thể xóa!");
                throw new Exception("Lỗi xóa chi nhánh: " + ex.Message);
            }
        }

        /// <summary>
        /// Kích hoạt/Vô hiệu hóa chi nhánh
        /// </summary>
        public async Task<bool> ToggleBranchStatusAsync(int branchId, bool isActive)
        {
            if (branchId <= 0)
                throw new Exception("ID Chi nhánh không hợp lệ!");

            try
            {
                return await dbHelper.ToggleBranchStatusAsync(branchId, isActive);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi cập nhật trạng thái chi nhánh: " + ex.Message);
            }
        }
    }
}
