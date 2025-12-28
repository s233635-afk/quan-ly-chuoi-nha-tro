using System;
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
    /// Module Chi nhánh dạng Card (thay cho DataGridView).
    /// Click card -> mở chi tiết chi nhánh theo tabs.
    /// </summary>
    public class FrmBranchCards : Form
    {
        private const int MaxBranchCards = 2;

        private readonly BranchBLL _branchBll = new BranchBLL();
        private readonly AdminDataBLL _adminBll = new AdminDataBLL();

        private DataTable _branches;
        private DataTable _rooms;

        private Panel _top;
        private FlowLayoutPanel _cardsHost;
        private SplitContainer _split;
        private TextBox _txtSearch;
        private Label _lblCount;
        private Label _lblStatTotalValue;
        private Label _lblStatActiveValue;
        private Label _lblStatInactiveValue;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnRefresh;

        private int _selectedBranchId;

        public FrmBranchCards()
        {
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
            Load += async (s, e) => await LoadDataAsync();
            Shown += (s, e) => BeginInvoke((System.Action)ApplySplitterLayout);
            SizeChanged += (s, e) => ApplySplitterLayout();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý chi nhánh";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1280;
            Height = 720;
            BackColor = Color.FromArgb(245, 247, 250);

            _top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 168,
                BackColor = Color.White,
                Padding = new Padding(14, 12, 14, 12)
            };
            _top.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 3, BackColor = UiKit.Primary });

            _btnAdd = MakeButton("Thêm", Color.FromArgb(0, 122, 204), async (s, e) => await AddBranchAsync());
            _btnEdit = MakeButton("Sửa", Color.FromArgb(255, 193, 7), async (s, e) => await EditSelectedAsync());
            _btnDelete = MakeButton("Xóa", Color.FromArgb(220, 53, 69), async (s, e) => await DeleteSelectedAsync());
            _btnRefresh = MakeButton("Tải lại", Color.FromArgb(108, 117, 125), async (s, e) => await LoadDataAsync());

            _txtSearch = new TextBox { Width = 360 };
            _txtSearch.TextChanged += (s, e) => RebuildCards();

            _lblCount = new Label
            {
                AutoSize = true,
                Text = "Tổng: 0 chi nhánh",
                ForeColor = Color.FromArgb(90, 90, 90)
            };

            var statTotal = MakeStatCard("Tổng số chi nhánh", Color.FromArgb(0, 122, 204), out _lblStatTotalValue);
            var statActive = MakeStatCard("Chi nhánh hoạt động", Color.FromArgb(40, 167, 69), out _lblStatActiveValue);
            var statInactive = MakeStatCard("Chi nhánh tạm dừng", Color.FromArgb(220, 53, 69), out _lblStatInactiveValue);

            var pnlActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 42,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            pnlActions.Controls.Add(_btnAdd);
            pnlActions.Controls.Add(_btnEdit);
            pnlActions.Controls.Add(_btnDelete);
            pnlActions.Controls.Add(_btnRefresh);

            // Stats row
            var pnlStats = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 0)
            };
            pnlStats.Controls.Add(statTotal);
            pnlStats.Controls.Add(statActive);
            pnlStats.Controls.Add(statInactive);

            var pnlSearch = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 32,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 4, 0, 0)
            };
            var lblSearch = new Label
            {
                AutoSize = true,
                Text = "Tìm kiếm:",
                ForeColor = Color.FromArgb(90, 90, 90),
                Margin = new Padding(0, 6, 6, 0)
            };
            _txtSearch.Margin = new Padding(0, 2, 10, 0);
            _lblCount.Margin = new Padding(0, 6, 0, 0);
            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(_txtSearch);
            pnlSearch.Controls.Add(_lblCount);

            _top.Controls.Add(pnlSearch);
            _top.Controls.Add(pnlStats);
            _top.Controls.Add(pnlActions);

            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterWidth = 6,
                BackColor = Color.FromArgb(230, 235, 240),
                Panel1MinSize = 0,
                Panel2MinSize = 0,
                Panel2Collapsed = true
            };
            _split.HandleCreated += (s, e) => ApplySplitterLayout();
            _split.Layout += (s, e) => ApplySplitterLayout();

            _cardsHost = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(14),
                BackColor = BackColor
            };
            _split.Panel1.Controls.Add(_cardsHost);

            
            Controls.Add(_split);
            Controls.Add(_top);
        }

        private static Button MakeButton(string text, Color color, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Width = 92,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 10, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }


        private static Panel MakeStatCard(string title, Color valueColor, out Label valueLabel)
        {
            var panel = new Panel
            {
                Width = 240,
                Height = 42,
                Margin = new Padding(0, 0, 16, 0),
                BackColor = Color.Transparent
            };

            var titleLabel = new Label
            {
                AutoSize = true,
                Text = title ?? string.Empty,
                ForeColor = Color.FromArgb(90, 90, 90),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Location = new Point(0, 0)
            };

            valueLabel = new Label
            {
                AutoSize = true,
                Text = "0",
                ForeColor = valueColor,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(0, 18)
            };

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(valueLabel);
            return panel;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _btnRefresh.Enabled = false;
                _branches = null;
                _rooms = null;
                _selectedBranchId = 0;

                var branchTask = _branchBll.GetAllBranchesAsync();
                var roomTask = _adminBll.GetRoomsAsync();
                await Task.WhenAll(branchTask, roomTask);

                var allBranches = branchTask.Result;
                if (allBranches == null) allBranches = new DataTable();
                TextFixer.FixDataTable(allBranches, "BranchName", "Address", "Description");
                _branches = AdminBranchScope.Apply(allBranches);
                _branches = LimitBranches(_branches, MaxBranchCards);

                _rooms = roomTask.Result ?? new DataTable();

                UpdateStats();
                RebuildCards();
                AutoSelectFirstBranch();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi nhánh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnRefresh.Enabled = true;
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

        private void UpdateStats()
        {
            int total = _branches?.Rows.Count ?? 0;
            int active = 0;
            int inactive = 0;

            if (_branches != null && _branches.Columns.Contains("IsActive"))
            {
                foreach (DataRow r in _branches.Rows)
                {
                    bool isActive = false;
                    try { isActive = Convert.ToBoolean(r["IsActive"]); } catch { }
                    if (isActive) active++; else inactive++;
                }
            }
            else
            {
                active = total;
                inactive = 0;
            }

            _lblCount.Text = $"Tổng: {total} chi nhánh";
            if (_lblStatTotalValue != null) _lblStatTotalValue.Text = total.ToString("N0");
            if (_lblStatActiveValue != null) _lblStatActiveValue.Text = active.ToString("N0");
            if (_lblStatInactiveValue != null) _lblStatInactiveValue.Text = inactive.ToString("N0");
        }

        private void RebuildCards()
        {
            _cardsHost.SuspendLayout();
            _cardsHost.Controls.Clear();

            if (_branches == null || !_branches.Columns.Contains("BranchId"))
            {
                _cardsHost.ResumeLayout();
                return;
            }

            string keyword = (_txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            var rows = _branches.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rows = rows.Where(r =>
                    Contains(r, "BranchCode", keyword) ||
                    Contains(r, "BranchName", keyword) ||
                    Contains(r, "Address", keyword) ||
                    Contains(r, "Phone", keyword) ||
                    Contains(r, "Hotline", keyword));
            }

            foreach (var r in rows)
            {
                if (!int.TryParse(r["BranchId"]?.ToString(), out var branchId) || branchId <= 0)
                    continue;
                _cardsHost.Controls.Add(MakeBranchCard(r, branchId));
            }

            _cardsHost.ResumeLayout();
        }

        private void AutoSelectFirstBranch()
        {
            if (_branches == null || !_branches.Columns.Contains("BranchId")) return;
            if (_branches.Rows.Count == 0) return;
            if (_selectedBranchId > 0) return;

            int branchId;
            if (!int.TryParse(_branches.Rows[0]["BranchId"]?.ToString(), out branchId)) return;
            _selectedBranchId = branchId;
        }

        private Control MakeBranchCard(DataRow row, int branchId)
        {
            string code = ReadString(row, "BranchCode");
            string name = TextFixer.FixUtf8Mojibake(ReadString(row, "BranchName"));
            string address = TextFixer.FixUtf8Mojibake(ReadString(row, "Address"));
            string phone = ReadString(row, "Phone");
            string hotline = ReadString(row, "Hotline");
            string hours = ReadString(row, "OperatingHours");

            bool isActive = true;
            if (row.Table.Columns.Contains("IsActive"))
            {
                try { isActive = Convert.ToBoolean(row["IsActive"]); } catch { isActive = true; }
            }

            int roomsTotal = CountRooms(branchId, null);
            int roomsActive = CountRooms(branchId, true);

            var card = new Panel
            {
                Width = 360,
                Height = 170,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 14, 14),
                Cursor = Cursors.Hand,
                Tag = branchId
            };

            var accent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = isActive ? Color.FromArgb(0, 122, 204) : Color.FromArgb(220, 53, 69)
            };

            var title = new Label
            {
                AutoSize = false,
                Height = 44,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 12, 0),
                Text = string.IsNullOrWhiteSpace(code) ? name : $"{code} - {name}"
            };

            var sub = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                Padding = new Padding(12, 0, 12, 8),
                Text = BuildSubText(address, phone, hotline, hours, roomsTotal, roomsActive, isActive)
            };

            var border = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            border.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(_selectedBranchId == branchId ? Color.FromArgb(0, 122, 204) : Color.FromArgb(230, 230, 230), 1.4f))
                {
                    var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    g.DrawRectangle(pen, rect);
                }
            };

            border.Controls.Add(sub);
            border.Controls.Add(title);
            card.Controls.Add(border);
            card.Controls.Add(accent);

            void SelectThis()
            {
                _selectedBranchId = branchId;
                foreach (Control c in _cardsHost.Controls)
                    c.Invalidate();
            }

            void OpenDetails()
            {
                SelectThis();
                OpenBranchDialog(branchId, FrmBranchOverview.BranchOverviewTab.Branch);
            }

            card.Click += (s, e) => OpenDetails();
            title.Click += (s, e) => OpenDetails();
            sub.Click += (s, e) => OpenDetails();
            accent.Click += (s, e) => OpenDetails();

            card.DoubleClick += (s, e) => OpenDetails();
            card.MouseDown += (s, e) =>
            {
                SelectThis();
                if (e.Button == MouseButtons.Right)
                {
                    var menu = new ContextMenuStrip();
                    menu.Items.Add("Xem chi tiết", null, (ss, ee) => OpenDetails());
                    menu.Items.Add("Sửa", null, async (ss, ee) => await EditSelectedAsync());
                    menu.Items.Add("Xóa", null, async (ss, ee) => await DeleteSelectedAsync());
                    menu.Show(card, e.Location);
                }
            };

            return card;
        }

        private string BuildSubText(string address, string phone, string hotline, string hours, int roomsTotal, int roomsActive, bool isActive)
        {
            string status = isActive ? "Hoạt động" : "Vô hiệu";
            string line1 = string.IsNullOrWhiteSpace(address) ? "—" : address;
            string line2 = $"Điện thoại: {NullDash(phone)}   |   Hotline: {NullDash(hotline)}";
            string line3 = $"Giờ hoạt động: {NullDash(hours)}   |   Phòng hoạt động: {roomsActive}/{roomsTotal}";
            string line4 = $"Trạng thái: {status}";
            return string.Join("\n", new[] { line1, line2, line3, line4 });
        }

        private int CountRooms(int branchId, bool? onlyActive)
        {
            if (_rooms == null || !_rooms.Columns.Contains("BranchId")) return 0;
            var rows = _rooms.AsEnumerable().Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == branchId);
            if (onlyActive.HasValue && _rooms.Columns.Contains("IsActive"))
            {
                rows = rows.Where(r =>
                {
                    try { return Convert.ToBoolean(r["IsActive"]) == onlyActive.Value; } catch { return false; }
                });
            }
            return rows.Count();
        }

        private static bool Contains(DataRow r, string col, string keyword)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return false;
            var v = r[col];
            if (v == null || v == DBNull.Value) return false;
            return (v.ToString() ?? string.Empty).ToLowerInvariant().Contains(keyword);
        }

        private static string ReadString(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return null;
            var v = r[col];
            return v == null || v == DBNull.Value ? null : v.ToString();
        }

        private static string NullDash(string s) => string.IsNullOrWhiteSpace(s) ? "—" : s.Trim();

        private static DataTable LimitBranches(DataTable branches, int maxCount)
        {
            if (branches == null) return branches;
            if (maxCount <= 0) return branches;
            if (branches.Rows.Count <= maxCount) return branches;
            if (!branches.Columns.Contains("BranchId")) return branches;

            var rows = branches.AsEnumerable()
                .Where(r => int.TryParse(r["BranchId"]?.ToString(), out _))
                .OrderBy(r => Convert.ToInt32(r["BranchId"]))
                .Take(maxCount)
                .ToList();

            var result = branches.Clone();
            foreach (var r in rows)
                result.ImportRow(r);
            return result;
        }

        private void ApplySplitterLayout()
        {
            if (_split == null) return;
            if (_split.Panel2Collapsed) return;
            int total = _split.ClientSize.Width;
            if (total <= 0) return;

            int minLeftDesired = 260;
            int minRightDesired = 320;

            int minLeft = minLeftDesired;
            int minRight = minRightDesired;
            if (total < (minLeft + minRight))
            {
                minLeft = 0;
                minRight = 0;
            }

            _split.Panel1MinSize = minLeft;
            _split.Panel2MinSize = minRight;

            int maxLeft = total - minRight;
            if (maxLeft < minLeft) maxLeft = minLeft;

            int preferred = Math.Max(minLeft, Math.Min(total / 3, maxLeft));
            int desired = _split.SplitterDistance;
            if (desired < minLeft || desired > maxLeft)
                desired = preferred;

            if (_split.SplitterDistance != desired)
                _split.SplitterDistance = desired;
        }

        private void OpenBranchDialog(int branchId, FrmBranchOverview.BranchOverviewTab tab)
        {
            if (branchId <= 0)
            {
                MessageBox.Show("Vui lòng chọn một chi nhánh trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmBranchOverview(branchId))
            {
                frm.SelectTab(tab);
                frm.ShowDialog(FindForm() ?? this);
            }
        }

        private async Task AddBranchAsync()
        {
            using (var frm = new FrmBranchDetail())
            {
                var owner = FindForm() ?? this;
                if (frm.ShowDialog(owner) == DialogResult.OK)
                {
                    await LoadDataAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async Task EditSelectedAsync()
        {
            if (_selectedBranchId <= 0)
            {
                MessageBox.Show("Vui lòng chọn 1 chi nhánh.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmBranchDetail(_selectedBranchId))
            {
                var owner = FindForm() ?? this;
                if (frm.ShowDialog(owner) == DialogResult.OK)
                {
                    await LoadDataAsync();
                    AdminEvents.NotifyDataChanged();
                }
            }
        }

        private async Task DeleteSelectedAsync()
        {
            if (_selectedBranchId <= 0)
            {
                MessageBox.Show("Vui lòng chọn 1 chi nhánh.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Bạn chắc chắn muốn xóa chi nhánh này? (xóa mềm)", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _branchBll.DeleteBranchAsync(_selectedBranchId);
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa chi nhánh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
