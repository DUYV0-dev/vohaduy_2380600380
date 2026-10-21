using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.DTOs
{
    public class CreateMeetingRoomDto
    {
        [Required(ErrorMessage = "Tiêu đề phòng họp/gọi nhóm không được để trống")]
        public string RoomTitle { get; set; } = "Cuộc gọi nhóm";

        public string? HostName { get; set; }
        public int? HostEmployeeId { get; set; }
        public int? DepartmentId { get; set; }
        public string CallType { get; set; } = "GroupVideo"; // GroupVideo, GroupAudio, OneOnOne
        public int MaxParticipants { get; set; } = 20;
    }

    public class JoinRoomDto
    {
        [Required(ErrorMessage = "Mã phòng gọi video không được để trống")]
        public string RoomCode { get; set; } = string.Empty;

        public string? ParticipantName { get; set; }
        public int? EmployeeId { get; set; }
        public string? PeerId { get; set; }
    }

    public class WebRtcSignalDto
    {
        [Required]
        public string RoomCode { get; set; } = string.Empty;

        public string SenderPeerId { get; set; } = string.Empty;
        public string? TargetPeerId { get; set; }

        [Required]
        public string SignalType { get; set; } = "offer"; // offer, answer, ice-candidate, join, leave

        [Required]
        public string SignalData { get; set; } = string.Empty; // SDP or ICE candidate JSON string
    }

    public class VideoRoomResponseDto
    {
        public int Id { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomTitle { get; set; } = string.Empty;
        public int? HostEmployeeId { get; set; }
        public string HostName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string CallType { get; set; } = "GroupVideo";
        public bool IsActive { get; set; }
        public DateTime StartedAt { get; set; }
        public int MaxParticipants { get; set; }
    }
}
