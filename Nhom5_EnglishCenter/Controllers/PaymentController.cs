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
    [Authorize(Roles = "Student")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Student> GetCurrentStudentProfile()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return null;

            return await _context.Students
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
        }

        public async Task<IActionResult> Checkout(int enrollmentId)
        {
            var studentProfile = await GetCurrentStudentProfile();
            if (studentProfile == null)
            {
                return Challenge();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentProfile.Id);

            if (enrollment == null)
            {
                return NotFound("Không tìm thấy đơn đăng ký của bạn.");
            }

            if (enrollment.Status == EnrollmentStatus.Paid)
            {
                TempData["ErrorMessage"] = "Lớp học này đã được thanh toán.";
                return RedirectToAction("Index", "Dashboard");
            }

            return View(enrollment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int enrollmentId)
        {
            var studentProfile = await GetCurrentStudentProfile();
            if (studentProfile == null)
            {
                return Challenge();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Class.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentProfile.Id);

            if (enrollment == null)
            {
                return NotFound("Không tìm thấy đơn đăng ký.");
            }

            enrollment.Status = EnrollmentStatus.Paid;
            enrollment.PaymentId = $"FAKE_PAYMENT_{Guid.NewGuid()}";

            _context.Update(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thanh toán thành công cho lớp {enrollment.Class.Course.Title}!";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}