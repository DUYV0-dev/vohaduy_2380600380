using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vohaduy_ltdd_api.DTOs;
using vohaduy_ltdd_api.Models;
using vohaduy_ltdd_api.Services;

namespace vohaduy_ltdd_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ApplicationDbContext _context;

        public LocationController(ILocationService locationService, ApplicationDbContext context)
        {
            _locationService = locationService;
            _context = context;
        }

        /// <summary>
        /// 5.b Lấy cấu hình vùng địa lý Geofence của văn phòng công ty
        /// </summary>
        [HttpGet("company-geofence")]
        public IActionResult GetCompanyGeofence()
        {
            var geofence = _locationService.GetCompanyGeofenceSettings();
            return Ok(new { status = true, data = geofence });
        }

        /// <summary>
        /// 5.b Kiểm tra một tọa độ GPS bất kỳ có nằm trong vùng cho phép của công ty không
        /// </summary>
        [HttpGet("check-coordinates")]
        public IActionResult CheckCoordinates([FromQuery] double latitude, [FromQuery] double longitude)
        {
            bool isInside = _locationService.IsWithinOfficeGeofence(latitude, longitude, out double distance);
            var geofence = _locationService.GetCompanyGeofenceSettings();

            return Ok(new
            {
                status = true,
                isInside = isInside,
                distanceMeters = distance,
                allowedRadiusMeters = geofence.AllowedRadiusMeters,
                message = isInside
                    ? $"Hợp lệ! Bạn đang ở trong văn phòng công ty (khoảng cách: {distance}m <= {geofence.AllowedRadiusMeters}m)"
                    : $"Ngoài phạm vi! Bạn đang cách văn phòng {distance}m (cho phép tối đa {geofence.AllowedRadiusMeters}m)"
            });
        }

        /// <summary>
        /// 5.b Nhân viên chia sẻ vị trí hiện tại (Live Location) khi đi công tác / ra ngoài hiện trường
        /// </summary>
        [HttpPost("share-live-location")]
        public async Task<IActionResult> ShareLiveLocation([FromBody] ShareLocationDto dto, [FromQuery] int employeeId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy nhân viên" });
            }

            var liveLoc = await _context.EmployeeLiveLocations
                .FirstOrDefaultAsync(l => l.EmployeeId == employeeId);

            if (liveLoc == null)
            {
                liveLoc = new EmployeeLiveLocation
                {
                    EmployeeId = employeeId,
                    EmployeeName = employee.FullName,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    Address = dto.Address,
                    ActivityStatus = dto.ActivityStatus ?? "Đang làm việc",
                    UpdatedAt = DateTime.UtcNow
                };
                _context.EmployeeLiveLocations.Add(liveLoc);
            }
            else
            {
                liveLoc.Latitude = dto.Latitude;
                liveLoc.Longitude = dto.Longitude;
                liveLoc.Address = dto.Address;
                liveLoc.ActivityStatus = dto.ActivityStatus ?? liveLoc.ActivityStatus;
                liveLoc.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = true,
                message = "Cập nhật vị trí thời gian thực thành công!",
                data = liveLoc
            });
        }

        /// <summary>
        /// 5.b Admin/Manager theo dõi danh sách vị trí thời gian thực của các nhân viên
        /// </summary>
        [HttpGet("live-locations")]
        public async Task<IActionResult> GetLiveLocations()
        {
            var locations = await _context.EmployeeLiveLocations
                .OrderByDescending(l => l.UpdatedAt)
                .ToListAsync();

            return Ok(new { status = true, total = locations.Count, data = locations });
        }
    }
}
