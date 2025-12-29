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
            Height = 40;
            Padding = new Padding(12, 8, 12, 8);
            BackColor = Color.White;

            var statsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            _lblOccupancy = new Label
            {
                Text = "Äang á»Ÿ/Tá»•ng: 0/0 phĂ²ng",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };

            _lblRoomCount = new Label
            {
                Text = "PhĂ²ng: 0/0",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Margin = new Padding(12, 0, 0, 0)
            };

            statsFlow.Controls.Add(_lblOccupancy);
            statsFlow.Controls.Add(_lblRoomCount);

            Controls.Add(statsFlow);
        }

        public void UpdateStats(IEnumerable<System.Data.DataRow> displayedRooms, int displayLimit)
        {
            if (displayedRooms == null)
            {
                _lblOccupancy.Text = "Äang á»Ÿ/Tá»•ng: 0/0 phĂ²ng";
                _lblRoomCount.Text = "PhĂ²ng: 0/0";
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

            string limitText = displayLimit <= 0 ? "âˆ" : displayLimit.ToString();

            _lblOccupancy.Text = $"Äang á»Ÿ/Tá»•ng: {occupied}/{totalDisplayed} phĂ²ng";
            _lblRoomCount.Text = $"PhĂ²ng: {totalDisplayed}/{limitText}";
        }
    }
}
