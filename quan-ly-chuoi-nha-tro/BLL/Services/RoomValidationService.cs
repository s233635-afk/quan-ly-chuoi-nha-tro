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
        public ValidationResult ValidateRoomData(string roomNumber, int? roomTypeId, decimal? roomPrice,  decimal? area)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(roomNumber))
                result.AddError("Sá»‘ phĂ²ng khĂ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng");
            else if (roomNumber.Length > 20)
                result.AddError("Sá»‘ phĂ²ng khĂ´ng Ä‘Æ°á»£c dĂ i quĂ¡ 20 kĂ½ tá»±");

            if (!roomTypeId.HasValue || roomTypeId.Value <= 0)
                result.AddError("Loáº¡i phĂ²ng khĂ´ng há»£p lá»‡");

            if (roomPrice.HasValue && roomPrice.Value < 0)
                result.AddError("GiĂ¡ phĂ²ng khĂ´ng Ä‘Æ°á»£c Ă¢m");

            if (area.HasValue && area.Value <= 0)
                result.AddError("Diá»‡n tĂ­ch phĂ²ng pháº£i lá»›n hÆ¡n 0");

            return result;
        }

        /// <summary>
        /// Validate room status change
        /// </summary>
        public ValidationResult ValidateStatusChange(string currentStatus, string newStatus, int occupants)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(newStatus))
            {
                result.AddError("Tráº¡ng thĂ¡i má»›i khĂ´ng há»£p lá»‡");
                return result;
            }

            // Cannot change to "empty" if room has occupants
            var isChangingToEmpty = newStatus.ToLowerInvariant().Contains("trá»‘ng") || 
                                   newStatus.ToLowerInvariant().Contains("empty");
            if (isChangingToEmpty && occupants > 0)
            {
                result.AddError($"KhĂ´ng thá»ƒ Ä‘á»•i sang tráº¡ng thĂ¡i trá»‘ng khi phĂ²ng cĂ²n {occupants} ngÆ°á»i á»Ÿ");
            }

            return result;
        }
    }
}
