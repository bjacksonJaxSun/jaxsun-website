using JaxSun.Web.Models;

namespace JaxSun.Web.Services
{
    public interface ITimeTrackingService
    {
        // Time Entry Management
        Task<TimeEntry> StartTimerAsync(string userId, int projectId, int? categoryId = null, string? description = null);
        Task<TimeEntry> StopTimerAsync(int timeEntryId);
        Task<TimeEntry> CreateManualEntryAsync(string userId, int projectId, DateTime startTime, DateTime endTime, int? categoryId = null, string? description = null);
        Task<TimeEntry> UpdateTimeEntryAsync(int timeEntryId, DateTime? startTime = null, DateTime? endTime = null, int? categoryId = null, string? description = null);
        Task<bool> DeleteTimeEntryAsync(int timeEntryId);

        // Time Entry Retrieval
        Task<TimeEntry?> GetTimeEntryByIdAsync(int timeEntryId);
        Task<IEnumerable<TimeEntry>> GetTimeEntriesForUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<TimeEntry>> GetTimeEntriesForProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
        Task<TimeEntry?> GetActiveTimerForUserAsync(string userId);
        Task<IEnumerable<TimeEntry>> GetActiveTimersAsync();

        // Time Categories
        Task<IEnumerable<TimeCategory>> GetAllCategoriesAsync();
        Task<TimeCategory?> GetCategoryByIdAsync(int categoryId);
        Task<TimeCategory> CreateCategoryAsync(string name, string? description = null, string color = "#007bff");
        Task<TimeCategory> UpdateCategoryAsync(int categoryId, string? name = null, string? description = null, string? color = null);
        Task<bool> DeleteCategoryAsync(int categoryId);

        // Statistics and Analytics
        Task<decimal> GetTotalHoursForUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalHoursForProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, decimal>> GetHoursByDateAsync(string userId, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, decimal>> GetHoursByCategoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, decimal>> GetHoursByProjectAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

        // Validation
        Task<bool> ValidateTimeEntryAsync(DateTime startTime, DateTime? endTime, string userId);
        Task<bool> HasOverlappingEntriesAsync(string userId, DateTime startTime, DateTime? endTime, int? excludeEntryId = null);
    }
}