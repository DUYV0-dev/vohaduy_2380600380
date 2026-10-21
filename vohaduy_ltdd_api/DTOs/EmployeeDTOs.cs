using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.DTOs
{
    public class CreateEmployeeDto
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống"), EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Position { get; set; } = "Nhân viên";

        public int? DepartmentId { get; set; }

        public decimal? BaseSalary { get; set; }

        public string? AvatarUrl { get; set; }

        public string? FaceImageBase64 { get; set; }

        public string? Role { get; set; }
    }

    public class UpdateEmployeeDto
    {
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Position { get; set; }
        public int? DepartmentId { get; set; }
        public decimal? BaseSalary { get; set; }
        public string? AvatarUrl { get; set; }
        public bool? IsActive { get; set; }
        public string? Role { get; set; }
    }

    public class RegisterFaceDto
    {
        [Required(ErrorMessage = "Ảnh khuôn mặt base64 không được để trống")]
        public string FaceImageBase64 { get; set; } = string.Empty;
    }

    public class EmployeeResponseDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Phone => PhoneNumber;
        public string Username => Email;
        public string Role { get; set; } = "Employee";
        public string Position { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime HireDate { get; set; }
        public decimal BaseSalary { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsFaceRegistered { get; set; }
        public bool IsActive { get; set; }
        public int AttendanceReliabilityScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
