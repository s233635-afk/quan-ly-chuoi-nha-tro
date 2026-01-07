using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// ModernInputField - TextBox with floating label, validation message, and modern styling
    /// </summary>
    public class ModernInputField : UserControl
    {
        private TextBox _textBox;
        private Label _floatingLabel;
        private Label _validationLabel;
        private Panel _borderPanel;
        private Button _clearButton;

        private string _label = "";
        private string _placeholder = "";
        private string _validationMessage = "";
        private InputState _state = InputState.Normal;
        private bool _showCharacterCount = false;
        private int _maxLength = 100;

        public enum InputState
        {
            Normal,
            Success,
            Error,
            Disabled
        }

        #region Properties

        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                UpdateLabels();
            }
        }

        public string Placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value;
                UpdatePlaceholder();
            }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                UpdateValidationMessage();
            }
        }

        public InputState State
        {
            get => _state;
            set
            {
                _state = value;
                UpdateStateColors();
            }
        }

        public bool ShowCharacterCount
        {
            get => _showCharacterCount;
            set
            {
                _showCharacterCount = value;
                UpdateCharacterCount();
            }
        }

        public new int MaxLength
        {
            get => _maxLength;
            set
            {
                _maxLength = value;
                _textBox.MaxLength = value;
            }
        }

        public string TextValue
        {
            get => _textBox.Text;
            set => _textBox.Text = value;
        }

        public bool IsPassword
        {
            get => _textBox.UseSystemPasswordChar;
            set => _textBox.UseSystemPasswordChar = value;
        }

        public bool IsMultiline
        {
            get => _textBox.Multiline;
            set
            {
                _textBox.Multiline = value;
                if (value)
                {
                    _textBox.Height = 80;
                    Height = 120;
                }
            }
        }

        #endregion

        public ModernInputField()
        {
            InitializeComponents();
            WireUpEvents();
        }

        private void InitializeComponents()
        {
            // Main container
            AutoSize = false;
            Height = 70;
            MinimumSize = new Size(200, 70);
            BackColor = Color.Transparent;
            Padding = new Padding(0);

            // Border panel (provides colored border)
            _borderPanel = new Panel
            {
                BackColor = ModernTheme.Colors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(ModernTheme.Spacing.SM),
                Height = 36,
                Top = 22,
                Left = 0,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };

            // Floating label
            _floatingLabel = new Label
            {
                Text = _label,
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Small),
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(ModernTheme.Spacing.SM, 2),
                Visible = false
            };

            // TextBox
            _textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = ModernTheme.Fonts.NormalFont,
                ForeColor = ModernTheme.Colors.TextPrimary,
                BackColor = ModernTheme.Colors.Background,
                MaxLength = _maxLength
            };

            // Clear button (X)
            _clearButton = new Button
            {
                Text = "✕",
                Width = 20,
                Height = 20,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = ModernTheme.Colors.TextSecondary,
                Dock = DockStyle.Right,
                Cursor = Cursors.Hand,
                Visible = false,
                Font = new Font("Segoe UI", 9f)
            };
            _clearButton.FlatAppearance.BorderSize = 0;

            // Validation label
            _validationLabel = new Label
            {
                AutoSize = true,
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Tiny),
                ForeColor = ModernTheme.Colors.Error,
                Location = new Point(ModernTheme.Spacing.SM, 65),
                Visible = false,
                MaximumSize = new Size(Width - ModernTheme.Spacing.MD, 0)
            };

            // Assemble controls
            _borderPanel.Controls.Add(_textBox);
            _borderPanel.Controls.Add(_clearButton);
            Controls.Add(_validationLabel);
            Controls.Add(_floatingLabel);
            Controls.Add(_borderPanel);

            UpdateStateColors();
        }

        private void WireUpEvents()
        {
            _textBox.Enter += OnTextBoxEnter;
            _textBox.Leave += OnTextBoxLeave;
            _textBox.TextChanged += OnTextBoxTextChanged;
            _clearButton.Click += (s, e) =>
            {
                _textBox.Clear();
                _textBox.Focus();
            };
        }

        private void OnTextBoxEnter(object sender, EventArgs e)
        {
            // Show floating label
            if (!string.IsNullOrEmpty(_label))
            {
                _floatingLabel.Visible = true;
            }

            // Update border color on focus
            if (_state == InputState.Normal)
            {
                _borderPanel.BackColor = ModernTheme.Colors.PrimaryLight;
            }

            UpdatePlaceholder();
        }

        private void OnTextBoxLeave(object sender, EventArgs e)
        {
            // Hide floating label if no text
            if (string.IsNullOrEmpty(_textBox.Text) && !string.IsNullOrEmpty(_label))
            {
                _floatingLabel.Visible = false;
            }

            // Reset border color
            UpdateStateColors();
            UpdatePlaceholder();
        }

        private void OnTextBoxTextChanged(object sender, EventArgs e)
        {
            // Show/hide clear button
            _clearButton.Visible = !string.IsNullOrEmpty(_textBox.Text) && !IsPassword;

            // Update character count
            UpdateCharacterCount();

            // Update floating label
            _floatingLabel.Visible = !string.IsNullOrEmpty(_textBox.Text) || _textBox.Focused;
        }

        private void UpdateLabels()
        {
            _floatingLabel.Text = _label;
        }

        private void UpdatePlaceholder()
        {
            // Note: WinForms TextBox doesn't have native placeholder
            // Could implement using gray text on Enter/Leave, but keeping it simple
        }

        private void UpdateValidationMessage()
        {
            _validationLabel.Text = _validationMessage;
            _validationLabel.Visible = !string.IsNullOrEmpty(_validationMessage);

            // Adjust height to accommodate validation message
            if (_validationLabel.Visible)
            {
                Height = Math.Max(70, _borderPanel.Bottom + _validationLabel.Height + ModernTheme.Spacing.SM);
            }
            else
            {
                Height = 70;
            }
        }

        private void UpdateCharacterCount()
        {
            if (_showCharacterCount)
            {
                int remaining = _maxLength - _textBox.Text.Length;
                _validationLabel.Text = $"{_textBox.Text.Length}/{_maxLength}";
                _validationLabel.ForeColor = remaining < 10 ? ModernTheme.Colors.Warning : ModernTheme.Colors.TextSecondary;
                _validationLabel.Visible = true;
            }
        }

        private void UpdateStateColors()
        {
            switch (_state)
            {
                case InputState.Normal:
                    _borderPanel.BackColor = ModernTheme.Colors.Background;
                    _textBox.Enabled = true;
                    break;

                case InputState.Success:
                    _borderPanel.BackColor = ModernTheme.Colors.SuccessLight;
                    _textBox.Enabled = true;
                    break;

                case InputState.Error:
                    _borderPanel.BackColor = ModernTheme.Colors.ErrorLight;
                    _textBox.Enabled = true;
                    break;

                case InputState.Disabled:
                    _borderPanel.BackColor = ModernTheme.Colors.Surface;
                    _textBox.Enabled = false;
                    break;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_validationLabel != null)
            {
                _validationLabel.MaximumSize = new Size(Width - ModernTheme.Spacing.MD, 0);
            }
            if (_borderPanel != null)
            {
                _borderPanel.Width = Width;
            }
        }
    }
}
