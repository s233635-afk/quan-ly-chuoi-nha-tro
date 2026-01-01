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
    /// FrmRoomManagerModern - Quản lý phòng hiện đại
    /// UI đẹp, tính năng đầy đủ: lọc nâng cao, thống kê, bulk operations
    /// </summary>
    public class FrmRoomManagerModern : Form
    {
        private AdminDataBLL _bll;
        private DataTable _rawTable;

        private Panel _headerPanel;
        private Panel _toolbarPanel;
        private Panel _filterPanel;
        private Panel _statsPanel;
        private ModernDataGridView _grid;

        private TextBox _txtSearch;
        private ComboBox _cboBranch;
        private ComboBox _cboStatus;
        private ComboBox _cboType;
        private ComboBox _cboFloor;

        private Label _lblTotalRooms;
        private Label _lblOccupied;
        private Label _lblAvailable;
        private Label _lblMaintenance;

        private ModernButton _btnAdd;
        private ModernButton _btnEdit;
        private ModernButton _btnDelete;
        private ModernButton _btnRefresh;
        private ModernButton _btnChangeStatus;
        private ModernButton _btnViewTenant;

        public FrmRoomManagerModern(AdminDataBLL bll)
        {
            _bll = bll;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý phòng";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1600;
            Height = 900;
            BackColor = ModernTheme.Colors.Background;
            Font = ModernTheme.Fonts.NormalFont;

            // ============ HEADER PANEL ============
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = ModernTheme.Colors.Primary,
                Padding = new Padding(ModernTheme.Spacing.LG)
            };

            var headerTitle = new Label
            {
                Text = "🏠 QUẢN LÝ PHÒNG TRỌ",
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Heading2),
                ForeColor = ModernTheme.Colors.TextInverse,
                AutoSize = true,
                Dock = DockStyle.Left
            };

            var headerDesc = new Label
            {
                Text = "Quản lý tất cả phòng trong hệ thống, cập nhật trạng thái, khách thuê và thông tin chi tiết",
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

            _btnAdd = CreateToolButton("➕ Thêm phòng", ModernTheme.Colors.Success);
            _btnAdd.Click += async (s, e) => await AddNewRoomAsync();

            _btnEdit = CreateToolButton("✏️ Sửa", ModernTheme.Colors.Primary);
            _btnEdit.Click += async (s, e) => await EditSelectedRoomAsync();

            _btnDelete = CreateToolButton("🗑️ Xóa", ModernTheme.Colors.Error);
            _btnDelete.Click += async (s, e) => await DeleteSelectedRoomAsync();

            _btnRefresh = CreateToolButton("🔄 Tải lại", ModernTheme.Colors.Secondary);
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();

            _btnChangeStatus = CreateToolButton("📌 Đổi trạng thái", Color.FromArgb(255, 152, 0));
            _btnChangeStatus.Click += async (s, e) => await ChangeRoomStatusAsync();

            _btnViewTenant = CreateToolButton("👥 Xem khách", Color.FromArgb(33, 150, 243));
            _btnViewTenant.Click += (s, e) => ViewRoomTenant();

            var toolActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ModernTheme.Colors.Background
            };

            toolActions.Controls.Add(_btnAdd);
            toolActions.Controls.Add(_btnEdit);
            toolActions.Controls.Add(_btnDelete);
            toolActions.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            toolActions.Controls.Add(_btnRefresh);
            toolActions.Controls.Add(_btnChangeStatus);
            toolActions.Controls.Add(_btnViewTenant);

            _toolbarPanel.Controls.Add(toolActions);

            // ============ FILTER PANEL ============
            _filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
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
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Trống", "Có khách", "Bảo trì", "Tạm khóa" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            var lblType = new Label { Text = "Loại phòng:", AutoSize = true, ForeColor = ModernTheme.Colors.TextPrimary };
            _cboType = new ComboBox { Width = 150, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_cboType);
            _cboType.Items.Add("Tất cả");
            _cboType.SelectedIndex = 0;
            _cboType.SelectedIndexChanged += (s, e) => ApplyFilter();

            var lblFloor = new Label { Text = "Tầng:", AutoSize = true, ForeColor = ModernTheme.Colors.TextPrimary };
            _cboFloor = new ComboBox { Width = 100, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_cboFloor);
            _cboFloor.Items.Add("Tất cả");
            _cboFloor.SelectedIndex = 0;
            _cboFloor.SelectedIndexChanged += (s, e) => ApplyFilter();

            var filterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ModernTheme.Colors.Surface
            };

            filterFlow.Controls.Add(lblSearch);
            filterFlow.Controls.Add(_txtSearch);
            filterFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.MD, AutoSize = false });
            filterFlow.Controls.Add(lblBranch);
            filterFlow.Controls.Add(_cboBranch);
            filterFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.MD, AutoSize = false });
            filterFlow.Controls.Add(lblStatus);
            filterFlow.Controls.Add(_cboStatus);
            filterFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.MD, AutoSize = false });
            filterFlow.Controls.Add(lblType);
            filterFlow.Controls.Add(_cboType);
            filterFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.MD, AutoSize = false });
            filterFlow.Controls.Add(lblFloor);
            filterFlow.Controls.Add(_cboFloor);

            _filterPanel.Controls.Add(filterFlow);

            // ============ STATS PANEL ============
            _statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ModernTheme.Colors.InfoLight,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            var statFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ModernTheme.Colors.InfoLight
            };

            _lblTotalRooms = CreateStatLabel("📊 Tổng phòng: 0", ModernTheme.Colors.Primary);
            _lblOccupied = CreateStatLabel("👥 Có khách: 0", ModernTheme.Colors.Success);
            _lblAvailable = CreateStatLabel("🟢 Trống: 0", Color.FromArgb(76, 175, 80));
            _lblMaintenance = CreateStatLabel("🔧 Bảo trì: 0", ModernTheme.Colors.Warning);

            statFlow.Controls.Add(_lblTotalRooms);
            statFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            statFlow.Controls.Add(_lblOccupied);
            statFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            statFlow.Controls.Add(_lblAvailable);
            statFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            statFlow.Controls.Add(_lblMaintenance);

            _statsPanel.Controls.Add(statFlow);

            // ============ GRID ============
            _grid = new ModernDataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = ModernTheme.Colors.Background,
                BorderStyle = BorderStyle.None
            };
            _grid.DoubleClick += async (s, e) => await EditSelectedRoomAsync();

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(ModernTheme.Spacing.MD), BackColor = BackColor };
            gridHost.Controls.Add(_grid);

            // ============ ASSEMBLE FORM ============
            Controls.Add(gridHost);
            Controls.Add(_statsPanel);
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
                Width = 120,
                Height = 36,
                Margin = new Padding(ModernTheme.Spacing.SM)
            };
            ModernTheme.StyleButton(btn, bgColor);
            return btn;
        }

        private Label CreateStatLabel(string text, Color color)
        {
            return new Label
            {
                Text = text,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal),
                ForeColor = color,
                AutoSize = true
            };
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var rooms = await _bll.GetRoomsAsync();
                _rawTable = NormalizeRooms(rooms);
                BindDataToGrid();
                UpdateStats();
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi tải dữ liệu: " + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private DataTable NormalizeRooms(DataTable rooms)
        {
            if (rooms == null) return new DataTable();
            var clone = rooms.Copy();
            TextFixer.ForceFixDataTable(clone, "RoomNumber", "TypeName", "RoomTypeName", "StatusName", "BranchName", "SectionName");
            RoomTypeCatalog.CanonicalizeRoomTypeColumn(clone, "TypeName");
            RoomTypeCatalog.CanonicalizeRoomTypeColumn(clone, "RoomTypeName");
            if (!clone.Columns.Contains("TypeDisplay"))
                clone.Columns.Add("TypeDisplay", typeof(string));
            foreach (DataRow row in clone.Rows)
            {
                string raw = row["TypeName"]?.ToString() ?? row["RoomTypeName"]?.ToString();
                row["TypeDisplay"] = RoomTypeCatalog.Canonicalize(raw);
            }
            return clone;
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

                var types = _rawTable?.DefaultView?.ToTable(true, new[] { "RoomTypeName" })?.Rows;
                if (types != null && types.Count > 0)
                {
                    _cboType.Items.Clear();
                    _cboType.Items.Add("Tất cả");
                    foreach (DataRow row in types)
                    {
                        _cboType.Items.Add(row["RoomTypeName"]);
                    }
                    _cboType.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void ConfigureGridColumns()
        {
            if (_grid.Columns.Count == 0) return;

            var columnsToHide = new[] { "BranchId", "RoomTypeId", "Notes", "IsActive" };
            var columnWidths = new Dictionary<string, int>
            {
                { "RoomId", 60 },
                { "RoomNumber", 80 },
                { "BranchName", 120 },
                { "RoomTypeName", 100 },
                { "Floor", 60 },
                { "Area", 70 },
                { "Capacity", 80 },
                { "RentPrice", 100 },
                { "Status", 100 },
                { "TenantName", 130 }
            };

            foreach (DataGridViewColumn col in _grid.Columns)
            {
                if (columnsToHide.Contains(col.Name))
                    col.Visible = false;

                if (columnWidths.ContainsKey(col.Name))
                    col.Width = columnWidths[col.Name];
            }
        }

        private void ApplyFilter()
        {
            if (_rawTable == null || _rawTable.Rows.Count == 0)
            {
                _grid.DataSource = _rawTable;
                return;
            }

            var filtered = _rawTable.Copy();
            var query = filtered.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_txtSearch.Text))
            {
                string searchTerm = _txtSearch.Text.ToLower();
                query = query.Where(r =>
                    r["RoomNumber"]?.ToString()?.ToLower()?.Contains(searchTerm) == true ||
                    r["BranchName"]?.ToString()?.ToLower()?.Contains(searchTerm) == true ||
                    r["TenantName"]?.ToString()?.ToLower()?.Contains(searchTerm) == true
                );
            }

            if (_cboBranch.SelectedIndex > 0)
            {
                string branch = _cboBranch.SelectedItem.ToString();
                query = query.Where(r => r["BranchName"]?.ToString() == branch);
            }

            if (_cboStatus.SelectedIndex > 0)
            {
                string status = _cboStatus.SelectedItem.ToString();
                query = query.Where(r => r["Status"]?.ToString() == status);
            }

            if (_cboType.SelectedIndex > 0)
            {
                string type = _cboType.SelectedItem.ToString();
                query = query.Where(r => r["RoomTypeName"]?.ToString() == type);
            }

            _grid.DataSource = query.CopyToDataTable();
            UpdateStats();
        }

        private void UpdateStats()
        {
            try
            {
                var dt = _grid.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    _lblTotalRooms.Text = "📊 Tổng phòng: 0";
                    _lblOccupied.Text = "👥 Có khách: 0";
                    _lblAvailable.Text = "🟢 Trống: 0";
                    _lblMaintenance.Text = "🔧 Bảo trì: 0";
                    return;
                }

                int total = dt.Rows.Count;
                int occupied = dt.AsEnumerable().Count(r => r["Status"]?.ToString() == "Có khách");
                int available = dt.AsEnumerable().Count(r => r["Status"]?.ToString() == "Trống");
                int maintenance = dt.AsEnumerable().Count(r => r["Status"]?.ToString() == "Bảo trì");

                _lblTotalRooms.Text = $"📊 Tổng phòng: {total}";
                _lblOccupied.Text = $"👥 Có khách: {occupied}";
                _lblAvailable.Text = $"🟢 Trống: {available}";
                _lblMaintenance.Text = $"🔧 Bảo trì: {maintenance}";
            }
            catch { }
        }

        private async Task AddNewRoomAsync()
        {
            using (var form = new FrmRoomEditor(_bll, null))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
        }

        private async Task EditSelectedRoomAsync()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                ToastNotification.Warning("Vui lòng chọn một phòng để sửa");
                return;
            }

            int roomId = Convert.ToInt32(_grid.SelectedRows[0].Cells["RoomId"].Value);
            using (var form = new FrmRoomEditor(_bll, roomId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
        }

        private async Task DeleteSelectedRoomAsync()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                ToastNotification.Warning("Vui lòng chọn một phòng để xóa");
                return;
            }

            if (!ModernConfirmDialog.ConfirmDanger("Bạn chắc chắn muốn xóa phòng này?"))
                return;

            int roomId = Convert.ToInt32(_grid.SelectedRows[0].Cells["RoomId"].Value);
            try
            {
                await _bll.DeleteRoomAsync(roomId);
                ToastNotification.Success("Xóa thành công!");
                AdminEvents.NotifyDataChanged();
                DataSyncManager.NotifyRoomsChanged();
                DataSyncManager.NotifyTenantsChanged();
                DataSyncManager.NotifyContractsChanged();
                DataSyncManager.NotifyInvoicesChanged();
                DataSyncManager.NotifyPaymentsChanged();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ModernDialog.Error($"Lỗi xóa: {ex.Message}");
            }
        }

        private async Task ChangeRoomStatusAsync()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Implementation for status change dialog
            ToastNotification.Info("Tính năng thay đổi trạng thái sắp được bổ sung");
        }

        private void ViewRoomTenant()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var tenantName = _grid.SelectedRows[0].Cells["TenantName"].Value?.ToString();
            ToastNotification.Info($"Khách thuê: {tenantName ?? "Chưa có khách"}");
        }
    }
}
