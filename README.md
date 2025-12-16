# Quản Lý Chuỗi Nhà Trọ - Admin Dashboard

Ứng dụng WinForms quản lý chuỗi nhà trọ toàn diện theo chế độ **Admin-only** (chỉ tài khoản Admin đăng nhập và sử dụng).

## 🎯 Tính Năng Admin (12 mô-đun)

1. **🏢 Quản Lý Chi Nhánh** - HOÀN THÀNH ✅
   - Thêm, sửa, xóa chi nhánh
   - Quản lý thông tin chi nhánh (địa chỉ, điện thoại, quản lý)
   - Trạng thái hoạt động

2. **🚪 Quản Lý Phòng** - HOÀN THÀNH ✅
   - CRUD phòng + lọc theo chi nhánh/trạng thái/kích hoạt
   - Quản lý loại phòng, trạng thái phòng, khu/Block (BranchSection)
   - Thiết lập giá/diện tích/tầng

3. **🧑 Quản Lý Khách Thuê** - HOÀN THÀNH ✅
   - CRUD khách thuê + bật/tắt hoạt động
   - Quản lý người phụ thuộc + lịch sử ở phòng
   - Lưu đường dẫn ảnh CCCD mặt trước/mặt sau (FrontIdPhoto/BackIdPhoto)

4. **📜 Quản Lý Hợp Đồng** - HOÀN THÀNH ✅
   - CRUD hợp đồng, trạng thái hợp đồng, ngày bắt đầu/kết thúc

5. **💰 Quản Lý Ký Cược** - HOÀN THÀNH ✅
   - CRUD phiếu cọc theo hợp đồng/khách/phòng

6. **💡 Quản Lý Tiện Ích** - HOÀN THÀNH ✅
   - CRUD loại tiện ích + nhập chỉ số (UtilityReadings)

7. **🧾 Quản Lý Hóa Đơn** - HOÀN THÀNH ✅
   - CRUD hóa đơn + xem công nợ
   - Tạo hóa đơn tháng (GenerateMonthlyInvoices)

8. **🔧 Quản Lý Bảo Trì** - HOÀN THÀNH ✅
   - CRUD ticket bảo trì/sự cố + trạng thái/ưu tiên/phân công

9. **🏠 Quản Lý Tài Sản** - HOÀN THÀNH ✅
    - CRUD tài sản theo phòng + số lượng/tình trạng/giá trị

10. **📊 Báo Cáo & Thống Kê** - HOÀN THÀNH ✅
    - Màn xem dữ liệu (DataViewer) cho hóa đơn/thống kê nhanh

11. **📢 Quản Lý Thông Báo** - HOÀN THÀNH ✅
    - CRUD thông báo + lịch sử gửi

12. **⚙️ Cài Đặt Hệ Thống** - HOÀN THÀNH ✅
    - CRUD cấu hình hệ thống (SystemSettings)

## 🏗️ Kiến Trúc 3-Tier

```
GUI Layer (Windows Forms + Controls)
    ↓
BLL Layer (Business Logic)
    ↓
DAL Layer (Database Access)
    ↓
SQL Server Database
```

## 📂 Cấu Trúc Thư Mục

```
quan-ly-chuoi-nha-tro/
├── GUI/
│   ├── FrmLogin.cs            (Đăng nhập)
│   ├── FrmRegister.cs         (Đăng ký - hiện đang tắt trong chế độ Admin-only)
│   ├── FrmAdminDashboard.cs   (Bảng điều khiển Admin) ✅
│   ├── FrmBranch.cs / FrmBranchDetail.cs ✅
│   ├── FrmRoomManager.cs / FrmRoomEditor.cs ✅
│   ├── FrmTenantManager.cs / FrmTenantEditor.cs ✅
│   ├── FrmContractManager.cs / FrmContractEditor.cs ✅
│   ├── FrmDepositManager.cs / FrmDepositEditor.cs ✅
│   ├── FrmInvoiceManager.cs / FrmInvoiceEditor.cs ✅
│   ├── FrmPaymentManager.cs / FrmPaymentEditor.cs ✅
│   ├── FrmUtilityManager.cs / FrmUtilityTypeEditor.cs / FrmUtilityReadingEditor.cs ✅
│   ├── FrmMaintenanceManager.cs / FrmMaintenanceEditor.cs ✅
│   ├── FrmAssetManager.cs / FrmAssetEditor.cs ✅
│   ├── FrmNotificationManager.cs / FrmNotificationEditor.cs ✅
│   └── FrmSystemSettingsManager.cs / FrmSystemSettingEditor.cs ✅
├── BLL/
│   ├── UserBLL.cs            (Xử lý người dùng)
│   ├── BranchBLL.cs          (Xử lý chi nhánh) ✅
│   └── AdminDataBLL.cs       (CRUD các module Admin) ✅
├── DAL/
│   ├── DatabaseHelper.cs     (Kết nối database) ✅
│   ├── BranchDAL.cs          (Truy vấn chi nhánh) ✅
│   └── AdminDataDAL.cs       (Truy vấn các module Admin) ✅
└── Properties/
    └── AssemblyInfo.cs
```

## 🗄️ Database

**SQL Server**: SmarterASP.NET
- Host: `SQL9001.site4now.net`
- Database: `db_ac1f11_quanlynhatro`
- User: `db_ac1f11_quanlynhatro_admin`

Scripts:
- `Database/setup_database_final.sql` (schema)
- `sample_data.sql` (dữ liệu mẫu)

**Các bảng chính (20 bảng)**:
- Roles, Users
- Branches, BranchSections
- RoomTypes, RoomStatuses, Rooms
- Tenants, Dependents, TenantRoomHistory
- Deposits, Contracts
- UtilityTypes, UtilityReadings
- Invoices, Payments
- MaintenanceTickets, Assets
- Notifications, SystemSettings

## 🔐 Role-Based Access Control

```
RoleId = 1: Admin (Truy cập tất cả - app chỉ cho phép role này đăng nhập)
```

## 🚀 Cách Chạy

### 1. Tạo schema + dữ liệu mẫu

```sql
-- 1) Chạy Database/setup_database_final.sql
-- 2) Chạy sample_data.sql
-- Đã có sẵn: admin / 123456
```

### 2. Build Project

```powershell
dotnet build .\\quan-ly-chuoi-nha-tro.sln
```

### 3. Chạy Application

```powershell
cd .\\quan-ly-chuoi-nha-tro
dotnet run
```

### 4. Đăng Nhập Test

- **Username**: `admin`
- **Password**: `123456`

## 📝 Sample Data

**Dữ liệu mẫu bao gồm**:
- 3 Chi nhánh (Q.1, Q.3, Q.7)
- 5 Phòng (101-201, 301-302)
- 4 Khách thuê
- 3 Hợp đồng hoạt động
- 3 Chi nhánh với quản lý
- Hóa đơn, ký cược, bảo trì mẫu

Run file `sample_data.sql` để load dữ liệu test.

## 🔧 Cài Đặt

### Requirements
- .NET 4.7.2 Framework
- SQL Server
- Windows OS

### Packages
- Xem `quan-ly-chuoi-nha-tro/packages.config` và `quan-ly-chuoi-nha-tro/quan-ly-chuoi-nha-tro.csproj`

## 📊 Tính Năng Chi Tiết - Chi Nhánh ✅

### BranchBLL.cs (Business Logic)
```csharp
- GetAllBranchesAsync() - Lấy danh sách tất cả
- GetBranchByIdAsync(int id) - Lấy chi tiết
- AddBranchAsync(...) - Thêm mới (7 params)
- UpdateBranchAsync(...) - Cập nhật
- DeleteBranchAsync(int id) - Xóa mềm
- ToggleBranchStatusAsync(int id, bool status) - Bật/tắt
```

### FrmBranch.cs (GUI)
```csharp
- DataGridView CRUD Interface
- Tìm kiếm theo tên/mã
- Xóa mềm với xác nhận
- Refresh danh sách
```

### FrmBranchDetail.cs (Form Dialog)
```csharp
- Thêm chi nhánh mới
- Sửa thông tin chi nhánh
- Xóa chi nhánh
- Validation đầy đủ
```

## 🎨 UI/UX Features

- **Modern Theme**: Dark header, light content
- **Responsive Grid Layout**: 2 cột × 7 hàng (12 modules)
- **Emoji Icons**: Visual representation
- **Hover Effects**: Color change on interaction
- **Modal Dialogs**: Dialog cho Add/Edit
- **Data Validation**: Kiểm tra input
- **Status Indicators**: Hoạt động/Vô hiệu

## ⚠️ Lưu Ý

1. **Soft Delete**: Xóa chỉ đánh dấu (IsActive = 0)
2. **Async/Await**: Tất cả database operations không blocking
3. **SQL Parameters**: Chống SQL Injection
4. **Connection Pooling**: Tự động quản lý connection
5. **Error Handling**: Try-catch với meaningful messages

## 🚧 Next Steps

- (Admin-only) Hoàn thiện dần theo nhu cầu thực tế.

## 👨‍💻 Developer Notes

**Architecture Pattern**: 3-Tier (GUI/BLL/DAL)
**Design Pattern**: CRUD operations
**Database Pattern**: Soft Delete (logical delete)
**Async Pattern**: Task-based async/await
**Security**: Parameter binding, role-based access

## 📞 Support

Database connection string:
```
Data Source=SQL9001.site4now.net;
Initial Catalog=db_ac1f11_quanlynhatro;
User Id=db_ac1f11_quanlynhatro_admin;
Password=admin123
```

---

**Version**: 1.0.0 - Admin Dashboard Complete ✅
**Status**: In Development 🚀
