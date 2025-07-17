using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using JaxSun.Web.Services;
using JaxSun.Web.Models;
using JaxSun.Web.Models.ViewModels;

namespace JaxSun.Web.Controllers
{
    [Authorize(Roles = "Admin,Partner")]
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly ITimeTrackingService _timeTrackingService;
        private readonly IIdeaSubmissionService _ideaSubmissionService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            IProjectService projectService,
            ITimeTrackingService timeTrackingService,
            IIdeaSubmissionService ideaSubmissionService,
            UserManager<IdentityUser> userManager,
            ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _timeTrackingService = timeTrackingService;
            _ideaSubmissionService = ideaSubmissionService;
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
            var allProjects = await _projectService.GetAllProjectsAsync();
            var userProjects = await _projectService.GetProjectsForUserAsync(userId);

            var viewModel = new ProjectIndexViewModel
            {
                AllProjects = allProjects.ToList(),
                UserProjects = userProjects.ToList(),
                OverBudgetProjects = (await _projectService.GetOverBudgetProjectsAsync()).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var userId = await GetCurrentUserIdAsync();
            if (!await _projectService.CanUserAccessProjectAsync(userId, id))
            {
                return Forbid();
            }

            var timeEntries = await _projectService.GetProjectTimeEntriesAsync(id);
            var hoursByUser = await _projectService.GetProjectHoursByUserAsync(id);
            var hoursByCategory = await _projectService.GetProjectHoursByCategoryAsync(id);
            var progressPercentage = await _projectService.GetProjectProgressPercentageAsync(id);

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                TimeEntries = timeEntries.ToList(),
                HoursByUser = hoursByUser,
                HoursByCategory = hoursByCategory,
                ProgressPercentage = progressPercentage
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var ideaSubmissions = await _ideaSubmissionService.GetAllSubmissionsAsync();
            
            var viewModel = new CreateProjectViewModel
            {
                IdeaSubmissions = ideaSubmissions.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var ideaSubmissions = await _ideaSubmissionService.GetAllSubmissionsAsync();
                model.IdeaSubmissions = ideaSubmissions.ToList();
                return View(model);
            }

            try
            {
                var userId = await GetCurrentUserIdAsync();
                
                Project project;
                if (model.IdeaSubmissionId.HasValue)
                {
                    project = await _projectService.CreateProjectFromIdeaAsync(
                        model.IdeaSubmissionId.Value, model.Name, model.EstimatedHours, userId);
                }
                else
                {
                    project = await _projectService.CreateProjectAsync(
                        model.Name, model.Description, model.EstimatedHours, model.Budget, userId);
                }

                TempData["Success"] = "Project created successfully!";
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var ideaSubmissions = await _ideaSubmissionService.GetAllSubmissionsAsync();
                model.IdeaSubmissions = ideaSubmissions.ToList();
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var userId = await GetCurrentUserIdAsync();
            if (!await _projectService.CanUserAccessProjectAsync(userId, id))
            {
                return Forbid();
            }

            var viewModel = new EditProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                EstimatedHours = project.EstimatedHours,
                Budget = project.Budget,
                StartDate = project.StartDate,
                EndDate = project.EndDate
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (!await _projectService.CanUserAccessProjectAsync(userId, model.Id))
                {
                    return Forbid();
                }

                await _projectService.UpdateProjectAsync(
                    model.Id, model.Name, model.Description, model.Status,
                    model.EstimatedHours, model.Budget, model.StartDate, model.EndDate);

                TempData["Success"] = "Project updated successfully!";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, ProjectStatus status)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (!await _projectService.CanUserAccessProjectAsync(userId, id))
                {
                    TempData["Error"] = "You don't have access to this project.";
                    return RedirectToAction(nameof(Index));
                }

                switch (status)
                {
                    case ProjectStatus.Active:
                        await _projectService.StartProjectAsync(id);
                        TempData["Success"] = "Project started successfully!";
                        break;
                    case ProjectStatus.OnHold:
                        await _projectService.PauseProjectAsync(id);
                        TempData["Success"] = "Project paused successfully!";
                        break;
                    case ProjectStatus.Completed:
                        await _projectService.CompleteProjectAsync(id);
                        TempData["Success"] = "Project completed successfully!";
                        break;
                    case ProjectStatus.Cancelled:
                        await _projectService.CancelProjectAsync(id);
                        TempData["Success"] = "Project cancelled successfully!";
                        break;
                    default:
                        await _projectService.UpdateProjectAsync(id, status: status);
                        TempData["Success"] = "Project status updated successfully!";
                        break;
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (!await _projectService.CanUserAccessProjectAsync(userId, id))
                {
                    TempData["Error"] = "You don't have access to this project.";
                    return RedirectToAction(nameof(Index));
                }

                await _projectService.DeleteProjectAsync(id);
                TempData["Success"] = "Project deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Analytics()
        {
            var allProjects = await _projectService.GetAllProjectsAsync();
            var overBudgetProjects = await _projectService.GetOverBudgetProjectsAsync();
            
            var projectAnalytics = new List<ProjectAnalyticsData>();
            
            foreach (var project in allProjects)
            {
                var progressPercentage = await _projectService.GetProjectProgressPercentageAsync(project.Id);
                var duration = await _projectService.GetProjectDurationAsync(project.Id);
                
                projectAnalytics.Add(new ProjectAnalyticsData
                {
                    Project = project,
                    ProgressPercentage = progressPercentage,
                    Duration = duration,
                    IsOverBudget = project.IsOverBudget
                });
            }

            var viewModel = new ProjectAnalyticsViewModel
            {
                ProjectAnalytics = projectAnalytics,
                TotalProjects = allProjects.Count(),
                ActiveProjects = allProjects.Count(p => p.Status == ProjectStatus.Active),
                CompletedProjects = allProjects.Count(p => p.Status == ProjectStatus.Completed),
                OverBudgetProjects = overBudgetProjects.Count()
            };

            return View(viewModel);
        }
    }
}