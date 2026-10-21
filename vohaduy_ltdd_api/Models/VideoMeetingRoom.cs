using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.Models
{
    public class VideoMeetingRoom
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoomCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RoomTitle { get; set; } = string.Empty;

        public int? HostEmployeeId { get; set; }
        public virtual Employee? HostEmployee { get; set; }

        [MaxLength(100)]
        public string? HostName { get; set; }

        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        [MaxLength(30)]
        public string CallType { get; set; } = "GroupVideo"; // GroupVideo, GroupAudio, OneOnOne

        public bool IsActive { get; set; } = true;

        public int MaxParticipants { get; set; } = 20;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndedAt { get; set; }
    }
}
