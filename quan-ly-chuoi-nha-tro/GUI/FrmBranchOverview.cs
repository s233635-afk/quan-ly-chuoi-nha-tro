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
        private const int RoomDetailsCollapsedHeight = 190;
        private const int RoomDetailsExpandedHeight = 270;

        private readonly int _branchId;
        private readonly BranchBLL _branchBll = new BranchBLL();
        private readonly AdminDataBLL _adminBll = new AdminDataBLL();

        private FrmRoomTenantQuickView _roomQuickView;

        private Panel _header;
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
        private DataGridView _gridOverviewRooms;
        private Label _lblOverviewRooms;
        private Label _lblOverviewStaff;
        private FlowLayoutPanel _overviewStaffPreview;

        private DataGridView _gridRooms;
        private Panel _roomDetails;
        private Label _lblRoomTitle;
        private Label _lblRoomInfo;
        private Panel _tenantDetails;
        private Label _lblTenantTitle;
        private Label _lblTenantInfo;
        private int _selectedRoomId;
        private bool _tenantVisible;

        private DataGridView _gridTenants;
        private DataGridView _gridStaff;
        private DataGridView _gridContracts;
        private DataGridView _gridDeposits;
        private DataGridView _gridInvoices;
        private DataGridView _gridPayments;
        private DataGridView _gridUtilities;
        private DataGridView _gridMaintenance;
        private DataGridView _gridAssets;

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
            _tabOverview = new TabPage("Tổng quan");
            _tabRooms = new TabPage("Phòng");
            _tabSections = new TabPage("Khu/Dãy");
            _tabTenants = new TabPage("Khách thuê");
            _tabStaff = new TabPage("Nhân viên");
            _tabContracts = new TabPage("Hợp đồng");
            _tabDeposits = new TabPage("Đặt cọc");
            _tabInvoices = new TabPage("Hóa đơn");
            _tabPayments = new TabPage("Thanh toán");
            _tabUtilities = new TabPage("Điện/Nước/DV");
            _tabMaintenance = new TabPage("Bảo trì");
            _tabAssets = new TabPage("Tài sản");

            _tabs.TabPages.AddRange(new[] { _tabOverview, _tabRooms, _tabSections, _tabTenants, _tabStaff, _tabContracts, _tabDeposits, _tabInvoices, _tabPayments, _tabUtilities, _tabMaintenance, _tabAssets });
            foreach (TabPage p in _tabs.TabPages) p.BackColor = Color.White;

            _overviewInfo = new Panel { Dock = DockStyle.Top, Height = 320, BackColor = Color.White, Padding = new Padding(14) };
            _lblOverviewRooms = new Label { AutoSize = true, Text = "Phòng: 0", ForeColor = Color.FromArgb(90, 90, 90), Location = new Point(14, 142) };
            _lblOverviewStaff = new Label { AutoSize = true, Text = "Nhân viên: 0", ForeColor = Color.FromArgb(90, 90, 90), Location = new Point(160, 142) };
            _overviewInfo.Controls.Add(_lblOverviewRooms);
            _overviewInfo.Controls.Add(_lblOverviewStaff);

            _gridOverviewRooms = CreateGrid();
            _gridOverviewRooms.Dock = DockStyle.Fill;
            _gridOverviewRooms.CellClick += (s, e) => JumpToRoomFromOverview();
            _gridOverviewRooms.CellFormatting += GridRoomCellFormatting;
            _tabOverview.Controls.Add(_gridOverviewRooms);
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

            _gridRooms = CreateGrid();
            _gridRooms.Dock = DockStyle.Fill;
            _gridRooms.CellClick += (s, e) => HandleRoomClickFromGrid(_gridRooms);
            _gridRooms.CellDoubleClick += (s, e) => HandleRoomClickFromGrid(_gridRooms);
            _gridRooms.CellFormatting += GridRoomCellFormatting;
            _tabRooms.Controls.Add(_gridRooms);
            _tabRooms.Controls.Add(_roomDetails);

            BuildSectionsTab();
            _gridTenants = CreateGrid(); _gridTenants.Dock = DockStyle.Fill; _tabTenants.Controls.Add(_gridTenants);
            _gridStaff = CreateGrid(); _gridStaff.Dock = DockStyle.Fill; _gridStaff.CellFormatting += GridActiveCellFormatting; _tabStaff.Controls.Add(_gridStaff);
            _gridContracts = CreateGrid(); _gridContracts.Dock = DockStyle.Fill; _tabContracts.Controls.Add(_gridContracts);
            _gridDeposits = CreateGrid(); _gridDeposits.Dock = DockStyle.Fill; _tabDeposits.Controls.Add(_gridDeposits);
            _gridInvoices = CreateGrid(); _gridInvoices.Dock = DockStyle.Fill; _tabInvoices.Controls.Add(_gridInvoices);
            _gridPayments = CreateGrid(); _gridPayments.Dock = DockStyle.Fill; _tabPayments.Controls.Add(_gridPayments);
            _gridUtilities = CreateGrid(); _gridUtilities.Dock = DockStyle.Fill; _tabUtilities.Controls.Add(_gridUtilities);
            _gridMaintenance = CreateGrid(); _gridMaintenance.Dock = DockStyle.Fill; _tabMaintenance.Controls.Add(_gridMaintenance);
            _gridAssets = CreateGrid(); _gridAssets.Dock = DockStyle.Fill; _tabAssets.Controls.Add(_gridAssets);

            Controls.Add(_tabs);
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
                BindRooms(_gridRooms, _roomsBranch);
                BindRooms(_gridOverviewRooms, _roomsBranch);
                _lblOverviewRooms.Text = $"Phòng: {_roomsBranch.Rows.Count:N0}";

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
                BindTenants(_gridTenants, tenantsRelated);

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

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var title = new Label
            {
                AutoSize = true,
                Text = "Thông tin chi nhánh",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Margin = new Padding(0, 0, 0, 6)
            };

            var info = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                Margin = new Padding(0, 0, 0, 0)
            };

            string line1 = $"Mã: {NullDash(code)}   |   Tên: {NullDash(name)}";
            string line2 = $"Địa chỉ: {NullDash(address)}";
            string line3 = $"Điện thoại: {NullDash(phone)}   |   Hotline: {NullDash(hotline)}   |   Giờ hoạt động: {NullDash(hours)}";
            string line4 = $"Trạng thái: {NullDash(statusText)}";
            string line5 = string.IsNullOrWhiteSpace(desc) ? null : $"Mô tả: {desc}";
            info.Text = string.Join("\n", new[] { line1, line2, line3, line4, line5 }.Where(x => !string.IsNullOrWhiteSpace(x)));

            var staffBox = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 249, 255),
                Padding = new Padding(10),
                Margin = new Padding(0, 10, 0, 0)
            };
            staffBox.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 230, 240)))
                {
                    var rect = new Rectangle(0, 0, staffBox.Width - 1, staffBox.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            var staffTitle = new Label
            {
                AutoSize = true,
                Text = "Nhân viên tại chi nhánh",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 6)
            };

            _overviewStaffPreview = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            staffBox.Controls.Add(_overviewStaffPreview);
            staffBox.Controls.Add(staffTitle);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 6, 0, 0)
            };
            _lblOverviewRooms.Location = new Point(0, 0);
            _lblOverviewRooms.Margin = new Padding(0, 0, 16, 0);
            _lblOverviewStaff.Location = new Point(0, 0);
            _lblOverviewStaff.Margin = new Padding(0, 0, 0, 0);
            footer.Controls.Add(_lblOverviewRooms);
            footer.Controls.Add(_lblOverviewStaff);

            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(info, 0, 1);
            layout.Controls.Add(staffBox, 0, 2);
            layout.Controls.Add(footer, 0, 3);

            _overviewInfo.Controls.Add(layout);
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
                    ForeColor = Color.FromArgb(90, 90, 90)
                });
                _overviewStaffPreview.ResumeLayout();
                return;
            }

            int shown = 0;
            foreach (DataRow r in staff.Rows)
            {
                if (shown >= 8) break;
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
                shown++;
            }

            if (staff.Rows.Count > shown)
            {
                _overviewStaffPreview.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = $"+{staff.Rows.Count - shown} nhân viên khác",
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Margin = new Padding(6, 7, 0, 0)
                });
            }

            _overviewStaffPreview.ResumeLayout();
        }

        private static Control CreateStaffCard(string fullName, string userName, string phone, string email, string roleName, bool isActive, Func<Task> onEdit)
        {
            var panel = new Panel
            {
                Width = 360,
                Height = 92,
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 10),
                Margin = new Padding(0, 0, 12, 12),
                Cursor = Cursors.Hand
            };
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 230, 240)))
                {
                    var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            var status = new Label
            {
                AutoSize = true,
                Text = isActive ? "Hoạt động" : "Vô hiệu",
                ForeColor = isActive ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69),
                BackColor = isActive ? Color.FromArgb(232, 247, 239) : Color.FromArgb(252, 236, 238),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(8, 4, 8, 4),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var title = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 22,
                Text = NullDash(fullName),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159)
            };

            var line1 = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 20,
                Text = $"Tài khoản: {NullDash(userName)}   |   SĐT: {NullDash(phone)}",
                Font = new Font("Segoe UI", 9.2f, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            var line2 = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 20,
                Text = $"Email: {NullDash(email)}",
                Font = new Font("Segoe UI", 9.2f, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            var line3 = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 20,
                Text = $"Vai trò: {NullDash(roleName)}",
                Font = new Font("Segoe UI", 9.2f, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            panel.Controls.Add(line3);
            panel.Controls.Add(line2);
            panel.Controls.Add(line1);
            panel.Controls.Add(title);
            panel.Controls.Add(status);

            panel.Resize += (s, e) =>
            {
                status.Location = new Point(panel.ClientSize.Width - status.Width - 10, 10);
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

        private static void BindTenants(DataGridView grid, DataTable dt)
        {
            if (grid == null) return;
            grid.DataSource = dt;
            SetHeader(grid, "TenantId", "ID");
            SetHeader(grid, "FullName", "Họ tên");
            SetHeader(grid, "IdentityCard", "CCCD");
            SetHeader(grid, "PhoneNumber", "SĐT");
            SetHeader(grid, "TemporaryRegistration", "Tạm trú");
            HideIfExists(grid, "FrontIdPhoto");
            HideIfExists(grid, "BackIdPhoto");
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
            int total = staff?.Rows.Count ?? 0;
            int active = 0;
            if (staff != null && staff.Columns.Contains("IsActive"))
            {
                foreach (DataRow r in staff.Rows)
                {
                    try { if (Convert.ToBoolean(r["IsActive"])) active++; } catch { }
                }
            }
            _lblOverviewStaff.Text = $"Nhân viên: {active}/{total}";
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
                RenderTenantInfo(roomId);

            ShowRoomQuickView(roomId);
        }

        private void JumpToRoomFromOverview()
        {
            if (_gridOverviewRooms == null || _gridOverviewRooms.CurrentRow == null || _gridOverviewRooms.CurrentRow.DataBoundItem == null)
                return;

            DataRow row = null;
            if (_gridOverviewRooms.CurrentRow.DataBoundItem is DataRowView drv)
                row = drv.Row;
            if (row == null) return;

            int roomId = TryReadInt(row, "RoomId");
            if (roomId <= 0) return;

            _tabs.SelectedTab = _tabRooms;
            SelectRoomInRoomsGrid(roomId);
            ShowRoomQuickView(roomId);
        }

        private void ShowRoomQuickView(int roomId)
        {
            var room = FindById(_roomsBranch, "RoomId", roomId);
            if (room == null) return;

            var contract = FindActiveContractForRoom(roomId);
            DataRow tenant = null;
            if (contract != null)
            {
                int tenantId = TryReadInt(contract, "TenantId");
                if (tenantId > 0)
                    tenant = FindById(_tenantsAll, "TenantId", tenantId);
            }

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
                catch { }
                _roomQuickView.Show(this);
            }
            else if (!_roomQuickView.Visible)
            {
                _roomQuickView.Show(this);
            }

            _roomQuickView.UpdateData(roomId, room, tenant, contract);
            _roomQuickView.BringToFront();
        }

        private async Task RefreshRoomQuickViewAsync(int roomId)
        {
            await LoadAllAsync();
            _tabs.SelectedTab = _tabRooms;
            SelectRoomInRoomsGrid(roomId);
            ShowRoomQuickView(roomId);
        }

        private void SelectRoomInRoomsGrid(int roomId)
        {
            if (_gridRooms == null || _gridRooms.Rows == null) return;

            foreach (DataGridViewRow r in _gridRooms.Rows)
            {
                if (r?.DataBoundItem is DataRowView drv)
                {
                    int id = 0;
                    try { id = Convert.ToInt32(drv.Row["RoomId"]); } catch { id = 0; }
                    if (id == roomId)
                    {
                        _gridRooms.ClearSelection();
                        r.Selected = true;
                        _gridRooms.CurrentCell = r.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => c.Visible) ?? r.Cells[0];
                        _selectedRoomId = roomId;
                        _tenantVisible = false;
                        _tenantDetails.Visible = false;
                        RenderRoomInfo(roomId);
                        _gridRooms.FirstDisplayedScrollingRowIndex = Math.Max(0, r.Index - 2);
                        return;
                    }
                }
            }
        }

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
    }
}
