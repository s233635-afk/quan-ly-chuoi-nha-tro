using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// EmptyStatePanel - Display when no data is available with optional action button
    /// </summary>
    public class EmptyStatePanel : Panel
    {
        private readonly Label _iconLabel;
        private readonly Label _titleLabel;
        private readonly Label _descriptionLabel;
        private readonly Button _actionButton;

        public EmptyStatePanel()
        {
            InitializeComponent();
            
            _iconLabel = CreateIconLabel();
            _titleLabel = CreateTitleLabel();
            _descriptionLabel = CreateDescriptionLabel();
            _actionButton = CreateActionButton();

            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent,
                Padding = new Padding(ModernTheme.Spacing.XXL)
            };

            container.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Icon
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Title
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Description
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Button

            container.Controls.Add(_iconLabel, 0, 0);
            container.Controls.Add(_titleLabel, 0, 1);
            container.Controls.Add(_descriptionLabel, 0, 2);
            container.Controls.Add(_actionButton, 0, 3);

            Controls.Add(container);
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = ModernTheme.Colors.Surface;
            MinimumSize = new Size(200, 200);
        }

        private Label CreateIconLabel()
        {
            return new Label
            {
                Text = "📂",
                Font = new Font("Segoe UI Emoji", 48f),
                ForeColor = ModernTheme.Colors.TextTertiary,
                AutoSize = true,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, ModernTheme.Spacing.LG)
            };
        }

        private Label CreateTitleLabel()
        {
            return new Label
            {
                Text = "Không có dữ liệu",
                Font = ModernTheme.Fonts.Heading2Font,
                ForeColor = ModernTheme.Colors.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, ModernTheme.Spacing.SM)
            };
        }

        private Label CreateDescriptionLabel()
        {
            return new Label
            {
                Text = "Chưa có dữ liệu để hiển thị",
                Font = ModernTheme.Fonts.NormalFont,
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = true,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, ModernTheme.Spacing.LG)
            };
        }

        private Button CreateActionButton()
        {
            var button = new Button
            {
                Text = "Thêm mới",
                AutoSize = true,
                Height = 40,
                Visible = false,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.Colors.Primary,
                ForeColor = ModernTheme.Colors.TextInverse,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, ModernTheme.Spacing.MD, 0, 0),
                Padding = new Padding(ModernTheme.Spacing.LG, ModernTheme.Spacing.SM, ModernTheme.Spacing.LG, ModernTheme.Spacing.SM)
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ModernTheme.Gradient.Darken(ModernTheme.Colors.Primary, 0.1f);
            button.FlatAppearance.MouseDownBackColor = ModernTheme.Gradient.Darken(ModernTheme.Colors.Primary, 0.2f);

            return button;
        }

        // Properties for customization
        public string Icon
        {
            get => _iconLabel.Text;
            set => _iconLabel.Text = value;
        }

        public string Title
        {
            get => _titleLabel.Text;
            set => _titleLabel.Text = value;
        }

        public string Description
        {
            get => _descriptionLabel.Text;
            set => _descriptionLabel.Text = value;
        }

        public string ActionText
        {
            get => _actionButton.Text;
            set
            {
                _actionButton.Text = value;
                _actionButton.Visible = !string.IsNullOrEmpty(value);
            }
        }

        public event EventHandler ActionClicked
        {
            add => _actionButton.Click += value;
            remove => _actionButton.Click -= value;
        }

        // Convenience method to set action with delegate
        public Action OnAction
        {
            set
            {
                if (value != null)
                {
                    _actionButton.Click += (s, e) => value();
                }
            }
        }

        // Factory methods for common scenarios
        public static EmptyStatePanel ForNoData(string entityName = "dữ liệu")
        {
            return new EmptyStatePanel
            {
                Icon = "📂",
                Title = $"Chưa có {entityName}",
                Description = $"Hiện chưa có {entityName} nào để hiển thị"
            };
        }

        public static EmptyStatePanel ForNoSearchResults()
        {
            return new EmptyStatePanel
            {
                Icon = "🔍",
                Title = "Không tìm thấy kết quả",
                Description = "Thử tìm kiếm với từ khóa khác hoặc xóa bộ lọc"
            };
        }

        public static EmptyStatePanel ForError(string message = null)
        {
            return new EmptyStatePanel
            {
                Icon = "⚠️",
                Title = "Có lỗi xảy ra",
                Description = message ?? "Không thể tải dữ liệu. Vui lòng thử lại sau."
            };
        }

        public static EmptyStatePanel ForNoPermission()
        {
            return new EmptyStatePanel
            {
                Icon = "🔒",
                Title = "Không có quyền truy cập",
                Description = "Bạn không có quyền xem nội dung này"
            };
        }
    }
}
