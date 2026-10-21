using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vohaduy_ltdd_api.Models
{
    public class AttendanceRecord
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        public int? ShiftId { get; set; }
        public virtual Shift? Shift { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        [MaxLength(50)]
        public string CheckInMethod { get; set; } = "Location"; // Location, Face, QR, MultiFactor, Manual

        [MaxLength(50)]
        public string? CheckOutMethod { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = "OnTime"; // OnTime, Late, EarlyLeave, Overtime, Absent

        public int LateMinutes { get; set; } = 0;
        public int EarlyMinutes { get; set; } = 0;
        public double WorkedHours { get; set; } = 0;

        // GPS Geolocation fields (Requirement 5.b)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? DistanceMeters { get; set; }

        // Dynamic QR Token (Requirement 5.e)
        [MaxLength(500)]
        public string? QrCodeToken { get; set; }

        // Face Recognition confidence (Requirement 5.a)
        public double? FaceConfidenceScore { get; set; }

        [MaxLength(100)]
        public string? DeviceName { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
