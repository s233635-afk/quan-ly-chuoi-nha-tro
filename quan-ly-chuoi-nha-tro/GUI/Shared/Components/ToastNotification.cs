using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Modern Toast Notification System
    /// Shows non-blocking notifications that auto-dismiss
    /// </summary>
    public class ToastNotification : Form
    {
        public enum ToastType { Success, Info, Warning, Error }

        private static readonly List<ToastNotification> _activeToasts = new List<ToastNotification>();
        private static readonly object _lock = new object();

        private readonly Timer _timer;
        private readonly Timer _fadeTimer;
        private float _opacity = 0f;
        private bool _isClosing = false;
        private readonly ToastType _type;
        private readonly string _message;
        private readonly string _title;

        private ToastNotification(string message, string title, ToastType type, int duration)
        {
            _message = message;
            _title = title;
            _type = type;

            InitializeForm();

            // Auto-close timer
            _timer = new Timer { Interval = duration };
            _timer.Tick += (s, e) => { _timer.Stop(); FadeOut(); };

            // Fade animation timer
            _fadeTimer = new Timer { Interval = 16 }; // ~60fps
            _fadeTimer.Tick += OnFadeTimerTick;
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Size = new Size(320, 80);
            BackColor = GetBackgroundColor(_type);
            Opacity = 0;
            Padding = new Padding(12);

            // Enable double buffering
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            Paint += OnPaint;
            Click += (s, e) => FadeOut();
            MouseEnter += (s, e) => _timer.Stop();
            MouseLeave += (s, e) => { if (!_isClosing) _timer.Start(); };
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw rounded background
            using (var path = CreateRoundedRect(ClientRectangle, 8))
            using (var brush = new SolidBrush(GetBackgroundColor(_type)))
            {
                g.FillPath(brush, path);
            }

            // Draw left accent bar
            using (var brush = new SolidBrush(GetAccentColor(_type)))
            {
                g.FillRectangle(brush, 0, 0, 5, Height);
            }

            // Draw icon
            var iconRect = new Rectangle(16, (Height - 24) / 2, 24, 24);
            DrawIcon(g, iconRect, _type);

            // Draw title
            if (!string.IsNullOrEmpty(_title))
            {
                using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                using (var brush = new SolidBrush(GetTextColor(_type)))
                {
                    g.DrawString(_title, font, brush, 48, 12);
                }
            }

            // Draw message
            using (var font = new Font("Segoe UI", 10, FontStyle.Regular))
            using (var brush = new SolidBrush(GetTextColor(_type)))
            {
                var msgY = string.IsNullOrEmpty(_title) ? (Height - 20) / 2 : 36;
                var msgRect = new RectangleF(48, msgY, Width - 60, Height - msgY - 8);
                g.DrawString(_message, font, brush, msgRect);
            }

            // Draw close button
            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(150, GetTextColor(_type))))
            {
                g.DrawString("×", font, brush, Width - 24, 8);
            }
        }

        private void DrawIcon(Graphics g, Rectangle rect, ToastType type)
        {
            var iconColor = GetAccentColor(type);
            using (var pen = new Pen(iconColor, 2))
            using (var brush = new SolidBrush(iconColor))
            {
                var cx = rect.X + rect.Width / 2;
                var cy = rect.Y + rect.Height / 2;

                switch (type)
                {
                    case ToastType.Success:
                        // Checkmark
                        g.DrawEllipse(pen, rect);
                        var points = new[] {
                            new Point(cx - 6, cy),
                            new Point(cx - 2, cy + 5),
                            new Point(cx + 6, cy - 4)
                        };
                        g.DrawLines(new Pen(iconColor, 2.5f), points);
                        break;

                    case ToastType.Error:
                        // X mark
                        g.DrawEllipse(pen, rect);
                        g.DrawLine(pen, cx - 5, cy - 5, cx + 5, cy + 5);
                        g.DrawLine(pen, cx + 5, cy - 5, cx - 5, cy + 5);
                        break;

                    case ToastType.Warning:
                        // Triangle with exclamation
                        var trianglePoints = new[] {
                            new Point(cx, rect.Y + 2),
                            new Point(rect.Right - 2, rect.Bottom - 2),
                            new Point(rect.X + 2, rect.Bottom - 2)
                        };
                        g.DrawPolygon(pen, trianglePoints);
                        g.FillEllipse(brush, cx - 2, cy + 4, 4, 4);
                        g.DrawLine(pen, cx, cy - 6, cx, cy + 1);
                        break;

                    case ToastType.Info:
                        // Circle with i
                        g.DrawEllipse(pen, rect);
                        g.FillEllipse(brush, cx - 2, cy - 6, 4, 4);
                        g.DrawLine(pen, cx, cy - 1, cx, cy + 7);
                        break;
                }
            }
        }

        private GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Color GetBackgroundColor(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success: return Color.FromArgb(237, 247, 237);
                case ToastType.Error: return Color.FromArgb(253, 237, 237);
                case ToastType.Warning: return Color.FromArgb(255, 249, 231);
                case ToastType.Info: return Color.FromArgb(227, 242, 253);
                default: return Color.White;
            }
        }

        private static Color GetAccentColor(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success: return Color.FromArgb(46, 125, 50);
                case ToastType.Error: return Color.FromArgb(211, 47, 47);
                case ToastType.Warning: return Color.FromArgb(245, 124, 0);
                case ToastType.Info: return Color.FromArgb(25, 103, 210);
                default: return Color.Gray;
            }
        }

        private static Color GetTextColor(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success: return Color.FromArgb(30, 70, 32);
                case ToastType.Error: return Color.FromArgb(95, 33, 32);
                case ToastType.Warning: return Color.FromArgb(102, 60, 0);
                case ToastType.Info: return Color.FromArgb(13, 60, 97);
                default: return Color.Black;
            }
        }

        private void OnFadeTimerTick(object sender, EventArgs e)
        {
            if (_isClosing)
            {
                _opacity -= 0.1f;
                if (_opacity <= 0)
                {
                    _fadeTimer.Stop();
                    CloseAndRemove();
                }
            }
            else
            {
                _opacity += 0.15f;
                if (_opacity >= 1f)
                {
                    _opacity = 1f;
                    _fadeTimer.Stop();
                    _timer.Start();
                }
            }
            Opacity = _opacity;
        }

        private void FadeOut()
        {
            _isClosing = true;
            _timer.Stop();
            _fadeTimer.Start();
        }

        private void CloseAndRemove()
        {
            lock (_lock)
            {
                _activeToasts.Remove(this);
                RepositionToasts();
            }
            Close();
            Dispose();
        }

        private static void RepositionToasts()
        {
            var screen = Screen.PrimaryScreen.WorkingArea;
            var y = screen.Bottom - 20;

            for (int i = _activeToasts.Count - 1; i >= 0; i--)
            {
                var toast = _activeToasts[i];
                y -= toast.Height + 10;
                toast.Location = new Point(screen.Right - toast.Width - 20, y);
            }
        }

        /// <summary>
        /// Show a toast notification
        /// </summary>
        public static void Show(string message, ToastType type = ToastType.Info, int duration = 3000)
        {
            Show(message, null, type, duration);
        }

        /// <summary>
        /// Show a toast notification with title
        /// </summary>
        public static void Show(string message, string title, ToastType type = ToastType.Info, int duration = 3000)
        {
            var toast = new ToastNotification(message, title, type, duration);

            lock (_lock)
            {
                _activeToasts.Add(toast);
            }

            var screen = Screen.PrimaryScreen.WorkingArea;
            toast.Location = new Point(screen.Right - toast.Width - 20, screen.Bottom);

            toast.Show();
            RepositionToasts();
            toast._fadeTimer.Start();
        }

        /// <summary>
        /// Show success toast
        /// </summary>
        public static void Success(string message, string title = "Thành công")
        {
            Show(message, title, ToastType.Success);
        }

        /// <summary>
        /// Show error toast
        /// </summary>
        public static void Error(string message, string title = "Lỗi")
        {
            Show(message, title, ToastType.Error, 5000);
        }

        /// <summary>
        /// Show warning toast
        /// </summary>
        public static void Warning(string message, string title = "Cảnh báo")
        {
            Show(message, title, ToastType.Warning, 4000);
        }

        /// <summary>
        /// Show info toast
        /// </summary>
        public static void Info(string message, string title = "Thông báo")
        {
            Show(message, title, ToastType.Info);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
                _fadeTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
