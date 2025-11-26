using Microsoft.AspNetCore.Mvc;
using Nhom5_EnglishCenter.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using TrungTamAnhNgu.Web.Data;
using TrungTamAnhNgu.Web.Models;
using TrungTamAnhNgu.Web.Services;
using TrungTamAnhNgu.Web.ViewModels;

namespace Nhom5_EnglishCenter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseService _courseService;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ICourseService courseService, ApplicationDbContext context)
        {
            _logger = logger;
            _courseService = courseService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Courses", new { area = "Admin" });
                }

                if (User.IsInRole("Teacher"))
                {
                    return RedirectToAction("Index", "TeacherDashboard");
                }
            }

            var featuredCourses = await _courseService.GetFeaturedCoursesAsync();
            return View(featuredCourses);
        }

        // GET: /Home/Error/{statusCode}
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? id) // id statusCode (404, 403, 500)
        {
            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            if (id.HasValue)
            {
                if (id == 404)
                {
                    ViewBag.ErrorMessage = "Xin lỗi, trang bạn tìm kiếm không tồn tại.";
                    ViewBag.ErrorTitle = "404 - Không tìm thấy";
                }
                else if (id == 403)
                {
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập vào trang này.";
                    ViewBag.ErrorTitle = "403 - Từ chối truy cập";
                }
                else
                {
                    ViewBag.ErrorMessage = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.";
                    ViewBag.ErrorTitle = "Lỗi Hệ Thống";
                }
            }
            else
            {
                // Lỗi 500 hoặc Exception
                ViewBag.ErrorMessage = "Hệ thống đang gặp sự cố. Chúng tôi đang khắc phục.";
                ViewBag.ErrorTitle = "Đã xảy ra lỗi";
            }

            return View("Error");
        }

        // GET: /Home/Contact
        public IActionResult Contact(string? subject)
        {
            var model = new ContactViewModel();
            if (!string.IsNullOrEmpty(subject))
            {
                model.Subject = subject;
            }
            return View(model);
        }

        // POST: /Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Chuyển đổi từ ViewModel sang Model thực thể
                var contactEntity = new Contact
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Subject = model.Subject,
                    Message = model.Message,
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };

                // 2. Lưu vào Database
                _context.Contacts.Add(contactEntity);
                await _context.SaveChangesAsync();

                // 3. Thông báo thành công
                TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Yêu cầu của bạn đã được gửi đến bộ phận tư vấn.";
                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}