using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using quan_ly_chuoi_nha_tro.GUI;
using QuanLyNhaTro.BLL.Services;

namespace quan_ly_chuoi_nha_tro.GUI.Rooms.Components
{
    /// <summary>
    /// Room card UI component - Displays room information in card format
    /// Extracted from FrmRoomManager.CreateRoomCard() (lines 876-1090)
    /// </summary>
    public class RoomCard : Panel
    {
        public event EventHandler<int> CardClicked;
        
        private readonly int _roomId;
        private readonly DataRow _roomData;
        private readonly RoomUIService _uiService;
        private bool _isSelected;
        private bool _isInspecting;

        public int RoomId => _roomId;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateSelectionState();
            }
        }

        public bool IsInspecting
        {
            get => _isInspecting;
            set
            {
                _isInspecting = value;
                Invalidate(); // Trigger repaint
            }
        }

        public RoomCard(DataRow roomData, int roomId, RoomUIService uiService, bool isSelected = false)
        {
            _roomData = roomData ?? throw new ArgumentNullException(nameof(roomData));
            _roomId = roomId;
            _uiService = uiService ?? new RoomUIService();
            _isSelected = isSelected;

            InitializeCard();
            RenderCardContent();
        }

        private void InitializeCard()
        {
            Width = 320;
            Height = 200; // Fixed height for uniform card sizes
            MinimumSize = new Size(320, 200);
            MaximumSize = new Size(320, 200);
            BackColor = _isSelected ? Color.FromArgb(236, 242, 255) : Color.White;
            BorderStyle = BorderStyle.None;
            Margin = new Padding(12, 12, 12, 12);
            Cursor = Cursors.Hand;
            Tag = _roomId;

            // Enable smooth rendering for rounded corners
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            // Custom paint for border and shadow
            Paint += OnCardPaint;
            Resize += (s, e) => UpdateRegion();
            MouseEnter += OnCardMouseEnter;
            MouseLeave += OnCardMouseLeave;
            Click += OnCardClick;
            
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            UiKit.SetRoundedRegion(this, 18);
        }

        private void OnCardPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Enhanced border colors for better visibility
            var borderColor = _isSelected ? Color.FromArgb(0, 95, 180) : Color.FromArgb(160, 170, 180);
            var borderWidth = _isSelected ? 3 : 2;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            
            // Draw subtle shadow for depth
            var shadowRect = new Rectangle(2, 2, Width - 4, Height - 4);
            using (var shadowPath = UiKit.GetRoundPath(shadowRect, 18))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            {
                e.Graphics.FillPath(shadowBrush, shadowPath);
            }
            
            // Draw background with rounded corners
            using (var path = UiKit.GetRoundPath(rect, 18))
            using (var bgBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(bgBrush, path);
            }
            
            // Draw prominent border with rounded corners
            using (var path = UiKit.GetRoundPath(rect, 18))
            using (var pen = new Pen(borderColor, borderWidth))
            {
                e.Graphics.DrawPath(pen, path);
            }

            if (_isInspecting)
            {
                var inspectRect = new Rectangle(Width - 26, Height - 26, 20, 20);
                using (var fill = new SolidBrush(Color.FromArgb(225, 238, 255)))
                    e.Graphics.FillRectangle(fill, inspectRect);
                using (var pen = new Pen(Color.FromArgb(0, 122, 204), 2))
                    e.Graphics.DrawRectangle(pen, inspectRect);
            }
        }

        private void OnCardMouseEnter(object sender, EventArgs e)
        {
            if (!_isSelected)
                BackColor = Color.FromArgb(245, 249, 255);
        }

        private void OnCardMouseLeave(object sender, EventArgs e)
        {
            BackColor = _isSelected ? Color.FromArgb(236, 242, 255) : Color.White;
        }

        private void OnCardClick(object sender, EventArgs e)
        {
            CardClicked?.Invoke(this, _roomId);
        }

        private void RenderCardContent()
        {
            SuspendLayout();
            Controls.Clear();

            string roomNumber = SafeReadString("RoomNumber") ?? "N/A";
            string typeName = SafeReadString("TypeName") ?? "N/A";
            string statusName = SafeReadString("StatusName") ?? "N/A";
            var statusColor = _uiService.GetStatusColor(statusName);
            decimal price = TryGetDecimal("RoomPrice") ?? 0m;
            int occupants = TryGetInt("Occupants");
            string assetSummary = (string)Tag; // Will be set externally

            // Status bar at top
            var statusBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 10,
                BackColor = statusColor
            };
            Controls.Add(statusBar);

            // Main layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(14, 14, 14, 14)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Header: Room number + icon
            var headerPanel = CreateHeaderPanel(roomNumber, typeName, occupants);
            
            // Price
            var lblPrice = new Label
            {
                Text = $"{price:N0}Ä‘",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top,
                Height = 28,
                AutoSize = false,
                TextAlign = ContentAlignment.TopRight,
                Margin = new Padding(0, 0, 0, 6)
            };
            lblPrice.Click += OnCardClick;

            // Info panel
            var infoPanel = CreateInfoPanel(typeName, statusName, occupants, assetSummary);

            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(lblPrice, 0, 1);
            mainLayout.Controls.Add(infoPanel, 0, 2);

            Controls.Add(mainLayout);
            ResumeLayout();
        }

        private Panel CreateHeaderPanel(string roomNumber, string typeName, int occupants)
        {
            var lblRoom = new Label
            {
                Text = $"PhĂ²ng {roomNumber}",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 50, 90),
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 0, 0)
            };
            lblRoom.Click += OnCardClick;

            var lblTypeIcon = new Label
            {
                Text = GetOccupancyIcon(occupants, typeName),
                Font = new Font("Segoe UI Symbol", 16, FontStyle.Regular),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Right,
                Width = 38,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0)
            };
            lblTypeIcon.Click += OnCardClick;

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32
            };
            headerPanel.Controls.Add(lblRoom);
            headerPanel.Controls.Add(lblTypeIcon);

            return headerPanel;
        }

        private Panel CreateInfoPanel(string typeName, string statusName, int occupants, string assetSummary)
        {
            var infoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = string.IsNullOrEmpty(assetSummary) ? 3 : 4,
                Padding = new Padding(0)
            };

            var lblTypeInfo = new Label
            {
                Text = $"Loáº¡i: {GetTypeIcon(typeName)} {typeName}",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 0, 0, 6)
            };
            lblTypeInfo.Click += OnCardClick;

            var lblStatusInfo = new Label
            {
                Text = $"Tráº¡ng thĂ¡i: {statusName}",
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 55, 110),
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 0, 0, 6)
            };
            lblStatusInfo.Click += OnCardClick;

            var lblOccupantsInfo = new Label
            {
                Text = $"Sá»‘ ngÆ°á»i: {occupants}",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };
            lblOccupantsInfo.Click += OnCardClick;

            infoLayout.Controls.Add(lblTypeInfo, 0, 0);
            infoLayout.Controls.Add(lblStatusInfo, 0, 1);
            infoLayout.Controls.Add(lblOccupantsInfo, 0, 2);

            if (!string.IsNullOrEmpty(assetSummary))
            {
                assetSummary = TextFixer.FixUtf8Mojibake(assetSummary) ?? assetSummary;
                var lblAssetsInfo = new Label
                {
                    Text = $"TĂ i sáº£n: {assetSummary}",
                    Font = new Font("Segoe UI", 10.5f),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Dock = DockStyle.Top,
                    Height = 24,
                    AutoSize = false,
                    TextAlign = ContentAlignment.TopLeft
                };
                lblAssetsInfo.Click += OnCardClick;
                infoLayout.Controls.Add(lblAssetsInfo, 0, 3);
            }

            var infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0)
            };
            infoPanel.Controls.Add(infoLayout);

            return infoPanel;
        }

        private void UpdateSelectionState()
        {
            BackColor = _isSelected ? Color.FromArgb(236, 242, 255) : Color.White;
            Invalidate(); // Trigger repaint for border
        }

        // Helper methods
        private string SafeReadString(string columnName)
        {
            if (_roomData == null || !_roomData.Table.Columns.Contains(columnName)) return null;
            var value = _roomData[columnName]?.ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;
            var fixedValue = TextFixer.ForceFixUtf8Mojibake(value) ?? value;
            return fixedValue.Trim();
        }

        private int TryGetInt(string columnName)
        {
            if (_roomData == null || !_roomData.Table.Columns.Contains(columnName)) return 0;
            return int.TryParse(_roomData[columnName]?.ToString(), out var result) ? result : 0;
        }

        private decimal? TryGetDecimal(string columnName)
        {
            if (_roomData == null || !_roomData.Table.Columns.Contains(columnName)) return null;
            return decimal.TryParse(_roomData[columnName]?.ToString(), out var result) ? (decimal?)result : null;
        }

        private string GetTypeIcon(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return "đŸ ";
            var normalized = typeName.ToLowerInvariant();
            if (normalized.Contains("Ä‘Æ¡n") || normalized.Contains("single")) return "đŸ›ï¸";
            if (normalized.Contains("Ä‘Ă´i") || normalized.Contains("double")) return "đŸ›ï¸đŸ›ï¸";
            if (normalized.Contains("vip") || normalized.Contains("premium")) return "â­";
            return "đŸ ";
        }

        private string GetOccupancyIcon(int occupants, string typeName)
        {
            if (occupants > 0) return "đŸ‘¥";
            if (string.IsNullOrWhiteSpace(typeName)) return "đŸ”‘";
            var normalized = typeName.ToLowerInvariant();
            if (normalized.Contains("vip") || normalized.Contains("premium")) return "â­";
            return "đŸ”‘";
        }
    }
}
