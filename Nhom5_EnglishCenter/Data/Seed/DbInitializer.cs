using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrungTamAnhNgu.Web.Models;

namespace TrungTamAnhNgu.Web.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Roles & Admin (Bắt buộc phải có)
            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);

            // 2. Khóa học (Tạo 2 khóa cơ bản)
            await SeedCoursesAsync(context);

            // 3. Giáo viên (Tạo 2 GV để test xung đột)
            await SeedTeachersAsync(userManager, context);

            // 4. Lớp học (Tạo các lớp theo kịch bản Status)
            await SeedClassesAsync(context);

            // 5. Học viên (Tạo 1 HV để test ghi danh)
            await SeedStudentsAsync(userManager, context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Teacher", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByEmailAsync("admin@ecenter.com") == null)
            {
                var user = new ApplicationUser { UserName = "admin@ecenter.com", Email = "admin@ecenter.com", FullName = "Super Admin", EmailConfirmed = true };
                await userManager.CreateAsync(user, "Password123!");
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }

        private static async Task SeedCoursesAsync(ApplicationDbContext context)
        {
            if (await context.Courses.AnyAsync()) return;

            var courses = new List<Course>
            {
                new Course { Title = "IELTS Foundation", Description = "Nền tảng IELTS.", Price = 5000000, DurationInHours = 60, Category = "IELTS", ImageUrl = "/images/courses/ielts.jpg" },
                new Course { Title = "Tiếng Anh Giao Tiếp", Description = "Giao tiếp tự tin.", Price = 3000000, DurationInHours = 40, Category = "Giao Tiếp", ImageUrl = "/images/courses/communication.jpg" }
            };
            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();
        }

        private static async Task SeedTeachersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            if (await context.Teachers.AnyAsync()) return;

            // GV 1: Chuyên IELTS (Sẽ dùng để test xung đột lịch)
            var t1 = new ApplicationUser { UserName = "teacher1@ecenter.com", Email = "teacher1@ecenter.com", FullName = "Mr. Test A (IELTS)", EmailConfirmed = true };
            await userManager.CreateAsync(t1, "Password123!");
            await userManager.AddToRoleAsync(t1, "Teacher");
            await context.Teachers.AddAsync(new Teacher { ApplicationUserId = t1.Id, Bio = "Chuyên gia IELTS", Specialization = "IELTS" });

            // GV 2: Chuyên Giao tiếp (Rảnh rỗi)
            var t2 = new ApplicationUser { UserName = "teacher2@ecenter.com", Email = "teacher2@ecenter.com", FullName = "Ms. Test B (Giao Tiếp)", EmailConfirmed = true };
            await userManager.CreateAsync(t2, "Password123!");
            await userManager.AddToRoleAsync(t2, "Teacher");
            await context.Teachers.AddAsync(new Teacher { ApplicationUserId = t2.Id, Bio = "Chuyên gia Giao tiếp", Specialization = "Giao Tiếp" });

            await context.SaveChangesAsync();
        }

        private static async Task SeedClassesAsync(ApplicationDbContext context)
        {
            if (await context.Classes.AnyAsync()) return;

            var ielts = await context.Courses.FirstOrDefaultAsync(c => c.Title.Contains("IELTS"));
            var comm = await context.Courses.FirstOrDefaultAsync(c => c.Title.Contains("Giao Tiếp"));

            var teacher1 = await context.Teachers.Include(t => t.ApplicationUser).FirstOrDefaultAsync(t => t.ApplicationUser.Email == "teacher1@ecenter.com");
            var teacher2 = await context.Teachers.Include(t => t.ApplicationUser).FirstOrDefaultAsync(t => t.ApplicationUser.Email == "teacher2@ecenter.com");

            var classes = new List<Class>
            {
                // 1. Lớp ĐANG TUYỂN SINH (Open) - Để test đăng ký thành công
                new Class { ClassName = "IELTS-OPEN-01", CourseId = ielts.Id, TeacherId = teacher1.Id,
                            StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddMonths(3),
                            Schedule = "T2, T4, T6 | 18:00 - 20:00", // Lịch tối 2-4-6
                            MinStudents = 5, MaxStudents = 20, Status = ClassStatus.Open, Format = ClassFormat.Offline, Location = "P.101" },

                // 2. Lớp DỰ KIẾN (Planned) - Để test nút "Sắp mở"
                new Class { ClassName = "COMM-PLAN-01", CourseId = comm.Id, TeacherId = teacher2.Id,
                            StartDate = DateTime.Now.AddDays(30), EndDate = DateTime.Now.AddMonths(4),
                            Schedule = "T3, T5, T7 | 19:00 - 21:00",
                            MinStudents = 10, MaxStudents = 30, Status = ClassStatus.Planned, Format = ClassFormat.Online, MeetingUrl = "zoom.us" },

                // 3. Lớp ĐANG HỌC (Active) - Để test nút "Đang học" và test TRÙNG LỊCH
                new Class { ClassName = "IELTS-ACTIVE-01", CourseId = ielts.Id, TeacherId = teacher1.Id,
                            StartDate = DateTime.Now.AddDays(-10), EndDate = DateTime.Now.AddMonths(2),
                            Schedule = "CN | 08:00 - 10:00", // Lịch sáng CN
                            MinStudents = 5, MaxStudents = 15, Status = ClassStatus.Active, Format = ClassFormat.Offline, Location = "P.202" },

                // 4. Lớp ĐÃ KẾT THÚC (Finished) - Để test nút "Đã đóng"
                new Class { ClassName = "COMM-OLD-01", CourseId = comm.Id, TeacherId = teacher2.Id,
                            StartDate = DateTime.Now.AddMonths(-4), EndDate = DateTime.Now.AddMonths(-1),
                            Schedule = "T2, T4 | 18:00 - 20:00",
                            MinStudents = 5, MaxStudents = 20, Status = ClassStatus.Finished, Format = ClassFormat.Online }
            };

            await context.Classes.AddRangeAsync(classes);
            await context.SaveChangesAsync();
        }

        private static async Task SeedStudentsAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            if (await context.Students.AnyAsync()) return;

            // Tạo 1 học viên để test
            string email = "hv1@email.com";
            var user = new ApplicationUser { UserName = email, Email = email, FullName = "Nguyễn Văn Test", EmailConfirmed = true };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Student");

            var student = new Student { ApplicationUserId = user.Id };
            await context.Students.AddAsync(student);
            await context.SaveChangesAsync();

            // Cho học viên này ĐANG HỌC lớp Active (Lớp số 3 ở trên)
            // Mục đích: Để test xem nếu nó đăng ký lớp khác trùng giờ với lớp này thì có bị chặn không
            var activeClass = await context.Classes.FirstOrDefaultAsync(c => c.Status == ClassStatus.Active);
            if (activeClass != null)
            {
                var enrollment = new Enrollment
                {
                    StudentId = student.Id,
                    ClassId = activeClass.Id,
                    EnrollmentDate = activeClass.StartDate.AddDays(-5),
                    Status = EnrollmentStatus.Paid
                };
                await context.Enrollments.AddAsync(enrollment);
                await context.SaveChangesAsync();
            }
        }
    }
}