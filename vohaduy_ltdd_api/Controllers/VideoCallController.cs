using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using vohaduy_ltdd_api.DTOs;
using vohaduy_ltdd_api.Hubs;
using vohaduy_ltdd_api.Models;

namespace vohaduy_ltdd_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoCallController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatAndVideoHub> _hubContext;
        private readonly UserManager<User> _userManager;

        public VideoCallController(
            ApplicationDbContext context,
            IHubContext<ChatAndVideoHub> hubContext,
            UserManager<User> userManager)
        {
            _context = context;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        private async Task<(int? empId, string? displayName)> GetCurrentUserInfoAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return (null, null);

            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return (null, null);

            return (user.EmployeeId, user.FullName ?? user.UserName);
        }

        /// <summary>
        /// Tạo nhóm gọi video / phòng họp trực tuyến (Tự động sinh mã phòng 6 số để chia sẻ)
        /// </summary>
        [HttpPost("create-room")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateMeetingRoomDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (currentUserEmpId, currentUserName) = await GetCurrentUserInfoAsync();

            int? resolvedHostId = dto.HostEmployeeId ?? currentUserEmpId;
            string hostName = !string.IsNullOrWhiteSpace(dto.HostName) 
                ? dto.HostName 
                : (!string.IsNullOrWhiteSpace(currentUserName) ? currentUserName : "Chủ phòng");

            // Nếu có employeeId, kiểm tra và lấy tên nhân viên nếu có
            if (resolvedHostId.HasValue)
            {
                var emp = await _context.Employees.FindAsync(resolvedHostId.Value);
                if (emp != null)
                {
                    hostName = emp.FullName;
                }
            }

            // Sinh mã phòng gồm 6 số ngẫu nhiên không trùng lặp (ví dụ: 682914)
            string roomCode;
            do
            {
                roomCode = Random.Shared.Next(100000, 999999).ToString();
            } 
            while (await _context.VideoMeetingRooms.AnyAsync(r => r.RoomCode == roomCode && r.IsActive));

            var room = new VideoMeetingRoom
            {
                RoomCode = roomCode,
                RoomTitle = string.IsNullOrWhiteSpace(dto.RoomTitle) ? $"Phòng gọi nhóm #{roomCode}" : dto.RoomTitle,
                HostEmployeeId = resolvedHostId,
                HostName = hostName,
                DepartmentId = dto.DepartmentId,
                CallType = dto.CallType ?? "GroupVideo",
                MaxParticipants = dto.MaxParticipants > 0 ? dto.MaxParticipants : 20,
                IsActive = true,
                StartedAt = DateTime.UtcNow
            };

            _context.VideoMeetingRooms.Add(room);
            await _context.SaveChangesAsync();

            // Phát tín hiệu SignalR thông báo phòng mới
            await _hubContext.Clients.All.SendAsync("IncomingCallInvitation", new
            {
                RoomCode = roomCode,
                RoomTitle = room.RoomTitle,
                HostName = hostName,
                CallType = room.CallType
            });

            return Ok(new
            {
                status = true,
                message = "Tạo phòng gọi video nhóm thành công!",
                roomCode = room.RoomCode,
                data = new VideoRoomResponseDto
                {
                    Id = room.Id,
                    RoomCode = room.RoomCode,
                    RoomTitle = room.RoomTitle,
                    HostEmployeeId = room.HostEmployeeId,
                    HostName = hostName,
                    DepartmentName = null,
                    CallType = room.CallType,
                    IsActive = room.IsActive,
                    StartedAt = room.StartedAt,
                    MaxParticipants = room.MaxParticipants
                }
            });
        }

        /// <summary>
        /// Người dùng nhập mã phòng để tham gia nhóm gọi video
        /// </summary>
        [HttpPost("join-room")]
        public async Task<IActionResult> JoinRoom([FromBody] JoinRoomDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var room = await _context.VideoMeetingRooms
                .Include(r => r.HostEmployee)
                .Include(r => r.Department)
                .FirstOrDefaultAsync(r => r.RoomCode == dto.RoomCode && r.IsActive);

            if (room == null)
            {
                return NotFound(new 
                { 
                    status = false, 
                    message = "Mã phòng không tồn tại hoặc cuộc gọi đã kết thúc. Vui lòng kiểm tra lại mã phòng!" 
                });
            }

            var (currentUserEmpId, currentUserName) = await GetCurrentUserInfoAsync();

            string participantName = !string.IsNullOrWhiteSpace(dto.ParticipantName)
                ? dto.ParticipantName
                : (!string.IsNullOrWhiteSpace(currentUserName) ? currentUserName : "Thành viên");

            int? empId = dto.EmployeeId ?? currentUserEmpId;
            if (empId.HasValue)
            {
                var emp = await _context.Employees.FindAsync(empId.Value);
                if (emp != null) participantName = emp.FullName;
            }

            return Ok(new
            {
                status = true,
                message = "Tham gia phòng gọi thành công!",
                roomCode = room.RoomCode,
                roomTitle = room.RoomTitle,
                hostName = room.HostName ?? room.HostEmployee?.FullName ?? "Chủ phòng",
                callType = room.CallType,
                participantName = participantName,
                peerId = dto.PeerId ?? Guid.NewGuid().ToString("N")
            });
        }

        /// <summary>
        /// Tra cứu thông tin phòng họp theo mã phòng
        /// </summary>
        [HttpGet("room/{roomCode}")]
        public async Task<IActionResult> GetRoomByCode(string roomCode)
        {
            var room = await _context.VideoMeetingRooms
                .Include(r => r.HostEmployee)
                .Include(r => r.Department)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode && r.IsActive);

            if (room == null)
            {
                return NotFound(new { status = false, message = "Phòng gọi không tồn tại hoặc đã kết thúc" });
            }

            return Ok(new
            {
                status = true,
                data = new VideoRoomResponseDto
                {
                    Id = room.Id,
                    RoomCode = room.RoomCode,
                    RoomTitle = room.RoomTitle,
                    HostEmployeeId = room.HostEmployeeId,
                    HostName = room.HostName ?? room.HostEmployee?.FullName ?? "Chủ phòng",
                    DepartmentName = room.Department?.DepartmentName,
                    CallType = room.CallType,
                    IsActive = room.IsActive,
                    StartedAt = room.StartedAt,
                    MaxParticipants = room.MaxParticipants
                }
            });
        }

        /// <summary>
        /// Chuyển tiếp tín hiệu WebRTC (Offer, Answer, ICE Candidate) giữa các thiết bị trong phòng
        /// </summary>
        [HttpPost("signal")]
        public async Task<IActionResult> RelaySignal([FromBody] WebRtcSignalDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _hubContext.Clients.Group($"Room_{dto.RoomCode}").SendAsync("ReceiveSignal", dto);

            return Ok(new { status = true, message = "Tín hiệu WebRTC đã được chuyển tiếp" });
        }

        /// <summary>
        /// Kết thúc cuộc gọi / đóng phòng họp bằng mã phòng
        /// </summary>
        [HttpPost("end-room/{roomCode}")]
        public async Task<IActionResult> EndRoom(string roomCode)
        {
            var room = await _context.VideoMeetingRooms.FirstOrDefaultAsync(r => r.RoomCode == roomCode && r.IsActive);
            if (room == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy phòng gọi đang hoạt động" });
            }

            room.IsActive = false;
            room.EndedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"Room_{roomCode}").SendAsync("RoomEnded", new { RoomCode = roomCode });

            return Ok(new { status = true, message = "Đã kết thúc phòng gọi video nhóm" });
        }

        /// <summary>
        /// Lấy danh sách các phòng họp / cuộc gọi nhóm đang diễn ra
        /// </summary>
        [HttpGet("active-rooms")]
        public async Task<IActionResult> GetActiveRooms()
        {
            var rooms = await _context.VideoMeetingRooms
                .Include(r => r.HostEmployee)
                .Include(r => r.Department)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => new VideoRoomResponseDto
                {
                    Id = r.Id,
                    RoomCode = r.RoomCode,
                    RoomTitle = r.RoomTitle,
                    HostEmployeeId = r.HostEmployeeId,
                    HostName = r.HostName ?? (r.HostEmployee != null ? r.HostEmployee.FullName : "Chủ phòng"),
                    DepartmentName = r.Department != null ? r.Department.DepartmentName : null,
                    CallType = r.CallType,
                    IsActive = r.IsActive,
                    StartedAt = r.StartedAt,
                    MaxParticipants = r.MaxParticipants
                })
                .ToListAsync();

            return Ok(new { status = true, total = rooms.Count, data = rooms });
        }
    }
}
