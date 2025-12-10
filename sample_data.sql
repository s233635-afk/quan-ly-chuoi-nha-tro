-- =====================================================
-- DATABASE SAMPLE DATA FOR ADMIN TESTING
-- =====================================================

-- Insert Test Admin User
INSERT INTO Users (UserName, Password, FullName, Email, Phone, RoleId, IsActive, CreatedDate)
VALUES (N'admin', N'123456', N'Admin', N'admin@example.com', N'0123456789', 1, 1, GETDATE())

-- Insert Test Staff Users
INSERT INTO Users (UserName, Password, FullName, Email, Phone, RoleId, IsActive, CreatedDate)
VALUES 
(N'nv001', N'123456', N'Nhân Viên 1', N'nv001@example.com', N'0123456781', 2, 1, GETDATE()),
(N'nv002', N'123456', N'Nhân Viên 2', N'nv002@example.com', N'0123456782', 2, 1, GETDATE());

-- Insert Sample Branches (Chi Nhánh)
INSERT INTO Branches (BranchCode, BranchName, Address, Phone, ManagerName, IsActive)
VALUES 
(N'CN001', N'Chi Nhánh Quận 1', N'123 Đường Nguyễn Huệ, Q.1, TP.HCM', N'0223456789', N'Nguyễn Văn A', 1),
(N'CN002', N'Chi Nhánh Quận 3', N'456 Đường Ba Tháng Hai, Q.3, TP.HCM', N'0233456789', N'Trần Thị B', 1),
(N'CN003', N'Chi Nhánh Quận 7', N'789 Đường Nguyễn Lương Bằng, Q.7, TP.HCM', N'0243456789', N'Phạm Văn C', 1);

-- Insert Sample Rooms (Phòng)
INSERT INTO Rooms (BranchId, RoomNumber, RoomType, Area, RentPrice, UtilitiesPrice, MaxPersons, Description, Status)
VALUES 
(1, N'101', 1, 20, 3000000, 500000, 2, N'Phòng đơn giản', 1),
(1, N'102', 1, 25, 3500000, 500000, 2, N'Phòng tiêu chuẩn', 1),
(1, N'201', 2, 35, 5000000, 700000, 3, N'Phòng hai phòng ngủ', 1),
(2, N'301', 1, 22, 3200000, 500000, 2, N'Phòng tiêu chuẩn', 1),
(2, N'302', 1, 28, 4000000, 600000, 2, N'Phòng rộng', 1);

-- Insert Sample Tenants (Khách Thuê)
INSERT INTO Tenants (BranchId, FullName, IDNumber, DateOfBirth, Gender, Phone, Email, Nationality, Address, Occupation)
VALUES 
(1, N'Trần Minh Đức', N'123456789012', N'1990-01-15', 1, N'0912345678', N'minh.duc@email.com', N'Việt Nam', N'TP.HCM', N'Kỹ sư'),
(1, N'Lê Thị Hoài', N'987654321098', N'1995-05-20', 0, N'0912345679', N'hoai.le@email.com', N'Việt Nam', N'Hà Nội', N'Nhân viên văn phòng'),
(2, N'Nguyễn Văn Huy', N'111222333444', N'1988-08-10', 1, N'0912345680', N'huy.nguyen@email.com', N'Việt Nam', N'TP.HCM', N'Bán hàng'),
(2, N'Phạm Thu Thảo', N'555666777888', N'1992-12-25', 0, N'0912345681', N'thao.pham@email.com', N'Việt Nam', N'Hải Phòng', N'Giáo viên');

-- Insert Sample Contracts (Hợp Đồng)
INSERT INTO Contracts (RoomId, TenantId, StartDate, EndDate, RentPrice, UtilitiesPrice, DepositAmount, Status)
VALUES 
(1, 1, N'2024-01-01', N'2025-01-01', 3000000, 500000, 6000000, 1),
(2, 2, N'2024-02-01', N'2025-02-01', 3500000, 500000, 7000000, 1),
(3, 3, N'2024-03-01', N'2025-03-01', 5000000, 700000, 10000000, 1);

-- Insert Sample Deposits (Ký Cược)
INSERT INTO Deposits (TenantId, Amount, DepositDate, Status)
VALUES 
(1, 6000000, N'2024-01-01', 1),
(2, 7000000, N'2024-02-01', 1),
(3, 10000000, N'2024-03-01', 1),
(4, 5000000, N'2024-04-01', 1);

-- Insert Sample Invoices (Hóa Đơn Thanh Toán)
INSERT INTO Invoices (RoomId, TenantId, InvoiceDate, Month, Year, RentAmount, UtilitiesAmount, Status)
VALUES 
(1, 1, GETDATE(), 12, 2024, 3000000, 500000, 1),
(2, 2, GETDATE(), 12, 2024, 3500000, 500000, 1),
(3, 3, GETDATE(), 12, 2024, 5000000, 700000, 0);

-- Insert Sample Maintenance Records (Bảo Trì)
INSERT INTO MaintenanceRecords (RoomId, IssueType, Description, ReportDate, Status, Resolution)
VALUES 
(1, N'Điện', N'Công tắc điện bị hỏng', GETDATE(), 2, N'Đã thay công tắc'),
(2, N'Nước', N'Vòi nước bị chảy', N'2024-12-15', 1, NULL),
(3, N'Khác', N'Cửa sổ bị kính nứt', N'2024-12-10', 2, N'Đã thay kính mới');

-- Insert Sample Assets (Tài Sản)
INSERT INTO Assets (RoomId, AssetType, AssetName, Quantity, PurchaseDate, Condition, Location)
VALUES 
(1, N'Giường', N'Giường đôi 1.4x2.0m', 1, N'2023-01-01', N'Tốt', N'Phòng 101'),
(1, N'Tủ', N'Tủ áo gỗ', 1, N'2023-01-01', N'Tốt', N'Phòng 101'),
(1, N'Bàn', N'Bàn làm việc', 1, N'2023-06-15', N'Bình thường', N'Phòng 101'),
(2, N'Giường', N'Giường đôi 1.4x2.0m', 1, N'2023-02-01', N'Tốt', N'Phòng 102'),
(2, N'Quạt', N'Quạt cây', 1, N'2023-02-01', N'Bình thường', N'Phòng 102');

-- Insert Sample Utilities (Tiện Ích)
INSERT INTO Utilities (RoomId, UtilityType, UsageAmount, UsageDate, UnitPrice, TotalPrice)
VALUES 
(1, N'Điện', 100, N'2024-12-31', 3500, 350000),
(1, N'Nước', 15, N'2024-12-31', 10000, 150000),
(2, N'Điện', 120, N'2024-12-31', 3500, 420000),
(2, N'Nước', 18, N'2024-12-31', 10000, 180000),
(3, N'Điện', 150, N'2024-12-31', 3500, 525000),
(3, N'Nước', 25, N'2024-12-31', 10000, 250000);

-- Insert Sample Notifications (Thông Báo)
INSERT INTO Notifications (UserId, Title, Message, Status, CreatedDate)
VALUES 
(1, N'Thanh toán quá hạn', N'Khách thuê phòng 101 chưa thanh toán tiền phòng tháng 12', 1, GETDATE()),
(1, N'Bảo trì khẩn', N'Phòng 102 cần bảo trì nước nóng', 1, GETDATE());

-- Insert Sample System Settings (Cài Đặt Hệ Thống)
INSERT INTO SystemSettings (SettingKey, SettingValue, Description)
VALUES 
(N'AppName', N'Quản Lý Chuỗi Nhà Trọ', N'Tên ứng dụng'),
(N'CompanyName', N'Công Ty TNHH ABC', N'Tên công ty'),
(N'CompanyPhone', N'0234567890', N'Số điện thoại công ty'),
(N'CompanyEmail', N'info@abc.com', N'Email công ty'),
(N'MaxRoomPerBranch', N'100', N'Số phòng tối đa mỗi chi nhánh'),
(N'InvoiceDueDay', N'5', N'Ngày hạn thanh toán hóa đơn'),
(N'DepositPercentage', N'200', N'Phần trăm tiền ký cược so với tiền thuê');
