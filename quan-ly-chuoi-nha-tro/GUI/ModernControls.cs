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
        private Color _hoverColor;
        private Color _pressedColor;
        private Color _normalColor;
        private bool _isHovered;

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Height = 36;
            Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal);
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        public Color HoverColor
        {
            get => _hoverColor == Color.Empty ? ControlPaint.Dark(BackColor, 0.1f) : _hoverColor;
            set => _hoverColor = value;
        }

        public Color PressedColor
        {
            get => _pressedColor == Color.Empty ? ControlPaint.Dark(BackColor, 0.2f) : _pressedColor;
            set => _pressedColor = value;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            _normalColor = BackColor;
            BackColor = HoverColor;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            BackColor = _normalColor;
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
        private Color _accentColor;

        public ModernStatCard(string title, string value, string subtitle, Color accentColor)
        {
            _accentColor = accentColor;
            BackColor = ModernTheme.Colors.Background;
            BorderStyle = BorderStyle.FixedSingle;
            Width = 200;
            Height = 120;
            Padding = new Padding(ModernTheme.Spacing.MD);

            // Title label
            _titleLabel = new Label
            {
                Text = title,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Small),
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 20
            };

            // Value label
            _valueLabel = new Label
            {
                Text = value,
                Font = ModernTheme.Fonts.Bold(20),
                ForeColor = accentColor,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Subtitle label
            _subtitleLabel = new Label
            {
                Text = subtitle,
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Tiny),
                ForeColor = ModernTheme.Colors.TextTertiary,
                AutoSize = false,
                Dock = DockStyle.Fill
            };

            Controls.Add(_subtitleLabel);
            Controls.Add(_valueLabel);
            Controls.Add(_titleLabel);
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
            }
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
