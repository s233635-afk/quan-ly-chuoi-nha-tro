using System;
using System.Data;
using System.Text;

namespace quan_ly_chuoi_nha_tro.GUI
{
    internal static class TextFixer
    {
        private static readonly Encoding Win1252 = Encoding.GetEncoding(1252);
        private static readonly Encoding Latin1 = Encoding.GetEncoding(28591);

        public static string FixUtf8Mojibake(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (!LooksLikeUtf8Mojibake(input)) return input;

            string candidate = TryRecode(input, Win1252);
            if (IsGoodFix(input, candidate)) return candidate;

            candidate = TryRecode(input, Latin1);
            if (IsGoodFix(input, candidate)) return candidate;

            return input;
        }

        public static string ForceFixUtf8Mojibake(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string fixedText = FixUtf8Mojibake(input);
            if (!string.Equals(fixedText, input, StringComparison.Ordinal))
                return fixedText;

            string candidate = TryRecode(input, Win1252);
            if (IsGoodFix(input, candidate)) return candidate;

            candidate = TryRecode(input, Latin1);
            if (IsGoodFix(input, candidate)) return candidate;

            return input;
        }

        public static void FixDataTable(DataTable table, params string[] columns)
        {
            if (table == null || columns == null || columns.Length == 0) return;

            foreach (DataRow row in table.Rows)
            {
                foreach (string column in columns)
                {
                    if (string.IsNullOrWhiteSpace(column)) continue;
                    if (!table.Columns.Contains(column)) continue;
                    if (row[column] == DBNull.Value) continue;

                    string raw = row[column]?.ToString();
                    string fixedValue = FixUtf8Mojibake(raw);
                    if (!string.Equals(raw, fixedValue, StringComparison.Ordinal))
                        row[column] = fixedValue;
                }
            }
        }

        public static void ForceFixDataTable(DataTable table, params string[] columns)
        {
            if (table == null || columns == null || columns.Length == 0) return;

            foreach (DataRow row in table.Rows)
            {
                foreach (string column in columns)
                {
                    if (string.IsNullOrWhiteSpace(column)) continue;
                    if (!table.Columns.Contains(column)) continue;
                    if (row[column] == DBNull.Value) continue;

                    string raw = row[column]?.ToString();
                    string fixedValue = ForceFixUtf8Mojibake(raw);
                    if (!string.Equals(raw, fixedValue, StringComparison.Ordinal))
                        row[column] = fixedValue;
                }
            }
        }

        private static bool LooksLikeUtf8Mojibake(string s)
        {
            // Heuristic: common sequences when UTF-8 bytes are decoded as Windows-1252/Latin1.
            // Avoid triggering on valid Vietnamese letters like "Â/Ă/Ê/Ô/Ơ/Ư/Đ".
            return s.IndexOf('Ã') >= 0
                || s.IndexOf('Ä') >= 0
                || s.IndexOf('Æ') >= 0
                || s.Contains("áº")
                || s.Contains("á»")
                || s.Contains("â€");
        }

        private static string TryRecode(string input, Encoding sourceEncoding)
        {
            try
            {
                byte[] bytes = sourceEncoding.GetBytes(input);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return input;
            }
        }

        private static bool IsGoodFix(string original, string candidate)
        {
            if (string.IsNullOrEmpty(candidate)) return false;
            if (string.Equals(original, candidate, StringComparison.Ordinal)) return false;
            if (candidate.IndexOf('\uFFFD') >= 0) return false;
            if (LooksLikeUtf8Mojibake(candidate)) return false;
            return Score(candidate) > Score(original);
        }

        private static int Score(string s)
        {
            int score = 0;
            foreach (char ch in s)
            {
                if (ch == '\uFFFD') score -= 10;
                else if (ch == 'Ã' || ch == 'Ä' || ch == 'Æ') score -= 3;
                else if (ch >= 0x20 && ch <= 0x7E) score += 1;
                else if (ch > 0x7E) score += 2;
            }
            return score;
        }
    }
}

