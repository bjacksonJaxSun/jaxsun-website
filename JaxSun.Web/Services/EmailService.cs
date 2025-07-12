using System.Net;
using System.Net.Mail;
using System.Text;
using JaxSun.Web.Models;

namespace JaxSun.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string userName)
        {
            try
            {
                var subject = "Thank You for Your Business Idea Submission - JaxSun.us";
                var body = CreateConfirmationEmailBody(userName);

                await SendEmailAsync(toEmail, subject, body);
                _logger.LogInformation($"Confirmation email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send confirmation email to {toEmail}");
                throw;
            }
        }

        public async Task SendNewSubmissionNotificationAsync(IdeaSubmissionModel submission)
        {
            try
            {
                var toEmail = _configuration["EmailSettings:FromEmail"] ?? "contact@jaxsun.us";
                var subject = $"New Business Idea Submission from {submission.Name}";
                var body = CreateSubmissionNotificationBody(submission);

                await SendEmailAsync(toEmail, subject, body);
                _logger.LogInformation($"New submission notification sent for {submission.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send submission notification for {submission.Name}");
                throw;
            }
        }

        public async Task SendContactEmailAsync(ContactModel contact)
        {
            try
            {
                var toEmail = _configuration["EmailSettings:FromEmail"] ?? "contact@jaxsun.us";
                var subject = $"Contact Form Submission: {contact.Subject}";
                var body = CreateContactEmailBody(contact);

                await SendEmailAsync(toEmail, subject, body);
                _logger.LogInformation($"Contact email sent from {contact.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send contact email from {contact.Email}");
                throw;
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"];
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogWarning("Email configuration is incomplete. Email not sent.");
                return;
            }

            using var client = new SmtpClient(smtpServer, smtpPort);
            client.Credentials = new NetworkCredential(username, password);
            client.EnableSsl = true;

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }

        private string CreateConfirmationEmailBody(string userName)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }");
            html.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            html.AppendLine(".header { background-color: #2c5f2d; color: white; padding: 20px; text-align: center; }");
            html.AppendLine(".content { background-color: #f9f9f9; padding: 30px; }");
            html.AppendLine(".footer { background-color: #e9ecef; padding: 20px; text-align: center; font-size: 14px; }");
            html.AppendLine(".btn { background-color: #2c5f2d; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='container'>");
            
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>JaxSun.us</h1>");
            html.AppendLine("<h2>Thank You for Your Submission!</h2>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='content'>");
            html.AppendLine($"<p>Dear {userName},</p>");
            html.AppendLine("<p>Thank you for submitting your business idea to JaxSun.us! We're excited to learn about your vision and explore how we might partner together to bring it to life.</p>");
            
            html.AppendLine("<h3>What Happens Next?</h3>");
            html.AppendLine("<ol>");
            html.AppendLine("<li><strong>AI-Powered Analysis (Starting Now):</strong> Our advanced systems are beginning comprehensive market research and feasibility analysis of your business idea.</li>");
            html.AppendLine("<li><strong>Initial Contact (1-3 Days):</strong> We'll reach out within 1-3 business days to discuss our preliminary findings and ask any clarifying questions.</li>");
            html.AppendLine("<li><strong>Detailed Review (1-3 Weeks):</strong> Complete evaluation and partnership discussion to determine if there's a good fit for collaboration.</li>");
            html.AppendLine("</ol>");
            
            html.AppendLine("<h3>Our Partnership Approach</h3>");
            html.AppendLine("<p>At JaxSun.us, we believe in shared risk and shared reward. Unlike traditional development agencies that require large upfront payments, we invest alongside you and share in both the challenges and successes of building your business.</p>");
            
            html.AppendLine("<p>With our combined 47+ years of enterprise software experience, we bring Fortune 500-level expertise to startup ideas, helping entrepreneurs validate their concepts and build sustainable businesses.</p>");
            
            html.AppendLine("<h3>Questions?</h3>");
            html.AppendLine("<p>If you have any questions about the process or want to provide additional information, feel free to reply to this email or contact us directly.</p>");
            
            html.AppendLine("<p>Best regards,<br>");
            html.AppendLine("Bobby & Robert<br>");
            html.AppendLine("The JaxSun.us Partnership Team</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>JaxSun.us - Turning Great Ideas Into Thriving Software Businesses</p>");
            html.AppendLine("<p>Email: contact@jaxsun.us | Based in the Midwest USA</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private string CreateSubmissionNotificationBody(IdeaSubmissionModel submission)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }");
            html.AppendLine(".container { max-width: 800px; margin: 0 auto; padding: 20px; }");
            html.AppendLine(".header { background-color: #2c5f2d; color: white; padding: 20px; }");
            html.AppendLine(".content { background-color: #f9f9f9; padding: 30px; }");
            html.AppendLine(".field { margin-bottom: 20px; padding: 15px; background-color: white; border-left: 4px solid #2c5f2d; }");
            html.AppendLine(".field-label { font-weight: bold; color: #2c5f2d; margin-bottom: 5px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='container'>");
            
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>New Business Idea Submission</h1>");
            html.AppendLine($"<p>Submitted: {submission.SubmissionDate:F}</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='content'>");
            
            html.AppendLine("<div class='field'>");
            html.AppendLine("<div class='field-label'>Name:</div>");
            html.AppendLine($"<div>{submission.Name}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='field'>");
            html.AppendLine("<div class='field-label'>Email:</div>");
            html.AppendLine($"<div>{submission.Email}</div>");
            html.AppendLine("</div>");
            
            if (!string.IsNullOrEmpty(submission.Company))
            {
                html.AppendLine("<div class='field'>");
                html.AppendLine("<div class='field-label'>Company:</div>");
                html.AppendLine($"<div>{submission.Company}</div>");
                html.AppendLine("</div>");
            }
            
            html.AppendLine("<div class='field'>");
            html.AppendLine("<div class='field-label'>Partnership Interest Level:</div>");
            html.AppendLine($"<div>{submission.PartnershipInterest}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='field'>");
            html.AppendLine("<div class='field-label'>Business Idea Description:</div>");
            html.AppendLine($"<div style='white-space: pre-wrap;'>{submission.IdeaDescription}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='field'>");
            html.AppendLine("<div class='field-label'>Market Opportunity:</div>");
            html.AppendLine($"<div style='white-space: pre-wrap;'>{submission.MarketOpportunity}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private string CreateContactEmailBody(ContactModel contact)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }");
            html.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            html.AppendLine(".header { background-color: #2c5f2d; color: white; padding: 20px; }");
            html.AppendLine(".content { background-color: #f9f9f9; padding: 30px; }");
            html.AppendLine(".field { margin-bottom: 15px; padding: 10px; background-color: white; border-left: 4px solid #2c5f2d; }");
            html.AppendLine(".field-label { font-weight: bold; color: #2c5f2d; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='container'>");
            
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>Contact Form Submission</h1>");
            html.AppendLine($"<p>Submitted: {contact.SubmissionDate:F}</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='content'>");
            
            html.AppendLine("<div class='field'>");
            html.AppendLine("<span class='field-label'>Name:</span> " + contact.Name);
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='field'>");
            html.AppendLine("<span class='field-label'>Email:</span> " + contact.Email);
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='field'>");
            html.AppendLine("<span class='field-label'>Subject:</span> " + contact.Subject);
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='field'>");
            html.AppendLine("<div class='field-label'>Message:</div>");
            html.AppendLine($"<div style='white-space: pre-wrap; margin-top: 5px;'>{contact.Message}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}