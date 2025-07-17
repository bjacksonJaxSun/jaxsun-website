using System.ComponentModel.DataAnnotations;

namespace JaxSun.Web.Models
{
    public class IdeaSubmissionModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Company/Organization")]
        public string Company { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe your business idea")]
        [Display(Name = "Business Idea Description")]
        [StringLength(2000, ErrorMessage = "Description must be less than 2000 characters")]
        public string IdeaDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe the market opportunity")]
        [Display(Name = "Market Opportunity")]
        [StringLength(1000, ErrorMessage = "Market opportunity must be less than 1000 characters")]
        public string MarketOpportunity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please indicate your partnership interest level")]
        [Display(Name = "Partnership Interest Level")]
        public string PartnershipInterest { get; set; } = string.Empty;

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    }
}