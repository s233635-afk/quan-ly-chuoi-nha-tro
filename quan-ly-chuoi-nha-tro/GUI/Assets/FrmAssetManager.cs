using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmAssetManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo mã/tên/phòng/nhóm...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly int? _presetBranchId;
        private readonly bool _isStaffMode;

        private DataTable _rawTable;
        private DataTable _branchTable;

        private FlowLayoutPanel _cardsHost;
        private TextBox _txtSearch;
        private ComboBox _cboBranch;
        private ComboBox _cboActive;
        private Label _lblCount;
        private (Panel Card, DataRow Row)? _selectedItem;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnToggleActive;
        private Button _btnRefresh;

        public FrmAssetManager() : this(null, false)
        {
        }

        public FrmAssetManager(int? branchId) : this(branchId, false)
        {
        }

        public FrmAssetManager(int? branchId, bool isStaffMode)
        {
            _presetBranchId = branchId;
            _isStaffMode = isStaffMode;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            Text = "Tài Sản";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1400;
            Height = 800;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);

            // ===== TOOLBAR PANEL WITH TITLE =====
            var pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                Padding = new Padding(0),
                BorderStyle = BorderStyle.None
            };

            // Actions bar
            var pnlActionBar = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                Padding = new Padding(12, 8, 12, 8),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Buttons
            _btnAdd = MakeButton("➕ Thêm", Color.FromArgb(0, 122, 204), async (s, e) => await AddNewAsync());
            _btnEdit = MakeButton("✎ Sửa", Color.FromArgb(0, 122, 204), async (s, e) => await EditSelectedAsync());
            _btnDelete = MakeButton("🗑 Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteSelectedAsync());
            _btnToggleActive = MakeButton("⚙ Bật/Tắt", Color.FromArgb(103, 58, 183), async (s, e) => await ToggleActiveAsync());
            _btnRefresh = MakeButton("⟳ Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadAsync());
            _btnEdit.Enabled = false;
            _btnDelete.Enabled = false;
            _btnToggleActive.Enabled = false;
            _btnDelete.Visible = !_isStaffMode;
            _btnDelete.Enabled = !_isStaffMode && _btnDelete.Enabled;

            var pnlActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            pnlActions.Controls.Add(_btnAdd);
            pnlActions.Controls.Add(_btnEdit);
            pnlActions.Controls.Add(_btnDelete);
            pnlActions.Controls.Add(_btnToggleActive);
            pnlActions.Controls.Add(_btnRefresh);

            // Search and Filters
            _txtSearch = MakeSearchBox(SearchPlaceholder, () => ApplyFilter());
            _cboBranch = new ComboBox
            {
                Width = 180,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9)
            };
            _cboBranch.SelectedIndexChanged += (s, e) => ApplyFilter();

            _cboActive = new ComboBox
            {
                Width = 140,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9)
            };
            _cboActive.Items.AddRange(new object[] { "Tất cả", "🟢 Kích hoạt", "🔴 Đã tắt" });
            _cboActive.SelectedIndex = 0;
            _cboActive.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblCount = new Label
            {
                AutoSize = true,
                Text = "Tổng: 0",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Margin = new Padding(15, 4, 0, 0)
            };

            var pnlFilters = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 3, 0, 0)
            };
            pnlFilters.Controls.Add(new Label { Text = "🔍 Tìm:", AutoSize = true, Margin = new Padding(0, 4, 6, 0), Font = new Font("Segoe UI", 9) });
            pnlFilters.Controls.Add(_txtSearch);
            pnlFilters.Controls.Add(new Label { Text = "Chi nhánh:", AutoSize = true, Margin = new Padding(15, 4, 6, 0), Font = new Font("Segoe UI", 9) });
            pnlFilters.Controls.Add(_cboBranch);
            pnlFilters.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Margin = new Padding(15, 4, 6, 0), Font = new Font("Segoe UI", 9) });
            pnlFilters.Controls.Add(_cboActive);
            pnlFilters.Controls.Add(_lblCount);

            var barLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            barLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            barLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            barLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            pnlActions.Dock = DockStyle.Fill;
            pnlFilters.Dock = DockStyle.Fill;
            pnlActions.Margin = new Padding(0, 0, 0, 6);
            pnlFilters.Margin = new Padding(0);

            barLayout.Controls.Add(pnlActions, 0, 0);
            barLayout.Controls.Add(pnlFilters, 0, 1);

            pnlActionBar.Controls.Add(barLayout);
            pnlToolbar.Controls.Add(pnlActionBar);

            _cardsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = BackColor,
                Padding = new Padding(12)
            };

            // Add all controls
            Controls.Add(_cardsHost);
            Controls.Add(pnlToolbar);

            Load += async (s, e) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                await LoadBranchesAsync();
                _rawTable = await _bll.GetAssetsAsync();
                ApplyAdminBranchScopeToAssets();
                TextFixer.FixDataTable(_rawTable, "AssetName", "Category", "Condition", "Description", "RoomNumber");
                _selectedItem = null;
                ApplyFilter();
                UpdateActionState();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "LoadAssets", "Không thể tải tài sản");
            }
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private async System.Threading.Tasks.Task LoadBranchesAsync()
        {
            try
            {
                var dt = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                TextFixer.FixDataTable(dt, "BranchName");
                _branchTable = new DataTable();
                _branchTable.Columns.Add("BranchId", typeof(int));
                _branchTable.Columns.Add("BranchDisplay", typeof(string));
                _branchTable.Rows.Add(0, "Tất cả");

                if (dt != null && dt.Columns.Contains("BranchId") && dt.Columns.Contains("BranchName"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int id = 0;
                        try { id = Convert.ToInt32(r["BranchId"]); } catch { }
                        string code = r.Table.Columns.Contains("BranchCode") ? r["BranchCode"]?.ToString() : null;
                        string name = r["BranchName"]?.ToString();
                        string display = $"{code} - {name}".Trim(' ', '-');
                        if (string.IsNullOrWhiteSpace(display)) display = "Chi nhánh " + id;
                        _branchTable.Rows.Add(id, display);
                    }
                }

                _cboBranch.DataSource = _branchTable;
                _cboBranch.DisplayMember = "BranchDisplay";
                _cboBranch.ValueMember = "BranchId";
                if (_presetBranchId.HasValue)
                {
                    _cboBranch.SelectedValue = _presetBranchId.Value;
                    _cboBranch.Enabled = false;
                }
            }
            catch
            {
                _cboBranch.Items.Clear();
                _cboBranch.Items.Add("Tất cả");
                _cboBranch.SelectedIndex = 0;
            }
        }

        private void ApplyAdminBranchScopeToAssets()
        {
            if (!AdminBranchScope.IsEnabled) return;
            if (_rawTable == null || !_rawTable.Columns.Contains("BranchId")) return;
            if (_branchTable == null || !_branchTable.Columns.Contains("BranchId")) return;

            var allowed = _branchTable.AsEnumerable()
                .Select(r => r["BranchId"]?.ToString())
                .Where(s => int.TryParse(s, out var id) && id > 0)
                .Select(int.Parse)
                .ToHashSet();

            if (allowed.Count == 0) return;

            var filtered = _rawTable.Clone();
            foreach (DataRow r in _rawTable.Rows)
            {
                if (!int.TryParse(r["BranchId"]?.ToString(), out var bid)) continue;
                if (!allowed.Contains(bid)) continue;
                filtered.ImportRow(r);
            }
            _rawTable = filtered;
        }

        private void RenderCards(DataTable table)
        {
            if (_cardsHost == null) return;
            _cardsHost.SuspendLayout();
            _cardsHost.Controls.Clear();

            if (table == null || table.Rows.Count == 0)
            {
                _cardsHost.ResumeLayout();
                return;
            }

            foreach (DataRow row in table.Rows)
            {
                _cardsHost.Controls.Add(CreateCard(row));
            }

            _cardsHost.ResumeLayout();
        }

        private Control CreateCard(DataRow row)
        {
            string code = ReadString(row, "AssetCode") ?? ReadString(row, "AssetId") ?? "—";
            string name = ReadString(row, "AssetName") ?? "—";
            string category = ReadString(row, "Category") ?? "—";
            string room = ReadString(row, "RoomNumber") ?? ReadString(row, "RoomId") ?? "—";
            string condition = TextFixer.ToVietnameseCondition(ReadString(row, "Condition")) ?? "—";
            string description = ReadString(row, "Description") ?? string.Empty;
            string date = FormatDate(ReadString(row, "PurchaseDate"));
            string price = ReadMoney(row, "PurchasePrice");
            int qty = ReadInt(row, "Quantity");
            bool isActive = TryReadBool(row, "IsActive") ?? true;

            var card = new Panel
            {
                Width = 340,
                Height = 210, // Increased to 210 to ensure all content is visible
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(1),
                Cursor = Cursors.Hand,
                Tag = row
            };

            var statusStrip = new Panel
            {
                Dock = DockStyle.Left,
                Width = 6,
                BackColor = isActive ? Color.SeaGreen : Color.DarkGray
            };

            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 12) }; // Increased bottom padding from 8 to 12

            var lblTitle = new Label
            {
                Text = $"{code} • {name}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                AutoSize = false,
                Width = 240, // Reduced to make room for status
                Height = 22,
                Location = new Point(0, 0),
                AutoEllipsis = true
            };

            var lblStatus = new Label
            {
                Text = isActive ? "[Kích hoạt]" : "[Đã tắt]",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = isActive ? Color.SeaGreen : Color.DimGray,
                AutoSize = true,
                Location = new Point(245, 2) // Positioned to the right of title
            };

            var lblCategory = new Label
            {
                Text = $"Nhóm: {category} | Phòng: {room}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = false,
                Width = 300,
                Height = 20,
                Location = new Point(0, 26),
                AutoEllipsis = true
            };

            var lblQuantity = new Label
            {
                Text = $"SL: {qty} | Giá mua: {price}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = false,
                Width = 300,
                Height = 20,
                Location = new Point(0, 48),
                AutoEllipsis = true
            };

            var lblDate = new Label
            {
                Text = $"Ngày mua: {date}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.DimGray,
                AutoSize = false,
                Width = 300,
                Height = 20,
                Location = new Point(0, 70)
            };

            var lblCondition = new Label
            {
                Text = $"Tình trạng: {condition}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(90, 90, 90),
                AutoSize = false,
                Width = 300,
                Height = 20,
                Location = new Point(0, 92),
                AutoEllipsis = true
            };

            var lblDescription = new Label
            {
                Text = $"Mô tả: {description}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(90, 90, 90),
                AutoSize = false,
                Width = 310,
                Height = 50,
                Location = new Point(0, 114),
                AutoEllipsis = true,
                MaximumSize = new Size(310, 50)
            };

            content.Controls.Add(lblTitle);
            content.Controls.Add(lblStatus);
            content.Controls.Add(lblCategory);
            content.Controls.Add(lblQuantity);
            content.Controls.Add(lblDate);
            content.Controls.Add(lblCondition);
            content.Controls.Add(lblDescription);

            var border = new Panel { Dock = DockStyle.Fill };
            border.Paint += (s, e) =>
            {
                using (var pen = new Pen(IsSelected(card) ? Color.FromArgb(0, 122, 204) : Color.FromArgb(220, 230, 240), 1.4f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            border.Controls.Add(content);
            card.Controls.Add(border);
            card.Controls.Add(statusStrip);

            void SelectAction()
            {
                SelectCard(card, row);
            }

            card.Click += (s, e) => SelectAction();
            foreach (Control c in content.Controls) c.Click += (s, e) => SelectAction();
            card.DoubleClick += async (s, e) => await EditSelectedAsync();
            foreach (Control c in content.Controls) c.DoubleClick += async (s, e) => await EditSelectedAsync();

            return card;
        }

        private void SelectCard(Panel card, DataRow row)
        {
            if (_selectedItem.HasValue && _selectedItem.Value.Card != null)
                _selectedItem.Value.Card.Invalidate();
            _selectedItem = (card, row);
            UpdateActionState();
            card.Invalidate();
        }

        private bool IsSelected(Panel card)
        {
            return _selectedItem.HasValue && ReferenceEquals(_selectedItem.Value.Card, card);
        }

        private void UpdateActionState()
        {
            bool has = _selectedItem.HasValue;
            _btnEdit.Enabled = has;
            _btnDelete.Enabled = has;
            _btnToggleActive.Enabled = has;
        }

        private void ApplyFilter()
        {
            if (_rawTable == null) return;

            string rawKeyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholder) rawKeyword = string.Empty;
            string keyword = rawKeyword.ToLowerInvariant();

            int branchId = _cboBranch.SelectedValue is int b ? b : 0;
            int activeChoice = _cboActive.SelectedIndex; // 0 all, 1 active, 2 inactive

            var rows = _rawTable.AsEnumerable();

            if (branchId > 0 && _rawTable.Columns.Contains("BranchId"))
                rows = rows.Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == branchId);

            if (activeChoice != 0 && _rawTable.Columns.Contains("IsActive"))
            {
                bool want = activeChoice == 1;
                rows = rows.Where(r =>
                {
                    if (r["IsActive"] == DBNull.Value) return false;
                    try { return Convert.ToBoolean(r["IsActive"]) == want; } catch { return false; }
                });
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rows = rows.Where(r =>
                    Contains(r, "AssetCode", keyword) ||
                    Contains(r, "AssetName", keyword) ||
                    Contains(r, "Category", keyword) ||
                    Contains(r, "RoomNumber", keyword));
            }

            var filtered = rows.Any() ? rows.CopyToDataTable() : _rawTable.Clone();
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
            RenderCards(filtered);
            _selectedItem = null;
            UpdateActionState();
        }

        private async System.Threading.Tasks.Task AddNewAsync()
        {
            using (var frm = new FrmAssetEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadAsync();
                    AdminEvents.NotifyDataChanged();
                    DataSyncManager.NotifyRoomsChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task EditSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một dòng để sửa");
                return;
            }

            using (var frm = new FrmAssetEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadAsync();
                    AdminEvents.NotifyDataChanged();
                    DataSyncManager.NotifyRoomsChanged();
                }
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedAsync()
        {
            if (_isStaffMode) return;
            var row = GetCurrentRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một dòng để xóa");
                return;
            }

            int id = ReadInt(row, "AssetId");
            string code = ReadString(row, "AssetCode") ?? id.ToString();

            if (!ModernConfirmDialog.ConfirmDanger($"Xóa tài sản \"{code}\"?")) return;

            try
            {
                await _bll.DeleteAssetAsync(id);
                ToastNotification.Success("Xóa tài sản thành công");
                await LoadAsync();
                AdminEvents.NotifyDataChanged();
                DataSyncManager.NotifyRoomsChanged();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "DeleteAsset", "Lỗi xóa tài sản");
            }
        }

        private async System.Threading.Tasks.Task ToggleActiveAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                ToastNotification.Warning("Chọn một dòng để bật/tắt");
                return;
            }

            int id = ReadInt(row, "AssetId");
            string name = ReadString(row, "AssetName") ?? id.ToString();
            bool current = TryReadBool(row, "IsActive") ?? true;
            bool next = !current;

            if (!ModernConfirmDialog.Confirm($"Chuyển \"{name}\" sang {(next ? "Kích hoạt" : "Đã tắt")}?", "Xác nhận")) return;

            try
            {
                await _bll.UpdateAssetAsync(
                    id,
                    ReadString(row, "AssetCode"),
                    ReadString(row, "AssetName"),
                    ReadString(row, "Category"),
                    TryReadIntNullable(row, "RoomId"),
                    ReadInt(row, "Quantity"),
                    ReadString(row, "Condition"),
                    TryReadDate(row, "PurchaseDate"),
                    TryReadDecimalNullable(row, "PurchasePrice"),
                    ReadString(row, "Description"),
                    next);

                ToastNotification.Success($"Đã {(next ? "kích hoạt" : "tắt")} tài sản");
                await LoadAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "ToggleAsset", "Lỗi cập nhật trạng thái");
            }
        }

        private DataRow GetCurrentRow()
        {
            return _selectedItem?.Row;
        }

        private static Button MakeButton(string text, Color backColor, EventHandler onClick)
        {
            var b = new ModernButton
            {
                Text = text,
                Width = 110,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BaseColor = backColor,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 6, 0),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ColorAdjust(backColor, 10);
            b.Click += onClick;
            return b;
        }

        private static Color ColorAdjust(Color c, int delta)
        {
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, c.R + delta)),
                Math.Max(0, Math.Min(255, c.G + delta)),
                Math.Max(0, Math.Min(255, c.B + delta))
            );
        }

        private static TextBox MakeSearchBox(string placeholder, Action onChanged)
        {
            var tb = new TextBox
            {
                Width = 280,
                Height = 32,
                ForeColor = Color.Gray,
                Text = placeholder,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };
            tb.GotFocus += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = string.Empty;
                    tb.ForeColor = Color.Black;
                }
            };
            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.ForeColor = Color.Gray;
                }
            };
            tb.TextChanged += (s, e) => onChanged?.Invoke();
            return tb;
        }

        private static string FormatDate(string raw)
        {
            if (DateTime.TryParse(raw, out var dt))
                return dt.ToString("dd/MM/yyyy");
            return "—";
        }

        private static string ReadMoney(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
            {
                if (!row.Table.Columns.Contains(c)) continue;
                var v = row[c];
                if (v == null || v == DBNull.Value) continue;
                if (decimal.TryParse(v.ToString(), out var d)) return d.ToString("N0");
                try { return Convert.ToDecimal(v).ToString("N0"); } catch { }
            }
            return "0";
        }

        private static bool Contains(DataRow row, string column, string keywordLower)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column)) return false;
            var v = row[column];
            if (v == null || v == DBNull.Value) return false;
            return v.ToString().ToLowerInvariant().Contains(keywordLower);
        }

        private static string ReadString(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v != null && v != DBNull.Value) return v.ToString();
                }
            }
            return null;
        }

        private static int ReadInt(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (int.TryParse(v.ToString(), out var i)) return i;
                    try { return Convert.ToInt32(v); } catch { }
                }
            }
            return 0;
        }

        private static int? TryReadIntNullable(DataRow row, params string[] cols)
        {
            int i = ReadInt(row, cols);
            return i > 0 ? (int?)i : null;
        }

        private static bool? TryReadBool(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (bool.TryParse(v.ToString(), out var b)) return b;
                    try { return Convert.ToBoolean(v); } catch { }
                }
            }
            return null;
        }

        private static DateTime? TryReadDate(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (DateTime.TryParse(v.ToString(), out var d)) return d.Date;
                    try
                    {
                        if (v is DateTime dt) return dt.Date;
                    }
                    catch { }
                }
            }
            return null;
        }

        private static decimal? TryReadDecimalNullable(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
            {
                if (row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (decimal.TryParse(v.ToString(), out var d)) return d;
                    try { return Convert.ToDecimal(v); } catch { }
                }
            }
            return null;
        }
    }
}
