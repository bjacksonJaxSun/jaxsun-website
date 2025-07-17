using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using JaxSun.Web.Services;
using JaxSun.Web.Models;
using JaxSun.Web.Models.ViewModels;

namespace JaxSun.Web.Controllers
{
    [Authorize(Roles = "Admin,Partner")]
    public class TimeTrackingController : Controller
    {
        private readonly ITimeTrackingService _timeTrackingService;
        private readonly IProjectService _projectService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<TimeTrackingController> _logger;

        public TimeTrackingController(
            ITimeTrackingService timeTrackingService,
            IProjectService projectService,
            UserManager<IdentityUser> userManager,
            ILogger<TimeTrackingController> logger)
        {
            _timeTrackingService = timeTrackingService;
            _projectService = projectService;
            _userManager = userManager;
            _logger = logger;
        }

        private async Task<string> GetCurrentUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id ?? throw new UnauthorizedAccessException("User not found");
        }

        public async Task<IActionResult> Index()
        {
            var userId = await GetCurrentUserIdAsync();
            var activeTimer = await _timeTrackingService.GetActiveTimerForUserAsync(userId);
            var recentEntries = await _timeTrackingService.GetTimeEntriesForUserAsync(userId);
            var projects = await _projectService.GetActiveProjectsAsync();
            var categories = await _timeTrackingService.GetAllCategoriesAsync();

            var viewModel = new TimeTrackingIndexViewModel
            {
                ActiveTimer = activeTimer,
                RecentEntries = recentEntries.Take(10).ToList(),
                Projects = projects.ToList(),
                Categories = categories.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> StartTimer(int projectId, int? categoryId, string? description)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var timeEntry = await _timeTrackingService.StartTimerAsync(userId, projectId, categoryId, description);
                
                TempData["Success"] = "Timer started successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> StopTimer(int timeEntryId)
        {
            try
            {
                var timeEntry = await _timeTrackingService.StopTimerAsync(timeEntryId);
                TempData["Success"] = $"Timer stopped! Logged {timeEntry.HoursWorked:F2} hours.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ManualEntry()
        {
            var projects = await _projectService.GetActiveProjectsAsync();
            var categories = await _timeTrackingService.GetAllCategoriesAsync();

            var viewModel = new ManualEntryViewModel
            {
                Projects = projects.ToList(),
                Categories = categories.ToList(),
                StartTime = DateTime.Now.Date.AddHours(9), // Default to 9 AM today
                EndTime = DateTime.Now.Date.AddHours(17)   // Default to 5 PM today
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ManualEntry(ManualEntryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var projects = await _projectService.GetActiveProjectsAsync();
                var categories = await _timeTrackingService.GetAllCategoriesAsync();
                model.Projects = projects.ToList();
                model.Categories = categories.ToList();
                return View(model);
            }

            try
            {
                var userId = await GetCurrentUserIdAsync();
                var timeEntry = await _timeTrackingService.CreateManualEntryAsync(
                    userId, model.ProjectId, model.StartTime, model.EndTime, 
                    model.CategoryId, model.Description);

                TempData["Success"] = $"Manual entry created successfully! Logged {timeEntry.HoursWorked:F2} hours.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var projects = await _projectService.GetActiveProjectsAsync();
                var categories = await _timeTrackingService.GetAllCategoriesAsync();
                model.Projects = projects.ToList();
                model.Categories = categories.ToList();
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var timeEntry = await _timeTrackingService.GetTimeEntryByIdAsync(id);
            if (timeEntry == null)
            {
                return NotFound();
            }

            var userId = await GetCurrentUserIdAsync();
            if (timeEntry.UserId != userId)
            {
                return Forbid();
            }

            var projects = await _projectService.GetActiveProjectsAsync();
            var categories = await _timeTrackingService.GetAllCategoriesAsync();

            var viewModel = new EditTimeEntryViewModel
            {
                Id = timeEntry.Id,
                ProjectId = timeEntry.ProjectId,
                CategoryId = timeEntry.CategoryId,
                StartTime = timeEntry.StartTime,
                EndTime = timeEntry.EndTime ?? DateTime.Now,
                Description = timeEntry.Description,
                Projects = projects.ToList(),
                Categories = categories.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditTimeEntryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var projects = await _projectService.GetActiveProjectsAsync();
                var categories = await _timeTrackingService.GetAllCategoriesAsync();
                model.Projects = projects.ToList();
                model.Categories = categories.ToList();
                return View(model);
            }

            try
            {
                await _timeTrackingService.UpdateTimeEntryAsync(
                    model.Id, model.StartTime, model.EndTime, 
                    model.CategoryId, model.Description);

                TempData["Success"] = "Time entry updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var projects = await _projectService.GetActiveProjectsAsync();
                var categories = await _timeTrackingService.GetAllCategoriesAsync();
                model.Projects = projects.ToList();
                model.Categories = categories.ToList();
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var timeEntry = await _timeTrackingService.GetTimeEntryByIdAsync(id);
                if (timeEntry == null)
                {
                    TempData["Error"] = "Time entry not found.";
                    return RedirectToAction(nameof(Index));
                }

                var userId = await GetCurrentUserIdAsync();
                if (timeEntry.UserId != userId)
                {
                    TempData["Error"] = "You can only delete your own time entries.";
                    return RedirectToAction(nameof(Index));
                }

                await _timeTrackingService.DeleteTimeEntryAsync(id);
                TempData["Success"] = "Time entry deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> History(DateTime? startDate, DateTime? endDate)
        {
            var userId = await GetCurrentUserIdAsync();
            
            // Default to current month if no dates provided
            startDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            endDate ??= startDate.Value.AddMonths(1).AddDays(-1);

            var timeEntries = await _timeTrackingService.GetTimeEntriesForUserAsync(userId, startDate, endDate);
            var totalHours = await _timeTrackingService.GetTotalHoursForUserAsync(userId, startDate, endDate);

            var viewModel = new TimeHistoryViewModel
            {
                TimeEntries = timeEntries.ToList(),
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalHours = totalHours
            };

            return View(viewModel);
        }

        // API endpoints for AJAX calls
        [HttpGet]
        public async Task<IActionResult> GetActiveTimer()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var activeTimer = await _timeTrackingService.GetActiveTimerForUserAsync(userId);
                
                if (activeTimer == null)
                {
                    return Json(new { hasActiveTimer = false });
                }

                var duration = DateTime.UtcNow - activeTimer.StartTime;
                return Json(new 
                { 
                    hasActiveTimer = true,
                    timeEntryId = activeTimer.Id,
                    projectName = activeTimer.Project.Name,
                    startTime = activeTimer.StartTime,
                    duration = duration.TotalMinutes
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> QuickStartTimer([FromBody] QuickTimerRequest request)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var timeEntry = await _timeTrackingService.StartTimerAsync(userId, request.ProjectId, request.CategoryId, request.Description);
                
                return Json(new { success = true, timeEntryId = timeEntry.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }

    // Helper classes for API requests
    public class QuickTimerRequest
    {
        public int ProjectId { get; set; }
        public int? CategoryId { get; set; }
        public string? Description { get; set; }
    }
}