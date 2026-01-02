using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Rooms.Components
{
    /// <summary>
    /// Room statistics panel - displays room count and occupancy stats
    /// Extracted from FrmRoomManager (lines 284-300, 1737-1750)
    /// </summary>
    public class RoomStatsPanel : Panel
    {
        private Label _lblOccupancy;
        private Label _lblRoomCount;

        public RoomStatsPanel()
        {
            InitializePanel();
        }

        private void InitializePanel()
        {
            Dock = DockStyle.Top;
            Height = 45; // Increased from 40 to 45 to ensure labels are not cut off
            Padding = new Padding(12, 10, 12, 10);
            BackColor = Color.White;

            var statsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false, // Changed to false to prevent wrapping
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            _lblOccupancy = new Label
            {
                Text = "Đang ở/Tổng: 0/0 phòng",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                MinimumSize = new Size(180, 20) // Ensure minimum width
            };

            _lblRoomCount = new Label
            {
                Text = "Phòng: 0/0",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Margin = new Padding(20, 0, 0, 0), // Increased margin from 12 to 20
                MinimumSize = new Size(120, 20) // Ensure minimum width
            };

            statsFlow.Controls.Add(_lblOccupancy);
            statsFlow.Controls.Add(_lblRoomCount);

            Controls.Add(statsFlow);
        }

        public void UpdateStats(IEnumerable<System.Data.DataRow> displayedRooms, int displayLimit)
        {
            if (displayedRooms == null)
            {
                _lblOccupancy.Text = "Đang ở/Tổng: 0/0 phòng";
                _lblRoomCount.Text = "Phòng: 0/0";
                return;
            }

            var roomsList = displayedRooms.ToList();
            int totalDisplayed = roomsList.Count;
            int occupied = roomsList.Count(r =>
            {
                var occupants = 0;
                if (r.Table.Columns.Contains("Occupants") && int.TryParse(r["Occupants"]?.ToString(), out var occ))
                    occupants = occ;
                return occupants > 0;
            });

            string limitText = displayLimit <= 0 ? "∞" : displayLimit.ToString();

            _lblOccupancy.Text = $"Đang ở/Tổng: {occupied}/{totalDisplayed} phòng";
            _lblRoomCount.Text = $"Phòng: {totalDisplayed}/{limitText}";
        }
    }
}
