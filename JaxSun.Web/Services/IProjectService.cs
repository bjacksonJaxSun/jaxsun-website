using JaxSun.Web.Models;

namespace JaxSun.Web.Services
{
    public interface IProjectService
    {
        // Project Management
        Task<Project> CreateProjectAsync(string name, string? description, decimal estimatedHours, decimal? budget, string createdById, int? ideaSubmissionId = null);
        Task<Project> UpdateProjectAsync(int projectId, string? name = null, string? description = null, 
            ProjectStatus? status = null, decimal? estimatedHours = null, decimal? budget = null, 
            DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> DeleteProjectAsync(int projectId);

        // Project Retrieval
        Task<Project?> GetProjectByIdAsync(int projectId);
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<IEnumerable<Project>> GetProjectsByStatusAsync(ProjectStatus status);
        Task<IEnumerable<Project>> GetProjectsForUserAsync(string userId);
        Task<IEnumerable<Project>> GetActiveProjectsAsync();

        // Project Analytics
        Task<decimal> GetProjectProgressPercentageAsync(int projectId);
        Task<TimeSpan> GetProjectDurationAsync(int projectId);
        Task<decimal> GetProjectBudgetUtilizationAsync(int projectId);
        Task<bool> IsProjectOverBudgetAsync(int projectId);
        Task<IEnumerable<Project>> GetOverBudgetProjectsAsync();

        // Project Time Entries
        Task<IEnumerable<TimeEntry>> GetProjectTimeEntriesAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetProjectTotalHoursAsync(int projectId);
        Task<Dictionary<string, decimal>> GetProjectHoursByUserAsync(int projectId);
        Task<Dictionary<string, decimal>> GetProjectHoursByCategoryAsync(int projectId);

        // Project-Idea Integration
        Task<Project> CreateProjectFromIdeaAsync(int ideaSubmissionId, string name, decimal estimatedHours, string createdById);
        Task<IEnumerable<Project>> GetProjectsFromIdeasAsync();

        // Project Status Management
        Task<Project> StartProjectAsync(int projectId);
        Task<Project> PauseProjectAsync(int projectId);
        Task<Project> CompleteProjectAsync(int projectId);
        Task<Project> CancelProjectAsync(int projectId);

        // Validation
        Task<bool> CanUserAccessProjectAsync(string userId, int projectId);
        Task<bool> IsProjectNameUniqueAsync(string name, int? excludeProjectId = null);
    }
}