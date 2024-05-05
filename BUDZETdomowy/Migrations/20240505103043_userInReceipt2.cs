using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Migrations
{
    /// <inheritdoc />
    public partial class userInReceipt2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Receipts2",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts2_UserId",
                table: "Receipts2",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts2_Users_UserId",
                table: "Receipts2",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts2_Users_UserId",
                table: "Receipts2");

            migrationBuilder.DropIndex(
                name: "IX_Receipts2_UserId",
                table: "Receipts2");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Receipts2");
        }
    }
}
