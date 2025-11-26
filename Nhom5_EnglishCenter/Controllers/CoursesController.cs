using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.Services;

namespace Nhom5_EnglishCenter.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseService _courseService; private readonly ApplicationDbContext _context;

        public CoursesController(ICourseService courseService, ApplicationDbContext context)
        {
            _courseService = courseService;
            _context = context;
        }

        // GET: /Courses?searchString=...
        public async Task<IActionResult> Index(string searchString)
        {
            var courses = await _courseService.GetAllCoursesAsync();
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                courses = courses.Where(c =>
                    c.Title.ToLower().Contains(searchString) ||
                    c.Description.ToLower().Contains(searchString));
            }
            return View(courses);
        }

        // GET: /Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Classes)
                .ThenInclude(cl => cl.Teacher)
                .ThenInclude(t => t.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
    }
}