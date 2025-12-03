using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TrungTamAnhNgu.Web.Models;

namespace TrungTamAnhNgu.Web.ViewModels
{
    public class AdminDashboardStatsViewModel
    {
        [Display(Name = "Tổng số Học viên")]
        public int TotalStudents { get; set; }

        [Display(Name = "Tổng số Giáo viên")]
        public int TotalTeachers { get; set; }

        [Display(Name = "Tổng số Khóa học")]
        public int TotalCourses { get; set; }

        [Display(Name = "Tổng số Lớp học")]
        public int TotalClasses { get; set; }

        [Display(Name = "Tổng số Lượt Ghi danh")]
        public int TotalEnrollments { get; set; }

        [Display(Name = "Tổng Doanh thu")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }

        // --- THÊM MỚI: Danh sách các lớp cần chú ý (Thiếu học viên / Quá hạn) ---
        public List<Class> AttentionClasses { get; set; } = new List<Class>();
    }
}