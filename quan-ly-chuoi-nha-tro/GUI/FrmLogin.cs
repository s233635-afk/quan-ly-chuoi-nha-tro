using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public partial class FrmLogin : Form
    {
        private readonly UserBLL userBLL = new UserBLL();

        private const string UserPlaceholder = "Tên đăng nhập";
        private const string PassPlaceholder = "Mật khẩu";

        private readonly List<PointF> _stars = new List<PointF>();
        private readonly Random _starRandom = new Random(20251213);

        private Panel _focusedInputPanel;
        private readonly Color _loginPrimary = Color.FromArgb(0, 173, 181);
        private readonly Color _loginPrimaryHover = Color.FromArgb(0, 153, 160);

        public FrmLogin()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            DoubleBuffered = true;
            AcceptButton = btnLogin;
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
            ApplyModernStyling();
            CenterCard();
        }

        private void ApplyModernStyling()
        {
            BackColor = Color.White;

            ApplyRoundedRegion(pnlForm, 22);
            pnlForm.Resize += (s, e) => ApplyRoundedRegion(pnlForm, 22);

            btnLogin.BackColor = _loginPrimary;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Cursor = Cursors.Hand;
            ApplyRoundedRegion(btnLogin, 14);
            btnLogin.Resize += (s, e) => ApplyRoundedRegion(btnLogin, 14);
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = _loginPrimaryHover;
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = _loginPrimary;

            txtUser.BackColor = Color.White;
            txtPass.BackColor = Color.White;

            WireInputFocus(txtUser, pnlUserBox);
            WireInputFocus(txtPass, pnlPassBox);

            SetupPlaceholder(txtUser, UserPlaceholder, isPassword: false);
            SetupPlaceholder(txtPass, PassPlaceholder, isPassword: true);

            TryLoadRememberedUsername();

            txtUser.Focus();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            btnLogin.Text = "Đang kiểm tra...";

            try
            {
                string user = ReadTextboxValue(txtUser, UserPlaceholder).Trim();
                string pass = ReadTextboxValue(txtPass, PassPlaceholder).Trim();

                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                {
                    MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string fullName = await userBLL.DangNhap(user, pass);
                int roleId = await userBLL.GetUserRoleAsync(user);

                SaveRememberedUsername(user);

                MessageBox.Show($"Xin chào {fullName}!", "Đăng nhập thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (roleId == 1) // Admin
                {
                    FrmAdminDashboard adminForm = new FrmAdminDashboard(user, 1);
                    Hide();
                    adminForm.ShowDialog();
                    Show();
                }
                else if (roleId == 2) // Staff
                {
                    FrmMain formMain = new FrmMain(user, "Nhân Viên");
                    Hide();
                    formMain.ShowDialog();
                    Show();
                }

                SetPlaceholderIfEmpty(txtUser, UserPlaceholder, isPassword: false);
                SetPlaceholderIfEmpty(txtPass, PassPlaceholder, isPassword: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Đăng nhập";
            }
        }

        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick();
                e.Handled = true;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterCard();
            pnlContainer?.Invalidate();
        }

        private void CenterCard()
        {
            if (pnlContainer == null || pnlForm == null) return;
            int x = (pnlContainer.ClientSize.Width - pnlForm.Width) / 2;
            int y = (pnlContainer.ClientSize.Height - pnlForm.Height) / 2;
            pnlForm.Location = new Point(Math.Max(12, x), Math.Max(12, y));
        }

        private static void ApplyRoundedRegion(Control control, int radius)
        {
            if (control == null || control.Width <= 0 || control.Height <= 0) return;

            int d = radius * 2;
            var rect = new Rectangle(0, 0, control.Width, control.Height);

            using (var path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                control.Region = new Region(path);
            }
        }

        private void WireInputFocus(TextBox textBox, Panel hostPanel)
        {
            if (textBox == null || hostPanel == null) return;

            textBox.Enter += (s, e) =>
            {
                _focusedInputPanel = hostPanel;
                hostPanel.Invalidate();
            };
            textBox.Leave += (s, e) =>
            {
                if (_focusedInputPanel == hostPanel) _focusedInputPanel = null;
                hostPanel.Invalidate();
            };
            hostPanel.Click += (s, e) => textBox.Focus();
        }

        private void SetupPlaceholder(TextBox textBox, string placeholder, bool isPassword)
        {
            if (textBox == null) return;

            textBox.GotFocus += (s, e) => ClearPlaceholder(textBox, isPassword);
            textBox.LostFocus += (s, e) => SetPlaceholderIfEmpty(textBox, placeholder, isPassword);

            textBox.Tag = new PlaceholderState { Placeholder = placeholder, IsPassword = isPassword, IsActive = false };
            SetPlaceholderIfEmpty(textBox, placeholder, isPassword);
        }

        private void ClearPlaceholder(TextBox textBox, bool isPassword)
        {
            if (!(textBox?.Tag is PlaceholderState st)) return;
            if (!st.IsActive) return;

            textBox.Text = string.Empty;
            textBox.ForeColor = Color.FromArgb(33, 37, 41);
            st.IsActive = false;
            textBox.Tag = st;

            if (isPassword)
                textBox.UseSystemPasswordChar = true;
        }

        private void SetPlaceholderIfEmpty(TextBox textBox, string placeholder, bool isPassword)
        {
            if (!(textBox?.Tag is PlaceholderState st)) return;

            if (!string.IsNullOrWhiteSpace(textBox.Text) && textBox.Text != placeholder)
                return;

            textBox.UseSystemPasswordChar = false;
            textBox.Text = placeholder;
            textBox.ForeColor = Color.FromArgb(140, 140, 140);
            st.IsActive = true;
            st.Placeholder = placeholder;
            st.IsPassword = isPassword;
            textBox.Tag = st;
        }

        private static string ReadTextboxValue(TextBox textBox, string placeholder)
        {
            if (textBox == null) return string.Empty;
            if (textBox.Text == placeholder) return string.Empty;
            if (textBox.Tag is PlaceholderState st && st.IsActive) return string.Empty;
            return textBox.Text ?? string.Empty;
        }

        private struct PlaceholderState
        {
            public string Placeholder;
            public bool IsPassword;
            public bool IsActive;
        }

        private string GetRememberPath()
        {
            try
            {
                return Path.Combine(Application.UserAppDataPath, "remember_user.txt");
            }
            catch
            {
                return null;
            }
        }

        private void TryLoadRememberedUsername()
        {
            var path = GetRememberPath();
            if (string.IsNullOrWhiteSpace(path)) return;
            if (!File.Exists(path)) return;

            try
            {
                var user = File.ReadAllText(path).Trim();
                if (!string.IsNullOrWhiteSpace(user))
                {
                    chkRemember.Checked = true;
                    ClearPlaceholder(txtUser, isPassword: false);
                    txtUser.Text = user;
                    txtUser.ForeColor = Color.FromArgb(33, 37, 41);
                }
            }
            catch
            {
                // ignore
            }
        }

        private void SaveRememberedUsername(string username)
        {
            var path = GetRememberPath();
            if (string.IsNullOrWhiteSpace(path)) return;

            try
            {
                if (chkRemember != null && chkRemember.Checked)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, username ?? string.Empty);
                }
                else
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
            }
            catch
            {
                // ignore
            }
        }

        private void pnlContainer_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = pnlContainer.ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(0, 60, 150), Color.FromArgb(0, 173, 181), 135f))
            {
                g.FillRectangle(brush, rect);
            }

            DrawBackgroundStreaks(g, rect);

            if (pnlForm != null && pnlForm.Visible)
            {
                DrawShadow(g, pnlForm.Bounds, radius: 22);
            }
        }

        private void DrawBackgroundStreaks(Graphics g, Rectangle rect)
        {
            using (var pen = new Pen(Color.FromArgb(40, 255, 255, 255), 2f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                int count = 22;
                for (int i = 0; i < count; i++)
                {
                    int x1 = rect.Left + (rect.Width * i / count);
                    int y1 = rect.Top - 40;
                    int x2 = x1 + 140;
                    int y2 = rect.Top + 120;
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }

            using (var glow = new SolidBrush(Color.FromArgb(35, 255, 255, 255)))
            {
                g.FillEllipse(glow, rect.Width - 220, rect.Height - 220, 360, 360);
                g.FillEllipse(glow, -140, rect.Height - 200, 280, 280);
            }
        }

        private void DrawShadow(Graphics g, Rectangle bounds, int radius)
        {
            var shadowRect = bounds;
            shadowRect.Inflate(18, 18);

            for (int i = 10; i >= 1; i--)
            {
                int alpha = 10 + (i * 7);
                var r = shadowRect;
                r.Inflate(-i, -i);
                using (var path = RoundedRectPath(r, radius + i))
                using (var brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private static GraphicsPath RoundedRectPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void pnlForm_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var pen = new Pen(Color.FromArgb(235, 235, 235), 1f))
            using (var path = RoundedRectPath(new Rectangle(0, 0, pnlForm.Width - 1, pnlForm.Height - 1), 22))
            {
                g.DrawPath(pen, path);
            }
        }

        private void pnlRight_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = pnlRight.ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(0, 70, 180), Color.FromArgb(0, 173, 181), 135f))
            {
                g.FillRectangle(brush, rect);
            }

            EnsureStars(rect);
            using (var starBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
            {
                foreach (var p in _stars)
                {
                    g.FillEllipse(starBrush, p.X, p.Y, 2f, 2f);
                }
            }

            DrawRightPanelWaves(g, rect);
        }

        private void EnsureStars(Rectangle rect)
        {
            if (_stars.Count > 0) return;

            for (int i = 0; i < 90; i++)
            {
                float x = (float)(_starRandom.NextDouble() * rect.Width);
                float y = (float)(_starRandom.NextDouble() * rect.Height);
                _stars.Add(new PointF(x, y));
            }
        }

        private void DrawRightPanelWaves(Graphics g, Rectangle rect)
        {
            using (var path = new GraphicsPath())
            {
                path.StartFigure();
                path.AddLine(rect.Left, rect.Top, rect.Left, rect.Bottom);

                var p1 = new Point(rect.Left, rect.Bottom);
                var c1 = new Point(rect.Left + rect.Width / 3, rect.Bottom - rect.Height / 5);
                var c2 = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
                var p2 = new Point(rect.Left + rect.Width / 5, rect.Top);
                path.AddBezier(p1, c1, c2, p2);
                path.CloseFigure();

                using (var brush = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                {
                    g.FillPath(brush, path);
                }
            }

            using (var pen = new Pen(Color.FromArgb(70, 255, 255, 255), 2f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                for (int i = 0; i < 14; i++)
                {
                    int x1 = rect.Left + (rect.Width * i / 14);
                    int y1 = rect.Top - 30;
                    g.DrawLine(pen, x1, y1, x1 + 120, y1 + 110);
                }
            }
        }

        private void inputBox_Paint(object sender, PaintEventArgs e)
        {
            if (!(sender is Panel panel)) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            var borderColor = panel == _focusedInputPanel
                ? _loginPrimary
                : Color.FromArgb(215, 215, 215);

            using (var path = RoundedRectPath(rect, 12))
            using (var pen = new Pen(borderColor, 1.6f))
            {
                g.DrawPath(pen, path);
            }
        }

        private void lnkForgot_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Vui lòng liên hệ quản trị viên để đặt lại mật khẩu.", "Quên mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var frm = new FrmRegister())
            {
                frm.ShowDialog(this);
            }
        }
    }
}

