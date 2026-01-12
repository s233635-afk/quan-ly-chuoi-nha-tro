using QuanLyNhaTro.BLL.Models;

namespace QuanLyNhaTro.BLL.Services
{
    /// <summary>
    /// Service for validating room data
    /// </summary>
    public class RoomValidationService
    {
        /// <summary>
        /// Validate room data before creating or updating
        /// </summary>
        public RoomValidationResult ValidateRoomData(string roomNumber, int? roomTypeId, decimal? roomPrice,  decimal? area)
        {
                        var result = new RoomValidationResult();
            
                        if (string.IsNullOrWhiteSpace(roomNumber))
                            result.AddError("Số phòng không được để trống");
                        else if (roomNumber.Length > 20)
                            result.AddError("Số phòng không được dài quá 20 ký tự");
            
                        if (!roomTypeId.HasValue || roomTypeId.Value <= 0)
                            result.AddError("Loại phòng không hợp lệ");
            
                        if (roomPrice.HasValue && roomPrice.Value < 0)
                            result.AddError("Giá phòng không được âm");
            
                        if (area.HasValue && area.Value <= 0)
                            result.AddError("Diện tích phòng phải lớn hơn 0");

            return result;
        }

        /// <summary>
        /// Validate room status change
        /// </summary>
        public RoomValidationResult ValidateStatusChange(string currentStatus, string newStatus, int occupants)
        {
            var result = new RoomValidationResult();

            if (string.IsNullOrWhiteSpace(newStatus))
            {
                result.AddError("Trạng thái mới không hợp lệ");
                return result;
            }

            // Cannot change to "empty" if room has occupants
            var isChangingToEmpty = newStatus.ToLowerInvariant().Contains("trống") || 
                                   newStatus.ToLowerInvariant().Contains("empty");
            if (isChangingToEmpty && occupants > 0)
            {
                result.AddError($"Không thể đổi sang trạng thái trống khi phòng còn {occupants} người ở");
            }

            return result;
        }
    }
}
