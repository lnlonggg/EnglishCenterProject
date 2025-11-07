using Microsoft.AspNetCore.Mvc;
using TrungTamAnhNgu.Web.Services;

namespace Nhom5_EnglishCenter.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // GET: /Courses
        public async Task<IActionResult> Index()
        {
            var allCourses = await _courseService.GetAllCoursesAsync();
            return View(allCourses);
        }
    }
}