using System;
using System.Collections.Generic;
using System.IO;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Error logging utility with file rotation
    /// </summary>
    public static class ErrorLogger
    {
        private static readonly object _lock = new object();
        private static string _logDirectory;
        private static int _maxFileSizeMB = 5;
        private static int _maxFiles = 10;

        /// <summary>
        /// Initialize the logger
        /// </summary>
        public static void Initialize(string logDirectory = null, int maxFileSizeMB = 5, int maxFiles = 10)
        {
            _logDirectory = logDirectory ?? GetDefaultLogDirectory();
            _maxFileSizeMB = maxFileSizeMB;
            _maxFiles = maxFiles;

            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        /// <summary>
        /// Log an error
        /// </summary>
        public static void LogError(Exception ex, string context = null)
        {
            Log("ERROR", FormatException(ex, context));
        }

        /// <summary>
        /// Log a warning
        /// </summary>
        public static void LogWarning(string message, string context = null)
        {
            Log("WARN", FormatMessage(message, context));
        }

        /// <summary>
        /// Log info
        /// </summary>
        public static void LogInfo(string message, string context = null)
        {
            Log("INFO", FormatMessage(message, context));
        }

        /// <summary>
        /// Log debug info
        /// </summary>
        public static void LogDebug(string message, string context = null)
        {
            #if DEBUG
            Log("DEBUG", FormatMessage(message, context));
            #endif
        }

        /// <summary>
        /// Handle and log an exception, show user-friendly message
        /// </summary>
        public static void HandleException(Exception ex, string context = null)
        {
            LogError(ex, context);

            var message = GetUserFriendlyMessage(ex);
            ToastNotification.Error(message);
        }

        /// <summary>
        /// Handle exception with custom user message
        /// </summary>
        public static void HandleException(Exception ex, string context, string userMessage)
        {
            LogError(ex, context);
            ToastNotification.Error(userMessage);
        }

        private static void Log(string level, string message)
        {
            if (string.IsNullOrEmpty(_logDirectory))
                Initialize();

            lock (_lock)
            {
                try
                {
                    var logFile = GetCurrentLogFile();
                    RotateIfNeeded(logFile);

                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                    File.AppendAllText(logFile, logEntry + Environment.NewLine);
                }
                catch { /* Ignore logging errors */ }
            }
        }

        private static string GetCurrentLogFile()
        {
            return Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
        }

        private static void RotateIfNeeded(string logFile)
        {
            try
            {
                if (File.Exists(logFile))
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.Length > _maxFileSizeMB * 1024 * 1024)
                    {
                        var newName = Path.Combine(_logDirectory, 
                            $"app_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                        File.Move(logFile, newName);
                    }
                }

                // Clean old files
                CleanOldFiles();
            }
            catch { /* Ignore */ }
        }

        private static void CleanOldFiles()
        {
            try
            {
                var files = Directory.GetFiles(_logDirectory, "*.log");
                if (files.Length > _maxFiles)
                {
                    Array.Sort(files);
                    for (int i = 0; i < files.Length - _maxFiles; i++)
                    {
                        File.Delete(files[i]);
                    }
                }
            }
            catch { /* Ignore */ }
        }

        private static string FormatException(Exception ex, string context)
        {
            var lines = new List<string>();
            if (!string.IsNullOrEmpty(context))
                lines.Add($"Context: {context}");
            lines.Add($"Exception: {ex.GetType().Name}");
            lines.Add($"Message: {ex.Message}");
            if (ex.InnerException != null)
                lines.Add($"Inner: {ex.InnerException.Message}");
            lines.Add($"Stack: {ex.StackTrace}");
            return string.Join(" | ", lines);
        }

        private static string FormatMessage(string message, string context)
        {
            if (!string.IsNullOrEmpty(context))
                return $"[{context}] {message}";
            return message;
        }

        private static string GetUserFriendlyMessage(Exception ex)
        {
            // SQL errors
            if (ex.GetType().Name.Contains("Sql"))
                return "Lỗi kết nối cơ sở dữ liệu. Vui lòng thử lại.";

            // Timeout
            if (ex is TimeoutException)
                return "Yêu cầu quá thời gian chờ. Vui lòng thử lại.";

            // IO errors
            if (ex is IOException)
                return "Lỗi đọc/ghi file. Kiểm tra quyền truy cập.";

            // Argument errors
            if (ex is ArgumentException)
                return "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";

            // Generic
            return "Đã xảy ra lỗi. Vui lòng liên hệ quản trị viên.";
        }

        private static string GetDefaultLogDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "QuanLyChuoiNhaTro", "Logs");
        }

        /// <summary>
        /// Get recent log entries
        /// </summary>
        public static List<string> GetRecentLogs(int lines = 50)
        {
            var result = new List<string>();
            try
            {
                var logFile = GetCurrentLogFile();
                if (File.Exists(logFile))
                {
                    var allLines = File.ReadAllLines(logFile);
                    var start = Math.Max(0, allLines.Length - lines);
                    for (int i = start; i < allLines.Length; i++)
                    {
                        result.Add(allLines[i]);
                    }
                }
            }
            catch { /* Ignore */ }
            return result;
        }

        /// <summary>
        /// Open log directory in explorer
        /// </summary>
        public static void OpenLogDirectory()
        {
            if (Directory.Exists(_logDirectory))
            {
                System.Diagnostics.Process.Start("explorer.exe", _logDirectory);
            }
        }
    }
}
