using System.ComponentModel.DataAnnotations;

namespace JaxSun.Web.Models.ViewModels
{
    public class TimeTrackingDashboardViewModel
    {
        public ActiveTimerViewModel? ActiveTimer { get; set; }
        public List<TimeEntryDisplayViewModel> RecentEntries { get; set; } = new();
        public List<Project> AvailableProjects { get; set; } = new();
        public List<TimeCategory> AvailableCategories { get; set; } = new();
        public decimal TodayHours { get; set; }
        public int TodayEntries { get; set; }
        public decimal WeekHours { get; set; }
        public decimal MonthHours { get; set; }
    }

    public class ActiveTimerViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class TimeEntryDisplayViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
    }

    public class TimeTrackingIndexViewModel
    {
        public TimeEntry? ActiveTimer { get; set; }
        public List<TimeEntry> RecentEntries { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public List<TimeCategory> Categories { get; set; } = new();
    }

    public class ManualEntryViewModel
    {
        [Required]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        // For dropdowns
        public List<Project> Projects { get; set; } = new();
        public List<TimeCategory> Categories { get; set; } = new();
    }

    public class EditTimeEntryViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        // For dropdowns
        public List<Project> Projects { get; set; } = new();
        public List<TimeCategory> Categories { get; set; } = new();
    }

    public class TimeHistoryViewModel
    {
        public List<TimeEntry> TimeEntries { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalHours { get; set; }
    }
}