using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentsForStudentsAPI.Migrations
{
    public partial class ajoutvaleurssectioncursus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Sections",
                columns: new[] { "Id", "Label" },
                values: new object[] { 1, "Technique" });

            migrationBuilder.InsertData(
                table: "Cursus",
                columns: new[] { "Id", "Label", "SectionId" },
                values: new object[] { 1, "Développement d'applications", 1 });

            migrationBuilder.InsertData(
                table: "Cursus",
                columns: new[] { "Id", "Label", "SectionId" },
                values: new object[] { 2, "Cybersécurité", 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cursus",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cursus",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Sections",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
