using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrungTamAnhNgu.Web.Models; 
using TrungTamAnhNgu.Web.Services;

namespace TrungTamAnhNgu.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseService _courseService;

        public HomeController(ILogger<HomeController> logger, ICourseService courseService)
        {
            _logger = logger;
            _courseService = courseService; 
        }


        public async Task<IActionResult> Index()
        {
            var featuredCourses = await _courseService.GetFeaturedCoursesAsync();

            // Truyền danh sách khóa học này cho View
            return View(featuredCourses);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}