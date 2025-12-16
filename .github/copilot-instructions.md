# AI Coding Instructions - Quản Lý Chuỗi Nhà Trọ

## 🏗️ Architecture Overview

**3-Tier Architecture** (GUI → BLL → DAL → SQL Server)
- **GUI Layer**: Windows Forms (`quan-ly-chuoi-nha-tro/GUI/`) - Direct user interaction
- **BLL Layer**: Business Logic (`quan-ly-chuoi-nha-tro/BLL/`) - Processing & validation
- **DAL Layer**: Data Access (`quan-ly-chuoi-nha-tro/DAL/DatabaseHelper.cs`) - SQL execution
- **Database**: SQL Server (SmarterASP.NET: `SQL9001.site4now.net`, db: `db_ac1f11_quanlynhatro`)

### Key Separation Principle
- GUI forms call BLL methods (never DAL directly)
- BLL delegates to DAL via `DatabaseHelper` instance
- All async operations use `Task<T>` pattern throughout

## 👤 Access Control Model

**Role-based Access** (from Users table):
- **RoleId 1**: Admin (can access all 12 admin modules)
- **RoleId 2**: Staff (limited to assigned branch data)
- **Staff Filtering**: Always use `branchId` parameter for queries (e.g., `GetTenantsByBranchAsync(int? branchId)`)

When creating forms:
- Accept optional `branchId` parameter in constructor
- Pass it to BLL methods to enforce data isolation
- Admin modules pass `null` for all branches; Staff modules use their assigned branch

## 📂 File Structure Conventions

**BLL Classes**: One main class per role/purpose
- `AdminDataBLL.cs`: All 12 admin modules (Room, Tenant, Contract, Deposit, Invoice, Payment, Utility, Maintenance, Asset, Notification, SystemSettings, Dashboard)
- `StaffBLL.cs`: Staff-limited operations with `branchId` filtering
- `UserBLL.cs`: Authentication (registration, login, password reset)

**DAL**: Centralized in `DatabaseHelper.cs` with patterns:
```csharp
public Task<DataTable> GetRoomsByBranchAsync(int? branchId = null)
    => _dbHelper.ExecuteAsync("sp_GetRooms", new { branchId });
```

**GUI Forms**: Pair pattern for list-edit:
- `FrmRoomManager.cs` → `FrmRoomEditor.cs`
- `FrmTenantManager.cs` → `FrmTenantEditor.cs`
- Use constructor DI: `new FrmEditor(bll, dataId, branchId)` for edit mode

## 🎨 UI Pattern Standards

**Form Layout Pattern** (seen in FrmTenantManager):
```
┌─ Toolbar (Height=70px) ────────────────────────┐
│  Search Box | Buttons: Add, Edit, Delete, Refresh
│  Label: "Total: X records"                      │
├─ TabControl (Fill) ───────────────────────────────┤
│  Tab1: DataGridView (Read-only, AllowUserToAddRows=false)
│  Tab2: DataGridView (Dependents/Related data)
│  Tab3: DataGridView (History/Related data)
└────────────────────────────────────────────────────┘
```

**Color Scheme**:
- Background: `Color.FromArgb(240, 242, 245)` (light gray)
- Toolbar: `Color.White`
- Grid: `Color.White` with `BorderStyle.None`
- Cards: `Color.FromArgb(52, 168, 219)` (blue) - adjust by category

**DataGridView Setup**:
```csharp
grid.ReadOnly = true;
grid.AllowUserToAddRows = false;
grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
grid.MultiSelect = false;
grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
grid.BackgroundColor = Color.White;
```

## 🔄 Common Workflows

### Adding a New Manager Form (e.g., FrmXxxManager)
1. Create `FrmXxxManager.cs` that accepts `(AdminDataBLL bll, int? branchId)`
2. Define toolbar with search + CRUD buttons
3. Add TabControl with grids for main entity + related data
4. Load data in `Load` event: `Load += async (s, e) => await LoadDataAsync();`
5. Implement `ApplySearch()`, `Add()`, `Edit()`, `Delete()`, `Refresh()`
6. Create corresponding `FrmXxxEditor.cs` modal form for editing

### SQL Query Pattern
- Use `sp_` prefixed stored procedures (preferred, see DatabaseHelper calls)
- Or raw SQL with parameterized queries: `@paramName`
- Always await async: `await _dbHelper.ExecuteScalarAsync(...)`
- DataTable results: Common return type for DataGridView binding

### Async/Await Pattern
```csharp
// BLL wrapper delegates to DAL:
public Task<DataTable> GetTenantsAsync() 
    => _dbHelper.GetTenantsAsync();

// GUI loads asynchronously:
Load += async (s, e) => await LoadDataAsync();
private async Task LoadDataAsync() {
    _tenants = await _bll.GetTenantsAsync();
    _dgvTenants.DataSource = _tenants;
}
```

## 📊 12 Admin Modules Structure

1. **Branch Management**: `FrmBranch.cs` (sections, details)
2. **Room Management**: `FrmRoomManager.cs` (types, statuses, availability)
3. **Tenant Management**: `FrmTenantManager.cs` (+ dependents, history)
4. **Contract Management**: `FrmContractManager.cs`
5. **Deposit Management**: `FrmDepositManager.cs`
6. **Invoice Management**: `FrmInvoiceManager.cs` (with monthly generation)
7. **Payment Management**: `FrmPaymentManager.cs` (linked to invoices)
8. **Utility Management**: `FrmUtilityManager.cs` (types + readings)
9. **Maintenance Management**: `FrmMaintenanceManager.cs` (tickets, status)
10. **Asset Management**: `FrmAssetManager.cs` (per room, condition)
11. **Notification Management**: `FrmNotificationManager.cs` (send history)
12. **System Settings**: `FrmSystemSettingsManager.cs`

Each module follows CRUD pattern with relationship handling.

## ⚠️ Critical Don'ts

- **Never skip branch filtering** for Staff forms (data isolation)
- **Never call DAL directly from GUI** (breaks 3-tier)
- **Never hardcode credentials** (use App.config connectionString)
- **Never use synchronous database calls** (always `async Task<T>`)
- **Never allow forms to modify read-only grids** (`AllowUserToAddRows = false`)
- **Don't instantiate BLL in GUI constructors** without DI - accept via parameter

## 🔌 Database Connection

**Location**: `quan-ly-chuoi-nha-tro/DAL/DatabaseHelper.cs`
**Config**: `App.config` → `connectionStrings[@"QuanLyNhaTro"]`
**Fallback**: Hardcoded in code (for deployment flexibility)
**Timeout**: 10 seconds (configurable via `DbCommandTimeoutSeconds` appsetting)

## 🛠️ Development & Testing

**Building**: Visual Studio - Standard .NET 4.7.2 WinForms build
**Testing Data**: `sample_data.sql` in project root (load if needed)
**Common Queries**: Check `setup_database_final.sql` for schema (18 tables total)

## 📝 Naming Conventions

- **Async methods**: Suffix `Async` (e.g., `GetTenantsAsync`)
- **UI controls**: Prefix type (e.g., `_dgvTenants`, `_btnAdd`, `_txtSearch`)
- **Private fields**: Underscore prefix (e.g., `_bll`, `_branchId`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `SearchPlaceholder = "Tìm..."`
- **Vietnamese UI text**: Use consistently (labels, messages, buttons)

## 🎯 Form Constructor Pattern

```csharp
// Admin form (all branches):
public FrmTenantManager(AdminDataBLL bll = null) {
    _bll = bll ?? new AdminDataBLL();
    _branchId = null;
}

// Staff form (single branch):
public FrmStaffRoomManager(StaffBLL bll, int branchId) {
    _bll = bll ?? new StaffBLL();
    _branchId = branchId;
}
```

When instantiating in parent: `new FrmTenantManager(_bll, _branchId)`
