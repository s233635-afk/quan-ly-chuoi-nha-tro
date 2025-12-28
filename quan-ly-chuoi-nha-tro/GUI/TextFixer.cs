using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace quan_ly_chuoi_nha_tro.GUI
{
    internal static class TextFixer
    {
        private static readonly Encoding Win1252 = Encoding.GetEncoding(1252);
        private static readonly Encoding Latin1 = Encoding.GetEncoding(28591);
        private static readonly string[] MojibakeMarkers = { "Ã", "Â", "Ä", "Å", "Ê", "Ô", "Ð", "ø", "æ", "¤", "¢" };

        public static string FixUtf8Mojibake(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (!LooksLikeMojibake(input))
                return FixLossyVietnamese(input);

            byte[] bytes = Encoding.Default.GetBytes(input);
            var fixedValue = Encoding.UTF8.GetString(bytes);
            return FixLossyVietnamese(fixedValue);
        }

        private static string ReplaceIgnoreCase(string source, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue)) return source;
            var comparison = StringComparison.OrdinalIgnoreCase;
            int index = source.IndexOf(oldValue, comparison);
            if (index < 0) return source;

            var builder = new StringBuilder(source.Length);
            int lastIndex = 0;

            while (index >= 0)
            {
                builder.Append(source, lastIndex, index - lastIndex);
                if (!string.IsNullOrEmpty(newValue))
                    builder.Append(newValue);
                lastIndex = index + oldValue.Length;
                index = source.IndexOf(oldValue, lastIndex, comparison);
            }

            builder.Append(source, lastIndex, source.Length - lastIndex);
            return builder.ToString();
        }

        private static readonly (string broken, string corrected)[] LossyVietnameseMap = new[]
        {
            ("Chi nhánh", "Chi nhánh"), ("Chi nhanh", "Chi nhánh"), ("Chi Nh�nh", "Chi nhánh"), ("Nhánh", "nhánh"),
            ("C?n Th?", "Cần Thơ"), ("Can Tho", "Cần Thơ"), ("C?n Th�", "Cần Thơ"),
            ("Ninh Kiều", "Ninh Kiều"), ("Ninh Kieu", "Ninh Kiều"), ("Ninh Kiều", "Ninh Kiều"),
            ("Dãy", "Dãy"),
            ("Phòng", "Phòng"), ("Phong", "Phòng"),
            ("đơn", "đơn"), ("Don", "Đơn"),
            ("đôi", "đôi"), ("Doi", "Đôi"),
            ("cao cấp", "cao cấp"), ("cao cấp", "cao cấp"),
            ("c?p", "cấp"),
            ("Bảo trì", "Bảo trì"), ("Bảo tri", "Bảo trì"),
            ("?? c?c", "Đã cọc"), ("Đ? c?c", "Đã cọc"), ("Đã coc", "Đã cọc"),
            ("Dang ?", "Đang ở"), ("Đang ?", "Đang ở"), ("Đang ở", "Đang ở"),
            ("Vệ sinh", "Vệ sinh"), ("Ve sinh", "Vệ sinh"), ("Vệ sinh", "Vệ sinh"),
            ("Khách", "Khách"),
            ("Máy", "Máy"), ("May", "Máy"),
            ("Máy lạnh", "Máy lạnh"), ("Máy lạnh", "Máy lạnh"),
            ("l?nh", "lạnh"), ("lanh", "lạnh"),
            ("Nguyẽnn", "Nguyễn"),
            ("Nhật", "Nhật"),
            ("Kiên", "Kiên"), ("Kiên", "Kiên"),
            ("Kiên", "Kiên"),
            ("Số người", "Số người"), ("So nguoi", "Số người"),
            ("Tự Do", "Tự do")
        };

        private static string FixLossyVietnamese(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string output = input;
            foreach (var (broken, corrected) in LossyVietnameseMap)
            {
                output = ReplaceIgnoreCase(output, broken, corrected);
            }

            var trimmed = output.Trim();
            if (string.Equals(trimmed, "Tr?ng", StringComparison.OrdinalIgnoreCase) || string.Equals(trimmed, "Trong", StringComparison.OrdinalIgnoreCase))
                output = output.Replace(trimmed, "Trống");
            return output;
        }

        public static string ForceFixUtf8Mojibake(string input, int maxIterations = 2)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string original = input;
            string current = original;

            if (LooksLikeMojibake(input))
            {
                int originalQuestions = CountChar(original, '?');
                for (int i = 0; i < Math.Max(1, maxIterations); i++)
                {
                    string next = FixUtf8Mojibake(current);
                    if (string.Equals(current, next, StringComparison.Ordinal))
                        break;
                    current = next;
                }

                if (originalQuestions == 0 && CountChar(current, '?') > 0)
                    current = original;
            }

            return FixLossyVietnamese(current);
        }

        public static string FixCurrencyString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            input = input.Trim();

            // Replace Vietnamese unit suffixes
            input = ReplaceIgnoreCase(input, "đ", string.Empty);
            input = ReplaceIgnoreCase(input, "vnd", string.Empty);
            input = ReplaceIgnoreCase(input, "vnđ", string.Empty);
            input = input.Replace("₫", string.Empty);

            // Remove spaces
            input = input.Replace(" ", "");

            // If contains both dot and comma, assume dot is thousand separator and comma is decimal
            if (input.Contains(".") && input.Contains(","))
            {
                input = input.Replace(".", "");
                input = input.Replace(",", ".");
            }
            else if (input.Count(ch => ch == '.') > 1 && !input.Contains(","))
            // If there are multiple dots but no comma, treat dot as thousand separator
            {
                input = input.Replace(".", "");
            }
            else
            {
                // Replace comma with dot for decimal
                input = input.Replace(",", ".");
            }

            // Remove any character that's not digit or dot
            var sanitized = new string(input.Where(ch => char.IsDigit(ch) || ch == '.').ToArray());

            if (string.IsNullOrWhiteSpace(sanitized))
                return null;

            if (decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                return value.ToString("0.##", CultureInfo.InvariantCulture);
            }

            return null;
        }

        public static decimal? ParseCurrency(string input)
        {
            var fixedString = FixCurrencyString(input);
            if (fixedString == null)
                return null;

            if (decimal.TryParse(fixedString, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return null;
        }

        public static void FixDataTable(DataTable table, params string[] columnNames)
        {
            if (table == null || columnNames == null || columnNames.Length == 0) return;

            foreach (DataRow row in table.Rows)
            {
                foreach (var col in columnNames)
                {
                    if (!table.Columns.Contains(col)) continue;
                    var value = row[col]?.ToString();
                    row[col] = FixUtf8Mojibake(value);
                }
            }
        }

        public static void ForceFixDataTable(DataTable table, params string[] columnNames)
        {
            if (table == null || columnNames == null || columnNames.Length == 0) return;

            foreach (DataRow row in table.Rows)
            {
                foreach (var col in columnNames)
                {
                    if (!table.Columns.Contains(col)) continue;
                    var value = row[col]?.ToString();
                    var fixedValue = ForceFixUtf8Mojibake(value);
                    row[col] = fixedValue ?? value;
                }
            }
        }

        public static string ReadCurrency(DataRow row, string col)
        {
            if (row == null || !row.Table.Columns.Contains(col)) return null;
            var value = row[col]?.ToString();
            return FixCurrencyString(value);
        }

        public static decimal ReadDecimal(DataRow row, string col)
        {
            if (row == null || !row.Table.Columns.Contains(col)) return 0m;
            var value = row[col];
            if (value == null || value == DBNull.Value) return 0m;

            switch (value)
            {
                case decimal d:
                    return d;
                case double db:
                    return Convert.ToDecimal(db);
                case float f:
                    return Convert.ToDecimal(f);
                case int i:
                    return i;
                case long l:
                    return l;
                case short s:
                    return s;
            }

            var text = value.ToString();
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                return parsed;

            // Normalize thousand separators (allow dot or comma) before final parse attempt
            var sanitized = FixCurrencyString(text) ?? StripCurrencySymbols(text)?.Replace(",", "")?.Replace(".", "");
            if (!string.IsNullOrEmpty(sanitized) && decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
                return parsed;

            return 0m;
        }

        public static string StripCurrencySymbols(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            var sb = new StringBuilder(input.Length);
            foreach (var ch in input)
            {
                if (char.IsDigit(ch) || ch == '.' || ch == ',' || ch == '-' || ch == ' ')
                    sb.Append(ch);
            }
            return sb.ToString();
        }

        private static bool LooksLikeMojibake(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (input.IndexOf('?') >= 0) return true;
            if (input.IndexOf('\uFFFD') >= 0) return true;
            foreach (var marker in MojibakeMarkers)
            {
                if (input.IndexOf(marker, StringComparison.Ordinal) >= 0)
                    return true;
            }
            return false;
        }

        private static int CountChar(string text, char ch)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int count = 0;
            foreach (var c in text)
            {
                if (c == ch) count++;
            }
            return count;
        }
    }
}
