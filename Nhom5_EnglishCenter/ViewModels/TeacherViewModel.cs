using System.ComponentModel.DataAnnotations;

namespace TrungTamAnhNgu.Web.ViewModels
{
    public class TeacherViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email Đăng nhập")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Họ và Tên")]
        [StringLength(100)]
        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Tiểu sử / Bằng cấp")]
        [StringLength(500)]
        public string? Bio { get; set; }

        [Display(Name = "Đường dẫn Ảnh đại diện")]
        [Url(ErrorMessage = "Đường dẫn ảnh không hợp lệ")]
        public string? AvatarUrl { get; set; }
    }
}