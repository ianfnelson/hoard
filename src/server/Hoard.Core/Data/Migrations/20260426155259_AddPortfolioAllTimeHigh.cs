using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioAllTimeHigh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "AllTimeHighDate",
                table: "Portfolio",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AllTimeHighValue",
                table: "Portfolio",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllTimeHighDate",
                table: "Portfolio");

            migrationBuilder.DropColumn(
                name: "AllTimeHighValue",
                table: "Portfolio");
        }
    }
}
