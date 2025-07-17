using System.ComponentModel.DataAnnotations;

namespace JaxSun.Web.Models.ViewModels
{
    public class ProjectIndexViewModel
    {
        public List<Project> AllProjects { get; set; } = new();
        public List<Project> UserProjects { get; set; } = new();
        public List<Project> OverBudgetProjects { get; set; } = new();
    }

    public class ProjectDetailsViewModel
    {
        public Project Project { get; set; } = null!;
        public List<TimeEntry> TimeEntries { get; set; } = new();
        public Dictionary<string, decimal> HoursByUser { get; set; } = new();
        public Dictionary<string, decimal> HoursByCategory { get; set; } = new();
        public decimal ProgressPercentage { get; set; }
    }

    public class CreateProjectViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Project Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Range(0.1, 10000)]
        [Display(Name = "Estimated Hours")]
        public decimal EstimatedHours { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Budget")]
        public decimal? Budget { get; set; }

        [Display(Name = "Create from Idea Submission")]
        public int? IdeaSubmissionId { get; set; }

        // For dropdown
        public List<IdeaSubmissionModel> IdeaSubmissions { get; set; } = new();
    }

    public class EditProjectViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Project Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Status")]
        public ProjectStatus Status { get; set; }

        [Required]
        [Range(0.1, 10000)]
        [Display(Name = "Estimated Hours")]
        public decimal EstimatedHours { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Budget")]
        public decimal? Budget { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
    }

    public class ProjectAnalyticsViewModel
    {
        public List<ProjectAnalyticsData> ProjectAnalytics { get; set; } = new();
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int OverBudgetProjects { get; set; }
    }

    public class ProjectAnalyticsData
    {
        public Project Project { get; set; } = null!;
        public decimal ProgressPercentage { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsOverBudget { get; set; }
    }
}