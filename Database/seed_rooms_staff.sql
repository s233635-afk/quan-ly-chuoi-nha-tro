-- Seed rooms for staff view (A01-A10, B01-B10) for BranchId 1.
-- Idempotent insert: only adds missing rooms/sections.

DECLARE @BranchId INT = 1;
DECLARE @SectionAId INT;
DECLARE @SectionBId INT;

SELECT @SectionAId = SectionId
FROM BranchSections
WHERE BranchId = @BranchId AND SectionCode = 'A';

IF @SectionAId IS NULL
BEGIN
    INSERT INTO BranchSections (BranchId, SectionCode, SectionName)
    VALUES (@BranchId, 'A', 'Day A');
    SET @SectionAId = SCOPE_IDENTITY();
END

SELECT @SectionBId = SectionId
FROM BranchSections
WHERE BranchId = @BranchId AND SectionCode = 'B';

IF @SectionBId IS NULL
BEGIN
    INSERT INTO BranchSections (BranchId, SectionCode, SectionName)
    VALUES (@BranchId, 'B', 'Day B');
    SET @SectionBId = SCOPE_IDENTITY();
END

DECLARE @StatusEmpty INT = (SELECT TOP 1 StatusId FROM RoomStatuses ORDER BY StatusId);
DECLARE @RoomTypeSingle INT = (SELECT TOP 1 RoomTypeId FROM RoomTypes ORDER BY RoomTypeId);
DECLARE @RoomTypeDouble INT = (SELECT TOP 1 RoomTypeId FROM RoomTypes WHERE RoomTypeId <> @RoomTypeSingle ORDER BY RoomTypeId);
DECLARE @RoomTypePremium INT = (SELECT TOP 1 RoomTypeId FROM RoomTypes WHERE RoomTypeId NOT IN (@RoomTypeSingle, @RoomTypeDouble) ORDER BY RoomTypeId);

;WITH nums AS (
    SELECT 1 AS n UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
    UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10
)
INSERT INTO Rooms (BranchId, SectionId, RoomNumber, RoomTypeId, RoomPrice, CurrentStatusId)
SELECT
    @BranchId,
    @SectionAId,
    CONCAT('A', RIGHT('0' + CAST(n AS VARCHAR(2)), 2)),
    CASE WHEN n <= 4 THEN @RoomTypeSingle WHEN n <= 7 THEN @RoomTypeDouble ELSE @RoomTypePremium END,
    CASE WHEN n <= 4 THEN 3000000 WHEN n <= 7 THEN 3500000 ELSE 5000000 END,
    @StatusEmpty
FROM nums
WHERE NOT EXISTS (
    SELECT 1
    FROM Rooms
    WHERE BranchId = @BranchId
      AND RoomNumber = CONCAT('A', RIGHT('0' + CAST(n AS VARCHAR(2)), 2))
);

;WITH nums AS (
    SELECT 1 AS n UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
    UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10
)
INSERT INTO Rooms (BranchId, SectionId, RoomNumber, RoomTypeId, RoomPrice, CurrentStatusId)
SELECT
    @BranchId,
    @SectionBId,
    CONCAT('B', RIGHT('0' + CAST(n AS VARCHAR(2)), 2)),
    CASE WHEN n <= 4 THEN @RoomTypeSingle WHEN n <= 7 THEN @RoomTypeDouble ELSE @RoomTypePremium END,
    CASE WHEN n <= 4 THEN 3000000 WHEN n <= 7 THEN 3500000 ELSE 5000000 END,
    @StatusEmpty
FROM nums
WHERE NOT EXISTS (
    SELECT 1
    FROM Rooms
    WHERE BranchId = @BranchId
      AND RoomNumber = CONCAT('B', RIGHT('0' + CAST(n AS VARCHAR(2)), 2))
);
