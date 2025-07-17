using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace JaxSun.Web.Models
{
    public enum ProjectStatus
    {
        Planning,
        Active,
        OnHold,
        Completed,
        Cancelled
    }

    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        [Range(0, double.MaxValue)]
        public decimal EstimatedHours { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Budget { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public string CreatedById { get; set; } = string.Empty;

        // Navigation properties
        public virtual IdentityUser CreatedBy { get; set; } = null!;
        public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

        // Optional: Link to idea submission if project originated from one
        public int? IdeaSubmissionId { get; set; }
        public virtual IdeaSubmissionModel? IdeaSubmission { get; set; }

        // Computed properties
        public decimal TotalHoursLogged => TimeEntries?.Sum(te => te.EndTime.HasValue ? (decimal)(te.EndTime.Value - te.StartTime).TotalHours : 0m) ?? 0m;
        public decimal RemainingHours => EstimatedHours - TotalHoursLogged;
        public bool IsOverBudget => TotalHoursLogged > EstimatedHours;
    }
}