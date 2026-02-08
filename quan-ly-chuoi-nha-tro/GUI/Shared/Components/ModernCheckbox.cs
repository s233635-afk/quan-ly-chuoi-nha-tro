using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// ModernCheckbox - Checkbox with custom styling, checkmark animation, and ripple effect
    /// </summary>
    public class ModernCheckbox : CheckBox
    {
        private bool _isAnimating = false;
        private float _checkProgress = 0f;
        private Timer _animationTimer;
        private Color _checkedColor = ModernTheme.Colors.Primary;
        private Color _uncheckedColor = ModernTheme.Colors.Border;

        #region Properties

        public Color CheckedColor
        {
            get => _checkedColor;
            set
            {
                _checkedColor = value;
                Invalidate();
            }
        }

        public Color UncheckedColor
        {
            get => _uncheckedColor;
            set
            {
                _uncheckedColor = value;
                Invalidate();
            }
        }

        #endregion

        public ModernCheckbox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = ModernTheme.Fonts.NormalFont;
            ForeColor = ModernTheme.Colors.TextPrimary;
            Cursor = Cursors.Hand;
            AutoSize = false;
            Height = 24;

            // Animation timer
            _animationTimer = new Timer { Interval = 16 }; // ~60fps
            _animationTimer.Tick += OnAnimationTick;

            CheckedChanged += OnCheckedChangedEvent;
        }

        private void OnCheckedChangedEvent(object sender, EventArgs e)
        {
            StartAnimation();
        }

        private void StartAnimation()
        {
            _isAnimating = true;
            _animationTimer.Start();
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            float target = Checked ? 1f : 0f;
            float diff = target - _checkProgress;

            if (Math.Abs(diff) < 0.02f)
            {
                _checkProgress = target;
                _isAnimating = false;
                _animationTimer.Stop();
            }
            else
            {
                _checkProgress += diff * 0.25f; // Smooth easing
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Draw background
            g.Clear(Parent?.BackColor ?? ModernTheme.Colors.Background);

            // Checkbox box dimensions
            int boxSize = 18;
            int boxX = 2;
            int boxY = (Height - boxSize) / 2;
            Rectangle boxRect = new Rectangle(boxX, boxY, boxSize, boxSize);

            // Determine colors based on state
            Color boxColor = Enabled ? 
                (Checked ? _checkedColor : _uncheckedColor) : 
                ModernTheme.Colors.TextDisabled;

            Color fillColor = Checked ? boxColor : ModernTheme.Colors.Background;

            // Draw checkbox box with rounded corners
            using (var path = GetRoundedRectPath(boxRect, 3))
            {
                // Fill
                using (var brush = new SolidBrush(fillColor))
                {
                    g.FillPath(brush, path);
                }

                // Border
                using (var pen = new Pen(boxColor, 2))
                {
                    g.DrawPath(pen, path);
                }
            }

            // Draw checkmark with animation
            if (_checkProgress > 0)
            {
                DrawCheckmark(g, boxRect, _checkProgress);
            }

            // Draw text
            if (!string.IsNullOrEmpty(Text))
            {
                Rectangle textRect = new Rectangle(
                    boxX + boxSize + 8,
                    0,
                    Width - (boxX + boxSize + 8),
                    Height
                );

                Color textColor = Enabled ? ForeColor : ModernTheme.Colors.TextDisabled;

                TextRenderer.DrawText(g, Text, Font, textRect, textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawCheckmark(Graphics g, Rectangle boxRect, float progress)
        {
            // Checkmark points
            PointF[] checkPoints = new PointF[]
            {
                new PointF(boxRect.Left + 4, boxRect.Top + 9),
                new PointF(boxRect.Left + 7, boxRect.Top + 12),
                new PointF(boxRect.Left + 14, boxRect.Top + 5)
            };

            // Animate checkmark by trimming the path based on progress
            using (var pen = new Pen(ModernTheme.Colors.TextInverse, 2))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                if (progress >= 0.5f)
                {
                    // Draw first segment (animated)
                    float seg1Progress = Math.Min(1f, (progress - 0f) / 0.5f);
                    PointF seg1End = InterpolatePoint(checkPoints[0], checkPoints[1], seg1Progress);
                    g.DrawLine(pen, checkPoints[0], seg1End);
                }

                if (progress >= 0.5f)
                {
                    // Draw second segment (animated)
                    float seg2Progress = Math.Min(1f, (progress - 0.5f) / 0.5f);
                    PointF seg2End = InterpolatePoint(checkPoints[1], checkPoints[2], seg2Progress);
                    g.DrawLine(pen, checkPoints[1], seg2End);
                }
            }
        }

        private PointF InterpolatePoint(PointF p1, PointF p2, float t)
        {
            return new PointF(
                p1.X + (p2.X - p1.X) * t,
                p1.Y + (p2.Y - p1.Y) * t
            );
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
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
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
