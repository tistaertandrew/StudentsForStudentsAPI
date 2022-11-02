using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentsForStudentsAPI.Migrations
{
    public partial class ajoutvaleurssectionsetcursus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Sections",
                columns: new[] { "Id", "Label" },
                values: new object[] { 2, "Economique" });

            migrationBuilder.InsertData(
                table: "Cursus",
                columns: new[] { "Id", "Label", "SectionId" },
                values: new object[] { 3, "Marketing", 2 });

            migrationBuilder.InsertData(
                table: "Cursus",
                columns: new[] { "Id", "Label", "SectionId" },
                values: new object[] { 4, "Droit", 2 });

            migrationBuilder.InsertData(
                table: "Cursus",
                columns: new[] { "Id", "Label", "SectionId" },
                values: new object[] { 5, "Commerce Extérieur", 2 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cursus",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Cursus",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Cursus",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Sections",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
