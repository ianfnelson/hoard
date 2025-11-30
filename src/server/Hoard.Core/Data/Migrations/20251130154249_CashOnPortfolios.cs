using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class CashOnPortfolios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CashValue",
                table: "PortfolioPerformanceCumulative",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CashWeightPercent",
                table: "PortfolioPerformanceCumulative",
                type: "decimal(9,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashValue",
                table: "PortfolioPerformanceCumulative");

            migrationBuilder.DropColumn(
                name: "CashWeightPercent",
                table: "PortfolioPerformanceCumulative");
        }
    }
}
