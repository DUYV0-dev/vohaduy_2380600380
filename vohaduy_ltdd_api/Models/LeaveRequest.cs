using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.Models
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        [Required]
        [MaxLength(50)]
        public string LeaveType { get; set; } = "AnnualLeave"; // AnnualLeave, SickLeave, PersonalLeave, LateArrival, EarlyLeave, BusinessTrip

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [MaxLength(100)]
        public string? ReviewerName { get; set; }

        [MaxLength(500)]
        public string? ReviewerNotes { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
