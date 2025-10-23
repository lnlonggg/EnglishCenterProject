using TrungTamAnhNgu.Web.Models;

namespace TrungTamAnhNgu.Web.Services
{
    // Đây là lớp triển khai (Implementation) của ICourseService
    // Nó định nghĩa "LÀM NHƯ THẾ NÀO"
    // Lớp này sẽ chứa logic nghiệp vụ và gọi tới DbContext
    public class CourseService : ICourseService
    {
        // Chúng ta sẽ tiêm (inject) DbContext vào đây sau khi cài đặt EF Core
        // private readonly ApplicationDbContext _context;
        // public CourseService(ApplicationDbContext context)
        // {
        //     _context = context;
        // }


        // ---- VÍ DỤ VỚI DỮ LIỆU GIẢ (Mock Data) TRƯỚC KHI CÓ DB ----

        private readonly List<Course> _mockCourses = new List<Course>
        {
            new Course { Id = 1, Name = "IELTS Foundation", Description = "Xây dựng nền tảng vững chắc cho kỳ thi IELTS, tập trung vào 4 kỹ năng.", Price = 5000000, ImageUrl = "https://placehold.co/400x250/E2E8F0/4A5568?text=IELTS+Foundation" },
            new Course { Id = 2, Name = "Giao tiếp Chuyên nghiệp", Description = "Tự tin giao tiếp trong môi trường công sở, thuyết trình và đàm phán.", Price = 3500000, ImageUrl = "https://placehold.co/400x250/E2E8F0/4A5568?text=Giao+Tiep" },
            new Course { Id = 3, Name = "TOEIC Mastery", Description = "Luyện thi TOEIC cấp tốc, tập trung vào chiến lược làm bài và từ vựng.", Price = 4000000, ImageUrl = "https://placehold.co/400x250/E2E8F0/4A5568?text=TOEIC+Mastery" }
        };

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            // Khi có DB:
            // return await _context.Courses.ToListAsync();

            // Dùng dữ liệu giả:
            return await Task.FromResult(_mockCourses);
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            // Khi có DB:
            // return await _context.Courses.FindAsync(id);

            // Dùng dữ liệu giả:
            return await Task.FromResult(_mockCourses.FirstOrDefault(c => c.Id == id));
        }

        public async Task<IEnumerable<Course>> GetFeaturedCoursesAsync()
        {
            // Khi có DB:
            // return await _context.Courses.Where(c => c.IsFeatured).Take(3).ToListAsync();

            // Dùng dữ liệu giả (lấy 3 khóa đầu tiên):
            return await Task.FromResult(_mockCourses.Take(3));
        }
    }
}
