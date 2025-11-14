using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrungTamAnhNgu.Web.Models;
using TrungTamAnhNgu.Web.Services;
using System.Threading.Tasks;

namespace Nhom5_EnglishCenter.Controllers
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
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Courses", new { area = "Admin" });
                }

                if (User.IsInRole("Teacher"))
                {
                    return RedirectToAction("Index", "TeacherDashboard");
                }
            }

            var featuredCourses = await _courseService.GetFeaturedCoursesAsync();
            return View(featuredCourses);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}