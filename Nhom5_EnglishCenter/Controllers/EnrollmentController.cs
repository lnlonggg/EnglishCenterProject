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

            if (classToEnroll.Status != ClassStatus.Open)
            {
                TempData["ErrorMessage"] = "Lớp học này hiện không nhận đăng ký (Đã đóng hoặc Chưa mở).";
                return RedirectToAction("Details", "Courses", new { id = classToEnroll.CourseId });
            }

            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentProfile.Id && e.ClassId == classId);

            if (existingEnrollment != null)
            {
                if (existingEnrollment.Status == EnrollmentStatus.Paid)
                {
                    TempData["ErrorMessage"] = "Bạn đã đăng ký và thanh toán lớp học này rồi.";
                    return RedirectToAction("Details", "Courses", new { id = classToEnroll.CourseId });
                }

                return RedirectToAction("Checkout", "Payment", new { enrollmentId = existingEnrollment.Id });
            }
            if (await IsStudentScheduleConflict(studentProfile.Id, classId))
            {
                TempData["ErrorMessage"] = "Bạn không thể đăng ký vì bị trùng lịch với một lớp học khác đang tham gia.";
                return RedirectToAction("Details", "Courses", new { id = classToEnroll.CourseId });
            }
            var enrollment = new Enrollment
            {
                StudentId = studentProfile.Id,
                ClassId = classId,
                EnrollmentDate = DateTime.Now,
                Status = EnrollmentStatus.Pending
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Checkout", "Payment", new { enrollmentId = enrollment.Id });
        }

        // --- N.1 HÀM PHỤ TRỢ: PHÂN TÍCH LỊCH ---
        private (List<string> Days, string Shift) ParseScheduleInfo(string schedule)
        {
            // Ví dụ: "T2, T4 | Ca 1 (08:00 - 10:00)"
            var result = (Days: new List<string>(), Shift: "");
            if (string.IsNullOrEmpty(schedule)) return result;

            var parts = schedule.Split('|');
            if (parts.Length >= 2)
            {
                result.Days = parts[0].Trim().Split(',').Select(d => d.Trim()).ToList();
                result.Shift = parts[1].Trim(); // Lấy nguyên cụm "Ca 1..." để so sánh
            }
            return result;
        }

        // --- N.2 HÀM CHECK TRÙNG LỊCH HỌC VIÊN ---
        private async Task<bool> IsStudentScheduleConflict(int studentId, int newClassId)
        {
            // 1. Lấy thông tin lớp muốn đăng ký
            var newClass = await _context.Classes.FindAsync(newClassId);
            if (newClass == null) return false;

            var newInfo = ParseScheduleInfo(newClass.Schedule);
            if (newInfo.Days.Count == 0) return false; // Lịch lỗi, bỏ qua

            // 2. Lấy danh sách các lớp ĐANG HỌC của học viên này
            // (Status là Paid hoặc Pending, và lớp đó chưa kết thúc)
            var activeEnrollments = await _context.Enrollments
                .Include(e => e.Class)
                .Where(e => e.StudentId == studentId &&
                            (e.Status == EnrollmentStatus.Paid || e.Status == EnrollmentStatus.Pending) &&
                            e.Class.Status != ClassStatus.Finished &&
                            e.Class.Status != ClassStatus.Cancelled)
                .ToListAsync();

            // 3. So sánh
            foreach (var enrollment in activeEnrollments)
            {
                // Kiểm tra trùng thời gian (StartDate - EndDate)
                if (enrollment.Class.StartDate < newClass.EndDate && enrollment.Class.EndDate > newClass.StartDate)
                {
                    var currentInfo = ParseScheduleInfo(enrollment.Class.Schedule);

                    // Nếu trùng Ca học
                    if (currentInfo.Shift == newInfo.Shift)
                    {
                        // Kiểm tra xem có trùng Thứ không
                        if (currentInfo.Days.Intersect(newInfo.Days).Any())
                        {
                            return true; // CÓ XUNG ĐỘT
                        }
                    }
                }
            }

            return false; // Không xung đột
        }
    }
}