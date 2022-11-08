using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentsForStudentsAPI.Migrations
{
    public partial class ajout_formulaire : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HandlerId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forms_AspNetUsers_HandlerId",
                        column: x => x.HandlerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Forms_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_HandlerId",
                table: "Forms",
                column: "HandlerId");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_SenderId",
                table: "Forms",
                column: "SenderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Forms");
        }
    }
}
