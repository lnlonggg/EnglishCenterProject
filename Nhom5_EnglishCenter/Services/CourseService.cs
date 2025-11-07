using Microsoft.EntityFrameworkCore; 
using TrungTamAnhNgu.Web.Data;   
using TrungTamAnhNgu.Web.Models;

namespace TrungTamAnhNgu.Web.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _context.Courses.FindAsync(id);
        }

        public async Task<IEnumerable<Course>> GetFeaturedCoursesAsync()
        {
            return await _context.Courses.Take(3).ToListAsync();
        }
    }
}