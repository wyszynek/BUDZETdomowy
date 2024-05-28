using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Migrations
{
    /// <inheritdoc />
    public partial class clientSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_SenderId",
                table: "TransactionBetweenAccounts");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_SenderId",
                table: "TransactionBetweenAccounts",
                column: "SenderId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_SenderId",
                table: "TransactionBetweenAccounts");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_SenderId",
                table: "TransactionBetweenAccounts",
                column: "SenderId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
