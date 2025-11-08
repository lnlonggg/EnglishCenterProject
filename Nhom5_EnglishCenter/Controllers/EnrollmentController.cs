using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.Models;

namespace Nhom5_EnglishCenter.Controllers
{
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrollmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Enroll(int classId)
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

            var classToEnroll = await _context.Classes.FindAsync(classId);
            if (classToEnroll == null)
            {
                return NotFound("Lớp học không tồn tại.");
            }

            var existingEnrollment = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentProfile.Id && e.ClassId == classId);

            if (existingEnrollment)
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký lớp học này rồi.";
                return RedirectToAction("Details", "Courses", new { id = classToEnroll.CourseId });
            }

            var enrollment = new Enrollment
            {
                StudentId = studentProfile.Id,
                ClassId = classId,
                EnrollmentDate = DateTime.Now
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đăng ký lớp {classToEnroll.ClassName} thành công!";
            return RedirectToAction("Details", "Courses", new { id = classToEnroll.CourseId });
        }
    }
}