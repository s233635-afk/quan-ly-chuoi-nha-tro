# 💻 Code Snippets - Staff Dashboard v2.0

## Snippet 1: Menu Navigation Setup

```csharp
// Tất cả quyền nhân viên được phân loại theo nhóm
nav.Controls.Add(_btnOverview);
nav.Controls.Add(new Label { Text = "━━━━ QUẢN LÝ ━━━━", Height = 20, ForeColor = Color.White, AutoSize = true });
nav.Controls.Add(_btnRoom);          // 01. Quản lý phòng (Chi nhánh)
nav.Controls.Add(_btnTenant);        // 02. Quản lý khách thuê (Chi nhánh)
nav.Controls.Add(_btnContract);      // 03. Quản lý hợp đồng (Chi nhánh)

nav.Controls.Add(new Label { Text = "━━━━ TÀI CHÍNH ━━━━", Height = 20, ForeColor = Color.White, AutoSize = true });
nav.Controls.Add(_btnUtility);       // 04. Điện, nước, dịch vụ (Chi nhánh)
nav.Controls.Add(_btnPayment);       // 05. Thanh toán (Chi nhánh)

nav.Controls.Add(new Label { Text = "━━ BẢO TRÌNHÁNG ━━", Height = 20, ForeColor = Color.White, AutoSize = true });
nav.Controls.Add(_btnMaintenance);   // 06. Bảo trì & sự cố (Chi nhánh)
nav.Controls.Add(_btnAsset);         // 07. Tài sản phòng (Chi nhánh)

nav.Controls.Add(new Label { Text = "━━━━ BÁOCÁO ━━━━", Height = 20, ForeColor = Color.White, AutoSize = true });
nav.Controls.Add(_btnReport);        // 08. Báo cáo (Chi nhánh)

// Ẩn nút không được quyền truy cập
_btnDeposit.Visible = false;  // Admin only
_btnInvoice.Visible = false;  // Admin only
```

---

## Snippet 2: Dashboard Overview Header

```csharp
private void ShowOverview()
{
    SetActive(_btnOverview);
    _lblHeader.Text = "🏠 Tổng quan - Công việc hôm nay";
    ClearCurrentModule();

    _overviewHost = new Panel { Dock = DockStyle.Fill, BackColor = BackColor, Padding = new Padding(18) };
    
    // Tiêu đề chính - Chào hỏi cá nhân
    var title = new Label
    {
        Text = $"Xin chào, {_fullName}! 👋",
        Font = new Font("Segoe UI", 16, FontStyle.Bold),
        ForeColor = Color.FromArgb(33, 37, 41),
        AutoSize = true,
        Location = new Point(0, 0)
    };
    _overviewHost.Controls.Add(title);

    // Tiêu đề phụ
    var subTitle = new Label
    {
        Text = "Danh sách công việc cần xử lý hôm nay",
        Font = new Font("Segoe UI", 11),
        ForeColor = Color.FromArgb(108, 117, 125),
        AutoSize = true,
        Location = new Point(0, 32)
    };
    _overviewHost.Controls.Add(subTitle);
    
    // ... Grid setup ...
}
```

---

## Snippet 3: Branch Filtering

```csharp
// Tất cả dữ liệu được lọc theo chi nhánh của nhân viên
rooms = FilterByBranch(rooms, _branchId);
invoices = FilterByBranch(invoices, _branchId);
payments = FilterByBranch(payments, _branchId);
contracts = FilterByBranch(contracts, _branchId);
maintenance = FilterByBranch(maintenance, _branchId);

// Hàm lọc chung
private static DataTable FilterByBranch(DataTable dt, int? branchId)
{
    if (dt == null) return dt;
    if (!branchId.HasValue) return dt;
    if (!dt.Columns.Contains("BranchId")) return dt;

    var filtered = dt.Clone();
    foreach (DataRow r in dt.Rows)
    {
        if (int.TryParse(r["BranchId"]?.ToString(), out var b) && b == branchId.Value)
            filtered.ImportRow(r);
    }
    return filtered;
}
```

---

## Snippet 4: Metrics Calculation

```csharp
// 01. Quản lý phòng - Tính số phòng trống vs có người
int totalRooms = rooms?.Rows.Count ?? 0;
int occupied = rooms?.AsEnumerable().Count(r => 
    r.Table.Columns.Contains("CurrentStatusId") && 
    int.TryParse(r["CurrentStatusId"]?.ToString(), out var s) && 
    s != 1) ?? 0;
int empty = totalRooms - occupied;

// 05. Thanh toán - Tính tiền thu tháng này
decimal collectedThisMonth = payments?.AsEnumerable()
    .Where(r => r.Table.Columns.Contains("PaymentDate") && 
           DateTime.TryParse(r["PaymentDate"]?.ToString(), out var d) && 
           d.Year == DateTime.Today.Year && 
           d.Month == DateTime.Today.Month)
    .Sum(r => TryDecimal(r["PaymentAmount"])) ?? 0m;

// Công nợ - Tính tổng tiền chưa thu
decimal debt = invoices?.AsEnumerable()
    .Where(r => r.Table.Columns.Contains("RemainingAmount"))
    .Sum(r => TryDecimal(r["RemainingAmount"])) ?? 0m;

// 03. Quản lý hợp đồng - Hợp đồng sắp hết hạn (7 ngày)
int endingSoon = contracts?.AsEnumerable().Count(r => 
    r.Table.Columns.Contains("EndDate") && 
    DateTime.TryParse(r["EndDate"]?.ToString(), out var d) && 
    d.Date >= DateTime.Today && 
    d.Date <= DateTime.Today.AddDays(7)) ?? 0;

// 06. Bảo trì - Ticket chưa hoàn thành
int openMaintenance = maintenance?.AsEnumerable().Count(r => 
    !string.Equals(r["Status"]?.ToString(), "Done", StringComparison.OrdinalIgnoreCase)
    && !string.Equals(r["Status"]?.ToString(), "Hoàn tất", StringComparison.OrdinalIgnoreCase)
    && !string.Equals(r["Status"]?.ToString(), "Hoan tat", StringComparison.OrdinalIgnoreCase)) ?? 0;

// Tính hiệu suất % phòng cho thuê
int occupancyRate = totalRooms > 0 ? (occupied * 100 / totalRooms) : 0;
```

---

## Snippet 5: Metric Cards with Click Events

```csharp
// Grid 4 cột × 2 hàng = 8 thẻ
grid.Controls.Clear();

// Row 1 (Hàng đầu tiên)
// (0,0) - Phòng Trống
grid.Controls.Add(MakeMetricCard("🏠 Phòng Trống", 
    $"{empty}/{totalRooms}", 
    "Sẵn sàng cho thuê", 
    Color.FromArgb(0, 150, 136),    // Teal
    (s, e) => _btnRoom.PerformClick()), 0, 0);

// (1,0) - Thu tháng này
grid.Controls.Add(MakeMetricCard("💳 Thu tháng này", 
    $"{collectedThisMonth:N0}₫", 
    "Tiền đã thu tháng hiện tại", 
    Color.FromArgb(76, 175, 80),    // Green
    (s, e) => _btnPayment.PerformClick()), 1, 0);

// (2,0) - Quá hạn
grid.Controls.Add(MakeMetricCard("⏰ Quá hạn", 
    $"{overdueCount}", 
    "Hóa đơn chưa thanh toán", 
    Color.FromArgb(255, 152, 0),    // Orange
    (s, e) => _btnReport.PerformClick()), 2, 0);

// (3,0) - Công nợ
grid.Controls.Add(MakeMetricCard("🧾 Công nợ", 
    $"{debt:N0}₫", 
    "Tổng tiền còn nợ", 
    Color.FromArgb(244, 67, 54),    // Red
    (s, e) => _btnReport.PerformClick()), 3, 0);

// Row 2 (Hàng thứ hai)
// (0,1) - Sắp hết hạn
grid.Controls.Add(MakeMetricCard("📄 Sắp hết hạn", 
    $"{endingSoon}", 
    "Hợp đồng trong 7 ngày", 
    Color.FromArgb(103, 58, 183),   // Purple
    (s, e) => _btnContract.PerformClick()), 0, 1);

// (1,1) - Bảo trì mở
grid.Controls.Add(MakeMetricCard("🔧 Bảo trì mở", 
    $"{openMaintenance}", 
    "Ticket chưa hoàn thành", 
    Color.FromArgb(156, 39, 176),   // Magenta
    (s, e) => _btnMaintenance.PerformClick()), 1, 1);

// (2,1) - Phòng Có người
grid.Controls.Add(MakeMetricCard("👥 Phòng Có người", 
    $"{occupied}/{totalRooms}", 
    "Đang cho thuê", 
    Color.FromArgb(33, 150, 243),   // Blue
    (s, e) => _btnRoom.PerformClick()), 2, 1);

// (3,1) - Hiệu suất
grid.Controls.Add(MakeMetricCard("⭐ Hiệu suất", 
    $"{occupancyRate}%", 
    "Tỷ lệ phòng cho thuê", 
    Color.FromArgb(0, 172, 193),    // Cyan
    (s, e) => _btnRoom.PerformClick()), 3, 1);
```

---

## Snippet 6: Create Metric Card

```csharp
private Panel MakeMetricCard(string title, string value, string subtitle, Color accent, EventHandler onClick)
{
    var card = new Panel
    {
        Dock = DockStyle.Fill,
        Margin = new Padding(8),
        BackColor = Color.White,
        Cursor = Cursors.Hand
    };

    // Thanh màu bên trái
    var bar = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accent };
    
    // Tiêu đề
    var lblTitle = new Label 
    { 
        Text = title, 
        AutoSize = true, 
        Font = new Font("Segoe UI", 11, FontStyle.Bold), 
        Location = new Point(14, 16) 
    };
    
    // Giá trị (lớn)
    var lblValue = new Label 
    { 
        Text = value, 
        AutoSize = true, 
        Font = new Font("Segoe UI", 18, FontStyle.Bold), 
        ForeColor = accent, 
        Location = new Point(14, 44) 
    };
    
    // Mô tả
    var lblSub = new Label 
    { 
        Text = subtitle, 
        AutoSize = true, 
        ForeColor = Color.Gray, 
        Location = new Point(14, 86) 
    };

    card.Controls.Add(bar);
    card.Controls.Add(lblTitle);
    card.Controls.Add(lblValue);
    card.Controls.Add(lblSub);

    // Click event - Điều hướng đến module liên quan
    card.Click += onClick;
    foreach (Control c in card.Controls) 
        c.Click += onClick;
    
    return card;
}
```

---

## Snippet 7: Load Module Title Update

```csharp
// Các tiêu đề được cập nhật rõ ràng là "(Chi nhánh)"

_btnRoom = MakeNavButton("🏠 Phòng", (s, e) => 
{ 
    SetActive(_btnRoom); 
    LoadModule(new FrmDataViewer("Trạng thái Phòng (Chi nhánh)", LoadRoomsAsync), "🏠 Phòng"); 
});

_btnUtility = MakeNavButton("⚡ Điện/Nước/DV", (s, e) => 
{ 
    SetActive(_btnUtility); 
    LoadModule(new FrmDataViewer("Điện - Nước - Dịch vụ (Chi nhánh)", LoadUtilitiesAsync), "⚡ Điện/Nước/DV"); 
});

_btnMaintenance = MakeNavButton("🔧 Bảo trì", (s, e) => 
{ 
    SetActive(_btnMaintenance); 
    LoadModule(new FrmDataViewer("Bảo trì & Sự cố (Chi nhánh)", LoadMaintenanceAsync), "🔧 Bảo trì"); 
});

_btnAsset = MakeNavButton("📦 Tài sản", (s, e) => 
{ 
    SetActive(_btnAsset); 
    LoadModule(new FrmDataViewer("Tài sản Phòng (Chi nhánh)", LoadAssetsAsync), "📦 Tài sản"); 
});

_btnReport = MakeNavButton("📊 Báo cáo", (s, e) => 
{ 
    SetActive(_btnReport); 
    LoadModule(new FrmDataViewer("Báo cáo Chi Nhánh", LoadInvoicesAsync), "📊 Báo cáo"); 
});
```

---

## Snippet 8: Async Loading

```csharp
private async Task LoadOverviewAsync(TableLayoutPanel grid, Label loadingLabel)
{
    try
    {
        // Load dữ liệu từ BLL
        var rooms = await _bll.GetRoomsAsync();
        var invoices = await _bll.GetInvoicesViewAsync();
        var payments = await _bll.GetPaymentsViewAsync();
        var contracts = await _bll.GetContractsAsync();
        var maintenance = await _bll.GetMaintenanceAsync();

        // Lọc dữ liệu theo chi nhánh của nhân viên
        rooms = FilterByBranch(rooms, _branchId);
        invoices = FilterByBranch(invoices, _branchId);
        payments = FilterByBranch(payments, _branchId);
        contracts = FilterByBranch(contracts, _branchId);
        maintenance = FilterByBranch(maintenance, _branchId);

        // Tính toán metrics
        // ... (xem Snippet 4)
        
        // Cập nhật UI với metrics
        // ... (xem Snippet 5)
        
        loadingLabel.Visible = false;
    }
    catch (Exception ex)
    {
        loadingLabel.Text = "Không thể tải thống kê: " + ex.Message;
        loadingLabel.Visible = true;
    }
}
```

---

## 🎯 Sử Dụng

1. **Copy & Paste**: Các snippet trên có thể dùng ngay
2. **Reference**: Sử dụng làm tài liệu tham khảo
3. **Tùy Chỉnh**: Thay đổi color, icon, text theo nhu cầu
4. **Testing**: Kiểm tra từng snippet riêng lẻ

---

**Phiên Bản**: 2.0  
**Cập Nhật**: 14/12/2025  
