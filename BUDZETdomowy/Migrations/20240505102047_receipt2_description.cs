using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Migrations
{
    /// <inheritdoc />
    public partial class receipt2_description : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Receipts2",
                type: "nvarchar(150)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Receipts2");
        }
    }
}
