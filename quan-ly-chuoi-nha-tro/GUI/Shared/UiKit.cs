using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI
{
    internal static class UiKit
    {
        public static readonly Color AppBackground = Color.FromArgb(245, 247, 250);
        public static readonly Color Primary = Color.FromArgb(0, 122, 204);
        public static readonly Color Success = Color.FromArgb(46, 125, 50);
        public static readonly Color Danger = Color.FromArgb(211, 47, 47);
        public static readonly Color Warning = Color.FromArgb(255, 152, 0);
        public static readonly Color Purple = Color.FromArgb(103, 58, 183);
        public static readonly Color MutedText = Color.FromArgb(70, 70, 70);

        public static Button MakeButton(string text, Color backColor, EventHandler onClick, int width = 110)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = 36, // Slightly taller for better proportions
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            
            // Apply rounded corners
            btn.SizeChanged += (s, e) => SetRoundedRegion(btn, 20);
            SetRoundedRegion(btn, 20); // Initial set
            
            // Hover effect
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            if (onClick != null) btn.Click += onClick;
            return btn;
        }

        public static void SetRoundedRegion(Control c, int radius)
        {
            if (c == null || c.IsDisposed) return;
            
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                var rect = c.ClientRectangle;
                rect.Width -= 0;
                rect.Height -= 0;
                
                if (rect.Width <= 0 || rect.Height <= 0) return;

                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();

                c.Region = new Region(path);
            }
        }

        public static void StyleGrid(DataGridView grid)
        {
            if (grid == null) return;

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Primary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 252);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.RowTemplate.Height = 32;
            grid.GridColor = Color.FromArgb(235, 240, 245);
        }

        public static Panel MakeSearchPanel(TextBox textBox, int width, string placeholder, Action onChanged)
        {
            var panel = new Panel
            {
                BackColor = AppBackground,
                Height = 34,
                Width = width,
                Padding = new Padding(10, 0, 10, 0), // Bỏ padding dọc
                Anchor = AnchorStyles.None // Để TableLayoutPanel tự căn giữa
            };

            textBox.BorderStyle = BorderStyle.None;
            textBox.Parent = panel;
            textBox.Location = new Point(2, 6);
            textBox.Location = new Point(2, (panel.ClientSize.Height - textBox.Height) / 2); // Căn giữa theo chiều dọc
            textBox.Width = panel.Width - 16;
            panel.Resize += (s, e) => textBox.Width = panel.Width - 16;

            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;

            textBox.TextChanged += (s, e) => onChanged?.Invoke();
            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = string.Empty;
                    textBox.ForeColor = Color.Black;
                }
            };
            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };

            return panel;
        }
    }
}
