using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace TrungTamAnhNgu.Web.Models
{

    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        public int DurationInHours { get; set; }

        public string? ImageUrl { get; set; }

        [StringLength(50)]
        public string? Category { get; set; } 

        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}