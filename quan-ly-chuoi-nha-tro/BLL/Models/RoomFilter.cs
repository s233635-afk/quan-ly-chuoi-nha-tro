namespace QuanLyNhaTro.BLL.Models
{
    /// <summary>
    /// Filter criteria for room queries
    /// </summary>
    public class RoomFilter
    {
        public string SearchText { get; set; }
        public int? StatusId { get; set; }
        public int DisplayLimit { get; set; }
        public int? BranchId { get; set; }
    }
}
