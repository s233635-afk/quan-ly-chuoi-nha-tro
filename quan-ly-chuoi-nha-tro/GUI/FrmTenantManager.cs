using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Màn hình quản lý khách thuê cho nhân viên.
    /// </summary>
    public class FrmTenantManager : Form
    {
        private const string SearchPlaceholder = "Tìm tên/CCCD/SĐT...";

        private readonly AdminDataBLL _bll;
        private readonly int? _branchId;

        private DataTable _tenants;
        private DataTable _dependents;
        private DataTable _history;
        private DataTable _rooms;

        private FlowLayoutPanel _tenantCardsHost;
        private Panel _tenantDetailPanel;
        private DataGridView _dgvDependents;
        private DataGridView _dgvHistory;

        private TextBox _txtSearch;
        private Label _lblTotal;
        
        private Label _lblTenantDetailTitle;
        private TextBox _txtTenantFullName;
        private TextBox _txtTenantIdentity;
        private TextBox _txtTenantPhone;
        private TextBox _txtTenantEmail;
        private DateTimePicker _dtTenantBirth;
        private TextBox _txtTenantAddress;
        private TextBox _txtTenantTempReg;
        private DateTimePicker _dtTenantTempFrom;
        private DateTimePicker _dtTenantTempTo;
        private TextBox _txtTenantFrontId;
        private TextBox _txtTenantBackId;
        private CheckBox _chkTenantActive;
        private Button _btnTenantSave;
        private DataRow _selectedTenantRow;
        private Control _selectedTenantCard;
        private int _selectedTenantId;

        public FrmTenantManager(AdminDataBLL bll, int? branchId = null)
        {
            _bll = bll ?? new AdminDataBLL();
            _branchId = branchId;
            InitializeComponent();
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
            actions.Controls.Add(btnDelete);
            actions.Controls.Add(btnRefresh);
            actions.Controls.Add(_lblTotal);

            toolbar.Controls.Add(actions);

            var host = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 400,
                SplitterWidth = 6,
                BackColor = Color.White
            };

            _tenantCardsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(6)
            };
            host.Panel1.Controls.Add(_tenantCardsHost);

            BuildTenantDetailsPanel();
            host.Panel2.Controls.Add(_tenantDetailPanel);

            Controls.Add(host);
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
            var btn = new Button
            {
                Text = text,
                Width = 90,
                Height = 32,
                BackColor = backColor,
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
                _dependents = await _bll.GetDependentsAsync() ?? new DataTable();
                _history = await _bll.GetTenantHistoryAsync() ?? new DataTable();
                _rooms = await _bll.GetRoomsAsync() ?? new DataTable();

                if (_branchId.HasValue && _tenants.Columns.Contains("BranchId"))
                {
                    var filtered = _tenants.AsEnumerable()
                        .Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == _branchId.Value);
                    _tenants = filtered.Any() ? filtered.CopyToDataTable() : _tenants.Clone();
                }

                PopulateRoomNumbers();

                _lblTotal.Text = $"Tổng: {_tenants.Rows.Count}";
                ApplySearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu khách thuê: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (_selectedTenantId > 0)
            {
                var match = FindById(filtered, "TenantId", _selectedTenantId);
                if (match != null)
                {
                    ShowTenantDetails(match, _selectedTenantCard);
                }
                else
                {
                    ClearTenantDetails();
                }
            }
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
                if (_selectedTenantId == 0)
                {
                    ShowTenantDetails(row, card);
                }
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

            void HandleClick(object sender, EventArgs args)
            {
                ShowTenantDetails(tenantRow, panel);
            }

            panel.Click += HandleClick;
            foreach (Control ctl in panel.Controls)
            {
                ctl.Click += HandleClick;
            }

            return panel;
        }

        private void ShowTenantDetails(DataRow tenantRow, Control card)
        {
            if (tenantRow == null)
            {
                ClearTenantDetails();
                return;
            }

            _selectedTenantRow = tenantRow;
            _selectedTenantId = TryReadInt(tenantRow, "TenantId");
            _lblTenantDetailTitle.Text = $"Khách thuê #{_selectedTenantId}";

            _txtTenantFullName.Text = ReadString(tenantRow, "FullName") ?? "";
            _txtTenantIdentity.Text = ReadString(tenantRow, "IdentityCard") ?? "";
            _txtTenantPhone.Text = ReadString(tenantRow, "PhoneNumber") ?? "";
            _txtTenantEmail.Text = ReadString(tenantRow, "Email") ?? "";
            SetDatePicker(_dtTenantBirth, ReadString(tenantRow, "BirthDate"));
            _txtTenantAddress.Text = TextFixer.FixUtf8Mojibake(ReadString(tenantRow, "Address") ?? "");
            _txtTenantTempReg.Text = TextFixer.FixUtf8Mojibake(ReadString(tenantRow, "TemporaryRegistration") ?? "");
            SetDatePicker(_dtTenantTempFrom, ReadString(tenantRow, "TemporaryRegistrationDate"));
            SetDatePicker(_dtTenantTempTo, ReadString(tenantRow, "TemporaryRegistrationExpiry"));
            _txtTenantFrontId.Text = ReadString(tenantRow, "FrontIdPhoto") ?? "";
            _txtTenantBackId.Text = ReadString(tenantRow, "BackIdPhoto") ?? "";
            _chkTenantActive.Checked = tenantRow.Table.Columns.Contains("IsActive") && bool.TryParse(tenantRow["IsActive"]?.ToString(), out var active) && active;

            HighlightSelectedCard(card);
        }

        private void HighlightSelectedCard(Control card)
        {
            if (_selectedTenantCard != null && !_selectedTenantCard.IsDisposed)
            {
                _selectedTenantCard.BackColor = Color.White;
            }
            _selectedTenantCard = card;
            if (_selectedTenantCard != null)
            {
                _selectedTenantCard.BackColor = Color.FromArgb(248, 252, 255);
            }
        }

        private void ClearTenantDetails()
        {
            _selectedTenantId = 0;
            _selectedTenantRow = null;
            _lblTenantDetailTitle.Text = "Chọn 1 khách thuê để xem thông tin";
            _txtTenantFullName.Text = "";
            _txtTenantIdentity.Text = "";
            _txtTenantPhone.Text = "";
            _txtTenantEmail.Text = "";
            _dtTenantBirth.Checked = false;
            _txtTenantAddress.Text = "";
            _txtTenantTempReg.Text = "";
            _dtTenantTempFrom.Checked = false;
            _dtTenantTempTo.Checked = false;
            _txtTenantFrontId.Text = "";
            _txtTenantBackId.Text = "";
            _chkTenantActive.Checked = false;
            _selectedTenantCard = null;
        }

        private void SetDatePicker(DateTimePicker picker, string rawValue)
        {
            if (picker == null) return;
            if (DateTime.TryParse(rawValue, out var dt))
            {
                picker.Value = dt;
                picker.Checked = true;
            }
            else
            {
                picker.Checked = false;
            }
        }

        private async Task SaveTenantAsync()
        {
            if (_selectedTenantId <= 0 || _selectedTenantRow == null)
            {
                MessageBox.Show("Vui lòng chọn một khách thuê trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                await _bll.UpdateTenantAsync(
                    _selectedTenantId,
                    _txtTenantFullName.Text.Trim(),
                    _txtTenantIdentity.Text.Trim(),
                    _txtTenantPhone.Text.Trim(),
                    _txtTenantEmail.Text.Trim(),
                    _dtTenantBirth.Checked ? _dtTenantBirth.Value : (DateTime?)null,
                    _txtTenantAddress.Text.Trim(),
                    _txtTenantTempReg.Text.Trim(),
                    _dtTenantTempFrom.Checked ? _dtTenantTempFrom.Value : (DateTime?)null,
                    _dtTenantTempTo.Checked ? _dtTenantTempTo.Value : (DateTime?)null,
                    _chkTenantActive.Checked,
                    _txtTenantFrontId.Text.Trim(),
                    _txtTenantBackId.Text.Trim()
                );

                await LoadDataAsync();
                var refreshed = FindById(_tenants, "TenantId", _selectedTenantId);
                if (refreshed != null) ShowTenantDetails(refreshed, _selectedTenantCard);
                MessageBox.Show("Đã lưu thông tin khách thuê.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu khách thuê.\n\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataRow GetSelectedTenant()
        {
            return _selectedTenantRow;
        }

        private void BuildTenantDetailsPanel()
        {
            _tenantDetailPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16),
                AutoScroll = true
            };

            _lblTenantDetailTitle = new Label
            {
                AutoSize = true,
                Text = "Chọn 1 khách thuê để xem thông tin",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Margin = new Padding(0, 0, 0, 12),
                Dock = DockStyle.Top
            };
            _tenantDetailPanel.Controls.Add(_lblTenantDetailTitle);

            var detailLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(0, 6, 0, 0)
            };
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Control AddRow(string label, Control ctl)
            {
                var lbl = new Label
                {
                    Text = label,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.FromArgb(70, 70, 70),
                    Margin = new Padding(0, 4, 8, 4)
                };
                ctl.Dock = DockStyle.Fill;
                ctl.Margin = new Padding(0, 4, 0, 4);
                detailLayout.Controls.Add(lbl);
                detailLayout.Controls.Add(ctl);
                return ctl;
            }

            _txtTenantFullName = new TextBox();
            _txtTenantIdentity = new TextBox();
            _txtTenantPhone = new TextBox();
            _txtTenantEmail = new TextBox();
            _dtTenantBirth = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            _txtTenantAddress = new TextBox { Multiline = true, Height = 60, ScrollBars = ScrollBars.Vertical };
            _txtTenantTempReg = new TextBox();
            _dtTenantTempFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            _dtTenantTempTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            _txtTenantFrontId = new TextBox();
            _txtTenantBackId = new TextBox();
            _chkTenantActive = new CheckBox { Text = "Đang hoạt động", AutoSize = true };

            AddRow("Họ tên", _txtTenantFullName);
            AddRow("CCCD", _txtTenantIdentity);
            AddRow("SĐT", _txtTenantPhone);
            AddRow("Email", _txtTenantEmail);
            AddRow("Ngày sinh", _dtTenantBirth);
            AddRow("Địa chỉ", _txtTenantAddress);
            AddRow("Tạm trú tại", _txtTenantTempReg);
            AddRow("Tạm trú từ", _dtTenantTempFrom);
            AddRow("Tạm trú đến", _dtTenantTempTo);
            AddRow("Ảnh CCCD (mặt trước)", _txtTenantFrontId);
            AddRow("Ảnh CCCD (mặt sau)", _txtTenantBackId);
            AddRow("Trạng thái", _chkTenantActive);

            detailLayout.Controls.Add(new Label());
            _btnTenantSave = new Button
            {
                Text = "Lưu",
                Width = 120,
                Height = 36,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnTenantSave.FlatAppearance.BorderSize = 0;
            _btnTenantSave.Click += async (s, e) => await SaveTenantAsync();
            detailLayout.Controls.Add(_btnTenantSave);

            _tenantDetailPanel.Controls.Add(detailLayout);
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

        private async void AddTenant()
        {
            using (var frm = new FrmTenantEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadDataAsync();
            }
        }

        private async void EditTenant()
        {
            var row = GetSelectedTenant();
            if (row == null)
            {
                MessageBox.Show("Chọn khách thuê trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmTenantEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadDataAsync();
            }
        }

        private async void DeleteTenant()
        {
            if (_selectedTenantId <= 0)
            {
                MessageBox.Show("Chọn khách thuê trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = _selectedTenantRow != null ? ReadString(_selectedTenantRow, "FullName") : _selectedTenantId.ToString();

            if (MessageBox.Show($"Xóa khách thuê \"{NullDash(name)}\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteTenantAsync(_selectedTenantId);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa khách thuê: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateRoomNumbers()
        {
            if (_tenants == null) return;

            if (!_tenants.Columns.Contains("RoomNumber"))
                _tenants.Columns.Add("RoomNumber", typeof(string));

            var roomLookup = _rooms?.AsEnumerable()
                .Where(r => _rooms.Columns.Contains("RoomId") && r["RoomId"] != DBNull.Value)
                .ToDictionary(r => Convert.ToInt32(r["RoomId"]), r => r["RoomNumber"]?.ToString() ?? string.Empty)
                ?? new System.Collections.Generic.Dictionary<int, string>();

            foreach (DataRow tenant in _tenants.Rows)
            {
                int tenantId = int.TryParse(tenant["TenantId"]?.ToString(), out var id) ? id : 0;
                if (tenantId <= 0) continue;

                var historyRows = _history?.AsEnumerable()
                    .Where(r => int.TryParse(r["TenantId"]?.ToString(), out var tid) && tid == tenantId)
                    .ToList();
                if (historyRows == null || historyRows.Count == 0) continue;

                DataRow latest = historyRows
                    .Where(r => string.IsNullOrWhiteSpace(r["CheckOutDate"]?.ToString()))
                    .OrderByDescending(r => ParseDate(r["CheckInDate"]))
                    .FirstOrDefault()
                    ?? historyRows.OrderByDescending(r => ParseDate(r["CheckInDate"])).FirstOrDefault();

                if (latest != null && int.TryParse(latest["RoomId"]?.ToString(), out var rid) && roomLookup.TryGetValue(rid, out var roomNo))
                {
                    tenant["RoomNumber"] = roomNo;
                }
            }
        }

        private DateTime ParseDate(object value)
        {
            if (value == null) return DateTime.MinValue;
            return DateTime.TryParse(value.ToString(), out var dt) ? dt : DateTime.MinValue;
        }


        private void ConfigureDependentsGrid()
        {
            if (_dgvDependents?.Columns == null) return;
            if (_dgvDependents.Columns.Contains("DependentId")) _dgvDependents.Columns["DependentId"].Visible = false;
            SetHeader(_dgvDependents, "FullName", "Họ tên");
            SetHeader(_dgvDependents, "Relationship", "Quan hệ");
            SetHeader(_dgvDependents, "PhoneNumber", "SĐT");
            SetHeader(_dgvDependents, "CreatedDate", "Ngày tạo");
        }

        private void ConfigureHistoryGrid()
        {
            if (_dgvHistory?.Columns == null) return;
            if (_dgvHistory.Columns.Contains("HistoryId")) _dgvHistory.Columns["HistoryId"].Visible = false;
            SetHeader(_dgvHistory, "RoomId", "Mã phòng", visible: false);
            SetHeader(_dgvHistory, "CheckInDate", "Ngày vào");
            SetHeader(_dgvHistory, "CheckOutDate", "Ngày ra");
            SetHeader(_dgvHistory, "Status", "Trạng thái");
            SetHeader(_dgvHistory, "Notes", "Ghi chú");
            SetHeader(_dgvHistory, "CreatedDate", "Ngày tạo");
        }

        private void SetHeader(DataGridView grid, string columnName, string header, bool visible = true)
        {
            if (grid == null || !grid.Columns.Contains(columnName)) return;
            var col = grid.Columns[columnName];
            col.HeaderText = header;
            col.Visible = visible;
        }
    }
}
