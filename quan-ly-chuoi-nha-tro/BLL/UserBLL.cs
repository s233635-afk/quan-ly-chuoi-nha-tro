using System;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL
{
    public class UserBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public async Task<bool> DangKy(string username, string password, string fullname)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
            }

            if (password.Length < 6)
            {
                throw new Exception("Mật khẩu phải dài hơn 6 ký tự!");
            }

            return await dbHelper.RegisterUserAsync(username, password, fullname);
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
    }
}
