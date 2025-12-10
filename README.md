# Quản Lý Chuỗi Nhà Trọ - Admin Dashboard

Ứng dụng WinForms quản lý chuỗi nhà trọ toàn diện với 13 tính năng Admin + 8 tính năng Nhân Viên

## 🎯 Tính Năng Admin (13 mô-đun)

1. **🏢 Quản Lý Chi Nhánh** - HOÀN THÀNH ✅
   - Thêm, sửa, xóa chi nhánh
   - Quản lý thông tin chi nhánh (địa chỉ, điện thoại, quản lý)
   - Trạng thái hoạt động

2. **🚪 Quản Lý Phòng** - Coming Soon...
   - Thêm/sửa/xóa phòng
   - Loại phòng (Studio, 1 phòng ngủ, 2 phòng ngủ...)
   - Diện tích, giá thuê, tiện ích

3. **👥 Quản Lý Nhân Viên** - Coming Soon...
   - Tuyển dụng nhân viên
   - Phân công công việc
   - Xem lịch sử giao dịch

4. **🧑 Quản Lý Khách Thuê** - Coming Soon...
   - Chi tiết khách (CMND, ngày sinh, liên lạc)
   - Lịch sử thuê, từng phòng

5. **📜 Quản Lý Hợp Đồng** - Coming Soon...
   - Ký hợp đồng mới
   - Xem hợp đồng hiện tại
   - Xuất PDF

6. **💰 Quản Lý Ký Cược** - Coming Soon...
   - Tiền ký cược khách
   - Lịch sử hoàn cọc

7. **💡 Quản Lý Tiện Ích** - Coming Soon...
   - Điện, nước, gas
   - Giá tiện ích
   - Tính toán tự động

8. **🧾 Quản Lý Hóa Đơn** - Coming Soon...
   - Phát hành hóa đơn
   - Theo dõi thanh toán
   - Báo cáo nợ

9. **🔧 Quản Lý Bảo Trì** - Coming Soon...
   - Báo cáo hỏng hóc
   - Lịch bảo trì
   - Theo dõi tình trạng

10. **🏠 Quản Lý Tài Sản** - Coming Soon...
    - Danh sách tài sản
    - Tình trạng tài sản
    - Giá trị tài sản

11. **📊 Báo Cáo & Thống Kê** - Coming Soon...
    - Doanh thu
    - Chiếm dụng phòng
    - Chi phí

12. **📢 Quản Lý Thông Báo** - Coming Soon...
    - Gửi thông báo khách
    - Gửi thông báo nhân viên
    - Lịch sử

13. **⚙️ Cài Đặt Hệ Thống** - Coming Soon...
    - Cấu hình hệ thống
    - Quản lý tài khoản
    - Phân quyền

## 📋 Tính Năng Nhân Viên (8 mô-đun)

1. **Xem Danh Sách Phòng** - Xem phòng được giao
2. **Quản Lý Khách Thuê** - Xem/cập nhật thông tin khách
3. **Lịch Sử Thanh Toán** - Xem ghi nhận thanh toán
4. **In Hóa Đơn** - In hóa đơn cho khách
5. **Báo Cáo Hỏng Hóc** - Ghi nhận sự cố
6. **Theo Dõi Bảo Trì** - Xem tiến độ bảo trì
7. **Xuất PDF Hợp Đồng** - In bản sao hợp đồng
8. **Thông Báo** - Nhận thông báo từ hệ thống

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
│   ├── FrmRegister.cs         (Đăng ký)
│   ├── FrmAdminDashboard.cs   (Bảng điều khiển Admin) ✅
│   ├── FrmBranch.cs           (Quản lý chi nhánh) ✅
│   └── FrmBranchDetail.cs     (Chi tiết chi nhánh) ✅
├── BLL/
│   ├── UserBLL.cs            (Xử lý người dùng)
│   └── BranchBLL.cs          (Xử lý chi nhánh) ✅
├── DAL/
│   ├── DatabaseHelper.cs     (Kết nối database) ✅
│   └── BranchDAL.cs          (Truy vấn chi nhánh) ✅
└── Properties/
    └── AssemblyInfo.cs
```

## 🗄️ Database

**SQL Server**: SmarterASP.NET
- Host: `SQL9001.site4now.net`
- Database: `db_ac1f11_quanlynhatro`
- User: `db_ac1f11_quanlynhatro_admin`

**18 Bảng**:
- Users, Roles, Branches, Rooms, Tenants
- Contracts, Deposits, Invoices, Payments
- MaintenanceRecords, Utilities, Assets
- Notifications, SystemSettings, AuditLogs
- RoomUtilities, TenantHistories, TransactionLogs

## 🔐 Role-Based Access Control

```
RoleId = 1: Admin (Truy cập tất cả)
RoleId = 2: Staff (Xem báo cáo, ghi nhận)
```

## 🚀 Cách Chạy

### 1. Khôi phục Database Sample Data

```sql
-- Thực thi file sample_data.sql trên SQL Server
-- Đã có sẵn: admin / 123456
```

### 2. Build Project

```powershell
cd quan-ly-chuoi-nha-tro
dotnet build
```

### 3. Chạy Application

```powershell
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
- MySqlConnector 2.5.0
- ServiceStack.OrmLite 10.0.2
- System.Data.SqlClient (built-in)

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
- **Responsive Grid Layout**: 2 cột × 7 hàng (13 modules)
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

- [ ] Room Management Module
- [ ] Staff Management Module
- [ ] Tenant Management Module
- [ ] Contract Management Module
- [ ] Invoice & Payment Module
- [ ] Maintenance Module
- [ ] Asset Management Module
- [ ] Reporting & Statistics
- [ ] Notification System
- [ ] System Settings

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
