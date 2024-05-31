using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Migrations
{
    /// <inheritdoc />
    public partial class userInRecurringPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ReccuringPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReccuringPayments_UserId",
                table: "ReccuringPayments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReccuringPayments_Users_UserId",
                table: "ReccuringPayments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReccuringPayments_Users_UserId",
                table: "ReccuringPayments");

            migrationBuilder.DropIndex(
                name: "IX_ReccuringPayments_UserId",
                table: "ReccuringPayments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ReccuringPayments");
        }
    }
}
