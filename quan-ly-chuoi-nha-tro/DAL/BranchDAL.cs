using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace QuanLyNhaTro.DAL
{
    public partial class DatabaseHelper
    {
        /// <summary>
        /// Lấy tất cả chi nhánh
        /// </summary>
        public async Task<DataTable> GetAllBranchesAsync()
        {
            DataTable dt = new DataTable();

            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    bool hasManager = await BranchHasManagerNameAsync(conn);
                    string managerSelect = hasManager ? "ManagerName" : "CAST(NULL AS NVARCHAR(200)) AS ManagerName";

                    string sql = $@"SELECT BranchId, BranchCode, BranchName, Address, Phone, {managerSelect}, 
                                   IsActive, CreatedDate, UpdatedDate
                                   FROM Branches 
                                   ORDER BY BranchName ASC";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi truy vấn chi nhánh: " + ex.Message);
                }
            }

            return dt;
        }

        /// <summary>
        /// Lấy chi tiết 1 chi nhánh theo ID
        /// </summary>
        public async Task<DataTable> GetBranchByIdAsync(int branchId)
        {
            DataTable dt = new DataTable();

            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    bool hasManager = await BranchHasManagerNameAsync(conn);
                    string managerSelect = hasManager ? "ManagerName" : "CAST(NULL AS NVARCHAR(200)) AS ManagerName";

                    string sql = $@"SELECT BranchId, BranchCode, BranchName, Address, Phone, {managerSelect}, 
                                   IsActive, CreatedDate, UpdatedDate 
                                   FROM Branches 
                                   WHERE BranchId = @id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", branchId);

                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi truy vấn chi tiết chi nhánh: " + ex.Message);
                }
            }

            return dt;
        }

        /// <summary>
        /// Thêm chi nhánh mới
        /// </summary>
        public async Task<bool> AddBranchAsync(string branchCode, string branchName, string address, 
            string phone, string managerName, bool isActive)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    bool hasManager = await BranchHasManagerNameAsync(conn);

                    string sql = hasManager
                        ? @"INSERT INTO Branches (BranchCode, BranchName, Address, Phone, ManagerName, 
                                   IsActive, CreatedDate, UpdatedDate) 
                                   VALUES (@code, @name, @addr, @phone, @manager, @active, GETDATE(), GETDATE())"
                        : @"INSERT INTO Branches (BranchCode, BranchName, Address, Phone, 
                                   IsActive, CreatedDate, UpdatedDate) 
                                   VALUES (@code, @name, @addr, @phone, @active, GETDATE(), GETDATE())";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", branchCode);
                        cmd.Parameters.AddWithValue("@name", branchName);
                        cmd.Parameters.AddWithValue("@addr", string.IsNullOrEmpty(address) ? DBNull.Value : (object)address);
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                        if (hasManager)
                        {
                            cmd.Parameters.AddWithValue("@manager", string.IsNullOrEmpty(managerName) ? DBNull.Value : (object)managerName);
                        }
                        cmd.Parameters.AddWithValue("@active", isActive ? 1 : 0);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2601 || ex.Number == 2627)
                        throw new Exception("Mã chi nhánh đã tồn tại!");

                    throw new Exception("Lỗi SQL: " + ex.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi thêm chi nhánh: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Cập nhật chi nhánh
        /// </summary>
        public async Task<bool> UpdateBranchAsync(int branchId, string branchCode, string branchName, 
            string address, string phone, string managerName, bool isActive)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    bool hasManager = await BranchHasManagerNameAsync(conn);

                    string sql = hasManager
                        ? @"UPDATE Branches SET BranchCode = @code, BranchName = @name, 
                                   Address = @addr, Phone = @phone, ManagerName = @manager, 
                                   IsActive = @active, UpdatedDate = GETDATE() 
                                   WHERE BranchId = @id"
                        : @"UPDATE Branches SET BranchCode = @code, BranchName = @name, 
                                   Address = @addr, Phone = @phone, 
                                   IsActive = @active, UpdatedDate = GETDATE() 
                                   WHERE BranchId = @id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", branchId);
                        cmd.Parameters.AddWithValue("@code", branchCode);
                        cmd.Parameters.AddWithValue("@name", branchName);
                        cmd.Parameters.AddWithValue("@addr", string.IsNullOrEmpty(address) ? DBNull.Value : (object)address);
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                        if (hasManager)
                        {
                            cmd.Parameters.AddWithValue("@manager", string.IsNullOrEmpty(managerName) ? DBNull.Value : (object)managerName);
                        }
                        cmd.Parameters.AddWithValue("@active", isActive ? 1 : 0);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi cập nhật chi nhánh: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Xóa chi nhánh (Logic delete - chỉ đánh dấu IsActive = false)
        /// </summary>
        public async Task<bool> DeleteBranchAsync(int branchId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    string sql = @"UPDATE Branches SET IsActive = 0, UpdatedDate = GETDATE() 
                                   WHERE BranchId = @id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", branchId);
                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi xóa chi nhánh: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Cập nhật trạng thái kích hoạt/Vô hiệu hóa chi nhánh
        /// </summary>
        public async Task<bool> ToggleBranchStatusAsync(int branchId, bool isActive)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    string sql = @"UPDATE Branches SET IsActive = @active, UpdatedDate = GETDATE() 
                                   WHERE BranchId = @id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", branchId);
                        cmd.Parameters.AddWithValue("@active", isActive ? 1 : 0);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi cập nhật trạng thái: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Kiểm tra cột ManagerName tồn tại để tránh lỗi trên DB cũ
        /// </summary>
        private async Task<bool> BranchHasManagerNameAsync(SqlConnection conn)
        {
            using (var cmd = new SqlCommand("SELECT CASE WHEN COL_LENGTH('Branches','ManagerName') IS NULL THEN 0 ELSE 1 END", conn))
            {
                var result = await cmd.ExecuteScalarAsync();
                return result != null && Convert.ToInt32(result) == 1;
            }
        }
    }
}
