using System;
using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL.Assets
{
    /// <summary>
    /// Business Logic Layer cho quản lý Tài sản
    /// </summary>
    public class AssetBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        /// <summary>
        /// Lấy danh sách tất cả tài sản
        /// </summary>
        public Task<DataTable> GetAllAsync() => dbHelper.GetAssetsAsync();

        /// <summary>
        /// Thêm tài sản mới
        /// </summary>
        public Task<int> AddAsync(
            string assetCode, 
            string assetName, 
            string category, 
            int? roomId, 
            int quantity, 
            string condition,
            DateTime? purchaseDate, 
            decimal? purchasePrice, 
            string description, 
            bool isActive)
        {
            if (string.IsNullOrWhiteSpace(assetCode))
                throw new Exception("Mã tài sản không được để trống.");
            if (string.IsNullOrWhiteSpace(assetName))
                throw new Exception("Tên tài sản không được để trống.");
            if (quantity <= 0)
                throw new Exception("Số lượng phải lớn hơn 0.");

            return dbHelper.AddAssetAsync(
                assetCode.Trim(), 
                assetName.Trim(), 
                category, 
                roomId, 
                quantity, 
                condition,
                purchaseDate, 
                purchasePrice, 
                description, 
                isActive);
        }

        /// <summary>
        /// Cập nhật tài sản
        /// </summary>
        public Task<bool> UpdateAsync(
            int assetId, 
            string assetCode, 
            string assetName, 
            string category, 
            int? roomId, 
            int quantity, 
            string condition,
            DateTime? purchaseDate, 
            decimal? purchasePrice, 
            string description, 
            bool isActive)
        {
            if (assetId <= 0)
                throw new Exception("ID tài sản không hợp lệ.");
            if (string.IsNullOrWhiteSpace(assetCode))
                throw new Exception("Mã tài sản không được để trống.");
            if (string.IsNullOrWhiteSpace(assetName))
                throw new Exception("Tên tài sản không được để trống.");
            if (quantity <= 0)
                throw new Exception("Số lượng phải lớn hơn 0.");

            return dbHelper.UpdateAssetAsync(
                assetId, 
                assetCode.Trim(), 
                assetName.Trim(), 
                category, 
                roomId, 
                quantity, 
                condition,
                purchaseDate, 
                purchasePrice, 
                description, 
                isActive);
        }

        /// <summary>
        /// Xóa tài sản
        /// </summary>
        public Task<bool> DeleteAsync(int assetId)
        {
            if (assetId <= 0)
                throw new Exception("ID tài sản không hợp lệ.");

            return dbHelper.DeleteAssetAsync(assetId);
        }
    }
}
