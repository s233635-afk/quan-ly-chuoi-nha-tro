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
    /// Xem tổng quan 1 chi nhánh: thông tin + các tab dữ liệu liên quan theo BranchId.
    /// </summary>
    public class FrmBranchOverview : Form
    {
        public enum BranchOverviewTab
        {
            Overview,
            Rooms,
            Sections,
            Tenants,
            Staff,
            Contracts,
            Deposits,
            Invoices,
            Payments,
            Utilities,
            Maintenance,
            Assets
        }

        private const int RoomDetailsCollapsedHeight = 190;
        private const int RoomDetailsExpandedHeight = 270;
        private const int TenantSplitMinLeft = 260;
        private const int TenantSplitMinRight = 320;
        private const int InvoiceSplitMinTop = 200;
        private const int InvoiceSplitMinBottom = 200;

        private readonly int _branchId;
        private readonly BranchBLL _branchBll = new BranchBLL();
        private readonly AdminDataBLL _adminBll = new AdminDataBLL();

        private FrmRoomTenantQuickView _roomQuickView;

        private Panel _header;
        private Panel _navBar;
        private Label _lblTitle;
        private Label _lblSub;
        private Button _btnEdit;
        private Button _btnClose;

        private TabControl _tabs;
        private TabPage _tabOverview;
        private TabPage _tabRooms;
        private TabPage _tabSections;
        private TabPage _tabTenants;
        private TabPage _tabStaff;
        private TabPage _tabContracts;
        private TabPage _tabDeposits;
        private TabPage _tabInvoices;
        private TabPage _tabPayments;
        private TabPage _tabUtilities;
        private TabPage _tabMaintenance;
        private TabPage _tabAssets;

        private Panel _overviewInfo;
        private FlowLayoutPanel _overviewStaffPreview;

        // private DataGridView _gridRooms; // Not used
        private FlowLayoutPanel _roomCardsHost;
        private Panel _roomDetails;
        private Label _lblRoomTitle;
        private Label _lblRoomInfo;
        private Panel _tenantDetails;
        private Label _lblTenantTitle;
        private Label _lblTenantInfo;
        private int _selectedRoomId;
        private bool _tenantVisible;
        // private DataTable _contractsAll; // Not used
        // private DataTable _tenantsHistoryAll; // Not used

        private FlowLayoutPanel _tenantCardsHost;
        private Panel _tenantDetailPanel;
        private TextBox _txtTenantSearch;
        private Label _lblTenantCount;
        private DataTable _tenantsBranch;
        private Label _lblTenantDetailTitle;
        private TextBox _txtTenantFullName;
        private TextBox _txtTenantIdentity;
        private TextBox _txtTenantPhone;
        private TextBox _txtTenantEmail;
        private DateTimePicker _dtTenantBirth;
        private TextBox _txtTenantAddress;
        private TextBox _txtTenantTempReg;
        private DateTimePicker _dtTenantTempFrom;
        private DateTimePicker _dtTenantTempTo;
        private TextBox _txtTenantFrontId;
        private TextBox _txtTenantBackId;
        private CheckBox _chkTenantActive;
        private Button _btnTenantSave;
        private DataRow _selectedTenantRow;
        private Control _selectedTenantCard;
        private int _selectedTenantId;
        private DataGridView _gridStaff;
        private DataGridView _gridContracts;
        private DataGridView _gridDeposits;
        private DataGridView _gridInvoices;
        private DataGridView _gridPayments;
        private DataGridView _gridUtilities;
        private DataGridView _gridMaintenance;
        private DataGridView _gridAssets;
        private SplitContainer _invoicePaymentSplit;
        private SplitContainer _tenantSplit;

        private DataTable _roomsAll;
        private DataTable _roomsBranch;
        private DataTable _sectionsBranch;
        private DataTable _tenantsAll;
        private DataTable _contractsBranch;
        private DataTable _depositsBranch;

        private Panel _sectionsTop;
        private FlowLayoutPanel _sectionsHost;
        private TextBox _txtSectionSearch;
        private ComboBox _cboSectionFilter;
        private Label _lblSectionsCount;

        public FrmBranchOverview(int branchId)
        {
            _branchId = branchId;
            InitializeComponent();
            Load += async (s, e) => await LoadAllAsync();
        }

        public void SelectTab(BranchOverviewTab tab)
        {
            if (_tabs == null) return;
            switch (tab)
            {
                case BranchOverviewTab.Overview:
                    _tabs.SelectedTab = _tabOverview;
                    break;
                case BranchOverviewTab.Rooms:
                    _tabs.SelectedTab = _tabRooms;
                    break;
                case BranchOverviewTab.Sections:
                    _tabs.SelectedTab = _tabSections;
                    break;
                case BranchOverviewTab.Tenants:
                    _tabs.SelectedTab = _tabTenants;
                    break;
                case BranchOverviewTab.Staff:
                    _tabs.SelectedTab = _tabStaff;
                    break;
                case BranchOverviewTab.Contracts:
                    _tabs.SelectedTab = _tabContracts;
                    break;
                case BranchOverviewTab.Deposits:
                    _tabs.SelectedTab = _tabDeposits;
                    break;
                case BranchOverviewTab.Invoices:
                    _tabs.SelectedTab = _tabInvoices;
                    break;
                case BranchOverviewTab.Payments:
                    _tabs.SelectedTab = _tabPayments;
                    break;
                case BranchOverviewTab.Utilities:
                    _tabs.SelectedTab = _tabUtilities;
                    break;
                case BranchOverviewTab.Maintenance:
                    _tabs.SelectedTab = _tabMaintenance;
                    break;
                case BranchOverviewTab.Assets:
                    _tabs.SelectedTab = _tabAssets;
                    break;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_roomQuickView != null && !_roomQuickView.IsDisposed)
            {
                try { _roomQuickView.Close(); } catch { }
                _roomQuickView = null;
            }
            base.OnFormClosing(e);
        }

        private void InitializeComponent()
        {
            Text = "Chi nhánh";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1220;
            Height = 740;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            _header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.White, Padding = new Padding(14, 10, 14, 10) };
            _lblTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Text = "Chi nhánh",
                Dock = DockStyle.Top,
                Margin = new Padding(0)
            };
            _lblSub = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(90, 90, 90),
                Text = $"BranchId: {_branchId}",
                Dock = DockStyle.Top,
                Margin = new Padding(0, 2, 0, 0)
            };

            _btnClose = new Button
            {
                Text = "Đóng",
                Width = 90,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.Click += (s, e) => Close();

            _btnEdit = new Button
            {
                Text = "Sửa",
                Width = 90,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnEdit.FlatAppearance.BorderSize = 0;
            _btnEdit.Click += async (s, e) => await EditBranchAsync();

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var headerText = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            headerText.Controls.Add(_lblSub);
            headerText.Controls.Add(_lblTitle);

            var headerButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0, 6, 0, 0)
            };
            _btnEdit.Margin = new Padding(0, 0, 10, 0);
            _btnClose.Margin = new Padding(0, 0, 0, 0);
            headerButtons.Controls.Add(_btnEdit);
            headerButtons.Controls.Add(_btnClose);

            headerLayout.Controls.Add(headerText, 0, 0);
            headerLayout.Controls.Add(headerButtons, 1, 0);

            _header.Controls.Add(headerLayout);
            _header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 3, BackColor = UiKit.Primary });

            _tabs = new TabControl { Dock = DockStyle.Fill };
            StyleTabs(_tabs);
            HideTabHeaders(_tabs);
            _tabOverview = new TabPage("Tổng quan");
            _tabRooms = new TabPage("Phòng");
            _tabSections = new TabPage("Khu/Dãy");
            _tabTenants = new TabPage("Khách thuê");
            _tabStaff = new TabPage("Nhân viên");
            _tabContracts = new TabPage("Hợp đồng");
            _tabDeposits = new TabPage("Đặt cọc");
            _tabInvoices = new TabPage("Hóa đơn/Thanh toán");
            _tabPayments = _tabInvoices;
            _tabUtilities = new TabPage("Điện/Nước/DV");
            _tabMaintenance = new TabPage("Bảo trì");
            _tabAssets = new TabPage("Tài sản");

            _tabs.TabPages.AddRange(new[] { _tabOverview, _tabRooms, _tabSections, _tabTenants, _tabStaff, _tabContracts, _tabDeposits, _tabInvoices, _tabUtilities, _tabMaintenance, _tabAssets });
            foreach (TabPage p in _tabs.TabPages) p.BackColor = Color.White;

            _overviewInfo = new Panel { Dock = DockStyle.Top, Height = 400, BackColor = Color.White, Padding = new Padding(14), AutoScroll = true };

            _tabOverview.Controls.Add(_overviewInfo);

            _roomDetails = new Panel { Dock = DockStyle.Top, Height = RoomDetailsCollapsedHeight, BackColor = Color.White, Padding = new Padding(14) };
            var roomLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            roomLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            roomLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            roomLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _lblRoomTitle = new Label { AutoSize = true, Text = "Chọn 1 phòng để xem thông tin", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0, 79, 159), Margin = new Padding(0, 0, 0, 6) };
            _lblRoomInfo = new Label { AutoSize = false, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(80, 80, 80) };

            _tenantDetails = new Panel { Dock = DockStyle.Top, Height = 86, BackColor = Color.FromArgb(245, 249, 255), Padding = new Padding(10), Visible = false, Margin = new Padding(0, 8, 0, 0) };
            _tenantDetails.MinimumSize = new Size(0, 86);
            _lblTenantTitle = new Label { AutoSize = true, Text = "Người đang sử dụng", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(0, 79, 159), Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 4) };
            _lblTenantInfo = new Label { AutoSize = false, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(70, 70, 70), AutoEllipsis = true };
            _tenantDetails.Controls.Add(_lblTenantInfo);
            _tenantDetails.Controls.Add(_lblTenantTitle);

            roomLayout.Controls.Add(_lblRoomTitle, 0, 0);
            roomLayout.Controls.Add(_lblRoomInfo, 0, 1);
            roomLayout.Controls.Add(_tenantDetails, 0, 2);
            _roomDetails.Controls.Add(roomLayout);

            _roomCardsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(16)
            };

            var roomsScrollPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            roomsScrollPanel.Controls.Add(_roomCardsHost);

            _tabRooms.Controls.Add(roomsScrollPanel);
            _tabRooms.Controls.Add(_roomDetails);

            BuildSectionsTab();
            BuildTenantTab();
            _gridStaff = CreateGrid(); _gridStaff.Dock = DockStyle.Fill; _gridStaff.CellFormatting += GridActiveCellFormatting; _tabStaff.Controls.Add(_gridStaff);
            _gridContracts = CreateGrid(); _gridContracts.Dock = DockStyle.Fill; _tabContracts.Controls.Add(_gridContracts);
            _gridDeposits = CreateGrid(); _gridDeposits.Dock = DockStyle.Fill; _tabDeposits.Controls.Add(_gridDeposits);
            _gridInvoices = CreateGrid();
            _gridPayments = CreateGrid();
            _invoicePaymentSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterWidth = 6,
                Panel1MinSize = 0,
                Panel2MinSize = 0,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            _invoicePaymentSplit.Panel1.Controls.Add(BuildLabeledGridPanel("Hóa đơn", _gridInvoices));
            _invoicePaymentSplit.Panel2.Controls.Add(BuildLabeledGridPanel("Thanh toán", _gridPayments));
            _invoicePaymentSplit.HandleCreated += (s, e) => FixInvoicePaymentSplitter();
            _invoicePaymentSplit.SizeChanged += (s, e) => FixInvoicePaymentSplitter();
            _tabInvoices.Controls.Add(_invoicePaymentSplit);

            _gridUtilities = CreateGrid(); _gridUtilities.Dock = DockStyle.Fill; _tabUtilities.Controls.Add(_gridUtilities);
            _gridMaintenance = CreateGrid(); _gridMaintenance.Dock = DockStyle.Fill; _tabMaintenance.Controls.Add(_gridMaintenance);
            _gridAssets = CreateGrid(); _gridAssets.Dock = DockStyle.Fill; _tabAssets.Controls.Add(_gridAssets);

            _navBar = BuildNavBar();
            Controls.Add(_tabs);
            Controls.Add(_navBar);
            Controls.Add(_header);
        }

        private void BuildSectionsTab()
        {
            _tabSections.Controls.Clear();
            _tabSections.BackColor = Color.White;

            _sectionsTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.White,
                Padding = new Padding(14, 12, 14, 10)
            };
            _sectionsTop.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(230, 235, 240) });

            _txtSectionSearch = new TextBox { Width = 320 };
            _txtSectionSearch.TextChanged += (s, e) => RebuildSectionCards();

            _cboSectionFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
            _cboSectionFilter.Items.AddRange(new object[] { "Tất cả", "Hoạt động", "Vô hiệu" });
            _cboSectionFilter.SelectedIndex = 0;
            _cboSectionFilter.SelectedIndexChanged += (s, e) => RebuildSectionCards();

            _lblSectionsCount = new Label
            {
                AutoSize = true,
                Text = "Tổng: 0 dãy",
                ForeColor = Color.FromArgb(90, 90, 90),
                Margin = new Padding(0, 6, 0, 0)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            var lblSearch = new Label { AutoSize = true, Text = "Tìm:", ForeColor = Color.FromArgb(90, 90, 90), Margin = new Padding(0, 6, 6, 0) };
            var lblFilter = new Label { AutoSize = true, Text = "Lọc:", ForeColor = Color.FromArgb(90, 90, 90), Margin = new Padding(18, 6, 6, 0) };
            _txtSectionSearch.Margin = new Padding(0, 2, 10, 0);
            _cboSectionFilter.Margin = new Padding(0, 2, 10, 0);
            flow.Controls.Add(lblSearch);
            flow.Controls.Add(_txtSectionSearch);
            flow.Controls.Add(lblFilter);
            flow.Controls.Add(_cboSectionFilter);
            flow.Controls.Add(_lblSectionsCount);

            _sectionsTop.Controls.Add(flow);

            _sectionsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(14),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            _tabSections.Controls.Add(_sectionsHost);
            _tabSections.Controls.Add(_sectionsTop);
        }

        private void BuildTenantTab()
        {
            _tabTenants.Controls.Clear();
            _tabTenants.BackColor = Color.White;

            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = Color.White,
                Padding = new Padding(14, 12, 14, 10)
            };
            top.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(230, 235, 240) });

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };

            var lblSearch = new Label
            {
                AutoSize = true,
                Text = "Tìm kiếm:",
                ForeColor = Color.FromArgb(90, 90, 90),
                Margin = new Padding(0, 6, 6, 0)
            };

            _txtTenantSearch = new TextBox { Width = 340, Margin = new Padding(0, 2, 12, 0) };
            _txtTenantSearch.TextChanged += (s, e) => ApplyTenantFilter();

            _lblTenantCount = new Label
            {
                AutoSize = true,
                Text = "Khách thuê: 0",
                ForeColor = Color.FromArgb(90, 90, 90),
                Margin = new Padding(0, 6, 0, 0)
            };

            flow.Controls.Add(lblSearch);
            flow.Controls.Add(_txtTenantSearch);
            flow.Controls.Add(_lblTenantCount);

            top.Controls.Add(flow);

            _tenantSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterWidth = 6,
                Panel1MinSize = 0,
                Panel2MinSize = 0,
                BackColor = Color.White
            };
            _tenantSplit.HandleCreated += (s, e) => FixTenantSplitter();
            _tenantSplit.SizeChanged += (s, e) => FixTenantSplitter();

            _tenantCardsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(6)
            };
            _tenantSplit.Panel1.Controls.Add(_tenantCardsHost);

            BuildTenantDetailsPanel();
            _tenantSplit.Panel2.Controls.Add(_tenantDetailPanel);

            _tabTenants.Controls.Add(_tenantSplit);
            _tabTenants.Controls.Add(top);
        }

        private Panel BuildNavBar()
        {
            var nav = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Color.White,
                Padding = new Padding(12, 8, 12, 8)
            };
            nav.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(230, 235, 240) });

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };

            flow.Controls.Add(MakeNavButton("Tổng quan", () => _tabs.SelectedTab = _tabOverview));
            flow.Controls.Add(MakeNavButton("Phòng", () => _tabs.SelectedTab = _tabRooms));
            flow.Controls.Add(MakeNavButton("Khu/Dãy", () => _tabs.SelectedTab = _tabSections));
            flow.Controls.Add(MakeNavButton("Khách thuê", () => _tabs.SelectedTab = _tabTenants));
            flow.Controls.Add(MakeNavButton("Nhân viên", () => _tabs.SelectedTab = _tabStaff));
            flow.Controls.Add(MakeNavButton("Hợp đồng", () => _tabs.SelectedTab = _tabContracts));
            flow.Controls.Add(MakeNavButton("Đặt cọc", () => _tabs.SelectedTab = _tabDeposits));
            flow.Controls.Add(MakeNavButton("Hóa đơn/Thanh toán", () => _tabs.SelectedTab = _tabInvoices));
            flow.Controls.Add(MakeNavButton("Điện/Nước/DV", () => _tabs.SelectedTab = _tabUtilities));
            flow.Controls.Add(MakeNavButton("Bảo trì", () => _tabs.SelectedTab = _tabMaintenance));
            flow.Controls.Add(MakeNavButton("Tài sản", () => _tabs.SelectedTab = _tabAssets));

            nav.Controls.Add(flow);
            return nav;
        }

        private void FixInvoicePaymentSplitter()
        {
            if (_invoicePaymentSplit == null) return;
            if (_invoicePaymentSplit.Orientation != Orientation.Horizontal) return;

            int total = _invoicePaymentSplit.ClientSize.Height;
            if (total <= 0) return;

            int minTop = InvoiceSplitMinTop;
            int minBottom = InvoiceSplitMinBottom;
            if (total < minTop + minBottom)
            {
                _invoicePaymentSplit.Panel1MinSize = 0;
                _invoicePaymentSplit.Panel2MinSize = 0;
                minTop = 0;
                minBottom = 0;
            }
            else
            {
                _invoicePaymentSplit.Panel1MinSize = minTop;
                _invoicePaymentSplit.Panel2MinSize = minBottom;
            }
            int maxTop = Math.Max(minTop, total - minBottom);
            int target = total / 2;

            if (target < minTop) target = minTop;
            if (target > maxTop) target = maxTop;

            if (_invoicePaymentSplit.SplitterDistance != target)
                _invoicePaymentSplit.SplitterDistance = target;
        }

        private void FixTenantSplitter()
        {
            if (_tenantSplit == null) return;
            if (_tenantSplit.Orientation != Orientation.Vertical) return;

            int total = _tenantSplit.ClientSize.Width;
            if (total <= 0) return;

            int minLeft = TenantSplitMinLeft;
            int minRight = TenantSplitMinRight;
            if (total < minLeft + minRight)
            {
                _tenantSplit.Panel1MinSize = 0;
                _tenantSplit.Panel2MinSize = 0;
                minLeft = 0;
                minRight = 0;
            }
            else
            {
                _tenantSplit.Panel1MinSize = minLeft;
                _tenantSplit.Panel2MinSize = minRight;
            }
            int maxLeft = Math.Max(minLeft, total - minRight);
            int target = total / 2;

            if (target < minLeft) target = minLeft;
            if (target > maxLeft) target = maxLeft;

            if (_tenantSplit.SplitterDistance != target)
                _tenantSplit.SplitterDistance = target;
        }

        private Button MakeNavButton(string text, Action onClick)
        {
            var btn = UiKit.MakeButton(text, UiKit.Primary, (s, e) => onClick?.Invoke(), 130);
            btn.Margin = new Padding(0, 0, 8, 0);
            return btn;
        }

        private Panel BuildLabeledGridPanel(string title, DataGridView grid)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var header = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.White, Padding = new Padding(12, 8, 12, 0) };
            var label = new Label
            {
                AutoSize = true,
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159)
            };
            header.Controls.Add(label);
            panel.Controls.Add(grid);
            panel.Controls.Add(header);
            grid.Dock = DockStyle.Fill;
            return panel;
        }

        private static void HideTabHeaders(TabControl tabControl)
        {
            if (tabControl == null) return;
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.ItemSize = new Size(0, 1);
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.Multiline = true;
        }

        private void BuildTenantDetailsPanel()
        {
            _tenantDetailPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            _lblTenantDetailTitle = new Label
            {
                AutoSize = true,
                Text = "Chọn 1 khách thuê để xem thông tin",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Margin = new Padding(0, 0, 0, 12)
            };
            _tenantDetailPanel.Controls.Add(_lblTenantDetailTitle);

            var detailLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(0, 6, 0, 0)
            };
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Control AddRow(string label, Control ctl)
            {
                var lbl = new Label
                {
                    Text = label,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.FromArgb(70, 70, 70),
                    Margin = new Padding(0, 4, 8, 4)
                };
                ctl.Dock = DockStyle.Fill;
                ctl.Margin = new Padding(0, 4, 0, 4);
                detailLayout.Controls.Add(lbl);
                detailLayout.Controls.Add(ctl);
                return ctl;
            }

            _txtTenantFullName = new TextBox();
            _txtTenantIdentity = new TextBox();
            _txtTenantPhone = new TextBox();
            _txtTenantEmail = new TextBox();
            _dtTenantBirth = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            _txtTenantAddress = new TextBox { Multiline = true, Height = 60, ScrollBars = ScrollBars.Vertical };
            _txtTenantTempReg = new TextBox();
            _dtTenantTempFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            _dtTenantTempTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            _txtTenantFrontId = new TextBox();
            _txtTenantBackId = new TextBox();
            _chkTenantActive = new CheckBox { Text = "Đang hoạt động", AutoSize = true };

            AddRow("Họ tên", _txtTenantFullName);
            AddRow("CCCD", _txtTenantIdentity);
            AddRow("SĐT", _txtTenantPhone);
            AddRow("Email", _txtTenantEmail);
            AddRow("Ngày sinh", _dtTenantBirth);
            AddRow("Địa chỉ", _txtTenantAddress);
            AddRow("Tạm trú tại", _txtTenantTempReg);
            AddRow("Tạm trú từ", _dtTenantTempFrom);
            AddRow("Tạm trú đến", _dtTenantTempTo);
            AddRow("Ảnh CCCD (mặt trước)", _txtTenantFrontId);
            AddRow("Ảnh CCCD (mặt sau)", _txtTenantBackId);
            AddRow("Trạng thái", _chkTenantActive);

            detailLayout.Controls.Add(new Label());
            _btnTenantSave = new Button
            {
                Text = "Lưu",
                Width = 120,
                Height = 36,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnTenantSave.FlatAppearance.BorderSize = 0;
            _btnTenantSave.Click += async (s, e) => await SaveTenantAsync();
            detailLayout.Controls.Add(_btnTenantSave);

            _tenantDetailPanel.Controls.Add(detailLayout);
        }

        private static DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 252);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.GridColor = Color.FromArgb(235, 240, 245);
            grid.RowTemplate.Height = 32;
            grid.DataError += (s, e) => { e.ThrowException = false; };
            return grid;
        }

        private static void StyleTabs(TabControl tabs)
        {
            if (tabs == null) return;
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.ItemSize = new Size(110, 32);
            tabs.Padding = new Point(12, 6);
            tabs.Multiline = false;

            tabs.DrawItem += (s, e) =>
            {
                var tc = (TabControl)s;
                var tab = tc.TabPages[e.Index];
                var rect = e.Bounds;

                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                Color back = selected ? UiKit.Primary : Color.FromArgb(245, 247, 250);
                Color fore = selected ? Color.White : Color.FromArgb(70, 70, 70);

                using (var b = new SolidBrush(back))
                    e.Graphics.FillRectangle(b, rect);

                using (var pen = new Pen(Color.FromArgb(220, 230, 240)))
                    e.Graphics.DrawRectangle(pen, rect);

                TextRenderer.DrawText(
                    e.Graphics,
                    tab.Text,
                    new Font("Segoe UI", 9.5f, selected ? FontStyle.Bold : FontStyle.Regular),
                    rect,
                    fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            };
        }

        private async Task EditBranchAsync()
        {
            using (var frm = new FrmBranchDetail(_branchId))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadAllAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async Task LoadAllAsync()
        {
            try
            {
                SetLoading(true);

                var branchTask = _branchBll.GetBranchByIdAsync(_branchId);
                var roomsTask = _adminBll.GetRoomsAsync();
                var sectionsTask = _adminBll.GetBranchSectionsAsync(_branchId);
                var tenantsTask = _adminBll.GetTenantsAsync();
                var staffTask = _adminBll.GetUsersByBranchAsync(_branchId);
                var contractsTask = _adminBll.GetContractsAsync();
                var depositsTask = _adminBll.GetDepositsAsync();
                var invoicesTask = _adminBll.GetInvoicesViewAsync();
                var paymentsTask = _adminBll.GetPaymentsViewAsync();
                var utilitiesTask = _adminBll.GetUtilitiesAsync();
                var maintenanceTask = _adminBll.GetMaintenanceAsync();
                var assetsTask = _adminBll.GetAssetsAsync();

                await Task.WhenAll(branchTask, roomsTask, sectionsTask, tenantsTask, staffTask, contractsTask, depositsTask, invoicesTask, paymentsTask, utilitiesTask, maintenanceTask, assetsTask);

                var branch = branchTask.Result;
                ApplyBranchInfo(branch);

                _roomsAll = roomsTask.Result ?? new DataTable();
                _roomsBranch = FilterByBranchId(_roomsAll, _branchId);
                TextFixer.ForceFixDataTable(_roomsBranch, "RoomNumber", "BranchName", "SectionName", "RoomTypeName", "StatusName");
                RenderRoomCards(_roomsBranch);

                var sections = sectionsTask.Result ?? new DataTable();
                TextFixer.ForceFixDataTable(sections, "SectionCode", "SectionName", "Description");
                _sectionsBranch = sections;
                RebuildSectionCards();

                _contractsBranch = contractsTask.Result ?? new DataTable();
                _contractsBranch = FilterByBranchId(_contractsBranch, _branchId);
                _depositsBranch = depositsTask.Result ?? new DataTable();
                _depositsBranch = FilterByBranchId(_depositsBranch, _branchId);

                _tenantsAll = tenantsTask.Result ?? new DataTable();
                TextFixer.ForceFixDataTable(_tenantsAll, "FullName", "Address", "TemporaryRegistration");
                var tenantsRelated = BuildTenantsForBranch(_tenantsAll, _contractsBranch, _depositsBranch);
                BindTenants(tenantsRelated);

                BindContracts(_gridContracts, EnrichContracts(_contractsBranch, _tenantsAll, _roomsAll));
                BindDeposits(_gridDeposits, EnrichDeposits(_depositsBranch, _tenantsAll, _roomsAll));

                var staff = staffTask.Result ?? new DataTable();
                TextFixer.ForceFixDataTable(staff, "FullName", "RoleName", "BranchName");
                BindStaff(_gridStaff, staff);
                UpdateStaffStats(staff);
                RenderOverviewStaffPreview(staff);

                var invoices = invoicesTask.Result ?? new DataTable();
                invoices = FilterByBranchId(invoices, _branchId);
                BindInvoices(_gridInvoices, invoices);

                var payments = paymentsTask.Result ?? new DataTable();
                payments = FilterByBranchId(payments, _branchId);
                BindPayments(_gridPayments, payments);

                var utilities = utilitiesTask.Result ?? new DataTable();
                utilities = FilterByBranchId(utilities, _branchId);
                BindUtilities(_gridUtilities, utilities);

                var maintenance = maintenanceTask.Result ?? new DataTable();
                maintenance = FilterByBranchId(maintenance, _branchId);
                BindMaintenance(_gridMaintenance, maintenance);

                var assets = assetsTask.Result ?? new DataTable();
                assets = FilterByBranchId(assets, _branchId);
                BindAssets(_gridAssets, assets);

                // reset room selection view
                _selectedRoomId = 0;
                _tenantVisible = false;
                _tenantDetails.Visible = false;
                if (_roomDetails != null) _roomDetails.Height = RoomDetailsCollapsedHeight;
                _lblRoomTitle.Text = "Chọn 1 phòng để xem thông tin";
                _lblRoomInfo.Text = string.Empty;
                _lblTenantInfo.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi nhánh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void SetLoading(bool loading)
        {
            UseWaitCursor = loading;
            _tabs.Enabled = !loading;
            _btnEdit.Enabled = !loading;
            _btnClose.Enabled = true;
        }

        private void ApplyBranchInfo(DataTable dt)
        {
            string code = null, name = null, address = null, phone = null, hotline = null, hours = null, desc = null;
            bool? isActive = null;

            if (dt != null && dt.Rows.Count > 0)
            {
                var r = dt.Rows[0];
                code = ReadString(r, "BranchCode");
                name = TextFixer.FixUtf8Mojibake(ReadString(r, "BranchName"));
                address = TextFixer.FixUtf8Mojibake(ReadString(r, "Address"));
                phone = ReadString(r, "Phone");
                hotline = ReadString(r, "Hotline");
                hours = ReadString(r, "OperatingHours");
                desc = TextFixer.FixUtf8Mojibake(ReadString(r, "Description"));
                if (r.Table.Columns.Contains("IsActive"))
                {
                    try { isActive = Convert.ToBoolean(r["IsActive"]); } catch { }
                }
            }

            _lblTitle.Text = string.IsNullOrWhiteSpace(code) ? (name ?? "Chi nhánh") : $"{code} - {name}";

            var statusText = isActive.HasValue ? (isActive.Value ? "Hoạt động" : "Vô hiệu") : "—";
            _lblSub.Text = $"BranchId: {_branchId} | Trạng thái: {statusText}";

            RenderOverviewInfo(code, name, address, phone, hotline, hours, desc, statusText);
        }

        private void RenderOverviewInfo(string code, string name, string address, string phone, string hotline, string hours, string desc, string statusText)
        {
            _overviewInfo.Controls.Clear();

            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            var content = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White
            };

            var title = new Label
            {
                AutoSize = true,
                Text = "Thông tin chi nhánh",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Margin = new Padding(0, 0, 0, 10),
                Dock = DockStyle.Top
            };

            var infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(250, 252, 255),
                Padding = new Padding(12, 10, 12, 10),
                Margin = new Padding(0, 0, 0, 12)
            };
            infoPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 220, 240), 1.5f))
                {
                    var rect = new Rectangle(0, 0, infoPanel.Width - 1, infoPanel.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            var infoContent = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 60, 60),
                Margin = new Padding(0),
                Dock = DockStyle.Top
            };

            string line1 = $"🏢 Mã: {NullDash(code)}   |   Tên: {NullDash(name)}";
            string line2 = $"📍 Địa chỉ: {NullDash(address)}";
            string line3 = $"☎️ Điện thoại: {NullDash(phone)}   |   Hotline: {NullDash(hotline)}";
            string line4 = $"🕐 Giờ hoạt động: {NullDash(hours)}";
            string line5 = $"✓ Trạng thái: {NullDash(statusText)}";
            string line6 = string.IsNullOrWhiteSpace(desc) ? null : $"📝 Mô tả: {desc}";
            infoContent.Text = string.Join("\n", new[] { line1, line2, line3, line4, line5, line6 }.Where(x => !string.IsNullOrWhiteSpace(x)));
            
            infoPanel.Controls.Add(infoContent);

            var staffTitle = new Label
            {
                AutoSize = true,
                Text = "Nhân viên tại chi nhánh",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Margin = new Padding(0, 12, 0, 10),
                Dock = DockStyle.Top
            };

            _overviewStaffPreview = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            content.Controls.Add(_overviewStaffPreview);
            content.Controls.Add(staffTitle);
            content.Controls.Add(infoPanel);
            content.Controls.Add(title);

            scroll.Controls.Add(content);
            _overviewInfo.Controls.Add(scroll);
        }

        private Panel BuildOverviewNotice()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(249, 252, 255),
                Padding = new Padding(14, 10, 14, 10)
            };

            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(225, 233, 245)))
                {
                    var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            var icon = new Label
            {
                AutoSize = false,
                Width = 36,
                Dock = DockStyle.Left,
                Font = new Font("Segoe UI Symbol", 18f, FontStyle.Regular),
                ForeColor = Color.FromArgb(0, 120, 215),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "ℹ️",
                Margin = new Padding(0, 0, 10, 0)
            };

            var text = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Text =
                    "Bảng phòng đã được gỡ khỏi tab Tổng quan. Vui lòng chuyển sang tab \"Phòng\" hoặc \"Khách thuê\" để xem danh sách chi tiết.",
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel.Controls.Add(text);
            panel.Controls.Add(icon);
            return panel;
        }

        private void RenderOverviewStaffPreview(DataTable staff)
        {
            if (_overviewStaffPreview == null) return;

            _overviewStaffPreview.SuspendLayout();
            _overviewStaffPreview.Controls.Clear();

            if (staff == null || staff.Rows.Count == 0)
            {
                _overviewStaffPreview.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = "Chưa có nhân viên.",
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Margin = new Padding(0, 10, 0, 0)
                });
                _overviewStaffPreview.ResumeLayout();
                return;
            }

            foreach (DataRow r in staff.Rows)
            {
                string name = TextFixer.FixUtf8Mojibake(ReadString(r, "FullName"));
                string userName = ReadString(r, "UserName");
                string phone = ReadString(r, "Phone");
                string email = ReadString(r, "Email");
                string roleName = TextFixer.FixUtf8Mojibake(ReadString(r, "RoleName"));
                bool isActive = false;
                if (staff.Columns.Contains("IsActive"))
                {
                    try { isActive = Convert.ToBoolean(r["IsActive"]); } catch { }
                }

                _overviewStaffPreview.Controls.Add(CreateStaffCard(
                    name,
                    userName,
                    phone,
                    email,
                    roleName,
                    isActive,
                    async () =>
                    {
                        int userId = TryReadInt(r, "UserId");
                        if (userId <= 0) return;
                        var latest = FindById(staff, "UserId", userId);
                        using (var frm = new FrmStaffEditor(_adminBll, latest ?? r))
                        {
                            var owner = FindForm();
                            if (frm.ShowDialog(owner ?? this) == DialogResult.OK)
                                await LoadAllAsync();
                        }
                    }));
            }

            _overviewStaffPreview.ResumeLayout();
        }

        private static Control CreateStaffCard(string fullName, string userName, string phone, string email, string roleName, bool isActive, Func<Task> onEdit)
        {
            var panel = new Panel
            {
                Width = 380,
                Height = 110,
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 10),
                Margin = new Padding(0, 0, 12, 12),
                Cursor = Cursors.Hand
            };
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 220, 240), 1.5f))
                {
                    var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            var btnEdit = new Button
            {
                Text = "✎ Sửa",
                Width = 60,
                Height = 26,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnEdit.FlatAppearance.BorderSize = 0;

            var status = new Label
            {
                AutoSize = true,
                Text = isActive ? "✓ Hoạt động" : "✗ Vô hiệu",
                ForeColor = isActive ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69),
                BackColor = isActive ? Color.FromArgb(232, 247, 239) : Color.FromArgb(252, 236, 238),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding = new Padding(8, 4, 8, 4),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var title = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 22,
                Text = $"👤 {NullDash(fullName)}",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159)
            };

            var line1 = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 18,
                Text = $"👤 Tài khoản: {NullDash(userName)}   |   ☎️ {NullDash(phone)}",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            var line2 = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 18,
                Text = $"✉️ Email: {NullDash(email)}",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            var line3 = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 18,
                Text = $"🎯 Vai trò: {NullDash(roleName)}",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            panel.Controls.Add(line3);
            panel.Controls.Add(line2);
            panel.Controls.Add(line1);
            panel.Controls.Add(title);
            panel.Controls.Add(btnEdit);
            panel.Controls.Add(status);

            panel.Resize += (s, e) =>
            {
                int right = panel.ClientSize.Width - 10;
                status.Location = new Point(right - status.Width, 10);
                btnEdit.Location = new Point(status.Left - btnEdit.Width - 6, 10);
            };
            panel.PerformLayout();

            if (onEdit != null)
            {
                async void DoEdit(object s, EventArgs e)
                {
                    try { await onEdit(); }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể sửa nhân viên.\n\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                btnEdit.Click += DoEdit;
                panel.DoubleClick += DoEdit;
                title.DoubleClick += DoEdit;
                line1.DoubleClick += DoEdit;
                line2.DoubleClick += DoEdit;
                line3.DoubleClick += DoEdit;
            }

            return panel;
        }

        private static DataTable FilterByBranchId(DataTable dt, int branchId)
        {
            if (dt == null) return new DataTable();
            if (!dt.Columns.Contains("BranchId")) return dt.Copy();
            var rows = dt.AsEnumerable().Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == branchId);
            var filtered = dt.Clone();
            foreach (var r in rows) filtered.ImportRow(r);
            return filtered;
        }

        private static DataTable EnrichContracts(DataTable contracts, DataTable tenants, DataTable rooms)
        {
            var dt = contracts?.Copy() ?? new DataTable();
            EnsureColumn(dt, "TenantName", typeof(string));
            EnsureColumn(dt, "RoomNumber", typeof(string));

            var tenantMap = BuildIdMap(tenants, "TenantId", "FullName");
            var roomMap = BuildIdMap(rooms, "RoomId", "RoomNumber");

            foreach (DataRow r in dt.Rows)
            {
                int tenantId = TryReadInt(r, "TenantId");
                int roomId = TryReadInt(r, "RoomId");
                if (tenantId > 0 && tenantMap.TryGetValue(tenantId, out var t)) r["TenantName"] = t;
                if (roomId > 0 && roomMap.TryGetValue(roomId, out var rn)) r["RoomNumber"] = rn;
            }

            return dt;
        }

        private static DataTable EnrichDeposits(DataTable deposits, DataTable tenants, DataTable rooms)
        {
            var dt = deposits?.Copy() ?? new DataTable();
            EnsureColumn(dt, "TenantName", typeof(string));
            EnsureColumn(dt, "RoomNumber", typeof(string));

            var tenantMap = BuildIdMap(tenants, "TenantId", "FullName");
            var roomMap = BuildIdMap(rooms, "RoomId", "RoomNumber");

            foreach (DataRow r in dt.Rows)
            {
                int tenantId = TryReadInt(r, "TenantId");
                int roomId = TryReadInt(r, "RoomId");
                if (tenantId > 0 && tenantMap.TryGetValue(tenantId, out var t)) r["TenantName"] = t;
                if (roomId > 0 && roomMap.TryGetValue(roomId, out var rn)) r["RoomNumber"] = rn;
            }

            return dt;
        }

        private static DataTable BuildTenantsForBranch(DataTable tenantsAll, DataTable contracts, DataTable deposits)
        {
            var ids = new HashSet<int>();
            AddIds(ids, contracts, "TenantId");
            AddIds(ids, deposits, "TenantId");

            if (ids.Count == 0) return tenantsAll?.Clone() ?? new DataTable();
            if (tenantsAll == null || !tenantsAll.Columns.Contains("TenantId")) return tenantsAll?.Copy() ?? new DataTable();

            var rows = tenantsAll.AsEnumerable().Where(r => int.TryParse(r["TenantId"]?.ToString(), out var tid) && ids.Contains(tid));
            var filtered = tenantsAll.Clone();
            foreach (var r in rows) filtered.ImportRow(r);
            return filtered;
        }

        private static void AddIds(HashSet<int> ids, DataTable dt, string col)
        {
            if (ids == null || dt == null || !dt.Columns.Contains(col)) return;
            foreach (DataRow r in dt.Rows)
            {
                if (int.TryParse(r[col]?.ToString(), out var id) && id > 0) ids.Add(id);
            }
        }

        private static void EnsureColumn(DataTable dt, string name, Type type)
        {
            if (dt == null) return;
            if (!dt.Columns.Contains(name))
                dt.Columns.Add(name, type ?? typeof(string));
        }

        private static Dictionary<int, string> BuildIdMap(DataTable dt, string idCol, string nameCol)
        {
            var map = new Dictionary<int, string>();
            if (dt == null || !dt.Columns.Contains(idCol) || !dt.Columns.Contains(nameCol)) return map;

            foreach (DataRow r in dt.Rows)
            {
                if (!int.TryParse(r[idCol]?.ToString(), out var id) || id <= 0) continue;
                var name = r[nameCol] == DBNull.Value ? null : r[nameCol]?.ToString();
                if (!map.ContainsKey(id))
                    map[id] = name;
            }
            return map;
        }

        private static void BindRooms(DataGridView grid, DataTable dt)
        {
            if (grid == null) return;
            grid.DataSource = dt;
            SetHeader(grid, "RoomNumber", "Số");
            SetHeader(grid, "SectionName", "Khu/Dãy");
            SetHeader(grid, "RoomTypeName", "Loại");
            SetHeader(grid, "RoomPrice", "Giá");
            SetHeader(grid, "StatusName", "Trạng thái");
            SetHeader(grid, "Floor", "Tầng");
            SetHeader(grid, "Area", "Diện tích");
            SetHeader(grid, "IsActive", "Kích hoạt");
            ReplaceBoolColumnWithText(grid, "IsActive");
            HideIfExists(grid, "BranchId");
            HideIfExists(grid, "BranchName");
            HideIfExists(grid, "SectionId");
            HideIfExists(grid, "RoomTypeId");
            HideIfExists(grid, "CurrentStatusId");
            HideIfExists(grid, "CreatedDate");
            HideIfExists(grid, "UpdatedDate");
        }

        private void RebuildSectionCards()
        {
            if (_sectionsHost == null) return;

            _sectionsHost.SuspendLayout();
            _sectionsHost.Controls.Clear();

            var dtRaw = _sectionsBranch ?? new DataTable();
            // Chuẩn hóa tên/Unicode và gộp dãy A/B/C (nếu có nhiều bản ghi)
            var dt = BranchSectionCatalog.NormalizeForRoomEditor(dtRaw, _branchId);
            string q = (_txtSectionSearch?.Text ?? string.Empty).Trim();
            int filterIndex = _cboSectionFilter?.SelectedIndex ?? 0;

            var rows = dt.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                string qq = q.ToLowerInvariant();
                rows = rows.Where(r =>
                {
                    string code = TextFixer.FixUtf8Mojibake(ReadString(r, "SectionCode")) ?? string.Empty;
                    string name = TextFixer.FixUtf8Mojibake(ReadString(r, "SectionName")) ?? string.Empty;
                    string desc = TextFixer.FixUtf8Mojibake(ReadString(r, "Description")) ?? string.Empty;
                    return code.ToLowerInvariant().Contains(qq)
                           || name.ToLowerInvariant().Contains(qq)
                           || desc.ToLowerInvariant().Contains(qq);
                });
            }

            if (filterIndex != 0 && dt.Columns.Contains("IsActive"))
            {
                bool wantActive = filterIndex == 1;
                rows = rows.Where(r =>
                {
                    try { return Convert.ToBoolean(r["IsActive"]) == wantActive; } catch { return wantActive; }
                });
            }

            // Chỉ giữ 3 dãy A, B, C (nếu có) và theo thứ tự A-B-C
            var list = rows.ToList();
            var abc = list
                .Where(r =>
                {
                    var c = (r["SectionCode"]?.ToString() ?? string.Empty).Trim();
                    return c.Equals("A", StringComparison.OrdinalIgnoreCase)
                        || c.Equals("B", StringComparison.OrdinalIgnoreCase)
                        || c.Equals("C", StringComparison.OrdinalIgnoreCase);
                })
                .OrderBy(r => (r["SectionCode"]?.ToString() ?? string.Empty))
                .Take(3)
                .ToList();
            if (abc.Count > 0)
                list = abc;
            if (_lblSectionsCount != null) _lblSectionsCount.Text = $"Tổng: {list.Count:N0} dãy";

            foreach (var r in list)
            {
                int sectionId = TryReadInt(r, "SectionId");
                if (sectionId <= 0) continue;
                _sectionsHost.Controls.Add(CreateSectionCard(r));
            }

            if (list.Count == 0)
            {
                _sectionsHost.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = "Không có dãy/khu phù hợp.",
                    ForeColor = Color.FromArgb(90, 90, 90)
                });
            }

            _sectionsHost.ResumeLayout();
        }

        private Control CreateSectionCard(DataRow sectionRow)
        {
            int sectionId = TryReadInt(sectionRow, "SectionId");
            string code = TextFixer.FixUtf8Mojibake(ReadString(sectionRow, "SectionCode"));
            string name = TextFixer.FixUtf8Mojibake(ReadString(sectionRow, "SectionName"));
            string desc = TextFixer.FixUtf8Mojibake(ReadString(sectionRow, "Description"));
            bool isActive = true;
            if (sectionRow.Table.Columns.Contains("IsActive"))
            {
                try { isActive = Convert.ToBoolean(sectionRow["IsActive"]); } catch { isActive = true; }
            }

            int totalRooms = CountRoomsBySection(sectionId, null);
            int activeRooms = CountRoomsBySection(sectionId, true);

            var card = new Panel
            {
                Width = 360,
                Height = 148,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 14, 14),
                Cursor = Cursors.Hand
            };

            var accent = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = isActive ? UiKit.Primary : Color.FromArgb(180, 180, 180) };

            var status = new Label
            {
                AutoSize = true,
                Text = isActive ? "Hoạt động" : "Vô hiệu",
                ForeColor = isActive ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69),
                BackColor = isActive ? Color.FromArgb(232, 247, 239) : Color.FromArgb(252, 236, 238),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(14, 14)
            };

            var title = new Label
            {
                AutoSize = false,
                Height = 26,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Padding = new Padding(14, 10, 14, 0),
                Text = string.IsNullOrWhiteSpace(code) ? (name ?? "Dãy/Khu") : $"{code} - {name}"
            };

            var sub = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                Padding = new Padding(14, 6, 14, 10),
                Text =
                    $"{(string.IsNullOrWhiteSpace(desc) ? "—" : desc)}\n" +
                    $"Phòng: {activeRooms}/{totalRooms} đang hoạt động"
            };

            var border = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            border.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(230, 235, 240), 1.2f))
                {
                    var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            border.Controls.Add(sub);
            border.Controls.Add(title);
            card.Controls.Add(border);
            card.Controls.Add(accent);
            border.Controls.Add(status);

            void OpenDetails()
            {
                try
                {
                    using (var frm = new FrmSectionOverview(_adminBll, _branchId, sectionRow, _roomsBranch, _tenantsAll, _contractsBranch))
                    {
                        if (frm.ShowDialog(this) == DialogResult.OK)
                        {
                            _ = LoadAllAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Không thể mở chi tiết khu/dãy.\n\n" + ex.Message,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            async Task EditSectionAsync()
            {
                using (var frm = new FrmBranchSectionEditor(_adminBll, sectionRow, _branchId))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        AdminEvents.NotifyDataChanged();
                        await LoadAllAsync();
                    }
                }
            }

            async Task DeleteSectionAsync()
            {
                if (sectionId <= 0) return;

                string displayName = string.IsNullOrWhiteSpace(code) ? name : $"{code} - {name}";
                var confirm = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa khu/dãy \"{displayName}\" không?",
                    "Xác nhận xóa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                try
                {
                    bool deleted = await _adminBll.DeleteBranchSectionAsync(sectionId);
                    if (deleted)
                    {
                        MessageBox.Show("Đã xóa khu/dãy.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        AdminEvents.NotifyDataChanged();
                        await LoadAllAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa khu/dãy. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa khu/dãy: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            var btnEdit = new Button
            {
                Text = "Sửa",
                Width = 56,
                Height = 28,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += async (s, e) => await EditSectionAsync();

            var btnDelete = new Button
            {
                Text = "Xóa",
                Width = 56,
                Height = 28,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += async (s, e) => await DeleteSectionAsync();

            border.Controls.Add(btnEdit);
            border.Controls.Add(btnDelete);

            card.Click += (s, e) => OpenDetails();
            title.Click += (s, e) => OpenDetails();
            sub.Click += (s, e) => OpenDetails();
            status.Click += (s, e) => OpenDetails();
            accent.Click += (s, e) => OpenDetails();
            card.DoubleClick += (s, e) => OpenDetails();

            void LayoutActionControls()
            {
                int right = border.ClientSize.Width - 14;
                btnDelete.Location = new Point(right - btnDelete.Width, 14);
                right -= btnDelete.Width + 6;
                btnEdit.Location = new Point(right - btnEdit.Width, 14);
                right -= btnEdit.Width + 6;
                status.Location = new Point(Math.Max(14, right - status.Width), 14);
            }

            border.Resize += (s, e) => LayoutActionControls();
            LayoutActionControls();

            return card;
        }

        private int CountRoomsBySection(int sectionId, bool? onlyActive)
        {
            if (_roomsBranch == null || !_roomsBranch.Columns.Contains("SectionId")) return 0;
            var rows = _roomsBranch.AsEnumerable().Where(r => int.TryParse(r["SectionId"]?.ToString(), out var sid) && sid == sectionId);
            if (onlyActive.HasValue && _roomsBranch.Columns.Contains("IsActive"))
            {
                rows = rows.Where(r =>
                {
                    try { return Convert.ToBoolean(r["IsActive"]) == onlyActive.Value; } catch { return false; }
                });
            }
            return rows.Count();
        }

        private void BindTenants(DataTable dt)
        {
            _tenantsBranch = dt?.Copy() ?? new DataTable();
            ApplyTenantFilter();
        }

        private static void BindStaff(DataGridView grid, DataTable dt)
        {
            if (grid == null) return;
            grid.DataSource = dt;
            SetHeader(grid, "UserId", "ID");
            SetHeader(grid, "UserName", "Tài khoản");
            SetHeader(grid, "FullName", "Họ tên");
            SetHeader(grid, "Phone", "SĐT");
            SetHeader(grid, "Email", "Email");
            SetHeader(grid, "RoleName", "Vai trò");
            SetHeader(grid, "BranchName", "Chi nhánh");
            SetHeader(grid, "BranchId", "BranchId");
            SetHeader(grid, "IsActive", "Trạng thái");
            ReplaceBoolColumnWithText(grid, "IsActive");
            HideIfExists(grid, "RoleId");
            HideIfExists(grid, "CreatedDate");
            HideIfExists(grid, "UpdatedDate");

            // Ưu tiên hiển thị chi nhánh cạnh vai trò
            TrySetDisplayIndex(grid, "RoleName", 5);
            TrySetDisplayIndex(grid, "BranchName", 6);
            TrySetDisplayIndex(grid, "BranchId", 7);
            TrySetDisplayIndex(grid, "IsActive", 8);
        }

        private static void TrySetDisplayIndex(DataGridView grid, string columnName, int index)
        {
            if (grid == null || string.IsNullOrWhiteSpace(columnName)) return;
            if (!grid.Columns.Contains(columnName)) return;
            try { grid.Columns[columnName].DisplayIndex = index; } catch { }
        }

        private void UpdateStaffStats(DataTable staff)
        {
            // Stats updated in RenderOverviewStaffPreview
        }

        private void GridActiveCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null) return;
            if (grid.Columns[e.ColumnIndex].Name != "IsActive") return;
            if (e.Value == null || e.Value == DBNull.Value) return;
            bool isActive = false;
            try { isActive = Convert.ToBoolean(e.Value); } catch { }
            e.Value = isActive ? "Hoạt động" : "Vô hiệu";
            e.CellStyle.ForeColor = isActive ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
            e.CellStyle.BackColor = isActive ? Color.FromArgb(232, 247, 239) : Color.FromArgb(252, 236, 238);
            e.CellStyle.SelectionBackColor = isActive ? Color.FromArgb(214, 237, 223) : Color.FromArgb(244, 214, 220);
            e.FormattingApplied = true;
        }

        private void HandleRoomClickFromGrid(DataGridView grid)
        {
            if (grid == null || grid.CurrentRow == null || grid.CurrentRow.DataBoundItem == null) return;

            DataRow row = null;
            if (grid.CurrentRow.DataBoundItem is DataRowView drv)
                row = drv.Row;
            if (row == null) return;
            var roomRow = row;

            int roomId = TryReadInt(row, "RoomId");
            if (roomId <= 0) return;

            if (_selectedRoomId != roomId)
            {
                _selectedRoomId = roomId;
                _tenantVisible = false;
                _tenantDetails.Visible = false;
                if (_roomDetails != null) _roomDetails.Height = RoomDetailsCollapsedHeight;
                RenderRoomInfo(roomId);
                ShowRoomQuickView(roomId);
                return;
            }

            _tenantVisible = !_tenantVisible;
            _tenantDetails.Visible = _tenantVisible;
            if (_roomDetails != null) _roomDetails.Height = _tenantVisible ? RoomDetailsExpandedHeight : RoomDetailsCollapsedHeight;
            if (_tenantVisible)
            {
                var contract = FindActiveContractForRoom(roomId);
                DataRow tenant = null;
                if (contract != null)
                {
                    int tenantId = TryReadInt(contract, "TenantId");
                    if (tenantId > 0)
                        tenant = FindById(_tenantsAll, "TenantId", tenantId);
                }

                ShowRoomQuickView(roomId, roomRow, tenant, contract);
            }
        }

        private async Task RefreshRoomQuickViewAsync(int roomId)
        {
            await LoadAllAsync();
            _tabs.SelectedTab = _tabRooms;
            // SelectRoomInRoomsGrid(roomId); // Method commented out
            ShowRoomQuickView(roomId);
        }

        private void ShowRoomQuickView(int roomId)
        {
            var roomRow = FindById(_roomsBranch, "RoomId", roomId);
            var contractRow = FindActiveContractForRoom(roomId);
            DataRow tenantRow = null;
            if (contractRow != null)
            {
                int tenantId = TryReadInt(contractRow, "TenantId");
                if (tenantId > 0)
                    tenantRow = FindById(_tenantsAll, "TenantId", tenantId);
            }

            ShowRoomQuickView(roomId, roomRow, tenantRow, contractRow);
        }

        private void ShowRoomQuickView(int roomId, DataRow roomRow, DataRow tenantRow, DataRow contractRow)
        {
            if (_roomQuickView == null || _roomQuickView.IsDisposed)
            {
                _roomQuickView = new FrmRoomTenantQuickView(_adminBll, RefreshRoomQuickViewAsync);
                try
                {
                    var ownerRect = RectangleToScreen(ClientRectangle);
                    int x = Math.Max(0, ownerRect.Right - _roomQuickView.Width - 16);
                    int y = Math.Max(0, ownerRect.Top + 90);
                    _roomQuickView.Location = new Point(x, y);
                }
                catch
                {
                    // ignore positioning issues
                }
                _roomQuickView.Show(this);
            }
            else if (!_roomQuickView.Visible)
            {
                _roomQuickView.Show(this);
            }

            _roomQuickView.UpdateData(roomId, roomRow, tenantRow, contractRow);
            _roomQuickView.BringToFront();
        }

        // private void SelectRoomInRoomsGrid(int roomId)
        // {
        //     if (_gridRooms == null || _gridRooms.Rows == null) return;
        //
        //     foreach (DataGridViewRow r in _gridRooms.Rows)
        //     {
        //         if (r?.DataBoundItem is DataRowView drv)
        //         {
        //             int id = 0;
        //             try { id = Convert.ToInt32(drv.Row["RoomId"]); } catch { id = 0; }
        //             if (id == roomId)
        //             {
        //                 _gridRooms.ClearSelection();
        //                 r.Selected = true;
        //                 _gridRooms.CurrentCell = r.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => c.Visible) ?? r.Cells[0];
        //                 _selectedRoomId = roomId;
        //                 _tenantVisible = false;
        //                 _tenantDetails.Visible = false;
        //                 RenderRoomInfo(roomId);
        //                 _gridRooms.FirstDisplayedScrollingRowIndex = Math.Max(0, r.Index - 2);
        //                 return;
        //             }
        //         }
        //     }
        // }

        private void GridRoomCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null) return;

            var colName = grid.Columns[e.ColumnIndex].Name;
            if (e.Value == null || e.Value == DBNull.Value) return;

            if (string.Equals(colName, "RoomPrice", StringComparison.OrdinalIgnoreCase))
            {
                if (decimal.TryParse(e.Value.ToString(), out var money))
                {
                    e.Value = money.ToString("N0");
                    e.FormattingApplied = true;
                }
                return;
            }

            if (string.Equals(colName, "IsActive", StringComparison.OrdinalIgnoreCase))
            {
                bool isActive = false;
                try { isActive = Convert.ToBoolean(e.Value); } catch { }
                e.Value = isActive ? "Có" : "Không";
                e.CellStyle.ForeColor = isActive ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
                e.FormattingApplied = true;
                return;
            }

            if (string.Equals(colName, "StatusName", StringComparison.OrdinalIgnoreCase))
            {
                string status = e.Value?.ToString() ?? string.Empty;
                Color fore = Color.FromArgb(60, 60, 60);
                Color back = Color.White;

                if (status.IndexOf("Trống", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    fore = Color.FromArgb(40, 167, 69);
                    back = Color.FromArgb(232, 247, 239);
                }
                else if (status.IndexOf("Đang", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    fore = Color.FromArgb(255, 152, 0);
                    back = Color.FromArgb(255, 243, 224);
                }
                else if (status.IndexOf("Bảo", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    fore = Color.FromArgb(103, 58, 183);
                    back = Color.FromArgb(237, 231, 246);
                }
                else if (status.IndexOf("cọc", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    fore = Color.FromArgb(0, 122, 204);
                    back = Color.FromArgb(232, 244, 252);
                }

                e.CellStyle.ForeColor = fore;
                e.CellStyle.BackColor = back;
                e.FormattingApplied = true;
                return;
            }
        }

        private void RenderRoomInfo(int roomId)
        {
            try
            {
                if (_roomsBranch == null)
                {
                    _lblRoomTitle.Text = "Dữ liệu phòng chưa được tải";
                    _lblRoomInfo.Text = string.Empty;
                    return;
                }

                var r = FindById(_roomsBranch, "RoomId", roomId);
                if (r == null)
                {
                    _lblRoomTitle.Text = "Không tìm thấy phòng";
                    _lblRoomInfo.Text = string.Empty;
                    return;
                }

                string number = ReadString(r, "RoomNumber");
                string section = ReadString(r, "SectionName");
                string type = ReadString(r, "RoomTypeName");
                string status = ReadString(r, "StatusName");
                string price = FormatMoney(ReadString(r, "RoomPrice"));
                string floor = ReadString(r, "Floor");
                string area = ReadString(r, "Area");
                bool isActive = true;
                if (r.Table.Columns.Contains("IsActive"))
                {
                    try { isActive = Convert.ToBoolean(r["IsActive"]); } catch { isActive = true; }
                }

                _lblRoomTitle.Text = $"Phòng {NullDash(number)}";
                _lblRoomInfo.Text =
                    $"Khu/Dãy: {NullDash(section)}   |   Loại: {NullDash(type)}   |   Trạng thái: {NullDash(status)}\n" +
                    $"Giá: {NullDash(price)}   |   Tầng: {NullDash(floor)}   |   Diện tích: {NullDash(area)}\n" +
                    $"Kích hoạt: {(isActive ? "Có" : "Không")}   |   (Click lần 2 để xem người đang sử dụng)";
            }
            catch (Exception ex)
            {
                _lblRoomTitle.Text = "Lỗi hiển thị thông tin phòng";
                _lblRoomInfo.Text = ex.Message;
            }
        }

        private void RenderTenantInfo(int roomId)
        {
            var contract = FindActiveContractForRoom(roomId);
            if (contract == null)
            {
                _lblTenantInfo.Text = "Phòng hiện chưa có người sử dụng (không có hợp đồng Active/Extended).";
                return;
            }

            int tenantId = TryReadInt(contract, "TenantId");
            var tenant = FindById(_tenantsAll, "TenantId", tenantId);

            string tenantName = tenant != null ? ReadString(tenant, "FullName") : null;
            string tenantPhone = tenant != null ? ReadString(tenant, "PhoneNumber") : null;
            string tenantIdCard = tenant != null ? ReadString(tenant, "IdentityCard") : null;

            string contractNo = ReadString(contract, "ContractNumber");
            string start = FormatDate(ReadString(contract, "StartDate"));
            string end = FormatDate(ReadString(contract, "EndDate"));
            string st = ReadString(contract, "Status");

            _lblTenantInfo.Text =
                $"Họ tên: {NullDash(tenantName)}   |   SĐT: {NullDash(tenantPhone)}   |   CCCD: {NullDash(tenantIdCard)}\n" +
                $"Hợp đồng: {NullDash(contractNo)}   |   {NullDash(st)}   |   {NullDash(start)} → {NullDash(end)}";
        }

        private DataRow FindActiveContractForRoom(int roomId)
        {
            if (_contractsBranch == null || !_contractsBranch.Columns.Contains("RoomId")) return null;

            var rows = _contractsBranch.AsEnumerable()
                .Where(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == roomId)
                .Where(r =>
                {
                    var st = r.Table.Columns.Contains("Status") ? (r["Status"]?.ToString() ?? string.Empty) : string.Empty;
                    return string.Equals(st, "Active", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(st, "Extended", StringComparison.OrdinalIgnoreCase);
                });

            DataRow best = null;
            DateTime bestStart = DateTime.MinValue;
            foreach (var r in rows)
            {
                DateTime start;
                if (DateTime.TryParse(r["StartDate"]?.ToString(), out start))
                {
                    if (best == null || start > bestStart) { best = r; bestStart = start; }
                }
                else if (best == null)
                {
                    best = r;
                }
            }
            return best;
        }

        private static DataRow FindById(DataTable dt, string col, int id)
        {
            if (dt == null || !dt.Columns.Contains(col)) return null;
            foreach (DataRow r in dt.Rows)
            {
                if (int.TryParse(r[col]?.ToString(), out var rid) && rid == id)
                    return r;
            }
            return null;
        }

        private static void BindContracts(DataGridView grid, DataTable dt)
        {
            if (grid == null) return;
            grid.DataSource = dt;
            SetHeader(grid, "ContractId", "ID");
            SetHeader(grid, "ContractNumber", "Số HĐ");
            SetHeader(grid, "TenantName", "Khách thuê");
            SetHeader(grid, "RoomNumber", "Phòng");
            SetHeader(grid, "StartDate", "Bắt đầu");
            SetHeader(grid, "EndDate", "Kết thúc");
            SetHeader(grid, "RentalPrice", "Giá thuê");
            SetHeader(grid, "DepositRequired", "Cọc");
            SetHeader(grid, "Status", "Trạng thái");
            HideIfExists(grid, "BranchId");
            HideIfExists(grid, "TenantId");
            HideIfExists(grid, "RoomId");
            HideIfExists(grid, "Terms");
            HideIfExists(grid, "ContractPdfPath");
        }

        private static void BindDeposits(DataGridView grid, DataTable dt)
        {
            if (grid == null) return;
            grid.DataSource = dt;
            SetHeader(grid, "DepositId", "ID");
            SetHeader(grid, "TenantName", "Khách thuê");
            SetHeader(grid, "RoomNumber", "Phòng");
            SetHeader(grid, "DepositAmount", "Tiền cọc");
            SetHeader(grid, "DepositDate", "Ngày cọc");
            SetHeader(grid, "DepositType", "Loại");
            SetHeader(grid, "Status", "Trạng thái");
            SetHeader(grid, "ReturnedAmount", "Tiền hoàn");
            SetHeader(grid, "ReturnedDate", "Ngày hoàn");
            HideIfExists(grid, "BranchId");
            HideIfExists(grid, "TenantId");
            HideIfExists(grid, "RoomId");
        }

        private static void BindInvoices(DataGridView grid, DataTable dt)
        {
            if (grid == null) return;
            grid.DataSource = dt;
            SetHeader(grid, "InvoiceId", "ID");
            SetHeader(grid, "InvoiceNumber", "Số HĐ");
            SetHeader(grid, "TenantName", "Khách thuê");
            SetHeader(grid, "RoomNumber", "Phòng");
            SetHeader(grid, "TotalAmount", "Tổng");
            SetHeader(grid, "PaidAmount", "Đã trả");
            SetHeader(grid, "RemainingAmount", "Còn nợ");
            SetHeader(grid, "Status", "Trạng thái");
            HideIfExists(grid, "BranchId");
        }

        private static void BindPayments(DataGridView grid, DataTable dt)
        {
            if (grid == null) return;
            grid.DataSource = dt;
            SetHeader(grid, "PaymentId", "ID");
            SetHeader(grid, "InvoiceNumber", "Hóa đơn");
            SetHeader(grid, "TenantName", "Khách thuê");
            SetHeader(grid, "RoomNumber", "Phòng");
            SetHeader(grid, "PaymentDate", "Ngày thu");
            SetHeader(grid, "PaymentAmount", "Số tiền");
            SetHeader(grid, "PaymentMethod", "Hình thức");
            HideIfExists(grid, "BranchId");
        }

        private static void BindUtilities(DataGridView grid, DataTable dt)
        {
            if (grid == null) return;
            grid.DataSource = dt;
            SetHeader(grid, "ReadingId", "ID");
            SetHeader(grid, "RoomNumber", "Phòng");
            SetHeader(grid, "UtilityName", "Dịch vụ");
            SetHeader(grid, "ReadingDate", "Ngày ghi");
            SetHeader(grid, "UsageAmount", "Sử dụng");
            SetHeader(grid, "TotalCost", "Thành tiền");
            HideIfExists(grid, "BranchId");
        }

        private static void BindMaintenance(DataGridView grid, DataTable dt)
        {
            if (grid == null) return;
            grid.DataSource = dt;
            SetHeader(grid, "TicketId", "ID");
            SetHeader(grid, "TicketNumber", "Số phiếu");
            SetHeader(grid, "RoomNumber", "Phòng");
            SetHeader(grid, "IssueDescription", "Nội dung");
            SetHeader(grid, "Priority", "Ưu tiên");
            SetHeader(grid, "Status", "Trạng thái");
            HideIfExists(grid, "BranchId");
        }

        private static void BindAssets(DataGridView grid, DataTable dt)
        {
            if (grid == null) return;
            grid.DataSource = dt;
            SetHeader(grid, "AssetId", "ID");
            SetHeader(grid, "AssetCode", "Mã");
            SetHeader(grid, "AssetName", "Tên");
            SetHeader(grid, "Category", "Nhóm");
            SetHeader(grid, "RoomNumber", "Phòng");
            SetHeader(grid, "Quantity", "SL");
            SetHeader(grid, "Condition", "Tình trạng");
            HideIfExists(grid, "BranchId");
        }

        private static void SetHeader(DataGridView grid, string col, string header)
        {
            if (grid == null || grid.Columns == null) return;
            if (!grid.Columns.Contains(col)) return;
            grid.Columns[col].HeaderText = header ?? col;
        }

        private static void HideIfExists(DataGridView grid, string col)
        {
            if (grid == null || grid.Columns == null) return;
            if (grid.Columns.Contains(col)) grid.Columns[col].Visible = false;
        }

        private string ReadString(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return null;
            var v = r[col];
            if (v == DBNull.Value || v == null) return null;
            return TextFixer.ForceFixUtf8Mojibake(v.ToString());
        }

        private static int TryReadInt(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return 0;
            if (int.TryParse(r[col]?.ToString(), out var v)) return v;
            return 0;
        }

        private static void ReplaceBoolColumnWithText(DataGridView grid, string columnName)
        {
            if (grid == null || grid.Columns == null) return;
            if (string.IsNullOrWhiteSpace(columnName)) return;
            if (!grid.Columns.Contains(columnName)) return;

            var col = grid.Columns[columnName];
            if (!(col is DataGridViewCheckBoxColumn)) return;

            int idx = col.Index;
            string header = col.HeaderText;
            string dataProp = col.DataPropertyName;
            bool visible = col.Visible;
            float fillWeight = col.FillWeight;

            grid.Columns.Remove(col);

            var textCol = new DataGridViewTextBoxColumn
            {
                Name = columnName,
                HeaderText = header,
                DataPropertyName = string.IsNullOrWhiteSpace(dataProp) ? columnName : dataProp,
                ReadOnly = true,
                Visible = visible,
                FillWeight = fillWeight
            };

            grid.Columns.Insert(Math.Min(idx, grid.Columns.Count), textCol);
        }

        private static string FormatMoney(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (decimal.TryParse(raw, out var d))
                return d.ToString("N0");
            return raw;
        }

        private static string FormatDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (DateTime.TryParse(raw, out var dt))
                return dt.ToString("dd/MM/yyyy");
            return raw;
        }

        private static string NullDash(string s) => string.IsNullOrWhiteSpace(s) ? "—" : s.Trim();

        private void ApplyTenantFilter()
        {
            if (_tenantsBranch == null) return;

            string searchText = (_txtTenantSearch?.Text ?? "").Trim().ToLower();
            var view = new DataView(_tenantsBranch);

            if (!string.IsNullOrEmpty(searchText))
            {
                var escaped = searchText.Replace("'", "''");
                view.RowFilter = $"Convert(FullName, 'System.String') LIKE '%{escaped}%' OR Convert(PhoneNumber, 'System.String') LIKE '%{escaped}%' OR Convert(IdentityCard, 'System.String') LIKE '%{escaped}%'";
            }
            else
            {
                view.RowFilter = "";
            }

            var filtered = view.ToTable();
            _lblTenantCount.Text = $"Khách thuê: {filtered.Rows.Count}";
            RenderTenantCards(filtered);
            if (_selectedTenantId > 0)
            {
                var match = FindById(filtered, "TenantId", _selectedTenantId);
                if (match != null)
                {
                    ShowTenantDetails(match, _selectedTenantCard);
                }
                else
                {
                    ClearTenantDetails();
                }
            }
        }

        private void RenderTenantCards(DataTable tenants)
        {
            if (_tenantCardsHost == null) return;
            _tenantCardsHost.SuspendLayout();
            _tenantCardsHost.Controls.Clear();

            if (tenants == null || tenants.Rows.Count == 0)
            {
                _tenantCardsHost.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = "Chưa có khách thuê.",
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Margin = new Padding(8, 6, 0, 0)
                });
                _tenantCardsHost.ResumeLayout();
                return;
            }

            foreach (DataRow row in tenants.Rows)
            {
                var card = CreateTenantCard(row);
                _tenantCardsHost.Controls.Add(card);
                if (_selectedTenantId == 0)
                {
                    ShowTenantDetails(row, card);
                }
            }

            _tenantCardsHost.ResumeLayout();
        }

        private Control CreateTenantCard(DataRow tenantRow)
        {
            var panel = new Panel
            {
                Width = 240,
                Height = 140,
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(12),
                Cursor = Cursors.Hand
            };
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 220, 230)))
                {
                    var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            int tenantId = TryReadInt(tenantRow, "TenantId");
            string name = TextFixer.FixUtf8Mojibake(ReadString(tenantRow, "FullName") ?? "");
            string phone = ReadString(tenantRow, "PhoneNumber");
            string identity = ReadString(tenantRow, "IdentityCard");
            string email = ReadString(tenantRow, "Email");
            string address = TextFixer.FixUtf8Mojibake(ReadString(tenantRow, "Address"));
            string status = (tenantRow.Table.Columns.Contains("IsActive") && bool.TryParse(tenantRow["IsActive"]?.ToString(), out var active) && active) ? "Đang ở" : "Tạm ngưng";

            var lblName = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Text = $"{tenantId} · {NullDash(name)}",
                ForeColor = Color.FromArgb(0, 79, 159)
            };

            var lblInfo = new Label
            {
                Dock = DockStyle.Top,
                Height = 46,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70),
                Text = $"☎ {NullDash(phone)}   |   CCCD: {NullDash(identity)}\n✉ {NullDash(email)}",
                AutoSize = false
            };

            var lblAddress = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = $"🏠 {NullDash(address)}",
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = Color.FromArgb(90, 90, 90)
            };

            var lblStatus = new Label
            {
                AutoSize = true,
                Text = status,
                BackColor = status.Contains("Đang") ? Color.FromArgb(232, 247, 239) : Color.FromArgb(252, 236, 238),
                ForeColor = status.Contains("Đang") ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding = new Padding(6, 2, 6, 2),
                Location = new Point(12, panel.Height - 28)
            };

            panel.Controls.Add(lblStatus);
            panel.Controls.Add(lblAddress);
            panel.Controls.Add(lblInfo);
            panel.Controls.Add(lblName);

            void HandleClick(object sender, EventArgs args)
            {
                ShowTenantDetails(tenantRow, panel);
            }

            panel.Click += HandleClick;
            foreach (Control ctl in panel.Controls)
            {
                ctl.Click += HandleClick;
            }

            return panel;
        }

        private void ShowTenantDetails(DataRow tenantRow, Control card)
        {
            if (tenantRow == null)
            {
                ClearTenantDetails();
                return;
            }

            _selectedTenantRow = tenantRow;
            _selectedTenantId = TryReadInt(tenantRow, "TenantId");
            _lblTenantDetailTitle.Text = $"Khách thuê #{_selectedTenantId}";

            _txtTenantFullName.Text = ReadString(tenantRow, "FullName") ?? "";
            _txtTenantIdentity.Text = ReadString(tenantRow, "IdentityCard") ?? "";
            _txtTenantPhone.Text = ReadString(tenantRow, "PhoneNumber") ?? "";
            _txtTenantEmail.Text = ReadString(tenantRow, "Email") ?? "";
            SetDatePicker(_dtTenantBirth, ReadString(tenantRow, "BirthDate"));
            _txtTenantAddress.Text = TextFixer.FixUtf8Mojibake(ReadString(tenantRow, "Address") ?? "");
            _txtTenantTempReg.Text = TextFixer.FixUtf8Mojibake(ReadString(tenantRow, "TemporaryRegistration") ?? "");
            SetDatePicker(_dtTenantTempFrom, ReadString(tenantRow, "TemporaryRegistrationDate"));
            SetDatePicker(_dtTenantTempTo, ReadString(tenantRow, "TemporaryRegistrationExpiry"));
            _txtTenantFrontId.Text = ReadString(tenantRow, "FrontIdPhoto") ?? "";
            _txtTenantBackId.Text = ReadString(tenantRow, "BackIdPhoto") ?? "";
            _chkTenantActive.Checked = tenantRow.Table.Columns.Contains("IsActive") && bool.TryParse(tenantRow["IsActive"]?.ToString(), out var active) && active;

            HighlightSelectedCard(card);
        }

        private void HighlightSelectedCard(Control card)
        {
            if (_selectedTenantCard != null && !_selectedTenantCard.IsDisposed)
            {
                _selectedTenantCard.BackColor = Color.White;
            }
            _selectedTenantCard = card;
            if (_selectedTenantCard != null)
            {
                _selectedTenantCard.BackColor = Color.FromArgb(248, 252, 255);
            }
        }

        private void ClearTenantDetails()
        {
            _selectedTenantId = 0;
            _selectedTenantRow = null;
            _lblTenantDetailTitle.Text = "Chọn 1 khách thuê để xem thông tin";
            _txtTenantFullName.Text = "";
            _txtTenantIdentity.Text = "";
            _txtTenantPhone.Text = "";
            _txtTenantEmail.Text = "";
            _dtTenantBirth.Checked = false;
            _txtTenantAddress.Text = "";
            _txtTenantTempReg.Text = "";
            _dtTenantTempFrom.Checked = false;
            _dtTenantTempTo.Checked = false;
            _txtTenantFrontId.Text = "";
            _txtTenantBackId.Text = "";
            _chkTenantActive.Checked = false;
            _selectedTenantCard = null;
        }

        private void SetDatePicker(DateTimePicker picker, string rawValue)
        {
            if (picker == null) return;
            if (DateTime.TryParse(rawValue, out var dt))
            {
                picker.Value = dt;
                picker.Checked = true;
            }
            else
            {
                picker.Checked = false;
            }
        }

        private void RenderRoomCards(DataTable roomsTable)
        {
            _roomCardsHost.SuspendLayout();
            _roomCardsHost.Controls.Clear();

            if (roomsTable == null || roomsTable.Rows.Count == 0)
            {
                _roomCardsHost.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = "Không có phòng nào.",
                    ForeColor = Color.FromArgb(90, 90, 90)
                });
                _roomCardsHost.ResumeLayout();
                return;
            }

            foreach (DataRow row in roomsTable.Rows)
            {
                int roomId = TryReadInt(row, "RoomId");
                if (roomId <= 0) continue;

                var card = CreateRoomCard(row, roomId);
                _roomCardsHost.Controls.Add(card);
            }

            _roomCardsHost.ResumeLayout();
        }

        private Panel CreateRoomCard(DataRow row, int roomId)
        {
            var card = new Panel
            {
                Width = 300,
                Height = 180,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10, 10, 10, 10),
                Cursor = Cursors.Hand,
                Tag = roomId
            };

            card.Paint += (s, e) =>
            {
                // Vẽ border chính - xanh dương
                using (var pen = new Pen(Color.FromArgb(70, 160, 230), 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
                // Vẽ shadow - gradient từ góc
                using (var shadowPen = new Pen(Color.FromArgb(200, 220, 245), 1))
                {
                    e.Graphics.DrawRectangle(shadowPen, 1, 1, card.Width - 3, card.Height - 3);
                }
            };

            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(245, 250, 255);
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.White;
            };

            string roomNumber = SafeReadString(row, "RoomNumber") ?? "—";
            string typeName = SafeReadString(row, "RoomTypeName") ?? "—";
            string statusName = SafeReadString(row, "StatusName") ?? "—";
            decimal price = TryReadDecimal(row, "RoomPrice") ?? 0m;
            int occupants = TryReadInt(row, "Occupants");

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(14, 12, 14, 12)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Header: Phòng + Giá
            var lblRoom = new Label
            {
                Text = $"Phòng {roomNumber}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Top,
                Height = 28,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 0, 4)
            };

            var lblPrice = new Label
            {
                Text = $"{price:N0}đ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false,
                TextAlign = ContentAlignment.TopRight,
                Margin = new Padding(0, 0, 0, 6)
            };

            // Info: Loại, Trạng thái, Số người
            var infoPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };
            var infoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0)
            };

            var lblTypeInfo = new Label
            {
                Text = $"Loại: {typeName}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                Dock = DockStyle.Top,
                Height = 20,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 0, 0, 2)
            };

            var lblStatusInfo = new Label
            {
                Text = $"Trạng thái: {statusName}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                Dock = DockStyle.Top,
                Height = 20,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 0, 0, 2)
            };

            var lblOccupantsInfo = new Label
            {
                Text = $"Số người: {occupants}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                Dock = DockStyle.Top,
                Height = 20,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };

            infoLayout.Controls.Add(lblTypeInfo, 0, 0);
            infoLayout.Controls.Add(lblStatusInfo, 0, 1);
            infoLayout.Controls.Add(lblOccupantsInfo, 0, 2);
            infoPanel.Controls.Add(infoLayout);

            mainLayout.Controls.Add(lblRoom, 0, 0);
            mainLayout.Controls.Add(lblPrice, 0, 1);
            mainLayout.Controls.Add(infoPanel, 0, 2);

            card.Controls.Add(mainLayout);

            card.Click += (s, e) => ShowRoomDetailsForm(row, roomId);
            lblRoom.Click += (s, e) => ShowRoomDetailsForm(row, roomId);
            lblPrice.Click += (s, e) => ShowRoomDetailsForm(row, roomId);
            lblTypeInfo.Click += (s, e) => ShowRoomDetailsForm(row, roomId);
            lblStatusInfo.Click += (s, e) => ShowRoomDetailsForm(row, roomId);
            lblOccupantsInfo.Click += (s, e) => ShowRoomDetailsForm(row, roomId);

            return card;
        }

        private void ShowRoomDetailsForm(DataRow row, int roomId)
        {
            _selectedRoomId = roomId;

            try
            {
                var frmType = Type.GetType("quan_ly_chuoi_nha_tro.GUI.FrmRoomDetailForm");
                if (frmType != null)
                {
                    // Pass null for tenant history since it's not populated
                    var frm = (Form)Activator.CreateInstance(frmType, _adminBll, row, _contractsBranch, null);
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        _ = LoadAllAsync();
                    }
                    frm.Dispose();
                }
                else
                {
                    MessageBox.Show("Form chi tiết phòng chưa được tải. Vui lòng rebuild project.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string SafeReadString(DataRow row, string col)
        {
            if (row == null || !row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            if (v == DBNull.Value || v == null) return null;
            return v.ToString();
        }

        private decimal? TryReadDecimal(DataRow row, string col)
        {
            if (row == null || !row.Table.Columns.Contains(col)) return null;
            return decimal.TryParse(row[col]?.ToString(), out var val) ? (decimal?)val : null;
        }

        private async Task SaveTenantAsync()
        {
            if (_selectedTenantId <= 0 || _selectedTenantRow == null)
            {
                MessageBox.Show("Vui lòng chọn một khách thuê trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                await _adminBll.UpdateTenantAsync(
                    _selectedTenantId,
                    _txtTenantFullName.Text.Trim(),
                    _txtTenantIdentity.Text.Trim(),
                    _txtTenantPhone.Text.Trim(),
                    _txtTenantEmail.Text.Trim(),
                    _dtTenantBirth.Checked ? _dtTenantBirth.Value : (DateTime?)null,
                    _txtTenantAddress.Text.Trim(),
                    _txtTenantTempReg.Text.Trim(),
                    _dtTenantTempFrom.Checked ? _dtTenantTempFrom.Value : (DateTime?)null,
                    _dtTenantTempTo.Checked ? _dtTenantTempTo.Value : (DateTime?)null,
                    _chkTenantActive.Checked,
                    _txtTenantFrontId.Text.Trim(),
                    _txtTenantBackId.Text.Trim()
                );

                await LoadAllAsync();
                var refreshed = FindById(_tenantsBranch, "TenantId", _selectedTenantId);
                if (refreshed != null) ShowTenantDetails(refreshed, _selectedTenantCard);
                MessageBox.Show("Đã lưu thông tin khách thuê.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu khách thuê.\n\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
