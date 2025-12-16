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

        private readonly AdminDataBLL _bll;
        private readonly int? _branchId;

        private DataTable _rooms;
        private DataTable _statuses;
        private DataTable _roomTypes;
        private DataTable _tenantHistory;

        private DataGridView _grid;
        private ComboBox _cboStatus;
        private TextBox _txtSearch;
        private Label _lblSummary;

        private Button _btnSearch;
        private Button _btnRefresh;
        private Button _btnChangeStatus;
        private Button _btnEdit;

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
            AutoScroll = true;
            Text = "Quản lý phòng";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Width = 1100;
            Height = 650;

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
                Margin = new Padding(0, 4, 8, 0)
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
                Margin = new Padding(0, 4, 8, 0)
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
            filters.Controls.Add(_btnEdit);

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
                Text = "Đang ở/Tổng: 0/0",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            stats.Controls.Add(_lblSummary);

            toolbar.Controls.Add(stats);
            toolbar.Controls.Add(filters);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.CellDoubleClick += (s, e) => ShowRoomInfo();

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "OrderNo", DataPropertyName = "OrderNo", HeaderText = "STT", FillWeight = 40 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RoomId", DataPropertyName = "RoomId", Visible = false });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RoomNumber", DataPropertyName = "RoomNumber", HeaderText = "Số phòng", FillWeight = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TypeName", DataPropertyName = "TypeName", HeaderText = "Loại phòng", FillWeight = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RoomPrice", DataPropertyName = "RoomPrice", HeaderText = "Giá/Tháng", FillWeight = 110, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "StatusName", DataPropertyName = "StatusName", HeaderText = "Trạng thái", FillWeight = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Occupants", DataPropertyName = "Occupants", HeaderText = "Số người", FillWeight = 80, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Area", DataPropertyName = "Area", HeaderText = "Diện tích", FillWeight = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00", Alignment = DataGridViewContentAlignment.MiddleRight } });

            Controls.Add(_grid);
            Controls.Add(toolbar);

            Load += async (s, e) => await LoadRoomsAsync();
        }

        private async Task LoadRoomsAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                _rooms = await _bll.GetRoomsAsync() ?? new DataTable();
                _statuses = await _bll.GetRoomStatusesAsync() ?? new DataTable();
                _roomTypes = await _bll.GetRoomTypesAsync() ?? new DataTable();
                _tenantHistory = await _bll.GetTenantHistoryAsync() ?? new DataTable();

                if (_branchId.HasValue && _rooms.Columns.Contains("BranchId"))
                {
                    var filtered = _rooms.AsEnumerable()
                        .Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == _branchId.Value);
                    _rooms = filtered.Any() ? filtered.CopyToDataTable() : _rooms.Clone();
                }

                EnsureDisplayColumns();
                PopulateOccupancy();
                PopulateStatusFilter();

                _grid.DataSource = _rooms;
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

            // Chỉ hiển thị tối đa 20 phòng để tránh danh sách quá dài
            var filteredTable = view.ToTable();
            var aRooms = filteredTable.AsEnumerable()
                .Where(r => SafeToString(r, "RoomNumber")?.StartsWith("A", StringComparison.OrdinalIgnoreCase) == true)
                .OrderBy(r => SafeToString(r, "RoomNumber"))
                .Take(10)
                .ToList();
            var bRooms = filteredTable.AsEnumerable()
                .Where(r => SafeToString(r, "RoomNumber")?.StartsWith("B", StringComparison.OrdinalIgnoreCase) == true)
                .OrderBy(r => SafeToString(r, "RoomNumber"))
                .Take(10)
                .ToList();

            // A ở trên, B ở dưới, giữ nguyên giới hạn 10 mỗi nhóm
            var selected = aRooms.Concat(bRooms).ToList();

            var displayTable = filteredTable.Clone();
            if (!displayTable.Columns.Contains("OrderNo"))
                displayTable.Columns.Add("OrderNo", typeof(int));
            displayTable.Columns["OrderNo"].SetOrdinal(0);

            int order = 1;
            foreach (var row in selected)
            {
                var newRow = displayTable.NewRow();
                foreach (DataColumn col in filteredTable.Columns)
                    newRow[col.ColumnName] = row[col];
                newRow["OrderNo"] = order++;
                displayTable.Rows.Add(newRow);
            }

            _grid.DataSource = displayTable;
            UpdateStats(displayTable);
        }

        private void UpdateStats(DataTable table)
        {
            int total = table?.Rows.Count ?? 0;
            int occupied = 0;

            if (table != null)
            {
                occupied = table.AsEnumerable()
                    .Count(r => (r["StatusName"]?.ToString() ?? string.Empty)
                        .IndexOf("đang", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _lblSummary.Text = $"Đang ở/Tổng: {occupied}/{total}";
        }

        private DataRowView GetCurrentRow()
        {
            return _grid.CurrentRow?.DataBoundItem as DataRowView;
        }

        private async void ChangeRoomStatus()
        {
            var rowView = GetCurrentRow();
            if (rowView == null)
            {
                MessageBox.Show("Chọn một phòng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!int.TryParse(rowView["RoomId"]?.ToString(), out var roomId) || roomId <= 0)
            {
                MessageBox.Show("Không xác định được phòng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        await UpdateRoomStatusAsync(rowView, roomId, newStatusId);
                        await LoadRoomsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi cập nhật: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ShowRoomInfo()
        {
            var rowView = GetCurrentRow();
            if (rowView == null)
            {
                MessageBox.Show("Chọn một phòng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new Form())
            {
                dlg.Text = "Thông tin phòng";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Size = new Size(420, 300);
                dlg.BackColor = Color.White;
                dlg.Font = Font;

                int y = 18;
                int xLabel = 18;
                int xValue = 140;
                int line = 26;

                dlg.Controls.Add(MakeLabel("Số phòng:", xLabel, y));
                dlg.Controls.Add(MakeValueLabel(SafeToString(rowView.Row, "RoomNumber"), xValue, y));
                y += line;
                dlg.Controls.Add(MakeLabel("Loại phòng:", xLabel, y));
                dlg.Controls.Add(MakeValueLabel(SafeToString(rowView.Row, "TypeName"), xValue, y));
                y += line;
                dlg.Controls.Add(MakeLabel("Trạng thái:", xLabel, y));
                dlg.Controls.Add(MakeValueLabel(SafeToString(rowView.Row, "StatusName"), xValue, y));
                y += line;
                dlg.Controls.Add(MakeLabel("Số người:", xLabel, y));
                dlg.Controls.Add(MakeValueLabel(TryGetInt(rowView, "Occupants").ToString(), xValue, y));
                y += line;
                dlg.Controls.Add(MakeLabel("Giá/Tháng:", xLabel, y));
                dlg.Controls.Add(MakeValueLabel(TryGetDecimal(rowView, "RoomPrice")?.ToString("N0") ?? "-", xValue, y));
                y += line;
                dlg.Controls.Add(MakeLabel("Diện tích:", xLabel, y));
                dlg.Controls.Add(MakeValueLabel(TryGetDecimal(rowView, "Area")?.ToString("0.##") ?? "-", xValue, y));

                var btnEdit = new Button
                {
                    Text = "Chỉnh sửa",
                    Width = 100,
                    Height = 30,
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(dlg.ClientSize.Width - 220, dlg.ClientSize.Height - 60),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };
                btnEdit.FlatAppearance.BorderSize = 0;

                var btnClose = new Button
                {
                    Text = "Đóng",
                    Width = 80,
                    Height = 30,
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(dlg.ClientSize.Width - 110, dlg.ClientSize.Height - 60),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

                dlg.Controls.Add(btnEdit);
                dlg.Controls.Add(btnClose);
                dlg.AcceptButton = btnClose;
                dlg.CancelButton = btnClose;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    EditCurrentRoom(rowView);
                }
            }
        }

        private Label MakeLabel(string text, int x, int y)
        {
            return new Label { Text = text, AutoSize = true, Location = new Point(x, y) };
        }

        private Label MakeValueLabel(string text, int x, int y)
        {
            return new Label { Text = text, AutoSize = true, Location = new Point(x, y), ForeColor = Color.FromArgb(50, 50, 50) };
        }

        private async void EditCurrentRoom(DataRowView rowView = null)
        {
            if (rowView == null)
                rowView = GetCurrentRow();
            if (rowView == null)
            {
                MessageBox.Show("Chọn một phòng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new RoomEditDialog(rowView, _roomTypes, _statuses))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    string roomNumber = rowView["RoomNumber"]?.ToString() ?? string.Empty;
                    int roomId = TryGetInt(rowView, "RoomId");
                    int branchId = TryGetInt(rowView, "BranchId");
                    int? sectionId = TryGetNullableInt(rowView, "SectionId");
                    int? roomTypeId = dlg.SelectedRoomTypeId ?? TryGetNullableInt(rowView, "RoomTypeId");
                    decimal? price = dlg.RoomPrice ?? TryGetDecimal(rowView, "RoomPrice");
                    int? statusId = dlg.SelectedStatusId ?? TryGetNullableInt(rowView, "CurrentStatusId");
                    int? floor = TryGetNullableInt(rowView, "Floor");
                    decimal? area = dlg.Area ?? TryGetDecimal(rowView, "Area");
                    bool? isActive = dlg.IsActive ?? TryGetBool(rowView, "IsActive");
                    int occupants = dlg.OccupantCount ?? TryGetInt(rowView, "Occupants");

                    await _bll.UpdateRoomAsync(roomId, roomNumber, branchId, sectionId, roomTypeId, price, statusId, floor, area, isActive, occupants);
                    await LoadRoomsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi cập nhật phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task UpdateRoomStatusAsync(DataRowView row, int roomId, int newStatusId)
        {
            string roomNumber = row["RoomNumber"]?.ToString() ?? string.Empty;
            int branchId = TryGetInt(row, "BranchId");
            int? sectionId = TryGetNullableInt(row, "SectionId");
            int? roomTypeId = TryGetNullableInt(row, "RoomTypeId");
            decimal? price = TryGetDecimal(row, "RoomPrice");
            int? floor = TryGetNullableInt(row, "Floor");
            decimal? area = TryGetDecimal(row, "Area");
            bool? isActive = TryGetBool(row, "IsActive");

            await _bll.UpdateRoomAsync(roomId, roomNumber, branchId, sectionId, roomTypeId, price, newStatusId, floor, area, isActive, TryGetInt(row, "Occupants"));
        }

        private static int TryGetInt(DataRowView row, string column)
        {
            if (row == null || !row.Row.Table.Columns.Contains(column)) return 0;
            return int.TryParse(row[column]?.ToString(), out var val) ? val : 0;
        }

        private static int? TryGetNullableInt(DataRowView row, string column)
        {
            if (row == null || !row.Row.Table.Columns.Contains(column)) return null;
            return int.TryParse(row[column]?.ToString(), out var val) ? (int?)val : null;
        }

        private static decimal? TryGetDecimal(DataRowView row, string column)
        {
            if (row == null || !row.Row.Table.Columns.Contains(column)) return null;
            return decimal.TryParse(row[column]?.ToString(), out var val) ? (decimal?)val : null;
        }

        private static bool? TryGetBool(DataRowView row, string column)
        {
            if (row == null || !row.Row.Table.Columns.Contains(column)) return null;
            return bool.TryParse(row[column]?.ToString(), out var val) ? (bool?)val : null;
        }

        private class RoomEditDialog : Form
        {
            private readonly DataRowView _row;
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
            // Không cho sửa giá, luôn null để giữ nguyên giá gốc
            public decimal? RoomPrice => null;
            public int? OccupantCount => (int)_numOccupants.Value;
            public decimal? Area => decimal.TryParse(_txtArea.Text.Replace(",", ""), out var d) ? d : (decimal?)null;
            public bool? IsActive => _chkActive.Checked;

            public RoomEditDialog(DataRowView row, DataTable roomTypes, DataTable statuses)
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
                    Text = _row["RoomNumber"]?.ToString() ?? string.Empty
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
                // room types
                var roomTypeSrc = new DataTable();
                roomTypeSrc.Columns.Add("RoomTypeId", typeof(int));
                roomTypeSrc.Columns.Add("RoomTypeName", typeof(string));
                if (_roomTypes != null && _roomTypes.Columns.Contains("RoomTypeId"))
                {
                    foreach (DataRow r in _roomTypes.Rows)
                    {
                        if (!int.TryParse(r["RoomTypeId"]?.ToString(), out var id)) continue;
                        roomTypeSrc.Rows.Add(id, r["RoomTypeName"]?.ToString());
                    }
                }
                _cboRoomType.DataSource = roomTypeSrc;
                _cboRoomType.DisplayMember = "RoomTypeName";
                _cboRoomType.ValueMember = "RoomTypeId";

                // statuses
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

                // set current values
                int? typeId = TryReadInt(_row, "RoomTypeId");
                if (typeId.HasValue) _cboRoomType.SelectedValue = typeId.Value;

                int? statusId = TryReadInt(_row, "CurrentStatusId");
                if (statusId.HasValue) _cboStatus.SelectedValue = statusId.Value;

                
                int occupants = TryReadInt(_row, "Occupants") ?? 0;
                _numOccupants.Value = Math.Max(_numOccupants.Minimum, Math.Min(_numOccupants.Maximum, occupants));

decimal? price = TryReadDecimal(_row, "RoomPrice");
                _lblPrice.Text = price.HasValue ? price.Value.ToString("N0") : "KhÃ´ng xÃ¡c Ä‘á»‹nh";

                decimal? area = TryReadDecimal(_row, "Area");
                if (area.HasValue) _txtArea.Text = area.Value.ToString("0.##");

                bool? active = TryReadBool(_row, "IsActive");
                _chkActive.Checked = active ?? true;
            }

            private static int? TryReadInt(DataRowView row, string column)
            {
                if (row == null || !row.Row.Table.Columns.Contains(column)) return null;
                return int.TryParse(row[column]?.ToString(), out var val) ? (int?)val : null;
            }

            private static decimal? TryReadDecimal(DataRowView row, string column)
            {
                if (row == null || !row.Row.Table.Columns.Contains(column)) return null;
                return decimal.TryParse(row[column]?.ToString(), out var val) ? (decimal?)val : null;
            }

            private static bool? TryReadBool(DataRowView row, string column)
            {
                if (row == null || !row.Row.Table.Columns.Contains(column)) return null;
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
