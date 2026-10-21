using vohaduy_ltdd_api.DTOs;

namespace vohaduy_ltdd_api.Services
{
    public class LocationService : ILocationService
    {
        private readonly IConfiguration _configuration;

        public LocationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CompanyGeofenceDto GetCompanyGeofenceSettings()
        {
            var companySection = _configuration.GetSection("CompanySettings");
            return new CompanyGeofenceDto
            {
                CompanyName = companySection["CompanyName"] ?? "Công ty Công nghệ Võ Hà Duy",
                OfficeAddress = companySection["OfficeAddress"] ?? "Tòa nhà Công nghệ Võ Hà Duy, TP. Hồ Chí Minh",
                OfficeLatitude = companySection.GetValue<double>("OfficeLatitude", 10.7769),
                OfficeLongitude = companySection.GetValue<double>("OfficeLongitude", 106.7009),
                AllowedRadiusMeters = companySection.GetValue<double>("AllowedRadiusMeters", 100.0),
                QrTokenExpirySeconds = companySection.GetValue<int>("QrTokenExpirySeconds", 45)
            };
        }

        /// <summary>
        /// Calculates distance between two coordinates in meters using the Haversine formula
        /// </summary>
        public double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusMeters = 6371000.0;

            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return Math.Round(EarthRadiusMeters * c, 2);
        }

        public bool IsWithinOfficeGeofence(double userLat, double userLon, out double calculatedDistanceMeters)
        {
            var settings = GetCompanyGeofenceSettings();
            calculatedDistanceMeters = CalculateDistanceMeters(userLat, userLon, settings.OfficeLatitude, settings.OfficeLongitude);

            return calculatedDistanceMeters <= settings.AllowedRadiusMeters;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }
    }
}
