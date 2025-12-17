using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
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
        private DataTable _contracts;
        private DataTable _history;
        private bool _isEditMode = false;
        private Dictionary<string, TextBox> _fieldTextBoxes = new Dictionary<string, TextBox>();
        private Button _btnEdit, _btnSave, _btnCancel;
        private DataGridView _gridContracts, _gridHistory;
        private Panel _pnlBasic;

        public FrmTenantQuickInfo(AdminDataBLL bll, int tenantId)
        {
            _bll = bll ?? new AdminDataBLL();
            _tenantId = tenantId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Thông Tin Khách Thuê";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 900;
            this.Height = 800;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Header Bar
            var headerBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 120, 215),
                Padding = new Padding(20, 10, 20, 10)
            };

            var lblTenantName = new Label
            {
                Text = "Khách Thuê #" + _tenantId,
                Font = new Font("Times New Roman", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            var btnClose = new Button
            {
                Text = "✕",
                Width = 45,
                Height = 40,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(0, 100, 180),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Times New Roman", 16, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            headerBar.Controls.Add(btnClose);
            headerBar.Controls.Add(lblTenantName);

            // Main content with TabControl
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(0, 0),
                BackColor = Color.White
            };
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.ItemSize = new Size(300, 65);  // Nhỏ hơn, gọn gàng hơn
            tabControl.SizeMode = TabSizeMode.Fixed;  // Fixed size
            tabControl.DrawItem += (s, e) =>
            {
                if (e.Index < 0 || e.Index >= tabControl.TabPages.Count) return;
                
                var tab = tabControl.TabPages[e.Index];
                var font = new Font("Times New Roman", 11, FontStyle.Bold);
                var color = (e.State == DrawItemState.Selected) ? Color.FromArgb(0, 120, 215) : Color.FromArgb(100, 100, 100);
                
                // Vẽ background cho tab
                e.Graphics.FillRectangle(new SolidBrush(e.State == DrawItemState.Selected ? Color.White : Color.FromArgb(240, 242, 245)), e.Bounds);
                
                // Measure text to center it properly
                var textSize = e.Graphics.MeasureString(tab.Text, font);
                var x = e.Bounds.X + (e.Bounds.Width - textSize.Width) / 2;
                var y = e.Bounds.Y + (e.Bounds.Height - textSize.Height) / 2;
                
                // Vẽ text ở giữa
                e.Graphics.DrawString(tab.Text, font, new SolidBrush(color), x, y);
                
                // Vẽ underline cho tab selected
                if (e.State == DrawItemState.Selected)
                    e.Graphics.DrawLine(new Pen(Color.FromArgb(0, 120, 215), 3), e.Bounds.X, e.Bounds.Bottom - 3, e.Bounds.Right, e.Bounds.Bottom - 3);
            };

            // Tab 1: Thông tin chi tiết
            var tabBasic = new TabPage { Text = "Thông Tin", Padding = new Padding(20) };
            tabBasic.BackColor = Color.White;
            _pnlBasic = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            tabBasic.Controls.Add(_pnlBasic);

            // Tab 2: Hợp đồng
            var tabContracts = new TabPage { Text = "Hợp Đồng", Padding = new Padding(10) };
            tabContracts.BackColor = Color.White;
            _gridContracts = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                MultiSelect = false,
                AutoGenerateColumns = true
            };
            _gridContracts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            _gridContracts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridContracts.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Bold);
            _gridContracts.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridContracts.ColumnHeadersHeight = 35;
            _gridContracts.DefaultCellStyle.Font = new Font("Times New Roman", 10);
            _gridContracts.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _gridContracts.RowTemplate.Height = 28;
            _gridContracts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            _gridContracts.EnableHeadersVisualStyles = false;
            _gridContracts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            tabContracts.Controls.Add(_gridContracts);

            // Tab 3: Lịch sử nhân phòng
            var tabHistory = new TabPage { Text = "Lịch Sử", Padding = new Padding(10) };
            tabHistory.BackColor = Color.White;
            _gridHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                MultiSelect = false,
                AutoGenerateColumns = true
            };
            _gridHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            _gridHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Bold);
            _gridHistory.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridHistory.ColumnHeadersHeight = 35;
            _gridHistory.DefaultCellStyle.Font = new Font("Times New Roman", 10);
            _gridHistory.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _gridHistory.RowTemplate.Height = 28;
            _gridHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            _gridHistory.EnableHeadersVisualStyles = false;
            _gridHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            tabHistory.Controls.Add(_gridHistory);

            tabControl.TabPages.Add(tabBasic);
            tabControl.TabPages.Add(tabContracts);
            tabControl.TabPages.Add(tabHistory);

            // Bottom Button Bar
            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            _btnEdit = new Button
            {
                Text = "Sửa",
                Width = 100,
                Height = 34,
                BackColor = Color.FromArgb(0, 120, 215),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Times New Roman", 11, FontStyle.Bold),
                Location = new Point(0, 8)
            };
            _btnEdit.FlatAppearance.BorderSize = 0;
            _btnEdit.Click += (s, e) => ToggleEditMode(_pnlBasic);

            _btnSave = new Button
            {
                Text = "Lưu",
                Width = 100,
                Height = 34,
                BackColor = Color.FromArgb(46, 125, 50),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Times New Roman", 11, FontStyle.Bold),
                Location = new Point(110, 8),
                Visible = false
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (s, e) => await SaveTenantInfoAsync();

            _btnCancel = new Button
            {
                Text = "Hủy",
                Width = 100,
                Height = 34,
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black,
                Font = new Font("Times New Roman", 11, FontStyle.Bold),
                Location = new Point(220, 8),
                Visible = false
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => ToggleEditMode(_pnlBasic);

            bottomBar.Controls.Add(_btnEdit);
            bottomBar.Controls.Add(_btnSave);
            bottomBar.Controls.Add(_btnCancel);

            this.Controls.Add(tabControl);
            this.Controls.Add(bottomBar);
            this.Controls.Add(headerBar);

            this.Load += async (s, e) => await LoadDataAsync(_pnlBasic, _gridContracts, _gridHistory);
            
            // Auto-refresh history when form is activated (e.g., after adding deposit)
            this.Activated += async (s, e) => await ReloadContractsAndHistoryAsync();
        }

        private void ToggleEditMode(Panel basicPanel)
        {
            _isEditMode = !_isEditMode;

            foreach (Control ctrl in basicPanel.Controls)
            {
                if (ctrl is TextBox txt)
                {
                    txt.ReadOnly = !_isEditMode;
                    txt.BackColor = _isEditMode ? Color.White : Color.FromArgb(250, 250, 250);
                }
            }

            _btnEdit.Visible = !_isEditMode;
            _btnSave.Visible = _isEditMode;
            _btnCancel.Visible = _isEditMode;
        }

        private async Task SaveTenantInfoAsync()
        {
            try
            {
                var fullName = GetTextBoxValue("FullName");
                var phoneNumber = GetTextBoxValue("PhoneNumber");
                var email = GetTextBoxValue("Email");
                var identityCard = GetTextBoxValue("IdentityCard");
                var birthDate = GetDateValue("BirthDate");
                var address = GetTextBoxValue("Address");
                var tempReg = GetTextBoxValue("TempReg");
                var tempRegDate = GetDateValue("TempRegDate");
                var tempRegExpiry = GetDateValue("TempRegExpiry");

                bool result = await _bll.UpdateTenantAsync(
                    _tenantId, fullName, identityCard, phoneNumber, email, birthDate, address, tempReg, tempRegDate, tempRegExpiry, true
                );

                if (result)
                {
                    MessageBox.Show("Cập nhật thông tin khách thuê thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Auto-reload contracts and history
                    await ReloadContractsAndHistoryAsync();

                    // Signal parent form to reload
                    this.DialogResult = DialogResult.OK;

                    // Toggle back to view mode
                    ToggleEditMode(_pnlBasic);
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ReloadContractsAndHistoryAsync()
        {
            try
            {
                // Load contracts with null checks
                _contracts = await _bll.GetContractsAsync();
                if (_contracts != null && _gridContracts != null && _contracts.Columns.Contains("TenantId"))
                {
                    var filteredContracts = _contracts.Clone();
                    var rows = _contracts.Select($"TenantId = {_tenantId}");
                    foreach (var row in rows)
                    {
                        filteredContracts.ImportRow(row);
                    }
                    _gridContracts.DataSource = filteredContracts;
                    try
                    {
                        FormatContractsGrid(_gridContracts);
                    }
                    catch { }
                }

                // Load history with null checks
                _history = await _bll.GetTenantHistoryAsync();
                if (_history != null && _gridHistory != null && _history.Columns.Contains("TenantId"))
                {
                    var filteredHistory = _history.Clone();
                    var rows = _history.Select($"TenantId = {_tenantId}");
                    foreach (var row in rows)
                    {
                        filteredHistory.ImportRow(row);
                    }
                    _gridHistory.DataSource = filteredHistory;
                    try
                    {
                        FormatHistoryGrid(_gridHistory);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadDataAsync(Panel basicPanel, DataGridView contractsGrid, DataGridView historyGrid)
        {
            try
            {
                // Safety check - ensure panels exist
                if (basicPanel == null || contractsGrid == null || historyGrid == null)
                    return;

                var allTenants = await _bll.GetTenantsAsync();
                if (allTenants == null)
                    return;

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

                // Load contracts
                _contracts = await _bll.GetContractsAsync();
                if (_contracts != null && _contracts.Columns.Contains("TenantId"))
                {
                    var filteredContracts = _contracts.Clone();
                    var rows = _contracts.Select($"TenantId = {_tenantId}");
                    foreach (var row in rows)
                    {
                        filteredContracts.ImportRow(row);
                    }
                    contractsGrid.DataSource = filteredContracts;
                    try
                    {
                        FormatContractsGrid(contractsGrid);
                    }
                    catch { }
                }

                // Load history
                _history = await _bll.GetTenantHistoryAsync();
                if (_history != null && _history.Columns.Contains("TenantId"))
                {
                    var filteredHistory = _history.Clone();
                    var rows = _history.Select($"TenantId = {_tenantId}");
                    foreach (var row in rows)
                    {
                        filteredHistory.ImportRow(row);
                    }
                    historyGrid.DataSource = filteredHistory;
                    try
                    {
                        FormatHistoryGrid(historyGrid);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayBasicInfo(Panel panel, DataRow tenant)
        {
            panel.Controls.Clear();
            _fieldTextBoxes.Clear();
            int y = 0;

            var sections = new (string title, string[] fields)[]
            {
                ("THÔNG TIN CÁ NHÂN", new[] {
                    "FullName:Họ và tên",
                    "IdentityCard:CMND/CCCD",
                    "BirthDate:Ngày sinh",
                    "Address:Địa chỉ"
                }),
                ("THÔNG TIN LIÊN HỆ", new[] {
                    "PhoneNumber:Số điện thoại",
                    "Email:Email"
                }),
                ("THÔNG TIN TẠM TRÚ", new[] {
                    "TempReg:Sổ tạm trú",
                    "TempRegDate:Ngày lập sổ",
                    "TempRegExpiry:Hết hạn sổ"
                })
            };

            foreach (var (sectionTitle, fields) in sections)
            {
                var lblSection = new Label
                {
                    Text = sectionTitle,
                    Font = new Font("Times New Roman", 11, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 120, 215),
                    AutoSize = true,
                    Location = new Point(0, y)
                };
                panel.Controls.Add(lblSection);
                y += 28;

                var divider = new Panel
                {
                    Height = 1,
                    BackColor = Color.FromArgb(200, 220, 240),
                    Location = new Point(0, y - 5),
                    Width = panel.Width - 30
                };
                panel.Controls.Add(divider);
                y += 12;

                foreach (var fieldDef in fields)
                {
                    var parts = fieldDef.Split(':');
                    var fieldName = parts[0];
                    var label = parts[1];

                    var lbl = new Label
                    {
                        Text = label + ":",
                        Location = new Point(0, y + 5),
                        Width = 130,
                        Font = new Font("Times New Roman", 10, FontStyle.Regular),
                        ForeColor = Color.FromArgb(70, 70, 70)
                    };

                    string value = GetFieldValue(tenant, fieldName);

                    var txt = new TextBox
                    {
                        Text = value,
                        Location = new Point(140, y),
                        Width = panel.Width - 170,
                        Height = 28,
                        ReadOnly = true,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.FromArgb(250, 250, 250),
                        Font = new Font("Times New Roman", 10)
                    };

                    _fieldTextBoxes[fieldName] = txt;

                    panel.Controls.Add(lbl);
                    panel.Controls.Add(txt);
                    y += 38;
                }

                y += 8;
            }
        }

        private string GetFieldValue(DataRow row, string fieldName)
        {
            if (row.Table.Columns.Contains(fieldName))
            {
                var value = row[fieldName];
                if (value == DBNull.Value)
                    return "";
                if (fieldName.Contains("Date") && value is DateTime dt)
                    return dt.ToString("dd/MM/yyyy");
                return value.ToString();
            }
            return "";
        }

        private string GetTextBoxValue(string fieldName)
        {
            if (_fieldTextBoxes.ContainsKey(fieldName))
                return _fieldTextBoxes[fieldName].Text ?? "";
            return "";
        }

        private DateTime? GetDateValue(string fieldName)
        {
            string val = GetTextBoxValue(fieldName);
            if (string.IsNullOrWhiteSpace(val))
                return null;
            if (DateTime.TryParse(val, out var date))
                return date;
            return null;
        }

        private void FilterContractsByTenant(DataTable contracts, int tenantId)
        {
            if (contracts == null) return;
            var rows = contracts.Select($"TenantId = {tenantId}");
            foreach (var row in rows)
                row.SetAdded();
        }

        private void FilterHistoryByTenant(DataTable history, int tenantId)
        {
            if (history == null) return;
            var rows = history.Select($"TenantId = {tenantId}");
            foreach (var row in rows)
                row.SetAdded();
        }

        private void FormatContractsGrid(DataGridView grid)
        {
            if (grid == null || grid.Columns.Count == 0) return;
            
            // Comprehensive translation dictionary - all possible column names
            var columnMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // CamelCase names
                { "ContractId", "ID" },
                { "TenantId", "Khách" },
                { "ContractNumber", "Số Hợp Đồng" },
                { "ContractType", "Loại Hợp Đồng" },
                { "RoomId", "Phòng ID" },
                { "RoomNumber", "Số Phòng" },
                { "BranchId", "Chi Nhánh" },
                { "SignDate", "Ngày Ký" },
                { "StartDate", "Ngày Bắt Đầu" },
                { "EndDate", "Ngày Kết Thúc" },
                { "RentAmount", "Tiền Thuê/Tháng" },
                { "RentalPrice", "Giá Thuê" },
                { "DepositAmount", "Tiền Cọc" },
                { "DepositRequired", "Cọc Yêu Cầu" },
                { "RentalPeriod", "Thời Hạn (Tháng)" },
                { "Status", "Trạng Thái" },
                { "CreatedDate", "Ngày Tạo" },
                { "CreatedBy", "Người Tạo" },
                { "UpdatedDate", "Ngày Cập Nhập" },
                { "UpdatedBy", "Người Cập Nhập" },
                { "Terms", "Điều Khoản" },
                { "Notes", "Ghi Chú" },
                { "DepositReturnDate", "Ngày Hoàn Cọc" },
                { "ContractPdfPath", "Đường Dẫn PDF" },
                // English names with spaces
                { "Contract Id", "ID" },
                { "Tenant Id", "Khách" },
                { "Contract Number", "Số Hợp Đồng" },
                { "Contract Type", "Loại Hợp Đồng" },
                { "Room Id", "Phòng ID" },
                { "Room Number", "Số Phòng" },
                { "Branch Id", "Chi Nhánh" },
                { "Sign Date", "Ngày Ký" },
                { "Start Date", "Ngày Bắt Đầu" },
                { "End Date", "Ngày Kết Thúc" },
                { "Rent Amount", "Tiền Thuê/Tháng" },
                { "Rental Price", "Giá Thuê" },
                { "Deposit Amount", "Tiền Cọc" },
                { "Deposit Required", "Cọc Yêu Cầu" },
                { "Rental Period", "Thời Hạn (Tháng)" },
                { "Created Date", "Ngày Tạo" },
                { "Created By", "Người Tạo" },
                { "Updated Date", "Ngày Cập Nhập" },
                { "Updated By", "Người Cập Nhập" },
                { "Deposit Return Date", "Ngày Hoàn Cọc" },
                { "Contract Pdf Path", "Đường Dẫn PDF" }
            };

            foreach (DataGridViewColumn col in grid.Columns)
            {
                // Try to find translation by column name first
                if (columnMap.TryGetValue(col.Name, out var translatedName))
                {
                    col.HeaderText = translatedName;
                }
                // Try by current header text
                else if (columnMap.TryGetValue(col.HeaderText, out var translatedName2))
                {
                    col.HeaderText = translatedName2;
                }
                // Fallback: convert camelCase to readable Vietnamese format
                else
                {
                    col.HeaderText = ConvertColumnNameToVietnamese(col.Name);
                }
            }
            
            // Auto-size columns with reasonable minimum width
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (col.Width < 90)
                    col.Width = 90;
            }
        }

        private void FormatHistoryGrid(DataGridView grid)
        {
            if (grid == null || grid.Columns.Count == 0) return;
            
            // Comprehensive translation dictionary - all possible column names
            var columnMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // CamelCase names
                { "HistoryId", "ID" },
                { "TenantId", "Khách" },
                { "RoomId", "Phòng ID" },
                { "RoomNumber", "Số Phòng" },
                { "BranchId", "Chi Nhánh" },
                { "CheckInDate", "Ngày Nhận Phòng" },
                { "CheckOutDate", "Ngày Trả Phòng" },
                { "Duration", "Thời Gian Ở (Tháng)" },
                { "RentAmount", "Tiền Thuê/Tháng" },
                { "RentalPrice", "Giá Thuê" },
                { "DepositAmount", "Tiền Cọc" },
                { "DepositRequired", "Cọc Yêu Cầu" },
                { "StartDate", "Ngày Bắt Đầu" },
                { "EndDate", "Ngày Kết Thúc" },
                { "Status", "Trạng Thái" },
                { "CreatedDate", "Ngày Tạo" },
                { "CreatedBy", "Người Tạo" },
                { "UpdatedDate", "Ngày Cập Nhập" },
                { "UpdatedBy", "Người Cập Nhập" },
                { "Terms", "Điều Khoản" },
                { "Notes", "Ghi Chú" },
                // English names with spaces
                { "History Id", "ID" },
                { "Tenant Id", "Khách" },
                { "Room Id", "Phòng ID" },
                { "Room Number", "Số Phòng" },
                { "Branch Id", "Chi Nhánh" },
                { "Check In Date", "Ngày Nhận Phòng" },
                { "Check Out Date", "Ngày Trả Phòng" },
                { "Rent Amount", "Tiền Thuê/Tháng" },
                { "Rental Price", "Giá Thuê" },
                { "Deposit Amount", "Tiền Cọc" },
                { "Deposit Required", "Cọc Yêu Cầu" },
                { "Start Date", "Ngày Bắt Đầu" },
                { "End Date", "Ngày Kết Thúc" },
                { "Created Date", "Ngày Tạo" },
                { "Created By", "Người Tạo" },
                { "Updated Date", "Ngày Cập Nhập" },
                { "Updated By", "Người Cập Nhập" }
            };

            foreach (DataGridViewColumn col in grid.Columns)
            {
                // Try to find translation by column name first
                if (columnMap.TryGetValue(col.Name, out var translatedName))
                {
                    col.HeaderText = translatedName;
                }
                // Try by current header text
                else if (columnMap.TryGetValue(col.HeaderText, out var translatedName2))
                {
                    col.HeaderText = translatedName2;
                }
                // Fallback: convert camelCase to readable Vietnamese format
                else
                {
                    col.HeaderText = ConvertColumnNameToVietnamese(col.Name);
                }
            }
            
            // Auto-size columns with reasonable minimum width
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (col.Width < 90)
                    col.Width = 90;
            }
        }

        private string ConvertColumnNameToVietnamese(string columnName)
        {
            // Convert camelCase column names to readable format
            if (string.IsNullOrEmpty(columnName)) return columnName;
            
            var result = new StringBuilder();
            foreach (char c in columnName)
            {
                if (char.IsUpper(c) && result.Length > 0)
                    result.Append(" ");
                result.Append(c);
            }
            return result.ToString();
        }
    }
}
