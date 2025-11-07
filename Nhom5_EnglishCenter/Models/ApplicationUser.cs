using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TrungTamAnhNgu.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public virtual Student? StudentProfile { get; set; }
        public virtual Teacher? TeacherProfile { get; set; }
    }
}