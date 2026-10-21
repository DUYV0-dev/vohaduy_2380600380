using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.DTOs
{
    public class FaceLoginDto
    {
        [Required(ErrorMessage = "Email không được để trống"), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ảnh khuôn mặt base64 không được để trống")]
        public string FaceImageBase64 { get; set; } = string.Empty;
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Position { get; set; }
        public string? DepartmentName { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsFaceRegistered { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public string TargetDashboard { get; set; } = "EmployeeDashboard";
    }
}
