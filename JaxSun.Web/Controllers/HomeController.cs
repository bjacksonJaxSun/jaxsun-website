using Microsoft.AspNetCore.Mvc;
using JaxSun.Web.Models;
using JaxSun.Web.Models.ViewModels;
using System.Diagnostics;

namespace JaxSun.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var viewModel = new HomeViewModel();
            
            // Get images from MainPage folder
            var mainPageImagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "mainpage");
            var heroImages = new List<string>();
            
            if (Directory.Exists(mainPageImagesPath))
            {
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var imageFiles = Directory.GetFiles(mainPageImagesPath)
                    .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    .Select(file => $"/images/mainpage/{Path.GetFileName(file)}")
                    .ToList();
                
                heroImages.AddRange(imageFiles);
            }
            
            viewModel.HeroImages = heroImages;
            
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}