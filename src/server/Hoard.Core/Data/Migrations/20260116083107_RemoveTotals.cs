using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTotals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDeposits",
                table: "PortfolioSnapshot");

            migrationBuilder.DropColumn(
                name: "TotalIncome",
                table: "PortfolioSnapshot");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeposits",
                table: "PortfolioSnapshot",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalIncome",
                table: "PortfolioSnapshot",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
