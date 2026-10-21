using System.ComponentModel.DataAnnotations;

namespace vohaduy_ltdd_api.DTOs
{
    public class CreateDepartmentDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }

        private string? _deptCode;
        public string DepartmentCode 
        { 
            get => !string.IsNullOrWhiteSpace(_deptCode) ? _deptCode : (Code ?? string.Empty); 
            set => _deptCode = value; 
        }

        private string? _deptName;
        public string DepartmentName 
        { 
            get => !string.IsNullOrWhiteSpace(_deptName) ? _deptName : (Name ?? string.Empty); 
            set => _deptName = value; 
        }

        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? ManagerName { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string? Name { get; set; }
        private string? _deptName;
        public string? DepartmentName
        {
            get => !string.IsNullOrWhiteSpace(_deptName) ? _deptName : Name;
            set => _deptName = value;
        }

        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? ManagerName { get; set; }
    }

    public class DepartmentResponseDto
    {
        public int Id { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Code => DepartmentCode;
        public string Name => DepartmentName;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? ManagerName { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
