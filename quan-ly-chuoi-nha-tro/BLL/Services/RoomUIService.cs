using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using quan_ly_chuoi_nha_tro.GUI;

namespace QuanLyNhaTro.BLL.Services
{
    /// <summary>
    /// Service for UI-related room operations (filtering, sorting, formatting)
    /// Extracted from FrmRoomManager (lines 470-722, 1990-2183)
    /// </summary>
    public class RoomUIService
    {
        /// <summary>
        /// Build inline asset summary for a room
        ///  Extracted from FrmRoomManager.BuildAssetInlineSummary()
        /// </summary>
        public string BuildAssetSummary(int roomId, Dictionary<int, List<DataRow>> assetsByRoom)
        {
            if (!assetsByRoom.TryGetValue(roomId, out var list) || list == null || list.Count == 0)
                return null;

            var grouped = list
                .GroupBy(r =>
                {
                    var rawName = SafeReadString(r, "AssetName");
                    var fixedName = NormalizeAssetText(rawName);
                    return fixedName ?? $"TĂ i sáº£n #{SafeToString(r, "AssetId") ?? "?"}";
                })
                .Select(g => new
                {
                    Name = g.Key,
                    Quantity = g.Sum(r => Math.Max(1, TryGetInt(r, "Quantity")))
                })
                .OrderByDescending(g => g.Quantity)
                .ThenBy(g => g.Name)
                .ToList();

            if (grouped.Count == 0) return null;

            var top = grouped.Take(2)
                .Select(g => $"{g.Name}Ă—{g.Quantity}")
                .ToList();

            var summary = string.Join(", ", top);
            int remaining = grouped.Count - top.Count;
            if (remaining > 0)
            {
                summary = string.IsNullOrEmpty(summary) ? $"(+{remaining} má»¥c)" : $"{summary} +{remaining}";
            }

            return summary;
        }

        /// <summary>
        /// Calculate total asset charge for a room
        /// Extracted from FrmRoomManager.GetAssetCharge()
        /// </summary>
        public decimal CalculateAssetCharge(int roomId, Dictionary<int, List<DataRow>> assetsByRoom)
        {
            if (!assetsByRoom.TryGetValue(roomId, out var list) || list == null || list.Count == 0)
                return 0m;

            decimal total = 0m;
            foreach (var asset in list)
            {
                int qty = Math.Max(1, TryGetInt(asset, "Quantity"));
                decimal price = TryGetDecimal(asset, "PurchasePrice") ?? 0m;
                total += qty * price;
            }

            return total;
        }

        /// <summary>
        /// Limit displayed rooms by section (A/B)
        /// Extracted from FrmRoomManager.LimitRoomsBySection()
        /// </summary>
        public List<DataRow> LimitRoomsBySection(IEnumerable<DataRow> rows, int displayLimit, int? branchId)
        {
            var list = rows?.ToList() ?? new List<DataRow>();
            if (list.Count == 0) return list;

            if (displayLimit <= 0)
            {
                if (branchId.HasValue)
                {
                    return list
                        .GroupBy(r => NormalizeRoomNumber(SafeToString(r, "RoomNumber")))
                        .Select(g => g.First())
                        .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                        .ToList();
                }
                return list;
            }

            int limitA = (displayLimit + 1) / 2;
            int limitB = displayLimit / 2;

            if (!branchId.HasValue)
            {
                var roomsA = list
                    .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("A", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                    .Take(limitA);

                var roomsB = list
                    .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("B", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                    .Take(limitB);

                return roomsA.Concat(roomsB).ToList();
            }

            var unique = list
                .GroupBy(r => NormalizeRoomNumber(SafeToString(r, "RoomNumber")))
                .Select(g => g.First())
                .ToList();

            var roomsAStaff = unique
                .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("A", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                .ToList();

            var roomsBStaff = unique
                .Where(r => (SafeToString(r, "RoomNumber") ?? string.Empty).StartsWith("B", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => GetRoomSortKey(SafeToString(r, "RoomNumber")))
                .Take(limitB)
                .ToList();

            var selectedA = SelectRoomsForSectionA(roomsAStaff, limitA);
            return selectedA.Concat(roomsBStaff).ToList();
        }

        /// <summary>
        /// Get status color based on status name
        /// Extracted from FrmRoomManager.GetStatusColor()
        /// </summary>
        public System.Drawing.Color GetStatusColor(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
                return System.Drawing.Color.FromArgb(149, 165, 166);

            var key = NormalizeStatusKey(statusName);
            if (key.Contains("trong") || key.Contains("empty"))
                return System.Drawing.Color.FromArgb(46, 204, 113);
            if (key.Contains("dang") || key.Contains("occupied"))
                return System.Drawing.Color.FromArgb(52, 152, 219);
            if (key.Contains("bao") || key.Contains("maintenance"))
                return System.Drawing.Color.FromArgb(243, 156, 18);
            if (key.Contains("dat") || key.Contains("reserved"))
                return System.Drawing.Color.FromArgb(155, 89, 182);

            return System.Drawing.Color.FromArgb(149, 165, 166);
        }

        // Helper methods
        private static List<DataRow> SelectRoomsForSectionA(List<DataRow> roomsA, int limit)
        {
            if (roomsA == null || roomsA.Count == 0) return new List<DataRow>();
            if (limit <= 0) return roomsA;

            var byType = new Dictionary<string, List<DataRow>>();
            foreach (var r in roomsA)
            {
                var typeKey = GetRoomTypeKey(SafeToString(r, "TypeName"));
                if (!byType.TryGetValue(typeKey, out var list))
                {
                    list = new List<DataRow>();
                    byType[typeKey] = list;
                }
                list.Add(r);
            }

            var result = new List<DataRow>();
            int taken = 0;
            foreach (var pair in byType.OrderBy(p => p.Key))
            {
                foreach (var room in pair.Value.Take(Math.Max(1, limit - taken)))
                {
                    result.Add(room);
                    taken++;
                    if (taken >= limit) break;
                }
                if (taken >= limit) break;
            }

            return result;
        }

        private static string GetRoomSortKey(string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber)) return "Z999";
            var normalized = NormalizeRoomNumber(roomNumber);
            return normalized.PadRight(10, '0');
        }

        private static string NormalizeRoomNumber(string roomNumber)
        {
            return (roomNumber ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string GetRoomTypeKey(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return "other";
            var normalized = (typeName ?? "").ToLowerInvariant().Trim();
            if (normalized.Contains("Ä‘Æ¡n") || normalized.Contains("single")) return "single";
            if (normalized.Contains("Ä‘Ă´i") || normalized.Contains("double")) return "double";
            if (normalized.Contains("vip") || normalized.Contains("premium")) return "premium";
            return "other";
        }

        private static string NormalizeStatusKey(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName)) return string.Empty;
            return RemoveDiacritics(statusName.ToLowerInvariant());
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var result = new System.Text.StringBuilder();
            foreach (var c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    result.Append(c);
            }
            return result.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        private static string NormalizeAssetText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            var fixedValue = TextFixer.ForceFixUtf8Mojibake(value) ?? value;
            return fixedValue.Trim();
        }

        private static string SafeToString(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName)) return null;
            return row[columnName]?.ToString();
        }

        private static string SafeReadString(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName)) return null;
            var value = row[columnName]?.ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;
            var fixed = TextFixer.ForceFixUtf8Mojibake(value) ?? value;
            return fixed.Trim();ó
        }

        private static int TryGetInt(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName)) return 0;
            return int.TryParse(row[columnName]?.ToString(), out var result) ? result : 0;
        }

        private static decimal? TryGetDecimal(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName)) return null;
            return decimal.TryParse(row[columnName]?.ToString(), out var result) ? (decimal?)result : null;
        }
    }
}
