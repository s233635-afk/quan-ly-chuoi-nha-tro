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
    /// FrmNotificationManager - Quản Lý Thông Báo & Hệ Thống Nhắc Nhở Tự Động
    /// Tính năng: Thêm/Sửa/Xóa thông báo, Gửi thông báo, Nhắc công nợ, Nhắc hết hạn hợp đồng, 
    /// Nhắc nhập chỉ số điện nước, Thông báo nội bộ, Lịch sử gửi
    /// </summary>
    public class FrmNotificationManager : Form
    {
        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private DataTable _notifications;
        private TabControl _tabMain;

        // Tab 1: Quản Lý Thông Báo
        private DataGridView _dgvNotifications;
        private TextBox _txtSearchTitle;
        private ComboBox _cboStatus;
        private Label _lblTotalCount;
        private Button _btnAdd, _btnEdit, _btnDelete, _btnSend, _btnRefresh;

        // Tab 2: Nhắc Nhở Tự Động
        private Label _lblOverdueCount, _lblExpiredCount, _lblMissingUtilityCount;
        private Button _btnGenerateReminders, _btnViewReminders;
        private ListBox _lbAutoReminders;

        // Tab 3: Thông Báo Nội Bộ
        private TextBox _txtNotificationTitle, _txtNotificationContent;
        private ComboBox _cboTarget;
        private Button _btnSendInternal;

        // Tab 4: Lịch Sử Gửi
        private DataGridView _dgvHistory;

        public FrmNotificationManager()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "📬 Quản Lý Thông Báo & Nhắc Nhở";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1400;
            Height = 850;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 9.75f);

            _tabMain = new TabControl { Dock = DockStyle.Fill, Padding = new Point(12, 6) };
            _tabMain.TabPages.Add(CreateManagerTab());
            _tabMain.TabPages.Add(CreateRemindersTab());
            _tabMain.TabPages.Add(CreateInternalNotificationTab());
            _tabMain.TabPages.Add(CreateHistoryTab());

            Controls.Add(_tabMain);
            Load += async (s, e) => await LoadAllDataAsync();
        }

        #region ========== TAB 1: QUẢN LÝ THÔNG BÁO ==========

        private TabPage CreateManagerTab()
        {
            var tab = new TabPage { Text = "📬 Quản Lý Thông Báo", BackColor = Color.White };

            var pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _btnAdd = CreateStyledButton("➕ Thêm", Color.FromArgb(46, 204, 113), 90);
            _btnEdit = CreateStyledButton("✏️ Sửa", Color.FromArgb(52, 168, 219), 90);
            _btnDelete = CreateStyledButton("🗑️ Xóa", Color.FromArgb(231, 76, 60), 90);
            _btnSend = CreateStyledButton("📤 Gửi", Color.FromArgb(155, 89, 182), 90);
            _btnRefresh = CreateStyledButton("🔄 Tải", Color.FromArgb(149, 165, 166), 90);

            _btnAdd.Click += async (s, e) => await AddNotificationAsync();
            _btnEdit.Click += async (s, e) => await EditSelectedAsync();
            _btnDelete.Click += async (s, e) => await DeleteSelectedAsync();
            _btnSend.Click += async (s, e) => await SendNotificationAsync();
            _btnRefresh.Click += async (s, e) => await LoadNotificationsAsync();

            var pnlLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            pnlLeft.Controls.AddRange(new[] { _btnAdd, _btnEdit, _btnDelete, _btnSend, _btnRefresh });

            _txtSearchTitle = new TextBox
            {
                Width = 250,
                Height = 36,
                Margin = new Padding(0, 2, 8, 0),
                Text = "🔍 Tìm tiêu đề...",
                ForeColor = Color.Gray
            };
            _txtSearchTitle.GotFocus += (s, e) => { if (_txtSearchTitle.Text == "🔍 Tìm tiêu đề...") { _txtSearchTitle.Text = string.Empty; _txtSearchTitle.ForeColor = Color.Black; } };
            _txtSearchTitle.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_txtSearchTitle.Text)) { _txtSearchTitle.Text = "🔍 Tìm tiêu đề..."; _txtSearchTitle.ForeColor = Color.Gray; } };
            _txtSearchTitle.TextChanged += (s, e) => ApplyFilter();

            _cboStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 140,
                Height = 36,
                Margin = new Padding(0, 2, 8, 0)
            };
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Chưa đọc", "Đã đọc", "Đã gửi" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblTotalCount = new Label
            {
                Text = "Tổng: 0",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(8, 8, 0, 0)
            };

            var pnlRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            pnlRight.Controls.AddRange(new Control[]
            {
                new Label { Text = "Tìm:", AutoSize = true, Margin = new Padding(0, 8, 6, 0) },
                _txtSearchTitle,
                new Label { Text = "Trạng thái:", AutoSize = true, Margin = new Padding(12, 8, 6, 0) },
                _cboStatus,
                _lblTotalCount
            });

            pnlToolbar.Controls.Add(pnlLeft);
            pnlToolbar.Controls.Add(pnlRight);

            _dgvNotifications = CreateDataGrid();
            _dgvNotifications.Dock = DockStyle.Fill;
            _dgvNotifications.DoubleClick += async (s, e) => await EditSelectedAsync();

            tab.Controls.Add(_dgvNotifications);
            tab.Controls.Add(pnlToolbar);
            return tab;
        }

        private async Task AddNotificationAsync()
        {
            using (var frm = new FrmNotificationEditor(_bll))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await LoadNotificationsAsync();
                }
            }
        }

        private async Task EditSelectedAsync()
        {
            if (_dgvNotifications.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn thông báo để chỉnh sửa.", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_notifications == null || _notifications.Rows.Count == 0) return;

            var selectedRow = _dgvNotifications.SelectedRows[0];
            var row = _notifications.Rows[selectedRow.Index];

            using (var frm = new FrmNotificationEditor(_bll, row))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await LoadNotificationsAsync();
                }
            }
        }

        private async Task DeleteSelectedAsync()
        {
            if (_dgvNotifications.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn thông báo để xóa.", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Bạn chắc chắn muốn xóa?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                if (_notifications == null || _notifications.Rows.Count == 0) return;

                var selectedRow = _dgvNotifications.SelectedRows[0];
                var notificationId = Convert.ToInt32(_notifications.Rows[selectedRow.Index]["NotificationId"]);

                await _bll.DeleteNotificationAsync(notificationId);
                MessageBox.Show("Xóa thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadNotificationsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SendNotificationAsync()
        {
            if (_dgvNotifications.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn thông báo để gửi.", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (_notifications == null || _notifications.Rows.Count == 0) return;

                var selectedRow = _dgvNotifications.SelectedRows[0];
                var row = _notifications.Rows[selectedRow.Index];
                var notificationId = Convert.ToInt32(row["NotificationId"]);

                var title = row["Title"].ToString();
                var message = row["Message"].ToString();
                var userId = row["UserId"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["UserId"]);

                await _bll.UpdateNotificationAsync(notificationId, userId, title, message, "Đã gửi");
                MessageBox.Show("Gửi thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadNotificationsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            if (_notifications == null || _notifications.Rows.Count == 0) return;

            var filtered = _notifications.AsEnumerable()
                .Where(r =>
                {
                    bool titleMatch = r["Title"].ToString().IndexOf(_txtSearchTitle.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool statusMatch = _cboStatus.SelectedIndex == 0 || r["Status"].ToString() == GetStatusValue(_cboStatus.SelectedIndex);
                    return titleMatch && statusMatch;
                })
                .ToList();

            // Handle empty result
            if (filtered.Count == 0)
            {
                _dgvNotifications.DataSource = _notifications.Clone(); // Show empty table with same columns
                _lblTotalCount.Text = "Tổng: 0";
            }
            else
            {
                _dgvNotifications.DataSource = filtered.CopyToDataTable();
                _lblTotalCount.Text = $"Tổng: {filtered.Count}";
            }
        }

        private string GetStatusValue(int index)
        {
            return index switch
            {
                1 => "Chưa đọc",
                2 => "Đã đọc",
                3 => "Đã gửi",
                _ => ""
            };
        }

        private async Task LoadNotificationsAsync()
        {
            try
            {
                _notifications = await _bll.GetNotificationsAsync();
                _dgvNotifications.DataSource = _notifications;
                _lblTotalCount.Text = $"Tổng: {_notifications.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ========== TAB 2: NHẮC NHỞ TỰ ĐỘNG ==========

        private TabPage CreateRemindersTab()
        {
            var tab = new TabPage { Text = "🔔 Nhắc Nhở Tự Động", BackColor = Color.White };

            var pnlStats = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                Padding = new Padding(15),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _lblOverdueCount = CreateStatCard("💰 Công Nợ Quá Hạn", "0", Color.FromArgb(231, 76, 60), pnlStats, 10);
            _lblExpiredCount = CreateStatCard("📜 Hợp Đồng Hết Hạn", "0", Color.FromArgb(230, 126, 34), pnlStats, 350);
            _lblMissingUtilityCount = CreateStatCard("💡 Chưa Nhập Chỉ Số", "0", Color.FromArgb(52, 168, 219), pnlStats, 690);

            var pnlActions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _btnGenerateReminders = CreateStyledButton("🎯 Tạo Nhắc Nhở", Color.FromArgb(46, 204, 113), 160);
            _btnViewReminders = CreateStyledButton("📋 Xem Chi Tiết", Color.FromArgb(52, 168, 219), 160);

            _btnGenerateReminders.Click += async (s, e) => await GenerateRemindersAsync();
            _btnViewReminders.Click += (s, e) => ShowReminderDetails();

            var pnlActionFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            pnlActionFlow.Controls.AddRange(new[] { _btnGenerateReminders, _btnViewReminders });
            pnlActions.Controls.Add(pnlActionFlow);

            _lbAutoReminders = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = SelectionMode.MultiSimple,
                Font = new Font("Segoe UI", 10)
            };

            tab.Controls.Add(_lbAutoReminders);
            tab.Controls.Add(pnlActions);
            tab.Controls.Add(pnlStats);
            return tab;
        }

        private Label CreateStatCard(string title, string value, Color color, Panel parent, int left)
        {
            var pnl = new Panel
            {
                Location = new Point(left, 15),
                Size = new Size(320, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitle = new Label
            {
                Text = title,
                Location = new Point(12, 10),
                Size = new Size(296, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = color,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblValue = new Label
            {
                Text = value,
                Location = new Point(12, 40),
                Size = new Size(296, 60),
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleLeft
            };

            pnl.Controls.AddRange(new[] { lblTitle, lblValue });
            parent.Controls.Add(pnl);

            return lblValue;
        }

        private async Task GenerateRemindersAsync()
        {
            try
            {
                _lbAutoReminders.Items.Clear();

                // Nhắc công nợ
                var overDueInvoices = await GetOverdueInvoicesAsync();
                _lblOverdueCount.Text = overDueInvoices.Count.ToString();
                foreach (var inv in overDueInvoices)
                {
                    _lbAutoReminders.Items.Add($"💰 {inv}");
                    await _bll.AddNotificationAsync(null, "⚠️ NHẮC CÔNG NỢ QUÁ HẠN", $"Công nợ {inv} đã quá hạn thanh toán!", "Chưa đọc");
                }

                // Nhắc hết hạn hợp đồng
                var expiredContracts = await GetExpiredContractsAsync();
                _lblExpiredCount.Text = expiredContracts.Count.ToString();
                foreach (var contract in expiredContracts)
                {
                    _lbAutoReminders.Items.Add($"📜 {contract}");
                    await _bll.AddNotificationAsync(null, "⚠️ NHẮC HẬN HỢP ĐỒNG", $"Hợp đồng {contract} sắp hết hạn!", "Chưa đọc");
                }

                // Nhắc nhập chỉ số điện nước
                var missingUtility = await GetMissingUtilityReadingsAsync();
                _lblMissingUtilityCount.Text = missingUtility.Count.ToString();
                foreach (var room in missingUtility)
                {
                    _lbAutoReminders.Items.Add($"💡 Phòng {room}");
                    await _bll.AddNotificationAsync(null, "⚠️ NHẮC NHẬP CHỈ SỐ ĐIỆN NƯỚC", $"Phòng {room} chưa cập nhật chỉ số tháng này!", "Chưa đọc");
                }

                MessageBox.Show($"✅ Tạo {_lbAutoReminders.Items.Count} nhắc nhở thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<string>> GetOverdueInvoicesAsync()
        {
            var result = new List<string>();
            try
            {
                var invoices = await _bll.GetInvoicesAsync();
                if (invoices == null) return result;

                var today = DateTime.Today;
                foreach (DataRow row in invoices.Rows)
                {
                    var dueDate = row["DueDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(row["DueDate"]);
                    var remainingAmount = row["RemainingAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["RemainingAmount"]);

                    if (dueDate.HasValue && dueDate.Value < today && remainingAmount > 0)
                    {
                        var invoiceNumber = row["InvoiceNumber"].ToString();
                        var days = (today - dueDate.Value).Days;
                        result.Add($"HĐ {invoiceNumber} (quá {days} ngày, nợ {remainingAmount:C})");
                    }
                }
            }
            catch { }
            return result;
        }

        private async Task<List<string>> GetExpiredContractsAsync()
        {
            var result = new List<string>();
            try
            {
                var contracts = await _bll.GetContractsAsync();
                if (contracts == null) return result;

                var today = DateTime.Today;
                var threshold = today.AddDays(30);

                foreach (DataRow row in contracts.Rows)
                {
                    var endDate = row["EndDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(row["EndDate"]);
                    var status = row["Status"]?.ToString() ?? "";

                    if (endDate.HasValue && endDate.Value >= today && endDate.Value <= threshold && status != "Hủy bỏ")
                    {
                        var contractNumber = row["ContractNumber"].ToString();
                        var days = (endDate.Value - today).Days;
                        result.Add($"HĐ {contractNumber} (còn {days} ngày)");
                    }
                }
            }
            catch { }
            return result;
        }

        private async Task<List<string>> GetMissingUtilityReadingsAsync()
        {
            var result = new List<string>();
            try
            {
                var rooms = await _bll.GetRoomsAsync();
                var utilities = await _bll.GetUtilitiesAsync();

                if (rooms == null || utilities == null) return result;

                var today = DateTime.Today;
                var thisMonthReadings = utilities.AsEnumerable()
                    .Where(r => r["ReadingDate"] != DBNull.Value &&
                           Convert.ToDateTime(r["ReadingDate"]).Year == today.Year &&
                           Convert.ToDateTime(r["ReadingDate"]).Month == today.Month)
                    .Select(r => Convert.ToInt32(r["RoomId"]))
                    .Distinct()
                    .ToList();

                var activeRooms = rooms.AsEnumerable()
                    .Where(r => r["IsActive"] != DBNull.Value && Convert.ToBoolean(r["IsActive"]))
                    .Select(r => Convert.ToInt32(r["RoomId"]))
                    .ToList();

                foreach (var roomId in activeRooms)
                {
                    if (!thisMonthReadings.Contains(roomId))
                    {
                        var roomNumber = rooms.AsEnumerable()
                            .FirstOrDefault(r => Convert.ToInt32(r["RoomId"]) == roomId)?["RoomNumber"]?.ToString() ?? $"#{roomId}";
                        result.Add(roomNumber);
                    }
                }
            }
            catch { }
            return result;
        }

        private void ShowReminderDetails()
        {
            if (_lbAutoReminders.Items.Count == 0)
            {
                MessageBox.Show("Chưa có nhắc nhở nào. Hãy bấm 'Tạo Nhắc Nhở' để sinh dữ liệu.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region ========== TAB 3: THÔNG BÁO NỘI BỘ ==========

        private TabPage CreateInternalNotificationTab()
        {
            var tab = new TabPage { Text = "📢 Thông Báo Nội Bộ", BackColor = Color.White };

            var pnlForm = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                AutoScroll = true,
                BackColor = Color.White
            };

            int top = 20;

            var lblTitle = new Label
            {
                Text = "Tiêu Đề Thông Báo (*)",
                Location = new Point(20, top),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            _txtNotificationTitle = new TextBox
            {
                Location = new Point(240, top),
                Size = new Size(450, 36),
                Font = new Font("Segoe UI", 10)
            };
            top += 55;

            var lblContent = new Label
            {
                Text = "Nội Dung",
                Location = new Point(20, top),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            _txtNotificationContent = new TextBox
            {
                Location = new Point(240, top),
                Size = new Size(450, 140),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10),
                WordWrap = true
            };
            top += 160;

            var lblTarget = new Label
            {
                Text = "Gửi Tới",
                Location = new Point(20, top),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            _cboTarget = new ComboBox
            {
                Location = new Point(240, top),
                Size = new Size(450, 36),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            _cboTarget.Items.AddRange(new object[] { "Tất cả nhân viên", "Tất cả quản lý", "Toàn hệ thống" });
            _cboTarget.SelectedIndex = 2;
            top += 50;

            _btnSendInternal = CreateStyledButton("📤 Gửi Thông Báo", Color.FromArgb(52, 168, 219), 160);
            _btnSendInternal.Location = new Point(240, top);
            _btnSendInternal.Click += async (s, e) => await SendInternalNotificationAsync();

            pnlForm.Controls.AddRange(new Control[]
            {
                lblTitle, _txtNotificationTitle,
                lblContent, _txtNotificationContent,
                lblTarget, _cboTarget,
                _btnSendInternal
            });

            tab.Controls.Add(pnlForm);
            return tab;
        }

        private async Task SendInternalNotificationAsync()
        {
            if (string.IsNullOrWhiteSpace(_txtNotificationTitle.Text))
            {
                MessageBox.Show("Vui lòng nhập tiêu đề thông báo.", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _txtNotificationTitle.Focus();
                return;
            }

            try
            {
                var title = _txtNotificationTitle.Text.Trim();
                var content = _txtNotificationContent.Text.Trim();
                var target = _cboTarget.SelectedItem?.ToString() ?? "Toàn hệ thống";

                var fullMessage = string.IsNullOrWhiteSpace(content) ? $"[Đối tượng: {target}]" : $"{content}\n\n[Đối tượng: {target}]";

                await _bll.AddNotificationAsync(null, title, fullMessage, "Chưa đọc");

                MessageBox.Show("✅ Gửi thông báo nội bộ thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _txtNotificationTitle.Clear();
                _txtNotificationContent.Clear();
                _cboTarget.SelectedIndex = 2;
                _txtNotificationTitle.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ========== TAB 4: LỊCH SỬ GỬI ==========

        private TabPage CreateHistoryTab()
        {
            var tab = new TabPage { Text = "📜 Lịch Sử Gửi", BackColor = Color.White };

            _dgvHistory = CreateDataGrid();
            _dgvHistory.Dock = DockStyle.Fill;

            tab.Controls.Add(_dgvHistory);
            return tab;
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                var data = await _bll.GetNotificationsAsync();
                if (data != null)
                {
                    var history = data.AsEnumerable()
                        .Where(r => r["Status"].ToString() == "Đã gửi")
                        .ToList();
                    if (history.Count > 0)
                    {
                        _dgvHistory.DataSource = history.CopyToDataTable();
                    }
                    else
                    {
                        _dgvHistory.DataSource = data.Clone(); // Empty table with same columns
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi tải: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ========== HELPER METHODS ==========

        private DataGridView CreateDataGrid()
        {
            return new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = new DataGridViewRow { Height = 30 },
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                EnableHeadersVisualStyles = false,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 10)
                }
            };
        }

        private Button CreateStyledButton(string text, Color bgColor, int width = 100)
        {
            var btn = new Button
            {
                Text = text,
                BackColor = bgColor,
                ForeColor = Color.White,
                Width = width,
                Height = 40,
                Margin = new Padding(4),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private async Task LoadAllDataAsync()
        {
            await LoadNotificationsAsync();
            await LoadHistoryAsync();
        }

        #endregion
    }
}

