using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

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
        private readonly Color _loginPrimary = ModernTheme.Colors.Primary;
        private readonly Color _loginPrimaryHover = ModernTheme.Colors.PrimaryDark;

        private Label _userPlaceholderLabel;
        private Label _passPlaceholderLabel;
        private PictureBox _logoBox;

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
            SetupRightPanelContent();
        }

        private void SetupRightPanelContent()
        {
            if (pnlRight == null) return;
            pnlRight.Controls.Clear();

            // 1. Logo nhỏ gọn phía trên
            _logoBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Size = new Size(190, 190),
                Image = LoadLogoImage() ?? BuildLogoImage(380)
            };
            pnlRight.Controls.Add(_logoBox);

            // 2. Tiêu đề phiên bản hệ thống
            var lblWelcome = new Label
            {
                Text = "Hệ thống quản lý\nchuỗi nhà trọ V2.0",
                Font = ModernTheme.Fonts.Bold(18),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlRight.Controls.Add(lblWelcome);

            // Căn chỉnh vị trí khi resize
            pnlRight.Resize += (s, e) =>
            {
                _logoBox.Location = new Point((pnlRight.Width - _logoBox.Width) / 2, (pnlRight.Height / 2) - _logoBox.Height + 10);
                lblWelcome.Location = new Point((pnlRight.Width - lblWelcome.Width) / 2, (pnlRight.Height / 2) + 20);
            };

            // Kích hoạt resize lần đầu
            _logoBox.Location = new Point((pnlRight.Width - _logoBox.Width) / 2, (pnlRight.Height / 2) - _logoBox.Height + 10);
            lblWelcome.Location = new Point((pnlRight.Width - lblWelcome.Width) / 2, (pnlRight.Height / 2) + 20);
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

            // Sync icons and links with ModernTheme
            lblUserIcon.ForeColor = _loginPrimary;
            lblPassIcon.ForeColor = _loginPrimary;
            lnkForgot.LinkColor = ModernTheme.Colors.TextSecondary;
            lnkForgot.ActiveLinkColor = _loginPrimary;
            lnkRegister.LinkColor = _loginPrimary;
            lnkRegister.ActiveLinkColor = ModernTheme.Colors.PrimaryDark;

            WireInputFocus(txtUser, pnlUserBox);
            WireInputFocus(txtPass, pnlPassBox);

            _userPlaceholderLabel = SetupOverlayPlaceholder(txtUser, pnlUserBox, UserPlaceholder, isPassword: false);
            _passPlaceholderLabel = SetupOverlayPlaceholder(txtPass, pnlPassBox, PassPlaceholder, isPassword: true);

            if (chkShowPassword != null)
            {
                chkShowPassword.Checked = false;
                chkShowPassword.CheckedChanged += (s, e) =>
                {
                    txtPass.UseSystemPasswordChar = !chkShowPassword.Checked;
                    UpdateOverlayPlaceholderVisibility(_passPlaceholderLabel, txtPass);
                };
            }

            txtUser.Focus();
        }

        private void SetupLogo()
        {
            // Đã được thay thế bởi SetupRightPanelContent
        }

        private void CenterLogo()
        {
            if (_logoBox == null || pnlRight == null) return;
            int target = (int)(Math.Min(pnlRight.Width, pnlRight.Height) * 0.9f);
            if (target < 120) target = Math.Min(pnlRight.Width, pnlRight.Height) - 16;
            if (target < 80) target = 80;
            _logoBox.Size = new Size(target, target);
            _logoBox.Location = new Point(
                (pnlRight.Width - _logoBox.Width) / 2,
                (pnlRight.Height - _logoBox.Height) / 2);
        }

        private static Image LoadLogoImage()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = FindLogoPath(baseDir);
            if (path == null || !File.Exists(path)) return null;
            try
            {
                using (var img = Image.FromFile(path))
                {
                    return RemoveDarkBackground(new Bitmap(img), 28, 0.9f);
                }
            }
            catch
            {
                return null;
            }
        }

        private static Bitmap RemoveDarkBackground(Bitmap source, int threshold, float boostSaturation)
        {
            if (source == null) return null;
            var bmp = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImageUnscaled(source, 0, 0);
            }

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    if (c.R <= threshold && c.G <= threshold && c.B <= threshold)
                    {
                        bmp.SetPixel(x, y, Color.Transparent);
                        continue;
                    }

                    bmp.SetPixel(x, y, BoostSaturation(c, boostSaturation));
                }
            }
            return bmp;
        }

        private static Color BoostSaturation(Color c, float amount)
        {
            float r = c.R / 255f;
            float g = c.G / 255f;
            float b = c.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;
            if (delta <= 0.0001f) return c;

            float l = (max + min) * 0.5f;
            float s = delta / (1f - Math.Abs(2f * l - 1f));
            s = Math.Min(1f, s + amount);

            float h;
            if (max == r) h = ((g - b) / delta) % 6f;
            else if (max == g) h = ((b - r) / delta) + 2f;
            else h = ((r - g) / delta) + 4f;
            h *= 60f;
            if (h < 0) h += 360f;

            return FromHsl(h, s, l, c.A);
        }

        private static Color FromHsl(float h, float s, float l, int a)
        {
            float c = (1f - Math.Abs(2f * l - 1f)) * s;
            float x = c * (1f - Math.Abs((h / 60f) % 2f - 1f));
            float m = l - c / 2f;

            float r1, g1, b1;
            if (h < 60f) { r1 = c; g1 = x; b1 = 0; }
            else if (h < 120f) { r1 = x; g1 = c; b1 = 0; }
            else if (h < 180f) { r1 = 0; g1 = c; b1 = x; }
            else if (h < 240f) { r1 = 0; g1 = x; b1 = c; }
            else if (h < 300f) { r1 = x; g1 = 0; b1 = c; }
            else { r1 = c; g1 = 0; b1 = x; }

            int r = (int)Math.Round((r1 + m) * 255);
            int g = (int)Math.Round((g1 + m) * 255);
            int b = (int)Math.Round((b1 + m) * 255);
            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));
            return Color.FromArgb(a, r, g, b);
        }

        private static string FindLogoPath(string baseDir)
        {
            var current = new DirectoryInfo(baseDir);
            for (int i = 0; i < 5 && current != null; i++)
            {
                var candidate = Path.Combine(current.FullName, "assets", "logo.png");
                if (File.Exists(candidate)) return candidate;
                current = current.Parent;
            }
            return null;
        }

        private static Image BuildLogoImage(int size)
        {
            var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float stroke = size * 0.078f;
                float innerStroke = stroke * 0.52f;

                var leftGrad = new LinearGradientBrush(
                    new PointF(size * 0.12f, size * 0.70f),
                    new PointF(size * 0.52f, size * 0.45f),
                    Color.FromArgb(70, 160, 245),
                    Color.FromArgb(120, 210, 255));

                var rightGrad = new LinearGradientBrush(
                    new PointF(size * 0.48f, size * 0.45f),
                    new PointF(size * 0.88f, size * 0.70f),
                    Color.FromArgb(140, 220, 120),
                    Color.FromArgb(245, 200, 90));

                using (var leftPen = new Pen(leftGrad, stroke))
                using (var rightPen = new Pen(rightGrad, stroke))
                using (var leftInner = new Pen(leftGrad, innerStroke))
                using (var rightInner = new Pen(rightGrad, innerStroke))
                {
                    leftPen.StartCap = LineCap.Round;
                    leftPen.EndCap = LineCap.Round;
                    leftPen.LineJoin = LineJoin.Round;
                    rightPen.StartCap = LineCap.Round;
                    rightPen.EndCap = LineCap.Round;
                    rightPen.LineJoin = LineJoin.Round;
                    leftInner.StartCap = LineCap.Round;
                    leftInner.EndCap = LineCap.Round;
                    leftInner.LineJoin = LineJoin.Round;
                    rightInner.StartCap = LineCap.Round;
                    rightInner.EndCap = LineCap.Round;
                    rightInner.LineJoin = LineJoin.Round;

                    var leftPath = new[]
                    {
                        new PointF(size * 0.26f, size * 0.76f),
                        new PointF(size * 0.26f, size * 0.48f),
                        new PointF(size * 0.40f, size * 0.48f),
                        new PointF(size * 0.50f, size * 0.64f)
                    };
                    g.DrawLines(leftPen, leftPath);

                    var rightPath = new[]
                    {
                        new PointF(size * 0.74f, size * 0.76f),
                        new PointF(size * 0.74f, size * 0.48f),
                        new PointF(size * 0.60f, size * 0.48f),
                        new PointF(size * 0.50f, size * 0.64f)
                    };
                    g.DrawLines(rightPen, rightPath);

                    g.DrawLine(leftPen, size * 0.26f, size * 0.48f, size * 0.40f, size * 0.48f);
                    g.DrawLine(rightPen, size * 0.60f, size * 0.48f, size * 0.74f, size * 0.48f);

                    g.DrawLine(leftInner, size * 0.32f, size * 0.58f, size * 0.32f, size * 0.68f);
                    g.DrawLine(rightInner, size * 0.68f, size * 0.58f, size * 0.68f, size * 0.68f);
                    g.DrawLine(leftInner, size * 0.40f, size * 0.54f, size * 0.46f, size * 0.62f);
                    g.DrawLine(rightInner, size * 0.60f, size * 0.54f, size * 0.54f, size * 0.62f);
                    g.DrawLine(leftInner, size * 0.50f, size * 0.58f, size * 0.50f, size * 0.70f);
                }

                DrawNode(g, new PointF(size * 0.26f, size * 0.48f), size * 0.022f, Color.FromArgb(100, 190, 255));
                DrawNode(g, new PointF(size * 0.74f, size * 0.48f), size * 0.022f, Color.FromArgb(240, 200, 90));
                DrawNode(g, new PointF(size * 0.50f, size * 0.64f), size * 0.026f, Color.FromArgb(140, 220, 150));
                DrawNode(g, new PointF(size * 0.40f, size * 0.48f), size * 0.018f, Color.FromArgb(120, 210, 255));
                DrawNode(g, new PointF(size * 0.60f, size * 0.48f), size * 0.018f, Color.FromArgb(200, 220, 120));

                DrawHouse(g, new PointF(size * 0.20f, size * 0.26f), size * 0.12f, Color.FromArgb(140, 210, 255));
                DrawHouse(g, new PointF(size * 0.80f, size * 0.26f), size * 0.12f, Color.FromArgb(140, 210, 255));

                DrawKey(g, new PointF(size * 0.50f, size * 0.34f), size * 0.14f);
                DrawTopHub(g, size);
            }

            return bmp;
        }

        private static void DrawNode(Graphics g, PointF center, float radius, Color color)
        {
            using (var brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
            }
        }

        private static void DrawHouse(Graphics g, PointF center, float size, Color color)
        {
            float half = size * 0.5f;
            var roof = new[]
            {
                new PointF(center.X - half, center.Y),
                new PointF(center.X, center.Y - half),
                new PointF(center.X + half, center.Y)
            };

            var body = new RectangleF(center.X - half * 0.7f, center.Y, half * 1.4f, half * 0.9f);

            using (var pen = new Pen(color, size * 0.08f))
            using (var brush = new SolidBrush(Color.FromArgb(30, color)))
            {
                pen.LineJoin = LineJoin.Round;
                g.DrawPolygon(pen, roof);
                g.FillRectangle(brush, body);
                g.DrawRectangle(pen, body.X, body.Y, body.Width, body.Height);
            }
        }

        private static void DrawKey(Graphics g, PointF center, float size)
        {
            float r = size * 0.22f;
            using (var pen = new Pen(Color.FromArgb(255, 210, 90), size * 0.08f))
            {
                pen.LineJoin = LineJoin.Round;
                g.DrawEllipse(pen, center.X - r, center.Y - r, r * 2f, r * 2f);
                g.DrawLine(pen, center.X, center.Y + r, center.X, center.Y + size * 0.8f);
                g.DrawLine(pen, center.X, center.Y + size * 0.55f, center.X + size * 0.18f, center.Y + size * 0.55f);
            }
        }

        private static void DrawTopHub(Graphics g, int size)
        {
            using (var pen = new Pen(Color.FromArgb(140, 220, 150), size * 0.04f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, size * 0.50f, size * 0.26f, size * 0.50f, size * 0.18f);
                g.DrawLine(pen, size * 0.44f, size * 0.28f, size * 0.36f, size * 0.22f);
                g.DrawLine(pen, size * 0.56f, size * 0.28f, size * 0.64f, size * 0.22f);
            }

            DrawNode(g, new PointF(size * 0.50f, size * 0.16f), size * 0.022f, Color.FromArgb(200, 220, 120));
            DrawNode(g, new PointF(size * 0.34f, size * 0.21f), size * 0.018f, Color.FromArgb(140, 220, 150));
            DrawNode(g, new PointF(size * 0.66f, size * 0.21f), size * 0.018f, Color.FromArgb(200, 220, 120));

            var roof = new[]
            {
                new PointF(size * 0.44f, size * 0.24f),
                new PointF(size * 0.50f, size * 0.20f),
                new PointF(size * 0.56f, size * 0.24f)
            };
            using (var pen = new Pen(Color.FromArgb(150, 210, 120), size * 0.04f))
            {
                pen.LineJoin = LineJoin.Round;
                g.DrawPolygon(pen, roof);
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            btnLogin.Text = "Đang kiểm tra...";

            try
            {
                string user = (txtUser.Text ?? string.Empty).Trim();
                string pass = txtPass.Text ?? string.Empty;

                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrEmpty(pass))
                {
                    ToastNotification.Warning("Vui lòng nhập tên đăng nhập và mật khẩu!");
                    return;
                }

                string fullName = await userBLL.DangNhap(user, pass);
                var access = await userBLL.GetUserAccessAsync(user);
                int roleId = access.RoleId;

                ToastNotification.Success($"Xin chào {fullName}!");

                // Show loading overlay BEFORE hiding the form
                using (var loading = new SimpleLoadingOverlay(this, "Đang tải..."))
                {
                    await System.Threading.Tasks.Task.Delay(800); // Show loading screen
                }

                Hide();

                if (roleId == 1)
                {
                    using (var frm = new FrmAdminDashboard(user, access.UserId))
                    {
                        frm.FormClosed += (s, args) => Show();
                        frm.ShowDialog(this);
                    }
                }
                else
                {
                    using (var frm = new FrmStaffDashboard(user, fullName, access.BranchId, access.UserId))
                    {
                        frm.FormClosed += (s, args) => Show();
                        frm.ShowDialog(this);
                    }
                }

                UpdateOverlayPlaceholderVisibility(_userPlaceholderLabel, txtUser);
                UpdateOverlayPlaceholderVisibility(_passPlaceholderLabel, txtPass);
            }
            catch (Exception ex)
            {
                ToastNotification.Error(ex.Message);
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
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (btnLogin.Enabled)
                    btnLogin.PerformClick();
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
                var oldRegion = control.Region;
                control.Region = new Region(path);
                oldRegion?.Dispose();
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

        private Label SetupOverlayPlaceholder(TextBox textBox, Panel hostPanel, string placeholder, bool isPassword)
        {
            if (textBox == null || hostPanel == null) return null;

            if (isPassword)
                textBox.UseSystemPasswordChar = true;

            var label = new Label
            {
                Text = placeholder ?? string.Empty,
                Font = textBox.Font,
                ForeColor = Color.FromArgb(140, 140, 140),
                BackColor = hostPanel.BackColor,
                AutoSize = false,
                Location = textBox.Location,
                Size = textBox.Size,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.IBeam
            };

            label.Click += (s, e) => textBox.Focus();
            hostPanel.Controls.Add(label);
            label.BringToFront();

            textBox.TextChanged += (s, e) => UpdateOverlayPlaceholderVisibility(label, textBox);
            textBox.GotFocus += (s, e) => UpdateOverlayPlaceholderVisibility(label, textBox, forceHideWhenFocused: true);
            textBox.LostFocus += (s, e) => UpdateOverlayPlaceholderVisibility(label, textBox);

            UpdateOverlayPlaceholderVisibility(label, textBox);
            return label;
        }

        private static void UpdateOverlayPlaceholderVisibility(Label placeholderLabel, TextBox textBox, bool forceHideWhenFocused = false)
        {
            if (placeholderLabel == null || textBox == null) return;
            if (textBox.IsDisposed || placeholderLabel.IsDisposed) return;

            if (forceHideWhenFocused && textBox.Focused)
            {
                placeholderLabel.Visible = false;
                return;
            }

            placeholderLabel.Visible = string.IsNullOrEmpty(textBox.Text);
        }

        private void pnlContainer_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = pnlContainer.ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;

            // Chuyển toàn bộ nền phía sau thành màu trắng theo yêu cầu
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillRectangle(brush, rect);
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
            shadowRect.Inflate(12, 12); // Giảm độ lan tỏa của shadow

            for (int i = 8; i >= 1; i--)
            {
                int alpha = 4 + (i * 3); // Shadow nhẹ nhàng hơn
                var r = shadowRect;
                r.Inflate(-i, -i);
                using (var path = RoundedRectPath(r, radius + i))
                using (var brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0))) // Shadow màu đen trung tính
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private void DrawGlow(Graphics g, Rectangle bounds, int radius)
        {
            var glowRect = bounds;
            glowRect.Inflate(10, 10);

            for (int i = 6; i >= 1; i--)
            {
                int alpha = 12 + (i * 10);
                var r = glowRect;
                r.Inflate(-i, -i);
                using (var path = RoundedRectPath(r, radius + i))
                using (var pen = new Pen(Color.FromArgb(alpha, 120, 210, 255), 1.4f))
                {
                    g.DrawPath(pen, path);
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

            var rect = new Rectangle(0, 0, pnlForm.Width - 1, pnlForm.Height - 1);
            // Thêm viền nhẹ để phân biệt card trắng trên nền trắng
            using (var pen = new Pen(ModernTheme.Colors.Border, 1f))
            using (var path = RoundedRectPath(rect, 22))
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

            // Sử dụng Gradient nhẹ từ Primary sang PrimaryDark để tạo chiều sâu cho phần nội dung
            using (var brush = new LinearGradientBrush(rect, ModernTheme.Colors.Primary, ModernTheme.Colors.PrimaryDark, 135f))
            {
                g.FillRectangle(brush, rect);
            }

            // Vẽ một vài họa tiết trang trí chìm (subtle patterns)
            using (var pen = new Pen(Color.FromArgb(20, 255, 255, 255), 1f))
            {
                for (int i = 0; i < rect.Width; i += 20)
                {
                    g.DrawLine(pen, i, 0, i + 100, rect.Height);
                }
            }
        }

        private void EnsureStars(Rectangle rect)
        {
            // Đã loại bỏ để tối giản giao diện
        }

        private void DrawRightPanelWaves(Graphics g, Rectangle rect)
        {
            // Đã loại bỏ để tránh hiệu ứng "trồng lớp"
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
            using (var frm = new FrmForgotPassword())
            {
                frm.ShowDialog(this);
            }
        }

        private void lnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ToastNotification.Info("Hệ thống chỉ sử dụng tài khoản Admin. Vui lòng liên hệ quản trị viên để cấp tài khoản.");
        }
    }
}
