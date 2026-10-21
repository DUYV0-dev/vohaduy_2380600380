using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.DTOs
{
    public class CreateLeaveRequestDto
    {
        [Required(ErrorMessage = "Loại nghỉ phép không được để trống")]
        public string LeaveType { get; set; } = "AnnualLeave"; // AnnualLeave, SickLeave, PersonalLeave, LateArrival, EarlyLeave, BusinessTrip

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Lý do không được để trống")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReviewLeaveRequestDto
    {
        [Required(ErrorMessage = "Trạng thái phê duyệt: Approved hoặc Rejected")]
        public string Status { get; set; } = "Approved"; // Approved, Rejected

        public string? AdminComment { get; set; }
        private string? _reviewerNotes;
        public string? ReviewerNotes 
        {
            get => !string.IsNullOrWhiteSpace(_reviewerNotes) ? _reviewerNotes : AdminComment;
            set => _reviewerNotes = value;
        }
    }

    public class LeaveRequestResponseDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ReviewerName { get; set; }
        public string? ReviewerNotes { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
