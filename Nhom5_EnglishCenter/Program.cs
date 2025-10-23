// Thêm các using cần thiết
using TrungTamAnhNgu.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Thêm dịch vụ vào container (Service Container) ---

builder.Services.AddControllersWithViews();

// --- Đây là bước quan trọng của SOLID (Dependency Injection) ---
// Đăng ký Service: Khi một Controller yêu cầu ICourseService,
// hãy cung cấp cho nó một instance của CourseService.
// AddScoped: Tạo một instance mới cho mỗi HTTP request.
builder.Services.AddScoped<ICourseService, CourseService>();

// Chúng ta sẽ thêm DbContext và Identity ở đây sau
// builder.Services.AddDbContext<...>(...)
// builder.Services.AddDefaultIdentity<...>(...)


var app = builder.Build();

// --- 2. Cấu hình HTTP request pipeline ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Sử dụng các file tĩnh trong wwwroot (nếu có)

app.UseRouting();

// Thêm UseAuthentication() và UseAuthorization() ở đây sau
// app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
