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
            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);
            await SeedTeachersAsync(userManager, context);
            await SeedAcademicDataAsync(context);
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
            string adminEmail = "admin@ecenter.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                ApplicationUser adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Super Admin",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedTeachersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            if (await context.Teachers.AnyAsync())
            {
                return;
            }

 
            var teacherUser1 = new ApplicationUser
            {
                UserName = "john.doe@ecenter.com",
                Email = "john.doe@ecenter.com",
                FullName = "John Doe",
                EmailConfirmed = true
            };
            var result1 = await userManager.CreateAsync(teacherUser1, "Password123!");
            if (result1.Succeeded)
            {
                await userManager.AddToRoleAsync(teacherUser1, "Teacher");
                var teacherProfile1 = new Teacher
                {
                    ApplicationUserId = teacherUser1.Id,
                    Bio = "8.5 IELTS, 5 năm kinh nghiệm giảng dạy tại các trung tâm lớn.",
                    AvatarUrl = "https://placehold.co/100x100/E2E8F0/4A5568?text=JohnD"
                };
                await context.Teachers.AddAsync(teacherProfile1);
            }

            var teacherUser2 = new ApplicationUser
            {
                UserName = "jane.smith@ecenter.com",
                Email = "jane.smith@ecenter.com",
                FullName = "Jane Smith",
                EmailConfirmed = true
            };
            var result2 = await userManager.CreateAsync(teacherUser2, "Password123!");
            if (result2.Succeeded)
            {
                await userManager.AddToRoleAsync(teacherUser2, "Teacher");
                var teacherProfile2 = new Teacher
                {
                    ApplicationUserId = teacherUser2.Id,
                    Bio = "Chuyên gia luyện thi TOEIC 990. Tác giả nhiều đầu sách bán chạy.",
                    AvatarUrl = "https://placehold.co/100x100/E2E8F0/4A5568?text=JaneS"
                };
                await context.Teachers.AddAsync(teacherProfile2);
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedAcademicDataAsync(ApplicationDbContext context)
        {
            if (await context.Courses.AnyAsync())
            {
                return;
            }

            var teacher1 = await context.Teachers.FirstOrDefaultAsync(t => t.ApplicationUser.Email == "john.doe@ecenter.com");
            var teacher2 = await context.Teachers.FirstOrDefaultAsync(t => t.ApplicationUser.Email == "jane.smith@ecenter.com");

            if (teacher1 == null || teacher2 == null)
            {
                return;
            }

            var courseIELTS = new Course
            {
                Title = "IELTS Intensive 6.5",
                Description = "Khóa học IELTS toàn diện 4 kỹ năng. Xây dựng tư duy ngôn ngữ học thuật và kỹ năng làm bài thi hiệu quả.",
                Price = 11000000,
                DurationInHours = 120,
                ImageUrl = "https://placehold.co/400x250/E2E8F0/4A5568?text=IELTS+6.5"
            };

            var courseTOEIC = new Course
            {
                Title = "TOEIC Conquer 750+",
                Description = "Chương trình luyện thi TOEIC cấp tốc, tập trung vào chiến lược giải đề chuyên sâu và các chủ điểm ngữ pháp.",
                Price = 6500000,
                DurationInHours = 96,
                ImageUrl = "https://placehold.co/400x250/E2E8F0/4A5568?text=TOEIC+750"
            };

            var courseGiaoTiep = new Course
            {
                Title = "Communicative English for Professionals",
                Description = "Tiếng Anh giao tiếp cho người đi làm. Tập trung vào các tình huống thực tế: họp, thuyết trình, viết email.",
                Price = 4800000,
                DurationInHours = 60,
                ImageUrl = "https://placehold.co/400x250/E2E8F0/4A5568?text=Giao+Tiep"
            };

            await context.Courses.AddRangeAsync(courseIELTS, courseTOEIC, courseGiaoTiep);
            await context.SaveChangesAsync();

            var class1 = new Class
            {
                ClassName = "IELTS6.5-S357-K11",
                StartDate = new DateTime(2025, 11, 15),
                EndDate = new DateTime(2025, 2, 15),
                Schedule = "09:00 - 11:00, T3-T5-T7",
                MaxStudents = 15,
                Format = ClassFormat.Offline,
                Location = "Phòng 201, Tòa A",
                CourseId = courseIELTS.Id,
                TeacherId = teacher1.Id
            };

            var class2 = new Class
            {
                ClassName = "IELTS6.5-ONLINE-K11",
                StartDate = new DateTime(2025, 11, 20),
                EndDate = new DateTime(2025, 2, 20),
                Schedule = "20:00 - 22:00, T2-T4-T6",
                MaxStudents = 25,
                Format = ClassFormat.Online,
                MeetingUrl = "https://zoom.us/j/123456789",
                CourseId = courseIELTS.Id,
                TeacherId = teacher1.Id
            };

            var class3 = new Class
            {
                ClassName = "TOEIC750-T246-K11",
                StartDate = new DateTime(2025, 11, 10),
                EndDate = new DateTime(2025, 1, 10),
                Schedule = "18:00 - 20:00, T2-T4-T6",
                MaxStudents = 20,
                Format = ClassFormat.Offline,
                Location = "Phòng 303, Tòa B",
                CourseId = courseTOEIC.Id,
                TeacherId = teacher2.Id
            };

            var class4 = new Class
            {
                ClassName = "GiaoTiep-ONLINE-K11",
                StartDate = new DateTime(2025, 11, 12),
                EndDate = new DateTime(2025, 1, 12),
                Schedule = "19:00 - 21:00, T3-T5",
                MaxStudents = 25,
                Format = ClassFormat.Online,
                MeetingUrl = "https://meet.google.com/xyz-abc-def",
                CourseId = courseGiaoTiep.Id,
                TeacherId = teacher2.Id
            };

            await context.Classes.AddRangeAsync(class1, class2, class3, class4);
            await context.SaveChangesAsync();
        }
    }
}