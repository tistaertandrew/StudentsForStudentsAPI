using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentsForStudentsAPI.Migrations
{
    public partial class addCourses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CursusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "CursusId", "Label" },
                values: new object[,]
                {
                    { 1, 1, "UE1 - Programmation de base (B1)" },
                    { 2, 1, "UE2 - Architecture des ordinateurs (B1)" },
                    { 3, 1, "UE3 - Conception d'applications 1 (B1)" },
                    { 4, 1, "UE35 - Communication écrite et orale en langue française (B1)" },
                    { 5, 1, "UE4 - Mathématiques appliquées à l'informatique 1 (B1)" },
                    { 6, 1, "UE5 - Base de données (B1)" },
                    { 7, 1, "UE6 - Développement web (B1)" },
                    { 8, 1, "UE7 - Anglais (B1)" },
                    { 9, 1, "UE8 - E-Business (B1)" },
                    { 10, 1, "UE9 - Programmation intermédiaire (B1)" },
                    { 11, 1, "UE13 - Systèmes d'exploitation (B2)" },
                    { 12, 1, "UE14 - Anglais (B2)" },
                    { 13, 1, "UE15 - Droit et Ethique du monde numérique (B2)" },
                    { 14, 1, "UE16 - Digitalisation et nouvelle économie (B2)" },
                    { 15, 1, "UE17 - Mathématiques appliquées à l'informatique 2 (B2)" },
                    { 16, 1, "UE18 - Développement mobile (B2)" },
                    { 17, 1, "UE19 - Développement web avancé (B2)" },
                    { 18, 1, "UE20 - Langage de scripts dynamiques (B2)" },
                    { 19, 1, "UE21 - Réseaux informatiques (B2)" },
                    { 20, 1, "UE36 - Programmation avancée (B2)" },
                    { 21, 1, "UE10 - Conception d'applications 2 (B2)" },
                    { 22, 1, "UE22 - Laboratoire pluridisciplinaire (B2)" },
                    { 23, 1, "UE23 - SALTo (B2)" },
                    { 24, 1, "UE25 - Architectures logicielles (B3)" },
                    { 25, 1, "UE26 - Frameworks web (B3)" },
                    { 26, 1, "UE27 - Entrepreneuriat (B3)" },
                    { 27, 1, "UE28 - Savoir être, culture d'entreprise (B3)" },
                    { 28, 1, "UE29 - Informatique managériale (B3)" },
                    { 29, 1, "UE30 - Stage et travail de fin d'études (B3)" },
                    { 30, 1, "UE31 - Mémoire (B3)" },
                    { 31, 1, "UE32 - Administration réseau et système (LINUX) (B3)" },
                    { 32, 1, "UE24 - Administration réseau et système (WINDOWS) (B3)" },
                    { 33, 1, "UE33 - Conférences - Visites - Séminaires (B3)" },
                    { 34, 1, "UE34 - SALTo (B3)" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Courses");
        }
    }
}
