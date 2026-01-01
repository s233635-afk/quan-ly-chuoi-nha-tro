using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// FrmContractDetailModern - Chi tiết hợp đồng với UI đẹp
    /// Hiển thị thông tin chi tiết của hợp đồng với giao diện hiện đại
    /// </summary>
    public class FrmContractDetailModern : Form
    {
        private AdminDataBLL _bll;
        private int _contractId;

        private Panel _headerPanel;
        private Panel _contentPanel;
        private Label _lblContractNumber;
        private Label _lblStatus;
        private Label _lblTenantName;
        private Label _lblRoomNumber;
        private Label _lblBranchName;
        private Label _lblStartDate;
        private Label _lblEndDate;
        private Label _lblMonthlyRent;
        private Label _lblDeposit;
        private Label _lblContractType;
        private Label _lblSignDate;
        private Label _lblNotes;
        private ModernButton _btnEdit;
        private ModernButton _btnClose;

        public FrmContractDetailModern(AdminDataBLL bll, int contractId)
        {
            _bll = bll;
            _contractId = contractId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Chi tiết Hợp đồng";
            Width = 900;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ModernTheme.Colors.Background;
            Font = ModernTheme.Fonts.NormalFont;
            FormBorderStyle = FormBorderStyle.Sizable;
            ShowIcon = false;

            // ============ HEADER PANEL ============
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = ModernTheme.Colors.Primary,
                Padding = new Padding(ModernTheme.Spacing.LG)
            };

            var headerTitle = new Label
            {
                Text = "📜 CHI TIẾT HỢP ĐỒNG",
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Heading2),
                ForeColor = ModernTheme.Colors.TextInverse,
                AutoSize = true,
                Dock = DockStyle.Top,
                Height = 30
            };

            _lblContractNumber = new Label
            {
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Large),
                ForeColor = Color.FromArgb(200, 230, 255),
                AutoSize = true,
                Dock = DockStyle.Top,
                Height = 25
            };

            var statusFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ModernTheme.Colors.Primary
            };

            _lblStatus = new Label
            {
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal),
                ForeColor = Color.White,
                AutoSize = true,
                Padding = new Padding(ModernTheme.Spacing.SM, 0, 0, 0)
            };

            statusFlow.Controls.Add(_lblStatus);

            _headerPanel.Controls.Add(statusFlow);
            _headerPanel.Controls.Add(_lblContractNumber);
            _headerPanel.Controls.Add(headerTitle);

            // ============ CONTENT PANEL ============
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.Colors.Background,
                Padding = new Padding(ModernTheme.Spacing.LG),
                AutoScroll = true
            };

            // Section 1: Thông tin khách thuê
            var section1 = CreateSection("👤 THÔNG TIN KHÁCH THUÊ");
            _lblTenantName = CreateDetailLabel();
            _lblRoomNumber = CreateDetailLabel();

            section1.Controls.Add(CreateLabelRow("Tên khách thuê:", _lblTenantName));
            section1.Controls.Add(CreateLabelRow("Phòng:", _lblRoomNumber));

            // Section 2: Thông tin chi nhánh
            var section2 = CreateSection("🏢 CHI NHÁNH");
            _lblBranchName = CreateDetailLabel();

            section2.Controls.Add(CreateLabelRow("Chi nhánh:", _lblBranchName));

            // Section 3: Ngày hợp đồng
            var section3 = CreateSection("📅 THỜI GIAN HỢP ĐỒNG");
            _lblStartDate = CreateDetailLabel();
            _lblEndDate = CreateDetailLabel();
            _lblSignDate = CreateDetailLabel();

            section3.Controls.Add(CreateLabelRow("Ngày bắt đầu:", _lblStartDate));
            section3.Controls.Add(CreateLabelRow("Ngày kết thúc:", _lblEndDate));
            section3.Controls.Add(CreateLabelRow("Ngày ký:", _lblSignDate));

            // Section 4: Thông tin tài chính
            var section4 = CreateSection("💰 THÔNG TIN TÀI CHÍNH");
            _lblMonthlyRent = CreateDetailLabel(true, ModernTheme.Colors.Success);
            _lblDeposit = CreateDetailLabel(true, ModernTheme.Colors.Warning);

            section4.Controls.Add(CreateLabelRow("Tiền thuê/tháng:", _lblMonthlyRent));
            section4.Controls.Add(CreateLabelRow("Tiền đặt cọc:", _lblDeposit));

            // Section 5: Loại hợp đồng
            var section5 = CreateSection("📋 LOẠI HỢP ĐỒNG");
            _lblContractType = CreateDetailLabel();

            section5.Controls.Add(CreateLabelRow("Loại hợp đồng:", _lblContractType));

            // Section 6: Ghi chú
            var section6 = CreateSection("📝 GHI CHÚ");
            _lblNotes = new Label
            {
                AutoSize = false,
                Text = "Chưa có ghi chú",
                Font = ModernTheme.Fonts.NormalFont,
                ForeColor = ModernTheme.Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(ModernTheme.Spacing.MD),
                BackColor = ModernTheme.Colors.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };

            section6.Controls.Add(_lblNotes);

            // Add all sections to content panel
            _contentPanel.Controls.Add(section6);
            _contentPanel.Controls.Add(section5);
            _contentPanel.Controls.Add(section4);
            _contentPanel.Controls.Add(section3);
            _contentPanel.Controls.Add(section2);
            _contentPanel.Controls.Add(section1);

            // Reverse order for proper stacking
            _contentPanel.Controls.SetChildIndex(section1, 5);
            _contentPanel.Controls.SetChildIndex(section2, 4);
            _contentPanel.Controls.SetChildIndex(section3, 3);
            _contentPanel.Controls.SetChildIndex(section4, 2);
            _contentPanel.Controls.SetChildIndex(section5, 1);
            _contentPanel.Controls.SetChildIndex(section6, 0);

            // ============ ACTION BAR ============
            var actionBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = ModernTheme.Colors.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            var actionFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = ModernTheme.Colors.Surface
            };

            _btnClose = new ModernButton
            {
                Text = "❌ Đóng",
                BackColor = ModernTheme.Colors.Error,
                ForeColor = ModernTheme.Colors.TextInverse,
                Width = 100,
                Height = 36,
                Margin = new Padding(ModernTheme.Spacing.SM)
            };
            ModernTheme.StyleButton(_btnClose, ModernTheme.Colors.Error);
            _btnClose.Click += (s, e) => Close();

            _btnEdit = new ModernButton
            {
                Text = "✏️ Sửa",
                BackColor = ModernTheme.Colors.Primary,
                ForeColor = ModernTheme.Colors.TextInverse,
                Width = 100,
                Height = 36,
                Margin = new Padding(ModernTheme.Spacing.SM)
            };
            ModernTheme.StyleButton(_btnEdit, ModernTheme.Colors.Primary);
            _btnEdit.Click += (s, e) => EditContract();

            actionFlow.Controls.Add(_btnClose);
            actionFlow.Controls.Add(_btnEdit);
            actionBar.Controls.Add(actionFlow);

            // ============ ASSEMBLE ============
            Controls.Add(actionBar);
            Controls.Add(_contentPanel);
            Controls.Add(_headerPanel);

            Load += async (s, e) => await LoadContractDetailsAsync();
        }

        private Panel CreateSection(string title)
        {
            var section = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = ModernTheme.Colors.Background,
                Padding = new Padding(0, ModernTheme.Spacing.MD, 0, ModernTheme.Spacing.MD)
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Heading3),
                ForeColor = ModernTheme.Colors.Primary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Height = 25,
                Padding = new Padding(0, 0, 0, ModernTheme.Spacing.SM)
            };

            var divider = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = ModernTheme.Colors.Divider,
                Margin = new Padding(0, 0, 0, ModernTheme.Spacing.SM)
            };

            section.Controls.Add(divider);
            section.Controls.Add(titleLabel);

            return section;
        }

        private Panel CreateLabelRow(string labelText, Label valueLabel)
        {
            var row = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false,
                BackColor = ModernTheme.Colors.Background,
                Padding = new Padding(0, ModernTheme.Spacing.SM, 0, ModernTheme.Spacing.SM)
            };

            var label = new Label
            {
                Text = labelText,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal),
                ForeColor = ModernTheme.Colors.TextSecondary,
                AutoSize = false,
                Width = 180,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft
            };

            valueLabel.Dock = DockStyle.Fill;
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;

            row.Controls.Add(valueLabel);
            row.Controls.Add(label);

            return row;
        }

        private Label CreateDetailLabel(bool isBold = false, Color? color = null)
        {
            var label = new Label
            {
                AutoSize = false,
                Font = isBold ? ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal) : ModernTheme.Fonts.NormalFont,
                ForeColor = color ?? ModernTheme.Colors.TextPrimary,
                Text = "Đang tải..."
            };

            return label;
        }

        private async Task LoadContractDetailsAsync()
        {
            try
            {
                // Simulate loading contract details
                // In real scenario, call _bll.GetContractDetailAsync(_contractId)

                _lblContractNumber.Text = "HD-2025" + _contractId;
                _lblStatus.Text = "✅ Hoạt động (Active)";
                _lblTenantName.Text = "Nguyễn Văn A";
                _lblRoomNumber.Text = "A201";
                _lblBranchName.Text = "Chi nhánh Cần Thơ 1";
                _lblStartDate.Text = "13/12/2025";
                _lblEndDate.Text = "13/12/2026";
                _lblMonthlyRent.Text = "3.000.000 VNĐ";
                _lblDeposit.Text = "6.000.000 VNĐ";
                _lblContractType.Text = "Hợp đồng thuê dài hạn (12 tháng)";
                _lblSignDate.Text = "10/12/2025";
                _lblNotes.Text = "Khách hàng uy tín, thanh toán đúng hạn. Không có yêu cầu đặc biệt.";

                await Task.Delay(500); // Simulate network delay
            }
            catch (Exception ex)
            {
                ModernDialog.Error($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        private void EditContract()
        {
            ToastNotification.Info("Tính năng sửa hợp đồng sắp được bổ sung");
        }
    }
}
