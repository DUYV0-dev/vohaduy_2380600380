using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vohaduy_ltdd_api.DTOs;
using vohaduy_ltdd_api.Models;
using vohaduy_ltdd_api.Services;

namespace vohaduy_ltdd_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAttendanceService _attendanceService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            ApplicationDbContext context,
            IAttendanceService attendanceService,
            UserManager<User> userManager,
            ILogger<AttendanceController> logger)
        {
            _context = context;
            _attendanceService = attendanceService;
            _userManager = userManager;
            _logger = logger;
        }

        private async Task<int?> GetCurrentEmployeeIdAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return null;

            var user = await _userManager.FindByNameAsync(username);
            return user?.EmployeeId;
        }

        /// <summary>
        /// 5.b Chấm công / Check-in bằng Tọa độ Vị trí GPS (Geofencing)
        /// </summary>
        [HttpPost("check-in-location")]
        public async Task<IActionResult> CheckInLocation([FromBody] LocationCheckInDto dto, [FromQuery] int? employeeId)
        {
            int? resolvedEmpId = employeeId ?? dto.EmployeeId ?? await GetCurrentEmployeeIdAsync();
            if (!resolvedEmpId.HasValue)
            {
                return BadRequest(new { status = false, message = "Không xác định được nhân viên thực hiện (vui lòng gửi employeeId hoặc đăng nhập)" });
            }

            var result = await _attendanceService.ProcessLocationCheckInAsync(resolvedEmpId.Value, dto);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    status = false,
                    message = result.Message,
                    distanceMeters = result.DistanceMeters
                });
            }

            return Ok(new
            {
                status = true,
                message = result.Message,
                attendanceStatus = result.Status,
                lateMinutes = result.LateMinutes,
                distanceMeters = result.DistanceMeters,
                data = result.Record
            });
        }

        /// <summary>
        /// 5.a Chấm công / Check-in bằng Khuôn mặt (Face Recognition)
        /// </summary>
        [HttpPost("check-in-face")]
        public async Task<IActionResult> CheckInFace([FromBody] FaceCheckInDto dto, [FromQuery] int? employeeId)
        {
            int? resolvedEmpId = employeeId ?? dto.EmployeeId ?? await GetCurrentEmployeeIdAsync();
            if (!resolvedEmpId.HasValue)
            {
                return BadRequest(new { status = false, message = "Không xác định được nhân viên thực hiện" });
            }

            var result = await _attendanceService.ProcessFaceCheckInAsync(resolvedEmpId.Value, dto);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    status = false,
                    message = result.Message,
                    confidenceScore = result.FaceConfidenceScore
                });
            }

            return Ok(new
            {
                status = true,
                message = result.Message,
                attendanceStatus = result.Status,
                lateMinutes = result.LateMinutes,
                confidenceScore = result.FaceConfidenceScore,
                data = result.Record
            });
        }

        /// <summary>
        /// <summary>
        /// 6. Tính năng mới: Check-in Đa tầng Chống gian lận (GPS Geofence + Face Biometric AI)
        /// </summary>
        [HttpPost("check-in-smart")]
        public async Task<IActionResult> CheckInSmart([FromBody] SmartCheckInDto dto, [FromQuery] int? employeeId)
        {
            int? resolvedEmpId = employeeId ?? dto.EmployeeId ?? await GetCurrentEmployeeIdAsync();
            if (!resolvedEmpId.HasValue)
            {
                return BadRequest(new { status = false, message = "Không xác định được nhân viên thực hiện" });
            }

            var result = await _attendanceService.ProcessSmartCheckInAsync(resolvedEmpId.Value, dto);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    status = false,
                    message = result.Message,
                    distanceMeters = result.DistanceMeters,
                    confidenceScore = result.FaceConfidenceScore
                });
            }

            return Ok(new
            {
                status = true,
                message = "Xác thực Chấm công Đa tầng Chống gian lận thành công!",
                attendanceStatus = result.Status,
                lateMinutes = result.LateMinutes,
                distanceMeters = result.DistanceMeters,
                faceConfidenceScore = result.FaceConfidenceScore,
                data = result.Record
            });
        }

        /// <summary>
        /// Check-Out kết thúc ngày/ca làm việc
        /// </summary>
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto, [FromQuery] int? employeeId)
        {
            int? resolvedEmpId = employeeId ?? dto.EmployeeId ?? await GetCurrentEmployeeIdAsync();
            if (!resolvedEmpId.HasValue && !string.IsNullOrWhiteSpace(dto.AttendanceId))
            {
                if (int.TryParse(dto.AttendanceId, out int parsedAttId))
                {
                    var attRecord = await _context.AttendanceRecords.FindAsync(parsedAttId);
                    resolvedEmpId = attRecord?.EmployeeId;
                }
            }
            if (!resolvedEmpId.HasValue)
            {
                return BadRequest(new { status = false, message = "Không xác định được nhân viên thực hiện" });
            }

            var result = await _attendanceService.ProcessCheckOutAsync(resolvedEmpId.Value, dto);
            if (!result.Success)
            {
                return BadRequest(new { status = false, message = result.Message });
            }

            return Ok(new
            {
                status = true,
                message = result.Message,
                workedHours = result.WorkedHours,
                earlyMinutes = result.EarlyMinutes,
                data = result.Record
            });
        }

        /// <summary>
        /// Lịch sử chấm công của nhân viên hiện tại
        /// </summary>
        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory([FromQuery] int? employeeId, [FromQuery] int? month, [FromQuery] int? year)
        {
            int? resolvedEmpId = employeeId ?? await GetCurrentEmployeeIdAsync();
            if (!resolvedEmpId.HasValue)
            {
                return BadRequest(new { status = false, message = "Vui lòng đăng nhập hoặc cung cấp employeeId" });
            }

            int targetMonth = month ?? DateTime.UtcNow.Month;
            int targetYear = year ?? DateTime.UtcNow.Year;

            var startDate = new DateTime(targetYear, targetMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var records = await _context.AttendanceRecords
                .Include(r => r.Shift)
                .Include(r => r.Employee)
                .Where(r => r.EmployeeId == resolvedEmpId.Value && r.Date >= startDate && r.Date <= endDate)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            return Ok(new
            {
                status = true,
                employeeId = resolvedEmpId.Value,
                month = targetMonth,
                year = targetYear,
                total = records.Count,
                data = records
            });
        }

        /// <summary>
        /// Bảng chấm công toàn công ty (Admin/Manager) kèm bộ lọc
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAttendance([FromQuery] AttendanceFilterDto filter)
        {
            var query = _context.AttendanceRecords
                .Include(r => r.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(r => r.Shift)
                .AsQueryable();

            if (filter.Date.HasValue)
            {
                var dateOnly = filter.Date.Value.Date;
                query = query.Where(r => r.Date == dateOnly);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(r => r.Date >= filter.FromDate.Value.Date);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(r => r.Date <= filter.ToDate.Value.Date);
            }

            if (filter.DepartmentId.HasValue)
            {
                query = query.Where(r => r.Employee != null && r.Employee.DepartmentId == filter.DepartmentId.Value);
            }

            if (filter.EmployeeId.HasValue)
            {
                query = query.Where(r => r.EmployeeId == filter.EmployeeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(r => r.Status == filter.Status);
            }

            var records = await query
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.CheckInTime)
                .ToListAsync();

            return Ok(new { status = true, total = records.Count, data = records });
        }

        /// <summary>
        /// 6. Tính năng mới: Thống kê Điểm chuyên cần AI & Bảng lương tạm tính thời gian thực
        /// </summary>
        [HttpGet("monthly-summary/{employeeId}")]
        public async Task<IActionResult> GetMonthlySummary(int employeeId, [FromQuery] int? month, [FromQuery] int? year)
        {
            int targetMonth = month ?? DateTime.UtcNow.Month;
            int targetYear = year ?? DateTime.UtcNow.Year;

            try
            {
                var summary = await _attendanceService.GetEmployeeMonthlySummaryAsync(employeeId, targetMonth, targetYear);
                return Ok(new { status = true, data = summary });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { status = false, message = "Không tìm thấy nhân viên" });
            }
        }

        /// <summary>
        /// 4. CRUD Chấm công: Tạo bản ghi thủ công (Admin hiệu chỉnh)
        /// </summary>
        [HttpPost("manual-record")]
        public async Task<IActionResult> CreateManualRecord([FromBody] ManualAttendanceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null)
                return BadRequest(new { status = false, message = "Nhân viên không tồn tại" });

            double workedHours = 0;
            if (dto.CheckInTime.HasValue && dto.CheckOutTime.HasValue)
            {
                workedHours = Math.Round((dto.CheckOutTime.Value - dto.CheckInTime.Value).TotalHours, 2);
            }

            var record = new AttendanceRecord
            {
                EmployeeId = dto.EmployeeId,
                ShiftId = dto.ShiftId,
                Date = dto.Date.Date,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                CheckInMethod = dto.CheckInMethod ?? "Manual",
                Status = dto.Status ?? "OnTime",
                WorkedHours = workedHours,
                Notes = dto.Notes ?? "Ghi nhận chấm công thủ công bởi Quản trị viên",
                CreatedAt = DateTime.UtcNow
            };

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Thêm bản ghi chấm công thành công", data = record });
        }

        /// <summary>
        /// 4. CRUD Chấm công: Cập nhật bản ghi chấm công
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendanceRecord(int id, [FromBody] ManualAttendanceDto dto)
        {
            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy bản ghi chấm công" });
            }

            record.Date = dto.Date.Date;
            record.CheckInTime = dto.CheckInTime;
            record.CheckOutTime = dto.CheckOutTime;
            record.Status = dto.Status;
            record.Notes = dto.Notes;

            if (dto.CheckInTime.HasValue && dto.CheckOutTime.HasValue)
            {
                record.WorkedHours = Math.Round((dto.CheckOutTime.Value - dto.CheckInTime.Value).TotalHours, 2);
            }

            await _context.SaveChangesAsync();
            return Ok(new { status = true, message = "Cập nhật bản ghi chấm công thành công", data = record });
        }

        /// <summary>
        /// 4. CRUD Chấm công: Xóa bản ghi chấm công
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendanceRecord(int id)
        {
            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy bản ghi" });
            }

            _context.AttendanceRecords.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Xóa bản ghi thành công" });
        }
    }
}
