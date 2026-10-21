using vohaduy_ltdd_api.DTOs;

namespace vohaduy_ltdd_api.Services
{
    public interface ILocationService
    {
        double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2);
        bool IsWithinOfficeGeofence(double userLat, double userLon, out double calculatedDistanceMeters);
        CompanyGeofenceDto GetCompanyGeofenceSettings();
    }
}
