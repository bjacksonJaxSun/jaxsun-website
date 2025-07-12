using JaxSun.Web.Models;

namespace JaxSun.Web.Services
{
    public interface IIdeaSubmissionService
    {
        Task ProcessSubmissionAsync(IdeaSubmissionModel submission);
        Task<List<IdeaSubmissionModel>> GetAllSubmissionsAsync();
        Task<IdeaSubmissionModel?> GetSubmissionByIdAsync(int id);
    }
}