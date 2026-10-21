using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email hoặc Tên đăng nhập không được để trống")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = string.Empty;
    }
}