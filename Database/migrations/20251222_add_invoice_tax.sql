-- Add tax fields for existing databases (safe to run multiple times).
IF COL_LENGTH('Invoices', 'TaxRate') IS NULL
    ALTER TABLE Invoices ADD TaxRate DECIMAL(6,2) NOT NULL CONSTRAINT DF_Invoices_TaxRate DEFAULT 0;

IF COL_LENGTH('Invoices', 'TaxAmount') IS NULL
    ALTER TABLE Invoices ADD TaxAmount DECIMAL(15,2) NOT NULL CONSTRAINT DF_Invoices_TaxAmount DEFAULT 0;

-- Seed default tax rate if missing.
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = N'DefaultTaxRatePercent')
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description)
    VALUES (N'DefaultTaxRatePercent', N'0', N'Mức thuế (%) áp dụng theo doanh thu hóa đơn');
