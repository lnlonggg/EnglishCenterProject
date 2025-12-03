using System.ComponentModel.DataAnnotations;

namespace TrungTamAnhNgu.Web.Models
{

    public class Class
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ClassName { get; set; } 

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [StringLength(100)]
        public string Schedule { get; set; }

        public int MaxStudents { get; set; }

        public int MinStudents { get; set; } = 5;
        public ClassStatus Status { get; set; } = ClassStatus.Planned;
        public ClassFormat Format { get; set; } 
        public string? Location { get; set; } 
        public string? MeetingUrl { get; set; } 

        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}