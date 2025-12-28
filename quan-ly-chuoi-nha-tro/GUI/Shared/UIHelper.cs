using System;
using System.Windows.Forms;
using System.Drawing;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// UIHelper - Helper methods để tạo UI hiện đại
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// Tạo một notification toast (thông báo ngắn)
        /// </summary>
        public static void ShowToast(Form parent, string message, int durationMs = 3000, Color? bgColor = null)
        {
            var toastForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                BackColor = bgColor ?? ModernTheme.Colors.Success,
                Width = 400,
                Height = 60,
                Location = new Point(parent.Right - 420, parent.Bottom - 100)
            };

            var label = new Label
            {
                Text = message,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal),
                ForeColor = ModernTheme.Colors.TextInverse,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            toastForm.Controls.Add(label);
            toastForm.Show(parent);

            var timer = new Timer();
            timer.Interval = durationMs;
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();
                toastForm.Close();
                toastForm.Dispose();
            };
            timer.Start();
        }

        /// <summary>
        /// Tạo một panel với header
        /// </summary>
        public static Panel CreateHeaderedPanel(string title, Color headerColor = default)
        {
            if (headerColor == default)
                headerColor = ModernTheme.Colors.Primary;

            var container = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = headerColor,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Heading3),
                ForeColor = ModernTheme.Colors.TextInverse,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            container.Controls.Add(titleLabel);
            return container;
        }

        /// <summary>
        /// Tạo một section panel với tiêu đề
        /// </summary>
        public static Panel CreateSectionPanel(string sectionTitle)
        {
            var container = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(ModernTheme.Spacing.LG),
                BackColor = ModernTheme.Colors.Background
            };

            var titleLabel = new Label
            {
                Text = sectionTitle,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Heading3),
                ForeColor = ModernTheme.Colors.TextPrimary,
                Dock = DockStyle.Top,
                AutoSize = true,
                Height = 30
            };

            var divider = new Panel
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = ModernTheme.Colors.Divider,
                Margin = new Padding(0, ModernTheme.Spacing.MD, 0, ModernTheme.Spacing.MD)
            };

            container.Controls.Add(divider);
            container.Controls.Add(titleLabel);
            return container;
        }

        /// <summary>
        /// Tạo một form dialog hiện đại
        /// </summary>
        public static Form CreateModernDialog(string title, int width = 600, int height = 400)
        {
            var form = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                Width = width,
                Height = height,
                BackColor = ModernTheme.Colors.Background,
                Font = ModernTheme.Fonts.NormalFont,
                FormBorderStyle = FormBorderStyle.Sizable,
                ShowIcon = false
            };

            return form;
        }

        /// <summary>
        /// Tạo một table layout panel cho form input
        /// </summary>
        public static TableLayoutPanel CreateFormLayout(int rows, int columns = 2)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = columns,
                RowCount = rows,
                AutoSize = false,
                Padding = new Padding(ModernTheme.Spacing.LG),
                BackColor = ModernTheme.Colors.Background
            };

            for (int i = 0; i < columns; i++)
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            }

            for (int i = 0; i < rows; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            return layout;
        }

        /// <summary>
        /// Tạo một button bar với các button hành động
        /// </summary>
        public static Panel CreateActionBar(params (string text, Color color, EventHandler click)[] actions)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = ModernTheme.Colors.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = ModernTheme.Colors.Surface
            };

            foreach (var (text, color, click) in actions)
            {
                var btn = new ModernButton
                {
                    Text = text,
                    BackColor = color,
                    ForeColor = ModernTheme.Colors.TextInverse,
                    Width = 100,
                    Height = 36,
                    Margin = new Padding(ModernTheme.Spacing.SM)
                };
                ModernTheme.StyleButton(btn, color);
                btn.Click += click;
                flow.Controls.Add(btn);
            }

            panel.Controls.Add(flow);
            return panel;
        }

        /// <summary>
        /// Tạo một info box hiển thị thông tin
        /// </summary>
        public static Panel CreateInfoBox(string label, string value, Color accentColor = default)
        {
            if (accentColor == default)
                accentColor = ModernTheme.Colors.Primary;

            var panel = new Panel
            {
                Width = 200,
                Height = 80,
                BackColor = ModernTheme.Colors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            var labelCtrl = new Label
            {
                Text = label,
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Small),
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 20
            };

            var valueCtrl = new Label
            {
                Text = value,
                Font = ModernTheme.Fonts.Bold(16),
                ForeColor = accentColor,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel.Controls.Add(valueCtrl);
            panel.Controls.Add(labelCtrl);
            return panel;
        }

        /// <summary>
        /// Tạo một busy spinner (loading indicator)
        /// </summary>
        public static Panel CreateLoadingPanel()
        {
            var panel = new Panel
            {
                BackColor = ModernTheme.Colors.Background,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };

            var spinner = new Label
            {
                Text = "⏳ Đang tải dữ liệu...",
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal),
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = true,
                Anchor = AnchorStyles.None,
                Location = new Point(panel.Width / 2 - 80, panel.Height / 2 - 20)
            };

            panel.Controls.Add(spinner);
            return panel;
        }

        /// <summary>
        /// Tạo một search bar hiện đại
        /// </summary>
        public static TextBox CreateSearchBox(EventHandler onTextChanged = null)
        {
            var searchBox = new TextBox
            {
                PlaceholderText = "🔍 Tìm kiếm...",
                BackColor = ModernTheme.Colors.Surface,
                ForeColor = ModernTheme.Colors.TextPrimary,
                Font = ModernTheme.Fonts.NormalFont,
                Height = 36,
                BorderStyle = BorderStyle.FixedSingle,
                Width = 250
            };

            if (onTextChanged != null)
                searchBox.TextChanged += onTextChanged;

            return searchBox;
        }

        /// <summary>
        /// Tạo một quick action button
        /// </summary>
        public static Button CreateQuickActionButton(string emoji, string text, Color color = default)
        {
            if (color == default)
                color = ModernTheme.Colors.Primary;

            var btn = new ModernButton
            {
                Text = $"{emoji} {text}",
                BackColor = color,
                ForeColor = ModernTheme.Colors.TextInverse,
                Width = 120,
                Height = 36,
                Margin = new Padding(ModernTheme.Spacing.SM)
            };

            ModernTheme.StyleButton(btn, color);
            return btn;
        }

        /// <summary>
        /// Enable/Disable tất cả controls trong một container
        /// </summary>
        public static void EnableControls(Control parent, bool enabled)
        {
            foreach (Control control in parent.Controls)
            {
                control.Enabled = enabled;
                if (control.HasChildren)
                    EnableControls(control, enabled);
            }
        }

        /// <summary>
        /// Tạo một badge (nhãn nhỏ)
        /// </summary>
        public static Label CreateBadge(string text, Color backgroundColor = default)
        {
            if (backgroundColor == default)
                backgroundColor = ModernTheme.Colors.Primary;

            var badge = new Label
            {
                Text = text,
                BackColor = backgroundColor,
                ForeColor = ModernTheme.Colors.TextInverse,
                AutoSize = true,
                Padding = new Padding(ModernTheme.Spacing.SM),
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Tiny),
                TextAlign = ContentAlignment.MiddleCenter
            };

            return badge;
        }

        /// <summary>
        /// Tạo một tab control hiện đại
        /// </summary>
        public static TabControl CreateModernTabControl()
        {
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.Colors.Background,
                ForeColor = ModernTheme.Colors.TextPrimary,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal)
            };

            return tabControl;
        }
    }
}
