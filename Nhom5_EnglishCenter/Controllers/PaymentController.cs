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
        public async Task<IActionResult> ProcessPayment(int enrollmentId, string paymentMethod)
        {
            var studentProfile = await GetCurrentStudentProfile();
            if (studentProfile == null) return Challenge();

            var enrollment = await _context.Enrollments
                .Include(e => e.Class.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentProfile.Id);

            if (enrollment == null) return NotFound();

            enrollment.Status = EnrollmentStatus.Pending;


            enrollment.PaymentId = $"{paymentMethod}_{DateTime.Now.Ticks}";

            _context.Update(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yêu cầu thanh toán đã được gửi! Vui lòng chờ Admin xác nhận.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}