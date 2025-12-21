using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmRoomDetailForm : Form
    {
        private readonly AdminDataBLL _bll;
        private DataRow _roomRow;
        private readonly DataTable _contracts;
        private readonly DataTable _tenantHistory;
        private DataTable _tenants;
        private DataRow _activeTenantRow;
        private DataRow _activeTenantHistory;
        private Panel _tenantInfoHost;
        private DataGridView _contractsGrid;
        private Button _btnEditTenant;
        private Button _btnEditContract;
        private Button _btnDeleteContract;
        private Button _btnEditRoom;
        private Panel _roomInfoHost;
        private Label _lblHeaderRoom;
        private Label _lblHeaderInfo;

        public FrmRoomDetailForm(AdminDataBLL bll, DataRow roomRow, DataTable contracts, DataTable tenantHistory)
        {
            _bll = bll;
            _roomRow = roomRow;
            _contracts = contracts;
            _tenantHistory = tenantHistory;
            InitializeComponent();
            Load += async (s, e) => await LoadTenantsAsync();
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

            var tabControl = new TabControl { Dock = DockStyle.Fill, BackColor = Color.White };

            // Tab 1: Thông tin phòng
            var tabRoom = new TabPage("Thông tin phòng") { BackColor = Color.White, Padding = new Padding(12) };
            var roomPanel = BuildRoomInfoPanel();
            tabRoom.Controls.Add(roomPanel);
            tabControl.TabPages.Add(tabRoom);

            // Tab 2: Thông tin khách thuê hiện tại
            var tabTenant = new TabPage("Khách thuê hiện tại") { BackColor = Color.White, Padding = new Padding(12) };
            var tenantPanel = BuildTenantInfoPanel();
            tabTenant.Controls.Add(tenantPanel);
            tabControl.TabPages.Add(tabTenant);

            // Tab 3: Lịch sử hợp đồng
            var tabContracts = new TabPage("Lịch sử hợp đồng") { BackColor = Color.White, Padding = new Padding(12) };
            var contractsPanel = BuildContractsPanel();
            tabContracts.Controls.Add(contractsPanel);
            tabControl.TabPages.Add(tabContracts);

            contentPanel.Controls.Add(tabControl);

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
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            var actionBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 6)
            };

            _btnEditContract = new Button
            {
                Text = "Chỉnh sửa",
                Width = 110,
                Height = 32,
                BackColor = Color.FromArgb(111, 66, 193),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnEditContract.FlatAppearance.BorderSize = 0;
            _btnEditContract.Click += async (s, e) => await EditSelectedContractAsync();

            _btnDeleteContract = new Button
            {
                Text = "Xóa",
                Width = 90,
                Height = 32,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 0, 0, 0)
            };
            _btnDeleteContract.FlatAppearance.BorderSize = 0;
            _btnDeleteContract.Click += async (s, e) => await DeleteSelectedContractAsync();

            actionBar.Controls.Add(_btnEditContract);
            actionBar.Controls.Add(_btnDeleteContract);

            _contractsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _contractsGrid.EnableHeadersVisualStyles = false;
            _contractsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            _contractsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _contractsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _contractsGrid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            _contractsGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);

            BindContractsGrid(_contracts);

            panel.Controls.Add(_contractsGrid);
            panel.Controls.Add(actionBar);
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

        private void BindContractsGrid(DataTable source)
        {
            if (_contractsGrid == null) return;
            var table = new DataTable();
            if (source != null && source.Rows.Count > 0)
            {
                int roomId = TryGetInt(_roomRow, "RoomId");
                var rows = source.AsEnumerable()
                    .Where(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == roomId);
                if (rows.Any())
                    table = rows.CopyToDataTable();
            }

            _contractsGrid.DataSource = table;
            if (_contractsGrid.Columns.Contains("ContractId")) _contractsGrid.Columns["ContractId"].HeaderText = "ID";
            if (_contractsGrid.Columns.Contains("ContractNumber")) _contractsGrid.Columns["ContractNumber"].HeaderText = "Số HĐ";
            if (_contractsGrid.Columns.Contains("TenantName")) _contractsGrid.Columns["TenantName"].HeaderText = "Khách thuê";
            if (_contractsGrid.Columns.Contains("StartDate")) _contractsGrid.Columns["StartDate"].HeaderText = "Ngày bắt đầu";
            if (_contractsGrid.Columns.Contains("EndDate")) _contractsGrid.Columns["EndDate"].HeaderText = "Ngày kết thúc";
        }

        private async System.Threading.Tasks.Task EditSelectedContractAsync()
        {
            if (_contractsGrid == null || _contractsGrid.CurrentRow == null) return;
            if (!(_contractsGrid.CurrentRow.DataBoundItem is DataRowView drv)) return;
            var row = drv.Row;
            using (var frm = new FrmContractEditor(_bll, row))
            {
                if (frm.ShowDialog(this) != DialogResult.OK) return;
                var refreshed = _bll != null ? await _bll.GetContractsAsync() : null;
                BindContractsGrid(refreshed);
                AdminEvents.NotifyDataChanged();
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedContractAsync()
        {
            if (_contractsGrid == null || _contractsGrid.CurrentRow == null) return;
            if (!(_contractsGrid.CurrentRow.DataBoundItem is DataRowView drv)) return;

            var row = drv.Row;
            if (!row.Table.Columns.Contains("ContractId")) return;

            if (MessageBox.Show("Bạn chắc chắn muốn xóa hợp đồng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            try
            {
                int id = Convert.ToInt32(row["ContractId"]);
                await _bll.DeleteContractAsync(id);
                var refreshed = _bll != null ? await _bll.GetContractsAsync() : null;
                BindContractsGrid(refreshed);
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
    }
}
