namespace JaxSun.Web.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string userName);
        Task SendNewSubmissionNotificationAsync(Models.IdeaSubmissionModel submission);
        Task SendContactEmailAsync(Models.ContactModel contact);
    }
}