using System;
using System.Windows.Forms;
using quan_ly_chuoi_nha_tro.GUI;

namespace quan_ly_chuoi_nha_tro
{
    /// <summary>
    /// Test Logo Generator
    /// </summary>
    internal class LogoTest
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Test: Generate logo và hiển thị
            var logo = LogoGenerator.GenerateLogo(200);
            MessageBox.Show($"Logo tạo thành công! Kích thước: {logo.Width}x{logo.Height}",
                "✅ Logo Generator Test", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Chạy LoginForm với logo
            Application.Run(new FrmLogin());
        }
    }
}
