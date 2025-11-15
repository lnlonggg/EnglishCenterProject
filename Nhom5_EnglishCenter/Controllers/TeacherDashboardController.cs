using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.Models;

namespace Nhom5_EnglishCenter.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Teacher> GetCurrentTeacherProfile()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return null;

            return await _context.Teachers
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
        }

        public async Task<IActionResult> Index()
        {
            var teacherProfile = await GetCurrentTeacherProfile();
            if (teacherProfile == null)
            {
                return NotFound("Không tìm thấy hồ sơ giáo viên.");
            }

            var myClasses = await _context.Classes
                .Where(c => c.TeacherId == teacherProfile.Id)
                .Include(c => c.Course)
                .OrderBy(c => c.StartDate)
                .ToListAsync();

            return View(myClasses);
        }

        public async Task<IActionResult> ClassDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacherProfile = await GetCurrentTeacherProfile();
            if (teacherProfile == null)
            {
                return Challenge();
            }

            var classDetails = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.ApplicationUser)
                .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherProfile.Id);

            if (classDetails == null)
            {
                return NotFound("Không tìm thấy lớp học hoặc bạn không có quyền xem lớp này.");
            }

            return View(classDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGrade(int enrollmentId, decimal? finalGrade)
        {
            var teacherProfile = await GetCurrentTeacherProfile();
            if (teacherProfile == null)
            {
                return Challenge();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Class)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null || enrollment.Class.TeacherId != teacherProfile.Id)
            {
                return NotFound("Không tìm thấy lượt ghi danh hoặc bạn không có quyền sửa.");
            }

            if (finalGrade.HasValue && (finalGrade < 0 || finalGrade > 10))
            {
                TempData["ErrorMessage"] = "Điểm số phải nằm trong khoảng từ 0 đến 10.";
            }
            else
            {
                enrollment.FinalGrade = finalGrade;
                _context.Update(enrollment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật điểm thành công.";
            }

            return RedirectToAction("ClassDetails", new { id = enrollment.ClassId });
        }
    }
}