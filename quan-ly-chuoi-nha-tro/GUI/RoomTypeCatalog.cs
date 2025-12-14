using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace quan_ly_chuoi_nha_tro.GUI
{
    internal static class RoomTypeCatalog
    {
        public const string PhongDon = "Phòng đơn";
        public const string PhongDoi = "Phòng đôi";
        public const string PhongCaoCap = "Phòng cao cấp";

        private static readonly string[] CanonicalNames = { PhongDon, PhongDoi, PhongCaoCap };

        public static string Canonicalize(string input)
        {
            string s = TextFixer.FixUtf8Mojibake(input)?.Trim();
            if (string.IsNullOrWhiteSpace(s)) return s;

            string key = RemoveDiacritics(s).ToLowerInvariant();

            // match by meaning; allow suffix like "- 15m2"
            if (key.Contains("phong cao cap")) return PhongCaoCap;
            if (key.Contains("phong doi")) return PhongDoi;
            if (key.Contains("phong don")) return PhongDon;

            // common broken cases after mojibake fix missing "đ" => "phong on/oi"
            if (key.Contains("phong on")) return PhongDon;
            if (key.Contains("phong oi")) return PhongDoi;

            // sometimes name is only "đơn/đôi/cao cấp"
            if (key.Contains("cao cap")) return PhongCaoCap;
            if (key.Contains("doi")) return PhongDoi;
            if (key.Contains("don")) return PhongDon;

            // fallback: if already equals one of canonical ignoring accents
            foreach (var canonical in CanonicalNames)
            {
                if (string.Equals(RemoveDiacritics(canonical), RemoveDiacritics(s), StringComparison.OrdinalIgnoreCase))
                    return canonical;
            }

            return s;
        }

        public static DataTable FilterToCanonicalTypes(DataTable rawTypes)
        {
            if (rawTypes == null) return null;
            if (!rawTypes.Columns.Contains("RoomTypeId") || !rawTypes.Columns.Contains("RoomTypeName"))
                return rawTypes;

            // Fix encoding in-place first
            TextFixer.FixDataTable(rawTypes, "RoomTypeName", "Amenities", "Description");

            var result = rawTypes.Clone();

            foreach (var canonical in CanonicalNames)
            {
                DataRow best = rawTypes.AsEnumerable()
                    .FirstOrDefault(r => string.Equals(Canonicalize(r["RoomTypeName"]?.ToString()), canonical, StringComparison.OrdinalIgnoreCase));

                if (best == null) continue;
                var row = result.NewRow();
                foreach (DataColumn c in result.Columns)
                {
                    row[c.ColumnName] = best[c.ColumnName];
                }
                row["RoomTypeName"] = canonical;
                result.Rows.Add(row);
            }

            // If nothing matched (unexpected DB), fall back to original.
            return result.Rows.Count > 0 ? result : rawTypes;
        }

        public static void CanonicalizeRoomTypeColumn(DataTable rooms, string columnName = "RoomTypeName")
        {
            if (rooms == null || string.IsNullOrWhiteSpace(columnName)) return;
            if (!rooms.Columns.Contains(columnName)) return;

            foreach (DataRow r in rooms.Rows)
            {
                if (r[columnName] == DBNull.Value) continue;
                string raw = r[columnName]?.ToString();
                string fixedValue = Canonicalize(raw);
                if (!string.Equals(raw, fixedValue, StringComparison.Ordinal))
                    r[columnName] = fixedValue;
            }
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);

            foreach (char ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
