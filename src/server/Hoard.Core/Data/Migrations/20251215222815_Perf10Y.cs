using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Perf10Y : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Return10Y",
                table: "PositionPerformanceCumulative",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Return10Y",
                table: "PortfolioPerformanceCumulative",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Return10Y",
                table: "PositionPerformanceCumulative");

            migrationBuilder.DropColumn(
                name: "Return10Y",
                table: "PortfolioPerformanceCumulative");
        }
    }
}
