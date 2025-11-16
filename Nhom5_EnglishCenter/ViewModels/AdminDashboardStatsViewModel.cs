using System.ComponentModel.DataAnnotations;

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

        [Display(Name = "Tổng Doanh thu (Đã thanh toán)")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }
    }
}