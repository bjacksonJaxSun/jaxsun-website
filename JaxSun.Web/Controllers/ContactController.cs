using Microsoft.AspNetCore.Mvc;
using JaxSun.Web.Models;
using JaxSun.Web.Services;

namespace JaxSun.Web.Controllers
{
    public class ContactController : Controller
    {
        private readonly IIdeaSubmissionService _ideaSubmissionService;
        private readonly IEmailService _emailService;

        public ContactController(IIdeaSubmissionService ideaSubmissionService, IEmailService emailService)
        {
            _ideaSubmissionService = ideaSubmissionService;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SubmitIdea()
        {
            TempData["InfoMessage"] = "Still working on this. Until then feel free to use the Contact form or email us at contact@JaxSun.us.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitIdea(IdeaSubmissionModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _ideaSubmissionService.ProcessSubmissionAsync(model);
                    await _emailService.SendConfirmationEmailAsync(model.Email, model.Name);
                    
                    TempData["SuccessMessage"] = "Thank you for submitting your idea! We'll be in touch within 1-3 business days.";
                    return RedirectToAction("ThankYou");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "There was an error processing your submission. Please try again.");
                }
            }

            return View(model);
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}