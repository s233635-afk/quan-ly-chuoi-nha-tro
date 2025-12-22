using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmRoomDetailForm : Form
    {
        private readonly AdminDataBLL _bll;
        private DataRow _roomRow;
        private DataTable _contracts;
        private DataTable _tenantHistory;
        private DataTable _tenants;
        private DataTable _utilities;
        private DataRow _activeTenantRow;
        private DataRow _activeTenantHistory;
        private Panel _tenantInfoHost;
        private Panel _contractsInfoHost;
        private Panel _utilitiesInfoHost;
        private ComboBox _cboUtilityMonth;
        private Label _lblUtilitySummary;
        private Button _btnAddUtility;
        private Button _btnEditTenant;
        private Button _btnEditRoom;
        private Panel _roomInfoHost;
        private Label _lblHeaderRoom;
        private Label _lblHeaderInfo;
        private TabControl _tabControl;

        public FrmRoomDetailForm(AdminDataBLL bll, DataRow roomRow, DataTable contracts, DataTable tenantHistory)
        {
            _bll = bll;
            _roomRow = roomRow;
            _contracts = contracts;
            _tenantHistory = tenantHistory;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
            Activated += async (s, e) => await RefreshOnActivateAsync();
            Load += async (s, e) =>
            {
                await LoadTenantHistoryAsync();
                await LoadTenantsAsync();
                await LoadContractsAsync();
                await LoadUtilitiesAsync();
            };
        }

        private void InitializeComponent()
        {
            Text = $"Chi tiết phòng {ReadString(_roomRow, "RoomNumber")}";
            StartPosition = FormStartPosition.CenterParent;
            Width = 800;
            Height = 600;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);

            var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20) };

            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(0, 120, 215), Padding = new Padding(16) };
            headerPanel.ForeColor = Color.White;

            _lblHeaderRoom = new Label
            {
                Text = $"Phòng {ReadString(_roomRow, "RoomNumber")}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 40,
                AutoSize = false
            };

            _lblHeaderInfo = new Label
            {
                Text = $"Loại: {ReadRoomType()} | Trạng thái: {ReadRoomStatus()} | Giá: {TryGetDecimal(_roomRow, "RoomPrice"):N0}đ/tháng",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false
            };

            headerPanel.Controls.Add(_lblHeaderInfo);
            headerPanel.Controls.Add(_lblHeaderRoom);

            var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };

            _tabControl = new TabControl { Dock = DockStyle.Fill, BackColor = Color.White };
            _tabControl.SelectedIndexChanged += HandleTabChanged;

            // Tab 1: Thông tin phòng
            var tabRoom = new TabPage("Thông tin phòng") { BackColor = Color.White, Padding = new Padding(12) };
            var roomPanel = BuildRoomInfoPanel();
            tabRoom.Controls.Add(roomPanel);
            _tabControl.TabPages.Add(tabRoom);

            // Tab 2: Thông tin khách thuê hiện tại
            var tabTenant = new TabPage("Khách thuê hiện tại") { BackColor = Color.White, Padding = new Padding(12) };
            var tenantPanel = BuildTenantInfoPanel();
            tabTenant.Controls.Add(tenantPanel);
            _tabControl.TabPages.Add(tabTenant);

            // Tab 3: Lịch sử hợp đồng
            var tabContracts = new TabPage("Lịch sử hợp đồng") { BackColor = Color.White, Padding = new Padding(12) };
            var contractsPanel = BuildContractsPanel();
            tabContracts.Controls.Add(contractsPanel);
            _tabControl.TabPages.Add(tabContracts);

            // Tab 4: Điện/Nước/DV
            var tabUtilities = new TabPage("Điện/Nước/DV") { BackColor = Color.White, Padding = new Padding(12) };
            var utilitiesPanel = BuildUtilitiesPanel();
            tabUtilities.Controls.Add(utilitiesPanel);
            _tabControl.TabPages.Add(tabUtilities);

            contentPanel.Controls.Add(_tabControl);

            var footerPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(245, 247, 250), Padding = new Padding(12) };
            var btnClose = new Button
            {
                Text = "Đóng lại",
                Width = 100,
                Height = 36,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(Width - 120, 7)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();
            footerPanel.Controls.Add(btnClose);

            mainPanel.Controls.Add(contentPanel);
            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(footerPanel);

            Controls.Add(mainPanel);
        }

        private Panel BuildRoomInfoPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };

            var actionBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 6)
            };

            _btnEditRoom = new Button
            {
                Text = "Chỉnh sửa",
                Width = 110,
                Height = 32,
                BackColor = Color.FromArgb(111, 66, 193),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnEditRoom.FlatAppearance.BorderSize = 0;
            _btnEditRoom.Click += async (s, e) => await EditRoomAsync();

            actionBar.Controls.Add(_btnEditRoom);

            _roomInfoHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };

            panel.Controls.Add(_roomInfoHost);
            panel.Controls.Add(actionBar);

            RenderRoomInfoPanel();
            return panel;
        }

        private Panel BuildTenantInfoPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };

            var actionBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 6)
            };

            _btnEditTenant = new Button
            {
                Text = "Chỉnh sửa",
                Width = 110,
                Height = 32,
                BackColor = Color.FromArgb(111, 66, 193),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnEditTenant.FlatAppearance.BorderSize = 0;
            _btnEditTenant.Click += (s, e) => EditActiveTenant();

            actionBar.Controls.Add(_btnEditTenant);

            _tenantInfoHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };

            panel.Controls.Add(_tenantInfoHost);
            panel.Controls.Add(actionBar);

            RefreshTenantInfoPanel();
            return panel;
        }

        private Panel BuildContractsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };

            _contractsInfoHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };
            _contractsInfoHost.SizeChanged += (s, e) => RenderContractsPanel(_contracts);

            panel.Controls.Add(_contractsInfoHost);
            RenderContractsPanel(_contracts);
            return panel;
        }

        private Panel BuildUtilitiesPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };

            var topBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, 8)
            };

            _btnAddUtility = new Button
            {
                Text = "Thêm chỉ số",
                Width = 120,
                Height = 32,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 10, 0)
            };
            _btnAddUtility.FlatAppearance.BorderSize = 0;
            _btnAddUtility.Click += async (s, e) => await AddUtilityReadingAsync();

            _cboUtilityMonth = new ComboBox
            {
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboUtilityMonth.SelectedIndexChanged += (s, e) => RenderUtilitiesPanel();

            _lblUtilitySummary = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Margin = new Padding(12, 4, 0, 0)
            };

            topBar.Controls.Add(_btnAddUtility);
            topBar.Controls.Add(new Label { Text = "Tháng:", AutoSize = true, Margin = new Padding(0, 6, 6, 0) });
            topBar.Controls.Add(_cboUtilityMonth);
            topBar.Controls.Add(_lblUtilitySummary);

            _utilitiesInfoHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };
            _utilitiesInfoHost.SizeChanged += (s, e) => RenderUtilitiesPanel();

            panel.Controls.Add(_utilitiesInfoHost);
            panel.Controls.Add(topBar);
            RenderUtilitiesPanel();
            return panel;
        }

        private async System.Threading.Tasks.Task LoadTenantsAsync()
        {
            if (_bll == null) return;
            try
            {
                _tenants = await _bll.GetTenantsAsync();
            }
            catch
            {
                _tenants = null;
            }

            RefreshTenantInfoPanel();
        }

        private async System.Threading.Tasks.Task LoadTenantHistoryAsync()
        {
            if (_bll == null) return;
            try
            {
                _tenantHistory = await _bll.GetTenantHistoryAsync();
            }
            catch
            {
                _tenantHistory = null;
            }

            RefreshTenantInfoPanel();
        }

        private async System.Threading.Tasks.Task LoadContractsAsync()
        {
            if (_bll == null) return;
            try
            {
                _contracts = await _bll.GetContractsAsync();
            }
            catch
            {
                _contracts = null;
            }

            RenderContractsPanel(_contracts);
        }

        private async System.Threading.Tasks.Task LoadUtilitiesAsync()
        {
            if (_bll == null) return;
            try
            {
                _utilities = await _bll.GetUtilitiesAsync();
                TextFixer.ForceFixDataTable(_utilities, "RoomNumber", "UtilityName", "UtilityCode", "Notes");
            }
            catch
            {
                _utilities = null;
            }

            PopulateUtilityMonthFilter();
            RenderUtilitiesPanel();
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadTenantHistoryAsync();
                await LoadTenantsAsync();
                await LoadContractsAsync();
                await LoadUtilitiesAsync();
                await ReloadRoomAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private async void HandleTabChanged(object sender, EventArgs e)
        {
            if (_tabControl?.SelectedTab == null) return;
            try
            {
                var tabName = _tabControl.SelectedTab.Text ?? string.Empty;
                if (tabName == "Khách thuê hiện tại")
                {
                    await LoadTenantHistoryAsync();
                    await LoadTenantsAsync();
                }
                else if (tabName == "Lịch sử hợp đồng")
                {
                    await LoadContractsAsync();
                }
                else if (tabName == "Điện/Nước/DV")
                {
                    await LoadUtilitiesAsync();
                }
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private async System.Threading.Tasks.Task RefreshOnActivateAsync()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadTenantHistoryAsync();
                await LoadTenantsAsync();
                await LoadContractsAsync();
                await LoadUtilitiesAsync();
                await ReloadRoomAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private void RenderRoomInfoPanel()
        {
            if (_roomInfoHost == null) return;
            _roomInfoHost.Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddRow(layout, "Số phòng:", ReadString(_roomRow, "RoomNumber"));
            AddRow(layout, "Loại phòng:", ReadRoomType());
            AddRow(layout, "Trạng thái:", ReadRoomStatus());
            AddRow(layout, "Giá/Tháng:", (TryGetDecimal(_roomRow, "RoomPrice") ?? 0m).ToString("N0") + "đ");
            AddRow(layout, "Diện tích:", (TryGetDecimal(_roomRow, "Area") ?? 0m).ToString("0.##") + " m²");
            AddRow(layout, "Số người:", TryGetInt(_roomRow, "Occupants").ToString());
            AddRow(layout, "Tầng:", TryGetInt(_roomRow, "Floor").ToString());
            AddRow(layout, "Kích hoạt:", TryGetBool(_roomRow, "IsActive") == true ? "Có" : "Không");

            _roomInfoHost.Controls.Add(layout);
        }

        private void RefreshTenantInfoPanel()
        {
            if (_tenantInfoHost == null)
                return;

            _tenantInfoHost.Controls.Clear();
            _activeTenantRow = null;
            _activeTenantHistory = null;

            if (_tenantHistory == null || _tenantHistory.Rows.Count == 0)
            {
                _tenantInfoHost.Controls.Add(BuildEmptyTenantLabel());
                _btnEditTenant.Enabled = false;
                return;
            }

            int roomId = TryGetInt(_roomRow, "RoomId");
            _activeTenantHistory = _tenantHistory.AsEnumerable()
                .FirstOrDefault(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == roomId &&
                                     string.IsNullOrWhiteSpace(r["CheckOutDate"]?.ToString()));

            if (_activeTenantHistory == null)
            {
                _tenantInfoHost.Controls.Add(BuildEmptyTenantLabel());
                _btnEditTenant.Enabled = false;
                return;
            }

            int tenantId = TryGetInt(_activeTenantHistory, "TenantId");
            if (_tenants != null && _tenants.Columns.Contains("TenantId"))
            {
                _activeTenantRow = _tenants.AsEnumerable()
                    .FirstOrDefault(t => int.TryParse(t["TenantId"]?.ToString(), out var tid) && tid == tenantId);
            }

            _btnEditTenant.Enabled = _activeTenantRow != null;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddRow(layout, "Tên khách:", ReadTenantValue("FullName", "TenantName"));
            AddRow(layout, "SĐT:", ReadTenantValue("PhoneNumber", "PhoneNumber"));
            AddRow(layout, "Email:", ReadTenantValue("Email", "Email"));
            AddRow(layout, "CCCD:", ReadTenantValue("IdentityCard", "IdentityCard"));
            AddRow(layout, "Ngày sinh:", FormatDate(ReadTenantValue("BirthDate", "DateOfBirth")));
            AddRow(layout, "Địa chỉ:", ReadTenantValue("Address", "Address"));
            AddRow(layout, "Tạm trú tại:", ReadTenantValue("TemporaryRegistration", "TemporaryRegistration"));
            AddRow(layout, "Tạm trú từ:", FormatDate(ReadTenantValue("TemporaryRegistrationDate", "TemporaryRegFrom")));
            AddRow(layout, "Tạm trú đến:", FormatDate(ReadTenantValue("TemporaryRegistrationExpiry", "TemporaryRegTo")));
            AddRow(layout, "Ảnh CCCD (trước):", ReadTenantValue("FrontIdPhoto", "FrontIdImage"));
            AddRow(layout, "Ảnh CCCD (sau):", ReadTenantValue("BackIdPhoto", "BackIdImage"));
            AddRow(layout, "Ngày vào:", FormatDate(ReadString(_activeTenantHistory, "CheckInDate")));

            _tenantInfoHost.Controls.Add(layout);
        }

        private Label BuildEmptyTenantLabel()
        {
            return new Label
            {
                Text = "Không có khách thuê hiện tại",
                ForeColor = Color.FromArgb(90, 90, 90),
                Dock = DockStyle.Top,
                Height = 30
            };
        }

        private string ReadTenantValue(string tenantColumn, string historyColumn)
        {
            var fromTenant = ReadString(_activeTenantRow, tenantColumn);
            if (!string.IsNullOrWhiteSpace(fromTenant)) return fromTenant;
            return ReadString(_activeTenantHistory, historyColumn);
        }

        private void EditActiveTenant()
        {
            if (_activeTenantRow == null) return;
            using (var frm = new FrmTenantEditor(_bll, _activeTenantRow))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadTenantsAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task EditRoomAsync()
        {
            if (_roomRow == null) return;
            using (var frm = new FrmRoomEditor(_bll, _roomRow))
            {
                if (frm.ShowDialog(this) != DialogResult.OK) return;
                await ReloadRoomAsync();
                AdminEvents.NotifyDataChanged();
            }
        }

        private void RenderContractsPanel(DataTable source)
        {
            if (_contractsInfoHost == null) return;
            _contractsInfoHost.Controls.Clear();

            var list = new System.Collections.Generic.List<DataRow>();
            if (source != null && source.Rows.Count > 0)
            {
                int roomId = TryGetInt(_roomRow, "RoomId");
                list = source.AsEnumerable()
                    .Where(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == roomId)
                    .OrderByDescending(r => TryGetDate(r, "StartDate") ?? DateTime.MinValue)
                    .ToList();
            }

            var container = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 0)
            };

            if (list.Count == 0)
            {
                container.Controls.Add(new Label
                {
                    Text = "Chưa có hợp đồng.",
                    AutoSize = true,
                    ForeColor = Color.FromArgb(100, 100, 100),
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    Margin = new Padding(0, 4, 0, 4)
                });
                _contractsInfoHost.Controls.Add(container);
                return;
            }

            foreach (var contract in list)
            {
                var card = new Panel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(10),
                    BackColor = Color.FromArgb(246, 250, 255),
                    Margin = new Padding(0, 0, 0, 8),
                    BorderStyle = BorderStyle.FixedSingle
                };
                int minWidth = Math.Max(520, _contractsInfoHost?.ClientSize.Width - 30 ?? 520);
                card.MinimumSize = new Size(minWidth, 0);

                var header = new TableLayoutPanel
                {
                    AutoSize = true,
                    ColumnCount = 2,
                    Dock = DockStyle.Top,
                    Margin = new Padding(0, 0, 0, 6)
                };
                header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

                string contractNumber = ReadString(contract, "ContractNumber");
                string contractId = ReadString(contract, "ContractId");
                string status = ReadString(contract, "Status");
                var title = !string.IsNullOrWhiteSpace(contractNumber)
                    ? $"Hợp đồng {contractNumber}"
                    : $"Hợp đồng #{contractId}";
                if (!string.IsNullOrWhiteSpace(status))
                    title = $"{title} ({status})";

                var lblTitle = new Label
                {
                    Text = title,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 55, 90),
                    Margin = new Padding(0, 0, 6, 0)
                };

                var actionHost = new FlowLayoutPanel
                {
                    AutoSize = true,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    Margin = new Padding(0)
                };

                var btnEdit = new Button
                {
                    Text = "Chỉnh sửa",
                    Width = 100,
                    Height = 28,
                    BackColor = Color.FromArgb(111, 66, 193),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(0, 0, 6, 0)
                };
                btnEdit.FlatAppearance.BorderSize = 0;
                btnEdit.Click += async (s, e) => await EditContractAsync(contract);

                var btnDelete = new Button
                {
                    Text = "Xóa",
                    Width = 70,
                    Height = 28,
                    BackColor = Color.FromArgb(220, 53, 69),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnDelete.FlatAppearance.BorderSize = 0;
                btnDelete.Click += async (s, e) => await DeleteContractAsync(contract);

                actionHost.Controls.Add(btnEdit);
                actionHost.Controls.Add(btnDelete);

                header.Controls.Add(lblTitle, 0, 0);
                header.Controls.Add(actionHost, 1, 0);

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    ColumnCount = 2,
                    RowCount = 10,
                    Padding = new Padding(0)
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                AddRow(layout, "Số HĐ:", contractNumber);
                AddRow(layout, "Khách thuê:", ReadString(contract, "TenantName"));
                AddRow(layout, "Ngày ký:", FormatDate(ReadString(contract, "SignDate")));
                AddRow(layout, "Ngày bắt đầu:", FormatDate(ReadString(contract, "StartDate")));
                AddRow(layout, "Ngày kết thúc:", FormatDate(ReadString(contract, "EndDate")));
                AddRow(layout, "Giá thuê:", FormatMoney(contract, "RentalPrice"));
                AddRow(layout, "Cọc yêu cầu:", FormatMoney(contract, "DepositRequired"));
                AddRow(layout, "Điều khoản:", ReadString(contract, "Terms"));
                AddRow(layout, "Tạo lúc:", FormatDate(ReadString(contract, "CreatedDate")));
                AddRow(layout, "Cập nhật:", FormatDate(ReadString(contract, "UpdatedDate")));

                card.Controls.Add(layout);
                card.Controls.Add(header);
                container.Controls.Add(card);
            }

            _contractsInfoHost.Controls.Add(container);
        }

        private void RenderUtilitiesPanel()
        {
            if (_utilitiesInfoHost == null) return;
            _utilitiesInfoHost.Controls.Clear();

            var list = new System.Collections.Generic.List<DataRow>();
            if (_utilities != null && _utilities.Rows.Count > 0)
            {
                int roomId = TryGetInt(_roomRow, "RoomId");
                list = _utilities.AsEnumerable()
                    .Where(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == roomId)
                    .OrderByDescending(r => TryGetDate(r, "ReadingDate") ?? DateTime.MinValue)
                    .ToList();
            }

            var container = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 0)
            };

            if (list.Count == 0)
            {
                if (_lblUtilitySummary != null)
                    _lblUtilitySummary.Text = "Tổng: 0";
                container.Controls.Add(new Label
                {
                    Text = "Chưa có dữ liệu điện/nước/dịch vụ.",
                    AutoSize = true,
                    ForeColor = Color.FromArgb(100, 100, 100),
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    Margin = new Padding(0, 4, 0, 4)
                });
                _utilitiesInfoHost.Controls.Add(container);
                return;
            }

            string selectedKey = GetSelectedUtilityMonthKey();
            if (!string.IsNullOrWhiteSpace(selectedKey) && selectedKey != "all")
            {
                list = list
                    .Where(r => (TryGetDate(r, "ReadingDate") ?? DateTime.MinValue).ToString("yyyy-MM") == selectedKey)
                    .ToList();
            }

            if (_lblUtilitySummary != null)
            {
                decimal totalCost = list.Sum(r => TryGetDecimal(r, "TotalCost") ?? 0m);
                _lblUtilitySummary.Text = $"Tổng: {list.Count} | Thành tiền: {totalCost:N0}đ";
            }

            if (list.Count == 0)
            {
                container.Controls.Add(new Label
                {
                    Text = "Không có dữ liệu cho tháng đã chọn.",
                    AutoSize = true,
                    ForeColor = Color.FromArgb(100, 100, 100),
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    Margin = new Padding(0, 4, 0, 4)
                });
                _utilitiesInfoHost.Controls.Add(container);
                return;
            }

            var grouped = list
                .GroupBy(r => (TryGetDate(r, "ReadingDate") ?? DateTime.MinValue).ToString("yyyy-MM"))
                .OrderByDescending(g => g.Key)
                .ToList();

            foreach (var monthGroup in grouped)
            {
                decimal monthTotal = monthGroup.Sum(r => TryGetDecimal(r, "TotalCost") ?? 0m);
                var monthPanel = new Panel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Top,
                    Padding = new Padding(0, 0, 0, 10)
                };

                var monthLabel = new Label
                {
                    Text = $"{FormatMonthLabel(monthGroup.Key)} - Tổng {monthTotal:N0}đ",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 79, 159),
                    Margin = new Padding(0, 0, 0, 6),
                    Padding = new Padding(0, 0, 0, 2)
                };

                var monthContent = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Margin = new Padding(0, 4, 0, 0)
                };

                foreach (var reading in monthGroup.OrderByDescending(r => TryGetDate(r, "ReadingDate") ?? DateTime.MinValue))
                {
                    var card = new Panel
                    {
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Padding = new Padding(10),
                        BackColor = Color.FromArgb(246, 250, 255),
                        Margin = new Padding(0, 0, 0, 8),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    int minWidth = Math.Max(520, _utilitiesInfoHost?.ClientSize.Width - 30 ?? 520);
                    card.MinimumSize = new Size(minWidth, 0);

                    var title = ReadString(reading, "UtilityName") ?? "Dịch vụ";
                    var code = ReadString(reading, "UtilityCode");
                    if (!string.IsNullOrWhiteSpace(code))
                        title = $"{title} ({code})";

                    var header = new TableLayoutPanel
                    {
                        AutoSize = true,
                        ColumnCount = 2,
                        Dock = DockStyle.Top,
                        Margin = new Padding(0, 0, 0, 6)
                    };
                    header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

                    var lblTitle = new Label
                    {
                        Text = title,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        ForeColor = Color.FromArgb(30, 55, 90),
                        Margin = new Padding(0, 0, 6, 0)
                    };

                    var actions = new FlowLayoutPanel
                    {
                        AutoSize = true,
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = false,
                        Margin = new Padding(0)
                    };
                    var btnEdit = new Button
                    {
                        Text = "Chỉnh sửa",
                        Width = 90,
                        Height = 28,
                        BackColor = Color.FromArgb(111, 66, 193),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnEdit.FlatAppearance.BorderSize = 0;
                    btnEdit.Click += async (s, e) => await EditUtilityReadingAsync(reading);
                    actions.Controls.Add(btnEdit);

                    header.Controls.Add(lblTitle, 0, 0);
                    header.Controls.Add(actions, 1, 0);

                    var layout = new TableLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        AutoSize = true,
                        ColumnCount = 2,
                        RowCount = 10,
                        Padding = new Padding(0)
                    };
                    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
                    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                    AddRow(layout, "Ngày ghi:", FormatDate(ReadString(reading, "ReadingDate")));
                    AddRow(layout, "Chỉ số cũ:", FormatNumber(reading, "PreviousReading", "0.##"));
                    AddRow(layout, "Chỉ số mới:", FormatNumber(reading, "CurrentReading", "0.##"));
                    AddRow(layout, "Tiêu thụ:", FormatNumber(reading, "UsageAmount", "0.##"));
                    AddRow(layout, "Đơn giá:", FormatNumber(reading, "UnitPrice", "N0"));
                    AddRow(layout, "Thành tiền:", FormatNumber(reading, "TotalCost", "N0"));
                    AddRow(layout, "Ghi chú:", ReadString(reading, "Notes"));
                    AddRow(layout, "Tạo lúc:", FormatDateTime(ReadString(reading, "CreatedDate")));

                    card.Controls.Add(header);
                    card.Controls.Add(layout);

                    var imagesPanel = BuildMeterImagesPanel(reading);
                    if (imagesPanel != null)
                        card.Controls.Add(imagesPanel);
                    monthContent.Controls.Add(card);
                }

                monthPanel.Controls.Add(monthContent);
                monthPanel.Controls.Add(monthLabel);
                container.Controls.Add(monthPanel);
            }

            _utilitiesInfoHost.Controls.Add(container);
        }

        private async System.Threading.Tasks.Task AddUtilityReadingAsync()
        {
            int roomId = TryGetInt(_roomRow, "RoomId");
            using (var frm = new FrmUtilityReadingEditor(_bll, null, roomId))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadUtilitiesAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task EditUtilityReadingAsync(DataRow row)
        {
            if (row == null) return;
            using (var frm = new FrmUtilityReadingEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadUtilitiesAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private Panel BuildMeterImagesPanel(DataRow reading)
        {
            if (reading == null) return null;
            var date = TryGetDate(reading, "ReadingDate");
            if (!date.HasValue) return null;

            string name = ReadString(reading, "UtilityName");
            string code = ReadString(reading, "UtilityCode");
            if (!IsElectric(code, name) && !IsWater(code, name)) return null;

            int roomId = TryGetInt(_roomRow, "RoomId");
            if (roomId <= 0) return null;

            string electricPath = FindImagePathByKind(roomId, "elec", date.Value);
            string waterPath = FindImagePathByKind(roomId, "water", date.Value);

            if (string.IsNullOrWhiteSpace(electricPath) && string.IsNullOrWhiteSpace(waterPath))
            {
                return new Panel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    Controls =
                    {
                        new Label
                        {
                            Text = "Chưa có ảnh đồng hồ cho tháng này.",
                            AutoSize = true,
                            ForeColor = Color.FromArgb(100, 100, 100),
                            Font = new Font("Segoe UI", 9, FontStyle.Italic),
                            Margin = new Padding(0, 2, 0, 6)
                        }
                    }
                };
            }

            var panel = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 6) };
            var lbl = new Label
            {
                Text = "Ảnh đồng hồ",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 55, 90),
                Margin = new Padding(0, 0, 0, 4)
            };

            var images = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            if (!string.IsNullOrWhiteSpace(electricPath))
                images.Controls.Add(BuildMeterCard("Điện", electricPath));
            if (!string.IsNullOrWhiteSpace(waterPath))
                images.Controls.Add(BuildMeterCard("Nước", waterPath));

            panel.Controls.Add(images);
            panel.Controls.Add(lbl);
            return panel;
        }

        private void PopulateUtilityMonthFilter()
        {
            if (_cboUtilityMonth == null) return;
            string current = GetSelectedUtilityMonthKey();

            var table = new DataTable();
            table.Columns.Add("Key", typeof(string));
            table.Columns.Add("Label", typeof(string));
            table.Rows.Add("all", "Tất cả");

            if (_utilities != null && _utilities.Rows.Count > 0)
            {
                int roomId = TryGetInt(_roomRow, "RoomId");
                var months = _utilities.AsEnumerable()
                    .Where(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == roomId)
                    .Select(r => (TryGetDate(r, "ReadingDate") ?? DateTime.MinValue).ToString("yyyy-MM"))
                    .Distinct()
                    .OrderByDescending(m => m)
                    .ToList();

                foreach (var key in months)
                    table.Rows.Add(key, FormatMonthLabel(key));
            }

            _cboUtilityMonth.DataSource = table;
            _cboUtilityMonth.DisplayMember = "Label";
            _cboUtilityMonth.ValueMember = "Key";
            if (!string.IsNullOrWhiteSpace(current))
                _cboUtilityMonth.SelectedValue = current;
        }

        private string GetSelectedUtilityMonthKey()
        {
            if (_cboUtilityMonth == null) return "all";
            if (_cboUtilityMonth.SelectedValue is string key)
                return key;
            if (_cboUtilityMonth.SelectedItem is DataRowView drv && drv.Row.Table.Columns.Contains("Key"))
                return drv.Row["Key"]?.ToString();
            return "all";
        }

        private Panel BuildMeterCard(string title, string path)
        {
            var panel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 12, 0)
            };

            var lbl = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Margin = new Padding(0, 0, 0, 4)
            };

            var pic = new PictureBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 180,
                Height = 120
            };
            SetMeterPreview(pic, path);

            panel.Controls.Add(pic);
            panel.Controls.Add(lbl);
            return panel;
        }

        private async System.Threading.Tasks.Task EditContractAsync(DataRow row)
        {
            if (row == null) return;
            using (var frm = new FrmContractEditor(_bll, row))
            {
                if (frm.ShowDialog(this) != DialogResult.OK) return;
                var refreshed = _bll != null ? await _bll.GetContractsAsync() : null;
                _contracts = refreshed;
                RenderContractsPanel(_contracts);
                AdminEvents.NotifyDataChanged();
            }
        }

        private async System.Threading.Tasks.Task DeleteContractAsync(DataRow row)
        {
            if (row == null) return;
            if (!row.Table.Columns.Contains("ContractId")) return;

            if (MessageBox.Show("Bạn chắc chắn muốn xóa hợp đồng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            try
            {
                int id = Convert.ToInt32(row["ContractId"]);
                await _bll.DeleteContractAsync(id);
                var refreshed = _bll != null ? await _bll.GetContractsAsync() : null;
                _contracts = refreshed;
                RenderContractsPanel(_contracts);
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa hợp đồng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task ReloadRoomAsync()
        {
            if (_bll == null || _roomRow == null) return;
            try
            {
                var rooms = await _bll.GetRoomsAsync();
                if (rooms == null || rooms.Rows.Count == 0) return;
                int roomId = TryGetInt(_roomRow, "RoomId");
                var refreshed = rooms.AsEnumerable()
                    .FirstOrDefault(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == roomId);
                if (refreshed != null)
                {
                    _roomRow = refreshed;
                    UpdateHeaderInfo();
                    RenderRoomInfoPanel();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lại phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateHeaderInfo()
        {
            if (_lblHeaderRoom != null)
                _lblHeaderRoom.Text = $"Phòng {ReadString(_roomRow, "RoomNumber")}";
            if (_lblHeaderInfo != null)
                _lblHeaderInfo.Text = $"Loại: {ReadRoomType()} | Trạng thái: {ReadRoomStatus()} | Giá: {TryGetDecimal(_roomRow, "RoomPrice"):N0}đ/tháng";
        }

        private string ReadRoomType()
        {
            var raw = ReadString(_roomRow, "TypeName") ?? ReadString(_roomRow, "RoomTypeName");
            return RoomTypeCatalog.Canonicalize(raw);
        }

        private string ReadRoomStatus()
        {
            return ReadString(_roomRow, "StatusName") ?? ReadString(_roomRow, "Status");
        }

        private void AddRow(TableLayoutPanel layout, string label, string value)
        {
            var lblLabel = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(70, 70, 70),
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 6)
            };

            var lblValue = new Label
            {
                Text = value ?? "—",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(30, 55, 90),
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 6)
            };

            layout.Controls.Add(lblLabel);
            layout.Controls.Add(lblValue);
        }

        private string ReadString(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return null;
            var v = r[col];
            if (v == DBNull.Value || v == null) return null;
            return v.ToString();
        }

        private int TryGetInt(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return 0;
            return int.TryParse(r[col]?.ToString(), out var val) ? val : 0;
        }

        private decimal? TryGetDecimal(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return null;
            return decimal.TryParse(r[col]?.ToString(), out var val) ? (decimal?)val : null;
        }

        private bool? TryGetBool(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return null;
            return bool.TryParse(r[col]?.ToString(), out var val) ? (bool?)val : null;
        }

        private string FormatDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return "—";
            if (DateTime.TryParse(dateStr, out var dt))
                return dt.ToString("dd/MM/yyyy");
            return dateStr;
        }

        private string FormatMoney(DataRow row, string column)
        {
            var value = TryGetDecimal(row, column);
            return value.HasValue ? value.Value.ToString("N0") : "—";
        }

        private string FormatNumber(DataRow row, string column, string format)
        {
            var value = TryGetDecimal(row, column);
            return value.HasValue ? value.Value.ToString(format) : "—";
        }

        private string FormatMonthLabel(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return "Không rõ tháng";
            if (DateTime.TryParse($"{key}-01", out var dt))
                return $"Tháng {dt:MM/yyyy}";
            return key;
        }

        private string FormatDateTime(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return "—";
            if (DateTime.TryParse(dateStr, out var dt))
                return dt.ToString("dd/MM/yyyy HH:mm");
            return dateStr;
        }

        private static DateTime? TryGetDate(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column)) return null;
            return DateTime.TryParse(row[column]?.ToString(), out var val) ? val : (DateTime?)null;
        }

        private static bool IsElectric(string code, string name)
        {
            string codeUpper = (code ?? string.Empty).ToUpperInvariant();
            string nameLower = (name ?? string.Empty).ToLowerInvariant();
            return codeUpper.Contains("ELEC") || nameLower.Contains("dien") || nameLower.Contains("điện");
        }

        private static bool IsWater(string code, string name)
        {
            string codeUpper = (code ?? string.Empty).ToUpperInvariant();
            string nameLower = (name ?? string.Empty).ToLowerInvariant();
            return codeUpper.Contains("WATER") || nameLower.Contains("nuoc") || nameLower.Contains("nước");
        }

        private static string GetImageFolder()
        {
            string root = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "UtilityMeterImages");
            Directory.CreateDirectory(root);
            return root;
        }

        private static string BuildImageKey(int roomId, string kind, DateTime date)
        {
            return $"{roomId}_{kind}_{date:yyyyMM}";
        }

        private static string FindImagePathByKind(int roomId, string kind, DateTime date)
        {
            string folder = GetImageFolder();
            string key = BuildImageKey(roomId, kind, date);
            var files = Directory.GetFiles(folder, key + ".*");
            return files.Length > 0 ? files[0] : null;
        }

        private static void SetMeterPreview(PictureBox target, string path)
        {
            if (target == null) return;
            try
            {
                if (target.Image != null)
                {
                    var old = target.Image;
                    target.Image = null;
                    old.Dispose();
                }
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    target.Image = null;
                    return;
                }

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    target.Image = Image.FromStream(fs);
                }
            }
            catch
            {
                target.Image = null;
            }
        }
    }
}
