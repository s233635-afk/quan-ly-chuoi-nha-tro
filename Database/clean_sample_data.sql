-- =====================================================
-- CLEAN SAMPLE DATA - XÓA TẤT CẢ DỮ LIỆU MẪU
-- Giữ lại schema (bảng trống)
-- =====================================================

-- Tắt constraint check tạm thời
SET IDENTITY_INSERT [dbo].[Payments] ON;
SET IDENTITY_INSERT [dbo].[Payments] OFF;

-- Xóa dữ liệu từ các bảng child trước (FK constraints)
DELETE FROM [dbo].[Payments];
DELETE FROM [dbo].[UtilityReadings];
DELETE FROM [dbo].[MaintenanceTickets];
DELETE FROM [dbo].[Assets];
DELETE FROM [dbo].[Invoices];
DELETE FROM [dbo].[Deposits];
DELETE FROM [dbo].[Contracts];
DELETE FROM [dbo].[TenantRoomHistory];
DELETE FROM [dbo].[Dependents];
DELETE FROM [dbo].[Tenants];
DELETE FROM [dbo].[Rooms];
DELETE FROM [dbo].[BranchSections];
DELETE FROM [dbo].[Branches];
DELETE FROM [dbo].[Notifications];
DELETE FROM [dbo].[Users];

-- Reset Identity Seed về 1
DBCC CHECKIDENT ([Payments], RESEED, 0);
DBCC CHECKIDENT ([UtilityReadings], RESEED, 0);
DBCC CHECKIDENT ([MaintenanceTickets], RESEED, 0);
DBCC CHECKIDENT ([Assets], RESEED, 0);
DBCC CHECKIDENT ([Invoices], RESEED, 0);
DBCC CHECKIDENT ([Deposits], RESEED, 0);
DBCC CHECKIDENT ([Contracts], RESEED, 0);
DBCC CHECKIDENT ([TenantRoomHistory], RESEED, 0);
DBCC CHECKIDENT ([Dependents], RESEED, 0);
DBCC CHECKIDENT ([Tenants], RESEED, 0);
DBCC CHECKIDENT ([Rooms], RESEED, 0);
DBCC CHECKIDENT ([BranchSections], RESEED, 0);
DBCC CHECKIDENT ([Branches], RESEED, 0);
DBCC CHECKIDENT ([Notifications], RESEED, 0);
DBCC CHECKIDENT ([Users], RESEED, 0);

-- =====================================================
-- CHỈ GIỮ LẠI: Roles, RoomStatuses, RoomTypes, UtilityTypes, SystemSettings
-- (Dữ liệu cấu hình hệ thống, không phải dữ liệu mẫu)
-- =====================================================

PRINT '✓ Đã xóa tất cả dữ liệu mẫu. Database sạch sẽ, schema vẫn còn.';
