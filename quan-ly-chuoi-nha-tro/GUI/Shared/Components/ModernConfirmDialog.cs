using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Modern confirmation dialog to replace MessageBox
    /// Provides beautiful, consistent dialogs for the application
    /// </summary>
    public class ModernConfirmDialog : Form
    {
        public enum DialogType { Info, Warning, Danger, Question }
        public enum DialogResult { None, Confirm, Cancel }

        private new DialogResult _result = DialogResult.None;
        private readonly string _message;
        private readonly string _title;
        private readonly DialogType _type;
        private readonly string _confirmText;
        private readonly string _cancelText;

        public new DialogResult Result => _result;

        private ModernConfirmDialog(string message, string title, DialogType type, 
            string confirmText, string cancelText)
        {
            _message = message;
            _title = title;
            _type = type;
            _confirmText = confirmText;
            _cancelText = cancelText;

            InitializeDialog();
        }

        private void InitializeDialog()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(400, 200);
            BackColor = Color.White;
            ShowInTaskbar = false;
            KeyPreview = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            Paint += OnPaint;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    _result = DialogResult.Cancel;
                    Close();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    _result = DialogResult.Confirm;
                    Close();
                }
            };

            CreateControls();
        }

        private void CreateControls()
        {
            // Icon panel
            var iconPanel = new Panel
            {
                Location = new Point(24, 50),
                Size = new Size(40, 40),
                BackColor = Color.Transparent
            };
            iconPanel.Paint += OnIconPaint;
            Controls.Add(iconPanel);

            // Message label
            var lblMessage = new Label
            {
                Text = _message,
                Location = new Point(80, 50),
                Size = new Size(290, 60),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            Controls.Add(lblMessage);

            // Buttons panel
            var btnPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // Cancel button
            var btnCancel = new Button
            {
                Text = _cancelText,
                Size = new Size(100, 36),
                Location = new Point(180, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnCancel.Click += (s, e) =>
            {
                _result = DialogResult.Cancel;
                Close();
            };
            btnPanel.Controls.Add(btnCancel);

            // Confirm button
            var btnConfirm = new Button
            {
                Text = _confirmText,
                Size = new Size(100, 36),
                Location = new Point(290, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = GetButtonColor(_type),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += (s, e) =>
            {
                _result = DialogResult.Confirm;
                Close();
            };
            btnPanel.Controls.Add(btnConfirm);

            Controls.Add(btnPanel);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw border
            using (var pen = new Pen(Color.FromArgb(220, 220, 220)))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            // Draw header
            using (var brush = new SolidBrush(GetHeaderColor(_type)))
            {
                g.FillRectangle(brush, 0, 0, Width, 40);
            }

            // Draw title
            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString(_title, font, brush, 16, 10);
            }

            // Draw close button
            using (var font = new Font("Segoe UI", 14, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                g.DrawString("×", font, brush, Width - 30, 8);
            }
        }

        private void OnIconPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, 36, 36);
            var color = GetIconColor(_type);

            using (var pen = new Pen(color, 3))
            using (var brush = new SolidBrush(color))
            {
                var cx = 18;
                var cy = 18;

                switch (_type)
                {
                    case DialogType.Danger:
                        // X in circle
                        g.DrawEllipse(pen, rect);
                        g.DrawLine(pen, cx - 8, cy - 8, cx + 8, cy + 8);
                        g.DrawLine(pen, cx + 8, cy - 8, cx - 8, cy + 8);
                        break;

                    case DialogType.Warning:
                        // Triangle with !
                        var triangle = new[] {
                            new Point(cx, 2),
                            new Point(rect.Right - 2, rect.Bottom - 2),
                            new Point(2, rect.Bottom - 2)
                        };
                        g.DrawPolygon(pen, triangle);
                        g.FillEllipse(brush, cx - 2, cy + 6, 5, 5);
                        g.DrawLine(new Pen(color, 3), cx, cy - 6, cx, cy + 2);
                        break;

                    case DialogType.Question:
                        // ? in circle
                        g.DrawEllipse(pen, rect);
                        using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
                        {
                            g.DrawString("?", font, brush, cx - 7, cy - 12);
                        }
                        break;

                    default: // Info
                        // i in circle
                        g.DrawEllipse(pen, rect);
                        g.FillEllipse(brush, cx - 2, cy - 10, 5, 5);
                        g.DrawLine(new Pen(color, 3), cx, cy - 2, cx, cy + 10);
                        break;
                }
            }
        }

        private static Color GetHeaderColor(DialogType type)
        {
            switch (type)
            {
                case DialogType.Danger: return Color.FromArgb(211, 47, 47);
                case DialogType.Warning: return Color.FromArgb(245, 124, 0);
                case DialogType.Question: return Color.FromArgb(25, 103, 210);
                default: return Color.FromArgb(0, 120, 215);
            }
        }

        private static Color GetButtonColor(DialogType type)
        {
            switch (type)
            {
                case DialogType.Danger: return Color.FromArgb(211, 47, 47);
                case DialogType.Warning: return Color.FromArgb(245, 124, 0);
                default: return Color.FromArgb(0, 120, 215);
            }
        }

        private static Color GetIconColor(DialogType type)
        {
            switch (type)
            {
                case DialogType.Danger: return Color.FromArgb(211, 47, 47);
                case DialogType.Warning: return Color.FromArgb(245, 124, 0);
                case DialogType.Question: return Color.FromArgb(25, 103, 210);
                default: return Color.FromArgb(0, 120, 215);
            }
        }

        // Static helper methods

        /// <summary>
        /// Show a confirmation dialog
        /// </summary>
        public static bool Confirm(string message, string title = "Xác nhận")
        {
            using (var dialog = new ModernConfirmDialog(message, title, DialogType.Question, "Xác nhận", "Hủy"))
            {
                dialog.ShowDialog();
                return dialog.Result == DialogResult.Confirm;
            }
        }

        /// <summary>
        /// Show a danger confirmation (for delete operations)
        /// </summary>
        public static bool ConfirmDanger(string message, string title = "Cảnh báo")
        {
            using (var dialog = new ModernConfirmDialog(message, title, DialogType.Danger, "Xóa", "Hủy"))
            {
                dialog.ShowDialog();
                return dialog.Result == DialogResult.Confirm;
            }
        }

        /// <summary>
        /// Show a warning dialog
        /// </summary>
        public static bool ConfirmWarning(string message, string title = "Cảnh báo")
        {
            using (var dialog = new ModernConfirmDialog(message, title, DialogType.Warning, "Tiếp tục", "Hủy"))
            {
                dialog.ShowDialog();
                return dialog.Result == DialogResult.Confirm;
            }
        }

        /// <summary>
        /// Show an info dialog
        /// </summary>
        public static void ShowInfo(string message, string title = "Thông báo")
        {
            using (var dialog = new ModernConfirmDialog(message, title, DialogType.Info, "OK", ""))
            {
                // Hide cancel button for info dialog
                dialog.ShowDialog();
            }
        }
    }
}
