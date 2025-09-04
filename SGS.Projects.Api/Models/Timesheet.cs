using System.ComponentModel.DataAnnotations;

namespace SGS.Projects.Api.Models
{
    public class Timesheet
    {
        public int? DocEntry { get; set; }
        
        [Required]
        public string DocNum { get; set; } = string.Empty;
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public string EmployeeId { get; set; } = string.Empty;
        
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        
        [Required]
        public string ActivityId { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 24)]
        public decimal Hours { get; set; }
        
        public string? Description { get; set; }
        
        public string? Status { get; set; }
        
        public DateTime? CreatedDate { get; set; }
        
        public string? CreatedBy { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
        
        public string? ModifiedBy { get; set; }
    }

    public class TimesheetCreateRequest
    {
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public string EmployeeId { get; set; } = string.Empty;
        
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        
        [Required]
        public string ActivityId { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 24)]
        public decimal Hours { get; set; }
        
        public string? Description { get; set; }
    }

    public class TimesheetUpdateRequest
    {
        [Required]
        public int DocEntry { get; set; }
        
        public DateTime? Date { get; set; }
        
        public string? EmployeeId { get; set; }
        
        public string? ProjectId { get; set; }
        
        public string? ActivityId { get; set; }
        
        [Range(0, 24)]
        public decimal? Hours { get; set; }
        
        public string? Description { get; set; }
    }
}
