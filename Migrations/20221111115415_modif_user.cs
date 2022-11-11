using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentsForStudentsAPI.Migrations
{
    public partial class modif_user : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalendarLink",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalendarLink",
                table: "AspNetUsers");
        }
    }
}
