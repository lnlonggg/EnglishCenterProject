using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhom5_EnglishCenter.Migrations
{
    /// <inheritdoc />
    public partial class AddClassLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinStudents",
                table: "Classes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Classes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinStudents",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Classes");
        }
    }
}
