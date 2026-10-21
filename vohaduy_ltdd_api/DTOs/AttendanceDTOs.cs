using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.DTOs
{
    // 5.b Check-in qua GPS Geofencing
    public class LocationCheckInDto
    {
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public int? ShiftId { get; set; }
        public string? DeviceName { get; set; }
        public string? Notes { get; set; }
    }

    // 5.a Check-in qua Khuôn mặt
    public class FaceCheckInDto
    {
        public int? EmployeeId { get; set; }

        [Required(ErrorMessage = "Ảnh khuôn mặt selfie không được để trống")]
        public string FaceImageBase64 { get; set; } = string.Empty;

        public int? ShiftId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? DeviceName { get; set; }
        public string? Notes { get; set; }
    }

    // 6. Tính năng mới: Check-in Đa tầng Chống gian lận (GPS Geofencing + Face Biometrics)
    public class SmartCheckInDto
    {
        public int? EmployeeId { get; set; }

        [Required(ErrorMessage = "Cần toạ độ GPS vị trí thực tế")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Cần toạ độ GPS vị trí thực tế")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Cần ảnh chụp khuôn mặt selfie xác thực")]
        public string FaceImageBase64 { get; set; } = string.Empty;

        public int? ShiftId { get; set; }
        public string? DeviceName { get; set; }
        public string? Notes { get; set; }
    }

    public class CheckOutDto
    {
        public int? EmployeeId { get; set; }
        public string? AttendanceId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? CheckOutMethod { get; set; } = "Location"; // Location, Face, QR, Manual
        public string? Notes { get; set; }
    }

    public class AttendanceFilterDto
    {
        public DateTime? Date { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? DepartmentId { get; set; }
        public int? EmployeeId { get; set; }
        public string? Status { get; set; }
    }

    public class ManualAttendanceDto
    {
        [Required]
        public int EmployeeId { get; set; }

        public int? ShiftId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public string Status { get; set; } = "OnTime";
        public string CheckInMethod { get; set; } = "Manual";
        public string? Notes { get; set; }
    }

    // 6. Thống kê & Tính điểm chuyên cần & Ước tính lương
    public class MonthlySummaryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalStandardWorkingDays { get; set; } = 22;
        public int ActualWorkedDays { get; set; }
        public double TotalWorkedHours { get; set; }
        public int LateCount { get; set; }
        public int TotalLateMinutes { get; set; }
        public int EarlyLeaveCount { get; set; }
        public int AbsentDays { get; set; }
        public int ApprovedLeaveDays { get; set; }

        // Điểm chuyên cần (1-100) & Xếp hạng
        public int AttendanceReliabilityScore { get; set; }
        public string ReliabilityTier { get; set; } = "Gold"; // Diamond (>=95), Gold (85-94), Silver (70-84), Bronze (<70)
        public string AiPredictionNotes { get; set; } = string.Empty;

        // Ước tính lương & Phạt trễ giờ
        public decimal BaseSalary { get; set; }
        public decimal DailyRate { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal EstimatedSalary { get; set; }
    }
}
