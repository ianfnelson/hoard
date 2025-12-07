using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWeightPercentages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PortfolioWeightPercent",
                table: "PositionPerformanceCumulative");

            migrationBuilder.DropColumn(
                name: "CashWeightPercent",
                table: "PortfolioPerformanceCumulative");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PortfolioWeightPercent",
                table: "PositionPerformanceCumulative",
                type: "decimal(9,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CashWeightPercent",
                table: "PortfolioPerformanceCumulative",
                type: "decimal(9,4)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
