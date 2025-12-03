using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.ViewModels;

namespace Nhom5_EnglishCenter.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // --- PHẦN 1: QUÉT TỰ ĐỘNG (AUTO-SCAN) ---
            var today = DateTime.Today;

            // Lấy tất cả các lớp đang "Mở" mà đã đến (hoặc quá) ngày khai giảng
            var pendingClasses = await _context.Classes
                .Include(c => c.Enrollments)
                .Where(c => c.Status == TrungTamAnhNgu.Web.Models.ClassStatus.Open && c.StartDate <= today)
                .ToListAsync();

            int autoActivatedCount = 0;
            var warningClasses = new List<TrungTamAnhNgu.Web.Models.Class>();

            foreach (var lop in pendingClasses)
            {
                // Đếm số học viên đã đóng tiền (hoặc đăng ký)
                int currentStudents = lop.Enrollments.Count;

                if (currentStudents >= lop.MinStudents)
                {
                    // TH1: Đủ người -> Tự động chuyển sang "Đang học"
                    lop.Status = TrungTamAnhNgu.Web.Models.ClassStatus.Active;
                    _context.Update(lop);
                    autoActivatedCount++;
                }
                else
                {
                    // TH2: Thiếu người -> Thêm vào danh sách cảnh báo
                    warningClasses.Add(lop);
                }
            }

            if (autoActivatedCount > 0)
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Hệ thống đã tự động kích hoạt {autoActivatedCount} lớp học đủ điều kiện.";
            }

            // --- PHẦN 2: THỐNG KÊ SỐ LIỆU (CODE CŨ) ---
            var stats = new AdminDashboardStatsViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalTeachers = await _context.Teachers.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalClasses = await _context.Classes.CountAsync(),
                TotalEnrollments = await _context.Enrollments.CountAsync(),
                TotalRevenue = await _context.Enrollments
                                    .Where(e => e.Status == TrungTamAnhNgu.Web.Models.EnrollmentStatus.Paid)
                                    .Select(e => e.Class.Course.Price)
                                    .SumAsync(),
                // Gán danh sách cảnh báo vào ViewModel
                AttentionClasses = warningClasses
            };

            return View(stats);
        }
    }
}