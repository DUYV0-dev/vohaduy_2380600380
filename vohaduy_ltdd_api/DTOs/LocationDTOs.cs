using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.DTOs
{
    public class ShareLocationDto
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public string? Address { get; set; }
        public string? ActivityStatus { get; set; } = "Đang làm việc tại hiện trường";
    }

    public class CompanyGeofenceDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string OfficeAddress { get; set; } = string.Empty;
        public double OfficeLatitude { get; set; }
        public double OfficeLongitude { get; set; }
        public double AllowedRadiusMeters { get; set; }
        public int QrTokenExpirySeconds { get; set; }
    }
}
