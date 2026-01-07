using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// SimpleLoadingOverlay - Simple loading overlay with centered text only
    /// </summary>
    public class SimpleLoadingOverlay : IDisposable
    {
        private readonly Panel _overlay;
        private readonly Label _statusLabel;
        private readonly Timer _animationTimer;
        private int _animationFrame = 0;

        public SimpleLoadingOverlay(Control parent, string initialStatus = "Đang tải thông kê...")
        {
            InitializeComponent(parent);
            
            _overlay = CreateOverlay();
            _statusLabel = CreateStatusLabel(initialStatus);
            _animationTimer = CreateAnimationTimer();

            _overlay.Controls.Add(_statusLabel);
            
            // Add overlay directly to parent control instead of as separate form
            if (parent != null)
            {
                parent.Controls.Add(_overlay);
                _overlay.BringToFront();
            }

            _animationTimer.Start();
        }

        private void InitializeComponent(Control parent)
        {
            // No longer need form-level initialization since we're adding directly to parent
        }

        private Panel CreateOverlay()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 255, 255, 255), // Almost opaque white
                Cursor = Cursors.WaitCursor
            };
            
            // Semi-transparent effect
            panel.Paint += (s, e) =>
            {
                using (var brush = new SolidBrush(Color.FromArgb(240, 255, 255, 255)))
                {
                    e.Graphics.FillRectangle(brush, panel.ClientRectangle);
                }
            };
            
            return panel;
        }

        private Label CreateStatusLabel(string initialStatus)
        {
            return new Label
            {
                Text = initialStatus,
                Font = new Font("Segoe UI", 16f, FontStyle.Regular),
                ForeColor = Color.FromArgb(0, 120, 215), // Blue color
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.None
            };
        }

        private Timer CreateAnimationTimer()
        {
            var timer = new Timer { Interval = 400 }; // Slower animation
            timer.Tick += (s, e) =>
            {
                _animationFrame = (_animationFrame + 1) % 4;
                var baseText = _statusLabel.Text;
                
                // Remove existing dots
                if (baseText.EndsWith("..."))
                    baseText = baseText.Substring(0, baseText.Length - 3);
                else if (baseText.EndsWith(".."))
                    baseText = baseText.Substring(0, baseText.Length - 2);
                else if (baseText.EndsWith("."))
                    baseText = baseText.Substring(0, baseText.Length - 1);
                
                // Add animated dots
                var dots = new string('.', _animationFrame);
                _statusLabel.Text = baseText + dots;
                
                // Re-center the label
                CenterLabel();
            };
            return timer;
        }

        private void CenterLabel()
        {
            if (_statusLabel != null && _overlay != null)
            {
                _statusLabel.Location = new Point(
                    (_overlay.Width - _statusLabel.Width) / 2,
                    (_overlay.Height - _statusLabel.Height) / 2
                );
            }
        }

        /// <summary>
        /// Update status message
        /// </summary>
        public void UpdateStatus(string status)
        {
            if (_statusLabel.InvokeRequired)
            {
                _statusLabel.Invoke(new Action(() => {
                    _statusLabel.Text = status;
                    CenterLabel();
                }));
            }
            else
            {
                _statusLabel.Text = status;
                CenterLabel();
            }
        }


        public new void Dispose()
        {
            _animationTimer?.Stop();
            _animationTimer?.Dispose();
            
            // Remove overlay from parent
            if (_overlay != null && _overlay.Parent != null)
            {
                _overlay.Parent.Controls.Remove(_overlay);
            }
            
            _overlay?.Dispose();
        }
    }
}