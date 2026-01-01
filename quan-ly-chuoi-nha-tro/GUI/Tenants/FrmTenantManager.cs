using System;
using System.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Màn hình quản lý khách thuê - Hiển thị cards, hỗ trợ tìm kiếm, thêm, sửa, chi tiết và xóa.
    /// </summary>
    public class FrmTenantManager : Form
    {
        private const string SearchPlaceholder = "Tìm tên/CCCD/SĐT...";

        private readonly AdminDataBLL _bll;
        private readonly int? _branchId;

        private DataTable _tenants;
        private DataTable _history;
        private DataTable _rooms;

        private FlowLayoutPanel _tenantCardsHost;
        private TextBox _txtSearch;
        private Label _lblTotal;
        
        // Selected tenant tracking
        private int _selectedTenantId = 0;

        public FrmTenantManager(AdminDataBLL bll, int? branchId = null)
        {
            _bll = bll ?? new AdminDataBLL();
            _branchId = branchId;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        public FrmTenantManager() : this(new AdminDataBLL(), null)
        {
        }

        private void InitializeComponent()
        {
            Text = "Quản lý khách thuê";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Width = 1100;
            Height = 650;

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White
            };

            var searchLabel = new Label { Text = "Tìm:", AutoSize = true, Margin = new Padding(0, 8, 6, 0) };
            _txtSearch = new TextBox { Width = 260, Text = SearchPlaceholder, ForeColor = Color.Gray, Margin = new Padding(0, 4, 12, 0) };
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
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) ApplySearch(); };

            var btnSearch = MakeButton("Tìm", Color.FromArgb(0, 122, 204), (s, e) => ApplySearch());
            var btnAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), (s, e) => AddTenant());
            var btnEdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), (s, e) => EditTenant());
            var btnDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), (s, e) => DeleteTenant());
            var btnRefresh = MakeButton("Làm mới", Color.FromArgb(40, 167, 69), async (s, e) => await LoadDataAsync());

            _lblTotal = new Label { Text = "Tổng: 0", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(0, 122, 204), Margin = new Padding(12, 8, 0, 0) };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            actions.Controls.Add(searchLabel);
            actions.Controls.Add(_txtSearch);
            actions.Controls.Add(btnSearch);
            actions.Controls.Add(btnAdd);
            actions.Controls.Add(btnEdit);
            
            // Chỉ hiển thị nút Xóa cho Admin (không phải Staff)
            if (_branchId == null)
            {
                actions.Controls.Add(btnDelete);
            }
            
            actions.Controls.Add(btnRefresh);
            actions.Controls.Add(_lblTotal);

            toolbar.Controls.Add(actions);

            _tenantCardsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(6)
            };

            Controls.Add(_tenantCardsHost);
            Controls.Add(toolbar);

            Load += async (s, e) => await LoadDataAsync();
        }

        private DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            return grid;
        }

        private Button MakeButton(string text, Color backColor, EventHandler onClick)
        {
            var btn = new ModernButton
            {
                Text = text,
                Width = 90,
                Height = 32,
                BaseColor = backColor,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 8, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                _tenants = await _bll.GetTenantsAsync() ?? new DataTable();
                _history = await _bll.GetTenantHistoryAsync() ?? new DataTable();
                _rooms = await _bll.GetRoomsAsync() ?? new DataTable();

                if (_branchId.HasValue && _tenants.Columns.Contains("BranchId"))
                {
                    var filtered = _tenants.AsEnumerable()
                        .Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == _branchId.Value);
                    _tenants = filtered.Any() ? filtered.CopyToDataTable() : _tenants.Clone();
                }

                _lblTotal.Text = $"Tổng: {_tenants.Rows.Count}";
                ApplySearch();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "LoadTenants", "Không thể tải dữ liệu khách thuê");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ApplySearch()
        {
            if (_tenants == null) return;

            string keyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (keyword == SearchPlaceholder) keyword = string.Empty;

            var view = new DataView(_tenants);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var escaped = keyword.Replace("'", "''");
                view.RowFilter = $"Convert(FullName, 'System.String') LIKE '%{escaped}%' OR Convert(IdentityCard, 'System.String') LIKE '%{escaped}%' OR Convert(PhoneNumber, 'System.String') LIKE '%{escaped}%'";
            }
            else
            {
                view.RowFilter = string.Empty;
            }

            var filtered = view.ToTable();
            _lblTotal.Text = $"Tổng: {filtered.Rows.Count}";
            RenderTenantCards(filtered);
        }

        private void RenderTenantCards(DataTable tenants)
        {
            if (_tenantCardsHost == null) return;
            _tenantCardsHost.SuspendLayout();
            _tenantCardsHost.Controls.Clear();

            if (tenants == null || tenants.Rows.Count == 0)
            {
                _tenantCardsHost.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = "Chưa có khách thuê.",
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Margin = new Padding(8, 6, 0, 0)
                });
                _tenantCardsHost.ResumeLayout();
                return;
            }

            foreach (DataRow row in tenants.Rows)
            {
                var card = CreateTenantCard(row);
                _tenantCardsHost.Controls.Add(card);
            }

            _tenantCardsHost.ResumeLayout();
        }

        private Control CreateTenantCard(DataRow tenantRow)
        {
            var panel = new Panel
            {
                Width = 240,
                Height = 140,
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(12),
                Cursor = Cursors.Hand
            };
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 220, 230)))
                {
                    var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            int tenantId = TryReadInt(tenantRow, "TenantId");
            string name = TextFixer.FixUtf8Mojibake(ReadString(tenantRow, "FullName") ?? "");
            string phone = ReadString(tenantRow, "PhoneNumber");
            string identity = ReadString(tenantRow, "IdentityCard");
            string email = ReadString(tenantRow, "Email");
            string address = TextFixer.FixUtf8Mojibake(ReadString(tenantRow, "Address"));
            string status = (tenantRow.Table.Columns.Contains("IsActive") && bool.TryParse(tenantRow["IsActive"]?.ToString(), out var active) && active) ? "Đang ở" : "Tạm ngưng";

            var lblName = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Text = $"{tenantId} · {NullDash(name)}",
                ForeColor = Color.FromArgb(0, 79, 159)
            };

            var lblInfo = new Label
            {
                Dock = DockStyle.Top,
                Height = 46,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70),
                Text = $"☎ {NullDash(phone)}   |   CCCD: {NullDash(identity)}\n✉ {NullDash(email)}",
                AutoSize = false
            };

            var lblAddress = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = $"🏠 {NullDash(address)}",
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = Color.FromArgb(90, 90, 90)
            };

            var lblStatus = new Label
            {
                AutoSize = true,
                Text = status,
                BackColor = status.Contains("Đang") ? Color.FromArgb(232, 247, 239) : Color.FromArgb(252, 236, 238),
                ForeColor = status.Contains("Đang") ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding = new Padding(6, 2, 6, 2),
                Location = new Point(12, panel.Height - 28)
            };

            panel.Controls.Add(lblStatus);
            panel.Controls.Add(lblAddress);
            panel.Controls.Add(lblInfo);
            panel.Controls.Add(lblName);

            // Click handler - open edit form with full info
            EventHandler onClick = (s, e) =>
            {
                _selectedTenantId = tenantId;
                OpenTenantEditor();
            };
            panel.Click += onClick;
            foreach (Control ctl in panel.Controls)
            {
                ctl.Click += onClick;
            }

            // Right-click context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Xem chi tiết", null, (s, e) =>
            {
                _selectedTenantId = tenantId;
                ShowDetail();
            });
            contextMenu.Items.Add("Sửa thông tin", null, (s, e) =>
            {
                _selectedTenantId = tenantId;
                EditTenant();
            });
            
            // Chỉ thêm "Xóa" nếu là Admin
            if (_branchId == null)
            {
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add("Xóa", null, (s, e) =>
                {
                    _selectedTenantId = tenantId;
                    DeleteTenant();
                });
            }

            panel.ContextMenuStrip = contextMenu;
            foreach (Control ctl in panel.Controls)
            {
                ctl.ContextMenuStrip = contextMenu;
            }

            return panel;
        }



        private DataRow GetSelectedTenant()
        {
            if (_selectedTenantId <= 0 || _tenants == null) return null;
            return FindById(_tenants, "TenantId", _selectedTenantId);
        }

        private void ShowDetail()
        {
            var row = GetSelectedTenant();
            if (row == null)
            {
                ToastNotification.Warning("Chọn khách thuê trước");
                return;
            }

            using (var frm = new FrmTenantDetail(_bll, row))
            {
                frm.ShowDialog(this);
            }
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadDataAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private void OpenTenantEditor()
        {
            var row = GetSelectedTenant();
            if (row == null)
            {
                ToastNotification.Warning("Chọn khách thuê trước");
                return;
            }

            using (var frm = new FrmTenantEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadDataAsync();
                }
            }
        }

        private string ReadString(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return null;
            var v = r[col];
            if (v == DBNull.Value || v == null) return null;
            return TextFixer.ForceFixUtf8Mojibake(v.ToString());
        }

        private static int TryReadInt(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return 0;
            if (int.TryParse(r[col]?.ToString(), out var v)) return v;
            return 0;
        }

        private static string NullDash(string s) => string.IsNullOrWhiteSpace(s) ? "—" : s.Trim();

        private static DataRow FindById(DataTable dt, string col, int id)
        {
            if (dt == null || !dt.Columns.Contains(col)) return null;
            foreach (DataRow r in dt.Rows)
            {
                if (int.TryParse(r[col]?.ToString(), out var currentId) && currentId == id) return r;
            }
            return null;
        }

        private void AddTenant()
        {
            using (var frm = new FrmTenantEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadDataAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private void EditTenant()
        {
            var row = GetSelectedTenant();
            if (row == null)
            {
                ToastNotification.Warning("Chọn khách thuê trước");
                return;
            }

            using (var frm = new FrmTenantEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadDataAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private void DeleteTenant()
        {
            var candidates = GetFilteredTenantsForDelete();
            if (candidates.Count == 0)
            {
                ToastNotification.Warning("Không có khách thuê để xóa");
                return;
            }

            using (var picker = new TenantDeletePicker(candidates, _selectedTenantId))
            {
                if (picker.ShowDialog(this) != DialogResult.OK) return;
                var selected = picker.SelectedRows;
                if (selected == null || selected.Count == 0)
                {
                    ToastNotification.Warning("Chưa chọn khách thuê cần xóa");
                    return;
                }

                if (!ModernConfirmDialog.ConfirmDanger($"Xóa {selected.Count} khách thuê?")) return;

                _ = DeleteTenantsAsync(selected);
            }
        }

        private async Task DeleteTenantsAsync(List<DataRow> rows)
        {
            try
            {
                int deleted = 0;
                var failures = new List<string>();
                var deletedIds = new HashSet<int>();

                foreach (var row in rows)
                {
                    int id = TryGetInt(row, "TenantId");
                    if (id <= 0 || deletedIds.Contains(id)) continue;
                    try
                    {
                        bool ok = await _bll.DeleteTenantAsync(id);
                        if (ok) deleted++;
                        else failures.Add(SafeToString(row, "FullName") ?? id.ToString());
                        deletedIds.Add(id);
                    }
                    catch (Exception ex)
                    {
                        var label = SafeToString(row, "FullName") ?? id.ToString();
                        failures.Add($"{label}: {ex.Message}");
                    }
                }

                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
                DataSyncManager.NotifyTenantsChanged();
                DataSyncManager.NotifyRoomsChanged();
                DataSyncManager.NotifyContractsChanged();
                DataSyncManager.NotifyInvoicesChanged();
                DataSyncManager.NotifyPaymentsChanged();

                if (failures.Count > 0)
                {
                    ToastNotification.Warning($"Một số khách thuê không xóa được: {failures.Count}");
                }
                else if (deleted > 0)
                {
                    ToastNotification.Success($"Đã xóa {deleted} khách thuê");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "DeleteTenants", "Lỗi xóa khách thuê");
            }
        }

        private List<DataRow> GetFilteredTenantsForDelete()
        {
            if (_tenants == null) return new List<DataRow>();

            string keyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (keyword == SearchPlaceholder) keyword = string.Empty;

            var view = new DataView(_tenants);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var escaped = keyword.Replace("'", "''");
                view.RowFilter = $"Convert(FullName, 'System.String') LIKE '%{escaped}%' OR Convert(IdentityCard, 'System.String') LIKE '%{escaped}%' OR Convert(PhoneNumber, 'System.String') LIKE '%{escaped}%'";
            }
            else
            {
                view.RowFilter = string.Empty;
            }

            var rows = view.ToTable().AsEnumerable().ToList();
            if (_tenants.Columns.Contains("TenantId"))
                rows = rows.GroupBy(r => TryGetInt(r, "TenantId")).Select(g => g.First()).ToList();
            return rows;
        }

        private static int TryGetInt(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column)) return 0;
            return int.TryParse(row[column]?.ToString(), out var val) ? val : 0;
        }

        private static string SafeToString(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column)) return null;
            var v = row[column];
            return v == null || v == DBNull.Value ? null : v.ToString();
        }

        private sealed class TenantDeletePicker : Form
        {
            private readonly CheckedListBox _list;
            private readonly Label _lblCount;
            public List<DataRow> SelectedRows { get; private set; }

            public TenantDeletePicker(List<DataRow> rows, int preselectedTenantId)
            {
                Text = "Chọn khách thuê cần xóa";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(520, 420);
                BackColor = Color.White;
                Font = new Font("Segoe UI", 10F);

                var lbl = new Label
                {
                    Text = "Chọn khách thuê:",
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
                    string name = SafeToString(row, "FullName") ?? "N/A";
                    string phone = SafeToString(row, "PhoneNumber") ?? "";
                    string cccd = SafeToString(row, "IdentityCard") ?? "";
                    int id = TryGetInt(row, "TenantId");
                    string label = $"{id} - {name}";
                    if (!string.IsNullOrWhiteSpace(phone) || !string.IsNullOrWhiteSpace(cccd))
                        label = $"{label} | {phone} | {cccd}";
                    int index = _list.Items.Add(new ListItem(label, row), false);
                    if (preselectedTenantId > 0 && preselectedTenantId == id)
                        _list.SetItemChecked(index, true);
                }

                _lblCount = new Label
                {
                    Text = $"Tổng: {rows.Count}",
                    AutoSize = true,
                    Location = new Point(14, 346),
                    ForeColor = Color.FromArgb(80, 80, 80)
                };

                var btnSelectAll = new ModernButton
                {
                    Text = "Chọn tất cả",
                    Width = 110,
                    Height = 28,
                    FlatStyle = FlatStyle.Flat,
                    BaseColor = Color.White,
                    BackColor = Color.Transparent,
                    ForeColor = Color.Black,
                    Location = new Point(230, 342)
                };
                btnSelectAll.FlatAppearance.BorderSize = 1;
                btnSelectAll.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
                btnSelectAll.Click += (s, e) =>
                {
                    for (int i = 0; i < _list.Items.Count; i++)
                        _list.SetItemChecked(i, true);
                };

                var btnClear = new ModernButton
                {
                    Text = "Bỏ chọn",
                    Width = 90,
                    Height = 28,
                    FlatStyle = FlatStyle.Flat,
                    BaseColor = Color.White,
                    BackColor = Color.Transparent,
                    ForeColor = Color.Black,
                    Location = new Point(346, 342)
                };
                btnClear.FlatAppearance.BorderSize = 1;
                btnClear.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
                btnClear.Click += (s, e) =>
                {
                    for (int i = 0; i < _list.Items.Count; i++)
                        _list.SetItemChecked(i, false);
                };

                var btnOk = new ModernButton
                {
                    Text = "Xóa",
                    Width = 100,
                    Height = 32,
                    BaseColor = Color.FromArgb(220, 53, 69),
                    BackColor = Color.Transparent,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(296, 360),
                    DialogResult = DialogResult.OK
                };
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Click += (s, e) => CollectSelection();

                var btnCancel = new ModernButton
                {
                    Text = "Hủy",
                    Width = 90,
                    Height = 32,
                    BaseColor = Color.FromArgb(220, 220, 220),
                    BackColor = Color.Transparent,
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

    }
}
