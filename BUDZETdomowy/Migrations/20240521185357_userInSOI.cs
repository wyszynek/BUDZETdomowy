using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Migrations
{
    /// <inheritdoc />
    public partial class userInSOI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "SourceOfIncomes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SourceOfIncomes_UserId",
                table: "SourceOfIncomes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SourceOfIncomes_Users_UserId",
                table: "SourceOfIncomes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SourceOfIncomes_Users_UserId",
                table: "SourceOfIncomes");

            migrationBuilder.DropIndex(
                name: "IX_SourceOfIncomes_UserId",
                table: "SourceOfIncomes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SourceOfIncomes");
        }
    }
}
