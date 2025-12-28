using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// ModernTheme - Hệ thống màu sắc và styling hiện đại
    /// Cung cấp tất cả các màu, font, kích thước cho toàn ứng dụng
    /// </summary>
    public static class ModernTheme
    {
        // ==================== PRIMARY COLORS ====================
        public static class Colors
        {
            // Main accent colors
            public static readonly Color Primary = Color.FromArgb(0, 120, 215);        // Xanh chính
            public static readonly Color PrimaryDark = Color.FromArgb(0, 90, 170);     // Xanh đậm
            public static readonly Color PrimaryLight = Color.FromArgb(230, 243, 255); // Xanh nhạt
            
            // Secondary colors
            public static readonly Color Secondary = Color.FromArgb(107, 79, 187);     // Tím
            public static readonly Color SecondaryLight = Color.FromArgb(243, 237, 255);
            
            // Status colors
            public static readonly Color Success = Color.FromArgb(46, 125, 50);        // Xanh lá
            public static readonly Color SuccessLight = Color.FromArgb(237, 247, 233);
            public static readonly Color Warning = Color.FromArgb(251, 188, 4);        // Vàng
            public static readonly Color WarningLight = Color.FromArgb(255, 249, 231);
            public static readonly Color Error = Color.FromArgb(211, 47, 47);          // Đỏ
            public static readonly Color ErrorLight = Color.FromArgb(253, 237, 237);
            public static readonly Color Info = Color.FromArgb(25, 103, 210);          // Xanh dương
            public static readonly Color InfoLight = Color.FromArgb(227, 242, 253);
            
            // Neutral colors
            public static readonly Color Background = Color.FromArgb(255, 255, 255);   // Trắng
            public static readonly Color Surface = Color.FromArgb(248, 249, 250);      // Xám nhạt
            public static readonly Color Border = Color.FromArgb(224, 224, 224);       // Viền xám
            public static readonly Color Divider = Color.FromArgb(230, 230, 230);      // Chia nhỏ
            
            // Text colors
            public static readonly Color TextPrimary = Color.FromArgb(33, 33, 33);     // Đen
            public static readonly Color TextSecondary = Color.FromArgb(117, 117, 117); // Xám đậm
            public static readonly Color TextTertiary = Color.FromArgb(158, 158, 158); // Xám nhạt
            public static readonly Color TextDisabled = Color.FromArgb(189, 189, 189); // Xám rất nhạt
            public static readonly Color TextInverse = Color.FromArgb(255, 255, 255);  // Trắng
            
            // Sidebar colors
            public static readonly Color SidebarBg = Color.FromArgb(245, 246, 248);
            public static readonly Color SidebarText = Color.FromArgb(55, 65, 81);
            public static readonly Color SidebarHover = Color.FromArgb(229, 231, 235);
            public static readonly Color SidebarActive = Color.FromArgb(0, 120, 215);
            
            // Card colors for statistics
            public static readonly Color Card1 = Color.FromArgb(63, 81, 181);           // Xanh dương
            public static readonly Color Card2 = Color.FromArgb(25, 135, 84);          // Xanh lá
            public static readonly Color Card3 = Color.FromArgb(255, 112, 67);         // Cam
            public static readonly Color Card4 = Color.FromArgb(233, 30, 99);          // Hồng
            public static readonly Color Card5 = Color.FromArgb(33, 150, 243);         // Xanh nhạt
            public static readonly Color Card6 = Color.FromArgb(156, 39, 176);         // Tím
            public static readonly Color Card7 = Color.FromArgb(255, 152, 0);          // Vàng cam
            public static readonly Color Card8 = Color.FromArgb(76, 175, 80);          // Xanh sáng
        }

        // ==================== FONTS ====================
        public static class Fonts
        {
            public static readonly string FontFamily = "Segoe UI";
            
            // Font sizes
            public const float PageTitle = 24f;      // Tiêu đề trang
            public const float Heading1 = 20f;       // Heading lớn
            public const float Heading2 = 18f;       // Heading trung
            public const float Heading3 = 16f;       // Heading nhỏ
            public const float Large = 13f;          // Text lớn
            public const float Normal = 11f;         // Text bình thường
            public const float Small = 9f;           // Text nhỏ
            public const float Tiny = 8f;            // Text rất nhỏ
            
            // Font weights
            public static Font Regular(float size) => new Font(FontFamily, size, FontStyle.Regular);
            public static Font Bold(float size) => new Font(FontFamily, size, FontStyle.Bold);
            public static Font SemiBold(float size) => new Font(FontFamily, size, FontStyle.Bold);
            public static Font Italic(float size) => new Font(FontFamily, size, FontStyle.Italic);
            
            // Predefined fonts
            public static readonly Font PageTitleFont = Bold(PageTitle);
            public static readonly Font Heading1Font = Bold(Heading1);
            public static readonly Font Heading2Font = Bold(Heading2);
            public static readonly Font Heading3Font = Bold(Heading3);
            public static readonly Font LargeFont = Regular(Large);
            public static readonly Font NormalFont = Regular(Normal);
            public static readonly Font SmallFont = Regular(Small);
            public static readonly Font TinyFont = Regular(Tiny);
        }

        // ==================== SPACING & SIZING ====================
        public static class Spacing
        {
            public const int XS = 4;
            public const int SM = 8;
            public const int MD = 12;
            public const int LG = 16;
            public const int XL = 24;
            public const int XXL = 32;
        }

        // ==================== BORDER RADIUS ====================
        public static class BorderRadius
        {
            public const int SM = 4;
            public const int MD = 8;
            public const int LG = 12;
            public const int CIRCLE = 999;
        }

        // ==================== SHADOW ====================
        public static class Shadow
        {
            public const int Elevation1 = 2;
            public const int Elevation2 = 4;
            public const int Elevation3 = 8;
            public const int Elevation4 = 12;
        }

        // ==================== HELPER METHODS ====================
        
        /// <summary>
        /// Tạo một button với style hiện đại
        /// </summary>
        public static void StyleButton(Button button, Color bgColor, Color textColor = default)
        {
            if (textColor.IsEmpty)
                textColor = Colors.TextInverse;

            button.BackColor = bgColor;
            button.ForeColor = textColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = Fonts.Bold(Fonts.Normal);
            button.Cursor = Cursors.Hand;
            button.Height = 36;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(bgColor, 0.1f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bgColor, 0.2f);
        }

        /// <summary>
        /// Tạo một button với style Primary
        /// </summary>
        public static void StylePrimaryButton(Button button)
        {
            StyleButton(button, Colors.Primary, Colors.TextInverse);
        }

        /// <summary>
        /// Tạo một button với style Secondary (outline)
        /// </summary>
        public static void StyleOutlineButton(Button button, Color borderColor = default)
        {
            if (borderColor.IsEmpty)
                borderColor = Colors.Primary;

            button.BackColor = Colors.Background;
            button.ForeColor = borderColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = borderColor;
            button.Font = Fonts.Bold(Fonts.Normal);
            button.Cursor = Cursors.Hand;
            button.Height = 36;
        }

        /// <summary>
        /// Tạo một button với style Success
        /// </summary>
        public static void StyleSuccessButton(Button button)
        {
            StyleButton(button, Colors.Success, Colors.TextInverse);
        }

        /// <summary>
        /// Tạo một button với style Error
        /// </summary>
        public static void StyleErrorButton(Button button)
        {
            StyleButton(button, Colors.Error, Colors.TextInverse);
        }

        /// <summary>
        /// Tạo một button với style Warning
        /// </summary>
        public static void StyleWarningButton(Button button)
        {
            StyleButton(button, Colors.Warning, Colors.TextInverse);
        }

        /// <summary>
        /// Style cho TextBox
        /// </summary>
        public static void StyleTextBox(TextBox textBox)
        {
            textBox.BackColor = Colors.Background;
            textBox.ForeColor = Colors.TextPrimary;
            textBox.Font = Fonts.NormalFont;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Height = 32;
        }

        /// <summary>
        /// Style cho ComboBox
        /// </summary>
        public static void StyleComboBox(ComboBox comboBox)
        {
            comboBox.BackColor = Colors.Background;
            comboBox.ForeColor = Colors.TextPrimary;
            comboBox.Font = Fonts.NormalFont;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Height = 32;
        }

        /// <summary>
        /// Style cho DataGridView
        /// </summary>
        public static void StyleDataGridView(DataGridView grid)
        {
            grid.BackgroundColor = Colors.Background;
            grid.GridColor = Colors.Border;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            
            // Header styling
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Colors.Primary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Colors.TextInverse;
            grid.ColumnHeadersDefaultCellStyle.Font = Fonts.Bold(Fonts.Normal);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(Spacing.SM);
            grid.ColumnHeadersHeight = 40;
            
            // Row styling
            grid.DefaultCellStyle.Font = Fonts.NormalFont;
            grid.DefaultCellStyle.ForeColor = Colors.TextPrimary;
            grid.DefaultCellStyle.BackColor = Colors.Background;
            grid.DefaultCellStyle.Padding = new Padding(Spacing.SM);
            grid.DefaultCellStyle.SelectionBackColor = Colors.PrimaryLight;
            grid.DefaultCellStyle.SelectionForeColor = Colors.Primary;
            
            // Alternating row colors
            grid.AlternatingRowsDefaultCellStyle.BackColor = Colors.Surface;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Colors.TextPrimary;
            
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
        }

        /// <summary>
        /// Style cho Label
        /// </summary>
        public static void StyleLabel(Label label, float fontSize = Fonts.Normal, bool isBold = false)
        {
            label.Font = isBold ? Fonts.Bold(fontSize) : Fonts.Regular(fontSize);
            label.ForeColor = Colors.TextPrimary;
            label.AutoSize = true;
        }

        /// <summary>
        /// Style cho Panel
        /// </summary>
        public static void StylePanel(Panel panel, Color bgColor = default)
        {
            if (bgColor.IsEmpty)
                bgColor = Colors.Surface;

            panel.BackColor = bgColor;
            panel.ForeColor = Colors.TextPrimary;
        }

        /// <summary>
        /// Tạo một card style panel
        /// </summary>
        public static Panel CreateCardPanel(int width = 200, int height = 120)
        {
            var panel = new Panel
            {
                Width = width,
                Height = height,
                BackColor = Colors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(Spacing.MD)
            };
            
            return panel;
        }

        /// <summary>
        /// Tạo một header panel
        /// </summary>
        public static Panel CreateHeaderPanel(string title)
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Colors.Primary,
                Padding = new Padding(Spacing.LG)
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = Fonts.PageTitleFont,
                ForeColor = Colors.TextInverse,
                AutoSize = true,
                Dock = DockStyle.Left
            };

            header.Controls.Add(titleLabel);
            return header;
        }

        /// <summary>
        /// Tạo một tool bar panel
        /// </summary>
        public static Panel CreateToolbarPanel()
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Colors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(Spacing.LG)
            };

            return toolbar;
        }
    }
}
