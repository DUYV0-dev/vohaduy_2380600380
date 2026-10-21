using Microsoft.AspNetCore.Identity;

namespace vohaduy_ltdd_api.Models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public int? EmployeeId { get; set; }
        public string? Initials { get; set; }
        public string? AvatarUrl { get; set; }
        public string? FaceFeatureHash { get; set; }
        public string? FacePhotoBase64 { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
