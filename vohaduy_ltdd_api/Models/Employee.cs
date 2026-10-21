using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace vohaduy_ltdd_api.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string Position { get; set; } = "Nhân viên";

        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        public string Role { get; set; } = "Employee";

        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; } = 10000000;

        public string? AvatarUrl { get; set; }

        public string? FaceData { get; set; }
        public bool IsFaceRegistered { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int AttendanceReliabilityScore { get; set; } = 100;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

        [JsonIgnore]
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    }
}
