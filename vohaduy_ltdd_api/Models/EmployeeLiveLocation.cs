using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.Models
{
    public class EmployeeLiveLocation
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        [MaxLength(100)]
        public string EmployeeName { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? ActivityStatus { get; set; } = "Đang làm việc";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
