using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class BetterValueChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousValue",
                table: "PositionPerformance");

            migrationBuilder.DropColumn(
                name: "ValueChange",
                table: "PositionPerformance");

            migrationBuilder.DropColumn(
                name: "PreviousValue",
                table: "PortfolioPerformance");

            migrationBuilder.DropColumn(
                name: "ValueChange",
                table: "PortfolioPerformance");

            migrationBuilder.AddColumn<decimal>(
                name: "ValueChange1D",
                table: "PositionPerformance",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValueChange1D",
                table: "PortfolioPerformance",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValueChange1D",
                table: "PositionPerformance");

            migrationBuilder.DropColumn(
                name: "ValueChange1D",
                table: "PortfolioPerformance");

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousValue",
                table: "PositionPerformance",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValueChange",
                table: "PositionPerformance",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousValue",
                table: "PortfolioPerformance",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValueChange",
                table: "PortfolioPerformance",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
