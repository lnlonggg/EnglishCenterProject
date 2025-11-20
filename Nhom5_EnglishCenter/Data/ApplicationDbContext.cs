using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Models;

namespace TrungTamAnhNgu.Web.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<Contact> Contacts { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình quan hệ 1-1 giữa ApplicationUser và Student
            builder.Entity<ApplicationUser>()
                .HasOne(au => au.StudentProfile)
                .WithOne(s => s.ApplicationUser)
                .HasForeignKey<Student>(s => s.ApplicationUserId);

            // Cấu hình quan hệ 1-1 giữa ApplicationUser và Teacher
            builder.Entity<ApplicationUser>()
                .HasOne(au => au.TeacherProfile)
                .WithOne(t => t.ApplicationUser)
                .HasForeignKey<Teacher>(t => t.ApplicationUserId);

            // Cấu hình quan hệ 1-nhiều giữa Teacher và Class
            builder.Entity<Teacher>()
                .HasMany(t => t.Classes)
                .WithOne(c => c.Teacher)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Cấu hình quan hệ 1-nhiều giữa Course và Class
            builder.Entity<Course>()
                .HasMany(co => co.Classes)
                .WithOne(cl => cl.Course)
                .HasForeignKey(cl => cl.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng ghi danh (Enrollment) - quan hệ nhiều-nhiều
            builder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Class)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình cho kiểu dữ liệu Enum (ClassFormat)
            builder.Entity<Class>()
                .Property(c => c.Format)
                .HasConversion<string>();

            // Cấu hình cho kiểu dữ liệu Enum (EnrollmentStatus)
            builder.Entity<Enrollment>()
    .Property(e => e.Status)
    .HasConversion<string>();
        }
    }
}