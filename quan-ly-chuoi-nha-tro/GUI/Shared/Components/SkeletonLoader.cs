using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// SkeletonLoader - Loading placeholder with shimmer animation
    /// </summary>
    public class SkeletonLoader : Control
    {
        private Timer _animationTimer;
        private float _shimmerPosition = -1f;
        private SkeletonShape _shape = SkeletonShape.Rectangle;
        private int _rows = 1;

        public enum SkeletonShape
        {
            Rectangle,
            Circle,
            Text
        }

        #region Properties

        public SkeletonShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                Invalidate();
            }
        }

        public int Rows
        {
            get => _rows;
            set
            {
                _rows = Math.Max(1, value);
                Invalidate();
            }
        }

        #endregion

        public SkeletonLoader()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            Height = 40;

            // Shimmer animation timer
            _animationTimer = new Timer { Interval = 30 }; // ~33fps
            _animationTimer.Tick += OnAnimationTick;
            _animationTimer.Start();
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            _shimmerPosition += 0.05f;
            if (_shimmerPosition > 2f)
            {
                _shimmerPosition = -1f;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_rows == 1)
            {
                DrawSingleSkeleton(g, ClientRectangle);
            }
            else
            {
                DrawMultipleSkeletons(g);
            }
        }

        private void DrawSingleSkeleton(Graphics g, Rectangle bounds)
        {
            // Base skeleton color
            Color baseColor = ModernTheme.Colors.Surface;

            if (_shape == SkeletonShape.Circle)
            {
                // Draw circle
                int size = Math.Min(bounds.Width, bounds.Height);
                Rectangle circleRect = new Rectangle(bounds.X, bounds.Y, size, size);

                using (var brush = new SolidBrush(baseColor))
                {
                    g.FillEllipse(brush, circleRect);
                }

                DrawShimmer(g, circleRect, isCircle: true);
            }
            else
            {
                // Draw rectangle (with rounded corners)
                int radius = _shape == SkeletonShape.Text ? 4 : ModernTheme.BorderRadius.SM;

                using (var path = GetRoundedRectPath(bounds, radius))
                {
                    using (var brush = new SolidBrush(baseColor))
                    {
                        g.FillPath(brush, path);
                    }

                    DrawShimmer(g, bounds, isCircle: false);
                }
            }
        }

        private void DrawMultipleSkeletons(Graphics g)
        {
            int rowHeight = (Height - (_rows - 1) * 8) / _rows; // 8px gap between rows
            
            for (int i = 0; i < _rows; i++)
            {
                int y = i * (rowHeight + 8);
                int width = i == _rows - 1 ? Width * 2 / 3 : Width; // Last row is shorter
                Rectangle rowRect = new Rectangle(0, y, width, rowHeight);
                DrawSingleSkeleton(g, rowRect);
            }
        }

        private void DrawShimmer(Graphics g, Rectangle bounds, bool isCircle)
        {
            // Calculate shimmer position
            float shimmerX = bounds.X + (bounds.Width * _shimmerPosition);

            // Create shimmer gradient
            Color shimmerLight = ModernTheme.Gradient.Lighten(ModernTheme.Colors.Surface, 0.15f);
            Color shimmerBase = ModernTheme.Colors.Surface;

            Rectangle shimmerRect = new Rectangle(
                (int)(shimmerX - 50),
                bounds.Y,
                100,
                bounds.Height
            );

            // Only draw shimmer if it's within visible bounds
            if (shimmerRect.Right >= bounds.Left && shimmerRect.Left <= bounds.Right)
            {
                using (var brush = new LinearGradientBrush(
                    shimmerRect,
                    shimmerBase,
                    shimmerLight,
                    LinearGradientMode.Horizontal))
                {
                    ColorBlend colorBlend = new ColorBlend();
                    colorBlend.Colors = new Color[] { shimmerBase, shimmerLight, shimmerBase };
                    colorBlend.Positions = new float[] { 0f, 0.5f, 1f };
                    brush.InterpolationColors = colorBlend;

                    if (isCircle)
                    {
                        int size = Math.Min(bounds.Width, bounds.Height);
                        g.FillEllipse(brush, bounds.X, bounds.Y, size, size);
                    }
                    else
                    {
                        int radius = _shape == SkeletonShape.Text ? 4 : ModernTheme.BorderRadius.SM;
                        using (var path = GetRoundedRectPath(bounds, radius))
                        {
                            g.FillPath(brush, path);
                        }
                    }
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

            if (rect.Width < diameter || rect.Height < diameter)
            {
                path.AddRectangle(rect);
                return path;
            }

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
