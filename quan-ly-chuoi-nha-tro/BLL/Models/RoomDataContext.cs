using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QuanLyNhaTro.BLL.Models
{
    /// <summary>
    /// Data context containing all room-related data tables
    /// </summary>
    public class RoomDataContext
    {
        public DataTable Rooms { get; set; }
        public DataTable Statuses { get; set; }
        public DataTable RoomTypes { get; set; }
        public DataTable TenantHistory { get; set; }
        public DataTable Contracts { get; set; }
        public DataTable Tenants { get; set; }
        public DataTable Assets { get; set; }
    }
}
