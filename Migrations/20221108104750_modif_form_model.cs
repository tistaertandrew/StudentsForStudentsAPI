using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentsForStudentsAPI.Migrations
{
    public partial class modif_form_model : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderEmail",
                table: "Forms",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderEmail",
                table: "Forms");
        }
    }
}
