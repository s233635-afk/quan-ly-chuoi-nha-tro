using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Màn hình quản lý phòng cho nhân viên (xem, lọc, đổi trạng thái, xem chi tiết).
    /// </summary>
    public class FrmRoomManager : Form
    {
        private const int DefaultDisplayedRooms = 20;

        private readonly AdminDataBLL _bll;
        private readonly int? _branchId;
        private readonly bool _isStaffMode;

        private DataTable _rooms;
        private DataTable _statuses;
        private DataTable _roomTypes;
        private DataTable _tenantHistory;
        private DataTable _contracts;
        private DataTable _tenants;
        private DataTable _assets;
        private readonly Dictionary<int, List<DataRow>> _assetsByRoom = new Dictionary<int, List<DataRow>>();

        private SplitContainer _splitContainer;
        private FlowLayoutPanel _roomCardsHost;
        private Panel _roomDetailPanel;
        private ComboBox _cboStatus;
        private ModernSearchBox _searchBox;
        private Label _lblSummary;
        private Label _lblRoomCount;
        private NumericUpDown _numDisplayLimit;

        private Button _btnSearch;
        private Button _btnRefresh;
        private Button _btnAddRoom;
        private Button _btnChangeStatus;
        private Button _btnEdit;
        private Button _btnDelete;

        private int _selectedRoomId = 0;
        private int _inspectingRoomId = 0;
        private DataRow _selectedRoomRow = null;
        private int _displayLimit = DefaultDisplayedRooms;

        public FrmRoomManager(AdminDataBLL bll, int? branchId = null, bool isStaffMode = false)
        {
            _bll = bll ?? new AdminDataBLL();
            _branchId = branchId;
            _isStaffMode = isStaffMode;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            DataSyncManager.RoomsDataChanged += HandleRoomsDataSync;
            FormClosing += (s, e) =>
            {
                AdminEvents.DataChanged -= HandleAdminDataChanged;
                DataSyncManager.RoomsDataChanged -= HandleRoomsDataSync;
            };
        }

        public FrmRoomManager() : this(new AdminDataBLL(), null, false)
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
                Height = 120,
                Padding = new Padding(12, 15, 12, 10),
                BackColor = Color.White
            };
            toolbar.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(ModernTheme.Colors.Border, 1), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            var toolbarLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            toolbarLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            toolbarLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 5, 0, 0)
            };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 6, 0)
            };

            // Remove separate "Tìm kiếm:" label - use placeholder text instead
            _searchBox = new ModernSearchBox
            {
                PlaceholderText = "Tìm theo số phòng/loại...",
                Width = 320,
                Height = 40,
                DebounceMs = 300,
                Margin = new Padding(0, 0, 20, 0)
            };
            _searchBox.SearchTriggered += (s, e) => ApplyFilter();

            var lblStatus = new Label
            {
                Text = "Trạng thái:",
                AutoSize = false,
                Width = 75,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 6, 0),
                Height = 40,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)
            };
            _cboStatus = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 6, 20, 0),
                Font = new Font("Segoe UI", 10F)
            };
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            var lblLimit = new Label
            {
                Text = "Hiển thị:",
                AutoSize = false,
                Width = 65,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 6, 0),
                Height = 40,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)
            };
            _numDisplayLimit = new NumericUpDown
            {
                Width = 70,
                Minimum = 0,
                Maximum = 9999,
                Value = DefaultDisplayedRooms,
                Margin = new Padding(0, 6, 0, 0),
                ThousandsSeparator = true,
                Font = new Font("Segoe UI", 10F)
            };
            _numDisplayLimit.ValueChanged += (s, e) =>
            {
                _displayLimit = (int)_numDisplayLimit.Value;
                ApplyFilter();
            };

            _btnSearch = new ModernButton
            {
                Text = "Tìm",
                Width = 80,
                Height = 32,
                BaseColor = ModernTheme.Colors.Primary,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 6, 0)
            };
            _btnSearch.FlatAppearance.BorderSize = 0;
            _btnSearch.Click += (s, e) => ApplyFilter();

            _btnRefresh = new ModernButton
            {
                Text = "Làm mới",
                Width = 88,
                Height = 32,
                BaseColor = Color.FromArgb(40, 167, 69),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 6, 0)
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += async (s, e) => await LoadRoomsAsync();

            _btnAddRoom = new ModernButton
            {
                Text = "Thêm phòng",
                Width = 100,
                Height = 32,
                BaseColor = Color.FromArgb(23, 162, 184),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 6, 0)
            };
            _btnAddRoom.FlatAppearance.BorderSize = 0;
            _btnAddRoom.Click += async (s, e) => await AddRoomAsync();

            _btnChangeStatus = new ModernButton
            {
                Text = "Đổi trạng thái",
                Width = 120,
                Height = 32,
                BaseColor = Color.FromArgb(255, 193, 7),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 8, 0),
                Visible = false
            };
            _btnChangeStatus.FlatAppearance.BorderSize = 0;
            _btnChangeStatus.Click += (s, e) => ChangeRoomStatus();

            _btnEdit = new ModernButton
            {
                Text = "Chỉnh sửa",
                Width = 90,
                Height = 32,
                BaseColor = Color.FromArgb(111, 66, 193),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 8, 0),
                Visible = false
            };
            _btnEdit.FlatAppearance.BorderSize = 0;
            _btnEdit.Click += (s, e) => EditCurrentRoom();

            _btnDelete = new ModernButton
            {
                Text = "Xóa",
                Width = 70,
                Height = 32,
                BaseColor = Color.FromArgb(220, 53, 69),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 0, 0),
                Visible = !_isStaffMode,
                Enabled = !_isStaffMode
            };
            _btnDelete.FlatAppearance.BorderSize = 0;
            _btnDelete.Click += async (s, e) => await DeleteCurrentRoomAsync();

            filters.Controls.Add(_searchBox);
            filters.Controls.Add(lblStatus);
            filters.Controls.Add(_cboStatus);
            filters.Controls.Add(lblLimit);
            filters.Controls.Add(_numDisplayLimit);

            actions.Controls.Add(_btnSearch);
            actions.Controls.Add(_btnRefresh);
            actions.Controls.Add(_btnAddRoom);
            actions.Controls.Add(_btnEdit);
            actions.Controls.Add(_btnChangeStatus);
            actions.Controls.Add(_btnDelete);

            // Apply Staff Mode restrictions
            if (_isStaffMode)
            {
                _btnAddRoom.Visible = false;
                _btnAddRoom.Enabled = false;
                _btnEdit.Enabled = false;
                _btnChangeStatus.Enabled = false;
            }

            var stats = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 10, 0, 0)
            };
            _lblSummary = new Label
            {
                Text = $"Đang ở/Tổng: 0/{GetLimitText()} phòng",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.Primary
            };
            _lblRoomCount = new Label
            {
                Text = $"Phòng: 0/{GetLimitText()}",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.Primary,
                Margin = new Padding(12, 0, 0, 0)
            };
            stats.Controls.Add(_lblSummary);
            stats.Controls.Add(_lblRoomCount);

            toolbarLayout.Controls.Add(filters, 0, 0);
            toolbarLayout.Controls.Add(actions, 1, 0);
            toolbarLayout.Controls.Add(stats, 0, 1);
            toolbarLayout.SetColumnSpan(stats, 2); // Span across both columns
            toolbar.Controls.Add(toolbarLayout);

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
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(16)
            };
            _roomCardsHost.SizeChanged += (s, e) => AdjustSectionWidths();

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
            using (var loading = new SimpleLoadingOverlay(this, "Đang tải dữ liệu phòng"))
            {
                try
                {
                    _selectedRoomId = 0;
                    _selectedRoomRow = null;
                    _btnEdit.Visible = false;
                    _btnDelete.Enabled = true;
                    if (_splitContainer != null)
                    {
                        _splitContainer.Panel2Collapsed = true;
                    }

                    var roomsTask = _bll.GetRoomsAsync();
                    var statusesTask = _bll.GetRoomStatusesAsync();
                    var typesTask = _bll.GetRoomTypesAsync();
                    var historyTask = _bll.GetTenantHistoryAsync();
                    var contractsTask = _bll.GetContractsAsync();
                    var tenantsTask = _bll.GetTenantsAsync();
                    var assetsTask = _bll.GetAssetsAsync();

                    await Task.WhenAll(roomsTask, statusesTask, typesTask, historyTask, contractsTask, tenantsTask, assetsTask);

                    _rooms = roomsTask.Result ?? new DataTable();
                    _statuses = statusesTask.Result ?? new DataTable();
                    _roomTypes = typesTask.Result ?? new DataTable();
                    _tenantHistory = historyTask.Result ?? new DataTable();
                    _contracts = contractsTask.Result ?? new DataTable();
                    _tenants = tenantsTask.Result ?? new DataTable();
                    _assets = assetsTask.Result ?? new DataTable();

                    NormalizeLoadedTables();

                    if (_branchId.HasValue && _rooms.Columns.Contains("BranchId"))
                    {
                        var filtered = _rooms.AsEnumerable()
                            .Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == _branchId.Value);
                        _rooms = filtered.Any() ? filtered.CopyToDataTable() : _rooms.Clone();
                    }

                    EnsureDisplayColumns();
                    BuildAssetLookup();
                    PopulateOccupancy();
                    NormalizeRoomStatusForOccupancy();
                    PopulateStatusFilter();

                    ApplyFilter();
                }
                catch (Exception ex)
                {
                    ErrorLogger.HandleException(ex, "LoadRooms", "Không thể tải danh sách phòng");
                }
            }
        }

        private void NormalizeLoadedTables()
        {
            TextFixer.ForceFixDataTable(_rooms, "RoomNumber", "TypeName", "RoomTypeName", "StatusName", "BranchName", "SectionName");
            TextFixer.ForceFixDataTable(_roomTypes, "RoomTypeName", "Amenities", "Description");
            TextFixer.ForceFixDataTable(_statuses, "StatusName", "Description");
            TextFixer.ForceFixDataTable(_assets, "AssetName", "Description");
            TextFixer.ForceFixDataTable(_tenants, "FullName", "PhoneNumber");

            RoomTypeCatalog.CanonicalizeRoomTypeColumn(_rooms, "TypeName");
            RoomTypeCatalog.CanonicalizeRoomTypeColumn(_roomTypes, "RoomTypeName");
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

        private void BuildAssetLookup()
        {
            _assetsByRoom.Clear();
            if (_assets == null || !_assets.Columns.Contains("RoomId")) return;

            IEnumerable<DataRow> rows = _assets.AsEnumerable();
            if (_branchId.HasValue && _assets.Columns.Contains("BranchId"))
            {
                rows = rows.Where(r => TryGetInt(r, "BranchId") == _branchId.Value);
            }

            foreach (var asset in rows)
            {
                if (!int.TryParse(asset["RoomId"]?.ToString(), out var roomId) || roomId <= 0)
                    continue;

                if (!_assetsByRoom.TryGetValue(roomId, out var list))
                {
                    list = new List<DataRow>();
                    _assetsByRoom[roomId] = list;
                }
                list.Add(asset);
            }
        }

        private string BuildAssetInlineSummary(int roomId)
        {
            if (!_assetsByRoom.TryGetValue(roomId, out var list) || list == null || list.Count == 0)
                return null;

            var grouped = list
                .GroupBy(r =>
                {
                    var rawName = SafeReadString(r, "AssetName");
                    var fixedName = NormalizeAssetText(rawName);
                    return fixedName ?? $"T\u00e0i s\u1ea3n #{SafeToString(r, "AssetId") ?? "?"}";
                })
                .Select(g => new
                {
                    Name = g.Key,
                    Quantity = g.Sum(r => Math.Max(1, TryGetInt(r, "Quantity")))
                })
                .OrderByDescending(g => g.Quantity)
                .ThenBy(g => g.Name)
                .ToList();

            if (grouped.Count == 0) return null;

            var top = grouped.Take(2)
                .Select(g => $"{g.Name}×{g.Quantity}")
                .ToList();

            var summary = string.Join(", ", top);
            int remaining = grouped.Count - top.Count;
            if (remaining > 0)
            {
                summary = string.IsNullOrEmpty(summary) ? $"(+{remaining} m\u1ee5c)" : $"{summary} +{remaining}";
            }

            return summary;
        }

        private decimal GetAssetCharge(int roomId)
        {
            if (!_assetsByRoom.TryGetValue(roomId, out var list) || list == null || list.Count == 0)
                return 0m;

            decimal total = 0m;
            foreach (var asset in list)
            {
                int qty = Math.Max(1, TryGetInt(asset, "Quantity"));
                decimal price = TryGetDecimal(asset, "PurchasePrice") ?? 0m;
                total += qty * price;
            }

            return total;
        }

        private static string NormalizeAssetText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            var fixedValue = TextFixer.ForceFixUtf8Mojibake(value) ?? value;
            return fixedValue.Trim();
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

        private async void HandleRoomsDataSync(object sender, EventArgs e)
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

        private void NormalizeRoomStatusForOccupancy()
        {
            if (_rooms == null) return;
            var emptyId = GetEmptyStatusId();
            var emptyName = GetEmptyStatusName();

            foreach (DataRow row in _rooms.Rows)
            {
                int occupants = TryGetInt(row, "Occupants");
                string statusName = row.Table.Columns.Contains("StatusName") ? row["StatusName"]?.ToString() ?? string.Empty : string.Empty;

                if (occupants < 1 && IsOccupiedStatusName(statusName))
                {
                    if (emptyId.HasValue && row.Table.Columns.Contains("CurrentStatusId"))
                        row["CurrentStatusId"] = emptyId.Value;
                    if (!string.IsNullOrWhiteSpace(emptyName) && row.Table.Columns.Contains("StatusName"))
                        row["StatusName"] = emptyName;
                }
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

            _cboStatus.DisplayMember = "StatusName";
            _cboStatus.ValueMember = "StatusId";
            _cboStatus.DataSource = dt;
            if (dt.Rows.Count > 0)
            {
                _cboStatus.SelectedValue = 0;
            }
        }

        private void ApplyFilter()
        {
            if (_rooms == null) return;

            var view = new DataView(_rooms);
            var filters = new List<string>();
            bool hasStatusColumn = _rooms.Columns.Contains("CurrentStatusId");

            var raw = (_searchBox.Text ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var escaped = raw.Replace("'", "''");
                filters.Add($"Convert(RoomNumber, 'System.String') LIKE '%{escaped}%' OR Convert(TypeName, 'System.String') LIKE '%{escaped}%'");
            }

            if (hasStatusColumn && _cboStatus.SelectedValue is int statusId && statusId > 0)
                filters.Add($"Convert(CurrentStatusId, 'System.Int32') = {statusId}");

            view.RowFilter = filters.Count > 0 ? string.Join(" AND ", filters) : string.Empty;

            var filteredTable = view.ToTable();
            var filteredRows = filteredTable.AsEnumerable().ToList();
            var displayedRows = LimitRoomsBySection(filteredRows);

            RenderRoomCards(displayedRows);
            UpdateStats(displayedRows);
        }

        private List<DataRow> LimitRoomsBySection(IEnumerable<DataRow> rows)
        {
            var list = rows?.ToList() ?? new List<DataRow>();
            if (list.Count == 0) return list;

            if (_displayLimit <= 0)
            {
                if (_branchId.HasValue)
                {
                    return list
                        .GroupBy(r => NormalizeRoomNumber(SafeToString(r, "RoomNumber")))
                        .Select(g => g.First())
                        .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                        .ToList();
                }
                return list;
            }

            int limitA = (_displayLimit + 1) / 2;
            int limitB = _displayLimit / 2;

            if (!_branchId.HasValue)
            {
                var roomsA = list
                    .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("A", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                    .Take(limitA);

                var roomsB = list
                    .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("B", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                    .Take(limitB);

                return roomsA.Concat(roomsB).ToList();
            }

            var unique = list
                .GroupBy(r => NormalizeRoomNumber(SafeToString(r, "RoomNumber")))
                .Select(g => g.First())
                .ToList();

            var roomsAStaff = unique
                .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("A", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                .ToList();

            var roomsBStaff = unique
                .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("B", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                .Take(limitB)
                .ToList();

            var selectedA = SelectRoomsForSectionA(roomsAStaff, limitA);
            return selectedA.Concat(roomsBStaff).ToList();
        }

        private void RenderRoomCards(IEnumerable<DataRow> displayedRows)
        {
            _roomCardsHost.SuspendLayout();
            _roomCardsHost.Controls.Clear();

            var roomsToRender = displayedRows?.ToList() ?? new List<DataRow>();

            if (roomsToRender.Count == 0)
            {
                var emptyState = EmptyStatePanel.ForNoSearchResults();
                emptyState.Dock = DockStyle.Fill;
                emptyState.MinimumSize = new Size(400, 300);
                _roomCardsHost.Controls.Add(emptyState);
                _roomCardsHost.ResumeLayout();
                _lblRoomCount.Text = $"Phòng: 0/{GetLimitText()}";
                return;
            }

            var roomsA = roomsToRender
                .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("A", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                .ToList();
            var roomsB = roomsToRender
                .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("B", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                .ToList();

            _roomCardsHost.Controls.Add(BuildRoomSection("Dãy A", roomsA));
            _roomCardsHost.Controls.Add(BuildSectionDivider());
            _roomCardsHost.Controls.Add(BuildRoomSection("Dãy B", roomsB));

            AdjustSectionWidths();

            _roomCardsHost.ResumeLayout();
            _lblRoomCount.Text = $"Phòng: {roomsToRender.Count}/{GetLimitText()}";
        }

        private Control BuildRoomSection(string title, IList<DataRow> rows)
        {
            var container = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 0, 0, 8),
                Tag = "room-section"
            };
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var header = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Margin = new Padding(4, 0, 0, 8)
            };

            var flow = new FlowLayoutPanel
            {
                AutoSize = false,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = new Padding(0),
                Tag = "room-section-flow"
            };
            int initialWidth = _roomCardsHost == null
                ? 900
                : Math.Max(200, _roomCardsHost.ClientSize.Width - _roomCardsHost.Padding.Left - _roomCardsHost.Padding.Right);
            flow.Width = initialWidth;
            flow.MaximumSize = new Size(initialWidth, 0);

            if (rows == null || rows.Count == 0)
            {
                var emptyState = EmptyStatePanel.ForNoData("phòng");
                emptyState.Dock = DockStyle.Fill;
                emptyState.MinimumSize = new Size(initialWidth - 20, 300);
                emptyState.Height = 300;
                flow.Height = 320;
                flow.AutoSize = false;
                flow.Controls.Add(emptyState);
            }
            else
            {
                foreach (var row in rows)
                {
                    int roomId = TryGetInt(row, "RoomId");
                    if (roomId <= 0) continue;
                    flow.Controls.Add(CreateRoomCard(row, roomId));
                }
                flow.Height = flow.PreferredSize.Height;
            }

            container.Controls.Add(header, 0, 0);
            container.Controls.Add(flow, 0, 1);
            return container;
        }

        private Control BuildSectionDivider()
        {
            var divider = new Panel
            {
                Height = 14,
                BackColor = Color.FromArgb(220, 225, 232),
                Margin = new Padding(0, 2, 0, 10),
                Tag = "room-divider"
            };

            var line = new Panel
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = Color.FromArgb(200, 205, 212)
            };
            divider.Controls.Add(line);
            return divider;
        }

        private void AdjustSectionWidths()
        {
            if (_roomCardsHost == null) return;
            int width = Math.Max(200, _roomCardsHost.ClientSize.Width - _roomCardsHost.Padding.Left - _roomCardsHost.Padding.Right);

            foreach (Control ctl in _roomCardsHost.Controls)
            {
                if (ctl is TableLayoutPanel table && (string)table.Tag == "room-section")
                {
                    table.Width = width;
                    table.MaximumSize = new Size(width, 0);

                    foreach (Control child in table.Controls)
                    {
                        if (child is FlowLayoutPanel flow && (string)flow.Tag == "room-section-flow")
                        {
                            flow.Width = width;
                            flow.MaximumSize = new Size(width, 0);
                            flow.Height = flow.PreferredSize.Height;
                        }
                    }
                }
                else if (ctl is Panel panel && (string)panel.Tag == "room-divider")
                {
                    panel.Width = width;
                }
            }
        }

        private Panel CreateRoomCard(DataRow row, int roomId)
        {
            string roomNumber = SafeReadString(row, "RoomNumber") ?? "N/A";
            string typeName = SafeReadString(row, "TypeName") ?? "N/A";
            string statusName = SafeReadString(row, "StatusName") ?? "N/A";
            var statusColor = GetStatusColor(statusName);
            var hoverColor = Color.FromArgb(245, 249, 255);
            decimal price = TryGetDecimal(row, "RoomPrice") ?? 0m;
            int occupants = TryGetInt(row, "Occupants");
            string assetSummary = BuildAssetInlineSummary(roomId);
            bool isSelected = _selectedRoomId == roomId;

            var card = new Panel
            {
                Width = 320,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(320, 0),
                MaximumSize = new Size(320, 0),
                BackColor = isSelected ? Color.FromArgb(236, 242, 255) : Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(12, 12, 12, 12),
                Cursor = Cursors.Hand,
                Tag = roomId
            };

            card.Paint += (s, e) =>
            {
                var borderColor = isSelected ? Color.FromArgb(0, 95, 180) : Color.FromArgb(210, 215, 220);
                var borderWidth = isSelected ? 3 : 1;
                using (var pen = new Pen(borderColor, borderWidth))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
                using (var shadowPen = new Pen(Color.FromArgb(230, 235, 240), 1))
                {
                    e.Graphics.DrawRectangle(shadowPen, 1, 1, card.Width - 3, card.Height - 3);
                }

                if (_inspectingRoomId == roomId)
                {
                    var rect = new Rectangle(card.Width - 26, card.Height - 26, 20, 20);
                    using (var fill = new SolidBrush(Color.FromArgb(225, 238, 255)))
                        e.Graphics.FillRectangle(fill, rect);
                    using (var pen = new Pen(ModernTheme.Colors.Primary, 2))
                        e.Graphics.DrawRectangle(pen, rect);
                }
            };

            card.MouseEnter += (s, e) =>
            {
                if (!isSelected) card.BackColor = hoverColor;
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = isSelected ? Color.FromArgb(236, 242, 255) : Color.White;
            };

            var statusBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 10,
                BackColor = statusColor
            };
            card.Controls.Add(statusBar);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(14, 14, 14, 14)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Header: Phòng + icon
            var lblRoom = new Label
            {
                Text = $"Phòng {roomNumber}",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 50, 90),
                Dock = DockStyle.Top,
                Height = 30,
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
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.Primary,
                Dock = DockStyle.Top,
                Height = 28,
                AutoSize = false,
                TextAlign = ContentAlignment.TopRight,
                Margin = new Padding(0, 0, 0, 6)
            };

            // Info: Loại, Trạng thái, Số người
            var infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0)
            };
            var infoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = string.IsNullOrEmpty(assetSummary) ? 3 : 4,
                Padding = new Padding(0)
            };

            var lblTypeInfo = new Label
            {
                Text = $"Loại: {GetTypeIcon(typeName)} {typeName}",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Regular),
                ForeColor = ModernTheme.Colors.Primary,
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 0, 0, 6)
            };

            var lblStatusInfo = new Label
            {
                Text = $"Trạng thái: {statusName}",
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 55, 110),
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 0, 0, 6)
            };

            var lblOccupantsInfo = new Label
            {
                Text = $"S\u1ed1 ng\u01b0\u1eddi: {occupants}",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };

            infoLayout.Controls.Add(lblTypeInfo, 0, 0);
            infoLayout.Controls.Add(lblStatusInfo, 0, 1);
            infoLayout.Controls.Add(lblOccupantsInfo, 0, 2);

            if (!string.IsNullOrEmpty(assetSummary))
            {
                assetSummary = TextFixer.FixUtf8Mojibake(assetSummary) ?? assetSummary;
                var lblAssetsInfo = new Label
                {
                    Text = $"T\u00e0i s\u1ea3n: {assetSummary}",
                    Font = new Font("Segoe UI", 10.5f),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Dock = DockStyle.Top,
                    Height = 24,
                    AutoSize = false,
                    TextAlign = ContentAlignment.TopLeft
                };
                infoLayout.Controls.Add(lblAssetsInfo, 0, 3);
            }
            infoPanel.Controls.Add(infoLayout);

            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.RowCount = 3;
            mainLayout.Controls.Add(lblPrice, 0, 1);
            mainLayout.Controls.Add(infoPanel, 0, 2);

            card.Controls.Add(mainLayout);


            card.Click += (s, e) =>
            {
                ShowRoomDetails(row, roomId);
                ShowRoomInfoForm(row, roomId);
            };
            lblRoom.Click += (s, e) => { ShowRoomDetails(row, roomId); ShowRoomInfoForm(row, roomId); };
            lblPrice.Click += (s, e) => { ShowRoomDetails(row, roomId); ShowRoomInfoForm(row, roomId); };
            lblTypeInfo.Click += (s, e) => { ShowRoomDetails(row, roomId); ShowRoomInfoForm(row, roomId); };
            lblStatusInfo.Click += (s, e) => { ShowRoomDetails(row, roomId); ShowRoomInfoForm(row, roomId); };
            lblOccupantsInfo.Click += (s, e) => { ShowRoomDetails(row, roomId); ShowRoomInfoForm(row, roomId); };


            return card;
        }

        private void ShowRoomDetailsForm(DataRow row, int roomId)
        {
            _selectedRoomId = roomId;
            _selectedRoomRow = row;
            _inspectingRoomId = roomId;
            _btnEdit.Visible = true;
            _btnDelete.Enabled = true;

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
                    ToastNotification.Warning("Form chi tiết phòng chưa được tải");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "ShowRoomDetails", "Lỗi hiển thị chi tiết phòng");
            }
        }

        private void ShowRoomDetails(DataRow row, int roomId)
        {

            ShowRoomDetailsForm(row, roomId);
        }

        private async Task AddRoomAsync()
        {
            using (var frm = new FrmRoomEditor(_bll) { DefaultBranchId = _branchId })
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

        private async Task DeleteCurrentRoomAsync()
        {
            var candidates = GetFilteredRowsForDelete();
            if (candidates.Count == 0)
            {
                ToastNotification.Warning("Không có phòng nào để xóa");
                return;
            }

            using (var picker = new RoomDeletePicker(candidates, _selectedRoomRow))
            {
                if (picker.ShowDialog(this) != DialogResult.OK) return;
                var selected = picker.SelectedRows;
                if (selected == null || selected.Count == 0)
                {
                    ToastNotification.Warning("Chưa chọn phòng cần xóa");
                    return;
                }

                if (!ModernConfirmDialog.ConfirmDanger($"Xóa {selected.Count} phòng?")) return;

                try
                {
                    int deleted = 0;
                    var failures = new List<string>();
                    var deletedRoomIds = new HashSet<int>();
                    foreach (var row in selected)
                    {
                        int roomId = TryGetInt(row, "RoomId");
                        if (roomId <= 0 || deletedRoomIds.Contains(roomId)) continue;
                        try
                        {
                            bool ok = await _bll.DeleteRoomAsync(roomId);
                            if (ok) deleted++;
                            else failures.Add(SafeToString(row, "RoomNumber") ?? roomId.ToString());
                            deletedRoomIds.Add(roomId);
                        }
                        catch (Exception ex)
                        {
                            var label = SafeToString(row, "RoomNumber") ?? roomId.ToString();
                            failures.Add($"{label}: {ex.Message}");
                        }
                    }

                    AdminEvents.NotifyDataChanged();
                    DataSyncManager.NotifyRoomsChanged();
                    DataSyncManager.NotifyTenantsChanged();
                    DataSyncManager.NotifyContractsChanged();
                    DataSyncManager.NotifyInvoicesChanged();
                    DataSyncManager.NotifyPaymentsChanged();

                    _selectedRoomRow = null;
                    _selectedRoomId = 0;
                    _inspectingRoomId = 0;
                    _btnEdit.Visible = false;
                    _btnDelete.Enabled = true;
                    if (_splitContainer != null) _splitContainer.Panel2Collapsed = true;

                    await LoadRoomsAsync();
                    ApplyFilter();

                    if (failures.Count > 0)
                    {
                        ToastNotification.Warning($"Một số phòng không xóa được: {failures.Count}");
                    }
                    else if (deleted > 0)
                    {
                        ToastNotification.Success($"Đã xóa {deleted} phòng");
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.HandleException(ex, "DeleteRooms", "Lỗi xóa phòng");
                }
            }
        }

        private List<DataRow> GetFilteredRowsForDelete()
        {
            if (_rooms == null) return new List<DataRow>();

            var view = new DataView(_rooms);
            var filters = new List<string>();
            bool hasStatusColumn = _rooms.Columns.Contains("CurrentStatusId");

            var raw = (_searchBox.Text ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var escaped = raw.Replace("'", "''");
                filters.Add($"Convert(RoomNumber, 'System.String') LIKE '%{escaped}%' OR Convert(TypeName, 'System.String') LIKE '%{escaped}%'");
            }

            if (hasStatusColumn && _cboStatus.SelectedValue is int statusId && statusId > 0)
                filters.Add($"Convert(CurrentStatusId, 'System.Int32') = {statusId}");

            view.RowFilter = filters.Count > 0 ? string.Join(" AND ", filters) : string.Empty;
            var result = view.ToTable().AsEnumerable().ToList();
            if (_rooms.Columns.Contains("RoomId"))
                result = result
                    .GroupBy(r => TryGetInt(r, "RoomId"))
                    .Select(g => g.First())
                    .ToList();
            return result;
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

            var deleteButton = new Button
            {
                Text = "Xóa",
                AutoSize = true,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(12, 6, 12, 6),
                Margin = new Padding(8, 12, 0, 0)
            };
            deleteButton.FlatAppearance.BorderSize = 0;
            deleteButton.Click += async (s, e) => await DeleteCurrentRoomAsync();

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            buttonPanel.Controls.Add(editButton);
            buttonPanel.Controls.Add(deleteButton);

            var tenantsPanel = BuildTenantListPanel();

            _roomDetailPanel.Controls.Add(buttonPanel);
            _roomDetailPanel.Controls.Add(tenantsPanel);
            _roomDetailPanel.Controls.Add(layout);
            _roomDetailPanel.Controls.Add(detailHeader);
        }

        private void ShowRoomInfoForm(DataRow row, int roomId)
        {
            if (row == null) return;

            DataRow activeHistory = null;
            if (_tenantHistory != null && _tenantHistory.Columns.Contains("RoomId"))
            {
                activeHistory = _tenantHistory.AsEnumerable()
                    .Where(r => TryGetInt(r, "RoomId") == roomId)
                    .Where(IsHistoryActive)
                    .OrderByDescending(r => TryGetDate(r, "CheckInDate") ?? DateTime.MinValue)
                    .FirstOrDefault();
            }

            DataRow tenantRow = null;
            int tenantId = 0;
            if (activeHistory != null)
            {
                tenantId = TryGetInt(activeHistory, "TenantId");
                if (_tenants != null && tenantId > 0)
                    tenantRow = _tenants.AsEnumerable().FirstOrDefault(t => TryGetInt(t, "TenantId") == tenantId);
            }

            DataRow contractRow = null;
            if (_contracts != null && _contracts.Columns.Contains("RoomId"))
            {
                var contracts = _contracts.AsEnumerable().Where(c => TryGetInt(c, "RoomId") == roomId);
                if (tenantId > 0)
                    contracts = contracts.Where(c => TryGetInt(c, "TenantId") == tenantId);

                contractRow = contracts
                    .OrderByDescending(c => TryGetDate(c, "StartDate") ?? DateTime.MinValue)
                    .FirstOrDefault();
            }

            using (var frm = new FrmRoomTenantQuickView(_bll, RefreshRoomQuickViewAsync, true))
            {
                var tenantSummary = BuildTenantSummary(roomId);
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.UpdateData(roomId, row, tenantRow, contractRow, tenantSummary);
                frm.ShowDialog(this);
            }
        }

        private string BuildTenantSummary(int roomId)
        {
            if (_tenantHistory == null || _tenants == null) return null;

            var tenantIds = _tenantHistory.AsEnumerable()
                .Where(r => TryGetInt(r, "RoomId") == roomId)
                .Where(IsHistoryActive)
                .Select(r => TryGetInt(r, "TenantId"))
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (tenantIds.Count == 0) return null;

            var lines = new List<string>();
            int index = 1;
            foreach (var tenantId in tenantIds)
            {
                var tenant = _tenants.AsEnumerable().FirstOrDefault(t => TryGetInt(t, "TenantId") == tenantId);
                if (tenant == null) continue;

                var name = SafeToString(tenant, "FullName");
                if (!string.IsNullOrWhiteSpace(name))
                    name = TextFixer.FixUtf8Mojibake(name) ?? name;

                var phone = SafeToString(tenant, "PhoneNumber");
                if (!string.IsNullOrWhiteSpace(phone))
                    phone = TextFixer.FixUtf8Mojibake(phone) ?? phone;

                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(phone)) continue;
                var line = string.IsNullOrWhiteSpace(phone) ? name : $"{name} - {phone}";
                lines.Add($"{index}. {line}".Trim());
                index++;
            }

            return lines.Count > 0 ? string.Join("\n", lines) : null;
        }

        private static bool IsHistoryActive(DataRow history)
        {
            if (history == null || history.Table == null) return false;

            var checkout = history.Table.Columns.Contains("CheckOutDate") ? history["CheckOutDate"] : null;
            bool hasCheckout = checkout != null && checkout != DBNull.Value;
            if (!hasCheckout) return true;

            if (history.Table.Columns.Contains("Status"))
            {
                var statusText = history["Status"]?.ToString() ?? string.Empty;
                return statusText.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0
                    || statusText.IndexOf("đang", StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
        }

        private static DateTime? TryGetDate(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return null;
            return DateTime.TryParse(row[column]?.ToString(), out var val) ? val : (DateTime?)null;
        }

        private async System.Threading.Tasks.Task RefreshRoomQuickViewAsync(int roomId)
        {
            await LoadRoomsAsync();
            var refreshed = _rooms?.AsEnumerable().FirstOrDefault(r => TryGetInt(r, "RoomId") == roomId);
            if (refreshed != null)
            {
                _selectedRoomId = roomId;
                _selectedRoomRow = refreshed;
                _inspectingRoomId = roomId;
                ApplyFilter();
            }
        }

        private Control BuildDetailHeader(string title)
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 40 };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 60, 130),
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
                _inspectingRoomId = 0;
                _btnEdit.Visible = false;
                _btnDelete.Enabled = true;
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
                Dock = DockStyle.Fill,
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
                var emptyState = new EmptyStatePanel
                {
                    Icon = "👥",
                    Title = "Chưa có khách ở",
                    Description = "Phòng này chưa có khách thuê",
                    Dock = DockStyle.Top
                };
                container.Controls.Add(emptyState);
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
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 45),
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 6)
            };

            var lblValue = new Label
            {
                Text = value ?? "—",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 55, 110),
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 6)
            };

            layout.Controls.Add(lblLabel);
            layout.Controls.Add(lblValue);
        }

        private void UpdateStats(IEnumerable<DataRow> rows)
        {
            var list = rows?.ToList() ?? new List<DataRow>();

            int occupied = list.Count(r =>
                IsOccupiedStatusName(r["StatusName"]?.ToString()) && TryGetInt(r, "Occupants") >= 1);

            _lblSummary.Text = $"Đang ở/Tổng: {occupied}/{GetLimitText()} phòng";
        }

        private string GetLimitText()
        {
            return _displayLimit > 0 ? _displayLimit.ToString() : "∞";
        }

        private async void ChangeRoomStatus()
        {
            if (_selectedRoomId <= 0)
            {
                ToastNotification.Warning("Chọn một phòng trước");
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
                    BackColor = ModernTheme.Colors.Primary,
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
                        int currentOccupants = TryGetInt(_selectedRoomRow, "Occupants");
                        if (IsOccupiedStatusId(newStatusId) && currentOccupants < 1)
                        {
                            ToastNotification.Warning("Phòng chưa có người, không thể chuyển sang Đang ở");
                            return;
                        }

                        await UpdateRoomStatusAsync(_selectedRoomId, newStatusId);
                        await LoadRoomsAsync();
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.HandleException(ex, "ChangeStatus", "Lỗi cập nhật trạng thái");
                    }
                }
            }
        }

        private async void EditCurrentRoom()
        {
            if (_selectedRoomRow == null)
            {
                ToastNotification.Warning("Chọn một phòng trước");
                return;
            }

            int roomId = TryGetInt(_selectedRoomRow, "RoomId");

            using (var dlg = new RoomEditDialog(_selectedRoomRow, _roomTypes, _statuses, _branchId.HasValue))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    string roomNumber = SafeToString(_selectedRoomRow, "RoomNumber") ?? string.Empty;
                    int branchId = TryGetInt(_selectedRoomRow, "BranchId");
                    int? sectionId = TryGetNullableInt(_selectedRoomRow, "SectionId");
                    int? roomTypeId = dlg.SelectedRoomTypeId ?? TryGetNullableInt(_selectedRoomRow, "RoomTypeId");
                    decimal? price = dlg.RoomPrice ?? TryGetDecimal(_selectedRoomRow, "RoomPrice");
                    int? statusId = dlg.SelectedStatusId ?? TryGetNullableInt(_selectedRoomRow, "CurrentStatusId");
                    int? floor = TryGetNullableInt(_selectedRoomRow, "Floor");
                    decimal? area = dlg.Area ?? TryGetDecimal(_selectedRoomRow, "Area");
                    bool? isActive = dlg.IsActive ?? TryGetBool(_selectedRoomRow, "IsActive");
                    int occupants = dlg.OccupantCount ?? TryGetInt(_selectedRoomRow, "Occupants");

                    if (occupants > 5)
                    {
                        ToastNotification.Warning("Mỗi phòng tối đa 5 người");
                        return;
                    }

                    if (IsOccupiedStatusId(statusId) && occupants < 1)
                    {
                        var emptyId = GetEmptyStatusId();
                        if (emptyId.HasValue)
                        {
                            statusId = emptyId.Value;
                            ToastNotification.Info("Phòng chưa có người, tự chuyển trạng thái về Trống");
                        }
                        else
                        {
                            ToastNotification.Warning("Trạng thái Đang ở yêu cầu ít nhất 1 người");
                            return;
                        }
                    }

                    await _bll.UpdateRoomAsync(roomId, roomNumber, branchId, sectionId, roomTypeId, price, statusId, floor, area, isActive, occupants);

                    await LoadRoomsAsync();
                    AdminEvents.NotifyDataChanged();


                    // Cập nhật tại chỗ cho thấy ngay kết quả
                    UpdateRowValues(_selectedRoomRow, roomNumber, roomTypeId, price, statusId, floor, area, isActive, occupants);
                    UpdateRowValues(_rooms?.AsEnumerable().FirstOrDefault(r => TryGetInt(r, "RoomId") == roomId), roomNumber, roomTypeId, price, statusId, floor, area, isActive, occupants);

                    ApplyFilter();
                    BuildRoomDetailsPanel();
                    _inspectingRoomId = roomId;

                }
                catch (Exception ex)
                {
                    ErrorLogger.HandleException(ex, "EditRoom", "Lỗi cập nhật phòng");
                }
            }
        }

        private void UpdateRowValues(DataRow row, string roomNumber, int? roomTypeId, decimal? price, int? statusId, int? floor, decimal? area, bool? isActive, int occupants)
        {
            if (row == null || row.Table == null) return;
            void Set(string col, object val)
            {
                if (row.Table.Columns.Contains(col))
                    row[col] = val ?? DBNull.Value;
            }

            Set("RoomNumber", roomNumber);
            Set("RoomTypeId", roomTypeId);
            Set("RoomPrice", price);
            Set("CurrentStatusId", statusId);
            Set("Floor", floor);
            Set("Area", area);
            Set("IsActive", isActive);
            Set("Occupants", occupants);

            if (row.Table.Columns.Contains("TypeName") && roomTypeId.HasValue && _roomTypes != null)
            {
                var typeRow = _roomTypes.AsEnumerable()
                    .FirstOrDefault(r => int.TryParse(r["RoomTypeId"]?.ToString(), out var id) && id == roomTypeId.Value);
                row["TypeName"] = typeRow?["RoomTypeName"]?.ToString() ?? row["TypeName"];
            }

            if (row.Table.Columns.Contains("StatusName") && statusId.HasValue && _statuses != null)
            {
                var statusRow = _statuses.AsEnumerable()
                    .FirstOrDefault(r => int.TryParse(r["StatusId"]?.ToString(), out var id) && id == statusId.Value);
                row["StatusName"] = statusRow?["StatusName"]?.ToString() ?? row["StatusName"];
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

            var key = NormalizeStatusKey(statusName);
            bool isDeposit = key.Contains("coc") || key.Contains("dat coc");
            bool isMaintenance = key.Contains("bao tri") || key.Contains("maintenance");
            bool isOccupied = key.Contains("dang o") || key.Contains("dang thue") || key.Contains("da thue");

            if (isMaintenance) return Color.FromArgb(255, 220, 220); // do nhat
            if (isDeposit) return Color.FromArgb(255, 241, 188);    // vang nhat
            if (isOccupied) return ModernTheme.Colors.Primary;     // xanh duong
            return Color.White;                                     // trong
        }



        private bool IsOccupiedStatusId(int? statusId)
        {
            if (!statusId.HasValue || _statuses == null || !_statuses.Columns.Contains("StatusId")) return false;
            var row = _statuses.AsEnumerable().FirstOrDefault(r => int.TryParse(r["StatusId"]?.ToString(), out var id) && id == statusId.Value);
            var name = row?["StatusName"]?.ToString() ?? string.Empty;
            return IsOccupiedStatusName(name);
        }

        private int? GetEmptyStatusId()
        {
            if (_statuses == null || !_statuses.Columns.Contains("StatusId")) return null;
            var row = _statuses.AsEnumerable()
                .FirstOrDefault(r =>
                    (r["StatusName"]?.ToString() ?? string.Empty)
                        .IndexOf("trống", StringComparison.OrdinalIgnoreCase) >= 0
                    || (r["StatusName"]?.ToString() ?? string.Empty)
                        .IndexOf("trong", StringComparison.OrdinalIgnoreCase) >= 0);

            if (row == null) return null;
            return int.TryParse(row["StatusId"]?.ToString(), out var id) ? (int?)id : null;
        }

        private string GetEmptyStatusName()
        {
            if (_statuses == null || !_statuses.Columns.Contains("StatusName")) return "Trống";
            var row = _statuses.AsEnumerable()
                .FirstOrDefault(r =>
                    (r["StatusName"]?.ToString() ?? string.Empty)
                        .IndexOf("trống", StringComparison.OrdinalIgnoreCase) >= 0
                    || (r["StatusName"]?.ToString() ?? string.Empty)
                        .IndexOf("trong", StringComparison.OrdinalIgnoreCase) >= 0);
            return row?["StatusName"]?.ToString() ?? "Trống";
        }


        private static string NormalizeStatusKey(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName)) return string.Empty;
            var fixedName = TextFixer.FixUtf8Mojibake(statusName) ?? statusName;
            return RemoveDiacritics(fixedName).ToLowerInvariant();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (char ch in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static bool IsAllowedStaffStatusName(string statusName)
        {
            var key = NormalizeStatusKey(statusName);
            return key.Contains("dang o")
                || key.Contains("dang thue")
                || key.Contains("da thue")
                || key.Contains("bao tri")
                || key.Contains("coc")
                || key.Contains("trong");
        }

        private static bool IsOccupiedStatusName(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName)) return false;
            var key = NormalizeStatusKey(statusName);
            return key.Contains("dang o") || key.Contains("dang thue") || key.Contains("da thue");
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

        private static string NormalizeRoomNumber(string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber)) return string.Empty;
            return roomNumber.Trim().ToUpperInvariant();
        }

        private static string GetRoomTypeKey(DataRow row)
        {
            var typeName = SafeToString(row, "TypeName");
            if (string.IsNullOrWhiteSpace(typeName))
                typeName = SafeToString(row, "RoomTypeName");
            return NormalizeStatusKey(typeName ?? string.Empty);
        }

        private static bool IsSingleRoomType(DataRow row)
        {
            var key = GetRoomTypeKey(row);
            return key.Contains("don") || key.Contains("single");
        }

        private static bool IsDoubleRoomType(DataRow row)
        {
            var key = GetRoomTypeKey(row);
            return key.Contains("doi") || key.Contains("double");
        }

        private static bool IsPremiumRoomType(DataRow row)
        {
            var key = GetRoomTypeKey(row);
            return key.Contains("cao cap") || key.Contains("vip") || key.Contains("premium");
        }

        private static void AppendRooms(List<DataRow> target, IEnumerable<DataRow> source, int targetCount, HashSet<string> used)
        {
            foreach (var row in source)
            {
                if (target.Count >= targetCount) break;
                var roomNumber = NormalizeRoomNumber(SafeToString(row, "RoomNumber"));
                if (string.IsNullOrWhiteSpace(roomNumber) || used.Contains(roomNumber)) continue;
                target.Add(row);
                used.Add(roomNumber);
            }
        }

        private static List<DataRow> SelectRoomsForSectionA(List<DataRow> rooms, int maxCount)
        {
            var selected = new List<DataRow>();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int singleLimit = Math.Min(4, maxCount);
            int doubleLimit = Math.Min(7, maxCount);
            int premiumLimit = Math.Min(10, maxCount);

            AppendRooms(selected, rooms.Where(IsSingleRoomType).OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber"))), singleLimit, used);
            AppendRooms(selected, rooms.Where(IsDoubleRoomType).OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber"))), doubleLimit, used);
            AppendRooms(selected, rooms.Where(IsPremiumRoomType).OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber"))), premiumLimit, used);

            if (selected.Count < maxCount)
                AppendRooms(selected, rooms.OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber"))), maxCount, used);

            return selected
                .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                .ToList();
        }

        private static (int prefix, int number) GetRoomSortKey(string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber)) return (int.MaxValue, int.MaxValue);
            roomNumber = roomNumber.Trim();
            int prefix = char.ToUpper(roomNumber[0]);
            var digits = new string(roomNumber.Skip(1).Where(char.IsDigit).ToArray());
            int number = int.TryParse(digits, out var n) ? n : int.MaxValue;
            return (prefix, number);
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

        private sealed class RoomDeletePicker : Form
        {
            private readonly CheckedListBox _list;
            private readonly Label _lblCount;
            public List<DataRow> SelectedRows { get; private set; }

            public RoomDeletePicker(List<DataRow> rows, DataRow preselected)
            {
                Text = "Chọn phòng cần xóa";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(520, 420);
                BackColor = Color.White;
                Font = new Font("Segoe UI", 10F);

                var lbl = new Label
                {
                    Text = "Chọn phòng:",
                    AutoSize = true,
                    Location = new Point(14, 12)
                };

                _list = new CheckedListBox
                {
                    CheckOnClick = true,
                    Location = new Point(14, 36),
                    Size = new Size(492, 300)
                };

                foreach (var row in rows)
                {
                    string roomNumber = SafeToString(row, "RoomNumber") ?? "N/A";
                    string typeName = SafeToString(row, "TypeName") ?? SafeToString(row, "RoomTypeName") ?? "";
                    string statusName = SafeToString(row, "StatusName") ?? "";
                    string label = string.IsNullOrWhiteSpace(typeName)
                        ? $"{roomNumber} - {statusName}"
                        : $"{roomNumber} - {typeName} - {statusName}";
                    int index = _list.Items.Add(new ListItem(label, row), false);
                    if (preselected != null && TryGetInt(preselected, "RoomId") == TryGetInt(row, "RoomId"))
                        _list.SetItemChecked(index, true);
                }

                _lblCount = new Label
                {
                    Text = $"Tổng: {rows.Count}",
                    AutoSize = true,
                    Location = new Point(14, 346),
                    ForeColor = Color.FromArgb(80, 80, 80)
                };

                var btnSelectAll = new Button
                {
                    Text = "Chọn tất cả",
                    Width = 110,
                    Height = 28,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(230, 342)
                };
                btnSelectAll.FlatAppearance.BorderSize = 1;
                btnSelectAll.Click += (s, e) =>
                {
                    for (int i = 0; i < _list.Items.Count; i++)
                        _list.SetItemChecked(i, true);
                };

                var btnClear = new Button
                {
                    Text = "Bỏ chọn",
                    Width = 90,
                    Height = 28,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(346, 342)
                };
                btnClear.FlatAppearance.BorderSize = 1;
                btnClear.Click += (s, e) =>
                {
                    for (int i = 0; i < _list.Items.Count; i++)
                        _list.SetItemChecked(i, false);
                };

                var btnOk = new Button
                {
                    Text = "Xoa",
                    Width = 100,
                    Height = 32,
                    BackColor = Color.FromArgb(220, 53, 69),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(296, 360),
                    DialogResult = DialogResult.OK
                };
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Click += (s, e) => CollectSelection();

                var btnCancel = new Button
                {
                    Text = "Hủy",
                    Width = 90,
                    Height = 32,
                    BackColor = Color.FromArgb(220, 220, 220),
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(412, 360),
                    DialogResult = DialogResult.Cancel
                };
                btnCancel.FlatAppearance.BorderSize = 0;

                Controls.Add(lbl);
                Controls.Add(_list);
                Controls.Add(_lblCount);
                Controls.Add(btnSelectAll);
                Controls.Add(btnClear);
                Controls.Add(btnOk);
                Controls.Add(btnCancel);

                AcceptButton = btnOk;
                CancelButton = btnCancel;
            }

            private void CollectSelection()
            {
                SelectedRows = new List<DataRow>();
                foreach (var item in _list.CheckedItems)
                {
                    if (item is ListItem li && li.Row != null)
                        SelectedRows.Add(li.Row);
                }
            }

            private sealed class ListItem
            {
                public string Text { get; }
                public DataRow Row { get; }

                public ListItem(string text, DataRow row)
                {
                    Text = text;
                    Row = row;
                }

                public override string ToString() => Text;
            }
        }

        internal class RoomEditDialog : Form
        {
            private readonly DataRow _row;
            private readonly DataTable _roomTypes;
            private readonly DataTable _statuses;
            private readonly bool _isStaffMode;

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

            public RoomEditDialog(DataRow row, DataTable roomTypes, DataTable statuses, bool isStaffMode = false)
            {
                _row = row;
                _roomTypes = roomTypes;
                _statuses = statuses;
                _isStaffMode = isStaffMode;

                InitializeComponent();
                LoadData();
            }

            private void InitializeComponent()
            {
                Text = "Chỉnh sửa";
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
                _numOccupants = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = 5,
                    Width = 120,
                    Location = new Point(120, 128),
                    ReadOnly = false,
                    Enabled = true,
                    Increment = 1
                };

                var lblPrice = new Label { Text = "Giá/Tháng:", AutoSize = true, Location = new Point(18, 166) };
                _lblPrice = new Label { AutoSize = true, Location = new Point(120, 166), ForeColor = Color.FromArgb(70, 70, 70) };

                var lblArea = new Label { Text = "Diện tích (m²):", AutoSize = true, Location = new Point(18, 202) };
                _txtArea = new TextBox { Width = 260, Location = new Point(120, 198) };

                _chkActive = new CheckBox { Text = "Kích hoạt", AutoSize = true, Location = new Point(120, 226) };

                var btnOk = new Button
                {
                    Text = "Cập nhật",
                    Width = 120,
                    Height = 34,
                    Location = new Point(190, 248),
                    BackColor = ModernTheme.Colors.Primary,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.OK
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "Hủy",
                    Width = 90,
                    Height = 34,
                    Location = new Point(318, 248),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel
                };

                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
                btnCancel.FlatAppearance.BorderSize = 1;
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
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
                btnCancel.FlatAppearance.BorderSize = 1;
                Controls.Add(btnCancel);

                AcceptButton = btnOk;
                CancelButton = btnCancel;
            }

            private void LoadData()
            {
                const string LabelRoomSingle = "Ph\u00f2ng \u0111\u01a1n";
                const string LabelRoomDouble = "Ph\u00f2ng \u0111\u00f4i";
                const string LabelRoomPremium = "Ph\u00f2ng cao c\u1ea5p";
                const string LabelStatusEmpty = "Tr\u1ed1ng";
                const string LabelStatusMaintenance = "B\u1ea3o tr\u00ec";
                const string LabelStatusOccupied = "\u0110ang \u1edf";
                const string LabelStatusDeposit = "\u0110\u00e3 c\u1ecdc";

                var roomTypeSrc = new DataTable();
                roomTypeSrc.Columns.Add("RoomTypeId", typeof(int));
                roomTypeSrc.Columns.Add("RoomTypeName", typeof(string));

                var filteredTypes = RoomTypeCatalog.FilterToCanonicalTypes(_roomTypes);
                var sourceTypes = filteredTypes != null && filteredTypes.Columns.Contains("RoomTypeId")
                    ? filteredTypes
                    : _roomTypes;

                if (sourceTypes != null && sourceTypes.Columns.Contains("RoomTypeId"))
                {
                    foreach (DataRow typeRow in sourceTypes.Rows)
                    {
                        if (!int.TryParse(typeRow["RoomTypeId"]?.ToString(), out var id)) continue;
                        roomTypeSrc.Rows.Add(id, typeRow["RoomTypeName"]?.ToString());
                    }
                }

                _cboRoomType.DataSource = roomTypeSrc;
                _cboRoomType.DisplayMember = "RoomTypeName";
                _cboRoomType.ValueMember = "RoomTypeId";

                var statusSrc = new DataTable();
                statusSrc.Columns.Add("StatusId", typeof(int));
                statusSrc.Columns.Add("StatusName", typeof(string));

                if (_isStaffMode)
                {
                    int? statusTrongId = null;
                    int? statusBaoTriId = null;
                    int? statusDangId = null;
                    int? statusCocId = null;
                    int? statusDaThueId = null;

                    if (_statuses != null && _statuses.Columns.Contains("StatusId"))
                    {
                        foreach (DataRow r in _statuses.Rows)
                        {
                            if (!int.TryParse(r["StatusId"]?.ToString(), out var id)) continue;
                            var rawName = r["StatusName"]?.ToString();
                            var name = TextFixer.FixUtf8Mojibake(rawName) ?? rawName ?? string.Empty;
                            var key = NormalizeStatusKey(name);

                            if (!statusTrongId.HasValue && key.Contains("trong"))
                                statusTrongId = id;
                            if (!statusBaoTriId.HasValue && key.Contains("bao tri"))
                                statusBaoTriId = id;
                            if (!statusCocId.HasValue && (key.Contains("coc") || key.Contains("dat coc")))
                                statusCocId = id;
                            if (!statusDangId.HasValue && (key.Contains("dang o") || key.Contains("dang thue") || key.Contains("dang")))
                                statusDangId = id;
                            if (!statusDaThueId.HasValue && key.Contains("da thue"))
                                statusDaThueId = id;
                        }
                    }

                    if (statusTrongId.HasValue) statusSrc.Rows.Add(statusTrongId.Value, LabelStatusEmpty);
                    if (statusBaoTriId.HasValue) statusSrc.Rows.Add(statusBaoTriId.Value, LabelStatusMaintenance);
                    var dangId = statusDangId ?? statusDaThueId;
                    if (dangId.HasValue) statusSrc.Rows.Add(dangId.Value, LabelStatusOccupied);
                    if (statusCocId.HasValue) statusSrc.Rows.Add(statusCocId.Value, LabelStatusDeposit);
                }
                else
                {
                    if (_statuses != null && _statuses.Columns.Contains("StatusId"))
                    {
                        foreach (DataRow r in _statuses.Rows)
                        {
                            if (!int.TryParse(r["StatusId"]?.ToString(), out var id)) continue;
                            statusSrc.Rows.Add(id, r["StatusName"]?.ToString());
                        }
                    }
                }

                _cboStatus.DataSource = statusSrc;
                _cboStatus.DisplayMember = "StatusName";
                _cboStatus.ValueMember = "StatusId";
                if (_cboStatus.Items.Count > 0 && _cboStatus.SelectedIndex < 0)
                    _cboStatus.SelectedIndex = 0;

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

        private static string SafeReadString(DataRow row, string column)
        {
            var value = SafeToString(row, column);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static string SafeToString(DataRow row, string column)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column)) return null;
            var value = row[column];
            if (value == null || value == DBNull.Value) return null;
            var text = value.ToString();
            return TextFixer.ForceFixUtf8Mojibake(text) ?? text;
        }
    }
}






