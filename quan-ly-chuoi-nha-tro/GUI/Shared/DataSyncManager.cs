using System;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public static class DataSyncManager
    {
        public static event EventHandler RoomsDataChanged;
        public static event EventHandler TenantsDataChanged;
        public static event EventHandler ContractsDataChanged;
        public static event EventHandler InvoicesDataChanged;
        public static event EventHandler PaymentsDataChanged;

        public static void NotifyRoomsChanged()
        {
            RoomsDataChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void NotifyTenantsChanged()
        {
            TenantsDataChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void NotifyContractsChanged()
        {
            ContractsDataChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void NotifyInvoicesChanged()
        {
            InvoicesDataChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void NotifyPaymentsChanged()
        {
            PaymentsDataChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
