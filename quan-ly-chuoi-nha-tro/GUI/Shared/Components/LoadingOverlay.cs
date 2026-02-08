using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Modern Loading Overlay with animated spinner
    /// Use to show loading state during async operations
    /// </summary>
    public class LoadingOverlay : Panel
    {
        private readonly Timer _animationTimer;
        private float _rotation = 0f;
        private readonly Label _messageLabel;
        private readonly Panel _spinnerPanel;

        public string Message
        {
            get => _messageLabel.Text;
            set => _messageLabel.Text = value;
        }

        public LoadingOverlay()
        {
            // Panel setup
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(180, 255, 255, 255);
            Visible = false;

            // Enable double buffering
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            // Center container
            var container = new Panel
            {
                Size = new Size(200, 120),
                BackColor = Color.White,
                Anchor = AnchorStyles.None
            };
            container.Paint += OnContainerPaint;

            // Spinner panel
            _spinnerPanel = new Panel
            {
                Size = new Size(50, 50),
                BackColor = Color.Transparent,
                Location = new Point(75, 15)
            };
            _spinnerPanel.Paint += OnSpinnerPaint;
            container.Controls.Add(_spinnerPanel);

            // Message label
            _messageLabel = new Label
            {
                Text = "Đang tải...",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 60, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(180, 30),
                Location = new Point(10, 75)
            };
            container.Controls.Add(_messageLabel);

            Controls.Add(container);

            // Animation timer
            _animationTimer = new Timer { Interval = 16 }; // ~60fps
            _animationTimer.Tick += OnAnimationTick;

            // Center container on resize
            Resize += (s, e) => CenterContainer(container);
            HandleCreated += (s, e) => CenterContainer(container);
        }

        private void CenterContainer(Panel container)
        {
            container.Location = new Point(
                (Width - container.Width) / 2,
                (Height - container.Height) / 2
            );
        }

        private void OnContainerPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var panel = sender as Panel;
            var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);

            // Draw shadow
            for (int i = 4; i > 0; i--)
            {
                using (var pen = new Pen(Color.FromArgb(10 * i, 0, 0, 0)))
                {
                    var shadowRect = new Rectangle(rect.X + i, rect.Y + i, rect.Width, rect.Height);
                    using (var path = CreateRoundedRect(shadowRect, 12))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            // Draw background
            using (var path = CreateRoundedRect(rect, 12))
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillPath(brush, path);
            }

            // Draw border
            using (var path = CreateRoundedRect(rect, 12))
            using (var pen = new Pen(Color.FromArgb(230, 230, 230)))
            {
                g.DrawPath(pen, path);
            }
        }

        private void OnSpinnerPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var panel = sender as Panel;
            var cx = panel.Width / 2f;
            var cy = panel.Height / 2f;
            var radius = Math.Min(cx, cy) - 4;

            // Save state and rotate
            g.TranslateTransform(cx, cy);
            g.RotateTransform(_rotation);

            // Draw spinner segments
            var segmentCount = 12;
            var colors = new Color[]
            {
                Color.FromArgb(0, 120, 215),
                Color.FromArgb(40, 140, 220),
                Color.FromArgb(80, 160, 225),
                Color.FromArgb(120, 180, 230),
                Color.FromArgb(160, 200, 235),
                Color.FromArgb(200, 220, 240),
                Color.FromArgb(220, 230, 245),
                Color.FromArgb(235, 240, 248),
                Color.FromArgb(240, 245, 250),
                Color.FromArgb(245, 248, 252),
                Color.FromArgb(248, 250, 253),
                Color.FromArgb(250, 252, 254)
            };

            for (int i = 0; i < segmentCount; i++)
            {
                var angle = (360f / segmentCount) * i;
                var rad = angle * Math.PI / 180;

                var x1 = (float)(Math.Cos(rad) * (radius - 8));
                var y1 = (float)(Math.Sin(rad) * (radius - 8));
                var x2 = (float)(Math.Cos(rad) * radius);
                var y2 = (float)(Math.Sin(rad) * radius);

                var colorIndex = i % colors.Length;
                using (var pen = new Pen(colors[colorIndex], 3) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }

            g.ResetTransform();
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            _rotation = (_rotation + 8) % 360;
            _spinnerPanel.Invalidate();
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

        /// <summary>
        /// Show the loading overlay
        /// </summary>
        public void ShowLoading(string message = "Đang tải...")
        {
            Message = message;
            BringToFront();
            Visible = true;
            _animationTimer.Start();
        }

        /// <summary>
        /// Hide the loading overlay
        /// </summary>
        public void HideLoading()
        {
            _animationTimer.Stop();
            Visible = false;
        }

        /// <summary>
        /// Execute an async operation with loading overlay
        /// </summary>
        public async System.Threading.Tasks.Task<T> ExecuteWithLoadingAsync<T>(
            System.Func<System.Threading.Tasks.Task<T>> operation,
            string message = "Đang xử lý...")
        {
            try
            {
                ShowLoading(message);
                return await operation();
            }
            finally
            {
                HideLoading();
            }
        }

        /// <summary>
        /// Execute an async operation with loading overlay (no return value)
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteWithLoadingAsync(
            System.Func<System.Threading.Tasks.Task> operation,
            string message = "Đang xử lý...")
        {
            try
            {
                ShowLoading(message);
                await operation();
            }
            finally
            {
                HideLoading();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
