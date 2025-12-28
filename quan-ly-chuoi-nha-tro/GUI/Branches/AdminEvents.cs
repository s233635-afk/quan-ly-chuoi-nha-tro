using System;

namespace quan_ly_chuoi_nha_tro.GUI
{
    internal static class AdminEvents
    {
        public static event Action DataChanged;

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
        }
    }
}

