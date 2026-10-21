using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vohaduy_ltdd_api.DTOs;
using vohaduy_ltdd_api.Models;

namespace vohaduy_ltdd_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public LeaveRequestsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentEmployeeIdAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return null;

            var user = await _userManager.FindByNameAsync(username);
            return user?.EmployeeId;
        }

        /// <summary>
        /// Nhân viên tạo đơn xin nghỉ phép / đi muộn / về sớm
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestDto dto, [FromQuery] int? employeeId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int? resolvedEmpId = employeeId ?? await GetCurrentEmployeeIdAsync();
            if (!resolvedEmpId.HasValue)
            {
                return BadRequest(new { status = false, message = "Không xác định được nhân viên tạo đơn" });
            }

            var employee = await _context.Employees.FindAsync(resolvedEmpId.Value);
            if (employee == null)
            {
                return NotFound(new { status = false, message = "Nhân viên không tồn tại" });
            }

            if (dto.EndDate < dto.StartDate)
            {
                return BadRequest(new { status = false, message = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu" });
            }

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = resolvedEmpId.Value,
                LeaveType = dto.LeaveType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = true,
                message = "Gửi đơn xin nghỉ phép thành công! Đơn đang chờ Quản lý phê duyệt.",
                data = leaveRequest
            });
        }

        /// <summary>
        /// Xem danh sách đơn của nhân viên hiện tại
        /// </summary>
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests([FromQuery] int? employeeId)
        {
            int? resolvedEmpId = employeeId ?? await GetCurrentEmployeeIdAsync();
            if (!resolvedEmpId.HasValue)
            {
                return BadRequest(new { status = false, message = "Vui lòng đăng nhập hoặc cung cấp employeeId" });
            }

            var requests = await _context.LeaveRequests
                .Where(l => l.EmployeeId == resolvedEmpId.Value)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return Ok(new { status = true, total = requests.Count, data = requests });
        }

        /// <summary>
        /// Admin/Manager xem toàn bộ đơn xin nghỉ phép
        /// </summary>
        [HttpGet]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRequests([FromQuery] string? status, [FromQuery] int? departmentId)
        {
            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                    .ThenInclude(e => e!.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(l => l.Employee != null && l.Employee.DepartmentId == departmentId.Value);
            }

            var requests = await query
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new LeaveRequestResponseDto
                {
                    Id = l.Id,
                    EmployeeId = l.EmployeeId,
                    EmployeeName = l.Employee != null ? l.Employee.FullName : "N/A",
                    EmployeeCode = l.Employee != null ? l.Employee.EmployeeCode : "N/A",
                    DepartmentName = l.Employee != null && l.Employee.Department != null ? l.Employee.Department.DepartmentName : null,
                    LeaveType = l.LeaveType,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Reason = l.Reason,
                    Status = l.Status,
                    ReviewerName = l.ReviewerName,
                    ReviewerNotes = l.ReviewerNotes,
                    ReviewedAt = l.ReviewedAt,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        /// <summary>
        /// Admin/Manager phê duyệt (Approve) hoặc từ chối (Reject) đơn nghỉ phép
        /// </summary>
        [HttpPut("{id}/review")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> ReviewLeaveRequest(int id, [FromBody] ReviewLeaveRequestDto dto)
        {
            var request = await _context.LeaveRequests
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (request == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy đơn xin nghỉ phép" });
            }

            string normalizedStatus = dto.Status?.Trim() ?? "";
            if (normalizedStatus.Equals("Đã duyệt", StringComparison.OrdinalIgnoreCase) || 
                normalizedStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                normalizedStatus = "Approved";
            }
            else if (normalizedStatus.Equals("Từ chối", StringComparison.OrdinalIgnoreCase) || 
                     normalizedStatus.Equals("Từ chối đơn", StringComparison.OrdinalIgnoreCase) || 
                     normalizedStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                normalizedStatus = "Rejected";
            }
            else
            {
                return BadRequest(new { status = false, message = "Trạng thái phê duyệt chỉ có thể là 'Approved' hoặc 'Rejected'" });
            }

            string reviewer = User.Identity?.Name ?? "Admin Võ Hà Duy";
            request.Status = normalizedStatus;
            request.ReviewerName = reviewer;
            request.ReviewerNotes = dto.ReviewerNotes;
            request.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            string statusMsg = normalizedStatus == "Approved" ? "ĐÃ DUYỆT" : "ĐÃ TỪ CHỐI";
            return Ok(new
            {
                status = true,
                message = $"Đã xử lý đơn nghỉ phép: {statusMsg}",
                data = request
            });
        }

        /// <summary>
        /// Hủy đơn xin phép khi chưa được duyệt
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelLeaveRequest(int id, [FromQuery] int? employeeId)
        {
            var request = await _context.LeaveRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy đơn" });
            }

            if (request.Status != "Pending")
            {
                return BadRequest(new { status = false, message = "Chỉ có thể hủy đơn khi đang ở trạng thái Chờ duyệt (Pending)" });
            }

            if (employeeId.HasValue && request.EmployeeId != employeeId.Value)
            {
                return Forbid();
            }

            _context.LeaveRequests.Remove(request);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Hủy đơn xin nghỉ phép thành công" });
        }
    }
}
