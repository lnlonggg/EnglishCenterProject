using System;
using System.Collections.Generic;
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

        // --- HÀM PHỤ TRỢ: PHÂN TÍCH LỊCH (Phiên bản nâng cấp) ---
        private (List<string> Days, int ShiftId) ParseScheduleInfo(string schedule)
        {
            // Output: Danh sách thứ (T2, T3...) và Mã Ca (1, 2, 3, 4)
            var result = (Days: new List<string>(), ShiftId: -1);

            if (string.IsNullOrEmpty(schedule)) return result;

            var parts = schedule.Split('|');
            if (parts.Length >= 2)
            {
                // 1. Xử lý Thứ: Tách dấu phẩy và Xóa khoảng trắng thừa
                // Ví dụ: " T2, T4 " -> ["T2", "T4"]
                result.Days = parts[0].Trim().Split(',')
                                     .Select(d => d.Trim().ToUpper())
                                     .ToList();

                // 2. Xử lý Ca: Chỉ lấy số hiệu Ca (1, 2, 3, 4) để so sánh
                string shiftPart = parts[1].Trim().ToUpper(); // VD: "CA 1 (08:00...)"

                if (shiftPart.Contains("CA 1")) result.ShiftId = 1;
                else if (shiftPart.Contains("CA 2")) result.ShiftId = 2;
                else if (shiftPart.Contains("CA 3")) result.ShiftId = 3;
                else if (shiftPart.Contains("CA 4")) result.ShiftId = 4;
            }
            return result;
        }

        // --- HÀM CHECK TRÙNG LỊCH (Logic chặt chẽ hơn) ---
        private async Task<string?> CheckScheduleConflict(int studentId, int newClassId)
        {
            var newClass = await _context.Classes.FindAsync(newClassId);
            if (newClass == null) return null;

            // Dùng Helper mới để lấy danh sách các slot (Thứ, Ca) của lớp mới
            var newSlots = TrungTamAnhNgu.Web.Helpers.ScheduleHelper.ParseString(newClass.Schedule);

            var activeEnrollments = await _context.Enrollments
                .Include(e => e.Class)
                .Where(e => e.StudentId == studentId &&
                            (e.Status == EnrollmentStatus.Paid || e.Status == EnrollmentStatus.Pending) &&
                            e.Class.Status != ClassStatus.Finished &&
                            e.Class.Status != ClassStatus.Cancelled &&
                            e.Class.Id != newClassId)
                .ToListAsync();

            foreach (var enrollment in activeEnrollments)
            {
                var existingClass = enrollment.Class;

                // Check trùng ngày tháng
                bool dateOverlap = newClass.StartDate < existingClass.EndDate && newClass.EndDate > existingClass.StartDate;

                if (dateOverlap)
                {
                    var existingSlots = TrungTamAnhNgu.Web.Helpers.ScheduleHelper.ParseString(existingClass.Schedule);

                    // So sánh 2 danh sách Slot
                    foreach (var nSlot in newSlots)
                    {
                        foreach (var eSlot in existingSlots)
                        {
                            // Nếu trùng cả Thứ và Ca
                            if (nSlot.Day == eSlot.Day && nSlot.Shift == eSlot.Shift)
                            {
                                return $"Bạn bị trùng lịch với lớp '{existingClass.ClassName}' vào {nSlot.Day} - Ca {nSlot.Shift}.";
                            }
                        }
                    }
                }
            }
            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Enroll(int classId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var studentProfile = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (studentProfile == null) return NotFound("Không tìm thấy hồ sơ học viên.");

            var classToEnroll = await _context.Classes.FindAsync(classId);
            if (classToEnroll == null) return NotFound("Lớp học không tồn tại.");

            // Check 1: Đã đăng ký chưa?
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentProfile.Id && e.ClassId == classId);

            if (existingEnrollment != null)
            {
                if (existingEnrollment.Status == EnrollmentStatus.Paid)
                {
                    TempData["ErrorMessage"] = "Bạn đã đăng ký lớp học này rồi.";
                    return RedirectToAction("Details", "Courses", new { id = classToEnroll.CourseId });
                }
                return RedirectToAction("Checkout", "Payment", new { enrollmentId = existingEnrollment.Id });
            }

            // Check 2: Lớp có đang mở không?
            if (classToEnroll.Status != ClassStatus.Open)
            {
                TempData["ErrorMessage"] = "Lớp học này hiện không nhận đăng ký.";
                return RedirectToAction("Details", "Courses", new { id = classToEnroll.CourseId });
            }

            // --- Check 3 (FIXED): Kiểm tra trùng lịch ---
            string? conflictMessage = await CheckScheduleConflict(studentProfile.Id, classId);
            if (conflictMessage != null)
            {
                TempData["ErrorMessage"] = $"Không thể đăng ký! {conflictMessage}";
                return RedirectToAction("Details", "Courses", new { id = classToEnroll.CourseId });
            }
            // -------------------------------------------

            // Tạo mới Enrollment
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
    }
}