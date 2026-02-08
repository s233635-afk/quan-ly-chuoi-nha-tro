using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// ModernDatePicker - DateTimePicker with modern styling and clear button
    /// </summary>
    public class ModernDatePicker : UserControl
    {
        private DateTimePicker _datePicker;
        private Button _calendarButton;
        private Button _clearButton;
        private Label _label;
        private Panel _pickerPanel;

        private string _labelText = "";
        private DateTime? _minDate = null;
        private DateTime? _maxDate = null;

        #region Properties

        public string Label
        {
            get => _labelText;
            set
            {
                _labelText = value;
                _label.Text = value;
                _label.Visible = !string.IsNullOrEmpty(value);
            }
        }

        public DateTime Value
        {
            get => _datePicker.Value;
            set => _datePicker.Value = value;
        }

        public DateTime? MinDate
        {
            get => _minDate;
            set
            {
                _minDate = value;
                if (value.HasValue)
                {
                    _datePicker.MinDate = value.Value;
                }
            }
        }

        public DateTime? MaxDate
        {
            get => _maxDate;
            set
            {
                _maxDate = value;
                if (value.HasValue)
                {
                    _datePicker.MaxDate = value.Value;
                }
            }
        }

        public bool ShowCheckBox
        {
            get => _datePicker.ShowCheckBox;
            set => _datePicker.ShowCheckBox = value;
        }

        public bool Checked
        {
            get => _datePicker.Checked;
            set => _datePicker.Checked = value;
        }

        public new event EventHandler ValueChanged
        {
            add => _datePicker.ValueChanged += value;
            remove => _datePicker.ValueChanged -= value;
        }

        #endregion

        public ModernDatePicker()
        {
            InitializeComponents();
            WireUpEvents();
        }

        private void InitializeComponents()
        {
            // Main container
            AutoSize = false;
            Height = 65;
            MinimumSize = new Size(200, 65);
            BackColor = Color.Transparent;

            // Label
            _label = new Label
            {
                Text = _labelText,
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Small),
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 2),
                Visible = !string.IsNullOrEmpty(_labelText)
            };

            // Picker panel (provides border)
            _pickerPanel = new Panel
            {
                Height = 36,
                Top = _label.Visible ? 22 : 0,
                Left = 0,
                Width = Width, // Will be resized in OnResize
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                BackColor = ModernTheme.Colors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(4, 0, 0, 0)
            };

            // DateTimePicker
            _datePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = ModernTheme.Fonts.NormalFont,
                Dock = DockStyle.Fill
            };

            // Calendar button
            _calendarButton = new Button
            {
                Text = "📅",
                Width = 32,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.Colors.Surface,
                ForeColor = ModernTheme.Colors.TextPrimary,
                Dock = DockStyle.Right,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Emoji", 11f),
                Margin = new Padding(2)
            };
            _calendarButton.FlatAppearance.BorderSize = 0;

            // Clear button
            _clearButton = new Button
            {
                Text = "✕",
                Width = 28,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = ModernTheme.Colors.TextSecondary,
                Dock = DockStyle.Right,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f),
                Visible = false
            };
            _clearButton.FlatAppearance.BorderSize = 0;

            // Assemble controls
            _pickerPanel.Controls.Add(_datePicker);
            _pickerPanel.Controls.Add(_clearButton);
            _pickerPanel.Controls.Add(_calendarButton);
            Controls.Add(_label);
            Controls.Add(_pickerPanel);
        }

        private void WireUpEvents()
        {
            _calendarButton.Click += (s, e) =>
            {
                _datePicker.Focus();
                SendKeys.Send("%{DOWN}"); // Send Alt+Down to open calendar
            };

            _clearButton.Click += (s, e) =>
            {
                if (_datePicker.ShowCheckBox)
                {
                    _datePicker.Checked = false;
                    _clearButton.Visible = false;
                }
            };

            _datePicker.ValueChanged += (s, e) =>
            {
                if (_datePicker.ShowCheckBox)
                {
                    _clearButton.Visible = _datePicker.Checked;
                }
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _pickerPanel.Width = Width;
        }
    }
}
