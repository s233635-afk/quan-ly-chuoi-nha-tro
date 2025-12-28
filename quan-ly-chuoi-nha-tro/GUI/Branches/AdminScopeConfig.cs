using System;
using System.Configuration;

namespace quan_ly_chuoi_nha_tro.GUI
{
    internal static class AdminScopeConfig
    {
        private static readonly Lazy<int> MaxRoomsLazy = new Lazy<int>(LoadMaxRooms);

        public static int MaxRooms => MaxRoomsLazy.Value;

        private static int LoadMaxRooms()
        {
            string raw = null;
            try { raw = ConfigurationManager.AppSettings["AdminMaxRooms"]; } catch { }

            if (raw == null) return 20;
            if (string.IsNullOrWhiteSpace(raw)) return 0;
            if (!int.TryParse(raw.Trim(), out var value)) return 20;
            return value > 0 ? value : 0;
        }
    }
}

