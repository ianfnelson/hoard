using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Yield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalIncomePromotion",
                table: "PortfolioSnapshot",
                newName: "TotalPromotion");

            migrationBuilder.AddColumn<decimal>(
                name: "Yield",
                table: "PortfolioSnapshot",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Yield",
                table: "PortfolioPerformance",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Yield",
                table: "PortfolioSnapshot");

            migrationBuilder.DropColumn(
                name: "Yield",
                table: "PortfolioPerformance");

            migrationBuilder.RenameColumn(
                name: "TotalPromotion",
                table: "PortfolioSnapshot",
                newName: "TotalIncomePromotion");
        }
    }
}
