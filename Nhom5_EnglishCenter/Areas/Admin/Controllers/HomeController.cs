using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.ViewModels;

namespace Nhom5_EnglishCenter.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new AdminDashboardStatsViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalTeachers = await _context.Teachers.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalClasses = await _context.Classes.CountAsync(),
                TotalEnrollments = await _context.Enrollments.CountAsync(),
                TotalRevenue = await _context.Enrollments
                                    .Where(e => e.Status == TrungTamAnhNgu.Web.Models.EnrollmentStatus.Paid)
                                    .Select(e => e.Class.Course.Price)
                                    .SumAsync()
            };

            return View(stats);
        }
    }
}