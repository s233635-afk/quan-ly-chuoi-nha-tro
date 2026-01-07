using System;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Modern dialog helpers to replace MessageBox.Show
    /// This class provides a drop-in replacement for common MessageBox patterns
    /// </summary>
    public static class ModernDialog
    {
        /// <summary>
        /// Show success message as toast
        /// </summary>
        public static void Success(string message, string title = "Thành công")
        {
            ToastNotification.Success(message, title);
        }

        /// <summary>
        /// Show error message as toast
        /// </summary>
        public static void Error(string message, string title = "Lỗi")
        {
            ToastNotification.Error(message, title);
        }

        /// <summary>
        /// Show warning message as toast
        /// </summary>
        public static void Warning(string message, string title = "Cảnh báo")
        {
            ToastNotification.Warning(message, title);
        }

        /// <summary>
        /// Show info message as toast
        /// </summary>
        public static void Info(string message, string title = "Thông báo")
        {
            ToastNotification.Info(message, title);
        }

        /// <summary>
        /// Confirm action (Yes/No dialog)
        /// </summary>
        public static bool Confirm(string message, string title = "Xác nhận")
        {
            return ModernConfirmDialog.Confirm(message, title);
        }

        /// <summary>
        /// Confirm dangerous action (styled red)
        /// </summary>
        public static bool ConfirmDanger(string message, string title = "Xác nhận xóa")
        {
            return ModernConfirmDialog.ConfirmDanger(message, title);
        }

        /// <summary>
        /// Handle exception with logging and user-friendly message
        /// </summary>
        public static void HandleError(Exception ex, string context = null, string userMessage = null)
        {
            ErrorLogger.HandleException(ex, context);
            if (!string.IsNullOrEmpty(userMessage))
            {
                ToastNotification.Error(userMessage);
            }
        }

        /// <summary>
        /// Show validation error
        /// </summary>
        public static void ValidationError(string message)
        {
            ToastNotification.Warning(message, "Thông tin không hợp lệ");
        }

        /// <summary>
        /// Show "Please select a row" message
        /// </summary>
        public static void SelectRequired(string itemName = "dòng")
        {
            ToastNotification.Warning($"Vui lòng chọn {itemName} để thực hiện");
        }
    }
}
