using System;
using System.Data;
using System.IO;
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
                    ExportGridToCsv(grid, dialog.FileName);
                    ToastNotification.Success($"Đã xuất {grid.Rows.Count} dòng ra file Excel");
                    
                    // Ask to open file
                    if (ModernConfirmDialog.Confirm("Mở file vừa xuất?"))
                    {
                        System.Diagnostics.Process.Start(dialog.FileName);
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
                    ExportTableToCsv(table, dialog.FileName);
                    ToastNotification.Success($"Đã xuất {table.Rows.Count} dòng ra file Excel");

                    if (ModernConfirmDialog.Confirm("Mở file vừa xuất?"))
                    {
                        System.Diagnostics.Process.Start(dialog.FileName);
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
