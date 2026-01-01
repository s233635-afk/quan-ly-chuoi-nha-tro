using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// ModernCard - Card container with shadow, hover effect, and optional click handling
    /// </summary>
    public class ModernCard : Panel
    {
        private int _elevation = 1;
        private bool _hoverable = false;
        private int _borderRadius = ModernTheme.BorderRadius.LG;
        private bool _isHovered = false;
        private Timer _hoverTimer;
        private float _currentElevation = 1f;

        #region Properties

        public int Elevation
        {
            get => _elevation;
            set
            {
                _elevation = Math.Max(0, Math.Min(4, value));
                _currentElevation = _elevation;
                Invalidate();
            }
        }

        public bool Hoverable
        {
            get => _hoverable;
            set
            {
                _hoverable = value;
                if (value)
                {
                    Cursor = Cursors.Hand;
                }
            }
        }

        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                _borderRadius = value;
                UpdateRegion();
                Invalidate();
            }
        }

        public event EventHandler OnCardClick;

        #endregion

        public ModernCard()
        {
            BackColor = ModernTheme.Colors.Background;
            Padding = new Padding(ModernTheme.Spacing.LG);
            BorderStyle = BorderStyle.None;
            
            // Enable double buffering for smooth animations
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            // Setup hover animation timer
            _hoverTimer = new Timer { Interval = 16 }; // ~60fps
            _hoverTimer.Tick += OnHoverTimerTick;

            WireUpEvents();
        }

        private void WireUpEvents()
        {
            MouseEnter += OnMouseEnterCard;
            MouseLeave += OnMouseLeaveCard;
            Click += OnClickCard;
        }

        private void OnMouseEnterCard(object sender, EventArgs e)
        {
            if (!_hoverable) return;

            _isHovered = true;
            _hoverTimer.Start();
        }

        private void OnMouseLeaveCard(object sender, EventArgs e)
        {
            if (!_hoverable) return;

            _isHovered = false;
            _hoverTimer.Start();
        }

        private void OnHoverTimerTick(object sender, EventArgs e)
        {
            // Animate elevation on hover (lift up effect)
            float target = _isHovered ? _elevation + 1.5f : _elevation;
            float diff = target - _currentElevation;

            if (Math.Abs(diff) < 0.05f)
            {
                _currentElevation = target;
                _hoverTimer.Stop();
            }
            else
            {
                _currentElevation += diff * 0.2f; // Smooth easing
            }

            Invalidate();
        }

        private void OnClickCard(object sender, EventArgs e)
        {
            OnCardClick?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw shadow
            if (_currentElevation > 0)
            {
                DrawShadow(g);
            }

            // Draw card background
            using (var path = GetRoundedRectPath(ClientRectangle, _borderRadius))
            {
                using (var brush = new SolidBrush(BackColor))
                {
                    g.FillPath(brush, path);
                }

                // Optional border
                using (var pen = new Pen(ModernTheme.Colors.Border, 1))
                {
                    g.DrawPath(pen, path);
                }
            }

            base.OnPaint(e);
        }

        private void DrawShadow(Graphics g)
        {
            int shadowSize = (int)(_currentElevation * 2);
            Color shadowColor = ModernTheme.Elevation.GetShadowColor((int)_currentElevation);

            // Simple shadow effect (blur would require more complex implementation)
            Rectangle shadowRect = ClientRectangle;
            shadowRect.Inflate(-1, -1);
            shadowRect.Offset(0, (int)_currentElevation);

            using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            {
                using (var path = GetRoundedRectPath(shadowRect, _borderRadius))
                {
                    g.FillPath(shadowBrush, path);
                }
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            rect.Width -= 1;
            rect.Height -= 1;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private void UpdateRegion()
        {
            using (var path = GetRoundedRectPath(ClientRectangle, _borderRadius))
            {
                Region = new Region(path);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hoverTimer?.Stop();
                _hoverTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
