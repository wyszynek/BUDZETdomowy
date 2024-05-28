using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Migrations
{
    /// <inheritdoc />
    public partial class cascade9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_RecipientId",
                table: "TransactionBetweenAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_SenderId",
                table: "TransactionBetweenAccounts");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_RecipientId",
                table: "TransactionBetweenAccounts",
                column: "RecipientId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_SenderId",
                table: "TransactionBetweenAccounts",
                column: "SenderId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_RecipientId",
                table: "TransactionBetweenAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_SenderId",
                table: "TransactionBetweenAccounts");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_RecipientId",
                table: "TransactionBetweenAccounts",
                column: "RecipientId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionBetweenAccounts_Accounts_SenderId",
                table: "TransactionBetweenAccounts",
                column: "SenderId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
