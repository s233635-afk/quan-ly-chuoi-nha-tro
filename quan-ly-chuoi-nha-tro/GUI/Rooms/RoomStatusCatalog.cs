using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace quan_ly_chuoi_nha_tro.GUI
{
    internal static class RoomStatusCatalog
    {
        public const string TrangThaiTrong = "Trống";
        public const string TrangThaiDangO = "Đang ở";
        public const string TrangThaiDaCoc = "Đã cọc";
        public const string TrangThaiBaoTri = "Bảo trì";
        public const string TrangThaiVeSinh = "Vệ sinh";

        private static readonly IReadOnlyList<string> CanonicalNames = new[]
        {
            TrangThaiTrong,
            TrangThaiDangO,
            TrangThaiDaCoc,
            TrangThaiBaoTri,
            TrangThaiVeSinh
        };

        public static string Canonicalize(string input)
        {
            var fixedInput = TextFixer.ForceFixUtf8Mojibake(input)?.Trim();
            if (string.IsNullOrEmpty(fixedInput)) return fixedInput;

            var key = RemoveDiacritics(fixedInput).ToLowerInvariant();

            if (key.Contains("trong")) return TrangThaiTrong;
            if (key.Contains("dang o") || key.Contains("dang thue") || key.Contains("dang?") || key.Contains("dang")) return TrangThaiDangO;
            if (key.Contains("da coc") || key.Contains("coc")) return TrangThaiDaCoc;
            if (key.Contains("bao tri") || key.Contains("bao tri")) return TrangThaiBaoTri;
            if (key.Contains("ve sinh") || key.Contains("vesinh")) return TrangThaiVeSinh;

            foreach (var canonical in CanonicalNames)
            {
                if (string.Equals(RemoveDiacritics(canonical), RemoveDiacritics(fixedInput), StringComparison.OrdinalIgnoreCase))
                    return canonical;
            }

            return fixedInput;
        }

        public static void CanonicalizeColumn(DataTable table, string columnName)
        {
            if (table == null || string.IsNullOrWhiteSpace(columnName) || !table.Columns.Contains(columnName)) return;
            foreach (DataRow row in table.Rows)
            {
                var current = row[columnName]?.ToString();
                row[columnName] = Canonicalize(current);
            }
        }

        public static void EnsureCanonicalSet(DataTable statuses)
        {
            if (statuses == null) return;
            if (!statuses.Columns.Contains("StatusId"))
                statuses.Columns.Add("StatusId", typeof(int));
            if (!statuses.Columns.Contains("StatusName"))
                statuses.Columns.Add("StatusName", typeof(string));

            var existing = new HashSet<string>(statuses.AsEnumerable()
                .Select(r => Canonicalize(r["StatusName"]?.ToString()))
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            int maxId = statuses.AsEnumerable()
                .Select(r => r["StatusId"])
                .Where(v => v != null && v != DBNull.Value)
                .Select(v => Convert.ToInt32(v))
                .DefaultIfEmpty(0)
                .Max();

            foreach (var canonical in CanonicalNames)
            {
                if (existing.Contains(canonical)) continue;
                var row = statuses.NewRow();
                row["StatusId"] = ++maxId;
                row["StatusName"] = canonical;
                statuses.Rows.Add(row);
            }
        }

        private static string RemoveDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
