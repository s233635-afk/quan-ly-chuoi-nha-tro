-- =====================================================
-- DATABASE: db_ac1f11_quanlynhatro
-- HỆ THỐNG QUẢN LÝ CHUỖI NHÀ TRỌ (18 Bảng)
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

-- Bảng Deposits (Tiền cọc & Đặt phòng)
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
(N'Đã cọc', N'Phòng đã cọc'),
(N'Bảo trì', N'Phòng đang bảo trì'),
(N'Vệ sinh', N'Phòng đang dọn vệ sinh');

-- Insert Branches
INSERT INTO Branches (BranchCode, BranchName, Address, Phone, Hotline, OperatingHours) VALUES
(N'HN01', N'Chi nhánh Cần Thơ 1', N'123 Ninh Kiều, Cần Thơ', N'0243123456', N'1900123456', N'08:00-17:00'),
(N'HN02', N'Chi nhánh Cần Thơ 2', N'456 Ninh Kiều, Cần Thơ', N'0243654321', N'1900654321', N'08:00-17:00');
-- Insert BranchSections
INSERT INTO BranchSections (BranchId, SectionCode, SectionName) VALUES
(1, N'A', N'Dãy A'),
(1, N'B', N'Dãy B'),
(2, N'C', N'Dãy C');

-- Insert RoomTypes
INSERT INTO RoomTypes (RoomTypeName, DefaultPrice, Amenities, MaxCapacity) VALUES
(N'Phòng đơn', 3000000, N'Giường, Tủ', 1),
(N'Phòng đôi', 5000000, N'Giường, Tủ, AC', 2),
(N'Phòng cao cấp', 8000000, N'Giường, Tủ, AC, TV', 2);

-- Insert Rooms
INSERT INTO Rooms (BranchId, SectionId, RoomNumber, RoomTypeId, RoomPrice, CurrentStatusId) VALUES
(1, 1, N'A01', 1, 3000000, 1),
(1, 1, N'A02', 2, 5000000, 1),
(1, 2, N'B01', 1, 3000000, 1),
(2, 3, N'C01', 2, 5000000, 1);

-- Insert UtilityTypes
INSERT INTO UtilityTypes (UtilityName, UtilityCode, Unit, DefaultPrice) VALUES
(N'Điện', N'ELEC', N'kWh', 3500),
(N'Nước', N'WATER', N'm3', 25000),
(N'Internet', N'INET', N'lần/tháng', 200000);

-- Insert Users
INSERT INTO Users (Username, Password, Email, FullName, RoleId, BranchId, IsActive) VALUES
(N'admin', N'admin123', N'admin@quanlynhatro.com', N'Quản trị viên', 1, NULL, 1),
(N'nhanvien1', N'pass123', N'nv1@quanlynhatro.com', N'Nguyễn Văn A', 2, 1, 1),
(N'nhanvien2', N'pass123', N'nv2@quanlynhatro.com', N'Trần Thị B', 2, 2, 1);

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
-- XONG - DATABASE INITIALIZATION (18 Bảng)
-- =====================================================
