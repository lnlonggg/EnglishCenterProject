using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Data;
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
    }
}
