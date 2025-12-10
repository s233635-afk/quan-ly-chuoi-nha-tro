using System.Data;
using System.Threading.Tasks;
using QuanLyNhaTro.DAL;

namespace QuanLyNhaTro.BLL
{
    public class AdminDataBLL
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public Task<DataTable> GetRoomsAsync() => dbHelper.GetRoomsAsync();
        public Task<DataTable> GetStaffAsync() => dbHelper.GetUsersByRoleAsync(2);
        public Task<DataTable> GetTenantsAsync() => dbHelper.GetTenantsAsync();
        public Task<DataTable> GetContractsAsync() => dbHelper.GetContractsAsync();
        public Task<DataTable> GetDepositsAsync() => dbHelper.GetDepositsAsync();
        public Task<DataTable> GetUtilitiesAsync() => dbHelper.GetUtilitiesAsync();
        public Task<DataTable> GetInvoicesAsync() => dbHelper.GetInvoicesAsync();
        public Task<DataTable> GetMaintenanceAsync() => dbHelper.GetMaintenanceAsync();
        public Task<DataTable> GetAssetsAsync() => dbHelper.GetAssetsAsync();
        public Task<DataTable> GetNotificationsAsync() => dbHelper.GetNotificationsAsync();
        public Task<DataTable> GetSystemSettingsAsync() => dbHelper.GetSystemSettingsAsync();
    }
}
