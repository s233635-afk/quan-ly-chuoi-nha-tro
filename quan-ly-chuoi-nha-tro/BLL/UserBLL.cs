using System;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL
{
    public class UserBLL
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        public async Task<bool> DangKy(string username, string password, string fullname)
        {
            // 1. Kiểm tra nghiệp vụ cơ bản
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
            }

            if (password.Length < 6)
            {
                throw new Exception("Mật khẩu phải dài hơn 6 ký tự!");
            }

            // 2. Gọi xuống tầng DAL để lưu
            return await dbHelper.RegisterUserAsync(username, password, fullname);
        }

        // Thêm vào class UserBLL trong thư mục BLL
        public async Task<string> DangNhap(string username, string password)
        {
            // 1. Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Vui lòng nhập tài khoản và mật khẩu!");
            }

            // 2. Gọi DAL kiểm tra trong Database
            // Lưu ý: Ở dự án thật, mật khẩu nên được mã hóa (MD5/BCrypt) trước khi so sánh
            string fullName = await dbHelper.LoginAsync(username, password);

            if (fullName == null)
            {
                throw new Exception("Sai tên tài khoản hoặc mật khẩu!");
            }

            // 3. Trả về tên người dùng để hiển thị xin chào
            return fullName;
        }
    }


}