using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace quan_ly_chuoi_nha_tro.GUI
{
    internal static class AdminBranchScope
    {
        private static readonly Lazy<HashSet<string>> AllowedCodesLazy = new Lazy<HashSet<string>>(LoadAllowedCodes);

        public static bool IsEnabled => AllowedCodesLazy.Value.Count > 0;

        public static HashSet<string> AllowedCodes => AllowedCodesLazy.Value;

        public static HashSet<int> GetAllowedBranchIds(DataTable scopedBranches)
        {
            var ids = new HashSet<int>();
            if (scopedBranches == null) return ids;
            if (!scopedBranches.Columns.Contains("BranchId")) return ids;

            foreach (DataRow r in scopedBranches.Rows)
            {
                if (int.TryParse(r["BranchId"]?.ToString(), out var id) && id > 0)
                    ids.Add(id);
            }

            return ids;
        }

        public static DataTable FilterByBranchIds(DataTable table, HashSet<int> allowedBranchIds, string branchIdColumn = "BranchId")
        {
            if (table == null) return table;
            if (allowedBranchIds == null || allowedBranchIds.Count == 0) return table;
            if (string.IsNullOrWhiteSpace(branchIdColumn) || !table.Columns.Contains(branchIdColumn)) return table;

            var filtered = table.Clone();
            foreach (DataRow r in table.Rows)
            {
                if (!int.TryParse(r[branchIdColumn]?.ToString(), out var bid)) continue;
                if (!allowedBranchIds.Contains(bid)) continue;
                filtered.ImportRow(r);
            }
            return filtered;
        }

        public static DataTable Apply(DataTable branches)
        {
            if (!IsEnabled || branches == null) return branches;
            if (!branches.Columns.Contains("BranchCode")) return branches;

            var filtered = branches.Clone();
            foreach (DataRow r in branches.Rows)
            {
                string code = r["BranchCode"]?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(code)) continue;
                if (!AllowedCodes.Contains(code)) continue;
                filtered.ImportRow(r);
            }

            // Nếu cấu hình không khớp DB hiện tại, fallback về 2 chi nhánh Cần Thơ theo tên,
            // nếu vẫn không tìm thấy thì lấy 2 chi nhánh đầu tiên (đang hoạt động) để tránh Tổng quan = 0.
            if (filtered.Rows.Count == 0)
                return FallbackPickTwoBranches(branches);

            return filtered;
        }

        private static DataTable FallbackPickTwoBranches(DataTable branches)
        {
            if (branches == null) return branches;
            if (!branches.Columns.Contains("BranchId")) return branches;

            var rows = branches.AsEnumerable();

            // Prefer active branches if possible
            if (branches.Columns.Contains("IsActive"))
            {
                rows = rows.Where(r =>
                {
                    var v = r["IsActive"];
                    if (v == null || v == DBNull.Value) return false;
                    try { return Convert.ToBoolean(v); } catch { return false; }
                });
            }

            var canTho = rows.Where(r =>
            {
                string name = null;
                if (branches.Columns.Contains("BranchName"))
                    name = r["BranchName"]?.ToString();

                name = TextFixer.FixUtf8Mojibake(name);
                string key = RemoveDiacritics(name ?? string.Empty).ToLowerInvariant();
                return key.Contains("can tho");
            }).ToList();

            IEnumerable<DataRow> chosen = canTho.Count > 0 ? canTho : rows.ToList();

            chosen = chosen
                .OrderBy(r =>
                {
                    if (int.TryParse(r["BranchId"]?.ToString(), out var id)) return id;
                    return int.MaxValue;
                })
                .Take(2);

            var dt = branches.Clone();
            foreach (var r in chosen)
                dt.ImportRow(r);
            return dt;
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

        private static HashSet<string> LoadAllowedCodes()
        {
            string raw = null;
            try { raw = ConfigurationManager.AppSettings["AdminAllowedBranchCodes"]; } catch { }

            // Missing key => default to 2 branches Cần Thơ per yêu cầu.
            if (raw == null)
                raw = "CT01,CT02";

            // Present-but-empty => disable scope (show all).
            if (string.IsNullOrWhiteSpace(raw))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var codes = raw
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return new HashSet<string>(codes, StringComparer.OrdinalIgnoreCase);
        }
    }
}
