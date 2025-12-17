-- =====================================================
-- SAMPLE DATA HOÀN CHỈNH CHO QUẢN LÝ NHÀ TRỌ
-- Dữ liệu test với diện tích (Area) đầy đủ
-- =====================================================

-- TRUNCATE DỮ LIỆU CŨ (Optional - Chỉ chạy nếu muốn xóa dữ liệu cũ)
-- DELETE FROM Payments;
-- DELETE FROM Invoices;
-- DELETE FROM Deposits;
-- DELETE FROM UtilityReadings;
-- DELETE FROM Contracts;
-- DELETE FROM TenantRoomHistory;
-- DELETE FROM MaintenanceTickets;
-- DELETE FROM Assets;
-- DELETE FROM Rooms;
-- DELETE FROM Tenants;
-- DELETE FROM Dependents;
-- DELETE FROM BranchSections;
-- DELETE FROM Branches;

-- =====================================================
-- 1. BRANCHES (Chi nhánh)
-- =====================================================
INSERT INTO Branches (BranchCode, BranchName, Address, Phone, Hotline, OperatingHours, IsActive)
VALUES
(N'CTH-01', N'Chi Nhánh Cần Thơ 1', N'123 Ninh Kiều, Cần Thơ', N'0292 3820 808', N'1900 0808', N'08:00-17:00', 1),
(N'CTH-02', N'Chi Nhánh Cần Thơ 2', N'456 Võ Văn Kiệt, Cần Thơ', N'0292 3821 888', N'1900 0888', N'08:00-17:00', 1),
(N'CTH-03', N'Chi Nhánh Hà Nội', N'789 Tôn Đức Thắng, Hà Nội', N'0243 9999 999', N'1900 9999', N'08:00-17:00', 1);

-- =====================================================
-- 2. BRANCH SECTIONS (Dãy/Khu)
-- =====================================================
INSERT INTO BranchSections (BranchId, SectionCode, SectionName, Description, IsActive)
VALUES
(1, N'A', N'Dãy A - Hạng I', N'Phòng cao cấp, full tiện ích', 1),
(1, N'B', N'Dãy B - Hạng II', N'Phòng bình thường, tiện ích cơ bản', 1),
(1, N'C', N'Dãy C - Hạng III', N'Phòng kinh tế, diện tích nhỏ', 1),
(2, N'D', N'Dãy D - Hạng I', N'Phòng cao cấp', 1),
(2, N'E', N'Dãy E - Hạng II', N'Phòng tiêu chuẩn', 1),
(3, N'F', N'Dãy F - Mới', N'Dãy mới xây dựng', 1);

-- =====================================================
-- 3. ROOM TYPES (Loại phòng)
-- =====================================================
INSERT INTO RoomTypes (RoomTypeName, DefaultPrice, Amenities, MaxCapacity, IsActive)
VALUES
(N'Phòng Đơn - 15m²', 2500000, N'Giường, Tủ, Bàn, Quạt', 1, 1),
(N'Phòng Đôi - 20m²', 3500000, N'Giường, Tủ, Bàn, Quạt', 2, 1),
(N'Phòng Cao Cấp - 25m²', 5000000, N'Giường, Tủ, Bàn, AC, TV', 2, 1),
(N'Phòng Studio - 30m²', 6500000, N'Giường, Bếp nhỏ, AC, Internet', 2, 1),
(N'Phòng VIP - 40m²', 9000000, N'Giường, Bếp đầy đủ, AC, TV, Internet, Ban Công', 3, 1);

-- =====================================================
-- 4. ROOMS (Phòng)
-- =====================================================
INSERT INTO Rooms (BranchId, SectionId, RoomNumber, RoomTypeId, RoomPrice, CurrentStatusId, Floor, Area, IsActive)
VALUES
-- Chi nhánh 1 - Dãy A (Hạng I)
(1, 1, N'A101', 3, 5000000, 2, 1, 25.0, 1),
(1, 1, N'A102', 3, 5000000, 2, 1, 25.0, 1),
(1, 1, N'A201', 4, 6500000, 2, 2, 30.0, 1),
(1, 1, N'A202', 4, 6500000, 1, 2, 30.0, 1),
(1, 1, N'A301', 5, 9000000, 1, 3, 40.0, 1),

-- Chi nhánh 1 - Dãy B (Hạng II)
(1, 2, N'B101', 2, 3500000, 2, 1, 20.0, 1),
(1, 2, N'B102', 2, 3500000, 1, 1, 20.0, 1),
(1, 2, N'B201', 2, 3500000, 2, 2, 20.0, 1),
(1, 2, N'B202', 3, 5000000, 1, 2, 25.0, 1),
(1, 2, N'B301', 3, 5000000, 2, 3, 25.0, 1),

-- Chi nhánh 1 - Dãy C (Hạng III)
(1, 3, N'C101', 1, 2500000, 2, 1, 15.0, 1),
(1, 3, N'C102', 1, 2500000, 2, 1, 15.0, 1),
(1, 3, N'C103', 1, 2500000, 1, 1, 15.0, 1),
(1, 3, N'C201', 1, 2500000, 2, 2, 15.0, 1),
(1, 3, N'C202', 2, 3500000, 1, 2, 20.0, 1),

-- Chi nhánh 2 - Dãy D (Hạng I)
(2, 4, N'D101', 4, 6500000, 2, 1, 30.0, 1),
(2, 4, N'D102', 4, 6500000, 1, 1, 30.0, 1),
(2, 4, N'D201', 5, 9000000, 2, 2, 40.0, 1),

-- Chi nhánh 2 - Dãy E (Hạng II)
(2, 5, N'E101', 2, 3500000, 2, 1, 20.0, 1),
(2, 5, N'E102', 3, 5000000, 1, 1, 25.0, 1),
(2, 5, N'E201', 3, 5000000, 2, 2, 25.0, 1),

-- Chi nhánh 3 - Dãy F (Mới)
(3, 6, N'F101', 4, 6500000, 2, 1, 30.0, 1),
(3, 6, N'F102', 4, 6500000, 1, 1, 30.0, 1),
(3, 6, N'F201', 5, 9000000, 1, 2, 40.0, 1);

-- =====================================================
-- 5. TENANTS (Khách thuê)
-- =====================================================
INSERT INTO Tenants (FullName, IdentityCard, PhoneNumber, Email, BirthDate, Address, TemporaryRegistration, TemporaryRegistrationDate, TemporaryRegistrationExpiry, IsActive)
VALUES
(N'Phạm Minh Tuấn', N'032456789123', N'0901234567', N'tuan@example.com', '1995-01-12', N'12 Nguyễn Huệ, Cần Thơ', N'Tạm trú Quận 1', '2024-01-15', '2025-01-15', 1),
(N'Lê Thị Hoa', N'074589632145', N'0938123456', N'hoa@example.com', '1993-05-20', N'45 Lý Thường Kiệt, Cần Thơ', N'Tạm trú Quận 3', '2024-02-01', '2025-02-01', 1),
(N'Nguyễn Văn Long', N'021345678901', N'0912987654', N'long@example.com', '1990-11-02', N'89 Trần Hưng Đạo, Cần Thơ', N'Tạm trú Quận 5', '2024-03-01', '2025-03-01', 1),
(N'Trần Thu Uyên', N'058963214789', N'0945123789', N'uyen@example.com', '1996-07-15', N'15 Võ Thị Sáu, Cần Thơ', N'Tạm trú Quận 1', '2024-04-01', '2025-04-01', 1),
(N'Hoàng Minh Khôi', N'096321478965', N'0987654321', N'khoi@example.com', '1992-03-28', N'67 Cách Mạng Tháng 8, Cần Thơ', N'Tạm trú Quận 7', '2024-05-10', '2025-05-10', 1),
(N'Võ Thị Lan', N'085147963258', N'0978563245', N'lan@example.com', '1998-09-14', N'23 Ngô Gia Tự, Cần Thơ', N'Tạm trú Quận 2', '2024-06-15', '2025-06-15', 1),
(N'Đinh Văn Sơn', N'074185296374', N'0961234567', N'son@example.com', '1991-12-05', N'56 Hàng Dương, Hà Nội', N'Tạm trú Quận Ba Đình', '2024-07-01', '2025-07-01', 1),
(N'Bùi Thị Mỹ', N'063214785963', N'0954321098', N'my@example.com', '1997-08-22', N'34 Láng Hạ, Hà Nội', N'Tạm trú Quận Đống Đa', '2024-08-10', '2025-08-10', 1);

-- =====================================================
-- 6. DEPENDENTS (Người ở chung)
-- =====================================================
INSERT INTO Dependents (TenantId, FullName, Relationship, PhoneNumber)
VALUES
(1, N'Nguyễn Thị Mai', N'Vợ', N'0902223344'),
(1, N'Nguyễn Minh Khang', N'Con', N'0903334455'),
(2, N'Phạm Văn Bình', N'Anh trai', N'0934332211'),
(3, N'Lê Hoàng Nam', N'Bạn ở ghép', N'0912888999'),
(4, N'Trần Anh Thư', N'Chị gái', N'0944332211'),
(5, N'Hoàng Hữu Phúc', N'Bạn', N'0923456789');

-- =====================================================
-- 7. TENANT ROOM HISTORY (Lịch sử phòng)
-- =====================================================
INSERT INTO TenantRoomHistory (TenantId, RoomId, CheckInDate, CheckOutDate, Status, Notes)
VALUES
(1, 1, '2024-04-01', NULL, N'Active', N'Đang ở phòng A101'),
(2, 2, '2024-04-15', NULL, N'Active', N'Đang ở phòng A102'),
(3, 3, '2024-05-01', NULL, N'Active', N'Đang ở phòng A201'),
(4, 4, '2024-06-01', NULL, N'Active', N'Đang ở phòng A202'),
(5, 5, '2024-06-15', NULL, N'Active', N'Đang ở phòng A301'),
(6, 6, '2024-07-01', NULL, N'Active', N'Đang ở phòng B101'),
(7, 7, '2024-07-15', NULL, N'Active', N'Đang ở phòng B102'),
(8, 8, '2024-08-01', NULL, N'Active', N'Đang ở phòng B201');

-- =====================================================
-- 8. DEPOSITS (Tiền cọc)
-- =====================================================
INSERT INTO Deposits (TenantId, RoomId, DepositAmount, DepositDate, DepositType, Status, ReturnedAmount, ReturnedDate, Notes)
VALUES
(1, 1, 5000000, '2024-03-20', N'Official', N'Confirmed', 0, NULL, N'Cọc hợp đồng'),
(2, 2, 5000000, '2024-04-10', N'Official', N'Confirmed', 0, NULL, N'Cọc hợp đồng'),
(3, 3, 6500000, '2024-04-20', N'Official', N'Confirmed', 0, NULL, N'Cọc hợp đồng'),
(4, 4, 6500000, '2024-05-20', N'Official', N'Confirmed', 0, NULL, N'Cọc hợp đồng'),
(5, 5, 9000000, '2024-06-10', N'Official', N'Confirmed', 0, NULL, N'Cọc hợp đồng'),
(6, 6, 3500000, '2024-06-25', N'Official', N'Confirmed', 0, NULL, N'Cọc hợp đồng'),
(7, 7, 3500000, '2024-07-10', N'Official', N'Confirmed', 0, NULL, N'Cọc hợp đồng'),
(8, 8, 3500000, '2024-07-25', N'Official', N'Confirmed', 0, NULL, N'Cọc hợp đồng');

-- =====================================================
-- 9. CONTRACTS (Hợp đồng)
-- =====================================================
INSERT INTO Contracts (TenantId, RoomId, ContractNumber, SignDate, StartDate, EndDate, RentalPrice, DepositRequired, Terms, Status)
VALUES
(1, 1, N'HD-2024-001', '2024-03-25', '2024-04-01', '2025-03-31', 5000000, 5000000, N'Thanh toán đầu tháng, điện nước theo chỉ số.', N'Active'),
(2, 2, N'HD-2024-002', '2024-04-05', '2024-04-15', '2025-04-14', 5000000, 5000000, N'Thanh toán đầu tháng, cho phép nuôi thú nhỏ.', N'Active'),
(3, 3, N'HD-2024-003', '2024-04-25', '2024-05-01', '2025-04-30', 6500000, 6500000, N'Thanh toán ngày 10 hàng tháng.', N'Active'),
(4, 4, N'HD-2024-004', '2024-05-20', '2024-06-01', '2025-05-31', 6500000, 6500000, N'Thanh toán ngày 5 hàng tháng.', N'Active'),
(5, 5, N'HD-2024-005', '2024-06-10', '2024-06-15', '2025-06-14', 9000000, 9000000, N'Thanh toán đầu tháng, hỗ trợ ban công.', N'Active'),
(6, 6, N'HD-2024-006', '2024-06-25', '2024-07-01', '2025-06-30', 3500000, 3500000, N'Thanh toán ngày 10 hàng tháng.', N'Active'),
(7, 7, N'HD-2024-007', '2024-07-10', '2024-07-15', '2025-07-14', 3500000, 3500000, N'Thanh toán ngày 15 hàng tháng.', N'Active'),
(8, 8, N'HD-2024-008', '2024-07-25', '2024-08-01', '2025-07-31', 3500000, 3500000, N'Thanh toán ngày 1 hàng tháng.', N'Active');

-- =====================================================
-- 10. INVOICES (Hóa đơn)
-- =====================================================
INSERT INTO Invoices (InvoiceNumber, TenantId, RoomId, InvoiceDate, FromDate, ToDate, RentalCost, UtilityCost, OtherCost, TotalAmount, PaidAmount, RemainingAmount, Status, DueDate)
VALUES
-- Tháng 6/2024
(N'INV-2024-06-001', 1, 1, '2024-06-02', '2024-06-01', '2024-06-30', 5000000, 1100000, 0, 6100000, 6100000, 0, N'Paid', '2024-06-10'),
(N'INV-2024-06-002', 2, 2, '2024-06-02', '2024-06-01', '2024-06-30', 5000000, 1030000, 0, 6030000, 6030000, 0, N'Paid', '2024-06-10'),
(N'INV-2024-06-003', 3, 3, '2024-06-02', '2024-06-01', '2024-06-30', 6500000, 1200000, 0, 7700000, 0, 7700000, N'Issued', '2024-06-10'),
(N'INV-2024-06-004', 4, 4, '2024-06-10', '2024-06-01', '2024-06-30', 6500000, 1100000, 0, 7600000, 0, 7600000, N'Issued', '2024-06-15'),

-- Tháng 7/2024
(N'INV-2024-07-001', 1, 1, '2024-07-02', '2024-07-01', '2024-07-31', 5000000, 1050000, 0, 6050000, 6050000, 0, N'Paid', '2024-07-10'),
(N'INV-2024-07-002', 2, 2, '2024-07-02', '2024-07-01', '2024-07-31', 5000000, 980000, 0, 5980000, 5980000, 0, N'Paid', '2024-07-10'),
(N'INV-2024-07-003', 5, 5, '2024-07-15', '2024-07-01', '2024-07-31', 9000000, 1500000, 0, 10500000, 5250000, 5250000, N'PartialPaid', '2024-07-10'),
(N'INV-2024-07-004', 6, 6, '2024-07-10', '2024-07-01', '2024-07-31', 3500000, 750000, 0, 4250000, 0, 4250000, N'Issued', '2024-07-15'),

-- Tháng 8/2024
(N'INV-2024-08-001', 1, 1, '2024-08-02', '2024-08-01', '2024-08-31', 5000000, 1080000, 200000, 6280000, 6280000, 0, N'Paid', '2024-08-10'),
(N'INV-2024-08-002', 3, 3, '2024-08-05', '2024-08-01', '2024-08-31', 6500000, 1150000, 0, 7650000, 3825000, 3825000, N'PartialPaid', '2024-08-10'),
(N'INV-2024-08-003', 7, 7, '2024-08-10', '2024-08-01', '2024-08-31', 3500000, 900000, 0, 4400000, 0, 4400000, N'Issued', '2024-08-15'),
(N'INV-2024-08-004', 8, 8, '2024-08-15', '2024-08-01', '2024-08-31', 3500000, 850000, 0, 4350000, 0, 4350000, N'Overdue', '2024-08-20');

-- =====================================================
-- 11. PAYMENTS (Thanh toán)
-- =====================================================
INSERT INTO Payments (InvoiceId, PaymentDate, PaymentAmount, PaymentMethod, TransactionReference, Notes)
VALUES
(1, '2024-06-03', 6100000, N'Transfer', N'TXN20240603001', N'Thanh toán đủ'),
(2, '2024-06-05', 6030000, N'Cash', N'', N'Thanh toán mặt'),
(5, '2024-07-03', 6050000, N'Transfer', N'TXN20240703001', N'Thanh toán đủ'),
(6, '2024-07-05', 5980000, N'QR', N'TXN20240705001', N'Thanh toán QR'),
(7, '2024-07-20', 5250000, N'Transfer', N'TXN20240720001', N'Thanh toán bộ phận'),
(9, '2024-08-05', 6280000, N'Cash', N'', N'Thanh toán mặt'),
(10, '2024-08-15', 3825000, N'Transfer', N'TXN20240815001', N'Thanh toán bộ phận');

-- =====================================================
-- 12. UTILITY READINGS (Chỉ số điện nước)
-- =====================================================
INSERT INTO UtilityReadings (RoomId, UtilityTypeId, ReadingDate, PreviousReading, CurrentReading, UsageAmount, UnitPrice, TotalCost, Notes)
VALUES
-- Điện - Tháng 6
(1, 1, '2024-06-01', 1000, 1100, 100, 3500, 350000, N'Điện tháng 6'),
(2, 1, '2024-06-01', 900, 980, 80, 3500, 280000, N'Điện tháng 6'),
(3, 1, '2024-06-01', 500, 560, 60, 3500, 210000, N'Điện tháng 6'),
(4, 1, '2024-06-01', 800, 860, 60, 3500, 210000, N'Điện tháng 6'),
(5, 1, '2024-06-01', 300, 400, 100, 3500, 350000, N'Điện tháng 6'),
-- Nước - Tháng 6
(1, 2, '2024-06-01', 200, 230, 30, 25000, 750000, N'Nước tháng 6'),
(2, 2, '2024-06-01', 150, 180, 30, 25000, 750000, N'Nước tháng 6'),
(3, 2, '2024-06-01', 120, 140, 20, 25000, 500000, N'Nước tháng 6'),
(4, 2, '2024-06-01', 180, 200, 20, 25000, 500000, N'Nước tháng 6'),
(5, 2, '2024-06-01', 80, 120, 40, 25000, 1000000, N'Nước tháng 6'),
-- Điện - Tháng 7
(1, 1, '2024-07-01', 1100, 1180, 80, 3500, 280000, N'Điện tháng 7'),
(2, 1, '2024-07-01', 980, 1050, 70, 3500, 245000, N'Điện tháng 7'),
(3, 1, '2024-07-01', 560, 630, 70, 3500, 245000, N'Điện tháng 7'),
(5, 1, '2024-07-01', 400, 500, 100, 3500, 350000, N'Điện tháng 7'),
-- Nước - Tháng 7
(1, 2, '2024-07-01', 230, 260, 30, 25000, 750000, N'Nước tháng 7'),
(2, 2, '2024-07-01', 180, 210, 30, 25000, 750000, N'Nước tháng 7'),
(3, 2, '2024-07-01', 140, 165, 25, 25000, 625000, N'Nước tháng 7'),
(5, 2, '2024-07-01', 120, 160, 40, 25000, 1000000, N'Nước tháng 7');

-- =====================================================
-- 13. MAINTENANCE TICKETS (Phiếu bảo trì)
-- =====================================================
INSERT INTO MaintenanceTickets (TicketNumber, RoomId, RequestorType, RequestorId, IssueDescription, Priority, Status, CreatedDate, Notes)
VALUES
(N'MT-2024-001', 1, N'Tenant', 1, N'Hỏng vòi nước phòng tắm', N'Medium', N'InProgress', GETDATE(), N'Đã giao cho kỹ thuật'),
(N'MT-2024-002', 2, N'Tenant', 2, N'Máy lạnh không lạnh', N'High', N'Created', GETDATE(), N'Chờ kiểm tra'),
(N'MT-2024-003', 3, N'Tenant', 3, N'Rò rỉ điện tường phòng', N'Urgent', N'InProgress', GETDATE(), N'Ưu tiên xử lý'),
(N'MT-2024-004', 4, N'Staff', NULL, N'Cửa phòng hỏng bản lề', N'Low', N'Completed', GETDATE(), N'Đã thay bản lề'),
(N'MT-2024-005', 5, N'Tenant', 5, N'Cửa sổ rê khó', N'Low', N'Created', GETDATE(), N'Đang chờ'),
(N'MT-2024-006', 6, N'Tenant', 6, N'Đèn phòng không sáng', N'Medium', N'InProgress', GETDATE(), N'Thay bóng đèn');

-- =====================================================
-- 14. ASSETS (Tài sản phòng)
-- =====================================================
INSERT INTO Assets (AssetCode, AssetName, Category, RoomId, Quantity, Condition, PurchaseDate, PurchasePrice, Description, IsActive)
VALUES
-- Phòng A101
(N'TS-A101-001', N'Máy lạnh Panasonic', N'Thiết bị', 1, 1, N'Good', '2023-01-10', 8000000, N'Hàng còn bảo hành', 1),
(N'TS-A101-002', N'Bàn gỗ', N'Nội thất', 1, 1, N'Good', '2023-03-12', 1200000, N'Bàn làm việc', 1),
(N'TS-A101-003', N'Ghế gỗ', N'Nội thất', 1, 2, N'Good', '2023-03-12', 600000, N'Ghế phòng', 1),
-- Phòng A102
(N'TS-A102-001', N'Máy lạnh LG', N'Thiết bị', 2, 1, N'Good', '2023-02-15', 7500000, N'Bảo hành 3 năm', 1),
(N'TS-A102-002', N'Giường gỗ', N'Nội thất', 2, 1, N'Good', '2023-04-10', 2500000, N'Giường đôi', 1),
-- Phòng A201
(N'TS-A201-001', N'Máy nước nóng', N'Thiết bị', 3, 1, N'Fair', '2023-05-15', 3500000, N'Bảo trì 1 lần/năm', 1),
(N'TS-A201-002', N'Tủ lạnh', N'Thiết bị', 3, 1, N'Good', '2023-06-20', 5000000, N'Bảo hành 2 năm', 1),
-- Phòng B101
(N'TS-B101-001', N'Quạt điện', N'Thiết bị', 6, 1, N'Good', '2023-07-01', 500000, N'Quạt trần', 1),
(N'TS-B101-002', N'Tủ quần áo', N'Nội thất', 6, 1, N'Good', '2023-08-10', 1500000, N'Tủ 2 buồng', 1);

-- =====================================================
-- KẾT THÚC
-- =====================================================
PRINT N'✓ Dữ liệu sample đã được thêm thành công!';
PRINT N'✓ Tổng: 8 phòng được cho thuê, 8 khách thuê, 8 hợp đồng, 12 hóa đơn, 7 thanh toán';
PRINT N'✓ Bạn có thể test filter theo diện tích phòng (15-40m²)';
