using TrungTamAnhNgu.Web.Models;

namespace TrungTamAnhNgu.Web.Services
{
    // Đây là Interface (Hợp đồng)
    // Nó định nghĩa "CÁI GÌ" có thể làm, không phải "LÀM NHƯ THẾ NÀO"
    // Giúp thực hiện nguyên tắc Dependency Inversion (D) và Open/Closed (O)
    public interface ICourseService
    {
        // Lấy danh sách các khóa học nổi bật cho trang chủ
        Task<IEnumerable<Course>> GetFeaturedCoursesAsync();

        // Lấy tất cả khóa học
        Task<IEnumerable<Course>> GetAllCoursesAsync();

        // Lấy chi tiết một khóa học
        Task<Course?> GetCourseByIdAsync(int id);

        // Thêm các hàm khác sau (Create, Update, Delete...)
    }
}
