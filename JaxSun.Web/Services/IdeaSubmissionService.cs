using JaxSun.Web.Models;
using System.Text.Json;

namespace JaxSun.Web.Services
{
    public class IdeaSubmissionService : IIdeaSubmissionService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<IdeaSubmissionService> _logger;
        private readonly string _dataFilePath;

        public IdeaSubmissionService(IEmailService emailService, ILogger<IdeaSubmissionService> logger, IWebHostEnvironment environment)
        {
            _emailService = emailService;
            _logger = logger;
            _dataFilePath = Path.Combine(environment.ContentRootPath, "Data", "submissions.json");
            
            // Ensure data directory exists
            var dataDirectory = Path.GetDirectoryName(_dataFilePath);
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory!);
            }
        }

        public async Task ProcessSubmissionAsync(IdeaSubmissionModel submission)
        {
            try
            {
                // Set submission date
                submission.SubmissionDate = DateTime.UtcNow;

                // Save submission to file
                await SaveSubmissionAsync(submission);

                // Send confirmation email to user
                await _emailService.SendConfirmationEmailAsync(submission.Email, submission.Name);

                // Send notification email to team
                await _emailService.SendNewSubmissionNotificationAsync(submission);

                _logger.LogInformation($"Successfully processed submission from {submission.Name} ({submission.Email})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process submission from {submission.Name} ({submission.Email})");
                throw;
            }
        }

        public async Task<List<IdeaSubmissionModel>> GetAllSubmissionsAsync()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    return new List<IdeaSubmissionModel>();
                }

                var json = await File.ReadAllTextAsync(_dataFilePath);
                var submissions = JsonSerializer.Deserialize<List<IdeaSubmissionModel>>(json) ?? new List<IdeaSubmissionModel>();
                
                return submissions.OrderByDescending(s => s.SubmissionDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve submissions");
                return new List<IdeaSubmissionModel>();
            }
        }

        public async Task<IdeaSubmissionModel?> GetSubmissionByIdAsync(int id)
        {
            var submissions = await GetAllSubmissionsAsync();
            return submissions.FirstOrDefault(s => s.GetHashCode() == id);
        }

        private async Task SaveSubmissionAsync(IdeaSubmissionModel submission)
        {
            var submissions = await GetAllSubmissionsAsync();
            submissions.Add(submission);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(submissions, options);
            await File.WriteAllTextAsync(_dataFilePath, json);
        }
    }
}