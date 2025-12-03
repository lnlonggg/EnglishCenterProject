using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.Helpers;
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
        public async Task<IActionResult> UpdateGrade(int enrollmentId, string? finalGrade) // 1. Đổi tham số thành string
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

            decimal? parsedGrade = null;
            if (!string.IsNullOrEmpty(finalGrade))
            {
                string normalizedGrade = finalGrade.Replace(",", ".");

                if (decimal.TryParse(normalizedGrade, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal result))
                {
                    parsedGrade = result;
                }
                else
                {
                    TempData["ErrorMessage"] = "Định dạng điểm không hợp lệ.";
                    return RedirectToAction("ClassDetails", new { id = enrollment.ClassId });
                }
            }
            if (parsedGrade.HasValue && (parsedGrade < 0 || parsedGrade > 10))
            {
                TempData["ErrorMessage"] = "Điểm số phải nằm trong khoảng từ 0 đến 10.";
            }
            else
            {
                enrollment.FinalGrade = parsedGrade;
                _context.Update(enrollment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật điểm thành công.";
            }

            return RedirectToAction("ClassDetails", new { id = enrollment.ClassId });
        }

        public async Task<IActionResult> Timetable(DateTime? date)
        {
            var teacher = await GetCurrentTeacherProfile();
            if (teacher == null) return Challenge();

            DateTime targetDate = date ?? DateTime.Today;

            var myClasses = await _context.Classes
                .Include(c => c.Course)
                .Where(c => c.TeacherId == teacher.Id)
                .ToListAsync();

            var model = ScheduleHelper.ParseForWeek(myClasses, targetDate);

            return View(model);
        }
    }
}