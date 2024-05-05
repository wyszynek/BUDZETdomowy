using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Migrations
{
    /// <inheritdoc />
    public partial class currencyInTBA2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "TransactionBetweenAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionBetweenAccounts_CurrencyId",
                table: "TransactionBetweenAccounts",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionBetweenAccounts_Currencies_CurrencyId",
                table: "TransactionBetweenAccounts",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionBetweenAccounts_Currencies_CurrencyId",
                table: "TransactionBetweenAccounts");

            migrationBuilder.DropIndex(
                name: "IX_TransactionBetweenAccounts_CurrencyId",
                table: "TransactionBetweenAccounts");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "TransactionBetweenAccounts");
        }
    }
}
