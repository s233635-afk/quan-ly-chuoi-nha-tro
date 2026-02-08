using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using QuanLyNhaTro.BLL.Models;
using quan_ly_chuoi_nha_tro.GUI;

namespace QuanLyNhaTro.BLL.Services
{
    /// <summary>
    /// Service for loading and managing room-related data
    /// Extracted from FrmRoomManager (lines 346-468)
    /// </summary>
    public class RoomDataService
    {
        private readonly AdminDataBLL _bll;

        public RoomDataService(AdminDataBLL bll)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
        }

        /// <summary>
        /// Load all room-related data asynchronously
        /// Extracted from FrmRoomManager.LoadRoomsAsync()
        /// </summary>
        public async Task<RoomDataContext> LoadAllDataAsync(int? branchId = null)
        {
            var roomsTask = _bll.GetRoomsAsync();
            var statusesTask = _bll.GetRoomStatusesAsync();
            var typesTask = _bll.GetRoomTypesAsync();
            var historyTask = _bll.GetTenantHistoryAsync();
            var contractsTask = _bll.GetContractsAsync();
            var tenantsTask = _bll.GetTenantsAsync();
            var assetsTask = _bll.GetAssetsAsync();

            await Task.WhenAll(roomsTask, statusesTask, typesTask, historyTask, contractsTask, tenantsTask, assetsTask);

            var context = new RoomDataContext
            {
                Rooms = roomsTask.Result ?? new DataTable(),
                Statuses = statusesTask.Result ?? new DataTable(),
                RoomTypes = typesTask.Result ?? new DataTable(),
                TenantHistory = historyTask.Result ?? new DataTable(),
                Contracts = contractsTask.Result ?? new DataTable(),
                Tenants = tenantsTask.Result ?? new DataTable(),
                Assets = assetsTask.Result ?? new DataTable()
            };

            // Normalize data
            NormalizeLoadedTables(context);

            // Filter by branch if specified
            if (branchId.HasValue && context.Rooms.Columns.Contains("BranchId"))
            {
                var filtered = context.Rooms.AsEnumerable()
                    .Where(r => int.TryParse(r["BranchId"]?.ToString(), out var bid) && bid == branchId.Value);
                context.Rooms = filtered.Any() ? filtered.CopyToDataTable() : context.Rooms.Clone();
            }

            // Enrich data
            EnsureDisplayColumns(context);
            PopulateOccupancy(context);
            NormalizeRoomStatusForOccupancy(context);

            return context;
        }

        /// <summary>
        /// Normalize text encoding in loaded tables
        /// Extracted from FrmRoomManager.NormalizeLoadedTables()
        /// </summary> 
        private void NormalizeLoadedTables(RoomDataContext context)
        {
            TextFixer.ForceFixDataTable(context.Rooms, "RoomNumber", "TypeName", "RoomTypeName", "StatusName", "BranchName", "SectionName");
            TextFixer.ForceFixDataTable(context.RoomTypes, "RoomTypeName", "Amenities", "Description");
            TextFixer.ForceFixDataTable(context.Statuses, "StatusName", "Description");
            TextFixer.ForceFixDataTable(context.Assets, "AssetName", "Description");
            TextFixer.ForceFixDataTable(context.Tenants, "FullName", "PhoneNumber");

            RoomTypeCatalog.CanonicalizeRoomTypeColumn(context.Rooms, "TypeName");
            RoomTypeCatalog.CanonicalizeRoomTypeColumn(context.RoomTypes, "RoomTypeName");
        }

        /// <summary>
        /// Ensure display columns exist and populate them
        /// Extracted from FrmRoomManager.EnsureDisplayColumns()
        /// </summary>
        private void EnsureDisplayColumns(RoomDataContext context)
        {
            if (context.Rooms == null) return;

            if (!context.Rooms.Columns.Contains("TypeName"))
                context.Rooms.Columns.Add("TypeName", typeof(string));
            if (!context.Rooms.Columns.Contains("StatusName"))
                context.Rooms.Columns.Add("StatusName", typeof(string));
            if (!context.Rooms.Columns.Contains("Occupants"))
                context.Rooms.Columns.Add("Occupants", typeof(int));

            var typeLookup = context.RoomTypes?.AsEnumerable()
                .Where(r => context.RoomTypes.Columns.Contains("RoomTypeId"))
                .ToDictionary(r => r["RoomTypeId"], r => SafeToString(r, "RoomTypeName"));

            var statusLookup = context.Statuses?.AsEnumerable()
                .Where(r => context.Statuses.Columns.Contains("StatusId"))
                .ToDictionary(r => r["StatusId"], r => SafeToString(r, "StatusName"));

            foreach (DataRow row in context.Rooms.Rows)
            {
                if (typeLookup != null && context.Rooms.Columns.Contains("RoomTypeId") && typeLookup.TryGetValue(row["RoomTypeId"], out var typeName))
                    row["TypeName"] = RoomTypeCatalog.Canonicalize(typeName);
                if (statusLookup != null && context.Rooms.Columns.Contains("CurrentStatusId") && statusLookup.TryGetValue(row["CurrentStatusId"], out var statusName))
                    row["StatusName"] = statusName;
            }
        }

        /// <summary>
        /// Populate occupancy data for each room
        /// Extracted from FrmRoomManager.PopulateOccupancy()
        /// </summary>
        private void PopulateOccupancy(RoomDataContext context)
        {
            if (context.Rooms == null) return;

            var activeByRoom = new Dictionary<int, int>();
            if (context.TenantHistory != null && context.TenantHistory.Rows.Count > 0 && context.TenantHistory.Columns.Contains("RoomId"))
            {
                foreach (DataRow r in context.TenantHistory.Rows)
                {
                    if (!int.TryParse(r["RoomId"]?.ToString(), out var rid)) continue;

                    bool isActive = string.IsNullOrWhiteSpace(r.Table.Columns.Contains("CheckOutDate") ? r["CheckOutDate"]?.ToString() : null);
                    if (!isActive && r.Table.Columns.Contains("Status"))
                    {
                        var statusText = r["Status"]?.ToString() ?? string.Empty;
                        isActive = statusText.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0
                                   || statusText.IndexOf("Ä‘ang", StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    if (!isActive) continue;
                    activeByRoom[rid] = activeByRoom.TryGetValue(rid, out var count) ? count + 1 : 1;
                }
            }

            if (activeByRoom.Count == 0) return;

            foreach (DataRow row in context.Rooms.Rows)
            {
                if (!int.TryParse(row["RoomId"]?.ToString(), out var rid)) continue;
                var current = 0;
                if (row.Table.Columns.Contains("Occupants") && int.TryParse(row["Occupants"]?.ToString(), out var stored))
                    current = stored;
                row["Occupants"] = activeByRoom.TryGetValue(rid, out var count) ? count : current;
            }
        }

        /// <summary>
        /// Normalize room status based on occupancy
        /// Extracted from FrmRoomManager.NormalizeRoomStatusForOccupancy()
        /// </summary>
        private void NormalizeRoomStatusForOccupancy(RoomDataContext context)
        {
            if (context.Rooms == null) return;
            var emptyId = GetEmptyStatusId(context.Statuses);
            var emptyName = GetEmptyStatusName(context.Statuses);

            foreach (DataRow row in context.Rooms.Rows)
            {
                int occupants = TryGetInt(row, "Occupants");
                string statusName = row.Table.Columns.Contains("StatusName") ? row["StatusName"]?.ToString() ?? string.Empty : string.Empty;

                if (occupants < 1 && IsOccupiedStatusName(statusName))
                {
                    if (emptyId.HasValue && row.Table.Columns.Contains("CurrentStatusId"))
                        row["CurrentStatusId"] = emptyId.Value;
                    if (!string.IsNullOrWhiteSpace(emptyName) && row.Table.Columns.Contains("StatusName"))
                        row["StatusName"] = emptyName;
                }
            }
        }

        /// <summary>
        /// Build asset lookup dictionary by room ID
        /// Extracted from FrmRoomManager.BuildAssetLookup()
        /// </summary>
        public Dictionary<int, List<DataRow>> BuildAssetLookup(DataTable assets, int? branchId = null)
        {
            var lookup = new Dictionary<int, List<DataRow>>();
            if (assets == null || !assets.Columns.Contains("RoomId")) return lookup;

            IEnumerable<DataRow> rows = assets.AsEnumerable();
            if (branchId.HasValue && assets.Columns.Contains("BranchId"))
            {
                rows = rows.Where(r => TryGetInt(r, "BranchId") == branchId.Value);
            }

            foreach (var asset in rows)
            {
                if (!int.TryParse(asset["RoomId"]?.ToString(), out var roomId) || roomId <= 0)
                    continue;

                if (!lookup.TryGetValue(roomId, out var list))
                {
                    list = new List<DataRow>();
                    lookup[roomId] = list;
                }
                list.Add(asset);
            }

            return lookup;
        }

        // Helper methods
        private static string SafeToString(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName)) return null;
            return row[columnName]?.ToString();
        }

        private static int TryGetInt(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName)) return 0;
            return int.TryParse(row[columnName]?.ToString(), out var result) ? result : 0;
        }

        private static int? GetEmptyStatusId(DataTable statuses)
        {
            if (statuses == null || !statuses.Columns.Contains("StatusId")) return null;
            var emptyRow = statuses.AsEnumerable()
                .FirstOrDefault(r => (SafeToString(r, "StatusName") ?? "").IndexOf("trá»‘ng", StringComparison.OrdinalIgnoreCase) >= 0
                                  || (SafeToString(r, "StatusName") ?? "").IndexOf("empty", StringComparison.OrdinalIgnoreCase) >= 0);
            return emptyRow != null && int.TryParse(emptyRow["StatusId"]?.ToString(), out var id) ? (int?)id : null;
        }

        private static string GetEmptyStatusName(DataTable statuses)
        {
            if (statuses == null) return null;
            var emptyRow = statuses.AsEnumerable()
                .FirstOrDefault(r => (SafeToString(r, "StatusName") ?? "").IndexOf("trá»‘ng", StringComparison.OrdinalIgnoreCase) >= 0
                                  || (SafeToString(r, "StatusName") ?? "").IndexOf("empty", StringComparison.OrdinalIgnoreCase) >= 0);
            return SafeToString(emptyRow, "StatusName");
        }

        private static bool IsOccupiedStatusName(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName)) return false;
            var lower = statusName.ToLowerInvariant();
            return lower.Contains("Ä‘ang") || lower.Contains("occupied") || lower.Contains("thuĂª");
        }
    }
}
