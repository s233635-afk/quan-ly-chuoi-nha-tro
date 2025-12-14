using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

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

                    var columns = await GetBranchSelectableColumnsAsync(conn);

                    string sql = $@"SELECT {string.Join(", ", columns)}
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

                    var columns = await GetBranchSelectableColumnsAsync(conn);

                    string sql = $@"SELECT {string.Join(", ", columns)}
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
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    bool hasHotline = await HasColumnAsync(conn, "Branches", "Hotline");
                    bool hasOperatingHours = await HasColumnAsync(conn, "Branches", "OperatingHours");
                    bool hasDescription = await HasColumnAsync(conn, "Branches", "Description");

                    var cols = new List<string> { "BranchCode", "BranchName", "Address", "Phone" };
                    var vals = new List<string> { "@code", "@name", "@addr", "@phone" };

                    if (hasHotline)
                    {
                        cols.Add("Hotline");
                        vals.Add("@hotline");
                    }
                    if (hasOperatingHours)
                    {
                        cols.Add("OperatingHours");
                        vals.Add("@hours");
                    }
                    if (hasDescription)
                    {
                        cols.Add("Description");
                        vals.Add("@desc");
                    }

                    cols.Add("IsActive");
                    vals.Add("@active");
                    cols.Add("CreatedDate");
                    vals.Add("GETDATE()");
                    cols.Add("UpdatedDate");
                    vals.Add("GETDATE()");

                    string sql = $@"INSERT INTO Branches ({string.Join(", ", cols)})
                                   VALUES ({string.Join(", ", vals)})";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", branchCode);
                        cmd.Parameters.AddWithValue("@name", branchName);
                        cmd.Parameters.AddWithValue("@addr", string.IsNullOrEmpty(address) ? DBNull.Value : (object)address);
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                        if (hasHotline) cmd.Parameters.AddWithValue("@hotline", string.IsNullOrEmpty(hotline) ? DBNull.Value : (object)hotline);
                        if (hasOperatingHours) cmd.Parameters.AddWithValue("@hours", string.IsNullOrEmpty(operatingHours) ? DBNull.Value : (object)operatingHours);
                        if (hasDescription) cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(description) ? DBNull.Value : (object)description);
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
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    bool hasHotline = await HasColumnAsync(conn, "Branches", "Hotline");
                    bool hasOperatingHours = await HasColumnAsync(conn, "Branches", "OperatingHours");
                    bool hasDescription = await HasColumnAsync(conn, "Branches", "Description");

                    var sets = new List<string>
                    {
                        "BranchCode = @code",
                        "BranchName = @name",
                        "Address = @addr",
                        "Phone = @phone"
                    };
                    if (hasHotline) sets.Add("Hotline = @hotline");
                    if (hasOperatingHours) sets.Add("OperatingHours = @hours");
                    if (hasDescription) sets.Add("Description = @desc");
                    sets.Add("IsActive = @active");
                    sets.Add("UpdatedDate = GETDATE()");

                    string sql = $@"UPDATE Branches SET {string.Join(", ", sets)}
                                   WHERE BranchId = @id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", branchId);
                        cmd.Parameters.AddWithValue("@code", branchCode);
                        cmd.Parameters.AddWithValue("@name", branchName);
                        cmd.Parameters.AddWithValue("@addr", string.IsNullOrEmpty(address) ? DBNull.Value : (object)address);
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                        if (hasHotline) cmd.Parameters.AddWithValue("@hotline", string.IsNullOrEmpty(hotline) ? DBNull.Value : (object)hotline);
                        if (hasOperatingHours) cmd.Parameters.AddWithValue("@hours", string.IsNullOrEmpty(operatingHours) ? DBNull.Value : (object)operatingHours);
                        if (hasDescription) cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(description) ? DBNull.Value : (object)description);
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
        /// Danh sách cột chi nhánh theo schema hiện tại (để tương thích DB cũ/mới).
        /// </summary>
        private async Task<List<string>> GetBranchSelectableColumnsAsync(SqlConnection conn)
        {
            var cols = new List<string>
            {
                "BranchId",
                "BranchCode",
                "BranchName",
                "Address",
                "Phone"
            };

            if (await HasColumnAsync(conn, "Branches", "Hotline")) cols.Add("Hotline");
            if (await HasColumnAsync(conn, "Branches", "OperatingHours")) cols.Add("OperatingHours");
            if (await HasColumnAsync(conn, "Branches", "Description")) cols.Add("Description");

            cols.Add("IsActive");
            cols.Add("CreatedDate");
            cols.Add("UpdatedDate");

            return cols;
        }

        private async Task<bool> HasColumnAsync(SqlConnection conn, string tableName, string columnName)
        {
            using (var cmd = new SqlCommand("SELECT CASE WHEN COL_LENGTH(@t, @c) IS NULL THEN 0 ELSE 1 END", conn))
            {
                cmd.Parameters.AddWithValue("@t", tableName);
                cmd.Parameters.AddWithValue("@c", columnName);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && Convert.ToInt32(result) == 1;
            }
        }
    }
}
