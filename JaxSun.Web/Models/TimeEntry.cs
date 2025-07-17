using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace JaxSun.Web.Models
{
    public class TimeEntry
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ProjectId { get; set; }

        public int? CategoryId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsManualEntry { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastModifiedDate { get; set; }

        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
        public virtual Project Project { get; set; } = null!;
        public virtual TimeCategory? Category { get; set; }

        // Computed properties
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
        public decimal HoursWorked => Duration?.TotalHours > 0 ? (decimal)Duration.Value.TotalHours : 0;
        public bool IsActive => !EndTime.HasValue;
        public bool IsCompleted => EndTime.HasValue;

        // Validation
        public bool IsValid => StartTime < EndTime || !EndTime.HasValue;
    }
}