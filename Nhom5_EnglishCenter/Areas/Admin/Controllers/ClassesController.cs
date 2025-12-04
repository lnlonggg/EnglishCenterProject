using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using TrungTamAnhNgu.Web.ViewModels;

namespace Nhom5_EnglishCenter.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var classes = _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher.ApplicationUser)
                .Include(c => c.Enrollments);
            return View(await classes.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var classModel = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher.ApplicationUser)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (classModel == null) return NotFound();

            return View(classModel);
        }

        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title");
            ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (await IsScheduleConflict(viewModel.TeacherId, viewModel.Schedule, viewModel.StartDate, viewModel.EndDate))
                {
                    ModelState.AddModelError("", "Xung đột lịch dạy! Giáo viên này đã có lớp khác vào thời gian và lịch học này.");
                    ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
                    ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
                    return View(viewModel);
                }

                if (!await IsTeacherQualified(viewModel.TeacherId, viewModel.CourseId))
                {

                    var course = await _context.Courses.FindAsync(viewModel.CourseId);
                    var teacher = await _context.Teachers.Include(t => t.ApplicationUser).FirstOrDefaultAsync(t => t.Id == viewModel.TeacherId);

                    ModelState.AddModelError("", $"Giáo viên {teacher?.ApplicationUser?.FullName} (Chuyên môn: {teacher?.Specialization ?? "Không"}) không phù hợp để dạy khóa {course?.Title} (Yêu cầu: {course?.Category}).");

                    ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
                    ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
                    return View(viewModel);
                }

                var workloadError = await CheckTeacherWorkload(viewModel.TeacherId, viewModel.Schedule, viewModel.StartDate, viewModel.EndDate);
                if (workloadError != null)
                {
                    ModelState.AddModelError("", workloadError); // Hiển thị lỗi cụ thể

                    ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
                    ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
                    return View(viewModel);
                }

                Class newClass = new Class
                {
                    ClassName = viewModel.ClassName,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    Schedule = viewModel.Schedule,
                    MaxStudents = viewModel.MaxStudents,
                    MinStudents = viewModel.MinStudents,
                    Status = viewModel.Status,
                    Format = viewModel.Format,
                    Location = viewModel.Location,
                    MeetingUrl = viewModel.MeetingUrl,
                    CourseId = viewModel.CourseId,
                    TeacherId = viewModel.TeacherId
                };

                _context.Add(newClass);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var classModel = await _context.Classes.FindAsync(id);
            if (classModel == null) return NotFound();

            ClassViewModel viewModel = new ClassViewModel
            {
                Id = classModel.Id,
                ClassName = classModel.ClassName,
                StartDate = classModel.StartDate,
                EndDate = classModel.EndDate,
                Schedule = classModel.Schedule,
                MaxStudents = classModel.MaxStudents,
                MinStudents = classModel.MinStudents,
                Status = classModel.Status,
                Format = classModel.Format,
                Location = classModel.Location,
                MeetingUrl = classModel.MeetingUrl,
                CourseId = classModel.CourseId,
                TeacherId = classModel.TeacherId
            };

            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClassViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (await IsScheduleConflict(viewModel.TeacherId, viewModel.Schedule, viewModel.StartDate, viewModel.EndDate, id))
                {
                    ModelState.AddModelError("", "Xung đột lịch dạy! Giáo viên này đã có lớp khác vào thời gian và lịch học này.");

                    ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
                    ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
                    return View(viewModel);
                }
                var workloadError = await CheckTeacherWorkload(viewModel.TeacherId, viewModel.Schedule, viewModel.StartDate, viewModel.EndDate, id);
                if (workloadError != null)
                {
                    ModelState.AddModelError("", workloadError);

                    ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
                    ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
                    return View(viewModel);
                }
                try
                {
                    var classFromDb = await _context.Classes.FindAsync(id);
                    if (classFromDb == null) return NotFound();

                    classFromDb.ClassName = viewModel.ClassName;
                    classFromDb.StartDate = viewModel.StartDate;
                    classFromDb.EndDate = viewModel.EndDate;
                    classFromDb.Schedule = viewModel.Schedule;
                    classFromDb.MaxStudents = viewModel.MaxStudents;
                    classFromDb.MinStudents = viewModel.MinStudents;
                    classFromDb.Status = viewModel.Status;
                    classFromDb.Format = viewModel.Format;
                    classFromDb.Location = viewModel.Location;
                    classFromDb.MeetingUrl = viewModel.MeetingUrl;
                    classFromDb.CourseId = viewModel.CourseId;
                    classFromDb.TeacherId = viewModel.TeacherId;

                    _context.Update(classFromDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(viewModel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", viewModel.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.ApplicationUser), "Id", "ApplicationUser.FullName", viewModel.TeacherId);
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var classModel = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classModel == null) return NotFound();

            return View(classModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classModel = await _context.Classes.FindAsync(id);
            if (classModel != null) _context.Classes.Remove(classModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }

        //Kiem tra xung dot lich day
        private async Task<bool> IsScheduleConflict(int teacherId, string schedule, DateTime start, DateTime end, int? ignoreClassId = null)
        {
            return await _context.Classes.AnyAsync(c =>
                c.Id != ignoreClassId &&             
                c.Status != ClassStatus.Cancelled && 
                c.TeacherId == teacherId &&          
                c.Schedule == schedule &&            
                c.StartDate < end && c.EndDate > start 
            );
        }

        //Kiem tra chuyen môn
        private async Task<bool> IsTeacherQualified(int teacherId, int courseId)
        {
            var teacher = await _context.Teachers.FindAsync(teacherId);
            var course = await _context.Courses.FindAsync(courseId);

            if (teacher == null || course == null) return false;

            // 1. Nếu khóa học KHÔNG yêu cầu phân loại (Category rỗng) -> Ai dạy cũng được -> Hợp lệ
            if (string.IsNullOrEmpty(course.Category)) return true;

            // 2. Nếu khóa học CÓ yêu cầu, mà Giáo viên chưa cập nhật chuyên môn -> Không hợp lệ
            if (string.IsNullOrEmpty(teacher.Specialization)) return false;

            // 3. Kiểm tra xem chuyên môn giáo viên có chứa phân loại khóa học không
            // Ví dụ: Course="IELTS", Teacher="TOEIC, IELTS" -> Có chứa -> Hợp lệ
            return teacher.Specialization.ToLower().Contains(course.Category.ToLower());
        }

        // --- HÀM PHỤ TRỢ: PHÂN TÍCH LỊCH HỌC ---
        // Input: "T2, T4 | Ca 1 (08:00 - 10:00)"
        // Output: List ngày ["T2", "T4"] và số giờ dạy (2)
        private (List<string> Days, int Duration) ParseSchedule(string schedule)
        {
            var result = (Days: new List<string>(), Duration: 0);
            if (string.IsNullOrEmpty(schedule)) return result;

            var parts = schedule.Split('|');
            if (parts.Length < 2) return result;
            var daysPart = parts[0].Trim();
            result.Days = daysPart.Split(',').Select(d => d.Trim()).ToList();
            result.Duration = 2;

            return result;
        }

        // CHECK WORKLOAD GIÁO VIÊN
        private async Task<string?> CheckTeacherWorkload(int teacherId, string newSchedule, DateTime start, DateTime end, int? ignoreClassId = null)
        {
            var newClassInfo = ParseSchedule(newSchedule);
            if (newClassInfo.Days.Count == 0) return null; // Lịch lỗi, bỏ qua check này

            var activeClasses = await _context.Classes
                .Where(c => c.TeacherId == teacherId &&
                            c.Status != ClassStatus.Cancelled &&
                            c.Status != ClassStatus.Finished &&
                            c.Id != ignoreClassId && 
                            c.StartDate < end && c.EndDate > start) 
                .ToListAsync();

            var dailyWorkload = new Dictionary<string, int>();
            var allDaysWorked = new HashSet<string>();

            foreach (var c in activeClasses)
            {
                var info = ParseSchedule(c.Schedule);
                foreach (var day in info.Days)
                {
                    if (!dailyWorkload.ContainsKey(day)) dailyWorkload[day] = 0;
                    dailyWorkload[day] += info.Duration;
                    allDaysWorked.Add(day);
                }
            }
            foreach (var day in newClassInfo.Days)
            {
                if (!dailyWorkload.ContainsKey(day)) dailyWorkload[day] = 0;
                dailyWorkload[day] += newClassInfo.Duration;
                allDaysWorked.Add(day);
            }

            foreach (var entry in dailyWorkload)
            {
                if (entry.Value > 8)
                {
                    return $"Giáo viên bị quá tải vào thứ {entry.Key} (Tổng: {entry.Value} giờ). Tối đa cho phép: 8 giờ/ngày.";
                }
            }

            if (allDaysWorked.Count >= 7)
            {
                return "Lịch dạy quá dày! Giáo viên cần ít nhất 1 ngày nghỉ trong tuần (Hiện tại dạy cả 7 ngày).";
            }

            return null;
        }

        // POST: Admin/Classes/ApproveEnrollment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveEnrollment(int enrollmentId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null) return NotFound();

            enrollment.Status = EnrollmentStatus.Paid; // Duyệt đơn -> Đã thanh toán
            _context.Update(enrollment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = enrollment.ClassId });
        }
    }
}