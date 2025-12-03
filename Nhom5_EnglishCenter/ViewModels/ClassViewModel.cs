using System;
using System.ComponentModel.DataAnnotations;
using TrungTamAnhNgu.Web.Models;

namespace TrungTamAnhNgu.Web.ViewModels
{
    public class ClassViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tên lớp")]
        [StringLength(100)]
        [Display(Name = "Tên lớp")]
        public string ClassName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Ngày khai giảng")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày khai giảng")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Ngày kết thúc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày kết thúc")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Lịch học")]
        [StringLength(100)]
        [Display(Name = "Lịch học")]
        public string Schedule { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Sĩ số tối đa")]
        [Range(1, 100, ErrorMessage = "Sĩ số phải từ 1 đến 100")]
        [Display(Name = "Sĩ số tối đa")]
        public int MaxStudents { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Sĩ số tối thiểu")]
        [Range(1, 100, ErrorMessage = "Sĩ số tối thiểu phải > 0")]
        [Display(Name = "Sĩ số tối thiểu")]
        public int MinStudents { get; set; } = 5;

        [Display(Name = "Trạng thái")]
        public ClassStatus Status { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Hình thức")]
        [Display(Name = "Hình thức")]
        public ClassFormat Format { get; set; }

        [Display(Name = "Địa điểm (Nếu Offline)")]
        public string? Location { get; set; }

        [Display(Name = "Link học (Nếu Online)")]
        [Url(ErrorMessage = "Link học không hợp lệ")]
        public string? MeetingUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Khóa học")]
        [Display(Name = "Thuộc Khóa học")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Giáo viên")]
        [Display(Name = "Giáo viên")]
        public int TeacherId { get; set; }
    }
}