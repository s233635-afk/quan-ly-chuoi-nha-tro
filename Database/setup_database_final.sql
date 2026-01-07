-- =====================================================
-- DATABASE: db_ac1f11_quanlynhatro
-- HỆ THỐNG QUẢN LÝ CHUỖI NHÀ TRỌ (18 BẢNG)
-- =====================================================

-- =====================================================
-- 1. BẢNG NGƯỜI DÙNG & PHÂN QUYỀN (2 bảng)
-- =====================================================

-- Bảng Roles (Vai trò)
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng Users (Tài khoản người dùng)
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100),
    FullName NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20),
    RoleId INT NOT NULL,
    BranchId INT,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- =====================================================
-- 2. BẢNG CHI NHÁNH (2 bảng)
-- =====================================================

-- Bảng Branches (Chi nhánh)
CREATE TABLE Branches (
    BranchId INT PRIMARY KEY IDENTITY(1,1),
    BranchCode NVARCHAR(20) NOT NULL UNIQUE,
    BranchName NVARCHAR(255) NOT NULL,
    Address NVARCHAR(500),
    Phone NVARCHAR(20),
    Hotline NVARCHAR(20),
    OperatingHours NVARCHAR(100),
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng BranchSections (Dãy/Khu trong chi nhánh)
CREATE TABLE BranchSections (
    SectionId INT PRIMARY KEY IDENTITY(1,1),
    BranchId INT NOT NULL,
    SectionCode NVARCHAR(20) NOT NULL,
    SectionName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (BranchId) REFERENCES Branches(BranchId)
);

-- =====================================================
-- 3. BẢNG PHÒNG (3 bảng)
-- =====================================================

-- Bảng RoomTypes (Loại phòng)
CREATE TABLE RoomTypes (
    RoomTypeId INT PRIMARY KEY IDENTITY(1,1),
    RoomTypeName NVARCHAR(100) NOT NULL,
    DefaultPrice DECIMAL(15,2),
    Amenities NVARCHAR(500),
    MaxCapacity INT,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng RoomStatuses (Trạng thái phòng)
CREATE TABLE RoomStatuses (
    StatusId INT PRIMARY KEY IDENTITY(1,1),
    StatusName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255)
);

-- Bảng Rooms (Phòng)
CREATE TABLE Rooms (
    RoomId INT PRIMARY KEY IDENTITY(1,1),
    BranchId INT NOT NULL,
    SectionId INT NOT NULL,
    RoomNumber NVARCHAR(20) NOT NULL,
    RoomTypeId INT NOT NULL,
    RoomPrice DECIMAL(15,2),
    CurrentStatusId INT DEFAULT 1,
    Floor INT,
    Area DECIMAL(10,2),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    FOREIGN KEY (SectionId) REFERENCES BranchSections(SectionId),
    FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(RoomTypeId),
    FOREIGN KEY (CurrentStatusId) REFERENCES RoomStatuses(StatusId)
);

-- =====================================================
-- 4. BẢNG KHÁCH THUÊ (2 bảng)
-- =====================================================

-- Bảng Tenants (Khách thuê)
CREATE TABLE Tenants (
    TenantId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(255) NOT NULL,
    IdentityCard NVARCHAR(20),
    PhoneNumber NVARCHAR(20),
    Email NVARCHAR(100),
    BirthDate DATE,
    Address NVARCHAR(500),
    FrontIdPhoto NVARCHAR(500),
    BackIdPhoto NVARCHAR(500),
    TemporaryRegistration NVARCHAR(100),
    TemporaryRegistrationDate DATE,
    TemporaryRegistrationExpiry DATE,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng Dependents (Người ở chung)
CREATE TABLE Dependents (
    DependentId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    FullName NVARCHAR(255) NOT NULL,
    Relationship NVARCHAR(50),
    PhoneNumber NVARCHAR(20),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId)
);

-- =====================================================
-- 5. BẢNG HỢP ĐỒNG & LỊCH SỬ (3 bảng)
-- =====================================================

-- Bảng TenantRoomHistory (Lịch sử phòng của khách thuê)
CREATE TABLE TenantRoomHistory (
    HistoryId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    RoomId INT NOT NULL,
    CheckInDate DATE NOT NULL,
    CheckOutDate DATE,
    Status NVARCHAR(50), -- 'Active', 'Completed', 'Cancelled'
    Notes NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId)
);

-- Bảng Deposits (Tiền cọc & đặt phòng)
CREATE TABLE Deposits (
    DepositId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    RoomId INT NOT NULL,
    DepositAmount DECIMAL(15,2),
    DepositDate DATE,
    DepositType NVARCHAR(50), -- 'Booking' or 'Official'
    Status NVARCHAR(50), -- 'Pending', 'Confirmed', 'Returned', 'Cancelled'
    ReturnedAmount DECIMAL(15,2),
    ReturnedDate DATE,
    Notes NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId)
);

-- Bảng Contracts (Hợp đồng thuê)
CREATE TABLE Contracts (
    ContractId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    RoomId INT NOT NULL,
    ContractNumber NVARCHAR(50) NOT NULL UNIQUE,
    SignDate DATE,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    RentalPrice DECIMAL(15,2),
    DepositRequired DECIMAL(15,2),
    Terms NVARCHAR(MAX),
    ContractPdfPath NVARCHAR(500),
    Status NVARCHAR(50), -- 'Active', 'Extended', 'Terminated', 'Expired'
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId)
);

-- =====================================================
-- 6. BẢNG TIỆN ÍCH (2 bảng)
-- =====================================================

-- Bảng UtilityTypes (Loại dịch vụ: Điện, Nước, etc)
CREATE TABLE UtilityTypes (
    UtilityTypeId INT PRIMARY KEY IDENTITY(1,1),
    UtilityName NVARCHAR(100) NOT NULL,
    UtilityCode NVARCHAR(20),
    Unit NVARCHAR(50),
    IsRecurring BIT DEFAULT 1,
    DefaultPrice DECIMAL(15,2),
    Description NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng UtilityReadings (Chỉ số điện nước)
CREATE TABLE UtilityReadings (
    ReadingId INT PRIMARY KEY IDENTITY(1,1),
    RoomId INT NOT NULL,
    UtilityTypeId INT NOT NULL,
    ReadingDate DATE,
    PreviousReading DECIMAL(15,2),
    CurrentReading DECIMAL(15,2),
    UsageAmount DECIMAL(15,2),
    UnitPrice DECIMAL(15,2),
    TotalCost DECIMAL(15,2),
    Notes NVARCHAR(255),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId),
    FOREIGN KEY (UtilityTypeId) REFERENCES UtilityTypes(UtilityTypeId)
);

-- =====================================================
-- 7. BẢNG HÓA ĐƠN & THANH TOÁN (2 bảng)
-- =====================================================

-- Bảng Invoices (Hóa đơn)
CREATE TABLE Invoices (
    InvoiceId INT PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
    TenantId INT NOT NULL,
    RoomId INT NOT NULL,
    InvoiceDate DATE NOT NULL,
    FromDate DATE,
    ToDate DATE,
    RentalCost DECIMAL(15,2),
    UtilityCost DECIMAL(15,2),
    OtherCost DECIMAL(15,2),
    TaxRate DECIMAL(6,2) DEFAULT 0,
    TaxAmount DECIMAL(15,2) DEFAULT 0,
    TotalAmount DECIMAL(15,2),
    PaidAmount DECIMAL(15,2) DEFAULT 0,
    RemainingAmount DECIMAL(15,2),
    Status NVARCHAR(50), -- 'Draft', 'Issued', 'Paid', 'PartialPaid', 'Overdue'
    DueDate DATE,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId)
);

-- Bảng Payments (Ghi nhận thanh toán)
CREATE TABLE Payments (
    PaymentId INT PRIMARY KEY IDENTITY(1,1),
    InvoiceId INT NOT NULL,
    PaymentDate DATE NOT NULL,
    PaymentAmount DECIMAL(15,2) NOT NULL,
    PaymentMethod NVARCHAR(50), -- 'Cash', 'Transfer', 'QR', 'Check'
    TransactionReference NVARCHAR(100),
    Notes NVARCHAR(255),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId)
);

-- =====================================================
-- 8. BẢNG BẢO TRÌ & TÀI SẢN (2 bảng)
-- =====================================================

-- Bảng MaintenanceTickets (Ticket sửa chữa)
CREATE TABLE MaintenanceTickets (
    TicketId INT PRIMARY KEY IDENTITY(1,1),
    TicketNumber NVARCHAR(50) NOT NULL UNIQUE,
    RoomId INT NOT NULL,
    RequestorType NVARCHAR(50), -- 'Tenant', 'Staff', 'System'
    RequestorId INT,
    IssueDescription NVARCHAR(500),
    Priority NVARCHAR(50), -- 'Low', 'Medium', 'High', 'Urgent'
    AssignedToUserId INT,
    Status NVARCHAR(50), -- 'Created', 'InProgress', 'Completed', 'Cancelled'
    CreatedDate DATETIME DEFAULT GETDATE(),
    CompletedDate DATETIME,
    Notes NVARCHAR(500),
    FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId),
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(UserId)
);

-- Bảng Assets (Tài sản/vật tư)
CREATE TABLE Assets (
    AssetId INT PRIMARY KEY IDENTITY(1,1),
    AssetCode NVARCHAR(50) NOT NULL UNIQUE,
    AssetName NVARCHAR(255) NOT NULL,
    Category NVARCHAR(100),
    RoomId INT,
    Quantity INT DEFAULT 1,
    Condition NVARCHAR(50), -- 'Good', 'Fair', 'Poor', 'Damaged'
    PurchaseDate DATE,
    PurchasePrice DECIMAL(15,2),
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId)
);

-- =====================================================
-- 9. BẢNG THÔNG BÁO & CẤU HÌNH (2 bảng)
-- =====================================================

CREATE TABLE Notifications (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT,
    Title NVARCHAR(255) NOT NULL,
    Message NVARCHAR(MAX),
    Status NVARCHAR(50), -- 'Unread', 'Read', 'Sent'
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE SystemSettings (
    SettingKey NVARCHAR(100) PRIMARY KEY,
    SettingValue NVARCHAR(500),
    Description NVARCHAR(500)
);

CREATE TABLE UserBankSettings (
    UserId INT PRIMARY KEY,
    BankId NVARCHAR(100) NOT NULL,
    BankAccountNumber NVARCHAR(50) NOT NULL,
    BankAccountName NVARCHAR(255) NOT NULL,
    BankTemplate NVARCHAR(50),
    UpdatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);


-- =====================================================
-- INSERT DỮ LIỆU MẪU
-- =====================================================

-- Insert Roles
INSERT INTO Roles (RoleName, Description) VALUES
(N'Admin', N'Quản trị viên toàn quyền'),
(N'Staff', N'Nhân viên chi nhánh'),
(N'Manager', N'Quản lý chi nhánh');

-- Insert RoomStatuses
INSERT INTO RoomStatuses (StatusName, Description) VALUES
(N'Trống', N'Phòng trống'),
(N'Đang ở', N'Phòng đang cho thuê'),
(N'Đã thuê', N'Phòng đã thuê'),
(N'Đã cọc', N'Phòng đã cọc'),
(N'Bảo trì', N'Phòng đang bảo trì'),
(N'Vệ sinh', N'Phòng đang dọn vệ sinh');

-- Insert Branches
INSERT INTO Branches (BranchCode, BranchName, Address, Phone, Hotline, OperatingHours) VALUES
(N'CT01', N'Chi nhánh Cần Thơ 1', N'123 Ninh Kiều, Cần Thơ', N'0243123456', N'1900123456', N'08:00-17:00'),
(N'CT02', N'Chi nhánh Cần Thơ 2', N'456 Ninh Kiều, Cần Thơ', N'0243654321', N'1900654321', N'08:00-17:00');

-- Insert BranchSections
INSERT INTO BranchSections (BranchId, SectionCode, SectionName) VALUES
(1, N'A', N'Dãy A'),
(1, N'B', N'Dãy B'),
(2, N'C', N'Dãy C');

-- 3 loại: đơn / đôi / cao cấp (sắp xếp lại để đúng 20 phòng cho nhân viên)
INSERT INTO RoomTypes (RoomTypeName, DefaultPrice, Amenities, MaxCapacity, Description) VALUES
(N'Phòng đơn', 3000000, N'Giường đơn, tủ, quạt/AC', 1, N'Phòng cho 1-2 người'),
(N'Phòng đôi', 3500000, N'2 giường, tủ, điều hòa, nước nóng', 2, N'Phòng cho 2-3 người'),
(N'Phòng cao cấp', 5000000, N'Giường lớn, điều hòa, TV, máy nước nóng', 3, N'Phòng cao cấp đầy đủ tiện nghi');

-- Insert Rooms
INSERT INTO Rooms (BranchId, SectionId, RoomNumber, RoomTypeId, RoomPrice, CurrentStatusId) VALUES
-- Dãy A: 4 phòng đơn, 3 phòng đôi, 3 phòng cao cấp
(1, 1, N'A01', 1, 3000000, 2),
(1, 1, N'A02', 1, 3000000, 2),
(1, 1, N'A03', 1, 3000000, 2),
(1, 1, N'A04', 1, 3000000, 2),
(1, 1, N'A05', 2, 3500000, 3),
(1, 1, N'A06', 2, 3500000, 2),
(1, 1, N'A07', 2, 3500000, 1),
(1, 1, N'A08', 3, 5000000, 2),
(1, 1, N'A09', 3, 5000000, 1),
(1, 1, N'A10', 3, 5000000, 1),
-- Dãy B: 4 phòng đơn, 3 phòng đôi, 3 phòng cao cấp
(1, 2, N'B01', 1, 3000000, 2),
(1, 2, N'B02', 1, 3000000, 2),
(1, 2, N'B03', 1, 3000000, 1),
(1, 2, N'B04', 1, 3000000, 1),
(1, 2, N'B05', 2, 3500000, 2),
(1, 2, N'B06', 2, 3500000, 3),
(1, 2, N'B07', 2, 3500000, 1),
(1, 2, N'B08', 3, 5000000, 2),
(1, 2, N'B09', 3, 5000000, 1),
(1, 2, N'B10', 3, 5000000, 1);

-- Insert UtilityTypes
INSERT INTO UtilityTypes (UtilityName, UtilityCode, Unit, DefaultPrice) VALUES
(N'Điện', N'ELEC', N'kWh', 3500),
(N'Nước', N'WATER', N'm3', 25000),
(N'Internet', N'INET', N'lần/tháng', 200000),
(N'Rác thải', N'TRASH', N'lần/tháng', 50000);

-- Insert Users
INSERT INTO Users (Username, Password, Email, FullName, Phone, RoleId, BranchId, IsActive) VALUES
(N'admin', N'admin123', N'admin@quanlynhatro.com', N'Quản trị viên', NULL, 1, NULL, 1),
(N'nhanvien1', N'pass123', N'nv1@quanlynhatro.com', N'Nguyễn Văn A', NULL, 2, 1, 1),
(N'nhanvien2', N'pass123', N'nv2@quanlynhatro.com', N'Trần Thị B', NULL, 2, 2, 1);

-- Insert Tenants (khách thuê mẫu)
INSERT INTO Tenants (FullName, IdentityCard, PhoneNumber, Email, BirthDate, Address, TemporaryRegistration, TemporaryRegistrationDate, TemporaryRegistrationExpiry)
VALUES
(N'Phạm Minh Tuấn', N'032456789', N'0901234567', N'tuan@example.com', '1995-01-12', N'12 Nguyễn Huệ, Q1', N'Tạm trú Q1', '2024-01-01', '2025-01-01'),
(N'Lê Thị Hoa', N'074589632', N'0938123456', N'hoa@example.com', '1993-05-20', N'45 Lê Thường Kiệt, Q3', N'Tạm trú Q3', '2024-02-01', '2025-02-01'),
(N'Nguyễn Văn Long', N'021345678', N'0912987654', N'long@example.com', '1990-11-02', N'89 Trần Hưng Đạo, Q5', N'Tạm trú Q5', '2024-03-01', '2025-03-01'),
(N'Trần Thu Uyên', N'058963214', N'0945123789', N'uyen@example.com', '1996-07-15', N'15 Võ Thị Sáu, Q1', N'Tạm trú Q1', '2024-04-01', '2025-04-01');

-- Insert Tenant room history
INSERT INTO TenantRoomHistory (TenantId, RoomId, CheckInDate, CheckOutDate, Status, Notes) VALUES
(1, 1, '2024-04-01', NULL, N'Active', N'Đang ở phòng A01'),
(2, 2, '2024-04-15', NULL, N'Active', N'Đang ở phòng A02'),
(3, 3, '2024-05-01', NULL, N'Active', N'Đang ở phòng B01'),
(4, 4, '2024-06-01', NULL, N'Active', N'Đang ở phòng C01');

-- Insert Dependents (người ở chung)
INSERT INTO Dependents (TenantId, FullName, Relationship, PhoneNumber) VALUES
(1, N'Nguyễn Thị Mai', N'Vợ', N'0902223344'),
(1, N'Nguyễn Minh Khang', N'Con', N'0903334455'),
(2, N'Phạm Văn Bình', N'Anh trai', N'0934332211'),
(3, N'Lê Hoàng Nam', N'Bạn ở ghép', N'0912888999'),
(4, N'Trần Anh Thư', N'Chị gái', N'0944332211');

-- Insert Deposits
INSERT INTO Deposits (TenantId, RoomId, DepositAmount, DepositDate, DepositType, Status, ReturnedAmount, ReturnedDate, Notes) VALUES
(1, 1, 5000000, '2024-03-20', N'Booking', N'Confirmed', 0, NULL, N'Cọc giữ phòng'),
(2, 2, 6000000, '2024-03-25', N'Official', N'Confirmed', 0, NULL, N'Cọc hợp đồng'),
(3, 3, 4000000, '2024-04-10', N'Booking', N'Pending', 0, NULL, N'Cọc giữ phòng'),
(4, 4, 4500000, '2024-05-20', N'Official', N'Returned', 4500000, '2024-06-05', N'Hoàn cọc do hủy phòng');

-- Insert Contracts
INSERT INTO Contracts (TenantId, RoomId, ContractNumber, SignDate, StartDate, EndDate, RentalPrice, DepositRequired, Terms, ContractPdfPath, Status)
VALUES
(1, 1, N'HD-2024-001', '2024-03-25', '2024-04-01', '2025-03-31', 3000000, 5000000, N'Thanh toán đầu tháng, điện nước theo chỉ số.', NULL, N'Active'),
(2, 2, N'HD-2024-002', '2024-04-05', '2024-04-15', '2025-04-14', 5000000, 6000000, N'Thanh toán đầu tháng, cho phép nuôi thú nhỏ.', NULL, N'Active'),
(3, 3, N'HD-2024-003', '2024-05-05', '2024-05-10', '2025-05-09', 3000000, 4000000, N'Thanh toán ngày 10 hằng tháng.', NULL, N'Active'),
(4, 4, N'HD-2024-004', '2024-06-10', '2024-06-15', '2025-06-14', 5000000, 4500000, N'Thanh toán ngày 5 hằng tháng, rút cọc 1 tháng.', NULL, N'Active');

-- Insert UtilityReadings
INSERT INTO UtilityReadings (RoomId, UtilityTypeId, ReadingDate, PreviousReading, CurrentReading, UsageAmount, UnitPrice, TotalCost, Notes)
VALUES
(1, 1, '2024-06-01', 1000, 1100, 100, 3500, 350000, NULL),
(1, 2, '2024-06-01', 200, 230, 30, 25000, 750000, NULL),
(2, 1, '2024-06-01', 900, 980, 80, 3500, 280000, NULL),
(2, 2, '2024-06-01', 150, 180, 30, 25000, 750000, NULL),
(3, 1, '2024-06-01', 500, 560, 60, 3500, 210000, NULL),
(3, 2, '2024-06-01', 120, 140, 20, 25000, 500000, NULL),
(4, 1, '2024-06-01', 300, 360, 60, 3500, 210000, NULL),
(4, 2, '2024-06-01', 80, 98, 18, 25000, 450000, NULL),
(1, 1, '2024-07-01', 1100, 1180, 80, 3500, 280000, NULL),
(1, 2, '2024-07-01', 230, 260, 30, 25000, 750000, NULL);

-- Insert Invoices
INSERT INTO Invoices (InvoiceNumber, TenantId, RoomId, InvoiceDate, FromDate, ToDate, RentalCost, UtilityCost, OtherCost, TotalAmount, PaidAmount, RemainingAmount, Status, DueDate)
VALUES
(N'INV-2024-06-01', 1, 1, '2024-06-02', '2024-06-01', '2024-06-30', 3000000, 1100000, 0, 4100000, 2000000, 2100000, N'PartialPaid', '2024-06-10'),
(N'INV-2024-06-02', 2, 2, '2024-06-02', '2024-06-01', '2024-06-30', 5000000, 1030000, 0, 6030000, 6030000, 0, N'Paid', '2024-06-10'),
(N'INV-2024-06-03', 3, 3, '2024-06-02', '2024-06-01', '2024-06-30', 3000000, 710000, 0, 3710000, 0, 3710000, N'Issued', '2024-06-10'),
(N'INV-2024-06-04', 4, 4, '2024-06-12', '2024-06-01', '2024-06-30', 5000000, 660000, 0, 5660000, 0, 5660000, N'Issued', '2024-06-15'),
(N'INV-2024-07-01', 1, 1, '2024-07-02', '2024-07-01', '2024-07-31', 3000000, 1030000, 0, 4030000, 0, 4030000, N'Draft', '2024-07-10');

-- Insert Payments
INSERT INTO Payments (InvoiceId, PaymentDate, PaymentAmount, PaymentMethod, TransactionReference, Notes) VALUES
(1, '2024-06-03', 2000000, N'Transfer', N'TXN12345', N'Thanh toán đợt 1'),
(2, '2024-06-04', 6030000, N'Cash', N'', N'Thanh toán đủ'),
(3, '2024-06-15', 1500000, N'QR', N'TXN88888', N'Thanh toán 1 phần'),
(4, '2024-06-16', 2000000, N'Transfer', N'TXN99999', N'Thanh toán trước hạn');

-- Insert Maintenance tickets
INSERT INTO MaintenanceTickets (TicketNumber, RoomId, RequestorType, RequestorId, IssueDescription, Priority, AssignedToUserId, Status, CreatedDate, Notes) VALUES
(N'MT-2024-001', 1, N'Tenant', 1, N'Hỏng vòi nước phòng tắm', N'Medium', 2, N'InProgress', GETDATE(), N'Đã giao cho NV A'),
(N'MT-2024-002', 2, N'Tenant', 2, N'Máy lạnh không lạnh', N'High', 2, N'Created', GETDATE(), N'Chờ kiểm tra'),
(N'MT-2024-003', 3, N'Tenant', 3, N'Rò rỉ đường điện', N'Urgent', 2, N'InProgress', GETDATE(), N'Ưu tiên xử lý'),
(N'MT-2024-004', 4, N'Tenant', 4, N'Cửa phòng hỏng bản lề', N'Low', 3, N'Completed', GETDATE(), N'Đã thay bản lề');

-- Insert Assets
INSERT INTO Assets (AssetCode, AssetName, Category, RoomId, Quantity, Condition, PurchaseDate, PurchasePrice, Description, IsActive)
VALUES
(N'TS-001', N'Máy lạnh Panasonic', N'Thiết bị', 1, 1, N'Good', '2023-01-10', 8000000, N'Hàng cần bảo hành', 1),
(N'TS-002', N'Bàn gỗ', N'Nội thất', 1, 1, N'Good', '2023-03-12', 1200000, N'Bàn làm việc', 1),
(N'TS-003', N'Ghế gỗ', N'Nội thất', 2, 2, N'Fair', '2023-03-12', 600000, N'Ghế phòng ngủ', 1),
(N'TS-004', N'Giường gỗ', N'Nội thất', 3, 1, N'Good', '2023-04-10', 2500000, N'Giường đơn', 1),
(N'TS-005', N'Máy nước nóng', N'Thiết bị', 4, 1, N'Good', '2023-05-15', 3500000, N'Bảo trì 1 lần/năm', 1);

-- Insert Notifications
INSERT INTO Notifications (UserId, Title, Message, Status, CreatedDate) VALUES
(1, N'Nhắc thanh toán', N'Khách 1 cần nộp 2.1 triệu tháng 6', N'Unread', GETDATE()),
(2, N'Lịch bảo trì', N'Kiểm tra máy lạnh phòng A02 ngày 15/06', N'Read', GETDATE()),
(3, N'Nhập chỉ số điện', N'Nhắc nhập chỉ số điện nước trước ngày 05/07', N'Unread', GETDATE());

-- Insert SystemSettings
INSERT INTO SystemSettings (SettingKey, SettingValue, Description) VALUES
(N'DefaultDepositRate', N'1_month', N'Cọc mức chuẩn 1 tháng tiền phòng'),
(N'InvoiceDueDay', N'10', N'Ngày đáo hạn hóa đơn hằng tháng'),
(N'DefaultTaxRatePercent', N'0', N'Mức thuế (%) áp dụng theo doanh thu hóa đơn'),
(N'DefaultContractTemplate', N'HD_CHUOI_NHA_TRO_V1', N'Mẫu hợp đồng chuẩn'),
(N'AutoReminderEnabled', N'true', N'Bật nhắc nhở tự động'),
(N'DefaultUtilityPrice_ELEC', N'3500', N'Giá điện mức chuẩn');

-- =====================================================
-- TẠO CÁC CHỈ MỤC
-- =====================================================

CREATE INDEX idx_Users_Username ON Users(Username);
CREATE INDEX idx_Users_RoleId ON Users(RoleId);
CREATE INDEX idx_Users_BranchId ON Users(BranchId);
CREATE INDEX idx_Rooms_BranchId ON Rooms(BranchId);
CREATE INDEX idx_Rooms_StatusId ON Rooms(CurrentStatusId);
CREATE INDEX idx_Tenants_Phone ON Tenants(PhoneNumber);
CREATE INDEX idx_Tenants_IdentityCard ON Tenants(IdentityCard);
CREATE INDEX idx_Contracts_TenantId ON Contracts(TenantId);
CREATE INDEX idx_Contracts_RoomId ON Contracts(RoomId);
CREATE INDEX idx_Contracts_Status ON Contracts(Status);
CREATE INDEX idx_Invoices_TenantId ON Invoices(TenantId);
CREATE INDEX idx_Invoices_Status ON Invoices(Status);
CREATE INDEX idx_Payments_InvoiceId ON Payments(InvoiceId);
CREATE INDEX idx_MaintenanceTickets_RoomId ON MaintenanceTickets(RoomId);
CREATE INDEX idx_MaintenanceTickets_Status ON MaintenanceTickets(Status);
CREATE INDEX idx_UtilityReadings_RoomId ON UtilityReadings(RoomId);
CREATE INDEX idx_Assets_RoomId ON Assets(RoomId);

-- =====================================================
-- XONG - DATABASE INITIALIZATION (18 BẢNG)
-- =====================================================
