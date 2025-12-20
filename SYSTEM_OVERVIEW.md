# 📚 TOÀN CẢNH HỆ THỐNG QUẢN LÝ CHUỖI NHÀ TRỌ

**Ngày tạo**: 20/12/2025  
**Phiên bản**: 2.0 - Full Admin + Staff System  
**Trạng thái**: ✅ HOÀN THÀNH & CHẠY ĐƯỢC

---

## 🎯 TỔNG QUAN

Ứng dụng **Quản Lý Chuỗi Nhà Trọ** là hệ thống quản lý hoàn chỉnh cho các chuỗi nhà trọ/ký túc xá, hỗ trợ:
- **Admin**: Quản lý toàn bộ 12 mô-đun hệ thống
- **Staff**: Quản lý dữ liệu theo chi nhánh được phân công

**Kiến trúc**: 3-Tier (GUI → BLL → DAL → SQL Server)  
**Framework**: .NET Framework 4.7.2 WinForms  
**Database**: SQL Server (SmarterASP.NET hoặc LocalDB)

---

## 🏗️ KIẾN TRÚC HỆ THỐNG

```
┌─────────────────────────────────────────────────────┐
│              GUI LAYER (Windows Forms)              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐          │
│  │ Admin    │  │ Staff    │  │ Utilities│          │
│  │Dashboard │  │Dashboard │  │ Forms    │          │
│  └──────────┘  └──────────┘  └──────────┘          │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│         BUSINESS LOGIC LAYER (BLL)                  │
│  ┌──────────────┐  ┌──────────────┐                │
│  │AdminDataBLL  │  │StaffBLL      │ UserBLL BranchBLL
│  │              │  │              │                │
│  │- 12 modules  │  │- Branch-      │                │
│  │- CRUD ops    │  │  filtered     │                │
│  │- Validation  │  │- Staff access │                │
│  └──────────────┘  └──────────────┘                │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│    DATA ACCESS LAYER (DAL)                          │
│  ┌──────────────┐  ┌──────────────┐                │
│  │AdminDataDAL  │  │StaffDAL      │                │
│  │              │  │              │                │
│  │SQL queries   │  │Branch filter  │                │
│  │Stored procs  │  │Async methods  │ DatabaseHelper
│  └──────────────┘  └──────────────┘                │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│         SQL SERVER DATABASE                         │
│  db_ac1f11_quanlynhatro (18 tables)                │
└─────────────────────────────────────────────────────┘
```

---

## 📁 CẤU TRÚC THƯ MỤC

```
quan-ly-chuoi-nha-tro/
│
├── GUI/                          (45+ form files)
│   ├── FrmLogin.cs              ✅ Đăng nhập
│   ├── FrmRegister.cs           ✅ Đăng ký (disabled)
│   ├── FrmAdminDashboard.cs     ✅ Dashboard Admin
│   ├── FrmStaffDashboard.cs     ✅ Dashboard Staff
│   ├── FrmBranch.cs             ✅ Quản lý chi nhánh
│   ├── FrmRoomManager.cs        ✅ Quản lý phòng
│   ├── FrmTenantManager.cs      ✅ Quản lý khách
│   ├── FrmContractManager.cs    ✅ Quản lý hợp đồng
│   ├── FrmDepositManager.cs     ✅ Quản lý tiền cọc
│   ├── FrmInvoiceManager.cs     ✅ Quản lý hóa đơn
│   ├── FrmPaymentManager.cs     ✅ Quản lý thanh toán
│   ├── FrmUtilityManager.cs     ✅ Quản lý tiện ích
│   ├── FrmMaintenanceManager.cs ✅ Quản lý bảo trì
│   ├── FrmAssetManager.cs       ✅ Quản lý tài sản
│   ├── FrmNotificationManager.cs ✅ Quản lý thông báo
│   ├── FrmSystemSettingsManager.cs ✅ Cài đặt hệ thống
│   ├── FrmReportManager.cs      ✅ Báo cáo/Thống kê
│   └── [Various Editors & Helpers]
│
├── BLL/                          (4 business logic files)
│   ├── AdminDataBLL.cs          ✅ Admin operations
│   ├── StaffBLL.cs              ✅ Staff operations
│   ├── UserBLL.cs               ✅ User authentication
│   └── BranchBLL.cs             ✅ Branch operations
│
├── DAL/                          (6 data access files)
│   ├── DatabaseHelper.cs        ✅ Database connection
│   ├── AdminDataDAL.cs          ✅ Admin SQL queries
│   ├── StaffDAL.cs              ✅ Staff SQL queries
│   ├── BranchDAL.cs             ✅ Branch SQL queries
│   ├── InvoicePaymentDAL.cs     ✅ Invoice operations
│   └── DatabaseInitializer.cs   ✅ DB initialization
│
├── Properties/                   (Assembly metadata)
│   └── AssemblyInfo.cs
│
├── App.config                    ✅ Database connection
├── Program.cs                    ✅ Entry point
└── quan-ly-chuoi-nha-tro.csproj  ✅ Project file
```

---

## 🗄️ DATABASE SCHEMA (18 BẢNG)

### 1. **Người Dùng & Phân Quyền** (2 bảng)
- `Roles` - Vai trò (Admin: 1, Staff: 2)
- `Users` - Tài khoản (Username, Password, RoleId, BranchId)

### 2. **Chi Nhánh** (2 bảng)
- `Branches` - Chi nhánh (BranchCode, BranchName, Address)
- `BranchSections` - Dãy/Khu (SectionCode, BranchId)

### 3. **Phòng** (3 bảng)
- `RoomTypes` - Loại phòng (RoomTypeName, DefaultPrice)
- `RoomStatuses` - Trạng thái phòng (Trống, Có người, Bảo trì)
- `Rooms` - Phòng (RoomNumber, BranchId, RoomPrice, CurrentStatusId)

### 4. **Khách Thuê** (2 bảng)
- `Tenants` - Khách thuê (FullName, IdentityCard, PhoneNumber)
- `Dependents` - Người ở chung (TenantId, FullName, Relationship)

### 5. **Hợp Đồng & Lịch Sử** (3 bảng)
- `Contracts` - Hợp đồng (TenantId, RoomId, StartDate, EndDate)
- `Deposits` - Tiền cọc (DepositAmount, DepositDate, Status)
- `TenantRoomHistory` - Lịch sử phòng (CheckInDate, CheckOutDate)

### 6. **Tiện Ích** (2 bảng)
- `UtilityTypes` - Loại tiện ích (Điện, Nước, Dịch vụ)
- `UtilityReadings` - Chỉ số tiện ích (MeterReading, ReadingDate)

### 7. **Hóa Đơn & Thanh Toán** (2 bảng)
- `Invoices` - Hóa đơn (InvoiceNumber, TenantId, TotalAmount)
- `Payments` - Thanh toán (InvoiceId, PaidAmount, PaymentDate)

### 8. **Bảo Trì & Tài Sản** (2 bảng)
- `Maintenance` - Bảo trì (TicketNumber, Description, Status)
- `Assets` - Tài sản (AssetName, Quantity, Condition, RoomId)

### 9. **Thông Báo & Cài Đặt** (2 bảng)
- `Notifications` - Thông báo (Title, Content, SentDate)
- `SystemSettings` - Cài đặt hệ thống (SettingKey, SettingValue)

---

## 👥 QUẢN LÝ QUYỀN TRUY CẬP

### Admin (RoleId = 1)
- ✅ Truy cập toàn bộ 12 mô-đun
- ✅ Quản lý tất cả chi nhánh
- ✅ Xem dữ liệu từ tất cả branch
- ✅ Quản lý tài khoản nhân viên

**Dashboard Admin**: FrmAdminDashboard
- Sidebar: 13 button điều hướng
- Thống kê tổng quan: Phòng, khách, hợp đồng, công nợ...
- Mở các form manager theo yêu cầu

### Staff (RoleId = 2)
- ✅ Chỉ quản lý dữ liệu của chi nhánh được phân công
- ✅ Được phép: Room, Tenant, Contract, Deposit, Invoice, Payment, Utility, Maintenance, Asset
- ❌ Không được: Cài đặt hệ thống, Quản lý nhân viên

**Dashboard Staff**: FrmStaffDashboard
- Header: Hiển thị tên nhân viên + Chi nhánh
- 11 menu tương ứng
- Tất cả query lọc theo `branchId` = Chi nhánh của staff

**Branch Filtering**:
```csharp
// Tất cả BLL/DAL methods cho Staff:
public Task<DataTable> GetTenantsByBranchAsync(int? branchId)
    => _dbHelper.GetTenantsByBranchAsync(branchId);

// Khi gọi từ Staff form:
await _bll.GetTenantsByBranchAsync(_branchId);  // _branchId = branch của staff
```

---

## 🎯 12 Mmodules ADMIN

### 1. **📊 Tổng Quan** ✅
- Dashboard thống kê
- Biểu đồ, số liệu chính

### 2. **🏢 Chi Nhánh** ✅
- CRUD chi nhánh
- Quản lý dãy/khu
- Danh sách phòng theo chi nhánh

### 3. **🚪 Phòng** ✅
- CRUD phòng
- Quản lý loại phòng + trạng thái
- Lọc theo: Branch, Trạng thái, Kích hoạt
- Đổi trạng thái, giá tiền

### 4. **👥 Khách Thuê** ✅
- CRUD khách
- Thêm/sửa người ở chung
- Lịch sử ở nhà
- Lưu ảnh CCCD

### 5. **📜 Hợp Đồng** ✅
- CRUD hợp đồng
- Trạng thái: Active, Extended, Terminated, Expired
- Liên kết: Khách - Phòng - Chi nhánh

### 6. **💰 Tiền Cọc** ✅
- CRUD phiếu cọc
- Loại: Booking (đặt cọc), Official (cọc chính)
- Trạng thái: Pending, Confirmed, Returned, Cancelled

### 7. **💡 Tiện Ích** ✅
- CRUD loại tiện ích (Điện, Nước, Dịch vụ)
- Nhập chỉ số (UtilityReadings)
- Tính toán chi phí

### 8. **🧾 Hóa Đơn** ✅
- CRUD hóa đơn
- Tạo hóa đơn tháng tự động
- Xem công nợ
- Tính tiền: RentalCost + UtilityCost + OtherCost

### 9. **💳 Thanh Toán** ✅
- Ghi nhận thanh toán
- Liên kết với hóa đơn
- Tracking: PaidAmount, RemainingAmount

### 10. **🔧 Bảo Trì** ✅
- CRUD ticket bảo trì
- Trạng thái: Open, In Progress, Completed, Cancelled
- Ưu tiên: Low, Medium, High, Critical
- Phân công nhân viên

### 11. **📦 Tài Sản** ✅
- CRUD tài sản
- Liên kết: Phòng (RoomId)
- Theo dõi: Số lượng, Tình trạng, Giá trị

### 12. **📢 Thông Báo** ✅
- CRUD thông báo
- Lịch sử gửi
- Đối tượng: Tất cả, Staff, Khách

### 13. **⚙️ Cài Đặt** ✅
- CRUD cài đặt hệ thống
- Cấu hình chung

---

## 🔄 LUỒNG HOẠT ĐỘNG CHÍNH

### 1. **Đăng Nhập** (FrmLogin)
```
User nhập username + password
            ↓
UserBLL.LoginAsync()
            ↓
DatabaseHelper.LoginAsync()
            ↓
Kiểm tra Users table
            ↓
Lấy RoleId (1=Admin, 2=Staff)
            ↓
├─ RoleId=1 → Mở FrmAdminDashboard(username, userId)
└─ RoleId=2 → Mở FrmStaffDashboard(username, fullName, branchId, userId)
```

### 2. **Admin Quản Lý Khách Thuê**
```
FrmAdminDashboard.btnNavTenant_Click()
            ↓
new FrmTenantManager(adminDataBLL, branchId=null)
            ↓
LoadDataAsync() → await adminDataBLL.GetTenantsAsync()
            ↓
Hiển thị tất cả khách (không filter)
            ↓
User click [Sửa] → FrmTenantEditor(tenantId, mode=Edit)
            ↓
Lưu → BLL.UpdateTenantAsync()
            ↓
Reload danh sách
```

### 3. **Staff Quản Lý Khách Thuê (Chi nhánh)**
```
FrmStaffDashboard.btnTenant_Click()
            ↓
new FrmTenantManager(staffBLL, branchId=_branchId)
            ↓
LoadDataAsync() → await staffBLL.GetTenantsByBranchAsync(_branchId)
            ↓
Chỉ hiển thị khách của chi nhánh này
            ↓
User click [Sửa] → FrmTenantEditor(tenantId, branchId=_branchId)
            ↓
Lưu → BLL.UpdateTenantAsync()
            ↓
Reload danh sách (branch-filtered)
```

---

## 📊 PATTERN & CONVENTIONS

### Async/Await Pattern
```csharp
// BLL (delegates to DAL)
public Task<DataTable> GetTenantsAsync() 
    => _dbHelper.GetTenantsAsync();

// GUI (load in background)
Load += async (s, e) => await LoadDataAsync();
private async Task LoadDataAsync() {
    _tenants = await _bll.GetTenantsAsync();
    _grid.DataSource = _tenants;
}
```

### CRUD Pattern
```csharp
// BLL methods follow naming:
GetXxxAsync()           // Read
AddXxxAsync(params)     // Create
UpdateXxxAsync(id, params)  // Update
DeleteXxxAsync(id)      // Delete (soft delete with IsActive=false)
```

### Form Constructor Pattern
```csharp
// Admin (all branches)
public FrmTenantManager(AdminDataBLL bll = null) 
{
    _bll = bll ?? new AdminDataBLL();
    _branchId = null;  // Admin sees all
}

// Staff (single branch)
public FrmTenantManager(StaffBLL bll, int branchId) 
{
    _bll = bll ?? new StaffBLL();
    _branchId = branchId;  // Staff sees only their branch
}
```

### Branch Filtering
```csharp
// DAL signature
public Task<DataTable> GetTenantsAsync(int? branchId = null)
    => ExecuteAsync("sp_GetTenants", new { branchId });

// BLL wrapper
public Task<DataTable> GetTenantsByBranchAsync(int? branchId = null)
    => _dbHelper.GetTenantsByBranchAsync(branchId);

// GUI usage
// Admin
await _adminBll.GetTenantsAsync();  // branchId = null → all

// Staff
await _staffBll.GetTenantsByBranchAsync(_branchId);  // branchId = 1 → only branch 1
```

### Naming Conventions
| Loại | Format | Ví dụ |
|------|--------|-------|
| Async methods | `XxxAsync` | `GetTenantsAsync()` |
| Form classes | `Frm` prefix | `FrmTenantManager` |
| Button controls | `_btn` prefix | `_btnAdd`, `_btnEdit` |
| Label controls | `_lbl` prefix | `_lblTotal` |
| Grid controls | `_dgv` prefix | `_dgvTenants` |
| TextBox controls | `_txt` prefix | `_txtSearch` |
| Private fields | `_` prefix | `_bll`, `_branchId` |
| Constants | `UPPER_SNAKE_CASE` | `DefaultTimeout = 10` |

---

## 🔐 DATABASE CONNECTION

**Location**: `quan-ly-chuoi-nha-tro/DAL/DatabaseHelper.cs`

**Connection Strings** (từ App.config):
```xml
<!-- SQL Server SmarterASP.NET (mặc định) -->
<add name="QuanLyNhaTro" 
     connectionString="Data Source=SQL9001.site4now.net;Initial Catalog=db_ac1f11_quanlynhatro;User Id=db_ac1f11_quanlynhatro_admin;Password=admin123;" />

<!-- LocalDB (khi UseLocalDb=true) -->
Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=db_ac1f11_quanlynhatro;Integrated Security=True
```

**Connection Management**:
- ✅ Async connection opening
- ✅ Timeout: 10 seconds (configurable)
- ✅ Anti SQL Injection (Parameterized queries)
- ✅ Error handling: TimeoutException, SqlException

---

## 🛠️ CÔNG NGHỆ SỬ DỤNG

- **Framework**: .NET Framework 4.7.2
- **GUI**: Windows Forms (WinForms)
- **Database**: SQL Server 2019+
- **Libraries**:
  - System.Data.SqlClient (Database access)
  - System.Configuration (Config reading)
  - System.Threading.Tasks (Async operations)
- **Design Patterns**: 
  - 3-Tier Architecture
  - Repository Pattern (DAL)
  - CRUD Operations
  - Soft Delete (IsActive flag)
  - RBAC (Role-Based Access Control)

---

## 📋 TRẠNG THÁI HOÀN THÀNH

### Build Status
```
✅ Build SUCCESS (0 Errors, 0 Warnings)
✅ All NuGet packages resolved
✅ Target Framework: .NET Framework 4.7.2
✅ Platform: Windows x86/x64
```

### Features Status
```
✅ Login/Register
✅ Admin Dashboard (13 modules)
✅ Staff Dashboard (11 modules)
✅ 18 Database Tables
✅ Async/Await Pattern
✅ Role-Based Access Control
✅ Branch Filtering
✅ Sample Data Included
```

### Testing
```
✅ Sample Data: sample_data.sql
✅ Database Schema: setup_database_final.sql
✅ Test Admin: admin / 123456
✅ All modules functional
```

---

## 📈 THỐNG KÊ DỰ ÁN

| Metric | Giá Trị |
|--------|--------|
| **Total C# Files** | 50+ |
| **GUI Forms** | 45+ |
| **BLL Classes** | 4 |
| **DAL Classes** | 6 |
| **Database Tables** | 18 |
| **Async Methods** | 100+ |
| **CRUD Operations** | 150+ |
| **Lines of Code** | ~15,000 |

---

## 🚀 CÁCH CHẠY

### 1. **Chuẩn Bị Database**
```sql
-- Chạy setup_database_final.sql để tạo schema
-- Chạy sample_data.sql để thêm dữ liệu mẫu
-- Hoặc dùng UseLocalDb=true trong App.config
```

### 2. **Build Solution**
```
File → Open → quan-ly-chuoi-nha-tro.sln
Ctrl+Shift+B → Build Solution
```

### 3. **Chạy Ứng Dụng**
```
Ctrl+F5 (Run without debugging)
hoặc F5 (Debug)
```

### 4. **Đăng Nhập**
```
Username: admin
Password: 123456
```

---

## 📚 TÀI LIỆU THÊM

- `README.md` - Mô tả tính năng chi tiết
- `.github/copilot-instructions.md` - Hướng dẫn phát triển
- `QUICK_REFERENCE.md` - Tham khảo nhanh
- `STAFF_GUIDE.md` - Hướng dẫn cho nhân viên
- `Database/setup_database_final.sql` - Database schema
- `sample_data.sql` - Sample data

---

**✅ Hệ thống hoàn thành và sẵn sàng sử dụng!**
