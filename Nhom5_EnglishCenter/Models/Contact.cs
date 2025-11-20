using System.ComponentModel.DataAnnotations;

namespace TrungTamAnhNgu.Web.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } // Nhu cầu (Tư vấn, Kiểm tra...)

        public string? Message { get; set; } // Lời nhắn thêm

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false; // Trạng thái: Admin đã đọc chưa?
    }
}