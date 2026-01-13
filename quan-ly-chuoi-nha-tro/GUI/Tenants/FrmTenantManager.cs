using System;
using System.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private Dictionary<int, string> _roomMap;

        private FlowLayoutPanel _tenantCardsHost;
        private ModernSearchBox _txtSearch;
        private Label _lblTotal;
        private Label _lblActive;
        private Label _lblInactive;
        private Button _btnExportTemp;
        
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
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);
            Width = 1100;
            Height = 650;
            DoubleBuffered = true;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                Padding = new Padding(18, 12, 18, 10),
                BackColor = Color.White
            };
            header.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(230, 235, 242)))
                {
                    e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
                }
            };

            var headerStack = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                Margin = new Padding(0)
            };

            var lblTitle = new Label
            {
                Text = "Khách thuê",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 40, 80)
            };

            var lblSubtitle = new Label
            {
                Text = "Quản lý hồ sơ, thông tin liên hệ và trạng thái",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.2f),
                ForeColor = Color.FromArgb(110, 120, 140),
                Margin = new Padding(0, 2, 0, 0)
            };

            headerStack.Controls.Add(lblTitle);
            headerStack.Controls.Add(lblSubtitle);

            _lblTotal = new Label
            {
                Text = "0 khách",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 98, 186),
                BackColor = Color.FromArgb(235, 242, 252),
                Padding = new Padding(10, 4, 10, 4),
                Dock = DockStyle.Right
            };

            header.Controls.Add(_lblTotal);
            header.Controls.Add(headerStack);

            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(18, 12, 18, 18)
            };

            var toolbarCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                Padding = new Padding(12, 12, 12, 12),
                BackColor = Color.White
            };
            ApplyCardStyle(toolbarCard);

            var searchLabel = new Label
            {
                Text = "Tìm kiếm",
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 105, 125),
                Margin = new Padding(0, 6, 8, 0)
            };
            _txtSearch = new ModernSearchBox
            {
                Width = 260,
                PlaceholderText = SearchPlaceholder,
                Margin = new Padding(0, 3, 14, 0)
            };
            _txtSearch.SearchTriggered += (s, e) => ApplySearch();

            var btnSearch = MakeButton("Tìm", Color.FromArgb(0, 98, 186), (s, e) => ApplySearch(), 70);
            var btnAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), (s, e) => AddTenant(), 78);
            var btnEdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), (s, e) => EditTenant(), 78);
            var btnDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), (s, e) => DeleteTenant(), 78);
            var btnRefresh = MakeButton("Làm mới", Color.FromArgb(46, 164, 79), async (s, e) => await LoadDataAsync(), 92);
            _btnExportTemp = MakeButton("Xuất Excel", Color.FromArgb(0, 176, 80), (s, e) => ExportTemporaryResidence(), 104);

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
            
            actions.Controls.Add(_btnExportTemp);
            actions.Controls.Add(btnRefresh);

            var summaryPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0)
            };

            _lblActive = CreateStatChip("Đang ở: 0", Color.FromArgb(232, 247, 239), Color.FromArgb(40, 167, 69));
            _lblInactive = CreateStatChip("Tạm ngưng: 0", Color.FromArgb(252, 236, 238), Color.FromArgb(220, 53, 69));

            summaryPanel.Controls.Add(_lblActive);
            summaryPanel.Controls.Add(_lblInactive);

            toolbarCard.Controls.Add(summaryPanel);
            toolbarCard.Controls.Add(actions);

            _tenantCardsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(6, 6, 6, 10)
            };

            content.Controls.Add(_tenantCardsHost);
            content.Controls.Add(toolbarCard);

            Controls.Add(content);
            Controls.Add(header);

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

        private Button MakeButton(string text, Color backColor, EventHandler onClick, int width = 90)
        {
            var btn = new ModernButton
            {
                Text = text,
                Width = width,
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
                _roomMap = BuildRoomMap();

                if (_branchId.HasValue && _tenants.Columns.Contains("BranchId"))
                {
                    var filtered = _tenants.AsEnumerable()
                        .Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == _branchId.Value);
                    _tenants = filtered.Any() ? filtered.CopyToDataTable() : _tenants.Clone();
                }

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
            UpdateSummary(filtered);
            RenderTenantCards(filtered);
        }

        private void ExportTemporaryResidence()
        {
            if (_tenants == null || _tenants.Rows.Count == 0)
            {
                ToastNotification.Warning("Không có dữ liệu để xuất");
                return;
            }

            var filtered = GetFilteredTenantsForExport();
            var report = BuildTemporaryResidenceTable(filtered);
            ExcelExporter.ExportToExcelHtml(report, "Danh sách tạm trú/tạm vắng");
        }

        private DataTable GetFilteredTenantsForExport()
        {
            var view = new DataView(_tenants);
            string keyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (keyword == SearchPlaceholder) keyword = string.Empty;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string escaped = keyword.Replace("'", "''");
                view.RowFilter = $"Convert(FullName, 'System.String') LIKE '%{escaped}%' " +
                                 $"OR Convert(IdentityCard, 'System.String') LIKE '%{escaped}%' " +
                                 $"OR Convert(PhoneNumber, 'System.String') LIKE '%{escaped}%'";
            }
            else
            {
                view.RowFilter = string.Empty;
            }
            return view.ToTable();
        }

        private DataTable BuildTemporaryResidenceTable(DataTable tenants)
        {
            var table = new DataTable("TamTru_TamVang");
            table.Columns.Add("Họ tên");
            table.Columns.Add("CCCD/CMND");
            table.Columns.Add("SĐT");
            table.Columns.Add("Email");
            table.Columns.Add("Phòng");
            table.Columns.Add("Tạm trú tại");
            table.Columns.Add("Ngày đăng ký tạm trú");
            table.Columns.Add("Hạn tạm trú");
            table.Columns.Add("Ngày vào");
            table.Columns.Add("Ngày ra");
            table.Columns.Add("Trạng thái");

            var roomMap = BuildRoomMap();

            foreach (DataRow tenant in tenants.Rows)
            {
                int tenantId = TryReadInt(tenant, "TenantId");
                var historyRow = FindLatestHistoryRow(tenantId);

                string roomNumber = historyRow != null
                    ? ReadString(roomMap, TryReadInt(historyRow, "RoomId"))
                    : string.Empty;

                string checkIn = FormatDate(ReadString(historyRow, "CheckInDate"));
                string checkOut = FormatDate(ReadString(historyRow, "CheckOutDate"));

                string status = ReadString(tenant, "IsActive");
                if (!string.IsNullOrWhiteSpace(status))
                    status = status.Equals("True", StringComparison.OrdinalIgnoreCase) ? "Đang hoạt động" : "Ngừng";

                table.Rows.Add(
                    ReadString(tenant, "FullName"),
                    ReadString(tenant, "IdentityCard"),
                    ReadString(tenant, "PhoneNumber"),
                    ReadString(tenant, "Email"),
                    roomNumber,
                    ReadString(tenant, "TemporaryRegistration"),
                    FormatDate(ReadString(tenant, "TemporaryRegistrationDate")),
                    FormatDate(ReadString(tenant, "TemporaryRegistrationExpiry")),
                    checkIn,
                    checkOut,
                    string.IsNullOrWhiteSpace(status) ? "—" : status
                );
            }

            return table;
        }

        private Dictionary<int, string> BuildRoomMap()
        {
            var map = new Dictionary<int, string>();
            if (_rooms == null || !_rooms.Columns.Contains("RoomId")) return map;
            foreach (DataRow row in _rooms.Rows)
            {
                if (!int.TryParse(row["RoomId"]?.ToString(), out var id)) continue;
                string roomNumber = _rooms.Columns.Contains("RoomNumber") ? row["RoomNumber"]?.ToString() : null;
                map[id] = roomNumber ?? id.ToString();
            }
            return map;
        }

        private DataRow FindLatestHistoryRow(int tenantId)
        {
            if (_history == null || !_history.Columns.Contains("TenantId")) return null;
            var rows = _history.AsEnumerable()
                .Where(r => TryReadInt(r, "TenantId") == tenantId);
            if (!rows.Any()) return null;
            return rows
                .OrderByDescending(r => ReadDate(r, "CheckInDate") ?? ReadDate(r, "CreatedDate") ?? DateTime.MinValue)
                .FirstOrDefault();
        }

        private static string ReadString(Dictionary<int, string> map, int key)
        {
            return map != null && map.TryGetValue(key, out var val) ? val : string.Empty;
        }

        private static DateTime? ReadDate(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column)) return null;
            if (DateTime.TryParse(row[column]?.ToString(), out var dt)) return dt;
            return null;
        }

        private static string FormatDate(string raw)
        {
            if (DateTime.TryParse(raw, out var dt))
                return dt.ToString("dd/MM/yyyy");
            return string.Empty;
        }

        private void RenderTenantCards(DataTable tenants)
        {
            if (_tenantCardsHost == null) return;
            _tenantCardsHost.SuspendLayout();
            _tenantCardsHost.Controls.Clear();

            if (tenants == null || tenants.Rows.Count == 0)
            {
                var emptyPanel = new Panel
                {
                    Width = 420,
                    Height = 120,
                    BackColor = Color.White,
                    Margin = new Padding(10),
                    Padding = new Padding(16)
                };
                ApplyCardStyle(emptyPanel);
                emptyPanel.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = "Chưa có khách thuê",
                    Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(40, 50, 70),
                    Location = new Point(16, 18)
                });
                emptyPanel.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = "Hãy thêm mới hoặc điều chỉnh bộ lọc tìm kiếm.",
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    Location = new Point(16, 46)
                });
                _tenantCardsHost.Controls.Add(emptyPanel);
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
            bool isActive = tenantRow.Table.Columns.Contains("IsActive") &&
                            bool.TryParse(tenantRow["IsActive"]?.ToString(), out var active) && active;
            var panel = new Panel
            {
                Width = 320,
                Height = 186,
                BackColor = Color.White,
                Margin = new Padding(10),
                Cursor = Cursors.Hand
            };
            ApplyCardStyle(panel);

            int tenantId = TryReadInt(tenantRow, "TenantId");
            string name = TextFixer.FixUtf8Mojibake(ReadString(tenantRow, "FullName") ?? "");
            string phone = ReadString(tenantRow, "PhoneNumber");
            string identity = ReadString(tenantRow, "IdentityCard");
            string email = ReadString(tenantRow, "Email");
            string address = TextFixer.FixUtf8Mojibake(ReadString(tenantRow, "Address"));
            string status = isActive ? "Đang ở" : "Tạm ngưng";
            string roomNumber = GetRoomNumberForTenant(tenantId);

            var accent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = isActive ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69)
            };

            var contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 12, 12, 10)
            };

            var headerRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44
            };

            var avatar = CreateAvatarBadge(name, isActive);
            avatar.Dock = DockStyle.Left;

            var nameStack = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 2, 0, 0)
            };

            var lblName = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                Font = new Font("Segoe UI Semibold", 10.2f, FontStyle.Bold),
                Text = NullDash(name),
                ForeColor = Color.FromArgb(24, 40, 80)
            };

            var lblMeta = new Label
            {
                Dock = DockStyle.Top,
                Height = 18,
                Font = new Font("Segoe UI", 8.6f),
                ForeColor = Color.FromArgb(120, 130, 150),
                Text = $"ID {tenantId} · CCCD {NullDash(identity)}"
            };

            var lblStatus = CreateStatusBadge(status, isActive);
            lblStatus.Dock = DockStyle.Right;

            nameStack.Controls.Add(lblMeta);
            nameStack.Controls.Add(lblName);

            headerRow.Controls.Add(nameStack);
            headerRow.Controls.Add(lblStatus);
            headerRow.Controls.Add(avatar);

            var lblContact = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(70, 80, 100),
                Text = $"☎ {NullDash(phone)}\n✉ {NullDash(email)}",
                AutoSize = false
            };

            var lblAddress = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = $"🏠 {NullDash(address)}",
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = Color.FromArgb(90, 100, 120)
            };

            var footerRow = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 24
            };

            var lblRoom = new Label
            {
                AutoSize = true,
                Text = string.IsNullOrWhiteSpace(roomNumber) ? "Chưa gán phòng" : $"Phòng {roomNumber}",
                Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 98, 186),
                BackColor = Color.FromArgb(234, 242, 252),
                Padding = new Padding(8, 2, 8, 2),
                Location = new Point(0, 2)
            };

            footerRow.Controls.Add(lblRoom);

            contentHost.Controls.Add(footerRow);
            contentHost.Controls.Add(lblAddress);
            contentHost.Controls.Add(lblContact);
            contentHost.Controls.Add(headerRow);

            panel.Controls.Add(contentHost);
            panel.Controls.Add(accent);
            ApplyHoverEffect(panel);

            // Click handler - open edit form with full info
            EventHandler onClick = (s, e) =>
            {
                _selectedTenantId = tenantId;
                OpenTenantEditor();
            };
            AttachClickHandlers(panel, onClick);

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

        private void UpdateSummary(DataTable filtered)
        {
            int total = filtered?.Rows.Count ?? 0;
            int active = 0;
            int inactive = 0;

            if (filtered != null && filtered.Columns.Contains("IsActive"))
            {
                foreach (DataRow row in filtered.Rows)
                {
                    if (bool.TryParse(row["IsActive"]?.ToString(), out var isActive) && isActive)
                        active++;
                    else
                        inactive++;
                }
            }
            else
            {
                inactive = total;
            }

            if (_lblTotal != null) _lblTotal.Text = $"{total} khách";
            if (_lblActive != null) _lblActive.Text = $"Đang ở: {active}";
            if (_lblInactive != null) _lblInactive.Text = $"Tạm ngưng: {inactive}";
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

        private string GetRoomNumberForTenant(int tenantId)
        {
            var historyRow = FindLatestHistoryRow(tenantId);
            if (historyRow == null || _roomMap == null) return string.Empty;
            return ReadString(_roomMap, TryReadInt(historyRow, "RoomId"));
        }

        private static Label CreateStatChip(string text, Color backColor, Color foreColor)
        {
            return new Label
            {
                AutoSize = true,
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = foreColor,
                BackColor = backColor,
                Padding = new Padding(8, 4, 8, 4),
                Margin = new Padding(6, 2, 0, 2)
            };
        }

        private static Control CreateAvatarBadge(string name, bool isActive)
        {
            var label = new Label
            {
                Width = 36,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = isActive ? Color.FromArgb(46, 164, 79) : Color.FromArgb(220, 53, 69),
                Text = GetInitials(name)
            };

            label.Resize += (s, e) =>
            {
                var path = new GraphicsPath();
                path.AddEllipse(0, 0, label.Width, label.Height);
                label.Region = new Region(path);
            };

            return label;
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "KH";
            var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private static Label CreateStatusBadge(string text, bool isActive)
        {
            return new Label
            {
                AutoSize = true,
                Text = text,
                BackColor = isActive ? Color.FromArgb(232, 247, 239) : Color.FromArgb(252, 236, 238),
                ForeColor = isActive ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding = new Padding(8, 2, 8, 2),
                Margin = new Padding(6, 2, 0, 0)
            };
        }

        private static void ApplyCardStyle(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(225, 232, 240)))
                {
                    var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };
        }

        private static void ApplyHoverEffect(Panel panel)
        {
            var normal = Color.White;
            var hover = Color.FromArgb(248, 250, 255);
            panel.MouseEnter += (s, e) => panel.BackColor = hover;
            panel.MouseLeave += (s, e) => panel.BackColor = normal;
        }

        private static void AttachClickHandlers(Control root, EventHandler handler)
        {
            if (root == null) return;
            root.Click += handler;
            foreach (Control child in root.Controls)
            {
                AttachClickHandlers(child, handler);
            }
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
