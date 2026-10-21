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
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IFaceRecognitionService _faceService;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IFaceRecognitionService faceService,
            ILogger<EmployeesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _faceService = faceService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách nhân viên (hỗ trợ tìm kiếm, lọc theo phòng ban)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] int? departmentId, [FromQuery] string? search)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(e =>
                    e.FullName.ToLower().Contains(lowerSearch) ||
                    e.EmployeeCode.ToLower().Contains(lowerSearch) ||
                    e.Email.ToLower().Contains(lowerSearch)
                );
            }

            var employees = await query
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new EmployeeResponseDto
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FullName,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    Position = e.Position,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
                    HireDate = e.HireDate,
                    BaseSalary = e.BaseSalary,
                    AvatarUrl = e.AvatarUrl,
                    IsFaceRegistered = e.IsFaceRegistered,
                    IsActive = e.IsActive,
                    AttendanceReliabilityScore = e.AttendanceReliabilityScore,
                    CreatedAt = e.CreatedAt,
                    Role = e.Role
                })
                .ToListAsync();

            return Ok(employees);
        }

        /// <summary>
        /// Lấy chi tiết thông tin một nhân viên theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy nhân viên" });
            }

            var response = new EmployeeResponseDto
            {
                Id = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                FullName = employee.FullName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                Position = employee.Position,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department?.DepartmentName,
                HireDate = employee.HireDate,
                BaseSalary = employee.BaseSalary,
                AvatarUrl = employee.AvatarUrl,
                IsFaceRegistered = employee.IsFaceRegistered,
                IsActive = employee.IsActive,
                AttendanceReliabilityScore = employee.AttendanceReliabilityScore,
                CreatedAt = employee.CreatedAt,
                Role = employee.Role
            };

            return Ok(new { status = true, data = response });
        }

        /// <summary>
        /// Thêm mới nhân viên
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Employees.AnyAsync(e => e.EmployeeCode == dto.EmployeeCode))
            {
                return BadRequest(new { status = false, message = "Mã nhân viên đã tồn tại" });
            }

            if (await _context.Employees.AnyAsync(e => e.Email == dto.Email))
            {
                return BadRequest(new { status = false, message = "Email đã tồn tại" });
            }

            if (dto.DepartmentId.HasValue)
            {
                var dept = await _context.Departments.FindAsync(dto.DepartmentId.Value);
                if (dept == null)
                    return BadRequest(new { status = false, message = "Phòng ban không tồn tại" });
            }

            string? faceData = null;
            bool isFaceRegistered = false;
            if (!string.IsNullOrWhiteSpace(dto.FaceImageBase64))
            {
                try
                {
                    faceData = _faceService.ExtractFaceDescriptor(dto.FaceImageBase64);
                    isFaceRegistered = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Face extraction failed for new employee: {Msg}", ex.Message);
                }
            }

            var employee = new Employee
            {
                EmployeeCode = dto.EmployeeCode,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Position = dto.Position,
                DepartmentId = dto.DepartmentId,
                BaseSalary = dto.BaseSalary ?? 10000000,
                AvatarUrl = dto.AvatarUrl,
                FaceData = faceData,
                IsFaceRegistered = isFaceRegistered,
                IsActive = true,
                Role = dto.Role ?? "Employee",
                AttendanceReliabilityScore = 100,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok(employee);
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy nhân viên" });
            }

            if (!string.IsNullOrWhiteSpace(dto.EmployeeCode) && dto.EmployeeCode != employee.EmployeeCode)
            {
                if (await _context.Employees.AnyAsync(e => e.EmployeeCode == dto.EmployeeCode))
                    return BadRequest(new { status = false, message = "Mã nhân viên mới đã tồn tại" });
                employee.EmployeeCode = dto.EmployeeCode;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != employee.Email)
            {
                if (await _context.Employees.AnyAsync(e => e.Email == dto.Email))
                    return BadRequest(new { status = false, message = "Email mới đã tồn tại" });
                employee.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                employee.FullName = dto.FullName;

            if (dto.PhoneNumber != null)
                employee.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Position))
                employee.Position = dto.Position;

            if (dto.DepartmentId.HasValue)
            {
                var dept = await _context.Departments.FindAsync(dto.DepartmentId.Value);
                if (dept == null)
                    return BadRequest(new { status = false, message = "Phòng ban không hợp lệ" });
                employee.DepartmentId = dto.DepartmentId.Value;
            }

            if (dto.BaseSalary.HasValue)
                employee.BaseSalary = dto.BaseSalary.Value;

            if (dto.AvatarUrl != null)
                employee.AvatarUrl = dto.AvatarUrl;

            if (dto.IsActive.HasValue)
                employee.IsActive = dto.IsActive.Value;

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                employee.Role = dto.Role;

                // ĐỒNG BỘ ROLE SANG ASP.NET IDENTITY (USER)
                if (!string.IsNullOrEmpty(employee.UserId))
                {
                    var user = await _userManager.FindByIdAsync(employee.UserId);
                    if (user != null)
                    {
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        if (!currentRoles.Contains(dto.Role))
                        {
                            await _userManager.RemoveFromRolesAsync(user, currentRoles);
                            await _userManager.AddToRoleAsync(user, dto.Role);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(employee);
        }

        /// <summary>
        /// Xóa nhân viên theo ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy nhân viên" });
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Xóa nhân viên thành công" });
        }
    }
}
