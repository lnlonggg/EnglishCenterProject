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

            // 1. Roles & Admin
            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);

            // 2. Khóa học
            await SeedCoursesAsync(context);

            // 3. Giáo viên (Đủ 15 người)
            await SeedTeachersAsync(userManager, context);

            // 4. Lớp học (Đủ số lượng)
            await SeedClassesAsync(context);

            // 5. Học viên (Đủ 100 người) - FIX LỖI TẠI ĐÂY
            await SeedStudentsAsync(userManager, context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Teacher", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
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
            if (!await context.Courses.AnyAsync())
            {
                var courses = new List<Course>
                {
                    new Course { Title = "IELTS Intensive 6.5", Description = "Khóa học IELTS toàn diện 4 kỹ năng.", Price = 11000000, DurationInHours = 120, ImageUrl = "https://placehold.co/400x250/004A99/FFFFFF?text=IELTS" },
                    new Course { Title = "TOEIC Conquer 750+", Description = "Luyện thi TOEIC cấp tốc.", Price = 6500000, DurationInHours = 96, ImageUrl = "https://placehold.co/400x250/FFD700/004A99?text=TOEIC" },
                    new Course { Title = "Tiếng Anh Giao Tiếp Pro", Description = "Tự tin giao tiếp công sở.", Price = 4800000, DurationInHours = 60, ImageUrl = "https://placehold.co/400x250/28a745/FFFFFF?text=Communication" },
                };
                await context.Courses.AddRangeAsync(courses);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTeachersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            int currentCount = await context.Teachers.CountAsync();
            if (currentCount < 15)
            {
                for (int i = currentCount + 1; i <= 15; i++)
                {
                    string email = $"teacher{i}@ecenter.com";
                    if (await userManager.FindByEmailAsync(email) == null)
                    {
                        var user = new ApplicationUser { UserName = email, Email = email, FullName = $"Giáo Viên {i}", EmailConfirmed = true };
                        var result = await userManager.CreateAsync(user, "Password123!");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "Teacher");
                            await context.Teachers.AddAsync(new Teacher { ApplicationUserId = user.Id, Bio = "Giảng viên kinh nghiệm.", AvatarUrl = $"https://placehold.co/150?text=GV{i}" });
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedClassesAsync(ApplicationDbContext context)
        {
            var teachers = await context.Teachers.ToListAsync();
            var courses = await context.Courses.ToListAsync();
            var random = new Random();

            if (teachers.Any() && courses.Any() && await context.Classes.CountAsync() < 30)
            {
                for (int i = 0; i < 20; i++)
                {
                    var course = courses[random.Next(courses.Count)];
                    var teacher = teachers[random.Next(teachers.Count)];
                    var startDate = DateTime.Now.AddDays(random.Next(-60, 60));

                    var newClass = new Class
                    {
                        ClassName = $"{course.Title.Substring(0, 3).ToUpper()}-{random.Next(100, 999)}",
                        StartDate = startDate,
                        EndDate = startDate.AddMonths(3),
                        Schedule = random.Next(0, 2) == 0 ? "T2-T4-T6 18h" : "T3-T5-T7 19h",
                        MaxStudents = 20,
                        Format = (ClassFormat)random.Next(0, 2),
                        Location = "Phòng " + random.Next(101, 404),
                        CourseId = course.Id,
                        TeacherId = teacher.Id
                    };
                    await context.Classes.AddAsync(newClass);
                }
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedStudentsAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            int currentStudents = await context.Students.CountAsync();
            var classes = await context.Classes.Include(c => c.Course).ToListAsync();
            var random = new Random();

            // Chỉ chạy nếu chưa đủ 100 học viên
            if (currentStudents < 100 && classes.Any())
            {
                // ĐỔI "student" THÀNH "hv" ĐỂ TRÁNH TRÙNG USER CŨ
                for (int i = currentStudents + 1; i <= 100; i++)
                {
                    string email = $"hv{i}@email.com"; // <<--- ĐỔI EMAIL MỚI

                    // Kiểm tra kỹ nếu user đã tồn tại thì bỏ qua
                    if (await userManager.FindByEmailAsync(email) == null)
                    {
                        var user = new ApplicationUser { UserName = email, Email = email, FullName = $"Học Viên {i}", EmailConfirmed = true };
                        var result = await userManager.CreateAsync(user, "Password123!");

                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "Student");
                            var studentProfile = new Student { ApplicationUserId = user.Id };
                            await context.Students.AddAsync(studentProfile);
                            // Lưu Student ngay để lấy Id dùng cho Enrollment
                            await context.SaveChangesAsync();

                            // Ghi danh ngẫu nhiên
                            var class1 = classes[random.Next(classes.Count)];
                            var enrollment = new Enrollment
                            {
                                StudentId = studentProfile.Id,
                                ClassId = class1.Id,
                                EnrollmentDate = DateTime.Now.AddDays(-random.Next(1, 30)),
                                Status = EnrollmentStatus.Paid,
                                FinalGrade = (decimal)(random.Next(50, 95) / 10.0)
                            };
                            await context.Enrollments.AddAsync(enrollment);
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}