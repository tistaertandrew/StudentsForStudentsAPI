using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentsForStudentsAPI.Migrations
{
    public partial class modifiernomchampmodelefile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_AspNetUsers_OnwerId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "OnwerId",
                table: "Files",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_OnwerId",
                table: "Files",
                newName: "IX_Files_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_AspNetUsers_OwnerId",
                table: "Files",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_AspNetUsers_OwnerId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Files",
                newName: "OnwerId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_OwnerId",
                table: "Files",
                newName: "IX_Files_OnwerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_AspNetUsers_OnwerId",
                table: "Files",
                column: "OnwerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
