using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Chi tiết 1 Khu/Dãy: thống kê + danh sách phòng dạng card + xem/sửa phòng & người thuê.
    /// </summary>
    public class FrmSectionOverview : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly int _branchId;
        private DataRow _sectionRow;

        private int _sectionId;
        private bool _changed;

        private DataTable _roomsBranch;
        private DataTable _tenantsAll;
        private DataTable _contractsBranch;

        private Panel _header;
        private Label _lblTitle;
        private Label _lblSub;
        private Button _btnEditSection;
        private Button _btnClose;

        private TextBox _txtSearch;
        private ComboBox _cboFilter;
        private Label _lblCount;

        private const int DetailPanelPreferredWidth = 360;
        private const int DetailPanelMinimumWidth = 280;
        private const int RoomsPanelMinimumWidth = 420;

        private bool _splitInitialized;
        private SplitContainer _splitContainer;
        private FlowLayoutPanel _roomsHost;
        private Panel _detail;
        private Label _lblRoomTitle;
        private Label _lblRoomInfo;
        private Label _lblTenantTitle;
        private Label _lblTenantInfo;
        private Button _btnEditRoom;
        private Button _btnEditTenant;

        private int _selectedRoomId;

        public FrmSectionOverview(AdminDataBLL bll, int branchId, DataRow sectionRow, DataTable roomsBranch, DataTable tenantsAll, DataTable contractsBranch)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
            _branchId = branchId;
            _sectionRow = sectionRow;

            _roomsBranch = roomsBranch;
            _tenantsAll = tenantsAll;
            _contractsBranch = contractsBranch;

            _sectionId = TryReadInt(_sectionRow, "SectionId");

            InitializeComponent();
            Load += async (s, e) => await RefreshAsync();
        }

        private void InitializeComponent()
        {
            Text = "Khu/Dãy";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1180;
            Height = 720;
            BackColor = Color.FromArgb(245, 247, 250);

            _header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.White, Padding = new Padding(14, 10, 14, 10) };
            _header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 3, BackColor = UiKit.Primary });

            _lblTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Text = "Khu/Dãy",
                Dock = DockStyle.Top
            };
            _lblSub = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(90, 90, 90),
                Text = "—",
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
            _btnClose.Click += (s, e) => { DialogResult = _changed ? DialogResult.OK : DialogResult.Cancel; Close(); };

            _btnEditSection = new Button
            {
                Text = "Sửa dãy",
                Width = 100,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnEditSection.FlatAppearance.BorderSize = 0;
            _btnEditSection.Click += async (s, e) => await EditSectionAsync();

            var headerLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var headerText = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            headerText.Controls.Add(_lblSub);
            headerText.Controls.Add(_lblTitle);

            var headerButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            _btnEditSection.Margin = new Padding(0, 0, 10, 0);
            _btnClose.Margin = new Padding(0);
            headerButtons.Controls.Add(_btnEditSection);
            headerButtons.Controls.Add(_btnClose);

            headerLayout.Controls.Add(headerText, 0, 0);
            headerLayout.Controls.Add(headerButtons, 1, 0);
            _header.Controls.Add(headerLayout);

            var top = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White, Padding = new Padding(14, 12, 14, 10) };
            top.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(230, 235, 240) });

            _txtSearch = new TextBox { Width = 320 };
            _txtSearch.TextChanged += (s, e) => RebuildRooms();
            _cboFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
            _cboFilter.Items.AddRange(new object[] { "Tất cả", "Đang ở", "Trống", "Đang hoạt động", "Vô hiệu" });
            _cboFilter.SelectedIndex = 0;
            _cboFilter.SelectedIndexChanged += (s, e) => RebuildRooms();
            _lblCount = new Label { AutoSize = true, Text = "Phòng: 0", ForeColor = Color.FromArgb(90, 90, 90), Margin = new Padding(0, 6, 0, 0) };

            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent };
            flow.Controls.Add(new Label { AutoSize = true, Text = "Tìm:", ForeColor = Color.FromArgb(90, 90, 90), Margin = new Padding(0, 6, 6, 0) });
            _txtSearch.Margin = new Padding(0, 2, 10, 0);
            flow.Controls.Add(_txtSearch);
            flow.Controls.Add(new Label { AutoSize = true, Text = "Lọc:", ForeColor = Color.FromArgb(90, 90, 90), Margin = new Padding(18, 6, 6, 0) });
            _cboFilter.Margin = new Padding(0, 2, 10, 0);
            flow.Controls.Add(_cboFilter);
            flow.Controls.Add(_lblCount);
            top.Controls.Add(flow);

            _splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterWidth = 6,
                BackColor = BackColor,
                Panel1MinSize = 0,
                Panel2MinSize = 0,
                FixedPanel = FixedPanel.Panel2
            };
            _splitContainer.Panel1.BackColor = BackColor;
            _splitContainer.Panel2.BackColor = Color.White;
            _splitContainer.Panel2.Padding = new Padding(0, 16, 12, 12);
            _splitContainer.HandleCreated += (s, e) => BeginInvoke(new Action(UpdateSplitterDistance));
            _splitContainer.SizeChanged += (s, e) => UpdateSplitterDistance();

            _roomsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(14),
                BackColor = BackColor
            };
            _splitContainer.Panel1.Controls.Add(_roomsHost);

            _detail = BuildDetailPanel();
            _splitContainer.Panel2.Controls.Add(_detail);

            Controls.Add(_splitContainer);
            Controls.Add(top);
            Controls.Add(_header);
            Layout += (s, e) => EnsureSplitInitialized();
            Resize += (s, e) => ClampSplitterDistance();
            UpdateSplitterDistance();
        }

        private void UpdateSplitterDistance()
        {
            if (_splitContainer == null || _splitContainer.IsDisposed) return;
            int width = _splitContainer.Width;
            if (width <= 0) return;

            int panel1Min = Math.Min(RoomsPanelMinimumWidth, Math.Max(0, width - DetailPanelMinimumWidth));
            int panel2Min = Math.Min(DetailPanelMinimumWidth, Math.Max(0, width - RoomsPanelMinimumWidth));

            if (panel1Min + panel2Min > width)
            {
                panel1Min = Math.Max(0, width / 2);
                panel2Min = width - panel1Min;
            }

            SafeSetMinSizes(panel1Min, panel2Min);

            int preferred = (int)Math.Round(width * 0.60);
            preferred = Math.Max(panel1Min, preferred);
            ClampSplitterDistance(preferred);
        }

        private void EnsureSplitInitialized()
        {
            if (_splitInitialized) return;
            if (_splitContainer == null || _splitContainer.IsDisposed) return;
            if (_splitContainer.Width <= 0) return;

            UpdateSplitterDistance();
            _splitInitialized = true;
        }

        private void ClampSplitterDistance(int? desiredOverride = null)
        {
            if (_splitContainer == null || _splitContainer.IsDisposed) return;
            if (_splitContainer.Width <= 0) return;
            int desired = desiredOverride ?? _splitContainer.SplitterDistance;
            SafeSetSplitterDistance(desired);
        }

        private void SafeSetMinSizes(int panel1Min, int panel2Min)
        {
            if (_splitContainer == null || _splitContainer.IsDisposed) return;
            try
            {
                _splitContainer.Panel1MinSize = Math.Max(0, panel1Min);
                _splitContainer.Panel2MinSize = Math.Max(0, panel2Min);
            }
            catch
            {
                try
                {
                    _splitContainer.Panel1MinSize = 0;
                    _splitContainer.Panel2MinSize = 0;
                }
                catch
                {
                    // ignore
                }
            }
        }

        private void SafeSetSplitterDistance(int desired)
        {
            if (_splitContainer == null || _splitContainer.IsDisposed) return;

            int min = _splitContainer.Panel1MinSize;
            int max = _splitContainer.Width - _splitContainer.Panel2MinSize;
            if (max < min)
            {
                _splitContainer.Panel1MinSize = 0;
                _splitContainer.Panel2MinSize = 0;
                min = 0;
                max = Math.Max(0, _splitContainer.Width - 1);
            }

            int clamped = Math.Max(min, Math.Min(max, desired));
            try
            {
                if (_splitContainer.SplitterDistance != clamped)
                    _splitContainer.SplitterDistance = clamped;
            }
            catch
            {
                // ignore to avoid crashing
            }
        }

        private Panel BuildDetailPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14), BackColor = Color.White };

            var card = new Panel { Dock = DockStyle.Top, Height = 300, BackColor = Color.White, Padding = new Padding(14) };
            card.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(230, 235, 240) });

            _lblRoomTitle = new Label { AutoSize = true, Text = "Chọn 1 phòng", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0, 79, 159) };
            _lblRoomInfo = new Label { AutoSize = false, Dock = DockStyle.Top, Height = 78, Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(80, 80, 80), Margin = new Padding(0, 8, 0, 0) };

            var tenantBox = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.FromArgb(245, 249, 255), Padding = new Padding(10), Margin = new Padding(0, 10, 0, 0) };
            tenantBox.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 230, 240)))
                {
                    var rect = new Rectangle(0, 0, tenantBox.Width - 1, tenantBox.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };
            _lblTenantTitle = new Label { AutoSize = true, Text = "Người đang sử dụng", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(0, 79, 159), Dock = DockStyle.Top };
            _lblTenantInfo = new Label { Dock = DockStyle.Fill, AutoSize = false, Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(70, 70, 70) };
            tenantBox.Controls.Add(_lblTenantInfo);
            tenantBox.Controls.Add(_lblTenantTitle);

            _btnEditRoom = new Button { Text = "Sửa phòng", Width = 96, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            _btnEditRoom.FlatAppearance.BorderSize = 0;
            _btnEditRoom.Enabled = false;
            _btnEditRoom.Click += async (s, e) => await EditSelectedRoomAsync();

            _btnEditTenant = new Button { Text = "Sửa người thuê", Width = 120, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 193, 7), ForeColor = Color.Black };
            _btnEditTenant.FlatAppearance.BorderSize = 0;
            _btnEditTenant.Enabled = false;
            _btnEditTenant.Click += async (s, e) => await EditSelectedTenantAsync();

            var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent, Padding = new Padding(0, 10, 0, 0) };
            _btnEditRoom.Margin = new Padding(0, 0, 10, 0);
            actions.Controls.Add(_btnEditRoom);
            actions.Controls.Add(_btnEditTenant);

            card.Controls.Add(actions);
            card.Controls.Add(tenantBox);
            card.Controls.Add(_lblRoomInfo);
            card.Controls.Add(_lblRoomTitle);

            var filler = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 247, 250) };

            panel.Controls.Add(filler);
            panel.Controls.Add(card);
            return panel;
        }

        private async Task RefreshAsync()
        {
            try
            {
                UseWaitCursor = true;
                Enabled = false;

                if (_sectionId <= 0)
                {
                    _lblTitle.Text = "Khu/Dãy";
                    _lblSub.Text = "Không xác định SectionId.";
                    return;
                }

                var sectionsTask = _bll.GetBranchSectionsAsync(_branchId);
                var roomsTask = _bll.GetRoomsAsync();
                var tenantsTask = _bll.GetTenantsAsync();
                var contractsTask = _bll.GetContractsAsync();

                await Task.WhenAll(sectionsTask, roomsTask, tenantsTask, contractsTask);

                var sections = sectionsTask.Result ?? new DataTable();
                TextFixer.ForceFixDataTable(sections, "SectionCode", "SectionName", "Description");
                _sectionRow = FindById(sections, "SectionId", _sectionId) ?? _sectionRow;

                _roomsBranch = FilterByBranchId(roomsTask.Result ?? new DataTable(), _branchId);
                _tenantsAll = tenantsTask.Result ?? new DataTable();
                _contractsBranch = FilterByBranchId(contractsTask.Result ?? new DataTable(), _branchId);

                TextFixer.ForceFixDataTable(_roomsBranch, "RoomNumber", "SectionName", "RoomTypeName", "StatusName");
                TextFixer.ForceFixDataTable(_tenantsAll, "FullName", "Address", "TemporaryRegistration");

                ApplyHeader();
                RebuildRooms();

                if (_selectedRoomId > 0)
                    SelectRoom(_selectedRoomId);
                else
                    ClearDetail();
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi tải khu/dãy: " + ex.Message);
            }
            finally
            {
                Enabled = true;
                UseWaitCursor = false;
            }
        }

        private void ApplyHeader()
        {
            string code = TextFixer.FixUtf8Mojibake(ReadString(_sectionRow, "SectionCode"));
            string name = TextFixer.FixUtf8Mojibake(ReadString(_sectionRow, "SectionName"));
            string desc = TextFixer.FixUtf8Mojibake(ReadString(_sectionRow, "Description"));
            bool isActive = ReadBool(_sectionRow, "IsActive", true);

            _lblTitle.Text = string.IsNullOrWhiteSpace(code) ? (name ?? "Khu/Dãy") : $"{code} - {name}";
            _lblSub.Text = $"SectionId: {_sectionId}   |   Trạng thái: {(isActive ? "Hoạt động" : "Vô hiệu")}{(string.IsNullOrWhiteSpace(desc) ? "" : $"   |   {desc}")}";
        }

        private void RebuildRooms()
        {
            if (_roomsHost == null) return;

            _roomsHost.SuspendLayout();
            _roomsHost.Controls.Clear();

            var rooms = FilterBySectionId(_roomsBranch, _sectionId);
            string q = (_txtSearch?.Text ?? string.Empty).Trim().ToLowerInvariant();
            int filter = _cboFilter?.SelectedIndex ?? 0;

            var list = rooms.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                list = list.Where(r =>
                {
                    string number = ReadString(r, "RoomNumber") ?? string.Empty;
                    string type = ReadString(r, "RoomTypeName") ?? string.Empty;
                    string st = ReadString(r, "StatusName") ?? string.Empty;
                    return number.ToLowerInvariant().Contains(q)
                           || type.ToLowerInvariant().Contains(q)
                           || st.ToLowerInvariant().Contains(q);
                });
            }

            if (filter != 0)
            {
                list = list.Where(r =>
                {
                    string st = (ReadString(r, "StatusName") ?? string.Empty).ToLowerInvariant();
                    bool isActive = ReadBool(r, "IsActive", true);
                    if (filter == 1) return st.Contains("đang") || st.Contains("ở");
                    if (filter == 2) return st.Contains("trống");
                    if (filter == 3) return isActive;
                    if (filter == 4) return !isActive;
                    return true;
                });
            }

            var data = list.ToList();
            _lblCount.Text = $"Phòng: {data.Count:N0}";

            foreach (var r in data)
            {
                _roomsHost.Controls.Add(CreateRoomCard(r));
            }

            if (data.Count == 0)
            {
                _roomsHost.Controls.Add(new Label { AutoSize = true, Text = "Chưa có phòng trong dãy/khu này.", ForeColor = Color.FromArgb(90, 90, 90) });
            }

            _roomsHost.ResumeLayout();
        }

        private Control CreateRoomCard(DataRow roomRow)
        {
            int roomId = TryReadInt(roomRow, "RoomId");
            string number = ReadString(roomRow, "RoomNumber");
            string type = ReadString(roomRow, "RoomTypeName");
            string status = ReadString(roomRow, "StatusName");
            string price = FormatMoney(ReadString(roomRow, "RoomPrice"));
            bool isActive = ReadBool(roomRow, "IsActive", true);

            var card = new Panel
            {
                Width = 300,
                Height = 118,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 14, 14),
                Cursor = Cursors.Hand
            };

            var accent = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = isActive ? UiKit.Primary : Color.FromArgb(180, 180, 180) };
            card.Controls.Add(accent);

            var title = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Padding = new Padding(12, 10, 12, 0),
                Text = $"Phòng {NullDash(number)}"
            };

            var sub = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(80, 80, 80),
                Padding = new Padding(12, 6, 12, 10),
                Text = $"Loại: {NullDash(type)}\nTrạng thái: {NullDash(status)}   |   Giá: {NullDash(price)}"
            };

            var badge = new Label
            {
                AutoSize = true,
                Text = isActive ? "Kích hoạt" : "Vô hiệu",
                ForeColor = isActive ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69),
                BackColor = isActive ? Color.FromArgb(232, 247, 239) : Color.FromArgb(252, 236, 238),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(8, 4, 8, 4)
            };

            var border = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            border.Paint += (s, e) =>
            {
                using (var pen = new Pen(roomId == _selectedRoomId ? Color.FromArgb(0, 122, 204) : Color.FromArgb(230, 235, 240), 1.2f))
                {
                    var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };
            border.Controls.Add(sub);
            border.Controls.Add(title);
            border.Controls.Add(badge);
            border.Resize += (s, e) => { badge.Location = new Point(border.ClientSize.Width - badge.Width - 12, 12); };

            card.Controls.Add(border);

            void SelectThis()
            {
                _selectedRoomId = roomId;
                foreach (Control c in _roomsHost.Controls) c.Invalidate();
                SelectRoom(roomId);
            }

            card.Click += (s, e) => SelectThis();
            title.Click += (s, e) => SelectThis();
            sub.Click += (s, e) => SelectThis();
            badge.Click += (s, e) => SelectThis();
            card.DoubleClick += (s, e) => SelectThis();

            return card;
        }

        private void SelectRoom(int roomId)
        {
            var room = FindById(_roomsBranch, "RoomId", roomId);
            if (room == null)
            {
                ClearDetail();
                return;
            }

            string number = ReadString(room, "RoomNumber");
            string type = ReadString(room, "RoomTypeName");
            string status = ReadString(room, "StatusName");
            string price = FormatMoney(ReadString(room, "RoomPrice"));
            string floor = ReadString(room, "Floor");
            string area = ReadString(room, "Area");
            bool isActive = ReadBool(room, "IsActive", true);

            _lblRoomTitle.Text = $"Phòng {NullDash(number)}";
            _lblRoomInfo.Text =
                $"Loại: {NullDash(type)}   |   Trạng thái: {NullDash(status)}\n" +
                $"Giá: {NullDash(price)}   |   Tầng: {NullDash(floor)}   |   Diện tích: {NullDash(area)}\n" +
                $"Kích hoạt: {(isActive ? "Có" : "Không")}";

            var contract = FindActiveContractForRoom(roomId);
            var tenant = contract != null ? FindById(_tenantsAll, "TenantId", TryReadInt(contract, "TenantId")) : null;

            if (tenant == null || contract == null)
            {
                _lblTenantInfo.Text = "Phòng hiện chưa có người sử dụng (không có hợp đồng Active/Extended).";
                _btnEditTenant.Enabled = false;
            }
            else
            {
                string tenantName = ReadString(tenant, "FullName");
                string tenantPhone = ReadString(tenant, "PhoneNumber");
                string tenantIdCard = ReadString(tenant, "IdentityCard");
                string contractNo = ReadString(contract, "ContractNumber");
                string st = ReadString(contract, "Status");
                string start = FormatDate(ReadString(contract, "StartDate"));
                string end = FormatDate(ReadString(contract, "EndDate"));

                _lblTenantInfo.Text =
                    $"Họ tên: {NullDash(tenantName)}   |   SĐT: {NullDash(tenantPhone)}   |   CCCD: {NullDash(tenantIdCard)}\n" +
                    $"Hợp đồng: {NullDash(contractNo)}   |   {NullDash(st)}   |   {NullDash(start)} → {NullDash(end)}";
                _btnEditTenant.Enabled = true;
            }

            _btnEditRoom.Enabled = true;
        }

        private void ClearDetail()
        {
            _selectedRoomId = 0;
            _lblRoomTitle.Text = "Chọn 1 phòng";
            _lblRoomInfo.Text = string.Empty;
            _lblTenantInfo.Text = string.Empty;
            _btnEditRoom.Enabled = false;
            _btnEditTenant.Enabled = false;
        }

        private async Task EditSectionAsync()
        {
            if (_sectionRow == null) return;
            using (var frm = new FrmBranchSectionEditor(_bll, _sectionRow, _branchId))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _changed = true;
                    await RefreshAsync();
                }
            }
        }

        private async Task EditSelectedRoomAsync()
        {
            if (_selectedRoomId <= 0) return;
            var room = FindById(_roomsBranch, "RoomId", _selectedRoomId);
            if (room == null) return;

            using (var frm = new FrmRoomEditor(_bll, room))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _changed = true;
                    await RefreshAsync();
                    SelectRoom(_selectedRoomId);
                }
            }
        }

        private async Task EditSelectedTenantAsync()
        {
            if (_selectedRoomId <= 0) return;
            var contract = FindActiveContractForRoom(_selectedRoomId);
            if (contract == null) return;
            var tenant = FindById(_tenantsAll, "TenantId", TryReadInt(contract, "TenantId"));
            if (tenant == null) return;

            using (var frm = new FrmTenantEditor(_bll, tenant))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _changed = true;
                    await RefreshAsync();
                    SelectRoom(_selectedRoomId);
                }
            }
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

        private static DataTable FilterByBranchId(DataTable dt, int branchId)
        {
            if (dt == null) return new DataTable();
            if (!dt.Columns.Contains("BranchId")) return dt.Copy();
            var rows = dt.AsEnumerable().Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == branchId);
            var filtered = dt.Clone();
            foreach (var r in rows) filtered.ImportRow(r);
            return filtered;
        }

        private static DataTable FilterBySectionId(DataTable dt, int sectionId)
        {
            if (dt == null) return new DataTable();
            if (!dt.Columns.Contains("SectionId")) return dt.Copy();
            var rows = dt.AsEnumerable().Where(r => int.TryParse(r["SectionId"]?.ToString(), out var sid) && sid == sectionId);
            var filtered = dt.Clone();
            foreach (var r in rows) filtered.ImportRow(r);
            return filtered;
        }

        private static DataRow FindById(DataTable dt, string idCol, int id)
        {
            if (dt == null || !dt.Columns.Contains(idCol)) return null;
            foreach (DataRow r in dt.Rows)
            {
                try { if (Convert.ToInt32(r[idCol]) == id) return r; } catch { }
            }
            return null;
        }

        private static int TryReadInt(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return 0;
            if (int.TryParse(r[col]?.ToString(), out var v)) return v;
            return 0;
        }

        private static string ReadString(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return null;
            var v = r[col];
            return v == null || v == DBNull.Value ? null : TextFixer.ForceFixUtf8Mojibake(v.ToString());
        }

        private static bool ReadBool(DataRow r, string col, bool defaultValue)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return defaultValue;
            try { return Convert.ToBoolean(r[col]); } catch { return defaultValue; }
        }

        private static string NullDash(string s) => string.IsNullOrWhiteSpace(s) ? "—" : s;

        private static string FormatMoney(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (decimal.TryParse(raw, out var money))
                return money.ToString("N0");
            return raw;
        }

        private static string FormatDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (DateTime.TryParse(raw, out var dt))
                return dt.ToString("dd/MM/yyyy");
            return raw;
        }
    }
}

