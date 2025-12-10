using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace QuanLyNhaTro.DAL
{
    public partial class DatabaseHelper
    {
        private string connectionString = "Data Source=SQL9001.site4now.net;Initial Catalog=db_ac1f11_quanlynhatro;User Id=db_ac1f11_quanlynhatro_admin;Password=admin123";

        public async Task<bool> RegisterUserAsync(string username, string password, string fullName)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    // Mở kết nối bất đồng bộ (Async) để không treo máy
                    await conn.OpenAsync();

                    string sql = "INSERT INTO Users (Username, Password, FullName) VALUES (@user, @pass, @name)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        // Dùng tham số (Parameters) để chống hack SQL Injection
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", password); // Thực tế nên mã hóa MD5/Bcrypt
                        cmd.Parameters.AddWithValue("@name", fullName);

                        // ExecuteNonQueryAsync trả về số dòng bị ảnh hưởng
                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
                catch (SqlException ex)
                {
                    // Lỗi 2627 là trùng lặp dữ liệu (Trùng Username)
                    if (ex.Number == 2627)
                        throw new Exception("Tài khoản này đã tồn tại rồi!");

                    throw new Exception("Lỗi database: " + ex.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi kết nối: " + ex.Message);
                }
            }
        }

        // Thêm vào class DatabaseHelper trong thư mục DAL
        public async Task<string> LoginAsync(string username, string password)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    // Câu lệnh tìm người dùng
                    string sql = "SELECT FullName FROM Users WHERE Username = @user AND Password = @pass";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", password);

                        // ExecuteScalarAsync: Chỉ lấy 1 giá trị đầu tiên (FullName)
                        // Nếu không tìm thấy ai, nó sẽ trả về null
                        var result = await cmd.ExecuteScalarAsync();

                        if (result != null)
                        {
                            return result.ToString(); // Trả về Tên người dùng
                        }
                        return null; // Đăng nhập thất bại
                    }
                }
                catch (Exception ex)
                {
                    // Ném lỗi ra để tầng UI xử lý hiển thị
                    throw new Exception("Lỗi kết nối database: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Lấy RoleId của người dùng từ username
        /// </summary>
        public async Task<int> GetUserRoleAsync(string username)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    string sql = "SELECT RoleId FROM Users WHERE Username = @user AND IsActive = 1";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);

                        var result = await cmd.ExecuteScalarAsync();

                        if (result != null && int.TryParse(result.ToString(), out int roleId))
                        {
                            return roleId;
                        }

                        return 0; // Không tìm thấy hoặc không hoạt động
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi lấy quyền người dùng: " + ex.Message);
                }
            }
        }
    }
}
