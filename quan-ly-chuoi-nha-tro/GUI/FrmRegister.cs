using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmRegister : Form
    {
        private UserBLL userBLL = new UserBLL();

        public FrmRegister()
        {
            InitializeComponent();
        }



        private async void btnRegister_Click_1(object sender, EventArgs e)
        {
            // Chống spam nút bấm làm treo app
            btnRegister.Enabled = false;
            btnRegister.Text = "Đang xử lý...";

            try
            {
                string user = txtUser.Text.Trim();
                string pass = txtPass.Text.Trim();
                string name = txtName.Text.Trim();

                // Gọi hàm BLL với từ khóa 'await'
                // Lúc này UI vẫn mượt, không bị xoay vòng tròn
                bool isSuccess = await userBLL.DangKy(user, pass, name);

                if (isSuccess)
                {
                    MessageBox.Show("Đăng ký thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Reset form
                    txtUser.Text = "";
                    txtPass.Text = "";
                    txtName.Text = "";
                }
            }
            catch (Exception ex)
            {
                // Hứng mọi lỗi từ BLL và DAL ném lên để hiện thông báo đẹp
                MessageBox.Show(ex.Message, "Lỗi Đăng Ký", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                // Dù thành công hay thất bại cũng phải mở lại nút
                btnRegister.Enabled = true;
                btnRegister.Text = "Đăng Ký";
            }
        }
    }
}
