using vohaduy_ltdd_api.DTOs;
using vohaduy_ltdd_api.Models;

namespace vohaduy_ltdd_api.Services
{
    public class CheckInResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AttendanceRecord? Record { get; set; }
        public string Status { get; set; } = "OnTime";
        public int LateMinutes { get; set; }
        public double? DistanceMeters { get; set; }
        public double? FaceConfidenceScore { get; set; }
    }

    public class CheckOutResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AttendanceRecord? Record { get; set; }
        public double WorkedHours { get; set; }
        public int EarlyMinutes { get; set; }
    }

    public interface IAttendanceService
    {
        Task<CheckInResult> ProcessLocationCheckInAsync(int employeeId, LocationCheckInDto dto);
        Task<CheckInResult> ProcessFaceCheckInAsync(int employeeId, FaceCheckInDto dto);
        Task<CheckInResult> ProcessSmartCheckInAsync(int employeeId, SmartCheckInDto dto);
        Task<CheckOutResult> ProcessCheckOutAsync(int employeeId, CheckOutDto dto);
        Task<MonthlySummaryDto> GetEmployeeMonthlySummaryAsync(int employeeId, int month, int year);
    }
}
