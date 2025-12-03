using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhom5_EnglishCenter.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherQualification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "Teachers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Courses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Courses");
        }
    }
}
