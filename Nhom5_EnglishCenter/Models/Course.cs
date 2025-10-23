namespace TrungTamAnhNgu.Web.Models
{
    // Đây là lớp POCO (Plain Old C# Object) đại diện cho dữ liệu
    // Sẽ được ánh xạ tới bảng trong DB bởi Entity Framework Core
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; } // Dấu ? cho phép giá trị là null

        // Thêm các thuộc tính khác sau...
    }
}
