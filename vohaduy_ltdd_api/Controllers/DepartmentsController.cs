using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vohaduy_ltdd_api.DTOs;
using vohaduy_ltdd_api.Models;

namespace vohaduy_ltdd_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    DepartmentCode = d.DepartmentCode,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description,
                    Location = d.Location,
                    ManagerName = d.ManagerName,
                    EmployeeCount = d.Employees.Count,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var dept = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dept == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy phòng ban" });
            }

            var response = new DepartmentResponseDto
            {
                Id = dept.Id,
                DepartmentCode = dept.DepartmentCode,
                DepartmentName = dept.DepartmentName,
                Description = dept.Description,
                Location = dept.Location,
                ManagerName = dept.ManagerName,
                EmployeeCount = dept.Employees.Count,
                CreatedAt = dept.CreatedAt
            };

            return Ok(new { status = true, data = response });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var codeExists = await _context.Departments.AnyAsync(d => d.DepartmentCode == dto.DepartmentCode);
            if (codeExists)
            {
                return BadRequest(new { status = false, message = "Mã phòng ban đã tồn tại" });
            }

            var department = new Department
            {
                DepartmentCode = dto.DepartmentCode,
                DepartmentName = dto.DepartmentName,
                Description = dto.Description,
                Location = dto.Location,
                ManagerName = dto.ManagerName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, new
            {
                status = true,
                message = "Tạo phòng ban thành công",
                data = department
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto dto)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy phòng ban" });
            }

            if (!string.IsNullOrWhiteSpace(dto.DepartmentName))
                department.DepartmentName = dto.DepartmentName;

            if (dto.Description != null)
                department.Description = dto.Description;

            if (dto.Location != null)
                department.Location = dto.Location;

            if (dto.ManagerName != null)
                department.ManagerName = dto.ManagerName;

            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Cập nhật phòng ban thành công", data = department });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy phòng ban" });
            }

            if (department.Employees.Any())
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Không thể xóa phòng ban đang có nhân viên trực thuộc. Vui lòng chuyển nhân viên sang phòng ban khác trước."
                });
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Xóa phòng ban thành công" });
        }
    }
}
