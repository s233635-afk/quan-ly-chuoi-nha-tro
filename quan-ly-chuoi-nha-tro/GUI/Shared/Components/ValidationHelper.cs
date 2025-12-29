using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Helper class for real-time form validation
    /// Provides visual feedback and validation rules
    /// </summary>
    public class ValidationHelper
    {
        private readonly Dictionary<Control, ValidationRule> _rules = new Dictionary<Control, ValidationRule>();
        private readonly Dictionary<Control, Label> _errorLabels = new Dictionary<Control, Label>();
        private readonly Color _errorBorderColor = Color.FromArgb(211, 47, 47);
        private readonly Color _normalBorderColor = Color.FromArgb(200, 200, 200);
        private readonly Color _errorTextColor = Color.FromArgb(211, 47, 47);

        /// <summary>
        /// Add required validation to a control
        /// </summary>
        public ValidationHelper Required(Control control, string errorMessage = null)
        {
            var rule = GetOrCreateRule(control);
            rule.IsRequired = true;
            rule.RequiredMessage = errorMessage ?? "Trường này là bắt buộc";
            AttachValidation(control);
            return this;
        }

        /// <summary>
        /// Add minimum length validation
        /// </summary>
        public ValidationHelper MinLength(Control control, int minLength, string errorMessage = null)
        {
            var rule = GetOrCreateRule(control);
            rule.MinLength = minLength;
            rule.MinLengthMessage = errorMessage ?? $"Tối thiểu {minLength} ký tự";
            AttachValidation(control);
            return this;
        }

        /// <summary>
        /// Add maximum length validation
        /// </summary>
        public ValidationHelper MaxLength(Control control, int maxLength, string errorMessage = null)
        {
            var rule = GetOrCreateRule(control);
            rule.MaxLength = maxLength;
            rule.MaxLengthMessage = errorMessage ?? $"Tối đa {maxLength} ký tự";
            AttachValidation(control);
            return this;
        }

        /// <summary>
        /// Add email validation
        /// </summary>
        public ValidationHelper Email(Control control, string errorMessage = null)
        {
            var rule = GetOrCreateRule(control);
            rule.IsEmail = true;
            rule.EmailMessage = errorMessage ?? "Email không hợp lệ";
            AttachValidation(control);
            return this;
        }

        /// <summary>
        /// Add phone validation (Vietnam format)
        /// </summary>
        public ValidationHelper Phone(Control control, string errorMessage = null)
        {
            var rule = GetOrCreateRule(control);
            rule.IsPhone = true;
            rule.PhoneMessage = errorMessage ?? "Số điện thoại không hợp lệ";
            AttachValidation(control);
            return this;
        }

        /// <summary>
        /// Add numeric validation
        /// </summary>
        public ValidationHelper Numeric(Control control, string errorMessage = null)
        {
            var rule = GetOrCreateRule(control);
            rule.IsNumeric = true;
            rule.NumericMessage = errorMessage ?? "Chỉ được nhập số";
            AttachValidation(control);
            return this;
        }

        /// <summary>
        /// Add positive number validation
        /// </summary>
        public ValidationHelper PositiveNumber(Control control, string errorMessage = null)
        {
            var rule = GetOrCreateRule(control);
            rule.IsPositive = true;
            rule.PositiveMessage = errorMessage ?? "Giá trị phải lớn hơn 0";
            AttachValidation(control);
            return this;
        }

        /// <summary>
        /// Add custom validation
        /// </summary>
        public ValidationHelper Custom(Control control, Func<string, bool> validator, string errorMessage)
        {
            var rule = GetOrCreateRule(control);
            rule.CustomValidator = validator;
            rule.CustomMessage = errorMessage;
            AttachValidation(control);
            return this;
        }

        /// <summary>
        /// Attach an error label to display validation messages
        /// </summary>
        public ValidationHelper WithErrorLabel(Control control, Label errorLabel)
        {
            _errorLabels[control] = errorLabel;
            errorLabel.ForeColor = _errorTextColor;
            errorLabel.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            errorLabel.Visible = false;
            return this;
        }

        /// <summary>
        /// Validate a single control
        /// </summary>
        public bool ValidateControl(Control control)
        {
            if (!_rules.TryGetValue(control, out var rule))
                return true;

            var value = control.Text?.Trim() ?? string.Empty;
            var error = ValidateValue(value, rule);

            ShowError(control, error);
            return string.IsNullOrEmpty(error);
        }

        /// <summary>
        /// Validate all registered controls
        /// </summary>
        public bool ValidateAll()
        {
            bool allValid = true;
            Control firstInvalid = null;

            foreach (var kvp in _rules)
            {
                var control = kvp.Key;
                if (!ValidateControl(control))
                {
                    allValid = false;
                    if (firstInvalid == null)
                        firstInvalid = control;
                }
            }

            // Focus first invalid control
            if (firstInvalid != null)
                firstInvalid.Focus();

            return allValid;
        }

        /// <summary>
        /// Clear all validation errors
        /// </summary>
        public void ClearErrors()
        {
            foreach (var control in _rules.Keys)
            {
                ShowError(control, null);
            }
        }

        private ValidationRule GetOrCreateRule(Control control)
        {
            if (!_rules.TryGetValue(control, out var rule))
            {
                rule = new ValidationRule();
                _rules[control] = rule;
            }
            return rule;
        }

        private void AttachValidation(Control control)
        {
            // Remove existing handlers to prevent duplicates
            control.Validating -= OnControlValidating;
            control.TextChanged -= OnControlTextChanged;

            // Add handlers
            control.Validating += OnControlValidating;
            control.TextChanged += OnControlTextChanged;
        }

        private void OnControlValidating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is Control control)
                ValidateControl(control);
        }

        private void OnControlTextChanged(object sender, EventArgs e)
        {
            if (sender is Control control && _rules.ContainsKey(control))
            {
                // Clear error on typing (will revalidate on blur)
                ShowError(control, null);
            }
        }

        private string ValidateValue(string value, ValidationRule rule)
        {
            // Required
            if (rule.IsRequired && string.IsNullOrWhiteSpace(value))
                return rule.RequiredMessage;

            // Skip other validations if empty and not required
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // MinLength
            if (rule.MinLength.HasValue && value.Length < rule.MinLength.Value)
                return rule.MinLengthMessage;

            // MaxLength
            if (rule.MaxLength.HasValue && value.Length > rule.MaxLength.Value)
                return rule.MaxLengthMessage;

            // Email
            if (rule.IsEmail)
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(value, emailPattern))
                    return rule.EmailMessage;
            }

            // Phone (Vietnam: 10 digits starting with 0)
            if (rule.IsPhone)
            {
                var phonePattern = @"^0\d{9,10}$";
                var phoneValue = value.Replace(" ", "").Replace("-", "");
                if (!Regex.IsMatch(phoneValue, phonePattern))
                    return rule.PhoneMessage;
            }

            // Numeric
            if (rule.IsNumeric && !decimal.TryParse(value, out _))
                return rule.NumericMessage;

            // Positive
            if (rule.IsPositive)
            {
                if (!decimal.TryParse(value, out var num) || num <= 0)
                    return rule.PositiveMessage;
            }

            // Custom
            if (rule.CustomValidator != null && !rule.CustomValidator(value))
                return rule.CustomMessage;

            return null;
        }

        private void ShowError(Control control, string error)
        {
            bool hasError = !string.IsNullOrEmpty(error);

            // Update border color if TextBox
            if (control is TextBox textBox)
            {
                // Use custom painting would be better, but for now just change BackColor slightly
                textBox.BackColor = hasError ? Color.FromArgb(255, 245, 245) : Color.White;
            }

            // Update error label
            if (_errorLabels.TryGetValue(control, out var label))
            {
                label.Text = error ?? string.Empty;
                label.Visible = hasError;
            }
        }

        private class ValidationRule
        {
            public bool IsRequired { get; set; }
            public string RequiredMessage { get; set; }

            public int? MinLength { get; set; }
            public string MinLengthMessage { get; set; }

            public int? MaxLength { get; set; }
            public string MaxLengthMessage { get; set; }

            public bool IsEmail { get; set; }
            public string EmailMessage { get; set; }

            public bool IsPhone { get; set; }
            public string PhoneMessage { get; set; }

            public bool IsNumeric { get; set; }
            public string NumericMessage { get; set; }

            public bool IsPositive { get; set; }
            public string PositiveMessage { get; set; }

            public Func<string, bool> CustomValidator { get; set; }
            public string CustomMessage { get; set; }
        }
    }
}
