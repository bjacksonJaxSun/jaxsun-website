using System.ComponentModel.DataAnnotations;

namespace JaxSun.Web.Models
{
    public class ContactModel
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [Display(Name = "Message")]
        [StringLength(1000, ErrorMessage = "Message must be less than 1000 characters")]
        public string Message { get; set; } = string.Empty;

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    }
}