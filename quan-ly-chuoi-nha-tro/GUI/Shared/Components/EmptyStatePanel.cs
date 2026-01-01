using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// EmptyStatePanel - Display when no data is available with icon, message, and optional action
    /// </summary>
    public class EmptyStatePanel : Panel
    {
        private Label _iconLabel;
        private Label _primaryMessageLabel;
        private Label _secondaryMessageLabel;
        private ModernButton _actionButton;

        private string _emptyIcon = "📭";
        private string _primaryMessage = "Không có dữ liệu";
        private string _secondaryMessage = "";

        #region Properties

        public string EmptyIcon
        {
            get => _emptyIcon;
            set
            {
                _emptyIcon = value;
                _iconLabel.Text = value;
            }
        }

        public string PrimaryMessage
        {
            get => _primaryMessage;
            set
            {
                _primaryMessage = value;
                _primaryMessageLabel.Text = value;
            }
        }

        public string SecondaryMessage
        {
            get => _secondaryMessage;
            set
            {
                _secondaryMessage = value;
                _secondaryMessageLabel.Text = value;
                _secondaryMessageLabel.Visible = !string.IsNullOrEmpty(value);
                RepositionControls();
            }
        }

        public ModernButton ActionButton
        {
            get => _actionButton;
            set
            {
                if (_actionButton != null && Controls.Contains(_actionButton))
                {
                    Controls.Remove(_actionButton);
                }
                _actionButton = value;
                if (_actionButton != null)
                {
                    _actionButton.Visible = true;
                    Controls.Add(_actionButton);
                    RepositionControls();
                }
            }
        }

        #endregion

        public EmptyStatePanel()
        {
            InitializeComponents();
            Dock = DockStyle.Fill;
            BackColor = ModernTheme.Colors.Background;
            Padding = new Padding(ModernTheme.Spacing.XXL);
        }

        private void InitializeComponents()
        {
            // Icon label (emoji)
            _iconLabel = new Label
            {
                Text = _emptyIcon,
                Font = new Font("Segoe UI Emoji", 48f),
                ForeColor = ModernTheme.Colors.TextTertiary,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Primary message
            _primaryMessageLabel = new Label
            {
                Text = _primaryMessage,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Heading3),
                ForeColor = ModernTheme.Colors.TextPrimary,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Secondary message
            _secondaryMessageLabel = new Label
            {
                Text = _secondaryMessage,
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Normal),
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                MaximumSize = new Size(400, 0), // Wrap text
                Visible = !string.IsNullOrEmpty(_secondaryMessage)
            };

            Controls.Add(_iconLabel);
            Controls.Add(_primaryMessageLabel);
            Controls.Add(_secondaryMessageLabel);

            // Position controls on resize
            Resize += (s, e) => RepositionControls();
        }

        private void RepositionControls()
        {
            if (Width <= 0 || Height <= 0) return;

            int totalHeight = _iconLabel.Height + ModernTheme.Spacing.LG +
                             _primaryMessageLabel.Height + ModernTheme.Spacing.SM;

            if (_secondaryMessageLabel.Visible)
            {
                totalHeight += _secondaryMessageLabel.Height + ModernTheme.Spacing.MD;
            }

            if (_actionButton != null && _actionButton.Visible)
            {
                totalHeight += _actionButton.Height + ModernTheme.Spacing.LG;
            }

            // Start from center and position each element
            int startY = (Height - totalHeight) / 2;
            int currentY = startY;

            // Icon
            _iconLabel.Location = new Point(
                (Width - _iconLabel.Width) / 2,
                currentY
            );
            currentY += _iconLabel.Height + ModernTheme.Spacing.LG;

            // Primary message
            _primaryMessageLabel.Location = new Point(
                (Width - _primaryMessageLabel.Width) / 2,
                currentY
            );
            currentY += _primaryMessageLabel.Height + ModernTheme.Spacing.SM;

            // Secondary message
            if (_secondaryMessageLabel.Visible)
            {
                _secondaryMessageLabel.Location = new Point(
                    (Width - _secondaryMessageLabel.Width) / 2,
                    currentY
                );
                currentY += _secondaryMessageLabel.Height + ModernTheme.Spacing.MD;
            }

            // Action button
            if (_actionButton != null && _actionButton.Visible)
            {
                _actionButton.Location = new Point(
                    (Width - _actionButton.Width) / 2,
                    currentY + ModernTheme.Spacing.LG
                );
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Optional: Draw subtle background pattern or icon
        }
    }
}
