using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace TrungTamAnhNgu.Web.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public string? AvatarUrl { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}