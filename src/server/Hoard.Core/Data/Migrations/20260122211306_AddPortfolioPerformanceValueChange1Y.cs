using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioPerformanceValueChange1Y : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValueChange1Y",
                table: "PositionPerformance",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValueChange1Y",
                table: "PortfolioPerformance",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValueChange1Y",
                table: "PositionPerformance");

            migrationBuilder.DropColumn(
                name: "ValueChange1Y",
                table: "PortfolioPerformance");
        }
    }
}
