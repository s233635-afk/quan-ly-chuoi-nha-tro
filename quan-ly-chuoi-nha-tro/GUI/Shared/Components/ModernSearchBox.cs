using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// ModernSearchBox - Search box with built-in placeholder, clear button, and debounce
    /// </summary>
    public class ModernSearchBox : Panel
    {
        private TextBox _textBox;
        private Button _clearButton;
        private Label _iconLabel;
        private Timer _debounceTimer;
        private string _placeholderText = "Tìm kiếm...";
        private int _debounceMs = 300;
        private bool _isPlaceholderActive = true;

        public event EventHandler TextChanged;
        public event EventHandler SearchTriggered;

        public ModernSearchBox()
        {
            InitializeComponent();
            _debounceTimer = new Timer { Interval = _debounceMs };
            _debounceTimer.Tick += OnDebounceTimerTick;
        }

        private void InitializeComponent()
        {
            // Container setup with rounded corners
            Height = 32; // Reduced from 40 to 32 for better layout compatibility
            MinimumSize = new Size(100, 28);
            BackColor = Color.White;
            BorderStyle = BorderStyle.None;
            Padding = new Padding(10, 6, 10, 6); // Reduced padding for compact design
            
            // Enable custom painting for rounded corners and borders
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            // Search icon
            _iconLabel = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI Emoji", 10f), // Smaller icon for compact design
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = false,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 28, // Reduced from 32
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };

            // TextBox
            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f), // Reduced from 11f for compact design
                ForeColor = ModernTheme.Colors.TextSecondary,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Text = _placeholderText,
                Margin = new Padding(0)
            };
            _textBox.TextChanged += OnTextBoxTextChanged;
            _textBox.GotFocus += OnTextBoxGotFocus;
            _textBox.LostFocus += OnTextBoxLostFocus;
            _textBox.KeyDown += OnTextBoxKeyDown;

            // Clear button with better styling
            _clearButton = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold), // Reduced from 11f
                ForeColor = ModernTheme.Colors.TextSecondary,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Width = 24, // Reduced from 28
                Height = 24, // Reduced from 28
                Cursor = Cursors.Hand,
                Visible = false,
                TabStop = false
            };
            _clearButton.FlatAppearance.BorderSize = 0;
            _clearButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            _clearButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(220, 220, 220);
            _clearButton.Click += OnClearButtonClick;

            // Add controls in order
            Controls.Add(_textBox);
            Controls.Add(_clearButton);
            Controls.Add(_iconLabel);

            // Custom painting for rounded borders
            Paint += OnSearchBoxPaint;
        }
        
        private void OnSearchBoxPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var borderRadius = 8;
            
            // Draw background with rounded corners
            using (var path = GetRoundedRectPath(rect, borderRadius))
            using (var bgBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(bgBrush, path);
            }
            
            // Draw border
            Color borderColor;
            int borderWidth;
            
            if (_textBox.Focused)
            {
                borderColor = ModernTheme.Colors.Primary;
                borderWidth = 2;
            }
            else
            {
                borderColor = Color.FromArgb(180, 190, 200);
                borderWidth = 2;
            }
            
            using (var path = GetRoundedRectPath(rect, borderRadius))
            using (var pen = new Pen(borderColor, borderWidth))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }
        
        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
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

        public string PlaceholderText
        {
            get => _placeholderText;
            set
            {
                _placeholderText = value;
                if (_isPlaceholderActive)
                {
                    _textBox.Text = _placeholderText;
                }
            }
        }

        public int DebounceMs
        {
            get => _debounceMs;
            set
            {
                _debounceMs = Math.Max(0, value);
                _debounceTimer.Interval = _debounceMs;
            }
        }

        public bool ShowClearButton { get; set; } = true;
        public bool ShowSearchIcon { get; set; } = true;

        public new string Text
        {
            get => _isPlaceholderActive ? string.Empty : _textBox.Text;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _textBox.Text = _placeholderText;
                    _textBox.ForeColor = ModernTheme.Colors.TextSecondary;
                    _isPlaceholderActive = true;
                    _clearButton.Visible = false;
                }
                else
                {
                    _textBox.Text = value;
                    _textBox.ForeColor = ModernTheme.Colors.TextPrimary;
                    _isPlaceholderActive = false;
                    _clearButton.Visible = ShowClearButton;
                }
            }
        }

        public void Clear()
        {
            Text = string.Empty;
        }

        private void OnTextBoxGotFocus(object sender, EventArgs e)
        {
            if (_isPlaceholderActive)
            {
                _textBox.Text = string.Empty;
                _textBox.ForeColor = ModernTheme.Colors.TextPrimary;
                _isPlaceholderActive = false;
            }
            Invalidate(); // Trigger border redraw
        }

        private void OnTextBoxLostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_textBox.Text))
            {
                _textBox.Text = _placeholderText;
                _textBox.ForeColor = ModernTheme.Colors.TextSecondary;
                _isPlaceholderActive = true;
                _clearButton.Visible = false;
            }
            Invalidate(); // Trigger border redraw
        }

        private void OnTextBoxTextChanged(object sender, EventArgs e)
        {
            if (!_isPlaceholderActive)
            {
                _clearButton.Visible = ShowClearButton && !string.IsNullOrEmpty(_textBox.Text);
                
                // Restart debounce timer
                _debounceTimer.Stop();
                if (_debounceMs > 0)
                {
                    _debounceTimer.Start();
                }
                else
                {
                    TriggerSearch();
                }
            }

            TextChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDebounceTimerTick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();
            TriggerSearch();
        }

        private void TriggerSearch()
        {
            SearchTriggered?.Invoke(this, EventArgs.Empty);
        }

        private void OnClearButtonClick(object sender, EventArgs e)
        {
            Clear();
            _textBox.Focus();
            TriggerSearch();
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _debounceTimer.Stop();
                TriggerSearch();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Clear();
                e.SuppressKeyPress = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _debounceTimer?.Stop();
                _debounceTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}