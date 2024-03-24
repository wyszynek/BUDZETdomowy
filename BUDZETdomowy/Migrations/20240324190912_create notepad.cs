using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUDZETdomowy.Migrations
{
    /// <inheritdoc />
    public partial class createnotepad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notepad",
                columns: table => new
                {
                    NoteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(35)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notepad", x => x.NoteID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notepad");
        }
    }
}
