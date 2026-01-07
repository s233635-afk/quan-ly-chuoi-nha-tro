using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Manages tooltips with consistent styling across the application
    /// </summary>
    public class TooltipManager : IDisposable
    {
        private readonly ToolTip _tooltip;
        private readonly Dictionary<Control, string> _registeredControls = new Dictionary<Control, string>();

        public TooltipManager()
        {
            _tooltip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 100,
                ShowAlways = true,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                OwnerDraw = true
            };

            _tooltip.Draw += OnTooltipDraw;
            _tooltip.Popup += OnTooltipPopup;
        }

        /// <summary>
        /// Set tooltip for a control
        /// </summary>
        public TooltipManager SetTooltip(Control control, string text)
        {
            if (control == null || string.IsNullOrEmpty(text)) return this;

            _tooltip.SetToolTip(control, text);
            _registeredControls[control] = text;
            return this;
        }

        /// <summary>
        /// Set tooltip with keyboard shortcut
        /// </summary>
        public TooltipManager SetTooltipWithShortcut(Control control, string text, string shortcut)
        {
            if (control == null) return this;

            var fullText = string.IsNullOrEmpty(shortcut) ? text : $"{text} ({shortcut})";
            return SetTooltip(control, fullText);
        }

        /// <summary>
        /// Set tooltip for button with action description
        /// </summary>
        public TooltipManager SetButtonTooltip(Button button, string action, string shortcut = null)
        {
            if (button == null) return this;

            var text = shortcut == null ? action : $"{action} ({shortcut})";
            return SetTooltip(button, text);
        }

        /// <summary>
        /// Auto-attach tooltips to common controls based on their name
        /// </summary>
        public TooltipManager AutoAttach(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                // Auto-detect button actions
                if (control is Button btn)
                {
                    var name = btn.Name.ToLower();
                    var text = btn.Text;

                    if (name.Contains("add") || name.Contains("new") || name.Contains("create"))
                        SetButtonTooltip(btn, text, "Ctrl+N");
                    else if (name.Contains("save"))
                        SetButtonTooltip(btn, text, "Ctrl+S");
                    else if (name.Contains("edit") || name.Contains("update"))
                        SetButtonTooltip(btn, text, "Ctrl+E");
                    else if (name.Contains("delete") || name.Contains("remove"))
                        SetButtonTooltip(btn, text, "Delete");
                    else if (name.Contains("refresh") || name.Contains("reload"))
                        SetButtonTooltip(btn, text, "F5");
                    else if (name.Contains("search") || name.Contains("find"))
                        SetButtonTooltip(btn, text, "Ctrl+F");
                    else if (!string.IsNullOrEmpty(text))
                        SetTooltip(btn, text);
                }

                // Recursively process child controls
                if (control.HasChildren)
                    AutoAttach(control.Controls);
            }

            return this;
        }

        /// <summary>
        /// Remove tooltip from a control
        /// </summary>
        public TooltipManager RemoveTooltip(Control control)
        {
            if (control == null) return this;

            _tooltip.SetToolTip(control, null);
            _registeredControls.Remove(control);
            return this;
        }

        /// <summary>
        /// Clear all tooltips
        /// </summary>
        public void ClearAll()
        {
            _tooltip.RemoveAll();
            _registeredControls.Clear();
        }

        private void OnTooltipDraw(object sender, DrawToolTipEventArgs e)
        {
            // Custom drawing for modern look
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Background
            using (var brush = new SolidBrush(Color.FromArgb(50, 50, 50)))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Border
            using (var pen = new Pen(Color.FromArgb(80, 80, 80)))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }

            // Text
            using (var font = new Font("Segoe UI", 9, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.White))
            {
                var rect = new RectangleF(6, 4, e.Bounds.Width - 12, e.Bounds.Height - 8);
                e.Graphics.DrawString(e.ToolTipText, font, brush, rect);
            }
        }

        private void OnTooltipPopup(object sender, PopupEventArgs e)
        {
            // Adjust size for custom drawing
            using (var g = e.AssociatedControl.CreateGraphics())
            using (var font = new Font("Segoe UI", 9, FontStyle.Regular))
            {
                var size = g.MeasureString(_tooltip.GetToolTip(e.AssociatedControl), font);
                e.ToolTipSize = new Size((int)size.Width + 16, (int)size.Height + 10);
            }
        }

        public void Dispose()
        {
            _tooltip?.Dispose();
            _registeredControls.Clear();
        }
    }
}
