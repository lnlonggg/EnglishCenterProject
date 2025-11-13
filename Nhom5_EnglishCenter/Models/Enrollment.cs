using System.ComponentModel.DataAnnotations.Schema;

namespace TrungTamAnhNgu.Web.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? FinalGrade { get; set; }

        public EnrollmentStatus Status { get; set; }
        public string? PaymentId { get; set; }

        public int StudentId { get; set; }
        public virtual Student Student { get; set; }

        public int ClassId { get; set; }
        public virtual Class Class { get; set; }
    }
}