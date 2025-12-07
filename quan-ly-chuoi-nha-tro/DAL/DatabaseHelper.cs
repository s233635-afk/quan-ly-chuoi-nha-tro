using MySqlConnector;
using System;
using System.Data;
using System.Threading.Tasks;

namespace WinformTiDB.DAL
{
    public class DatabaseHelper
    {
        // Tôi đã chuyển đổi chuỗi mysql:// của bạn sang chuẩn ADO.NET
        // Thêm SslMode=VerifyCA vì TiDB Cloud bắt buộc bảo mật SSL
        private string connectionString = "Server=gateway01.ap-southeast-1.prod.aws.tidbcloud.com;" +
                                          "Port=4000;" +
                                          "Database=test;" +
                                          "Uid=2AHiCRdnyxEBa2H.root;" +
                                          "Pwd=h4OkywilZcCWARp5;" +
                                          "SslMode=VerifyCA;";

        public async Task<bool> RegisterUserAsync(string username, string password, string fullName)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Mở kết nối bất đồng bộ (Async) để không treo máy
                    await conn.OpenAsync();

                    string sql = "INSERT INTO Users (Username, Password, FullName) VALUES (@user, @pass, @name)";

                    using (var cmd = new MySqlCommand(sql, conn))
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
                catch (MySqlException ex)
                {
                    // Lỗi 1062 là trùng lặp dữ liệu (Trùng Username)
                    if (ex.Number == 1062)
                        throw new Exception("Tài khoản này đã tồn tại rồi!");

                    throw new Exception("Lỗi TiDB: " + ex.Message);
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
            using (var conn = new MySqlConnection(connectionString)) // connectionString lấy từ bài trước
            {
                try
                {
                    await conn.OpenAsync();

                    // Câu lệnh tìm người dùng
                    string sql = "SELECT FullName FROM Users WHERE Username = @user AND Password = @pass";

                    using (var cmd = new MySqlCommand(sql, conn))
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
                    throw new Exception("Lỗi kết nối TiDB: " + ex.Message);
                }
            }
        }
    }
}