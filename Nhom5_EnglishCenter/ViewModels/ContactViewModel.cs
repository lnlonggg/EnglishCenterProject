using System.ComponentModel.DataAnnotations;

namespace TrungTamAnhNgu.Web.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ và tên")]
        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhu cầu")]
        [Display(Name = "Bạn quan tâm đến?")]
        public string Subject { get; set; } // dropdown (Tư vấn / Kiểm tra trình độ...)

        [Display(Name = "Lời nhắn / Câu hỏi thêm")]
        public string? Message { get; set; }
    }
}