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
        private readonly DataRow _roomRow;
        private readonly DataTable _contracts;
        private readonly DataTable _tenantHistory;

        public FrmRoomDetailForm(AdminDataBLL bll, DataRow roomRow, DataTable contracts, DataTable tenantHistory)
        {
            _bll = bll;
            _roomRow = roomRow;
            _contracts = contracts;
            _tenantHistory = tenantHistory;
            InitializeComponent();
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

            var lblRoom = new Label
            {
                Text = $"Phòng {ReadString(_roomRow, "RoomNumber")}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 40,
                AutoSize = false
            };

            var lblInfo = new Label
            {
                Text = $"Loại: {ReadString(_roomRow, "TypeName")} | Trạng thái: {ReadString(_roomRow, "StatusName")} | Giá: {TryGetDecimal(_roomRow, "RoomPrice"):N0}đ/tháng",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false
            };

            headerPanel.Controls.Add(lblInfo);
            headerPanel.Controls.Add(lblRoom);

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
                Text = "Đóng",
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
            AddRow(layout, "Loại phòng:", ReadString(_roomRow, "TypeName"));
            AddRow(layout, "Trạng thái:", ReadString(_roomRow, "StatusName"));
            AddRow(layout, "Giá/Tháng:", (TryGetDecimal(_roomRow, "RoomPrice") ?? 0m).ToString("N0") + "đ");
            AddRow(layout, "Diện tích:", (TryGetDecimal(_roomRow, "Area") ?? 0m).ToString("0.##") + " m²");
            AddRow(layout, "Số người:", TryGetInt(_roomRow, "Occupants").ToString());
            AddRow(layout, "Tầng:", TryGetInt(_roomRow, "Floor").ToString());
            AddRow(layout, "Kích hoạt:", TryGetBool(_roomRow, "IsActive") == true ? "Có" : "Không");

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel BuildTenantInfoPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };

            if (_tenantHistory == null || _tenantHistory.Rows.Count == 0)
            {
                panel.Controls.Add(new Label
                {
                    Text = "Không có khách thuê hiện tại",
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Dock = DockStyle.Top,
                    Height = 30
                });
                return panel;
            }

            int roomId = TryGetInt(_roomRow, "RoomId");
            var activeTenant = _tenantHistory.AsEnumerable()
                .Where(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == roomId &&
                            string.IsNullOrWhiteSpace(r["CheckOutDate"]?.ToString()))
                .FirstOrDefault();

            if (activeTenant == null)
            {
                panel.Controls.Add(new Label
                {
                    Text = "Không có khách thuê hiện tại",
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Dock = DockStyle.Top,
                    Height = 30
                });
                return panel;
            }

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

            AddRow(layout, "Tên khách:", ReadString(activeTenant, "TenantName"));
            AddRow(layout, "SĐT:", ReadString(activeTenant, "PhoneNumber"));
            AddRow(layout, "Email:", ReadString(activeTenant, "Email"));
            AddRow(layout, "CCCD:", ReadString(activeTenant, "IdentityCard"));
            AddRow(layout, "Ngày sinh:", FormatDate(ReadString(activeTenant, "DateOfBirth")));
            AddRow(layout, "Địa chỉ:", ReadString(activeTenant, "Address"));
            AddRow(layout, "Tạm trú tại:", ReadString(activeTenant, "TemporaryRegistration"));
            AddRow(layout, "Tạm trú từ:", FormatDate(ReadString(activeTenant, "TemporaryRegFrom")));
            AddRow(layout, "Tạm trú đến:", FormatDate(ReadString(activeTenant, "TemporaryRegTo")));
            AddRow(layout, "Ảnh CCCD (trước):", ReadString(activeTenant, "FrontIdImage"));
            AddRow(layout, "Ảnh CCCD (sau):", ReadString(activeTenant, "BackIdImage"));
            AddRow(layout, "Ngày vào:", FormatDate(ReadString(activeTenant, "CheckInDate")));

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel BuildContractsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);

            if (_contracts != null && _contracts.Rows.Count > 0)
            {
                int roomId = TryGetInt(_roomRow, "RoomId");
                var roomContracts = _contracts.AsEnumerable()
                    .Where(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == roomId)
                    .CopyToDataTable();

                if (roomContracts.Rows.Count > 0)
                {
                    grid.DataSource = roomContracts;
                    if (grid.Columns.Contains("ContractId")) grid.Columns["ContractId"].HeaderText = "ID";
                    if (grid.Columns.Contains("ContractNumber")) grid.Columns["ContractNumber"].HeaderText = "Số HĐ";
                    if (grid.Columns.Contains("TenantName")) grid.Columns["TenantName"].HeaderText = "Khách thuê";
                    if (grid.Columns.Contains("StartDate")) grid.Columns["StartDate"].HeaderText = "Ngày bắt đầu";
                    if (grid.Columns.Contains("EndDate")) grid.Columns["EndDate"].HeaderText = "Ngày kết thúc";
                }
            }

            panel.Controls.Add(grid);
            return panel;
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
