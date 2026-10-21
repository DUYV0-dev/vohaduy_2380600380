using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace vohaduy_ltdd_api.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int SenderId { get; set; }
        public virtual Employee? Sender { get; set; }

        public int? ReceiverId { get; set; }
        public virtual Employee? Receiver { get; set; }

        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        [MaxLength(100)]
        public string? RoomId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [MaxLength(30)]
        public string MessageType { get; set; } = "Text"; // Text, Image, File, CallNotice

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
