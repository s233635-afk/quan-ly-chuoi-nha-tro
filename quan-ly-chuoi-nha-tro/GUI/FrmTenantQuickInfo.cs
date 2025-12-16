using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmTenantQuickInfo : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly int _tenantId;
        private DataTable _tenantData;
        private DataTable _dependents;
        private DataTable _contracts;

        public FrmTenantQuickInfo(AdminDataBLL bll, int tenantId)
        {
            _bll = bll ?? new AdminDataBLL();
            _tenantId = tenantId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"Thông Tin Khách Thuê - ID {_tenantId}";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 700;
            this.Height = 650;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Toolbar với nút đóng
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(12, 8, 12, 8)
            };

            var lblTitle = new Label
            {
                Text = "Chi tiết khách thuê",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.FromArgb(0, 120, 215)
            };

            var btnClose = new Button
            {
                Text = "Đóng",
                Width = 80,
                Height = 34,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(220, 220, 220),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black
            };
            btnClose.FlatAppearance.BorderSize = 1;
            btnClose.FlatAppearance.BorderColor = Color.Silver;
            btnClose.Click += (s, e) => this.Close();

            topBar.Controls.Add(btnClose);
            topBar.Controls.Add(lblTitle);

            // TabControl
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(0, 0)
            };
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += (s, e) =>
            {
                var tab = tabControl.TabPages[e.Index];
                var font = new Font("Segoe UI", 10, FontStyle.Regular);
                e.Graphics.DrawString(tab.Text, font, Brushes.Black, e.Bounds.X + 10, e.Bounds.Y + 6);
            };

            // Tab 1: Thông tin cơ bản
            var tabBasic = new TabPage { Text = "Thông tin cơ bản", Padding = new Padding(15) };
            tabBasic.BackColor = Color.White;
            var pnlBasic = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            tabBasic.Controls.Add(pnlBasic);

            // Tab 2: Người phụ thuộc
            var tabDependents = new TabPage { Text = "Người phụ thuộc", Padding = new Padding(10) };
            tabDependents.BackColor = Color.White;
            var gridDependents = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            gridDependents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            gridDependents.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridDependents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            gridDependents.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            tabDependents.Controls.Add(gridDependents);

            // Tab 3: Hợp đồng
            var tabContracts = new TabPage { Text = "Hợp đồng", Padding = new Padding(10) };
            tabContracts.BackColor = Color.White;
            var gridContracts = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            gridContracts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            gridContracts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridContracts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            gridContracts.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            tabContracts.Controls.Add(gridContracts);

            tabControl.TabPages.Add(tabBasic);
            tabControl.TabPages.Add(tabDependents);
            tabControl.TabPages.Add(tabContracts);

            this.Controls.Add(tabControl);
            this.Controls.Add(topBar);

            this.Load += async (s, e) => await LoadDataAsync(pnlBasic, gridDependents, gridContracts);
        }

        private async Task LoadDataAsync(Panel basicPanel, DataGridView dependentsGrid, DataGridView contractsGrid)
        {
            try
            {
                // Load tenant data
                var allTenants = await _bll.GetTenantsAsync();
                _tenantData = new DataTable();
                foreach (DataColumn col in allTenants.Columns)
                    _tenantData.Columns.Add(col.ColumnName, col.DataType);

                foreach (DataRow row in allTenants.Rows)
                {
                    if (row["TenantId"] != DBNull.Value && Convert.ToInt32(row["TenantId"]) == _tenantId)
                    {
                        _tenantData.ImportRow(row);
                        break;
                    }
                }

                if (_tenantData.Rows.Count > 0)
                    DisplayBasicInfo(basicPanel, _tenantData.Rows[0]);

                // Load dependents
                _dependents = await _bll.GetDependentsAsync();
                FilterDependentsByTenant(_dependents, _tenantId);
                dependentsGrid.DataSource = _dependents;
                FormatDependentsGrid(dependentsGrid);

                // Load contracts
                _contracts = await _bll.GetContractsAsync();
                FilterContractsByTenant(_contracts, _tenantId);
                contractsGrid.DataSource = _contracts;
                FormatContractsGrid(contractsGrid);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayBasicInfo(Panel panel, DataRow tenant)
        {
            panel.Controls.Clear();
            int y = 0;
            const int itemHeight = 35;

            var fields = new[]
            {
                ("Họ và tên:", "FullName"),
                ("CMND/CCCD:", "IdentityCard"),
                ("Số điện thoại:", "PhoneNumber"),
                ("Email:", "Email"),
                ("Ngày sinh:", "BirthDate"),
                ("Địa chỉ:", "Address"),
                ("Sổ tạm trú:", "TempReg"),
                ("Ngày lập sổ:", "TempRegDate"),
                ("Hết hạn sổ:", "TempRegExpiry"),
                ("Trạng thái:", "IsActive")
            };

            foreach (var (label, fieldName) in fields)
            {
                var lbl = new Label
                {
                    Text = label,
                    Location = new Point(15, y + 5),
                    Width = 130,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(70, 70, 70)
                };

                string value = "";
                if (tenant.Table.Columns.Contains(fieldName))
                {
                    var val = tenant[fieldName];
                    if (val != DBNull.Value)
                    {
                        if (fieldName == "BirthDate" || fieldName == "TempRegDate" || fieldName == "TempRegExpiry")
                        {
                            if (DateTime.TryParse(val.ToString(), out var dt))
                                value = dt.ToString("dd/MM/yyyy");
                        }
                        else if (fieldName == "IsActive")
                        {
                            value = Convert.ToBoolean(val) ? "Hoạt động" : "Không hoạt động";
                        }
                        else
                            value = val.ToString();
                    }
                }

                var txt = new TextBox
                {
                    Text = value,
                    Location = new Point(150, y),
                    Width = panel.Width - 180,
                    Height = 30,
                    ReadOnly = true,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(250, 250, 250),
                    Font = new Font("Segoe UI", 9)
                };

                panel.Controls.Add(lbl);
                panel.Controls.Add(txt);
                y += itemHeight;
            }
        }

        private void FilterDependentsByTenant(DataTable dependents, int tenantId)
        {
            if (dependents == null || dependents.Rows.Count == 0)
                return;

            for (int i = dependents.Rows.Count - 1; i >= 0; i--)
            {
                var row = dependents.Rows[i];
                if (row["TenantId"] != DBNull.Value && Convert.ToInt32(row["TenantId"]) != tenantId)
                    row.Delete();
            }
            dependents.AcceptChanges();
        }

        private void FilterContractsByTenant(DataTable contracts, int tenantId)
        {
            if (contracts == null || contracts.Rows.Count == 0)
                return;

            for (int i = contracts.Rows.Count - 1; i >= 0; i--)
            {
                var row = contracts.Rows[i];
                if (row["TenantId"] != DBNull.Value && Convert.ToInt32(row["TenantId"]) != tenantId)
                    row.Delete();
            }
            contracts.AcceptChanges();
        }

        private void FormatDependentsGrid(DataGridView grid)
        {
            if (grid.Columns.Count == 0) return;

            foreach (DataGridViewColumn col in grid.Columns)
                col.Visible = !col.Name.Contains("TenantId");

            if (grid.Columns.Contains("DependentName"))
                grid.Columns["DependentName"].HeaderText = "Tên người phụ thuộc";
            if (grid.Columns.Contains("Relationship"))
                grid.Columns["Relationship"].HeaderText = "Quan hệ";
            if (grid.Columns.Contains("PhoneNumber"))
                grid.Columns["PhoneNumber"].HeaderText = "Số điện thoại";
        }

        private void FormatContractsGrid(DataGridView grid)
        {
            if (grid.Columns.Count == 0) return;

            foreach (DataGridViewColumn col in grid.Columns)
                col.Visible = !col.Name.Contains("TenantId") && !col.Name.Contains("BranchId");

            if (grid.Columns.Contains("ContractNumber"))
                grid.Columns["ContractNumber"].HeaderText = "Số HĐ";
            if (grid.Columns.Contains("RoomNumber"))
                grid.Columns["RoomNumber"].HeaderText = "Phòng";
            if (grid.Columns.Contains("StartDate"))
                grid.Columns["StartDate"].HeaderText = "Ngày bắt đầu";
            if (grid.Columns.Contains("EndDate"))
                grid.Columns["EndDate"].HeaderText = "Ngày kết thúc";
            if (grid.Columns.Contains("RentalPrice"))
                grid.Columns["RentalPrice"].HeaderText = "Giá thuê";
        }
    }
}
