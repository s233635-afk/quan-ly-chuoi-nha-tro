-- Seed missing "Dang o" status for staff view.
-- Idempotent insert: only adds the status if it does not exist.

IF OBJECT_ID(N'RoomStatuses', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RoomStatuses WHERE StatusName = N'Đang ở')
    BEGIN
        INSERT INTO RoomStatuses (StatusName, Description)
        VALUES (N'Đang ở', N'Phòng đang ở');
    END
END
