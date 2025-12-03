using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.Helpers;
using TrungTamAnhNgu.Web.Models;

namespace Nhom5_EnglishCenter.Controllers
{
    [Authorize(Roles = "Student")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var studentProfile = await _context.Students
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (studentProfile == null)
            {
                return NotFound("Không tìm thấy hồ sơ học viên.");
            }

            var myEnrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentProfile.Id)
                .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                .Include(e => e.Class)
                    .ThenInclude(c => c.Teacher)
                    .ThenInclude(t => t.ApplicationUser)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();

            return View(myEnrollments);
        }

        // GET: /Dashboard/Timetable?date=...
        public async Task<IActionResult> Timetable(DateTime? date)
        {
            var userId = _userManager.GetUserId(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (student == null) return Challenge();

            // Mặc định là hôm nay nếu không chọn ngày
            DateTime targetDate = date ?? DateTime.Today;

            var myEnrollments = await _context.Enrollments
                .Include(e => e.Class.Course)
                .Include(e => e.Class.Teacher.ApplicationUser)
                .Where(e => e.StudentId == student.Id &&
                            (e.Status == EnrollmentStatus.Paid || e.Status == EnrollmentStatus.Pending))
                .ToListAsync();

            var myClasses = myEnrollments.Select(e => e.Class).ToList();

            // Gọi Helper mới
            var model = ScheduleHelper.ParseForWeek(myClasses, targetDate);

            return View(model);
        }
    }
}
