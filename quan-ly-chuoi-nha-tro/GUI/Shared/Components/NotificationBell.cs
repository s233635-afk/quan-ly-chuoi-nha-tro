using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Notification bell icon with unread count badge
    /// Place in dashboard header for quick notification access
    /// </summary>
    public class NotificationBell : Panel
    {
        public event EventHandler BellClicked;
        public event EventHandler<int> UnreadCountChanged;

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly Timer _refreshTimer;
        private int _unreadCount = 0;
        private bool _isHovered = false;
        private int? _branchId;

        public int UnreadCount
        {
            get => _unreadCount;
            private set
            {
                if (_unreadCount != value)
                {
                    _unreadCount = value;
                    Invalidate();
                    UnreadCountChanged?.Invoke(this, value);
                }
            }
        }

        public NotificationBell(int? branchId = null)
        {
            _branchId = branchId;
            
            Size = new Size(44, 44);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer | 
                     ControlStyles.UserPaint, true);

            // Auto-refresh every 60 seconds
            _refreshTimer = new Timer { Interval = 60000 };
            _refreshTimer.Tick += async (s, e) => await RefreshCountAsync();

            MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
            Click += (s, e) => BellClicked?.Invoke(this, EventArgs.Empty);

            // Tooltip
            var tooltip = new ToolTip();
            tooltip.SetToolTip(this, "Thông báo");
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _ = RefreshCountAsync();
            _refreshTimer.Start();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
            base.OnHandleDestroyed(e);
        }

        /// <summary>
        /// Refresh unread notification count
        /// </summary>
        public async System.Threading.Tasks.Task RefreshCountAsync()
        {
            try
            {
                var notifications = await _bll.GetNotificationsAsync();
                
                // Filter by branch if specified
                if (_branchId.HasValue && notifications != null)
                {
                    int count = 0;
                    foreach (System.Data.DataRow row in notifications.Rows)
                    {
                        var status = row["Status"]?.ToString();
                        if (string.Equals(status, "Unread", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(status, "Sent", StringComparison.OrdinalIgnoreCase))
                        {
                            count++;
                        }
                    }
                    UnreadCount = count;
                }
                else if (notifications != null)
                {
                    int count = 0;
                    foreach (System.Data.DataRow row in notifications.Rows)
                    {
                        var status = row["Status"]?.ToString();
                        if (string.Equals(status, "Unread", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(status, "Sent", StringComparison.OrdinalIgnoreCase))
                        {
                            count++;
                        }
                    }
                    UnreadCount = count;
                }
            }
            catch
            {
                // Ignore errors during refresh
            }
        }

        /// <summary>
        /// Show notification toast for new messages
        /// </summary>
        public void ShowNewNotificationToast(string title = null)
        {
            if (UnreadCount > 0)
            {
                var message = string.IsNullOrEmpty(title)
                    ? $"Bạn có {UnreadCount} thông báo chưa đọc"
                    : title;
                ToastNotification.Info(message, "Thông báo mới");
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cx = Width / 2;
            int cy = Height / 2;

            // Hover background
            if (_isHovered)
            {
                using (var brush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    g.FillEllipse(brush, 2, 2, Width - 4, Height - 4);
                }
            }

            // Bell icon
            DrawBellIcon(g, cx, cy);

            // Badge with count
            if (UnreadCount > 0)
            {
                DrawBadge(g, UnreadCount);
            }
        }

        private void DrawBellIcon(Graphics g, int cx, int cy)
        {
            var color = _isHovered ? Color.FromArgb(0, 120, 215) : Color.FromArgb(80, 80, 80);
            
            using (var pen = new Pen(color, 2))
            using (var brush = new SolidBrush(color))
            {
                // Bell body
                var bellPath = new GraphicsPath();
                bellPath.AddArc(cx - 10, cy - 12, 20, 16, 180, 180);
                bellPath.AddLine(cx - 10, cy - 4, cx - 10, cy + 4);
                bellPath.AddLine(cx - 10, cy + 4, cx + 10, cy + 4);
                bellPath.AddLine(cx + 10, cy + 4, cx + 10, cy - 4);
                bellPath.CloseFigure();

                g.DrawPath(pen, bellPath);

                // Bell bottom
                g.DrawLine(pen, cx - 12, cy + 4, cx + 12, cy + 4);

                // Clapper
                g.FillEllipse(brush, cx - 3, cy + 6, 6, 6);
            }
        }

        private void DrawBadge(Graphics g, int count)
        {
            string text = count > 99 ? "99+" : count.ToString();
            int badgeSize = text.Length > 1 ? 20 : 16;
            int x = Width - badgeSize - 2;
            int y = 2;

            // Red circle
            using (var brush = new SolidBrush(Color.FromArgb(211, 47, 47)))
            {
                g.FillEllipse(brush, x, y, badgeSize, badgeSize);
            }

            // White border
            using (var pen = new Pen(Color.White, 2))
            {
                g.DrawEllipse(pen, x, y, badgeSize, badgeSize);
            }

            // Count text
            using (var font = new Font("Segoe UI", text.Length > 2 ? 7 : 8, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                var size = g.MeasureString(text, font);
                g.DrawString(text, font, brush, 
                    x + (badgeSize - size.Width) / 2, 
                    y + (badgeSize - size.Height) / 2);
            }
        }
    }
}
