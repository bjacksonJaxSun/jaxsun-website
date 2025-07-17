using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using JaxSun.Web.Data;
using JaxSun.Web.Models;
using Xunit;

namespace JaxSun.Web.Tests
{
    public class Phase1Tests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public Phase1Tests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Application_StartsSuccessfully()
        {
            // Arrange & Act
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Database_CreatesAllTables()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Act
            await context.Database.EnsureCreatedAsync();

            // Assert
            Assert.True(await context.Database.CanConnectAsync());
            
            // Check that our custom tables exist
            var timeCategories = await context.TimeCategories.ToListAsync();
            var projects = await context.Projects.ToListAsync();
            var timeEntries = await context.TimeEntries.ToListAsync();
            var ideaSubmissions = await context.IdeaSubmissions.ToListAsync();

            // Should not throw exceptions (tables exist)
            Assert.NotNull(timeCategories);
            Assert.NotNull(projects);
            Assert.NotNull(timeEntries);
            Assert.NotNull(ideaSubmissions);
        }

        [Fact]
        public async Task Roles_AreSeededCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Act & Assert
            Assert.True(await roleManager.RoleExistsAsync("Admin"));
            Assert.True(await roleManager.RoleExistsAsync("Partner"));
        }

        [Fact]
        public async Task TimeCategories_AreSeededCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            // Act
            var categories = await context.TimeCategories.ToListAsync();

            // Assert
            Assert.True(categories.Count >= 8, "Should have at least 8 default categories");
            Assert.Contains(categories, c => c.Name == "Development");
            Assert.Contains(categories, c => c.Name == "Design");
            Assert.Contains(categories, c => c.Name == "Business");
            Assert.Contains(categories, c => c.Name == "Research");
            Assert.Contains(categories, c => c.Name == "Marketing");
            Assert.Contains(categories, c => c.Name == "Meeting");
            Assert.Contains(categories, c => c.Name == "Testing");
            Assert.Contains(categories, c => c.Name == "Documentation");

            // Check colors are assigned
            Assert.All(categories, c => Assert.False(string.IsNullOrEmpty(c.Color)));
        }

        [Fact]
        public void TimeEntry_ComputedProperties_WorkCorrectly()
        {
            // Arrange
            var startTime = DateTime.Now;
            var endTime = startTime.AddHours(2.5);
            var timeEntry = new TimeEntry
            {
                StartTime = startTime,
                EndTime = endTime
            };

            // Act & Assert
            Assert.Equal(TimeSpan.FromHours(2.5), timeEntry.Duration);
            Assert.Equal(2.5m, timeEntry.HoursWorked);
            Assert.False(timeEntry.IsActive);
            Assert.True(timeEntry.IsCompleted);
            Assert.True(timeEntry.IsValid);
        }

        [Fact]
        public void TimeEntry_ActiveTimer_WorksCorrectly()
        {
            // Arrange
            var timeEntry = new TimeEntry
            {
                StartTime = DateTime.Now,
                EndTime = null // Active timer
            };

            // Act & Assert
            Assert.Null(timeEntry.Duration);
            Assert.Equal(0m, timeEntry.HoursWorked);
            Assert.True(timeEntry.IsActive);
            Assert.False(timeEntry.IsCompleted);
            Assert.True(timeEntry.IsValid);
        }

        [Fact]
        public void Project_ComputedProperties_WorkCorrectly()
        {
            // Arrange
            var startTime = new DateTime(2025, 1, 1, 10, 0, 0);
            var project = new Project
            {
                EstimatedHours = 10m,
                TimeEntries = new List<TimeEntry>
                {
                    new TimeEntry 
                    { 
                        StartTime = startTime, 
                        EndTime = startTime.AddHours(3) 
                    },
                    new TimeEntry 
                    { 
                        StartTime = startTime.AddHours(4), 
                        EndTime = startTime.AddHours(6) 
                    }
                }
            };

            // Act & Assert
            Assert.Equal(5m, project.TotalHoursLogged);
            Assert.Equal(5m, project.RemainingHours);
            Assert.False(project.IsOverBudget);
        }

        [Fact]
        public void Project_OverBudget_WorksCorrectly()
        {
            // Arrange
            var startTime = new DateTime(2025, 1, 1, 10, 0, 0);
            var project = new Project
            {
                EstimatedHours = 5m,
                TimeEntries = new List<TimeEntry>
                {
                    new TimeEntry 
                    { 
                        StartTime = startTime, 
                        EndTime = startTime.AddHours(6) 
                    }
                }
            };

            // Act & Assert
            Assert.Equal(6m, project.TotalHoursLogged);
            Assert.Equal(-1m, project.RemainingHours);
            Assert.True(project.IsOverBudget);
        }

        [Fact]
        public async Task Identity_LoginPage_IsAccessible()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/Identity/Account/Login");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Identity_RegisterPage_IsAccessible()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/Identity/Account/Register");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ExistingPages_StillWork()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act & Assert
            var homeResponse = await client.GetAsync("/");
            homeResponse.EnsureSuccessStatusCode();

            var aboutResponse = await client.GetAsync("/About");
            aboutResponse.EnsureSuccessStatusCode();

            var contactResponse = await client.GetAsync("/Contact");
            contactResponse.EnsureSuccessStatusCode();

            var partnershipResponse = await client.GetAsync("/Partnership");
            partnershipResponse.EnsureSuccessStatusCode();

            var processResponse = await client.GetAsync("/Process");
            processResponse.EnsureSuccessStatusCode();
        }
    }
}