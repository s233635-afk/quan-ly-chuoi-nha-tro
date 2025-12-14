using System;
using System.Linq;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL
{
    public class UserBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public async Task<bool> DangKy(string username, string password, string fullname)
        {
            return await DangKy(username, password, fullname, null, null);
        }

        public async Task<bool> DangKy(string username, string password, string fullname, string email, string phone)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
            }

            if (password.Length < 6)
            {
                throw new Exception("Mật khẩu phải dài hơn 6 ký tự!");
            }

            if (!string.IsNullOrWhiteSpace(fullname) && fullname.Trim().Length < 2)
                throw new Exception("Họ tên không hợp lệ!");

            if (!string.IsNullOrWhiteSpace(email) && (!email.Contains("@") || !email.Contains(".")))
                throw new Exception("Email không hợp lệ!");

            if (!string.IsNullOrWhiteSpace(phone))
            {
                var digits = new string(phone.Where(char.IsDigit).ToArray());
                if (digits.Length < 8 || digits.Length > 15)
                    throw new Exception("Số điện thoại không hợp lệ!");
            }

            return await dbHelper.RegisterUserAsync(username, password, fullname, email, phone);
        }

        public async Task<string> DangNhap(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Vui lòng nhập tài khoản và mật khẩu!");
            }

            string fullName = await dbHelper.LoginAsync(username, password);

            if (fullName == null)
            {
                throw new Exception("Sai tên tài khoản hoặc mật khẩu!");
            }

            return fullName;
        }

        public async Task<int> GetUserRoleAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Tên người dùng không hợp lệ!");
            }

            return await dbHelper.GetUserRoleAsync(username);
        }

        public async Task<(int UserId, int RoleId, int? BranchId)> GetUserAccessAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("Tên người dùng không hợp lệ!");

            return await dbHelper.GetUserAccessAsync(username);
        }

        public async Task<bool> ResetPasswordAsync(string username, string fullName, string email, string phone, string newPassword)
        {
            username = (username ?? string.Empty).Trim();
            fullName = (fullName ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();
            phone = (phone ?? string.Empty).Trim();
            newPassword = (newPassword ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("Vui lòng nhập tên đăng nhập!");

            if (newPassword.Length < 6)
                throw new Exception("Mật khẩu mới phải có ít nhất 6 ký tự!");

            bool hasRecoveryInfo = !string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(phone) || !string.IsNullOrWhiteSpace(fullName);
            if (!hasRecoveryInfo)
                throw new Exception("Vui lòng nhập ít nhất 1 thông tin: Email / Số điện thoại / Họ và tên.");

            bool ok = await dbHelper.ResetPasswordAsync(username, fullName, email, phone, newPassword);
            if (!ok)
                throw new Exception("Không tìm thấy tài khoản hoặc thông tin xác minh không khớp.");

            return true;
        }
    }
}
