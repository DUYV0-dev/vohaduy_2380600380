using Microsoft.EntityFrameworkCore;
using vohaduy_ltdd_api.DTOs;
using vohaduy_ltdd_api.Models;

namespace vohaduy_ltdd_api.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILocationService _locationService;
        private readonly IFaceRecognitionService _faceService;
        private readonly ILogger<AttendanceService> _logger;

        public AttendanceService(
            ApplicationDbContext context,
            ILocationService locationService,
            IFaceRecognitionService faceService,
            ILogger<AttendanceService> logger)
        {
            _context = context;
            _locationService = locationService;
            _faceService = faceService;
            _logger = logger;
        }

        public async Task<CheckInResult> ProcessLocationCheckInAsync(int employeeId, LocationCheckInDto dto)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return new CheckInResult { Success = false, Message = "Không tìm thấy thông tin nhân viên" };
            }

            // Check office geofence
            bool isInside = _locationService.IsWithinOfficeGeofence(dto.Latitude, dto.Longitude, out double distance);
            if (!isInside)
            {
                var geofence = _locationService.GetCompanyGeofenceSettings();
                return new CheckInResult
                {
                    Success = false,
                    DistanceMeters = distance,
                    Message = $"Bạn đang cách văn phòng {distance}m (bán kính cho phép là {geofence.AllowedRadiusMeters}m). Chấm công vị trí không hợp lệ!"
                };
            }

            return await SaveCheckInRecordAsync(
                employee,
                dto.ShiftId,
                "Location",
                dto.Latitude,
                dto.Longitude,
                distance,
                qrToken: null,
                faceConfidence: null,
                dto.DeviceName,
                dto.Notes);
        }

        public async Task<CheckInResult> ProcessFaceCheckInAsync(int employeeId, FaceCheckInDto dto)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return new CheckInResult { Success = false, Message = "Không tìm thấy thông tin nhân viên" };
            }

            if (string.IsNullOrWhiteSpace(employee.FaceData))
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = "Nhân viên chưa đăng ký khuôn mặt trên hệ thống. Vui lòng vào hồ sơ để chụp và đăng ký khuôn mặt trước."
                };
            }

            // Verify face biometrics
            var verifyResult = _faceService.VerifyFace(employee.FaceData, dto.FaceImageBase64);
            if (!verifyResult.IsMatch)
            {
                return new CheckInResult
                {
                    Success = false,
                    FaceConfidenceScore = verifyResult.ConfidenceScore,
                    Message = verifyResult.Message ?? "Khuôn mặt không trùng khớp với hồ sơ"
                };
            }

            double? distance = null;
            if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            {
                _locationService.IsWithinOfficeGeofence(dto.Latitude.Value, dto.Longitude.Value, out double dist);
                distance = dist;
            }

            return await SaveCheckInRecordAsync(
                employee,
                dto.ShiftId,
                "Face",
                dto.Latitude,
                dto.Longitude,
                distance,
                qrToken: null,
                faceConfidence: verifyResult.ConfidenceScore,
                dto.DeviceName,
                dto.Notes);
        }

        public async Task<CheckInResult> ProcessSmartCheckInAsync(int employeeId, SmartCheckInDto dto)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return new CheckInResult { Success = false, Message = "Không tìm thấy thông tin nhân viên" };
            }

            // 1. Verify GPS Geofence
            bool isInside = _locationService.IsWithinOfficeGeofence(dto.Latitude, dto.Longitude, out double distance);
            if (!isInside)
            {
                var geofence = _locationService.GetCompanyGeofenceSettings();
                return new CheckInResult
                {
                    Success = false,
                    DistanceMeters = distance,
                    Message = $"[Lớp 1 Thất Bại] Vị trí cách công ty {distance}m (cho phép tối đa {geofence.AllowedRadiusMeters}m). Nghi vấn gian lận vị trí!"
                };
            }

            // 2. Verify Face Biometrics
            if (string.IsNullOrWhiteSpace(employee.FaceData))
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = "[Lớp 2 Thất Bại] Nhân viên chưa đăng ký khuôn mặt trên hệ thống"
                };
            }

            var faceResult = _faceService.VerifyFace(employee.FaceData, dto.FaceImageBase64);
            if (!faceResult.IsMatch)
            {
                return new CheckInResult
                {
                    Success = false,
                    FaceConfidenceScore = faceResult.ConfidenceScore,
                    Message = $"[Lớp 2 Thất Bại] Khuôn mặt không khớp ({faceResult.ConfidenceScore}%). Phát hiện nguy cơ chấm công hộ!"
                };
            }

            // Multi-factor verified!
            return await SaveCheckInRecordAsync(
                employee,
                dto.ShiftId,
                "MultiFactor",
                dto.Latitude,
                dto.Longitude,
                distance,
                qrToken: null,
                faceConfidence: faceResult.ConfidenceScore,
                deviceName: dto.DeviceName,
                notes: $"{dto.Notes} | [Đã xác thực đồng thời GPS Geofencing + Face Recognition]");
        }

        public async Task<CheckOutResult> ProcessCheckOutAsync(int employeeId, CheckOutDto dto)
        {
            var today = DateTime.UtcNow.Date;
            var record = await _context.AttendanceRecords
                .Include(r => r.Shift)
                .Where(r => r.EmployeeId == employeeId && r.Date == today)
                .OrderByDescending(r => r.Id)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                return new CheckOutResult
                {
                    Success = false,
                    Message = "Hôm nay bạn chưa thực hiện Check-In, không thể Check-Out"
                };
            }

            if (record.CheckOutTime.HasValue)
            {
                return new CheckOutResult
                {
                    Success = false,
                    Record = record,
                    Message = $"Bạn đã Check-Out lúc {record.CheckOutTime.Value.ToLocalTime():HH:mm:ss} rồi!"
                };
            }

            var now = DateTime.UtcNow;
            record.CheckOutTime = now;
            record.CheckOutMethod = dto.CheckOutMethod ?? "Manual";

            // Calculate worked hours
            if (record.CheckInTime.HasValue)
            {
                var duration = now - record.CheckInTime.Value;
                record.WorkedHours = Math.Round(duration.TotalHours, 2);
            }

            // Check if left early
            if (record.Shift != null)
            {
                var shiftEndTimeToday = today.Add(record.Shift.EndTime);
                if (now < shiftEndTimeToday)
                {
                    var earlyDiff = shiftEndTimeToday - now;
                    record.EarlyMinutes = (int)Math.Max(0, earlyDiff.TotalMinutes);
                    if (record.Status == "OnTime" && record.EarlyMinutes > 5)
                    {
                        record.Status = "EarlyLeave";
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Notes))
            {
                record.Notes = $"{record.Notes}; Check-out note: {dto.Notes}".Trim(';', ' ');
            }

            await _context.SaveChangesAsync();

            return new CheckOutResult
            {
                Success = true,
                Message = $"Check-Out thành công lúc {now.ToLocalTime():HH:mm:ss}. Tổng giờ làm việc: {record.WorkedHours}h",
                Record = record,
                WorkedHours = record.WorkedHours,
                EarlyMinutes = record.EarlyMinutes
            };
        }

        public async Task<MonthlySummaryDto> GetEmployeeMonthlySummaryAsync(int employeeId, int month, int year)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                throw new KeyNotFoundException("Không tìm thấy nhân viên");

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var records = await _context.AttendanceRecords
                .Where(r => r.EmployeeId == employeeId && r.Date >= startDate && r.Date <= endDate)
                .ToListAsync();

            var leaveRequests = await _context.LeaveRequests
                .Where(l => l.EmployeeId == employeeId && l.Status == "Approved" &&
                            l.StartDate <= endDate && l.EndDate >= startDate)
                .ToListAsync();

            int actualWorkedDays = records.Count(r => r.CheckInTime.HasValue);
            double totalWorkedHours = Math.Round(records.Sum(r => r.WorkedHours), 2);
            int lateCount = records.Count(r => r.Status == "Late" || r.LateMinutes > 0);
            int totalLateMinutes = records.Sum(r => r.LateMinutes);
            int earlyLeaveCount = records.Count(r => r.Status == "EarlyLeave" || r.EarlyMinutes > 0);
            int approvedLeaveDays = leaveRequests.Count;

            const int standardWorkingDays = 22;
            int absentDays = Math.Max(0, Math.Min(DateTime.UtcNow.Day, standardWorkingDays) - actualWorkedDays - approvedLeaveDays);

            // Calculate reliability score (starts at 100, penalties for late/absent)
            int score = 100;
            score -= (lateCount * 2);
            score -= (earlyLeaveCount * 2);
            score -= (absentDays * 5);
            score = Math.Clamp(score, 10, 100);

            string tier = score switch
            {
                >= 95 => "Diamond (Kim Cương - Chuyên cần tuyệt đối)",
                >= 85 => "Gold (Vàng - Nhân viên xuất sắc)",
                >= 70 => "Silver (Bạc - Đạt chuẩn)",
                _ => "Bronze (Đồng - Cần cải thiện kỷ luật)"
            };

            // AI Prediction note based on late patterns
            string aiNote;
            if (lateCount == 0)
            {
                aiNote = "Nhân viên duy trì kỷ luật tuyệt vời! Dự báo khả năng hoàn thành 100% công chuẩn trong tháng.";
            }
            else if (lateCount <= 2)
            {
                aiNote = $"Nhân viên đã trễ {lateCount} lần ({totalLateMinutes} phút). Khuyến nghị nhắc nhở đi sớm 15 phút vào các ngày đầu tuần.";
            }
            else
            {
                aiNote = $"CẢNH BÁO AI: Tần suất đi trễ cao ({lateCount} lần). Nguy cơ bị trừ bậc xếp loại thi đua tháng {month}/{year}.";
            }

            // Payroll Estimation
            decimal baseSalary = employee.BaseSalary;
            decimal dailyRate = Math.Round(baseSalary / standardWorkingDays, 0);
            decimal penaltyAmount = (lateCount * 50000m) + (absentDays * dailyRate);
            decimal estimatedSalary = Math.Max(0, (actualWorkedDays * dailyRate) - (lateCount * 50000m));

            return new MonthlySummaryDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                EmployeeCode = employee.EmployeeCode,
                Month = month,
                Year = year,
                TotalStandardWorkingDays = standardWorkingDays,
                ActualWorkedDays = actualWorkedDays,
                TotalWorkedHours = totalWorkedHours,
                LateCount = lateCount,
                TotalLateMinutes = totalLateMinutes,
                EarlyLeaveCount = earlyLeaveCount,
                AbsentDays = absentDays,
                ApprovedLeaveDays = approvedLeaveDays,
                AttendanceReliabilityScore = score,
                ReliabilityTier = tier,
                AiPredictionNotes = aiNote,
                BaseSalary = baseSalary,
                DailyRate = dailyRate,
                PenaltyAmount = penaltyAmount,
                EstimatedSalary = estimatedSalary
            };
        }

        private async Task<CheckInResult> SaveCheckInRecordAsync(
            Employee employee,
            int? shiftId,
            string method,
            double? lat,
            double? lng,
            double? distance,
            string? qrToken,
            double? faceConfidence,
            string? deviceName,
            string? notes)
        {
            var today = DateTime.UtcNow.Date;
            var existingRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(r => r.EmployeeId == employee.Id && r.Date == today);

            if (existingRecord != null && existingRecord.CheckInTime.HasValue)
            {
                return new CheckInResult
                {
                    Success = false,
                    Record = existingRecord,
                    Message = $"Hôm nay bạn đã Check-In lúc {existingRecord.CheckInTime.Value.ToLocalTime():HH:mm:ss} rồi!"
                };
            }

            // Find shift
            Shift? shift = null;
            if (shiftId.HasValue)
            {
                shift = await _context.Shifts.FindAsync(shiftId.Value);
            }
            shift ??= await _context.Shifts.FirstOrDefaultAsync(s => s.IsActive);

            var now = DateTime.UtcNow;
            string status = "OnTime";
            int lateMinutes = 0;

            if (shift != null)
            {
                var shiftStartTimeToday = today.Add(shift.StartTime);
                var graceTime = shiftStartTimeToday.AddMinutes(shift.GracePeriodMinutes);

                if (now > graceTime)
                {
                    status = "Late";
                    lateMinutes = (int)Math.Max(0, (now - shiftStartTimeToday).TotalMinutes);
                }
            }

            var record = existingRecord ?? new AttendanceRecord
            {
                EmployeeId = employee.Id,
                ShiftId = shift?.Id,
                Date = today
            };

            record.CheckInTime = now;
            record.CheckInMethod = method;
            record.Status = status;
            record.LateMinutes = lateMinutes;
            record.Latitude = lat;
            record.Longitude = lng;
            record.DistanceMeters = distance;
            record.QrCodeToken = qrToken;
            record.FaceConfidenceScore = faceConfidence;
            record.DeviceName = deviceName;
            record.Notes = notes;

            // Adjust employee's ongoing attendance reliability score
            if (status == "Late")
            {
                employee.AttendanceReliabilityScore = Math.Max(10, employee.AttendanceReliabilityScore - 2);
            }
            else
            {
                employee.AttendanceReliabilityScore = Math.Min(100, employee.AttendanceReliabilityScore + 1);
            }

            if (existingRecord == null)
            {
                _context.AttendanceRecords.Add(record);
            }
            await _context.SaveChangesAsync();

            string message = status == "Late"
                ? $"Check-In ({method}) thành công nhưng BỊ TRỄ {lateMinutes} phút so với giờ làm việc"
                : $"Check-In ({method}) thành công ĐÚNG GIỜ!";

            return new CheckInResult
            {
                Success = true,
                Message = message,
                Record = record,
                Status = status,
                LateMinutes = lateMinutes,
                DistanceMeters = distance,
                FaceConfidenceScore = faceConfidence
            };
        }
    }
}
