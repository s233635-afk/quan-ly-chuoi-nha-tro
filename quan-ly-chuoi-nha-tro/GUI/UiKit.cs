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
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White
            };
            btn.FlatAppearance.BorderSize = 0;
            if (onClick != null) btn.Click += onClick;
            return btn;
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
                Padding = new Padding(10, 7, 10, 7)
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

