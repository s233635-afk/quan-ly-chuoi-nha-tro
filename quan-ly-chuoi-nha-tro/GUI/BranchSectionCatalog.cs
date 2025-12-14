using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace quan_ly_chuoi_nha_tro.GUI
{
    internal static class BranchSectionCatalog
    {
        public static DataTable NormalizeForRoomEditor(DataTable raw, int? branchId)
        {
            if (raw == null) return null;
            if (!raw.Columns.Contains("SectionId")) return raw;

            TextFixer.FixDataTable(raw, "SectionCode", "SectionName", "Description");

            DataTable dt;
            if (branchId.HasValue && raw.Columns.Contains("BranchId"))
            {
                dt = raw.Clone();
                foreach (DataRow r in raw.Rows)
                {
                    if (!int.TryParse(r["BranchId"]?.ToString(), out var bid)) continue;
                    if (bid != branchId.Value) continue;
                    dt.ImportRow(r);
                }
            }
            else
            {
                dt = raw.Copy();
            }

            if (!dt.Columns.Contains("SectionDisplay"))
                dt.Columns.Add("SectionDisplay", typeof(string));

            foreach (DataRow r in dt.Rows)
            {
                string code = dt.Columns.Contains("SectionCode") ? r["SectionCode"]?.ToString() : null;
                string name = dt.Columns.Contains("SectionName") ? r["SectionName"]?.ToString() : null;
                string normalizedName = CanonicalizeName(code, name);
                string display = BuildDisplay(code, normalizedName, r["SectionId"]?.ToString());
                r["SectionName"] = normalizedName ?? r["SectionName"];
                r["SectionDisplay"] = display;
            }

            // De-dup by SectionCode when it's A/B/C (your DB final uses these).
            if (dt.Columns.Contains("SectionCode"))
            {
                var abc = dt.AsEnumerable()
                    .Where(r =>
                    {
                        var c = (r["SectionCode"]?.ToString() ?? string.Empty).Trim();
                        return c.Equals("A", StringComparison.OrdinalIgnoreCase)
                            || c.Equals("B", StringComparison.OrdinalIgnoreCase)
                            || c.Equals("C", StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();

                if (abc.Count > 0)
                {
                    var dedup = dt.Clone();
                    foreach (var grp in abc.GroupBy(r => (r["SectionCode"]?.ToString() ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase))
                    {
                        // Prefer exact "Dãy X" name.
                        DataRow best = grp.FirstOrDefault(r =>
                        {
                            string c = grp.Key.ToUpperInvariant();
                            string n = r["SectionName"]?.ToString() ?? string.Empty;
                            return string.Equals(RemoveDiacritics(n).ToLowerInvariant(), ("day " + c.ToLowerInvariant()), StringComparison.Ordinal);
                        }) ?? grp.First();

                        dedup.ImportRow(best);
                    }

                    dt = dedup;
                }
            }

            return dt;
        }

        private static string BuildDisplay(string code, string name, string id)
        {
            string display = $"{code} - {name}".Trim(' ', '-');
            if (string.IsNullOrWhiteSpace(display))
                display = "Khu " + id;
            return display;
        }

        private static string CanonicalizeName(string code, string name)
        {
            string fixedName = TextFixer.FixUtf8Mojibake(name)?.Trim();
            string fixedCode = TextFixer.FixUtf8Mojibake(code)?.Trim();

            if (string.IsNullOrWhiteSpace(fixedName) && string.IsNullOrWhiteSpace(fixedCode)) return fixedName;

            string c = (fixedCode ?? string.Empty).Trim();
            if (c.Length == 1)
            {
                string upper = c.ToUpperInvariant();
                if (upper == "A" || upper == "B" || upper == "C")
                    return "Dãy " + upper;
            }

            // If name already starts with "Dãy X ..." => cut to "Dãy X"
            string key = RemoveDiacritics(fixedName ?? string.Empty).ToLowerInvariant();
            if (key.StartsWith("day a")) return "Dãy A";
            if (key.StartsWith("day b")) return "Dãy B";
            if (key.StartsWith("day c")) return "Dãy C";

            return fixedName;
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
