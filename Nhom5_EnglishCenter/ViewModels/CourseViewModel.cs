using System.ComponentModel.DataAnnotations;

namespace TrungTamAnhNgu.Web.ViewModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tiêu đề")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mô tả")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Học phí")]
        [Range(0, double.MaxValue, ErrorMessage = "Học phí phải là một số dương")]
        [Display(Name = "Học phí")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Thời lượng")]
        [Range(1, 1000, ErrorMessage = "Thời lượng phải từ 1 đến 1000 giờ")]
        [Display(Name = "Thời lượng (giờ)")]
        public int DurationInHours { get; set; }

        [Display(Name = "Đường dẫn Ảnh bìa")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Phân loại (VD: IELTS, TOEIC)")]
        [StringLength(50)]
        public string? Category { get; set; }
    }
}