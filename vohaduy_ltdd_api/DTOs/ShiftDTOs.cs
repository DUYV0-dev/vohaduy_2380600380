using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.DTOs
{
    public class CreateShiftDto
    {
        [Required(ErrorMessage = "Mã ca làm việc không được để trống")]
        public string ShiftCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên ca không được để trống")]
        public string ShiftName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ bắt đầu định dạng HH:mm, vd: 08:00")]
        public string StartTime { get; set; } = "08:00";

        [Required(ErrorMessage = "Giờ kết thúc định dạng HH:mm, vd: 17:00")]
        public string EndTime { get; set; } = "17:00";

        public int GracePeriodMinutes { get; set; } = 15;
        public string WorkDays { get; set; } = "2,3,4,5,6";
        public string? Description { get; set; }
    }

    public class UpdateShiftDto
    {
        public string? ShiftName { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int? GracePeriodMinutes { get; set; }
        public string? WorkDays { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ShiftResponseDto
    {
        public int Id { get; set; }
        public string ShiftCode { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int GracePeriodMinutes { get; set; }
        public string WorkDays { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
