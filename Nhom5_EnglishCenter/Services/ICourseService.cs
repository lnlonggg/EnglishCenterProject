using TrungTamAnhNgu.Web.Models;

namespace TrungTamAnhNgu.Web.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetFeaturedCoursesAsync();

        Task<IEnumerable<Course>> GetAllCoursesAsync();

        Task<Course?> GetCourseByIdAsync(int id);

    }
}
