using JaxSun.Web.Data;
using JaxSun.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace JaxSun.Web.Services
{
    public class TimeTrackingService : ITimeTrackingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TimeTrackingService> _logger;

        public TimeTrackingService(ApplicationDbContext context, ILogger<TimeTrackingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Time Entry Management

        public async Task<TimeEntry> StartTimerAsync(string userId, int projectId, int? categoryId = null, string? description = null)
        {
            // Check if user already has an active timer
            var activeTimer = await GetActiveTimerForUserAsync(userId);
            if (activeTimer != null)
            {
                throw new InvalidOperationException("User already has an active timer running.");
            }

            // Validate project exists
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                throw new ArgumentException("Project not found.", nameof(projectId));
            }

            var timeEntry = new TimeEntry
            {
                UserId = userId,
                ProjectId = projectId,
                CategoryId = categoryId,
                StartTime = DateTime.UtcNow,
                Description = description,
                IsManualEntry = false,
                CreatedDate = DateTime.UtcNow
            };

            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Started timer for user {UserId} on project {ProjectId}", userId, projectId);
            return timeEntry;
        }

        public async Task<TimeEntry> StopTimerAsync(int timeEntryId)
        {
            var timeEntry = await _context.TimeEntries.FindAsync(timeEntryId);
            if (timeEntry == null)
            {
                throw new ArgumentException("Time entry not found.", nameof(timeEntryId));
            }

            if (timeEntry.EndTime.HasValue)
            {
                throw new InvalidOperationException("Timer is already stopped.");
            }

            timeEntry.EndTime = DateTime.UtcNow;
            timeEntry.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Stopped timer {TimeEntryId} for user {UserId}", timeEntryId, timeEntry.UserId);
            return timeEntry;
        }

        public async Task<TimeEntry> CreateManualEntryAsync(string userId, int projectId, DateTime startTime, DateTime endTime, int? categoryId = null, string? description = null)
        {
            // Validation
            if (startTime >= endTime)
            {
                throw new ArgumentException("Start time must be before end time.");
            }

            if (!await ValidateTimeEntryAsync(startTime, endTime, userId))
            {
                throw new InvalidOperationException("Time entry overlaps with existing entries.");
            }

            var timeEntry = new TimeEntry
            {
                UserId = userId,
                ProjectId = projectId,
                CategoryId = categoryId,
                StartTime = startTime,
                EndTime = endTime,
                Description = description,
                IsManualEntry = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created manual time entry for user {UserId} on project {ProjectId}", userId, projectId);
            return timeEntry;
        }

        public async Task<TimeEntry> UpdateTimeEntryAsync(int timeEntryId, DateTime? startTime = null, DateTime? endTime = null, int? categoryId = null, string? description = null)
        {
            var timeEntry = await _context.TimeEntries.FindAsync(timeEntryId);
            if (timeEntry == null)
            {
                throw new ArgumentException("Time entry not found.", nameof(timeEntryId));
            }

            var newStartTime = startTime ?? timeEntry.StartTime;
            var newEndTime = endTime ?? timeEntry.EndTime;

            // Validate time range if either time is being updated
            if (startTime.HasValue || endTime.HasValue)
            {
                if (newEndTime.HasValue && newStartTime >= newEndTime)
                {
                    throw new ArgumentException("Start time must be before end time.");
                }

                if (await HasOverlappingEntriesAsync(timeEntry.UserId, newStartTime, newEndTime, timeEntryId))
                {
                    throw new InvalidOperationException("Updated time entry would overlap with existing entries.");
                }
            }

            // Update fields
            if (startTime.HasValue) timeEntry.StartTime = startTime.Value;
            if (endTime.HasValue) timeEntry.EndTime = endTime.Value;
            if (categoryId.HasValue) timeEntry.CategoryId = categoryId.Value;
            if (description != null) timeEntry.Description = description;

            timeEntry.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated time entry {TimeEntryId}", timeEntryId);
            return timeEntry;
        }

        public async Task<bool> DeleteTimeEntryAsync(int timeEntryId)
        {
            var timeEntry = await _context.TimeEntries.FindAsync(timeEntryId);
            if (timeEntry == null)
            {
                return false;
            }

            _context.TimeEntries.Remove(timeEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted time entry {TimeEntryId}", timeEntryId);
            return true;
        }

        #endregion

        #region Time Entry Retrieval

        public async Task<TimeEntry?> GetTimeEntryByIdAsync(int timeEntryId)
        {
            return await _context.TimeEntries
                .Include(te => te.Project)
                .Include(te => te.Category)
                .Include(te => te.User)
                .FirstOrDefaultAsync(te => te.Id == timeEntryId);
        }

        public async Task<IEnumerable<TimeEntry>> GetTimeEntriesForUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.TimeEntries
                .Include(te => te.Project)
                .Include(te => te.Category)
                .Where(te => te.UserId == userId);

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

        public async Task<IEnumerable<TimeEntry>> GetTimeEntriesForProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
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

        public async Task<TimeEntry?> GetActiveTimerForUserAsync(string userId)
        {
            return await _context.TimeEntries
                .Include(te => te.Project)
                .Include(te => te.Category)
                .FirstOrDefaultAsync(te => te.UserId == userId && !te.EndTime.HasValue);
        }

        public async Task<IEnumerable<TimeEntry>> GetActiveTimersAsync()
        {
            return await _context.TimeEntries
                .Include(te => te.User)
                .Include(te => te.Project)
                .Include(te => te.Category)
                .Where(te => !te.EndTime.HasValue)
                .OrderBy(te => te.StartTime)
                .ToListAsync();
        }

        #endregion

        #region Time Categories

        public async Task<IEnumerable<TimeCategory>> GetAllCategoriesAsync()
        {
            return await _context.TimeCategories
                .Where(tc => tc.IsActive)
                .OrderBy(tc => tc.Name)
                .ToListAsync();
        }

        public async Task<TimeCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.TimeCategories.FindAsync(categoryId);
        }

        public async Task<TimeCategory> CreateCategoryAsync(string name, string? description = null, string color = "#007bff")
        {
            var category = new TimeCategory
            {
                Name = name,
                Description = description,
                Color = color,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.TimeCategories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created time category {CategoryName}", name);
            return category;
        }

        public async Task<TimeCategory> UpdateCategoryAsync(int categoryId, string? name = null, string? description = null, string? color = null)
        {
            var category = await _context.TimeCategories.FindAsync(categoryId);
            if (category == null)
            {
                throw new ArgumentException("Category not found.", nameof(categoryId));
            }

            if (name != null) category.Name = name;
            if (description != null) category.Description = description;
            if (color != null) category.Color = color;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated time category {CategoryId}", categoryId);
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.TimeCategories.FindAsync(categoryId);
            if (category == null)
            {
                return false;
            }

            // Soft delete - just mark as inactive
            category.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deactivated time category {CategoryId}", categoryId);
            return true;
        }

        #endregion

        #region Statistics and Analytics

        public async Task<decimal> GetTotalHoursForUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.TimeEntries
                .Where(te => te.UserId == userId && te.EndTime.HasValue);

            if (startDate.HasValue)
            {
                query = query.Where(te => te.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(te => te.StartTime <= endDate.Value);
            }

            var entries = await query.ToListAsync();
            return entries.Sum(te => te.HoursWorked);
        }

        public async Task<decimal> GetTotalHoursForProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.TimeEntries
                .Where(te => te.ProjectId == projectId && te.EndTime.HasValue);

            if (startDate.HasValue)
            {
                query = query.Where(te => te.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(te => te.StartTime <= endDate.Value);
            }

            var entries = await query.ToListAsync();
            return entries.Sum(te => te.HoursWorked);
        }

        public async Task<Dictionary<string, decimal>> GetHoursByDateAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var entries = await _context.TimeEntries
                .Where(te => te.UserId == userId && te.EndTime.HasValue &&
                           te.StartTime >= startDate && te.StartTime <= endDate)
                .ToListAsync();

            return entries
                .GroupBy(te => te.StartTime.Date.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.Sum(te => te.HoursWorked));
        }

        public async Task<Dictionary<string, decimal>> GetHoursByCategoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.TimeEntries
                .Include(te => te.Category)
                .Where(te => te.UserId == userId && te.EndTime.HasValue);

            if (startDate.HasValue)
            {
                query = query.Where(te => te.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(te => te.StartTime <= endDate.Value);
            }

            var entries = await query.ToListAsync();

            return entries
                .GroupBy(te => te.Category?.Name ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Sum(te => te.HoursWorked));
        }

        public async Task<Dictionary<string, decimal>> GetHoursByProjectAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.TimeEntries
                .Include(te => te.Project)
                .Where(te => te.UserId == userId && te.EndTime.HasValue);

            if (startDate.HasValue)
            {
                query = query.Where(te => te.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(te => te.StartTime <= endDate.Value);
            }

            var entries = await query.ToListAsync();

            return entries
                .GroupBy(te => te.Project.Name)
                .ToDictionary(g => g.Key, g => g.Sum(te => te.HoursWorked));
        }

        #endregion

        #region Validation

        public async Task<bool> ValidateTimeEntryAsync(DateTime startTime, DateTime? endTime, string userId)
        {
            return !await HasOverlappingEntriesAsync(userId, startTime, endTime);
        }

        public async Task<bool> HasOverlappingEntriesAsync(string userId, DateTime startTime, DateTime? endTime, int? excludeEntryId = null)
        {
            var query = _context.TimeEntries
                .Where(te => te.UserId == userId);

            if (excludeEntryId.HasValue)
            {
                query = query.Where(te => te.Id != excludeEntryId.Value);
            }

            var existingEntries = await query.ToListAsync();

            foreach (var entry in existingEntries)
            {
                var entryEnd = entry.EndTime ?? DateTime.UtcNow; // Treat active timers as running until now

                // Check for overlap
                var newEnd = endTime ?? DateTime.UtcNow;

                if (startTime < entryEnd && newEnd > entry.StartTime)
                {
                    return true; // Overlap found
                }
            }

            return false; // No overlap
        }

        #endregion
    }
}