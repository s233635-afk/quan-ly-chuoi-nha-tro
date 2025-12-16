## 🔄 Quy Trình Luồng Dữ Liệu - Cập Nhật Khách Thuê

### Sơ Đồ Tương Tác Giữa Các Form

```
┌─────────────────────────────────────────────────────────────────┐
│              FrmDepositManager (Danh Sách Cọc)                   │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Deposit Grid:                                           │   │
│  │  ├─ TenantName   │ RoomNo │ Amount │ Status             │   │
│  │  ├─ Nguyễn A     │ 101    │ 5M     │ Pending  ← CLICK   │   │
│  │  ├─ Trần B       │ 102    │ 3M     │ Done               │   │
│  │  └─ Lê C         │ 103    │ 2M     │ Returned           │   │
│  └──────────────────────────────────────────────────────────┘   │
│                           ▲                                       │
│                           │                                       │
│                      DATA RELOAD                                  │
│                    (khi OK nhận)                                  │
│                           │                                       │
└───────────────────────────┼───────────────────────────────────────┘
                            │
                            │ ShowDialog(DialogResult.OK)
                            │
┌───────────────────────────▼───────────────────────────────────────┐
│           FrmTenantQuickInfo (Form Popup)                          │
│                                                                    │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │  Header: Khách Thuê #123                             ✕    │  │
│  ├────────────────────────────────────────────────────────────┤  │
│  │                                                            │  │
│  │  ┌─ Thông Tin ─┐  ┌─ Hợp Đồng ─┐  ┌─ Lịch Sử ─┐        │  │
│  │  │             │  │            │  │          │        │  │
│  │  │ THÔNG TIN CÁ NHÂN          │  │ HĐ Grid  │        │  │
│  │  │ ├─ Họ và tên:  [__Nguyễn A__]  │          │  │HS Grid  │        │  │
│  │  │ ├─ CMND:      [__123456789__]  │          │  │          │        │  │
│  │  │ ├─ Ngày sinh: [__01/01/1990__] │          │  │          │        │  │
│  │  │ └─ Địa chỉ:   [__123 Nguyễn Hữu Cảnh__]   │          │  │          │        │  │
│  │  │                                │  │          │  │          │        │  │
│  │  │ THÔNG TIN LIÊN HỆ              │  │ ├─ Contract ID    │  │ ├─ HistoryId│        │  │
│  │  │ ├─ Điện thoại: [__0123456789__]  │  │ ├─ Số HĐ        │  │ ├─ Phòng   │        │  │
│  │  │ └─ Email:      [__abc@email__]   │  │ ├─ Ngày bắt đầu │  │ ├─ Check-in│        │  │
│  │  │                                │  │ └─ Status       │  │ └─ Check-out│        │  │
│  │  │ THÔNG TIN TẠM TRÚ               │  │            │  │          │        │  │
│  │  │ ├─ Sổ tạm trú: [__TT-2024-001__] │  │            │  │          │        │  │
│  │  │ ├─ Ngày lập:   [__01/01/2024__]  │  │            │  │          │        │  │
│  │  │ └─ Hết hạn:    [__31/12/2024__]  │  │            │  │          │        │  │
│  │  │                                │  │            │  │          │        │  │
│  │  └────────────────────────────────┘  └────────────┘  └──────────┘        │  │
│  │                                                            │  │
│  ├────────────────────────────────────────────────────────────┤  │
│  │  [Sửa] [Lưu-hidden] [Hủy-hidden]                         │  │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
                            │
                            │
                  User Click [Sửa]
                            │
                            ▼
                    ┌────────────────┐
                    │  Edit Mode ON  │
                    │ [Sửa→hidden]   │
                    │ [Lưu→visible]  │
                    │ [Hủy→visible]  │
                    │                │
                    │ TextBox editable
                    └────────────────┘
                            │
                   User chỉnh sửa + ấn [Lưu]
                            │
                            ▼
                 ┌──────────────────────┐
                 │ SaveTenantInfoAsync()│
                 ├──────────────────────┤
                 │ 1. Validate input    │
                 │ 2. UpdateTenant DB   │
                 │ 3. ReloadContracts   │
                 │ 4. ReloadHistory     │
                 │ 5. Result = OK       │
                 │ 6. Auto toggle       │
                 └──────────────────────┘
                            │
                            ▼
              Form Close (DialogResult = OK)
                            │
                            ▼
         FrmDepositManager nhận DialogResult.OK
                            │
                            ▼
            ShowTenantQuickInfo() checks:
            if (result == DialogResult.OK)
                            │
                            ▼
                    LoadDataAsync()
                            │
                            ▼
           ✅ Deposit Grid tự động cập nhật
           (Tên khách, số điện thoại, etc.)
```

---

### 🔍 Chi Tiết Mỗi Bước

#### **1️⃣ User Click Dòng Khách Thuê**
```
Event: _grid.CellClick event
Xử lý: ShowTenantQuickInfo(rowIndex, columnIndex)
```

#### **2️⃣ Mở Form Popup**
```csharp
using (var frm = new FrmTenantQuickInfo(_bll, tenantId))
{
    var result = frm.ShowDialog(this);  // Dừng đợi kết quả
    
    if (result == DialogResult.OK)      // ✅ Nếu lưu thành công
    {
        await LoadDataAsync();           // Reload danh sách
    }
}
```

#### **3️⃣ Form Hiển Thị Dữ Liệu**
```
Load event → LoadDataAsync()
  ├─ GetTenantsAsync() → Lấy thông tin cơ bản
  ├─ GetContractsAsync() → Lấy danh sách hợp đồng
  └─ GetTenantHistoryAsync() → Lấy lịch sử
```

#### **4️⃣ User Ấn Nút "Sửa"**
```csharp
_btnEdit.Click += ToggleEditMode(_pnlBasic);
// ToggleEditMode():
//   ├─ _isEditMode = true
//   ├─ TextBox.ReadOnly = false
//   ├─ Hide [Sửa]
//   └─ Show [Lưu], [Hủy]
```

#### **5️⃣ User Ấn Nút "Lưu"**
```csharp
private async Task SaveTenantInfoAsync()
{
    // 1. Lấy dữ liệu từ TextBox
    var fullName = GetTextBoxValue("FullName");
    // ... các field khác ...
    
    // 2. Gọi BLL cập nhật database
    bool result = await _bll.UpdateTenantAsync(
        _tenantId, fullName, identityCard, ... true
    );
    
    if (result)
    {
        // 3. Thông báo thành công
        MessageBox.Show("Cập nhật thành công!");
        
        // 4. Auto-reload Hợp Đồng & Lịch Sử
        await ReloadContractsAndHistoryAsync();
        
        // 5. Signal parent form
        this.DialogResult = DialogResult.OK;  // ⭐ QUAN TRỌNG
        
        // 6. Tự động toggle về view mode
        ToggleEditMode(_pnlBasic);
    }
}
```

#### **6️⃣ ReloadContractsAndHistoryAsync()**
```csharp
private async Task ReloadContractsAndHistoryAsync()
{
    try
    {
        // Lấy dữ liệu mới
        _contracts = await _bll.GetContractsAsync();
        FilterContractsByTenant(_contracts, _tenantId);
        _gridContracts.DataSource = _contracts;  // Cập nhật grid
        FormatContractsGrid(_gridContracts);
        
        _history = await _bll.GetTenantHistoryAsync();
        FilterHistoryByTenant(_history, _tenantId);
        _gridHistory.DataSource = _history;      // Cập nhật grid
        FormatHistoryGrid(_gridHistory);
    }
}
```

#### **7️⃣ Form Đóng & Parent Reload**
```
Form close → DialogResult = OK
     ↓
ShowTenantQuickInfo() nhận kết quả
     ↓
if (result == DialogResult.OK) ✅
     ↓
await LoadDataAsync()
     ↓
Deposit Grid refresh với dữ liệu mới
```

---

### 📊 State Transitions

```
┌──────────────┐
│  View Mode   │  Buttons: [Sửa]
│              │  TextBox: ReadOnly
└──────┬───────┘
       │ Click [Sửa]
       ▼
┌──────────────┐
│  Edit Mode   │  Buttons: [Lưu] [Hủy]
│              │  TextBox: Editable
└──────┬───────┘
       │
       ├─ Click [Hủy] → View Mode
       │
       └─ Click [Lưu] → Save → ReloadTabs → Close → OK
```

---

### 🎯 Key Points

1. **DialogResult.OK là Signal**: Thay vì public event/delegate, dùng DialogResult để communicate
2. **Async Reload**: FrmDepositManager.LoadDataAsync() async, không block UI
3. **Auto-Update Tabs**: ReloadContractsAndHistoryAsync() gọi automatically khi save
4. **Tab Spacing**: ItemSize(200, 45) đủ rộng cho text Vietnamese
5. **Error Handling**: Tất cả catch exception + MessageBox

