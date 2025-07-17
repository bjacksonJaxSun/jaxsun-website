using JaxSun.Web.Data;
using JaxSun.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace JaxSun.Web.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(ApplicationDbContext context, ILogger<ProjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Project Management

        public async Task<Project> CreateProjectAsync(string name, string? description, decimal estimatedHours, decimal? budget, string createdById, int? ideaSubmissionId = null)
        {
            // Validate unique name
            if (!await IsProjectNameUniqueAsync(name))
            {
                throw new ArgumentException("A project with this name already exists.", nameof(name));
            }

            var project = new Project
            {
                Name = name,
                Description = description,
                EstimatedHours = estimatedHours,
                Budget = budget,
                CreatedById = createdById,
                IdeaSubmissionId = ideaSubmissionId,
                Status = ProjectStatus.Planning,
                CreatedDate = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created project {ProjectName} by user {UserId}", name, createdById);
            return project;
        }

        public async Task<Project> UpdateProjectAsync(int projectId, string? name = null, string? description = null, 
            ProjectStatus? status = null, decimal? estimatedHours = null, decimal? budget = null, 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                throw new ArgumentException("Project not found.", nameof(projectId));
            }

            // Validate unique name if changing
            if (name != null && name != project.Name && !await IsProjectNameUniqueAsync(name, projectId))
            {
                throw new ArgumentException("A project with this name already exists.", nameof(name));
            }

            // Update fields
            if (name != null) project.Name = name;
            if (description != null) project.Description = description;
            if (status.HasValue) project.Status = status.Value;
            if (estimatedHours.HasValue) project.EstimatedHours = estimatedHours.Value;
            if (budget.HasValue) project.Budget = budget.Value;
            if (startDate.HasValue) project.StartDate = startDate.Value;
            if (endDate.HasValue) project.EndDate = endDate.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated project {ProjectId}", projectId);
            return project;
        }

        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.TimeEntries)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return false;
            }

            // Check if project has time entries
            if (project.TimeEntries.Any())
            {
                throw new InvalidOperationException("Cannot delete project with existing time entries. Consider canceling the project instead.");
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted project {ProjectId}", projectId);
            return true;
        }

        #endregion

        #region Project Retrieval

        public async Task<Project?> GetProjectByIdAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.IdeaSubmission)
                .Include(p => p.TimeEntries)
                    .ThenInclude(te => te.User)
                .Include(p => p.TimeEntries)
                    .ThenInclude(te => te.Category)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.TimeEntries)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(ProjectStatus status)
        {
            return await _context.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.TimeEntries)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsForUserAsync(string userId)
        {
            return await _context.Projects
                .Include(p => p.TimeEntries)
                .Where(p => p.CreatedById == userId || p.TimeEntries.Any(te => te.UserId == userId))
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.CreatedBy)
                .Include(p => p.TimeEntries)
                .Where(p => p.Status == ProjectStatus.Active)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        #endregion

        #region Project Analytics

        public async Task<decimal> GetProjectProgressPercentageAsync(int projectId)
        {
            var project = await GetProjectByIdAsync(projectId);
            if (project == null || project.EstimatedHours == 0)
            {
                return 0;
            }

            var percentage = (project.TotalHoursLogged / project.EstimatedHours) * 100;
            return Math.Min(percentage, 100); // Cap at 100%
        }

        public async Task<TimeSpan> GetProjectDurationAsync(int projectId)
        {
            var project = await GetProjectByIdAsync(projectId);
            if (project == null || !project.StartDate.HasValue)
            {
                return TimeSpan.Zero;
            }

            var endDate = project.EndDate ?? DateTime.UtcNow;
            return endDate - project.StartDate.Value;
        }

        public async Task<decimal> GetProjectBudgetUtilizationAsync(int projectId)
        {
            var project = await GetProjectByIdAsync(projectId);
            if (project == null || !project.Budget.HasValue || project.Budget.Value == 0)
            {
                return 0;
            }

            // This is a simplified calculation - in reality you'd need hourly rates
            // For now, we'll assume budget is in hours or use a standard rate
            var utilizationPercentage = (project.TotalHoursLogged / project.EstimatedHours) * 100;
            return Math.Min(utilizationPercentage, 100);
        }

        public async Task<bool> IsProjectOverBudgetAsync(int projectId)
        {
            var project = await GetProjectByIdAsync(projectId);
            return project?.IsOverBudget ?? false;
        }

        public async Task<IEnumerable<Project>> GetOverBudgetProjectsAsync()
        {
            var projects = await GetAllProjectsAsync();
            return projects.Where(p => p.IsOverBudget).ToList();
        }

        #endregion

        #region Project Time Entries

        public async Task<IEnumerable<TimeEntry>> GetProjectTimeEntriesAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Category)
                .Where(te => te.ProjectId == projectId);

            if (startDate.HasValue)
            {
                query = query.Where(te => te.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(te => te.StartTime <= endDate.Value);
            }

            return await query
                .OrderByDescending(te => te.StartTime)
                .ToListAsync();
        }

        public async Task<decimal> GetProjectTotalHoursAsync(int projectId)
        {
            var project = await GetProjectByIdAsync(projectId);
            return project?.TotalHoursLogged ?? 0;
        }

        public async Task<Dictionary<string, decimal>> GetProjectHoursByUserAsync(int projectId)
        {
            var entries = await _context.TimeEntries
                .Include(te => te.User)
                .Where(te => te.ProjectId == projectId && te.EndTime.HasValue)
                .ToListAsync();

            return entries
                .GroupBy(te => te.User.UserName ?? te.User.Email ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(te => te.HoursWorked));
        }

        public async Task<Dictionary<string, decimal>> GetProjectHoursByCategoryAsync(int projectId)
        {
            var entries = await _context.TimeEntries
                .Include(te => te.Category)
                .Where(te => te.ProjectId == projectId && te.EndTime.HasValue)
                .ToListAsync();

            return entries
                .GroupBy(te => te.Category?.Name ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Sum(te => te.HoursWorked));
        }

        #endregion

        #region Project-Idea Integration

        public async Task<Project> CreateProjectFromIdeaAsync(int ideaSubmissionId, string name, decimal estimatedHours, string createdById)
        {
            var ideaSubmission = await _context.IdeaSubmissions.FindAsync(ideaSubmissionId);
            if (ideaSubmission == null)
            {
                throw new ArgumentException("Idea submission not found.", nameof(ideaSubmissionId));
            }

            var description = $"Project created from idea submission by {ideaSubmission.Name}.\n\n" +
                            $"Original idea: {ideaSubmission.IdeaDescription}\n\n" +
                            $"Market opportunity: {ideaSubmission.MarketOpportunity}";

            return await CreateProjectAsync(name, description, estimatedHours, null, createdById, ideaSubmissionId);
        }

        public async Task<IEnumerable<Project>> GetProjectsFromIdeasAsync()
        {
            return await _context.Projects
                .Include(p => p.IdeaSubmission)
                .Include(p => p.CreatedBy)
                .Where(p => p.IdeaSubmissionId.HasValue)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        #endregion

        #region Project Status Management

        public async Task<Project> StartProjectAsync(int projectId)
        {
            var project = await UpdateProjectAsync(projectId, status: ProjectStatus.Active, startDate: DateTime.UtcNow);
            _logger.LogInformation("Started project {ProjectId}", projectId);
            return project;
        }

        public async Task<Project> PauseProjectAsync(int projectId)
        {
            var project = await UpdateProjectAsync(projectId, status: ProjectStatus.OnHold);
            _logger.LogInformation("Paused project {ProjectId}", projectId);
            return project;
        }

        public async Task<Project> CompleteProjectAsync(int projectId)
        {
            var project = await UpdateProjectAsync(projectId, status: ProjectStatus.Completed, endDate: DateTime.UtcNow);
            _logger.LogInformation("Completed project {ProjectId}", projectId);
            return project;
        }

        public async Task<Project> CancelProjectAsync(int projectId)
        {
            var project = await UpdateProjectAsync(projectId, status: ProjectStatus.Cancelled, endDate: DateTime.UtcNow);
            _logger.LogInformation("Cancelled project {ProjectId}", projectId);
            return project;
        }

        #endregion

        #region Validation

        public async Task<bool> CanUserAccessProjectAsync(string userId, int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.TimeEntries)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return false;
            }

            // User can access if they created the project or have time entries on it
            return project.CreatedById == userId || project.TimeEntries.Any(te => te.UserId == userId);
        }

        public async Task<bool> IsProjectNameUniqueAsync(string name, int? excludeProjectId = null)
        {
            var query = _context.Projects.Where(p => p.Name.ToLower() == name.ToLower());

            if (excludeProjectId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProjectId.Value);
            }

            return !await query.AnyAsync();
        }

        #endregion
    }
}