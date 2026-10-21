using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.Models
{
    public class RegistrationModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống"), EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống"), MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? Initials { get; set; }

        public string? Role { get; set; } = "Employee"; // 2 roles: Admin, Employee

        public int? DepartmentId { get; set; }

        public string? Position { get; set; } = "Nhân viên";
    }
}