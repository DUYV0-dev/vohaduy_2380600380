using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace vohaduy_ltdd_api.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string ShiftCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ShiftName { get; set; } = string.Empty;

        [Required]
        public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);

        [Required]
        public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);

        public int GracePeriodMinutes { get; set; } = 15;

        [MaxLength(50)]
        public string WorkDays { get; set; } = "2,3,4,5,6"; // Monday to Friday

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
