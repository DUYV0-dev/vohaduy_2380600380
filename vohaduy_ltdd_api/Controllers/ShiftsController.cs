using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vohaduy_ltdd_api.DTOs;
using vohaduy_ltdd_api.Models;

namespace vohaduy_ltdd_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShiftsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetShifts([FromQuery] bool? isActive)
        {
            var query = _context.Shifts.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            var shifts = await query
                .OrderBy(s => s.StartTime)
                .Select(s => new ShiftResponseDto
                {
                    Id = s.Id,
                    ShiftCode = s.ShiftCode,
                    ShiftName = s.ShiftName,
                    StartTime = s.StartTime.ToString(@"hh\:mm"),
                    EndTime = s.EndTime.ToString(@"hh\:mm"),
                    GracePeriodMinutes = s.GracePeriodMinutes,
                    WorkDays = s.WorkDays,
                    Description = s.Description,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new { status = true, total = shifts.Count, data = shifts });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShiftById(int id)
        {
            var s = await _context.Shifts.FindAsync(id);
            if (s == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy ca làm việc" });
            }

            var response = new ShiftResponseDto
            {
                Id = s.Id,
                ShiftCode = s.ShiftCode,
                ShiftName = s.ShiftName,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                GracePeriodMinutes = s.GracePeriodMinutes,
                WorkDays = s.WorkDays,
                Description = s.Description,
                IsActive = s.IsActive
            };

            return Ok(new { status = true, data = response });
        }

        [HttpPost]
        public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var codeExists = await _context.Shifts.AnyAsync(s => s.ShiftCode == dto.ShiftCode);
            if (codeExists)
            {
                return BadRequest(new { status = false, message = "Mã ca làm việc đã tồn tại" });
            }

            if (!TimeSpan.TryParse(dto.StartTime, out var startTime) || !TimeSpan.TryParse(dto.EndTime, out var endTime))
            {
                return BadRequest(new { status = false, message = "Định dạng thời gian không hợp lệ. Vui lòng dùng HH:mm (ví dụ 08:00)" });
            }

            var shift = new Shift
            {
                ShiftCode = dto.ShiftCode,
                ShiftName = dto.ShiftName,
                StartTime = startTime,
                EndTime = endTime,
                GracePeriodMinutes = dto.GracePeriodMinutes,
                WorkDays = dto.WorkDays,
                Description = dto.Description,
                IsActive = true
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShiftById), new { id = shift.Id }, new
            {
                status = true,
                message = "Tạo ca làm việc thành công",
                data = shift
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShift(int id, [FromBody] UpdateShiftDto dto)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy ca làm việc" });
            }

            if (!string.IsNullOrWhiteSpace(dto.ShiftName))
                shift.ShiftName = dto.ShiftName;

            if (!string.IsNullOrWhiteSpace(dto.StartTime) && TimeSpan.TryParse(dto.StartTime, out var startTime))
                shift.StartTime = startTime;

            if (!string.IsNullOrWhiteSpace(dto.EndTime) && TimeSpan.TryParse(dto.EndTime, out var endTime))
                shift.EndTime = endTime;

            if (dto.GracePeriodMinutes.HasValue)
                shift.GracePeriodMinutes = dto.GracePeriodMinutes.Value;

            if (!string.IsNullOrWhiteSpace(dto.WorkDays))
                shift.WorkDays = dto.WorkDays;

            if (dto.Description != null)
                shift.Description = dto.Description;

            if (dto.IsActive.HasValue)
                shift.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Cập nhật ca làm việc thành công", data = shift });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy ca làm việc" });
            }

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Xóa ca làm việc thành công" });
        }
    }
}
