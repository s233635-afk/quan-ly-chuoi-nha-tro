using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// FrmContractManagerModern - Quản lý hợp đồng hiện đại
    /// Hiển thị dạng card, click để xem chi tiết, UI đẹp
    /// </summary>
    public class FrmContractManagerModern : Form
    {
        private AdminDataBLL _bll;
        private DataTable _rawTable;

        private Panel _headerPanel;
        private Panel _toolbarPanel;
        private Panel _filterPanel;
        private Panel _dividerPanel;
        private Panel _contentPanel;
        private FlowLayoutPanel _cardsPanel;

        private TextBox _txtSearch;
        private ComboBox _cboBranch;
        private ComboBox _cboStatus;

        private Label _lblCount;
        private Label _lblTotal;

        private ModernButton _btnAdd;
        private ModernButton _btnRefresh;

        public FrmContractManagerModern(AdminDataBLL bll)
        {
            _bll = bll;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý hợp đồng";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1400;
            Height = 800;
            BackColor = ModernTheme.Colors.Background;
            Font = ModernTheme.Fonts.NormalFont;

            // ============ HEADER PANEL ============
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = ModernTheme.Colors.Secondary,
                Padding = new Padding(ModernTheme.Spacing.LG)
            };

            var headerTitle = new Label
            {
                Text = "📜 QUẢN LÝ HỢP ĐỒNG",
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Heading2),
                ForeColor = ModernTheme.Colors.TextInverse,
                AutoSize = true,
                Dock = DockStyle.Left
            };

            var headerDesc = new Label
            {
                Text = "Quản lý tất cả hợp đồng thuê phòng, theo dõi thời hạn và trạng thái",
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Small),
                ForeColor = Color.FromArgb(200, 230, 255),
                AutoSize = true,
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.BottomLeft
            };

            _headerPanel.Controls.Add(headerDesc);
            _headerPanel.Controls.Add(headerTitle);

            // ============ TOOLBAR PANEL ============
            _toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = ModernTheme.Colors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            _btnAdd = CreateToolButton("➕ Thêm hợp đồng", ModernTheme.Colors.Success);
            _btnAdd.Click += async (s, e) => await AddNewContractAsync();

            _btnRefresh = CreateToolButton("🔄 Tải lại", ModernTheme.Colors.Secondary);
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();

            var toolActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ModernTheme.Colors.Background
            };

            toolActions.Controls.Add(_btnAdd);
            toolActions.Controls.Add(_btnRefresh);

            _toolbarPanel.Controls.Add(toolActions);

            // ============ FILTER PANEL (FIXED HEIGHT) ============
            _filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = ModernTheme.Colors.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            var lblSearch = new Label { Text = "🔍 Tìm:", AutoSize = true, ForeColor = ModernTheme.Colors.TextPrimary };
            _txtSearch = new TextBox { Width = 200, Height = 32 };
            ModernTheme.StyleTextBox(_txtSearch);
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            var lblBranch = new Label { Text = "Chi nhánh:", AutoSize = true, ForeColor = ModernTheme.Colors.TextPrimary };
            _cboBranch = new ComboBox { Width = 150, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_cboBranch);
            _cboBranch.Items.Add("Tất cả");
            _cboBranch.SelectedIndex = 0;
            _cboBranch.SelectedIndexChanged += (s, e) => ApplyFilter();

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, ForeColor = ModernTheme.Colors.TextPrimary };
            _cboStatus = new ComboBox { Width = 150, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_cboStatus);
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Hoạt động", "Kết thúc", "Tạm dừng" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            var filterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ModernTheme.Colors.Surface
            };

            filterFlow.Controls.Add(lblSearch);
            filterFlow.Controls.Add(_txtSearch);
            filterFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            filterFlow.Controls.Add(lblBranch);
            filterFlow.Controls.Add(_cboBranch);
            filterFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            filterFlow.Controls.Add(lblStatus);
            filterFlow.Controls.Add(_cboStatus);

            _filterPanel.Controls.Add(filterFlow);

            // ============ DIVIDER PANEL (NOT COVERED) ============
            _dividerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = ModernTheme.Colors.Background,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            _lblCount = new Label { AutoSize = true, Text = "📋 Tổng: 0 hợp đồng", Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal), ForeColor = ModernTheme.Colors.TextPrimary };
            _lblTotal = new Label { AutoSize = true, Text = "💰 Tổng tiền thuê: 0 VNĐ", Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal), ForeColor = ModernTheme.Colors.Success };

            var dividerFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ModernTheme.Colors.Background
            };

            dividerFlow.Controls.Add(_lblCount);
            dividerFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            dividerFlow.Controls.Add(_lblTotal);

            _dividerPanel.Controls.Add(dividerFlow);

            // ============ CONTENT PANEL WITH CARDS ============
            _cardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(ModernTheme.Spacing.MD),
                BackColor = ModernTheme.Colors.Background
            };

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.Colors.Background,
                Padding = new Padding(0)
            };

            _contentPanel.Controls.Add(_cardsPanel);

            // ============ ASSEMBLE ============
            Controls.Add(_contentPanel);
            Controls.Add(_dividerPanel);
            Controls.Add(_filterPanel);
            Controls.Add(_toolbarPanel);
            Controls.Add(_headerPanel);

            Load += async (s, e) => await LoadDataAsync();
        }

        private ModernButton CreateToolButton(string text, Color bgColor)
        {
            var btn = new ModernButton
            {
                Text = text,
                BackColor = bgColor,
                ForeColor = ModernTheme.Colors.TextInverse,
                Width = 130,
                Height = 36,
                Margin = new Padding(ModernTheme.Spacing.SM)
            };
            ModernTheme.StyleButton(btn, bgColor);
            return btn;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Load data from BLL
                var data = await _bll.GetContractListAsync();
                _rawTable = data ?? new DataTable();

                // Load branch filter
                LoadFilterData();

                // Build cards
                BuildContractCards();

                // Apply filter
                ApplyFilter();

                // Update stats
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFilterData()
        {
            try
            {
                var branches = _rawTable?.DefaultView?.ToTable(true, new[] { "BranchName" })?.Rows;
                if (branches != null && branches.Count > 0)
                {
                    _cboBranch.Items.Clear();
                    _cboBranch.Items.Add("Tất cả");
                    foreach (DataRow row in branches)
                    {
                        _cboBranch.Items.Add(row["BranchName"]);
                    }
                    _cboBranch.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void BuildContractCards()
        {
            _cardsPanel.Controls.Clear();

            if (_rawTable == null || _rawTable.Rows.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "📭 Không có dữ liệu hợp đồng",
                    Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Large),
                    ForeColor = ModernTheme.Colors.TextTertiary,
                    AutoSize = true,
                    Padding = new Padding(ModernTheme.Spacing.LG)
                };

                _cardsPanel.Controls.Add(emptyLabel);
                return;
            }

            foreach (DataRow row in _rawTable.Rows)
            {
                var card = CreateContractCard(row);
                _cardsPanel.Controls.Add(card);
            }
        }

        private Panel CreateContractCard(DataRow row)
        {
            int contractId = Convert.ToInt32(row["ContractId"]);
            string contractNumber = row["ContractNumber"].ToString();
            string tenantName = row["TenantName"].ToString();
            string roomNumber = row["RoomNumber"].ToString();
            string branchName = row["BranchName"].ToString();
            string status = row["Status"].ToString();
            decimal monthlyRent = Convert.ToDecimal(row["MonthlyRent"]);
            string startDate = Convert.ToDateTime(row["StartDate"]).ToString("dd/MM/yyyy");
            string endDate = Convert.ToDateTime(row["EndDate"]).ToString("dd/MM/yyyy");

            // Determine status color
            Color statusColor = status == "Hoạt động" ? ModernTheme.Colors.Success :
                                status == "Kết thúc" ? ModernTheme.Colors.Error :
                                ModernTheme.Colors.Warning;

            var card = new Panel
            {
                Width = 320,
                Height = 240,
                BackColor = ModernTheme.Colors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(ModernTheme.Spacing.MD),
                Padding = new Padding(ModernTheme.Spacing.MD),
                Cursor = Cursors.Hand
            };

            // Header with status color bar
            var headerBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = statusColor,
                Margin = new Padding(0, 0, 0, ModernTheme.Spacing.MD)
            };

            var contractNumberLabel = new Label
            {
                Text = contractNumber,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Large),
                ForeColor = ModernTheme.Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 25,
                AutoSize = false
            };

            var statusLabel = new Label
            {
                Text = $"🔹 {status}",
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Small),
                ForeColor = statusColor,
                Dock = DockStyle.Top,
                Height = 18,
                AutoSize = false
            };

            var tenantLabel = new Label
            {
                Text = $"👤 {tenantName}",
                Font = ModernTheme.Fonts.NormalFont,
                ForeColor = ModernTheme.Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 22,
                AutoSize = false
            };

            var roomLabel = new Label
            {
                Text = $"🚪 Phòng: {roomNumber}",
                Font = ModernTheme.Fonts.NormalFont,
                ForeColor = ModernTheme.Colors.TextSecondary,
                Dock = DockStyle.Top,
                Height = 20,
                AutoSize = false
            };

            var branchLabel = new Label
            {
                Text = $"🏢 {branchName}",
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Tiny),
                ForeColor = ModernTheme.Colors.TextTertiary,
                Dock = DockStyle.Top,
                Height = 18,
                AutoSize = false
            };

            var dateLabel = new Label
            {
                Text = $"📅 {startDate} → {endDate}",
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Small),
                ForeColor = ModernTheme.Colors.TextSecondary,
                Dock = DockStyle.Top,
                Height = 20,
                AutoSize = false,
                Padding = new Padding(0, ModernTheme.Spacing.SM, 0, 0)
            };

            var rentLabel = new Label
            {
                Text = $"💰 {monthlyRent:N0} VNĐ/tháng",
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal),
                ForeColor = ModernTheme.Colors.Success,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(0, ModernTheme.Spacing.MD, 0, 0)
            };

            card.Controls.Add(rentLabel);
            card.Controls.Add(dateLabel);
            card.Controls.Add(branchLabel);
            card.Controls.Add(roomLabel);
            card.Controls.Add(tenantLabel);
            card.Controls.Add(statusLabel);
            card.Controls.Add(contractNumberLabel);
            card.Controls.Add(headerBar);

            // Hover effect
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = ModernTheme.Colors.PrimaryLight;
                card.BorderStyle = BorderStyle.Fixed3D;
            };

            card.MouseLeave += (s, e) =>
            {
                card.BackColor = ModernTheme.Colors.Background;
                card.BorderStyle = BorderStyle.FixedSingle;
            };

            // Click to show detail
            card.Click += (s, e) => ShowContractDetail(contractId);
            contractNumberLabel.Click += (s, e) => ShowContractDetail(contractId);
            tenantLabel.Click += (s, e) => ShowContractDetail(contractId);

            return card;
        }

        private void ShowContractDetail(int contractId)
        {
            using (var detail = new FrmContractDetailModern(_bll, contractId))
            {
                detail.ShowDialog(this);
            }
        }

        private void ApplyFilter()
        {
            if (_rawTable == null || _rawTable.Rows.Count == 0)
            {
                _cardsPanel.Controls.Clear();
                return;
            }

            var filtered = _rawTable.Copy();
            var query = filtered.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(_txtSearch.Text))
            {
                string searchTerm = _txtSearch.Text.ToLower();
                query = query.Where(r =>
                    r["ContractNumber"]?.ToString()?.ToLower()?.Contains(searchTerm) == true ||
                    r["TenantName"]?.ToString()?.ToLower()?.Contains(searchTerm) == true ||
                    r["RoomNumber"]?.ToString()?.ToLower()?.Contains(searchTerm) == true
                );
            }

            // Branch filter
            if (_cboBranch.SelectedIndex > 0)
            {
                string branch = _cboBranch.SelectedItem.ToString();
                query = query.Where(r => r["BranchName"]?.ToString() == branch);
            }

            // Status filter
            if (_cboStatus.SelectedIndex > 0)
            {
                string status = _cboStatus.SelectedItem.ToString();
                query = query.Where(r => r["Status"]?.ToString() == status);
            }

            // Rebuild cards
            _cardsPanel.Controls.Clear();

            var filtered_data = query.Count() > 0 ? query.CopyToDataTable() : _rawTable.Clone();

            foreach (DataRow row in filtered_data.Rows)
            {
                var card = CreateContractCard(row);
                _cardsPanel.Controls.Add(card);
            }

            UpdateStats();
        }

        private void UpdateStats()
        {
            try
            {
                var cardCount = _cardsPanel.Controls.OfType<Panel>().Count();
                _lblCount.Text = $"📋 Tổng: {cardCount} hợp đồng";

                decimal totalRent = 0;
                foreach (DataRow row in _rawTable.Rows)
                {
                    if (decimal.TryParse(row["MonthlyRent"]?.ToString(), out decimal rent))
                        totalRent += rent;
                }

                _lblTotal.Text = $"💰 Tổng tiền thuê: {totalRent:N0} VNĐ";
            }
            catch { }
        }

        private async Task AddNewContractAsync()
        {
            MessageBox.Show("Tính năng thêm hợp đồng sắp được bổ sung.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await Task.CompletedTask;
        }
    }
}
