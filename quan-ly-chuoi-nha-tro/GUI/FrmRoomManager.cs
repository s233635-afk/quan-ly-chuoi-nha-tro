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

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Màn hình quản lý phòng cho nhân viên (xem, lọc, đổi trạng thái, xem chi tiết).
    /// </summary>
    public class FrmRoomManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo số phòng/loại...";
        private const int MaxDisplayedRooms = RoomsPerSection * 2;

        private const int RoomsPerSection = 10;
        private const int RoomCardDefaultWidth = 300;
        private const int RoomCardMinWidth = 220;
        private const int RoomCardMaxWidth = 320;
        private const int RoomCardHorizontalMargin = 20;
        private const string PlaceholderFlagColumn = "IsPlaceholder";

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
        private Button _btnChangeStatus;
        private Button _btnEdit;

        private int _selectedRoomId = 0;
        private int _inspectingRoomId = 0;
        private DataRow _selectedRoomRow = null;

        public FrmRoomManager(AdminDataBLL bll, int? branchId = null)
        {
            _bll = bll ?? new AdminDataBLL();
            _branchId = branchId;
            InitializeComponent();
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

                if (_branchId.HasValue)
                {
                    TextFixer.ForceFixDataTable(_roomTypes, "RoomTypeName");
                    TextFixer.ForceFixDataTable(_statuses, "StatusName");
                    TextFixer.ForceFixDataTable(_rooms, "RoomNumber");
                    TextFixer.ForceFixDataTable(_tenants, "FullName", "PhoneNumber");
                }

                if (_branchId.HasValue && _rooms.Columns.Contains("BranchId"))
                {
                    var filtered = _rooms.AsEnumerable()
                        .Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == _branchId.Value);
                    _rooms = filtered.Any() ? filtered.CopyToDataTable() : _rooms.Clone();
                }

                EnsureDisplayColumns();
                PopulateOccupancy();
                NormalizeRoomStatusForOccupancy();
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
                    row["TypeName"] = typeName;
                if (statusLookup != null && _rooms.Columns.Contains("CurrentStatusId") && statusLookup.TryGetValue(row["CurrentStatusId"], out var statusName))
                    row["StatusName"] = statusName;
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
            var filteredRows = filteredTable.AsEnumerable().ToList();
            var displayedRows = LimitRoomsBySection(filteredRows);

            RenderRoomCards(displayedRows);
            UpdateStats(displayedRows);
        }

        private List<DataRow> LimitRoomsBySection(IEnumerable<DataRow> rows)
        {
            var list = rows?.ToList() ?? new List<DataRow>();
            if (list.Count == 0) return list;

            if (!_branchId.HasValue)
            {
                var roomsA = list
                    .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("A", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                    .Take(RoomsPerSection);

                var roomsB = list
                    .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("B", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                    .Take(RoomsPerSection);

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
                .ToList();

            var selectedA = SelectRoomsForSectionA(roomsAStaff);
            if (!ShouldPadStaffRooms())
                return selectedA.Concat(roomsBStaff.Take(RoomsPerSection)).ToList();

            return BuildStaffRoomSlots(selectedA, roomsBStaff, list.First().Table);
        }

        private bool ShouldPadStaffRooms()
        {
            if (!_branchId.HasValue) return false;
            string raw = (_txtSearch?.Text ?? string.Empty).Trim();
            if (raw == SearchPlaceholder) raw = string.Empty;
            if (!string.IsNullOrWhiteSpace(raw)) return false;
            if (_cboStatus != null && _cboStatus.SelectedIndex > 0) return false;
            return true;
        }

        private List<DataRow> BuildStaffRoomSlots(IList<DataRow> roomsA, IList<DataRow> roomsB, DataTable table)
        {
            if (table == null)
                return roomsA.Concat(roomsB.Take(RoomsPerSection)).ToList();

            EnsurePlaceholderColumn(table);

            int placeholderId = -1;
            var finalA = BuildStaffSectionRooms(roomsA, 'A', table, ref placeholderId);

            var bLookup = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);
            if (roomsB != null)
            {
                foreach (var row in roomsB)
                {
                    var number = NormalizeRoomNumber(SafeToString(row, "RoomNumber"));
                    if (string.IsNullOrWhiteSpace(number) || number[0] != 'B') continue;
                    if (!bLookup.ContainsKey(number))
                        bLookup[number] = row;
                }
            }

            var finalB = new List<DataRow>();
            var usedB = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var aRow in finalA)
            {
                var aNumber = NormalizeRoomNumber(SafeToString(aRow, "RoomNumber"));
                if (string.IsNullOrWhiteSpace(aNumber) || aNumber.Length < 2) continue;

                string bNumber = "B" + aNumber.Substring(1);
                if (bLookup.TryGetValue(bNumber, out var bRow))
                {
                    finalB.Add(bRow);
                    usedB.Add(bNumber);
                }
                else
                {
                    finalB.Add(CreatePlaceholderRow(table, bNumber, 'B', ref placeholderId, aRow));
                    usedB.Add(bNumber);
                }
            }

            if (finalB.Count < RoomsPerSection)
            {
                foreach (var number in BuildRoomNumberTemplate('B'))
                {
                    if (finalB.Count >= RoomsPerSection) break;
                    if (usedB.Contains(number)) continue;
                    finalB.Add(CreatePlaceholderRow(table, number, 'B', ref placeholderId, null));
                    usedB.Add(number);
                }
            }

            return finalA.Concat(finalB).ToList();
        }

        private List<DataRow> BuildStaffSectionRooms(IList<DataRow> rooms, char prefix, DataTable table, ref int placeholderId)
        {
            var unique = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);
            if (rooms != null)
            {
                foreach (var row in rooms)
                {
                    var number = NormalizeRoomNumber(SafeToString(row, "RoomNumber"));
                    if (string.IsNullOrWhiteSpace(number) || number[0] != prefix) continue;
                    if (!unique.ContainsKey(number))
                        unique[number] = row;
                }
            }

            var result = unique.Values.ToList();
            if (result.Count < RoomsPerSection)
            {
                foreach (var number in BuildRoomNumberTemplate(prefix))
                {
                    if (result.Count >= RoomsPerSection) break;
                    if (unique.ContainsKey(number)) continue;
                    var placeholder = CreatePlaceholderRow(table, number, prefix, ref placeholderId, null);
                    unique[number] = placeholder;
                    result.Add(placeholder);
                }
            }

            return result
                .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                .Take(RoomsPerSection)
                .ToList();
        }

        private static IEnumerable<string> BuildRoomNumberTemplate(char prefix)
        {
            char code = char.ToUpperInvariant(prefix);
            for (int i = 1; i <= RoomsPerSection; i++)
                yield return $"{code}{i:00}";
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
            _lblRoomCount.Text = $"Phòng: {roomsToRender.Count}/{MaxDisplayedRooms}";
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
                flow.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = "Không có phòng.",
                    ForeColor = Color.FromArgb(90, 90, 90)
                });
            }
            else
            {
                foreach (var row in rows)
                {
                    bool isPlaceholder = IsPlaceholderRow(row);
                    int roomId = TryGetInt(row, "RoomId");
                    if (roomId <= 0 && !isPlaceholder) continue;
                    flow.Controls.Add(CreateRoomCard(row, roomId));
                }
            }

            container.Controls.Add(header, 0, 0);
            container.Controls.Add(flow, 0, 1);
            flow.Height = flow.PreferredSize.Height;
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
            int cardWidth = CalculateCardWidth(width);

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
                            ApplyCardWidths(flow, cardWidth);
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
            string roomNumber = SafeToString(row, "RoomNumber") ?? "N/A";
            string typeName = SafeToString(row, "TypeName") ?? "N/A";
            string statusName = SafeToString(row, "StatusName") ?? "N/A";
            var statusColor = GetStatusColor(statusName);
            var hoverColor = Color.FromArgb(245, 249, 255);
            decimal price = TryGetDecimal(row, "RoomPrice") ?? 0m;
            int occupants = TryGetInt(row, "Occupants");
            bool isPlaceholder = IsPlaceholderRow(row);
            bool isSelected = _selectedRoomId == roomId;
            int cardWidth = CalculateCardWidth(GetRoomCardsAvailableWidth());
            var baseBackColor = isPlaceholder ? Color.FromArgb(248, 249, 251) : Color.White;

            var card = new Panel
            {
                Width = cardWidth,
                Height = 180,
                BackColor = isSelected ? Color.FromArgb(236, 242, 255) : baseBackColor,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10, 10, 10, 10),
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
                    using (var pen = new Pen(Color.FromArgb(0, 122, 204), 2))
                        e.Graphics.DrawRectangle(pen, rect);
                }
            };

            card.MouseEnter += (s, e) =>
            {
                if (!isSelected) card.BackColor = hoverColor;
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = isSelected ? Color.FromArgb(236, 242, 255) : baseBackColor;
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
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(14, 12, 14, 12)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lblRoom = new Label
            {
                Text = $"Ph\u00f2ng {roomNumber}",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Left,
                AutoSize = false,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0)
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
                Text = $"{price:N0}d",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false,
                TextAlign = ContentAlignment.TopRight,
                Margin = new Padding(0, 0, 0, 6)
            };

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
                Text = $"Lo\u1ea1i: {GetTypeIcon(typeName)} {typeName}",
                Font = new Font("Segoe UI", 11f, FontStyle.Regular),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top,
                Height = 22,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 0, 0, 4)
            };

            var lblStatusInfo = new Label
            {
                Text = $"Tr\u1ea1ng th\u00e1i: {statusName}",
                Font = new Font("Segoe UI Semibold", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 55, 110),
                Dock = DockStyle.Top,
                Height = 22,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 0, 0, 4)
            };

            var lblOccupantsInfo = new Label
            {
                Text = $"S\u1ed1 ng\u01b0\u1eddi: {occupants}",
                Font = new Font("Segoe UI", 11f, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Top,
                Height = 22,
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

            Action handleClick = () =>
            {
                ShowRoomDetails(row, roomId);
                ShowRoomInfoForm(row, roomId);
            };
            if (_branchId.HasValue)
            {
                RegisterRoomCardClick(card, handleClick);
            }
            else
            {
                card.Click += (s, e) => handleClick();
                lblRoom.Click += (s, e) => handleClick();
                lblPrice.Click += (s, e) => handleClick();
                lblTypeInfo.Click += (s, e) => handleClick();
                lblStatusInfo.Click += (s, e) => handleClick();
                lblOccupantsInfo.Click += (s, e) => handleClick();
            }

            return card;
        }

        private void RegisterRoomCardClick(Control root, Action handler)
        {
            if (root == null || handler == null) return;
            root.Cursor = Cursors.Hand;
            root.Click += (s, e) => handler();
            foreach (Control child in root.Controls)
                RegisterRoomCardClick(child, handler);
        }

        private static bool IsPlaceholderRow(DataRow row)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(PlaceholderFlagColumn)) return false;
            var value = row[PlaceholderFlagColumn];
            if (value == null || value == DBNull.Value) return false;
            if (value is bool flag) return flag;
            return bool.TryParse(value.ToString(), out var parsed) && parsed;
        }

        private static void EnsurePlaceholderColumn(DataTable table)
        {
            if (table == null) return;
            if (!table.Columns.Contains(PlaceholderFlagColumn))
                table.Columns.Add(PlaceholderFlagColumn, typeof(bool));
        }

        private int GetRoomCardsAvailableWidth()
        {
            if (_roomCardsHost == null) return RoomCardDefaultWidth;
            return Math.Max(200, _roomCardsHost.ClientSize.Width - _roomCardsHost.Padding.Left - _roomCardsHost.Padding.Right);
        }

        private int CalculateCardWidth(int availableWidth)
        {
            if (!_branchId.HasValue) return RoomCardDefaultWidth;
            if (availableWidth <= 0) return RoomCardDefaultWidth;

            int columns = Math.Max(1, availableWidth / (RoomCardMinWidth + RoomCardHorizontalMargin));
            int width = (availableWidth / columns) - RoomCardHorizontalMargin;
            if (width > RoomCardMaxWidth)
            {
                columns = Math.Max(1, availableWidth / (RoomCardMaxWidth + RoomCardHorizontalMargin));
                width = (availableWidth / columns) - RoomCardHorizontalMargin;
            }

            return Math.Max(RoomCardMinWidth, Math.Min(width, RoomCardMaxWidth));
        }

        private static void ApplyCardWidths(FlowLayoutPanel flow, int cardWidth)
        {
            if (flow == null) return;
            foreach (Control child in flow.Controls)
            {
                if (child is Panel panel)
                    panel.Width = cardWidth;
            }
        }

private DataRow CreatePlaceholderRow(DataTable table, string roomNumber, char prefix, ref int placeholderId, DataRow templateRow)
        {
            if (table == null) return null;

            EnsurePlaceholderColumn(table);

            var row = table.NewRow();
            SetColumnValue(row, "RoomId", placeholderId--);
            SetColumnValue(row, "RoomNumber", roomNumber);
            SetColumnValue(row, PlaceholderFlagColumn, true);

            if (templateRow != null)
            {
                CopyColumnValue(row, templateRow, "RoomTypeId");
                CopyColumnValue(row, templateRow, "TypeName");
                CopyColumnValue(row, templateRow, "RoomTypeName");
                CopyColumnValue(row, templateRow, "RoomPrice");
                CopyColumnValue(row, templateRow, "Area");
                CopyColumnValue(row, templateRow, "BranchId");
                CopyColumnValue(row, templateRow, "IsActive");
            }

            ApplyPlaceholderType(row, roomNumber);
            ApplyPlaceholderStatus(row);

            if (row.Table.Columns.Contains("Occupants"))
                row["Occupants"] = 0;

            if (row.Table.Columns.Contains("BranchId") && row["BranchId"] == DBNull.Value && _branchId.HasValue)
                row["BranchId"] = _branchId.Value;

            if (row.Table.Columns.Contains("IsActive") && row["IsActive"] == DBNull.Value)
                row["IsActive"] = true;

            if (row.Table.Columns.Contains("SectionId"))
                row["SectionId"] = prefix == 'A' ? 1 : 2;

            table.Rows.Add(row);
            return row;
        }

        private void ApplyPlaceholderType(DataRow row, string roomNumber)
        {
            if (row == null || row.Table == null) return;

            var template = GetPlaceholderTypeTemplate(roomNumber);
            if (row.Table.Columns.Contains("RoomTypeId") && row["RoomTypeId"] == DBNull.Value && template.typeId.HasValue)
                row["RoomTypeId"] = template.typeId.Value;

            if (row.Table.Columns.Contains("TypeName") && string.IsNullOrWhiteSpace(row["TypeName"]?.ToString()) && !string.IsNullOrWhiteSpace(template.typeName))
                row["TypeName"] = template.typeName;

            if (row.Table.Columns.Contains("RoomTypeName") && string.IsNullOrWhiteSpace(row["RoomTypeName"]?.ToString()) && !string.IsNullOrWhiteSpace(template.typeName))
                row["RoomTypeName"] = template.typeName;

            if (row.Table.Columns.Contains("RoomPrice") && row["RoomPrice"] == DBNull.Value && template.price.HasValue)
                row["RoomPrice"] = template.price.Value;
        }

        private void ApplyPlaceholderStatus(DataRow row)
        {
            if (row == null || row.Table == null) return;

            var emptyId = GetEmptyStatusId();
            var emptyName = GetEmptyStatusName();
            if (row.Table.Columns.Contains("CurrentStatusId") && emptyId.HasValue)
                row["CurrentStatusId"] = emptyId.Value;
            if (row.Table.Columns.Contains("StatusName") && !string.IsNullOrWhiteSpace(emptyName))
                row["StatusName"] = emptyName;
        }

        private (int? typeId, string typeName, decimal? price) GetPlaceholderTypeTemplate(string roomNumber)
        {
            int number = GetRoomSortKey(roomNumber).number;
            string targetName;
            if (number >= 1 && number <= 4)
                targetName = RoomTypeCatalog.PhongDon;
            else if (number <= 7)
                targetName = RoomTypeCatalog.PhongDoi;
            else
                targetName = RoomTypeCatalog.PhongCaoCap;

            return GetRoomTypeTemplate(targetName);
        }

        private (int? typeId, string typeName, decimal? price) GetRoomTypeTemplate(string canonicalName)
        {
            if (_roomTypes != null && _roomTypes.Columns.Contains("RoomTypeName"))
            {
                foreach (DataRow r in _roomTypes.Rows)
                {
                    var rawName = r["RoomTypeName"]?.ToString();
                    var fixedName = RoomTypeCatalog.Canonicalize(rawName);
                    if (!string.Equals(fixedName, canonicalName, StringComparison.OrdinalIgnoreCase)) continue;

                    int? typeId = TryGetNullableInt(r, "RoomTypeId");
                    decimal? price = null;
                    if (r.Table.Columns.Contains("DefaultPrice"))
                        price = TryGetDecimal(r, "DefaultPrice");
                    if (!price.HasValue && r.Table.Columns.Contains("RoomPrice"))
                        price = TryGetDecimal(r, "RoomPrice");

                    return (typeId, fixedName, price);
                }
            }

            return (null, canonicalName, null);
        }

        private static bool CopyColumnValue(DataRow target, DataRow source, string columnName)
        {
            if (target == null || source == null) return false;
            if (!target.Table.Columns.Contains(columnName) || !source.Table.Columns.Contains(columnName)) return false;
            var value = source[columnName];
            if (value == null || value == DBNull.Value) return false;
            target[columnName] = value;
            return true;
        }

        private static void SetColumnValue(DataRow row, string columnName, object value)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(columnName)) return;
            row[columnName] = value ?? DBNull.Value;
        }

        private void ShowRoomDetailsForm(DataRow row, int roomId)
        {
            _selectedRoomId = roomId;
            _selectedRoomRow = row;
            _inspectingRoomId = roomId;

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
            _selectedRoomId = roomId;
            _selectedRoomRow = row;
            if (_rooms != null)
            {
                var current = _rooms.AsEnumerable().FirstOrDefault(r => TryGetInt(r, "RoomId") == roomId);
                if (current != null)
                    _selectedRoomRow = current;
            }
            _inspectingRoomId = roomId;
            ApplyFilter();
            _btnEdit.Visible = _selectedRoomRow != null && !IsPlaceholderRow(_selectedRoomRow);
            if (_splitContainer != null) _splitContainer.Panel2Collapsed = false;
            BuildRoomDetailsPanel();
        }

        private void BuildRoomDetailsPanel()
        {
            _roomDetailPanel.Controls.Clear();

            if (_selectedRoomRow == null)
            {
                var header = BuildDetailHeader("Th\u00f4ng tin ph\u00f2ng");
                var emptyLabel = new Label
                {
                    Text = "Ch\u1ecdn m\u1ed9t ph\u00f2ng \u0111\u1ec3 xem chi ti\u1ebft",
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Dock = DockStyle.Top,
                    Height = 30
                };

                _roomDetailPanel.Controls.Add(emptyLabel);
                _roomDetailPanel.Controls.Add(header);
                return;
            }

            if (IsPlaceholderRow(_selectedRoomRow) || TryGetInt(_selectedRoomRow, "RoomId") <= 0)
            {
                var header = BuildDetailHeader("Th\u00f4ng tin ph\u00f2ng");
                var emptyLabel = new Label
                {
                    Text = "Ph\u00f2ng ch\u01b0a c\u00f3 d\u1eef li\u1ec7u. Vui l\u00f2ng ch\u1ea1y seed database.",
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Dock = DockStyle.Top,
                    Height = 30
                };

                _roomDetailPanel.Controls.Add(emptyLabel);
                _roomDetailPanel.Controls.Add(header);
                return;
            }

            var detailHeader = BuildDetailHeader($"Ph\u00f2ng {SafeToString(_selectedRoomRow, "RoomNumber") ?? "-"}");

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

            AddDetailRow(layout, "S\u1ed1 ph\u00f2ng:", SafeToString(_selectedRoomRow, "RoomNumber"));
            AddDetailRow(layout, "Lo\u1ea1i ph\u00f2ng:", SafeToString(_selectedRoomRow, "TypeName"));
            AddDetailRow(layout, "Tr\u1ea1ng th\u00e1i:", SafeToString(_selectedRoomRow, "StatusName"));
            AddDetailRow(layout, "Gi\u00e1/Th\u00e1ng:", (TryGetDecimal(_selectedRoomRow, "RoomPrice") ?? 0m).ToString("N0"));
            AddDetailRow(layout, "Di\u1ec7n t\u00edch:", (TryGetDecimal(_selectedRoomRow, "Area") ?? 0m).ToString("0.##") + " m\u00b2");
            AddDetailRow(layout, "S\u1ed1 ng\u01b0\u1eddi:", TryGetInt(_selectedRoomRow, "Occupants").ToString());

            var editButton = new Button
            {
                Text = "Ch\u1ec9nh s\u1eeda",
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

        private void ShowRoomInfoForm(DataRow row, int roomId)
        {
            if (row == null) return;

            if (IsPlaceholderRow(row) || roomId <= 0)
            {
                MessageBox.Show("Ph\u00f2ng ch\u01b0a c\u00f3 d\u1eef li\u1ec7u. Vui l\u00f2ng ch\u1ea1y seed database.", "Th\u00f4ng b\u00e1o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

        private async Task RefreshRoomQuickViewAsync(int roomId)
        {
            await LoadRoomsAsync();
            if (_rooms == null) return;

            var row = _rooms.AsEnumerable().FirstOrDefault(r => TryGetInt(r, "RoomId") == roomId);
            if (row != null)
                ShowRoomDetails(row, roomId);
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

            return lines.Count > 0 ? string.Join("\r\n", lines) : null;
        }

        private static bool IsHistoryActive(DataRow history)
        {
            if (history == null || history.Table == null) return false;

            var checkout = history.Table.Columns.Contains("CheckOutDate") ? history["CheckOutDate"] : null;
            bool hasCheckout = checkout != null && checkout != DBNull.Value;
            if (hasCheckout) return false;

            if (!history.Table.Columns.Contains("Status")) return true;
            var statusText = history["Status"]?.ToString() ?? string.Empty;
            return statusText.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0
                   || statusText.IndexOf("dang", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static DateTime? TryGetDate(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return null;
            return DateTime.TryParse(row[column]?.ToString(), out var val) ? val : (DateTime?)null;
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
                if (_splitContainer != null) _splitContainer.Panel2Collapsed = true;
            };

            panel.Controls.Add(btnClose);
            panel.Controls.Add(lblTitle);
            return panel;
        }

        private Panel BuildTenantListPanel()
        {
            var panel = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 12, 0, 0) };

            var lblTitle = new Label
            {
                Text = "Ng\u01b0\u1eddi \u0111ang thu\u00ea",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false
            };
            panel.Controls.Add(lblTitle);

            var summary = BuildTenantSummary(_selectedRoomId);
            var lbl = new Label
            {
                Text = string.IsNullOrWhiteSpace(summary) ? "Ph\u00f2ng hi\u1ec7n ch\u01b0a c\u00f3 ng\u01b0\u1eddi s\u1eed d\u1ee5ng." : summary,
                Dock = DockStyle.Top,
                AutoSize = true,
                ForeColor = Color.FromArgb(50, 50, 50)
            };
            panel.Controls.Add(lbl);

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
                Text = value ?? "-",
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

            _lblSummary.Text = $"\u0110ang \u1edf/T\u1ed5ng: {occupied}/{MaxDisplayedRooms} ph\u00f2ng";
        }

        private async void ChangeRoomStatus()
        {
            if (_selectedRoomId <= 0)
            {
                MessageBox.Show("Ch\u1ecdn m\u1ed9t ph\u00f2ng tr\u01b0\u1edbc.", "Th\u00f4ng b\u00e1o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new Form())
            {
                dlg.Text = "\u0110\u1ed5i tr\u1ea1ng th\u00e1i ph\u00f2ng";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Size = new Size(420, 180);
                dlg.BackColor = Color.White;
                dlg.Font = Font;

                var lbl = new Label { Text = "Tr\u1ea1ng th\u00e1i m\u1edbi:", AutoSize = true, Location = new Point(16, 22) };
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
                    Text = "C\u1eadp nh\u1eadt",
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
                    Text = "H\u1ee7y",
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
                            MessageBox.Show("Ph\u00f2ng ch\u01b0a c\u00f3 ng\u01b0\u1eddi, kh\u00f4ng th\u1ec3 chuy\u1ec3n sang \u0110ang \u1edf.", "C\u1ea3nh b\u00e1o",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        await UpdateRoomStatusAsync(_selectedRoomId, newStatusId);
                        await LoadRoomsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"L\u1ed7i c\u1eadp nh\u1eadt: {ex.Message}", "L\u1ed7i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void EditCurrentRoom()
        {
            if (_selectedRoomRow == null)
            {
                MessageBox.Show("Ch\u1ecdn m\u1ed9t ph\u00f2ng tr\u01b0\u1edbc.", "Th\u00f4ng b\u00e1o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (IsPlaceholderRow(_selectedRoomRow) || TryGetInt(_selectedRoomRow, "RoomId") <= 0)
            {
                MessageBox.Show("Ph\u00f2ng ch\u01b0a c\u00f3 d\u1eef li\u1ec7u. Vui l\u00f2ng ch\u1ea1y seed database.", "Th\u00f4ng b\u00e1o", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    bool isStaffMode = _branchId.HasValue;

                    if (occupants > 5)
                    {
                        MessageBox.Show("M\u1ed7i ph\u00f2ng t\u1ed1i \u0111a 5 ng\u01b0\u1eddi.", "C\u1ea3nh b\u00e1o", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (isStaffMode)
                    {
                        if (occupants < 1)
                        {
                            var emptyId = GetEmptyStatusId();
                            if (emptyId.HasValue)
                                statusId = emptyId.Value;
                        }
                    }
                    else if (IsOccupiedStatusId(statusId) && occupants < 1)
                    {
                        var emptyId = GetEmptyStatusId();
                        if (emptyId.HasValue)
                        {
                            statusId = emptyId.Value;
                            MessageBox.Show("Ph\u00f2ng ch\u01b0a c\u00f3 ng\u01b0\u1eddi, t\u1ef1 chuy\u1ec3n tr\u1ea1ng th\u00e1i v\u1ec1 Tr\u1ed1ng.", "Th\u00f4ng b\u00e1o",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Tr\u1ea1ng th\u00e1i \u0110ang \u1edf y\u00eau c\u1ea7u \u00edt nh\u1ea5t 1 ng\u01b0\u1eddi.", "C\u1ea3nh b\u00e1o", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    await _bll.UpdateRoomAsync(roomId, roomNumber, branchId, sectionId, roomTypeId, price, statusId, floor, area, isActive, occupants);

                    UpdateRowValues(_selectedRoomRow, roomNumber, roomTypeId, price, statusId, floor, area, isActive, occupants);
                    UpdateRowValues(_rooms?.AsEnumerable().FirstOrDefault(r => TryGetInt(r, "RoomId") == roomId), roomNumber, roomTypeId, price, statusId, floor, area, isActive, occupants);

                    ApplyFilter();
                    BuildRoomDetailsPanel();
                    _inspectingRoomId = roomId;
                    _roomCardsHost.PerformLayout();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("L\u1ed7i c\u1eadp nh\u1eadt ph\u00f2ng: " + ex.Message, "L\u1ed7i", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (isOccupied) return Color.FromArgb(0, 122, 204);     // xanh duong
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

        private static List<DataRow> SelectRoomsForSectionA(List<DataRow> rooms)
        {
            var selected = new List<DataRow>();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AppendRooms(selected, rooms.Where(IsSingleRoomType).OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber"))), 4, used);
            AppendRooms(selected, rooms.Where(IsDoubleRoomType).OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber"))), 7, used);
            AppendRooms(selected, rooms.Where(IsPremiumRoomType).OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber"))), 10, used);

            if (selected.Count < 10)
                AppendRooms(selected, rooms.OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber"))), 10, used);

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
                BackColor = Color.White;

                int dialogWidth = _isStaffMode ? 480 : 420;
                int dialogHeight = _isStaffMode ? 330 : 300;
                int fieldWidth = _isStaffMode ? 300 : 260;
                int roomWidth = _isStaffMode ? 160 : 140;
                int buttonY = _isStaffMode ? 270 : 248;
                int cancelX = dialogWidth - 90 - 12;
                int okX = cancelX - 8 - 120;

                ClientSize = new Size(dialogWidth, dialogHeight);

                var lblRoom = new Label { Text = "Số phòng:", AutoSize = true, Location = new Point(18, 18) };
                var txtRoom = new TextBox
                {
                    ReadOnly = true,
                    Width = roomWidth,
                    Location = new Point(120, 14),
                    Text = _row != null ? _row["RoomNumber"]?.ToString() ?? string.Empty : string.Empty
                };

                var lblType = new Label { Text = "Loại phòng:", AutoSize = true, Location = new Point(18, 56) };
                _cboRoomType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = fieldWidth, Location = new Point(120, 52) };
                if (_isStaffMode)
                {
                    _cboRoomType.Enabled = false;
                    _cboRoomType.TabStop = false;
                }

                var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, Location = new Point(18, 94) };
                _cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = fieldWidth, Location = new Point(120, 90) };

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
                _txtArea = new TextBox { Width = fieldWidth, Location = new Point(120, 198) };

                _chkActive = new CheckBox { Text = "Kích hoạt", AutoSize = true, Location = new Point(120, 226) };

                if (_isStaffMode)
                {
                    _numOccupants.Minimum = 1;
                    _numOccupants.Maximum = 5;
                    _txtArea.ReadOnly = true;
                    _txtArea.TabStop = false;
                    _txtArea.BackColor = Color.White;
                }

                var btnOk = new Button
                {
                    Text = "Cập nhật",
                    Width = 120,
                    Height = 34,
                    Location = new Point(okX, buttonY),
                    BackColor = Color.FromArgb(0, 122, 204),
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
                    Location = new Point(cancelX, buttonY),
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

                if (_isStaffMode)
                {
                    int? typeDonId = null;
                    int? typeDoiId = null;
                    int? typeCaoCapId = null;

                    if (_roomTypes != null && _roomTypes.Columns.Contains("RoomTypeId"))
                    {
                        foreach (DataRow r in _roomTypes.Rows)
                        {
                            if (!int.TryParse(r["RoomTypeId"]?.ToString(), out var id)) continue;
                            var rawName = r["RoomTypeName"]?.ToString();
                            var name = TextFixer.FixUtf8Mojibake(rawName) ?? rawName ?? string.Empty;
                            var key = NormalizeStatusKey(name);

                            if (!typeDonId.HasValue && (key.Contains("don") || key.Contains("single")))
                                typeDonId = id;
                            if (!typeDoiId.HasValue && (key.Contains("doi") || key.Contains("double")))
                                typeDoiId = id;
                            if (!typeCaoCapId.HasValue && (key.Contains("cao cap") || key.Contains("vip") || key.Contains("premium")))
                                typeCaoCapId = id;
                        }
                    }

                    if (typeDonId.HasValue) roomTypeSrc.Rows.Add(typeDonId.Value, LabelRoomSingle);
                    if (typeDoiId.HasValue) roomTypeSrc.Rows.Add(typeDoiId.Value, LabelRoomDouble);
                    if (typeCaoCapId.HasValue) roomTypeSrc.Rows.Add(typeCaoCapId.Value, LabelRoomPremium);

                    if (roomTypeSrc.Rows.Count == 0 && _roomTypes != null && _roomTypes.Columns.Contains("RoomTypeId"))
                    {
                        foreach (DataRow r in _roomTypes.Rows)
                        {
                            if (!int.TryParse(r["RoomTypeId"]?.ToString(), out var id)) continue;
                            roomTypeSrc.Rows.Add(id, r["RoomTypeName"]?.ToString());
                        }
                    }
                }
                else
                {
                    if (_roomTypes != null && _roomTypes.Columns.Contains("RoomTypeId"))
                    {
                        foreach (DataRow r in _roomTypes.Rows)
                        {
                            if (!int.TryParse(r["RoomTypeId"]?.ToString(), out var id)) continue;
                            roomTypeSrc.Rows.Add(id, r["RoomTypeName"]?.ToString());
                        }
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

                if (_isStaffMode)
                {
                    bool hasOccupied = statusSrc.AsEnumerable()
                        .Select(r => r["StatusName"]?.ToString() ?? string.Empty)
                        .Any(name => NormalizeStatusKey(name).Contains("dang o"));
                    if (!hasOccupied)
                    {
                        MessageBox.Show("Thi\u1ebfu tr\u1ea1ng th\u00e1i \"\u0110ang \u1edf\". Vui l\u00f2ng ch\u1ea1y seed_room_statuses.sql tr\u01b0\u1edbc khi ch\u1ec9nh s\u1eeda.",
                            "Th\u00f4ng b\u00e1o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.Cancel;
                        Close();
                        return;
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
                    _lblPrice.Text = price.HasValue ? price.Value.ToString("N0") : "Kh\u00f4ng x\u00e1c \u0111\u1ecbnh";

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

