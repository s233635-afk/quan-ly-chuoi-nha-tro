using System;
using System.Data;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Export DataGridView or DataTable to Excel/CSV
    /// Uses CSV format for maximum compatibility (no external dependencies)
    /// </summary>
    public static class ExcelExporter
    {
        /// <summary>
        /// Export DataTable to Excel-compatible HTML file (XLS extension) with basic formatting.
        /// </summary>
        public static bool ExportToExcelHtml(DataTable table, string title = null)
        {
            if (table == null || table.Rows.Count == 0)
            {
                ToastNotification.Warning("Không có dữ liệu để xuất");
                return false;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel Files (*.xls)|*.xls|All Files (*.*)|*.*";
                dialog.Title = "Xuất dữ liệu ra Excel";
                string safeTitle = MakeSafeFileName(string.IsNullOrWhiteSpace(title) ? "Export" : title);
                dialog.FileName = $"{safeTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.xls";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return false;

                try
                {
                    var filePath = EnsureSafeExportPath(dialog.FileName, "xls");
                    ExportTableToExcelHtml(table, filePath, title);
                    ToastNotification.Success($"Đã xuất {table.Rows.Count} dòng ra file Excel");

                    if (ModernConfirmDialog.Confirm("Mở file vừa xuất?"))
                    {
                        System.Diagnostics.Process.Start(filePath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    ToastNotification.Error($"Lỗi xuất file: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Export DataGridView to Excel (CSV format)
        /// </summary>
        public static bool ExportToExcel(DataGridView grid, string title = null)
        {
            if (grid == null || grid.Rows.Count == 0)
            {
                ToastNotification.Warning("Không có dữ liệu để xuất");
                return false;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel Files (*.csv)|*.csv|All Files (*.*)|*.*";
                dialog.Title = "Xuất dữ liệu ra Excel";
                dialog.FileName = $"{title ?? "Export"}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return false;

                try
                {
                    var filePath = EnsureSafeExportPath(dialog.FileName, "csv");
                    ExportGridToCsv(grid, filePath);
                    ToastNotification.Success($"Đã xuất {grid.Rows.Count} dòng ra file Excel");
                    
                    // Ask to open file
                    if (ModernConfirmDialog.Confirm("Mở file vừa xuất?"))
                    {
                        System.Diagnostics.Process.Start(filePath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    ToastNotification.Error($"Lỗi xuất file: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Export DataTable to Excel (CSV format)
        /// </summary>
        public static bool ExportToExcel(DataTable table, string title = null)
        {
            if (table == null || table.Rows.Count == 0)
            {
                ToastNotification.Warning("Không có dữ liệu để xuất");
                return false;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel Files (*.csv)|*.csv|All Files (*.*)|*.*";
                dialog.Title = "Xuất dữ liệu ra Excel";
                dialog.FileName = $"{title ?? table.TableName ?? "Export"}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return false;

                try
                {
                    var filePath = EnsureSafeExportPath(dialog.FileName, "csv");
                    ExportTableToCsv(table, filePath);
                    ToastNotification.Success($"Đã xuất {table.Rows.Count} dòng ra file Excel");

                    if (ModernConfirmDialog.Confirm("Mở file vừa xuất?"))
                    {
                        System.Diagnostics.Process.Start(filePath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    ToastNotification.Error($"Lỗi xuất file: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Export to CSV file silently (no dialogs)
        /// </summary>
        public static void ExportGridToCsv(DataGridView grid, string filePath)
        {
            var sb = new StringBuilder();

            // Headers
            var headers = new string[grid.Columns.Count];
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                if (grid.Columns[i].Visible)
                    headers[i] = EscapeCsvField(grid.Columns[i].HeaderText);
            }
            sb.AppendLine(string.Join(",", headers));

            // Data rows
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;

                var values = new string[grid.Columns.Count];
                for (int i = 0; i < grid.Columns.Count; i++)
                {
                    if (grid.Columns[i].Visible)
                    {
                        var value = row.Cells[i].Value?.ToString() ?? "";
                        values[i] = EscapeCsvField(value);
                    }
                }
                sb.AppendLine(string.Join(",", values));
            }

            // Write with UTF-8 BOM for Excel compatibility
            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        /// <summary>
        /// Export DataTable to CSV file silently
        /// </summary>
        public static void ExportTableToCsv(DataTable table, string filePath)
        {
            var sb = new StringBuilder();

            // Headers
            var headers = new string[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                headers[i] = EscapeCsvField(table.Columns[i].ColumnName);
            }
            sb.AppendLine(string.Join(",", headers));

            // Data rows
            foreach (DataRow row in table.Rows)
            {
                var values = new string[table.Columns.Count];
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    var value = row[i]?.ToString() ?? "";
                    values[i] = EscapeCsvField(value);
                }
                sb.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        /// <summary>
        /// Export DataTable to Excel-compatible HTML table (XLS extension).
        /// </summary>
        private static void ExportTableToExcelHtml(DataTable table, string filePath, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:'Segoe UI',Arial,sans-serif;font-size:10pt;color:#222;}");
            sb.AppendLine("h1{font-size:13pt;margin:0 0 10px 0;color:#0c63a6;}");
            sb.AppendLine("table{border-collapse:collapse;}");
            sb.AppendLine("th{background:#0c63a6;color:#fff;font-weight:bold;padding:8px 14px;border:1px solid #0b5a96;}");
            sb.AppendLine("td{padding:8px 14px;border:1px solid #d7e0ea;white-space:nowrap;}");
            sb.AppendLine("tr:nth-child(even) td{background:#f7fbff;}");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            if (!string.IsNullOrWhiteSpace(title))
                sb.AppendLine($"<h1>{EscapeHtml(title)}</h1>");
            sb.AppendLine("<table>");

            sb.AppendLine("<tr>");
            foreach (DataColumn col in table.Columns)
            {
                sb.AppendLine($"<th>{EscapeHtml(col.ColumnName)}</th>");
            }
            sb.AppendLine("</tr>");

            foreach (DataRow row in table.Rows)
            {
                sb.AppendLine("<tr>");
                foreach (DataColumn col in table.Columns)
                {
                    bool forceText = IsTextColumn(col.ColumnName);
                    string value = forceText ? NormalizeIdValue(row[col]) : (row[col] == null || row[col] == DBNull.Value ? string.Empty : row[col].ToString());
                    string style = forceText ? " style=\"mso-number-format:'\\@';\"" : string.Empty;
                    sb.AppendLine($"<td{style}>{EscapeHtml(value)}</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        private static string EscapeHtml(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;")
                        .Replace("'", "&#39;");
        }

        private static string MakeSafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Export";
            var invalids = Path.GetInvalidFileNameChars();
            var sanitized = new string(name.Select(ch => invalids.Contains(ch) ? '_' : ch).ToArray());
            return string.IsNullOrWhiteSpace(sanitized) ? "Export" : sanitized;
        }

        private static bool IsTextColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName)) return false;
            return columnName.IndexOf("CCCD", StringComparison.OrdinalIgnoreCase) >= 0
                || columnName.IndexOf("CMND", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string NormalizeIdValue(object value)
        {
            if (value == null || value == DBNull.Value) return string.Empty;

            if (value is long longValue) return longValue.ToString(CultureInfo.InvariantCulture);
            if (value is int intValue) return intValue.ToString(CultureInfo.InvariantCulture);
            if (value is decimal decimalValue) return decimalValue.ToString("0", CultureInfo.InvariantCulture);
            if (value is double doubleValue) return doubleValue.ToString("0", CultureInfo.InvariantCulture);
            if (value is float floatValue) return floatValue.ToString("0", CultureInfo.InvariantCulture);

            return value.ToString();
        }

        private static string EnsureSafeExportPath(string filePath, string defaultExtension)
        {
            var directory = Path.GetDirectoryName(filePath);
            var extension = Path.GetExtension(filePath);
            var name = Path.GetFileNameWithoutExtension(filePath);

            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = "." + (defaultExtension ?? "xls").TrimStart('.');
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Export";
            }

            var safeName = MakeSafeFileName(name);

            if (string.IsNullOrWhiteSpace(directory))
            {
                return safeName + extension;
            }

            return Path.Combine(directory, safeName + extension);
        }

        /// <summary>
        /// Copy DataGridView to clipboard (for paste into Excel)
        /// </summary>
        public static void CopyToClipboard(DataGridView grid)
        {
            if (grid == null || grid.Rows.Count == 0) return;

            grid.SelectAll();
            var dataObj = grid.GetClipboardContent();
            if (dataObj != null)
            {
                Clipboard.SetDataObject(dataObj);
                ToastNotification.Info("Đã copy vào clipboard. Có thể paste vào Excel.");
            }
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            // Escape quotes and wrap in quotes if needed
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }
    }
}
