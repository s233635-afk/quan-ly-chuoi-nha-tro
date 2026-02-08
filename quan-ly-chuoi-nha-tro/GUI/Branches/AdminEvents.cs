using System;
using System.Threading.Tasks;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    internal static class AdminEvents
    {
        public static event Action DataChanged;

        private static string _actorName;
        private static string _actorRole;
        private static int? _actorUserId;
        private static string _activityScope;
        private static DateTime _lastLogTimeUtc = DateTime.MinValue;
        private static string _lastLogKey;

        public static void SetActor(string name, string role, int? userId = null)
        {
            _actorName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            _actorRole = string.IsNullOrWhiteSpace(role) ? null : role.Trim();
            _actorUserId = userId;
        }

        public static void SetActivityScope(string scope)
        {
            _activityScope = string.IsNullOrWhiteSpace(scope) ? null : scope.Trim();
        }

        public static void NotifyDataChanged()
        {
            try
            {
                DataChanged?.Invoke();
            }
            catch
            {
                // ignore subscriber errors
            }

            TryLogStaffActivity();
        }

        private static void TryLogStaffActivity()
        {
            if (!string.Equals(_actorRole, "Staff", StringComparison.OrdinalIgnoreCase)) return;
            if (string.IsNullOrWhiteSpace(_actorName)) return;

            string scope = _activityScope;
            string action = string.IsNullOrWhiteSpace(scope)
                ? "vừa cập nhật dữ liệu"
                : $"vừa cập nhật {scope}";

            string key = $"{_actorName}|{action}";
            var now = DateTime.UtcNow;
            if (now - _lastLogTimeUtc < TimeSpan.FromSeconds(3) && string.Equals(_lastLogKey, key, StringComparison.Ordinal))
                return;

            _lastLogTimeUtc = now;
            _lastLogKey = key;

            string title = "Hoạt động nhân viên";
            string message = $"Nhân viên {_actorName} {action}.";

            _ = Task.Run(async () =>
            {
                try
                {
                    var bll = new AdminDataBLL();
                    await bll.AddNotificationAsync(null, title, message, "Unread");
                }
                catch
                {
                    // ignore log failures
                }
            });
        }
    }
}

