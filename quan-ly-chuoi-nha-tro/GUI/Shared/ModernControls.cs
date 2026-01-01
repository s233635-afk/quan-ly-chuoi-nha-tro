using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// ModernButton - Button hiện đại với animation
    /// </summary>
    public class ModernButton : Button
    {
        private int _borderRadius = 18;
        public int BorderRadius 
        { 
            get => _borderRadius; 
            set { _borderRadius = value; UpdateRegion(); Invalidate(); } 
        }
        private Color _baseColor = UiKit.Primary;
        private bool _isHovered = false;
        private float _hoverProgress = 0f;
        private Timer _hoverTimer;
        private bool _isLoading = false;
        private int _loadingAngle = 0;
        private Timer _loadingTimer;
        private string _iconText = "";
        private IconPosition _iconPosition = IconPosition.Leading;
        private ButtonSize _size = ButtonSize.Medium;
        private bool _enableRipple = false;

        public enum IconPosition { Leading, Trailing }
        public enum ButtonSize { Small, Medium, Large }

        public Color BaseColor
        {
            get => _baseColor;
            set { _baseColor = value; Invalidate(); }
        }

        private Color? _hoverColor;
        public Color HoverColor
        {
            get => _hoverColor ?? ModernTheme.Gradient.Lighten(BaseColor, 0.1f);
            set => _hoverColor = value;
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                Enabled = !value;
                if (value)
                    _loadingTimer?.Start();
                else
                    _loadingTimer?.Stop();
                Invalidate();
            }
        }

        public string IconText
        {
            get => _iconText;
            set { _iconText = value; Invalidate(); }
        }

        public IconPosition IconPos
        {
            get => _iconPosition;
            set { _iconPosition = value; Invalidate(); }
        }

        public ButtonSize Size
        {
            get => _size;
            set
            {
                _size = value;
                UpdateSize();
                Invalidate();
            }
        }

        public bool EnableRipple
        {
            get => _enableRipple;
            set => _enableRipple = value;
        }

        public class ButtonParameters
        {
            public Color BaseColor { get; set; }
            public Color HoverColor { get; set; }
            public int BorderRadius { get; set; } = 18;
            public Font TextFont { get; set; }
            public Color TextColor { get; set; }
        }

        public ButtonParameters Parameters
        {
            set
            {
                if (value == null) return;
                BaseColor = value.BaseColor;
                if (value.HoverColor != Color.Empty) HoverColor = value.HoverColor;
                BorderRadius = value.BorderRadius;
                if (value.TextFont != null) Font = value.TextFont;
                if (value.TextColor != Color.Empty) ForeColor = value.TextColor;
            }
        }

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Height = 36;
            Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal);
            
            // Enable transparency and smooth redrawing
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            // Hover animation timer
            _hoverTimer = new Timer { Interval = 16 }; // ~60fps
            _hoverTimer.Tick += OnHoverTimerTick;

            // Loading animation timer
            _loadingTimer = new Timer { Interval = 50 };
            _loadingTimer.Tick += OnLoadingTimerTick;

            UpdateSize();
        }

        private void UpdateSize()
        {
            switch (_size)
            {
                case ButtonSize.Small:
                    Height = 28;
                    Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Small);
                    Padding = new Padding(12, 4, 12, 4);
                    break;
                case ButtonSize.Medium:
                    Height = 36;
                    Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal);
                    Padding = new Padding(16, 6, 16, 6);
                    break;
                case ButtonSize.Large:
                    Height = 44;
                    Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Large);
                    Padding = new Padding(20, 8, 20, 8);
                    break;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            _hoverTimer.Start();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _hoverTimer.Start();
        }

        private void OnHoverTimerTick(object sender, EventArgs e)
        {
            float target = _isHovered ? 1f : 0f;
            float diff = target - _hoverProgress;

            if (Math.Abs(diff) < 0.02f)
            {
                _hoverProgress = target;
                _hoverTimer.Stop();
            }
            else
            {
                _hoverProgress += diff * 0.3f; // Smooth easing
            }

            Invalidate();
        }

        private void OnLoadingTimerTick(object sender, EventArgs e)
        {
            _loadingAngle = (_loadingAngle + 15) % 360;
            Invalidate();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UpdateRegion();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            UiKit.SetRoundedRegion(this, BorderRadius);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Paint parent background
            InvokePaintBackground(this, pevent);

            var rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            // Interpolate color based on hover progress
            var color = InterpolateColor(BaseColor, HoverColor, _hoverProgress);

            using (var path = UiKit.GetRoundPath(rect, BorderRadius))
            {
                using (var brush = new SolidBrush(color))
                {
                    g.FillPath(brush, path);
                }

                // Draw loading spinner or content
                if (_isLoading)
                {
                    DrawLoadingSpinner(g, ClientRectangle);
                }
                else
                {
                    DrawContent(g, ClientRectangle);
                }
            }
        }

        private void DrawContent(Graphics g, Rectangle bounds)
        {
            string displayText = Text;
            int iconSize = 16;
            int iconPadding = 6;

            if (!string.IsNullOrEmpty(_iconText))
            {
                // Calculate layout with icon
                SizeF textSize = g.MeasureString(displayText, Font);
                int totalWidth = iconSize + iconPadding + (int)textSize.Width;
                int startX = (bounds.Width - totalWidth) / 2;
                int iconY = (bounds.Height - iconSize) / 2;

                if (_iconPosition == IconPosition.Leading)
                {
                    // Draw icon on left
                    TextRenderer.DrawText(g, _iconText, new Font("Segoe UI Emoji", 12f), 
                        new Rectangle(startX, iconY, iconSize, iconSize), ForeColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                    // Draw text
                    TextRenderer.DrawText(g, displayText, Font, 
                        new Rectangle(startX + iconSize + iconPadding, 0, bounds.Width, bounds.Height),
                        ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                }
                else
                {
                    // Draw text
                    TextRenderer.DrawText(g, displayText, Font, 
                        new Rectangle(startX, 0, (int)textSize.Width, bounds.Height),
                        ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                    // Draw icon on right
                    TextRenderer.DrawText(g, _iconText, new Font("Segoe UI Emoji", 12f), 
                        new Rectangle(startX + (int)textSize.Width + iconPadding, iconY, iconSize, iconSize),
                        ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                }
            }
            else
            {
                // No icon, center text
                TextRenderer.DrawText(g, displayText, Font, bounds, ForeColor, 
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private void DrawLoadingSpinner(Graphics g, Rectangle bounds)
        {
            int spinnerSize = 16;
            int centerX = bounds.Width / 2;
            int centerY = bounds.Height / 2;
            Rectangle spinnerRect = new Rectangle(
                centerX - spinnerSize / 2,
                centerY - spinnerSize / 2,
                spinnerSize,
                spinnerSize
            );

            using (var pen = new Pen(ForeColor, 2))
            {
                g.DrawArc(pen, spinnerRect, _loadingAngle, 270);
            }
        }

        private Color InterpolateColor(Color c1, Color c2, float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return Color.FromArgb(
                (int)(c1.R + (c2.R - c1.R) * t),
                (int)(c1.G + (c2.G - c1.G) * t),
                (int)(c1.B + (c2.B - c1.B) * t)
            );
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hoverTimer?.Stop();
                _hoverTimer?.Dispose();
                _loadingTimer?.Stop();
                _loadingTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// ModernCard - Card hiện đại với shadow
    /// </summary>
    public class ModernCard : Panel
    {
        public ModernCard()
        {
            BackColor = ModernTheme.Colors.Background;
            BorderStyle = BorderStyle.FixedSingle;
            Padding = new Padding(ModernTheme.Spacing.LG);
        }
    }

    /// <summary>
    /// ModernStatCard - Card hiển thị thống kê
    /// </summary>
    public class ModernStatCard : Panel
    {
        private Label _titleLabel;
        private Label _valueLabel;
        private Label _subtitleLabel;
        private Label _iconLabel;
        private Label _trendLabel;
        private Color _accentColor;
        private string _icon = "";
        private decimal? _trendValue = null;
        private TrendDirection? _trend = null;
        private bool _isHovered = false;

        public enum TrendDirection { Up, Down, Neutral }

        public ModernStatCard(string title, string value, string subtitle, Color accentColor)
        {
            _accentColor = accentColor;
            BackColor = Color.White;
            BorderStyle = BorderStyle.None;
            Width = 200;
            Height = 120;
            Padding = new Padding(14, ModernTheme.Spacing.MD, ModernTheme.Spacing.MD, ModernTheme.Spacing.MD);
            Cursor = Cursors.Hand;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            // Accent bar (left edge)
            var accentBar = new Panel
            {
                BackColor = accentColor,
                Width = 4,
                Dock = DockStyle.Left
            };

            // Icon label (top-right)
            _iconLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 24f),
                ForeColor = ModernTheme.Gradient.WithOpacity(accentColor, 0.3f),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Visible = false
            };
            
            // Position will be set after card resize
            Resize += (s, e) =>
            {
                if (_iconLabel != null && !string.IsNullOrEmpty(_iconLabel.Text))
                {
                    _iconLabel.Location = new Point(Width - _iconLabel.Width - 15, 10);
                }
            };

            // Title label
            _titleLabel = new Label
            {
                Text = title,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Small),
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 20,
                BackColor = Color.Transparent
            };

            // Value label
            _valueLabel = new Label
            {
                Text = value,  Font = ModernTheme.Fonts.Bold(20),
                ForeColor = accentColor,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // Trend label (inline with subtitle)
            _trendLabel = new Label
            {
                AutoSize = true,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Small),
                Location = new Point(14, Height - 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Visible = false,
                BackColor = Color.Transparent
            };

            // Subtitle label
            _subtitleLabel = new Label
            {
                Text = subtitle,
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Tiny),
                ForeColor = ModernTheme.Colors.TextTertiary,
                AutoSize = false,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            Controls.Add(_subtitleLabel);
            Controls.Add(_valueLabel);
            Controls.Add(_titleLabel);
            Controls.Add(_iconLabel);
            Controls.Add(_trendLabel);
            Controls.Add(accentBar);

            // Forward mouse events from all child controls to card
            MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
            
            // Attach same events to all children
            foreach (Control child in Controls)
            {
                child.MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
                child.MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
                child.Click += (s, e) => OnClick(e);
            }
        }

        public string Title
        {
            get => _titleLabel.Text;
            set => _titleLabel.Text = value;
        }

        public string Value
        {
            get => _valueLabel.Text;
            set => _valueLabel.Text = value;
        }

        public string Subtitle
        {
            get => _subtitleLabel.Text;
            set => _subtitleLabel.Text = value;
        }

        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                _accentColor = value;
                _valueLabel.ForeColor = value;
                if (_iconLabel != null)
                {
                    _iconLabel.ForeColor = ModernTheme.Gradient.WithOpacity(value, 0.3f);
                }
            }
        }

        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                if (_iconLabel != null)
                {
                    _iconLabel.Text = value;
                    _iconLabel.Visible = !string.IsNullOrEmpty(value);
                }
            }
        }

        public decimal? TrendValue
        {
            get => _trendValue;
            set
            {
                _trendValue = value;
                UpdateTrendLabel();
            }
        }

        public TrendDirection? Trend
        {
            get => _trend;
            set
            {
                _trend = value;
                UpdateTrendLabel();
            }
        }

        public event EventHandler OnCardClick;

        private void UpdateTrendLabel()
        {
            if (_trend.HasValue && _trendValue.HasValue)
            {
                string arrow = _trend.Value switch
                {
                    TrendDirection.Up => "↑",
                    TrendDirection.Down => "↓",
                    _ => "→"
                };

                Color trendColor = _trend.Value switch
                {
                    TrendDirection.Up => ModernTheme.Colors.Success,
                    TrendDirection.Down => ModernTheme.Colors.Error,
                    _ => ModernTheme.Colors.TextSecondary
                };

                _trendLabel.Text = $"{arrow} {_trendValue:+0.##;-0.##;0}%";
                _trendLabel.ForeColor = trendColor;
                _trendLabel.Visible = true;
            }
            else
            {
                _trendLabel.Visible = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw subtle shadow for elevation
            using (var shadowBrush = new SolidBrush(Color.FromArgb(8, 0, 0, 0)))
            {
                var shadowRect = new Rectangle(2, 2, Width - 2, Height - 2);
                e.Graphics.FillRectangle(shadowBrush, shadowRect);
            }

            // Draw white background with subtle border
            using (var bgBrush = new SolidBrush(Color.White))
            using (var borderPen = new Pen(Color.FromArgb(224, 231, 240), 1))
            {
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                e.Graphics.FillRectangle(bgBrush, rect);
                e.Graphics.DrawRectangle(borderPen, rect);
            }

            // Hover effect - blue border
            if (_isHovered)
            {
                using (var pen = new Pen(ModernTheme.Colors.Primary, 2))
                {
                    Rectangle borderRect = ClientRectangle;
                    borderRect.Inflate(-1, -1);
                    e.Graphics.DrawRectangle(pen, borderRect);
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            OnCardClick?.Invoke(this, e);
        }
    }

    /// <summary>
    /// ModernTextBox - TextBox hiện đại
    /// </summary>
    public class ModernTextBox : TextBox
    {
        public ModernTextBox()
        {
            BackColor = ModernTheme.Colors.Background;
            ForeColor = ModernTheme.Colors.TextPrimary;
            Font = ModernTheme.Fonts.NormalFont;
            BorderStyle = BorderStyle.FixedSingle;
            Height = 32;
            Margin = new Padding(ModernTheme.Spacing.SM);
        }
    }

    /// <summary>
    /// ModernComboBox - ComboBox hiện đại
    /// </summary>
    public class ModernComboBox : ComboBox
    {
        public ModernComboBox()
        {
            BackColor = ModernTheme.Colors.Background;
            ForeColor = ModernTheme.Colors.TextPrimary;
            Font = ModernTheme.Fonts.NormalFont;
            DropDownStyle = ComboBoxStyle.DropDownList;
            Height = 32;
            Margin = new Padding(ModernTheme.Spacing.SM);
        }
    }

    /// <summary>
    /// ModernLabel - Label hiện đại
    /// </summary>
    public class ModernLabel : Label
    {
        public ModernLabel(float fontSize = ModernTheme.Fonts.Normal, bool isBold = false)
        {
            Font = isBold ? ModernTheme.Fonts.Bold(fontSize) : ModernTheme.Fonts.Regular(fontSize);
            ForeColor = ModernTheme.Colors.TextPrimary;
            AutoSize = true;
        }
    }

    /// <summary>
    /// ModernDataGridView - DataGridView hiện đại
    /// </summary>
    public class ModernDataGridView : DataGridView
    {
        public ModernDataGridView()
        {
            ModernTheme.StyleDataGridView(this);
            ReadOnly = true;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect = false;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }

    /// <summary>
    /// ModernPanel - Panel hiện đại
    /// </summary>
    public class ModernPanel : Panel
    {
        public ModernPanel()
        {
            BackColor = ModernTheme.Colors.Background;
            BorderStyle = BorderStyle.FixedSingle;
        }
    }

    /// <summary>
    /// ModernProgressBar - Progress bar hiện đại
    /// </summary>
    public class ModernProgressBar : ProgressBar
    {
        public ModernProgressBar()
        {
            Height = 8;
            SetStyle(ControlStyles.UserPaint, true);
            ForeColor = ModernTheme.Colors.Primary;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = ClientRectangle;
            e.Graphics.FillRectangle(new SolidBrush(ModernTheme.Colors.Surface), rec);

            rec.Width = (int)(rec.Width * ((double)Value / Maximum));
            e.Graphics.FillRectangle(new SolidBrush(ForeColor), rec);
        }
    }
}
