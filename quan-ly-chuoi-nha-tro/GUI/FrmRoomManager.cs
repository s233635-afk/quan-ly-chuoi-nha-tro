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
    /// Màn hình quản lý phòng cho nhân viên (xem, lọc, đổi trạng thái, xem chi tiết).
    /// </summary>
    public class FrmRoomManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo số phòng/loại...";
        private const int MaxDisplayedRooms = 20;

        private readonly AdminDataBLL _bll;
        private readonly int? _branchId;

        private DataTable _rooms;
        private DataTable _statuses;
        private DataTable _roomTypes;
        private DataTable _tenantHistory;
        private DataTable _contracts;
        private DataTable _tenants;

        private SplitContainer _splitContainer;
        private FlowLayoutPanel _roomCardsHost;
        private Panel _roomDetailPanel;
        private ComboBox _cboStatus;
        private TextBox _txtSearch;
        private Label _lblSummary;
        private Label _lblRoomCount;

        private Button _btnSearch;
        private Button _btnRefresh;
        private Button _btnAddRoom;
        private Button _btnChangeStatus;
        private Button _btnEdit;

        private int _selectedRoomId = 0;
        private DataRow _selectedRoomRow = null;

        public FrmRoomManager(AdminDataBLL bll, int? branchId = null)
        {
            _bll = bll ?? new AdminDataBLL();
            _branchId = branchId;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        public FrmRoomManager() : this(new AdminDataBLL(), null)
        {
        }

        private void InitializeComponent()
        {
            Text = "Quản lý phòng";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Width = 1400;
            Height = 750;

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White
            };

            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            var lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, Margin = new Padding(0, 8, 6, 0) };
            _txtSearch = new TextBox { Width = 240, ForeColor = Color.Gray, Text = SearchPlaceholder, Margin = new Padding(0, 4, 12, 0) };
            _txtSearch.GotFocus += (s, e) =>
            {
                if (_txtSearch.Text == SearchPlaceholder)
                {
                    _txtSearch.Text = string.Empty;
                    _txtSearch.ForeColor = Color.Black;
                }
            };
            _txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _txtSearch.Text = SearchPlaceholder;
                    _txtSearch.ForeColor = Color.Gray;
                }
            };
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) ApplyFilter(); };

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, Margin = new Padding(0, 8, 6, 0) };
            _cboStatus = new ComboBox { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 12, 0) };
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _btnSearch = new Button
            {
                Text = "Tìm",
                Width = 80,
                Height = 32,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 8, 0)
            };
            _btnSearch.FlatAppearance.BorderSize = 0;
            _btnSearch.Click += (s, e) => ApplyFilter();

            _btnRefresh = new Button
            {
                Text = "Làm mới",
                Width = 90,
                Height = 32,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 8, 0)
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += async (s, e) => await LoadRoomsAsync();

            _btnAddRoom = new Button
            {
                Text = "Thêm phòng",
                Width = 110,
                Height = 32,
                BackColor = Color.FromArgb(23, 162, 184),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 8, 0)
            };
            _btnAddRoom.FlatAppearance.BorderSize = 0;
            _btnAddRoom.Click += async (s, e) => await AddRoomAsync();

            _btnChangeStatus = new Button
            {
                Text = "Đổi trạng thái",
                Width = 120,
                Height = 32,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 8, 0),
                Visible = false
            };
            _btnChangeStatus.FlatAppearance.BorderSize = 0;
            _btnChangeStatus.Click += (s, e) => ChangeRoomStatus();

            _btnEdit = new Button
            {
                Text = "Chỉnh sửa",
                Width = 90,
                Height = 32,
                BackColor = Color.FromArgb(111, 66, 193),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 8, 0),
                Visible = false
            };
            _btnEdit.FlatAppearance.BorderSize = 0;
            _btnEdit.Click += (s, e) => EditCurrentRoom();

            filters.Controls.Add(lblSearch);
            filters.Controls.Add(_txtSearch);
            filters.Controls.Add(lblStatus);
            filters.Controls.Add(_cboStatus);
            filters.Controls.Add(_btnSearch);
            filters.Controls.Add(_btnRefresh);
            filters.Controls.Add(_btnAddRoom);
            filters.Controls.Add(_btnChangeStatus);

            var stats = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 6, 0, 0)
            };
            _lblSummary = new Label
            {
                Text = $"Đang ở/Tổng: 0/{MaxDisplayedRooms} phòng",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            _lblRoomCount = new Label
            {
                Text = $"Phòng: 0/{MaxDisplayedRooms}",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Margin = new Padding(12, 0, 0, 0)
            };
            stats.Controls.Add(_lblSummary);
            stats.Controls.Add(_lblRoomCount);

            toolbar.Controls.Add(stats);
            toolbar.Controls.Add(filters);

            _splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 1000,
                BackColor = Color.White,
                IsSplitterFixed = false
            };

            _roomCardsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(16)
            };

            _roomDetailPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14),
                AutoScroll = true
            };

            _splitContainer.Panel1.Controls.Add(_roomCardsHost);
            _splitContainer.Panel2.Controls.Add(_roomDetailPanel);
            _splitContainer.Panel2Collapsed = true;

            Controls.Add(_splitContainer);
            Controls.Add(toolbar);

            Load += async (s, e) => await LoadRoomsAsync();
        }

        private async Task LoadRoomsAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _selectedRoomId = 0;
                _selectedRoomRow = null;
                _btnEdit.Visible = false;
                if (_splitContainer != null)
                {
                    _splitContainer.Panel2Collapsed = true;
                }

                _rooms = await _bll.GetRoomsAsync() ?? new DataTable();
                _statuses = await _bll.GetRoomStatusesAsync() ?? new DataTable();
                _roomTypes = await _bll.GetRoomTypesAsync() ?? new DataTable();
                _tenantHistory = await _bll.GetTenantHistoryAsync() ?? new DataTable();
                _contracts = await _bll.GetContractsAsync() ?? new DataTable();
                _tenants = await _bll.GetTenantsAsync() ?? new DataTable();

                if (_branchId.HasValue && _rooms.Columns.Contains("BranchId"))
                {
                    var filtered = _rooms.AsEnumerable()
                        .Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == _branchId.Value);
                    _rooms = filtered.Any() ? filtered.CopyToDataTable() : _rooms.Clone();
                }

                EnsureDisplayColumns();
                PopulateOccupancy();
                PopulateStatusFilter();

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải phòng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void EnsureDisplayColumns()
        {
            if (_rooms == null) return;

            if (!_rooms.Columns.Contains("TypeName"))
                _rooms.Columns.Add("TypeName", typeof(string));
            if (!_rooms.Columns.Contains("StatusName"))
                _rooms.Columns.Add("StatusName", typeof(string));
            if (!_rooms.Columns.Contains("Occupants"))
                _rooms.Columns.Add("Occupants", typeof(int));

            var typeLookup = _roomTypes?.AsEnumerable()
                .Where(r => _roomTypes.Columns.Contains("RoomTypeId"))
                .ToDictionary(r => r["RoomTypeId"], r => SafeToString(r, "RoomTypeName"));

            var statusLookup = _statuses?.AsEnumerable()
                .Where(r => _statuses.Columns.Contains("StatusId"))
                .ToDictionary(r => r["StatusId"], r => SafeToString(r, "StatusName"));

            foreach (DataRow row in _rooms.Rows)
            {
                if (typeLookup != null && _rooms.Columns.Contains("RoomTypeId") && typeLookup.TryGetValue(row["RoomTypeId"], out var typeName))
                    row["TypeName"] = RoomTypeCatalog.Canonicalize(typeName);
                if (statusLookup != null && _rooms.Columns.Contains("CurrentStatusId") && statusLookup.TryGetValue(row["CurrentStatusId"], out var statusName))
                    row["StatusName"] = statusName;
            }
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadRoomsAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private void PopulateOccupancy()
        {
            if (_rooms == null) return;

            var activeByRoom = new Dictionary<int, int>();
            if (_tenantHistory != null && _tenantHistory.Rows.Count > 0 && _tenantHistory.Columns.Contains("RoomId"))
            {
                foreach (DataRow r in _tenantHistory.Rows)
                {
                    if (!int.TryParse(r["RoomId"]?.ToString(), out var rid)) continue;

                    // xem là active nếu chưa checkout hoặc status chứa "Active" (không phân biệt hoa thường)
                    bool isActive = string.IsNullOrWhiteSpace(r.Table.Columns.Contains("CheckOutDate") ? r["CheckOutDate"]?.ToString() : null);
                    if (!isActive && r.Table.Columns.Contains("Status"))
                    {
                        var statusText = r["Status"]?.ToString() ?? string.Empty;
                        isActive = statusText.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0
                                   || statusText.IndexOf("đang", StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    if (!isActive) continue;
                    activeByRoom[rid] = activeByRoom.TryGetValue(rid, out var count) ? count + 1 : 1;
                }
            }

            if (activeByRoom.Count == 0) return;

            foreach (DataRow row in _rooms.Rows)
            {
                if (!int.TryParse(row["RoomId"]?.ToString(), out var rid)) continue;
                var current = 0;
                if (row.Table.Columns.Contains("Occupants") && int.TryParse(row["Occupants"]?.ToString(), out var stored))
                    current = stored;
                row["Occupants"] = activeByRoom.TryGetValue(rid, out var count) ? count : current;
            }
        }

        private void PopulateStatusFilter()
        {
            var dt = new DataTable();
            dt.Columns.Add("StatusId", typeof(int));
            dt.Columns.Add("StatusName", typeof(string));
            dt.Rows.Add(0, "Tất cả");

            if (_statuses != null && _statuses.Columns.Contains("StatusId"))
            {
                foreach (DataRow r in _statuses.Rows)
                {
                    if (!int.TryParse(r["StatusId"]?.ToString(), out var id)) continue;
                    dt.Rows.Add(id, SafeToString(r, "StatusName"));
                }
            }

            _cboStatus.DataSource = dt;
            _cboStatus.DisplayMember = "StatusName";
            _cboStatus.ValueMember = "StatusId";
            _cboStatus.SelectedIndex = 0;
        }

        private void ApplyFilter()
        {
            if (_rooms == null) return;

            var view = new DataView(_rooms);
            var filters = new List<string>();
            bool hasStatusColumn = _rooms.Columns.Contains("CurrentStatusId");

            var raw = (_txtSearch.Text ?? string.Empty).Trim();
            if (raw == SearchPlaceholder) raw = string.Empty;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var escaped = raw.Replace("'", "''");
                filters.Add($"Convert(RoomNumber, 'System.String') LIKE '%{escaped}%' OR Convert(TypeName, 'System.String') LIKE '%{escaped}%'");
            }

            if (hasStatusColumn && _cboStatus.SelectedValue is int statusId && statusId > 0)
                filters.Add($"Convert(CurrentStatusId, 'System.Int32') = {statusId}");

            view.RowFilter = filters.Count > 0 ? string.Join(" AND ", filters) : string.Empty;

            var filteredTable = view.ToTable();
            var displayedRows = filteredTable.AsEnumerable()
                .OrderBy(r => SafeToString(r, "RoomNumber"))
                .Take(MaxDisplayedRooms)
                .ToList();

            RenderRoomCards(displayedRows);
            UpdateStats(displayedRows);
        }

        private void RenderRoomCards(IEnumerable<DataRow> displayedRows)
        {
            _roomCardsHost.SuspendLayout();
            _roomCardsHost.Controls.Clear();

            var roomsToRender = displayedRows?.ToList() ?? new List<DataRow>();

            if (roomsToRender.Count == 0)
            {
                _roomCardsHost.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = "Không có phòng nào.",
                    ForeColor = Color.FromArgb(90, 90, 90)
                });
                _roomCardsHost.ResumeLayout();
                _lblRoomCount.Text = $"Phòng: 0/{MaxDisplayedRooms}";
                return;
            }

            foreach (DataRow row in roomsToRender)
            {
                int roomId = TryGetInt(row, "RoomId");
                if (roomId <= 0) continue;

                var card = CreateRoomCard(row, roomId);
                _roomCardsHost.Controls.Add(card);
            }

            _roomCardsHost.ResumeLayout();
            _lblRoomCount.Text = $"Phòng: {roomsToRender.Count}/{MaxDisplayedRooms}";
        }

        private Panel CreateRoomCard(DataRow row, int roomId)
        {
            string roomNumber = SafeToString(row, "RoomNumber") ?? "—";
            string typeName = SafeToString(row, "TypeName") ?? "—";
            string statusName = SafeToString(row, "StatusName") ?? "—";
            var statusColor = GetStatusColor(statusName);
            var hoverColor = ControlPaint.Light(statusColor, 0.1f);
            decimal price = TryGetDecimal(row, "RoomPrice") ?? 0m;
            int occupants = TryGetInt(row, "Occupants");
            bool isSelected = _selectedRoomId == roomId;
            var selectedColor = ControlPaint.Dark(statusColor, 0.05f);

            var card = new Panel
            {
                Width = 300,
                Height = 180,
                BackColor = isSelected ? selectedColor : statusColor,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10, 10, 10, 10),
                Cursor = Cursors.Hand,
                Tag = roomId
            };

            card.Paint += (s, e) =>
            {
                var borderColor = isSelected ? Color.FromArgb(0, 95, 180) : Color.FromArgb(70, 160, 230);
                var borderWidth = isSelected ? 3 : 2;
                using (var pen = new Pen(borderColor, borderWidth))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
                using (var shadowPen = new Pen(Color.FromArgb(200, 220, 245), 1))
                {
                    e.Graphics.DrawRectangle(shadowPen, 1, 1, card.Width - 3, card.Height - 3);
                }
            };

            card.MouseEnter += (s, e) =>
            {
                if (!isSelected) card.BackColor = hoverColor;
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = isSelected ? selectedColor : statusColor;
            };

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

            // Header: Phòng + icon
            var lblRoom = new Label
            {
                Text = $"Phòng {roomNumber}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Fill,
                Height = 28,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 0, 0)
            };

            var lblTypeIcon = new Label
            {
                Text = GetOccupancyIcon(occupants, typeName),
                Font = new Font("Segoe UI Symbol", 16, FontStyle.Regular),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Right,
                Width = 38,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0)
            };

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32
            };
            headerPanel.Controls.Add(lblRoom);
            headerPanel.Controls.Add(lblTypeIcon);

            var lblPrice = new Label
            {
                Text = $"{price:N0}đ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
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
                Text = $"Loại: {GetTypeIcon(typeName)} {typeName}",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
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
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
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
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
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

            mainLayout.Controls.Add(headerPanel, 0, 0);
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
            _selectedRoomRow = row;

            try
            {
                var frmType = Type.GetType("quan_ly_chuoi_nha_tro.GUI.FrmRoomDetailForm");
                if (frmType != null)
                {
                    var frm = (Form)Activator.CreateInstance(frmType, _bll, row, _contracts, _tenantHistory);
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        _ = LoadRoomsAsync();
                        var syncType = Type.GetType("quan_ly_chuoi_nha_tro.GUI.DataSyncManager");
                        if (syncType != null)
                        {
                            var method = syncType.GetMethod("NotifyRoomsChanged");
                            method?.Invoke(null, null);
                        }
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

        private void ShowRoomDetails(DataRow row, int roomId)
        {
            ShowRoomDetailsForm(row, roomId);
        }

        private async Task AddRoomAsync()
        {
            using (var frm = new FrmRoomEditor(_bll))
            {
                if (frm.ShowDialog(this) != DialogResult.OK) return;
                await LoadRoomsAsync();
                AdminEvents.NotifyDataChanged();
                var syncType = Type.GetType("quan_ly_chuoi_nha_tro.GUI.DataSyncManager");
                if (syncType != null)
                {
                    var method = syncType.GetMethod("NotifyRoomsChanged");
                    method?.Invoke(null, null);
                }
            }
        }

        private void BuildRoomDetailsPanel()
        {
            _roomDetailPanel.Controls.Clear();

            if (_selectedRoomRow == null)
            {
                var header = BuildDetailHeader("Thông tin phòng");
                var emptyLabel = new Label
                {
                    Text = "Chọn một phòng để xem chi tiết",
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Dock = DockStyle.Top,
                    Height = 30
                };

                _roomDetailPanel.Controls.Add(emptyLabel);
                _roomDetailPanel.Controls.Add(header);
                return;
            }

            var detailHeader = BuildDetailHeader($"Phòng {SafeToString(_selectedRoomRow, "RoomNumber") ?? "—"}");

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddDetailRow(layout, "Số phòng:", SafeToString(_selectedRoomRow, "RoomNumber"));
            AddDetailRow(layout, "Loại phòng:", SafeToString(_selectedRoomRow, "TypeName"));
            AddDetailRow(layout, "Trạng thái:", SafeToString(_selectedRoomRow, "StatusName"));
            AddDetailRow(layout, "Giá/Tháng:", (TryGetDecimal(_selectedRoomRow, "RoomPrice") ?? 0m).ToString("N0"));
            AddDetailRow(layout, "Diện tích:", (TryGetDecimal(_selectedRoomRow, "Area") ?? 0m).ToString("0.##") + " m²");
            AddDetailRow(layout, "Số người:", TryGetInt(_selectedRoomRow, "Occupants").ToString());

            var editButton = new Button
            {
                Text = "Chỉnh sửa",
                AutoSize = true,
                BackColor = Color.FromArgb(111, 66, 193),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(12, 6, 12, 6),
                Margin = new Padding(0, 12, 0, 0)
            };
            editButton.FlatAppearance.BorderSize = 0;
            editButton.Click += (s, e) => EditCurrentRoom();

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            buttonPanel.Controls.Add(editButton);

            var tenantsPanel = BuildTenantListPanel();

            _roomDetailPanel.Controls.Add(buttonPanel);
            _roomDetailPanel.Controls.Add(tenantsPanel);
            _roomDetailPanel.Controls.Add(layout);
            _roomDetailPanel.Controls.Add(detailHeader);
        }

        private Control BuildDetailHeader(string title)
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 40 };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Left,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 240
            };

            var btnClose = new Button
            {
                Text = "X",
                Width = 36,
                Height = 30,
                BackColor = Color.FromArgb(230, 64, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Margin = new Padding(0, 5, 0, 5)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) =>
            {
                _selectedRoomRow = null;
                _selectedRoomId = 0;
                _btnEdit.Visible = false;
                if (_splitContainer != null) _splitContainer.Panel2Collapsed = true;
            };

            panel.Controls.Add(btnClose);
            panel.Controls.Add(lblTitle);
            return panel;
        }

        private Panel BuildContractsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 12, 0, 0) };

            var lblTitle = new Label
            {
                Text = "Hợp đồng của phòng",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false
            };
            panel.Controls.Add(lblTitle);

            var grid = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 200,
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
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);

            if (_contracts != null && _contracts.Rows.Count > 0)
            {
                var roomContracts = _contracts.AsEnumerable()
                    .Where(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == _selectedRoomId)
                    .ToList();

                if (roomContracts.Count > 0)
                {
                    grid.DataSource = roomContracts.CopyToDataTable();
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

        private Panel BuildTenantListPanel()
        {
            var panel = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 12, 0, 0) };

            var lblTitle = new Label
            {
                Text = "Khách đang ở",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false
            };
            panel.Controls.Add(lblTitle);

            var container = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 0)
            };

            var activeTenants = new List<(DataRow tenant, DataRow history)>();
            if (_tenantHistory != null && _tenantHistory.Columns.Contains("RoomId"))
            {
                var rows = _tenantHistory.AsEnumerable()
                    .Where(r => int.TryParse(r["RoomId"]?.ToString(), out var rid) && rid == _selectedRoomId)
                    .Where(r =>
                    {
                        var statusText = r.Table.Columns.Contains("Status") ? r["Status"]?.ToString() ?? string.Empty : string.Empty;
                        var checkout = r.Table.Columns.Contains("CheckOutDate") ? r["CheckOutDate"] : null;
                        bool isOut = checkout != null && checkout != DBNull.Value;
                        if (!isOut) return true;
                        return statusText.IndexOf("đang", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               statusText.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .ToList();

                foreach (var history in rows)
                {
                    if (!int.TryParse(history["TenantId"]?.ToString(), out var tid)) continue;
                    var tenantRow = _tenants?.AsEnumerable()
                        .FirstOrDefault(t => int.TryParse(t["TenantId"]?.ToString(), out var tenantId) && tenantId == tid);
                    if (tenantRow != null)
                    {
                        activeTenants.Add((tenantRow, history));
                    }
                    else
                    {
                        // Fallback to history-only info when tenant data is missing
                        var fallbackTable = _tenantHistory?.Clone() ?? new DataTable();
                        if (!fallbackTable.Columns.Contains("TenantId"))
                            fallbackTable.Columns.Add("TenantId", typeof(int));
                        var fallback = fallbackTable.NewRow();
                        fallback["TenantId"] = tid;
                        activeTenants.Add((fallback, history));
                    }
                }
            }

            if (activeTenants.Count == 0)
            {
                container.Controls.Add(new Label
                {
                    Text = "Chưa có khách ở.",
                    AutoSize = true,
                    ForeColor = Color.FromArgb(100, 100, 100),
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    Margin = new Padding(0, 4, 0, 4)
                });
            }
            else
            {
                foreach (var (tenant, history) in activeTenants)
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

                    var inner = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        AutoSize = true,
                        FlowDirection = FlowDirection.TopDown,
                        WrapContents = false,
                        Padding = new Padding(0)
                    };

                    string name = SafeToString(tenant, "FullName") ?? $"Khách #{SafeToString(tenant, "TenantId")}";
                    string phone = SafeToString(tenant, "PhoneNumber") ?? "—";
                    string cccd = SafeToString(tenant, "IdentityCard") ?? "—";
                    string email = SafeToString(tenant, "Email") ?? "—";
                    string checkIn = FormatDateSafe(history, "CheckInDate");

                    var lblName = new Label
                    {
                        Text = name,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        ForeColor = Color.FromArgb(30, 55, 90),
                        Margin = new Padding(0, 0, 0, 2)
                    };
                    var lblPhone = new Label
                    {
                        Text = $"SĐT: {phone}",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.FromArgb(60, 60, 60)
                    };
                    var lblCccd = new Label
                    {
                        Text = $"CCCD: {cccd}",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.FromArgb(60, 60, 60)
                    };
                    var lblEmail = new Label
                    {
                        Text = $"Email: {email}",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.FromArgb(60, 60, 60)
                    };
                    var lblCheckIn = new Label
                    {
                        Text = $"Ngày vào: {checkIn}",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.FromArgb(60, 60, 60)
                    };

                    inner.Controls.Add(lblName);
                    inner.Controls.Add(lblPhone);
                    inner.Controls.Add(lblCccd);
                    inner.Controls.Add(lblEmail);
                    inner.Controls.Add(lblCheckIn);

                    card.Controls.Add(inner);
                    container.Controls.Add(card);
                }
            }

            panel.Controls.Add(container);
            return panel;
        }

        private void AddDetailRow(TableLayoutPanel layout, string label, string value)
        {
            var lblLabel = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 70, 70),
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 6)
            };

            var lblValue = new Label
            {
                Text = value ?? "—",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 55, 90),
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 6)
            };

            layout.Controls.Add(lblLabel);
            layout.Controls.Add(lblValue);
        }

        private void UpdateStats(IEnumerable<DataRow> rows)
        {
            var list = rows?.ToList() ?? new List<DataRow>();

            int occupied = list.Count(r => (r["StatusName"]?.ToString() ?? string.Empty)
                .IndexOf("đang", StringComparison.OrdinalIgnoreCase) >= 0);

            _lblSummary.Text = $"Đang ở/Tổng: {occupied}/{MaxDisplayedRooms} phòng";
        }

        private async void ChangeRoomStatus()
        {
            if (_selectedRoomId <= 0)
            {
                MessageBox.Show("Chọn một phòng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new Form())
            {
                dlg.Text = "Đổi trạng thái phòng";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Size = new Size(420, 180);
                dlg.BackColor = Color.White;
                dlg.Font = Font;

                var lbl = new Label { Text = "Trạng thái mới:", AutoSize = true, Location = new Point(16, 22) };
                var cbo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Width = 360,
                    Location = new Point(16, 50),
                    DataSource = _statuses?.Copy(),
                    DisplayMember = "StatusName",
                    ValueMember = "StatusId"
                };

                var btnOk = new Button
                {
                    Text = "Cập nhật",
                    DialogResult = DialogResult.OK,
                    Width = 110,
                    Height = 32,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(170, 100)
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "Hủy",
                    DialogResult = DialogResult.Cancel,
                    Width = 90,
                    Height = 32,
                    Location = new Point(290, 100)
                };

                dlg.Controls.Add(lbl);
                dlg.Controls.Add(cbo);
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog(this) == DialogResult.OK && cbo.SelectedValue != null)
                {
                    try
                    {
                        int newStatusId = Convert.ToInt32(cbo.SelectedValue);
                        await UpdateRoomStatusAsync(_selectedRoomId, newStatusId);
                        await LoadRoomsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi cập nhật: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void EditCurrentRoom()
        {
            if (_selectedRoomRow == null)
            {
                MessageBox.Show("Chọn một phòng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new RoomEditDialog(_selectedRoomRow, _roomTypes, _statuses))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    string roomNumber = SafeToString(_selectedRoomRow, "RoomNumber") ?? string.Empty;
                    int roomId = TryGetInt(_selectedRoomRow, "RoomId");
                    int branchId = TryGetInt(_selectedRoomRow, "BranchId");
                    int? sectionId = TryGetNullableInt(_selectedRoomRow, "SectionId");
                    int? roomTypeId = dlg.SelectedRoomTypeId ?? TryGetNullableInt(_selectedRoomRow, "RoomTypeId");
                    decimal? price = dlg.RoomPrice ?? TryGetDecimal(_selectedRoomRow, "RoomPrice");
                    int? statusId = dlg.SelectedStatusId ?? TryGetNullableInt(_selectedRoomRow, "CurrentStatusId");
                    int? floor = TryGetNullableInt(_selectedRoomRow, "Floor");
                    decimal? area = dlg.Area ?? TryGetDecimal(_selectedRoomRow, "Area");
                    bool? isActive = dlg.IsActive ?? TryGetBool(_selectedRoomRow, "IsActive");
                    int occupants = dlg.OccupantCount ?? TryGetInt(_selectedRoomRow, "Occupants");

                    await _bll.UpdateRoomAsync(roomId, roomNumber, branchId, sectionId, roomTypeId, price, statusId, floor, area, isActive, occupants);
                    await LoadRoomsAsync();
                    AdminEvents.NotifyDataChanged();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi cập nhật phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task UpdateRoomStatusAsync(int roomId, int newStatusId)
        {
            if (_selectedRoomRow == null) return;

            string roomNumber = SafeToString(_selectedRoomRow, "RoomNumber") ?? string.Empty;
            int branchId = TryGetInt(_selectedRoomRow, "BranchId");
            int? sectionId = TryGetNullableInt(_selectedRoomRow, "SectionId");
            int? roomTypeId = TryGetNullableInt(_selectedRoomRow, "RoomTypeId");
            decimal? price = TryGetDecimal(_selectedRoomRow, "RoomPrice");
            int? floor = TryGetNullableInt(_selectedRoomRow, "Floor");
            decimal? area = TryGetDecimal(_selectedRoomRow, "Area");
            bool? isActive = TryGetBool(_selectedRoomRow, "IsActive");

            await _bll.UpdateRoomAsync(roomId, roomNumber, branchId, sectionId, roomTypeId, price, newStatusId, floor, area, isActive, TryGetInt(_selectedRoomRow, "Occupants"));
            AdminEvents.NotifyDataChanged();
        }

        private static int TryGetInt(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return 0;
            return int.TryParse(row[column]?.ToString(), out var val) ? val : 0;
        }

        private static int? TryGetNullableInt(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return null;
            return int.TryParse(row[column]?.ToString(), out var val) ? (int?)val : null;
        }

        private static decimal? TryGetDecimal(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return null;
            return decimal.TryParse(row[column]?.ToString(), out var val) ? (decimal?)val : null;
        }

        private static bool? TryGetBool(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return null;
            return bool.TryParse(row[column]?.ToString(), out var val) ? (bool?)val : null;
        }

        private static string FormatDateSafe(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return "—";
            var val = row[column];
            if (val == null || val == DBNull.Value) return "—";
            if (val is DateTime dt) return dt.ToString("dd/MM/yyyy");
            if (DateTime.TryParse(val.ToString(), out var parsed)) return parsed.ToString("dd/MM/yyyy");
            return val.ToString();
        }

        private static Color GetStatusColor(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName)) return Color.White;

            var status = statusName.ToLowerInvariant();
            bool isDeposit = status.Contains("cọc") || status.Contains("coc") || status.Contains("đặt cọc");
            bool isMaintenance = status.Contains("bảo trì") || status.Contains("bao tri") || status.Contains("maintenance");
            bool isOccupied = status.Contains("đang ở") || status.Contains("dang o") || status.Contains("đang thuê") || status.Contains("dang thue");

            if (isMaintenance) return Color.FromArgb(255, 220, 220);
            if (isDeposit) return Color.FromArgb(255, 245, 200);
            if (isOccupied) return Color.FromArgb(220, 245, 230);
            return Color.White;
        }

        private static string GetTypeIcon(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return "🏠";

            var lower = typeName.ToLowerInvariant();
            if (lower.Contains("cao cấp") || lower.Contains("vip") || lower.Contains("premium"))
                return "⭐";
            if (lower.Contains("đôi") || lower.Contains("doi") || lower.Contains("double"))
                return "👥";
            if (lower.Contains("đơn") || lower.Contains("don") || lower.Contains("single"))
                return "👤";

            return "🏠";
        }

        private static string GetOccupancyIcon(int occupants, string typeName = null)
        {
            var lowerType = typeName?.ToLowerInvariant() ?? string.Empty;
            if (lowerType.Contains("cao cấp") || lowerType.Contains("vip") || lowerType.Contains("premium"))
                return "⭐";

            if (occupants >= 3)
                return "👤+";
            if (occupants == 2 || lowerType.Contains("đôi") || lowerType.Contains("doi") || lowerType.Contains("double"))
                return "👥";
            return "👤";
        }

        private class RoomEditDialog : Form
        {
            private readonly DataRow _row;
            private readonly DataTable _roomTypes;
            private readonly DataTable _statuses;

            private ComboBox _cboRoomType;
            private ComboBox _cboStatus;
            private Label _lblPrice;
            private TextBox _txtArea;
            private NumericUpDown _numOccupants;
            private CheckBox _chkActive;

            public int? SelectedRoomTypeId => _cboRoomType.SelectedValue is int v ? v : (int?)null;
            public int? SelectedStatusId => _cboStatus.SelectedValue is int v ? v : (int?)null;
            public decimal? RoomPrice => null;
            public int? OccupantCount => (int)_numOccupants.Value;
            public decimal? Area => decimal.TryParse(_txtArea.Text.Replace(",", ""), out var d) ? d : (decimal?)null;
            public bool? IsActive => _chkActive.Checked;

            public RoomEditDialog(DataRow row, DataTable roomTypes, DataTable statuses)
            {
                _row = row;
                _roomTypes = roomTypes;
                _statuses = statuses;

                InitializeComponent();
                LoadData();
            }

            private void InitializeComponent()
            {
                Text = "Chỉnh sửa phòng";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(420, 300);
                BackColor = Color.White;

                var lblRoom = new Label { Text = "Số phòng:", AutoSize = true, Location = new Point(18, 18) };
                var txtRoom = new TextBox
                {
                    ReadOnly = true,
                    Width = 140,
                    Location = new Point(120, 14),
                    Text = _row != null ? _row["RoomNumber"]?.ToString() ?? string.Empty : string.Empty
                };

                var lblType = new Label { Text = "Loại phòng:", AutoSize = true, Location = new Point(18, 56) };
                _cboRoomType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, Location = new Point(120, 52) };

                var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, Location = new Point(18, 94) };
                _cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, Location = new Point(120, 90) };

                var lblOccupants = new Label { Text = "Số người:", AutoSize = true, Location = new Point(18, 132) };
                _numOccupants = new NumericUpDown { Minimum = 0, Maximum = 50, Width = 120, Location = new Point(120, 128) };

                var lblPrice = new Label { Text = "Giá/Tháng:", AutoSize = true, Location = new Point(18, 166) };
                _lblPrice = new Label { AutoSize = true, Location = new Point(120, 166), ForeColor = Color.FromArgb(70, 70, 70) };

                var lblArea = new Label { Text = "Diện tích (m²):", AutoSize = true, Location = new Point(18, 202) };
                _txtArea = new TextBox { Width = 260, Location = new Point(120, 198) };

                _chkActive = new CheckBox { Text = "Kích hoạt", AutoSize = true, Location = new Point(120, 226) };

                var btnOk = new Button
                {
                    Text = "Cập nhật",
                    Width = 100,
                    Height = 30,
                    Location = new Point(200, 248),
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.OK
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "Hủy",
                    Width = 80,
                    Height = 30,
                    Location = new Point(310, 248),
                    DialogResult = DialogResult.Cancel
                };

                Controls.Add(lblRoom);
                Controls.Add(txtRoom);
                Controls.Add(lblType);
                Controls.Add(_cboRoomType);
                Controls.Add(lblStatus);
                Controls.Add(_cboStatus);
                Controls.Add(lblOccupants);
                Controls.Add(_numOccupants);
                Controls.Add(lblPrice);
                Controls.Add(_lblPrice);
                Controls.Add(lblArea);
                Controls.Add(_txtArea);
                Controls.Add(_chkActive);
                Controls.Add(btnOk);
                Controls.Add(btnCancel);

                AcceptButton = btnOk;
                CancelButton = btnCancel;
            }

            private void LoadData()
            {
                var roomTypeSrc = new DataTable();
                roomTypeSrc.Columns.Add("RoomTypeId", typeof(int));
                roomTypeSrc.Columns.Add("RoomTypeName", typeof(string));
                var filteredTypes = RoomTypeCatalog.FilterToCanonicalTypes(_roomTypes);
                if (filteredTypes != null && filteredTypes.Columns.Contains("RoomTypeId"))
                {
                    foreach (DataRow r in filteredTypes.Rows)
                    {
                        if (!int.TryParse(r["RoomTypeId"]?.ToString(), out var id)) continue;
                        roomTypeSrc.Rows.Add(id, r["RoomTypeName"]?.ToString());
                    }
                }
                _cboRoomType.DataSource = roomTypeSrc;
                _cboRoomType.DisplayMember = "RoomTypeName";
                _cboRoomType.ValueMember = "RoomTypeId";

                var statusSrc = new DataTable();
                statusSrc.Columns.Add("StatusId", typeof(int));
                statusSrc.Columns.Add("StatusName", typeof(string));
                if (_statuses != null && _statuses.Columns.Contains("StatusId"))
                {
                    foreach (DataRow r in _statuses.Rows)
                    {
                        if (!int.TryParse(r["StatusId"]?.ToString(), out var id)) continue;
                        statusSrc.Rows.Add(id, r["StatusName"]?.ToString());
                    }
                }
                _cboStatus.DataSource = statusSrc;
                _cboStatus.DisplayMember = "StatusName";
                _cboStatus.ValueMember = "StatusId";

                if (_row != null)
                {
                    int? typeId = TryReadInt(_row, "RoomTypeId");
                    if (typeId.HasValue) _cboRoomType.SelectedValue = typeId.Value;

                    int? statusId = TryReadInt(_row, "CurrentStatusId");
                    if (statusId.HasValue) _cboStatus.SelectedValue = statusId.Value;

                    int occupants = TryReadInt(_row, "Occupants") ?? 0;
                    _numOccupants.Value = Math.Max(_numOccupants.Minimum, Math.Min(_numOccupants.Maximum, occupants));

                    decimal? price = TryReadDecimal(_row, "RoomPrice");
                    _lblPrice.Text = price.HasValue ? price.Value.ToString("N0") : "Không xác định";

                    decimal? area = TryReadDecimal(_row, "Area");
                    if (area.HasValue) _txtArea.Text = area.Value.ToString("0.##");

                    bool? active = TryReadBool(_row, "IsActive");
                    _chkActive.Checked = active ?? true;
                }
            }

            private static int? TryReadInt(DataRow row, string column)
            {
                if (row == null || !row.Table.Columns.Contains(column)) return null;
                return int.TryParse(row[column]?.ToString(), out var val) ? (int?)val : null;
            }

            private static decimal? TryReadDecimal(DataRow row, string column)
            {
                if (row == null || !row.Table.Columns.Contains(column)) return null;
                return decimal.TryParse(row[column]?.ToString(), out var val) ? (decimal?)val : null;
            }

            private static bool? TryReadBool(DataRow row, string column)
            {
                if (row == null || !row.Table.Columns.Contains(column)) return null;
                return bool.TryParse(row[column]?.ToString(), out var val) ? (bool?)val : null;
            }
        }

        private static string SafeToString(DataRow row, string column)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column)) return null;
            var v = row[column];
            return v == null || v == DBNull.Value ? null : v.ToString();
        }
    }
}
