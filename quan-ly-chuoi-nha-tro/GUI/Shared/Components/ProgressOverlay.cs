using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// ProgressOverlay - Full-screen loading overlay with progress indicator
    /// </summary>
    public class ProgressOverlay : Form, IDisposable
    {
        private readonly Panel _overlay;
        private readonly Panel _contentPanel;
        private readonly ProgressBar _progressBar;
        private readonly Label _statusLabel;
        private readonly Label _percentLabel;
        private readonly Timer _animationTimer;
        private int _animationFrame = 0;

        public ProgressOverlay(Control parent, string initialStatus = "Đang tải...")
        {
            InitializeComponent(parent);
            
            _overlay = CreateOverlay();
            _contentPanel = CreateContentPanel();
            _progressBar = CreateProgressBar();
            _statusLabel = CreateStatusLabel(initialStatus);
            _percentLabel = CreatePercentLabel();
            _animationTimer = CreateAnimationTimer();

            _contentPanel.Controls.Add(_percentLabel);
            _contentPanel.Controls.Add(_progressBar);
            _contentPanel.Controls.Add(_statusLabel);
            _overlay.Controls.Add(_contentPanel);
            Controls.Add(_overlay);

            _animationTimer.Start();
            Show();
        }

        private void InitializeComponent(Control parent)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            BackColor = Color.Magenta;
            TransparencyKey = Color.Magenta;
            TopMost = true;
            
            if (parent != null)
            {
                var form = parent.FindForm();
                if (form != null)
                {
                    Owner = form;
                    Bounds = form.RectangleToScreen(form.ClientRectangle);
                }
                else
                {
                    Bounds = parent.RectangleToScreen(parent.ClientRectangle);
                }
            }
        }

        private Panel CreateOverlay()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(180, 0, 0, 0), // Semi-transparent black
                Cursor = Cursors.WaitCursor
            };
        }

        private Panel CreateContentPanel()
        {
            var panel = new Panel
            {
                Width = 320,
                Height = 140,
                BackColor = ModernTheme.Colors.Background,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(ModernTheme.Spacing.XL)
            };

            // Center the panel
            panel.Location = new Point(
                (_overlay.Width - panel.Width) / 2,
                (_overlay.Height - panel.Height) / 2
            );

            // Add shadow effect
            panel.Paint += (s, e) =>
            {
                var rect = panel.ClientRectangle;
                
                // Draw shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadowBrush, 4, 4, rect.Width, rect.Height);
                }
                
                // Draw main background
                using (var bgBrush = new SolidBrush(ModernTheme.Colors.Background))
                {
                    e.Graphics.FillRectangle(bgBrush, 0, 0, rect.Width - 4, rect.Height - 4);
                }
                
                // Draw border
                using (var borderPen = new Pen(ModernTheme.Colors.Border, 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, rect.Width - 5, rect.Height - 5);
                }
            };

            return panel;
        }

        private ProgressBar CreateProgressBar()
        {
            var progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 8,
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Margin = new Padding(0, ModernTheme.Spacing.SM, 0, ModernTheme.Spacing.MD)
            };

            // Custom paint for modern look (Windows Forms limitation workaround)
            progressBar.Paint += (s, e) =>
            {
                var rect = progressBar.ClientRectangle;
                
                // Background
                using (var bgBrush = new SolidBrush(ModernTheme.Colors.Surface))
                {
                    e.Graphics.FillRectangle(bgBrush, rect);
                }
                
                // Progress
                if (progressBar.Value > 0)
                {
                    var progressWidth = (int)((double)rect.Width * progressBar.Value / progressBar.Maximum);
                    var progressRect = new Rectangle(0, 0, progressWidth, rect.Height);
                    
                    using (var progressBrush = new SolidBrush(ModernTheme.Colors.Primary))
                    {
                        e.Graphics.FillRectangle(progressBrush, progressRect);
                    }
                }
            };

            return progressBar;
        }

        private Label CreateStatusLabel(string initialStatus)
        {
            return new Label
            {
                Text = initialStatus,
                Font = ModernTheme.Fonts.NormalFont,
                ForeColor = ModernTheme.Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 0, ModernTheme.Spacing.SM)
            };
        }

        private Label CreatePercentLabel()
        {
            return new Label
            {
                Text = "0%",
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Large),
                ForeColor = ModernTheme.Colors.Primary,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, ModernTheme.Spacing.SM, 0, 0)
            };
        }

        private Timer CreateAnimationTimer()
        {
            var timer = new Timer { Interval = 100 };
            timer.Tick += (s, e) =>
            {
                _animationFrame = (_animationFrame + 1) % 4;
                var dots = new string('.', _animationFrame);
                
                if (_statusLabel.Text.Contains("..."))
                {
                    var baseText = _statusLabel.Text.Replace("...", "").Replace("..", "").Replace(".", "");
                    _statusLabel.Text = baseText + dots;
                }
            };
            return timer;
        }

        /// <summary>
        /// Update progress value (0-100)
        /// </summary>
        public void UpdateProgress(int value)
        {
            value = Math.Max(0, Math.Min(100, value));
            
            if (_progressBar.InvokeRequired)
            {
                _progressBar.Invoke(new Action(() =>
                {
                    _progressBar.Value = value;
                    _percentLabel.Text = $"{value}%";
                    _progressBar.Invalidate();
                }));
            }
            else
            {
                _progressBar.Value = value;
                _percentLabel.Text = $"{value}%";
                _progressBar.Invalidate();
            }
        }

        /// <summary>
        /// Update status message
        /// </summary>
        public void UpdateStatus(string status)
        {
            if (_statusLabel.InvokeRequired)
            {
                _statusLabel.Invoke(new Action(() => _statusLabel.Text = status));
            }
            else
            {
                _statusLabel.Text = status;
            }
        }

        /// <summary>
        /// Update both status and progress
        /// </summary>
        public void UpdateStatus(string status, int progress)
        {
            UpdateStatus(status);
            UpdateProgress(progress);
        }

        /// <summary>
        /// Show indeterminate progress (no percentage)
        /// </summary>
        public void SetIndeterminate(bool indeterminate)
        {
            if (indeterminate)
            {
                _percentLabel.Visible = false;
                _progressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                _percentLabel.Visible = true;
                _progressBar.Style = ProgressBarStyle.Continuous;
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

        // Prevent closing via Alt+F4 or close button
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}