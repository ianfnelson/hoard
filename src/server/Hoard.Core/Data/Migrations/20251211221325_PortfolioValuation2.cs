using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioValuation2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PortfolioValuation",
                newName: "PortfolioValuationId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedUtc",
                table: "PortfolioValuation",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedUtc",
                table: "HoldingValuation",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PortfolioValuationId",
                table: "PortfolioValuation",
                newName: "Id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedUtc",
                table: "PortfolioValuation",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedUtc",
                table: "HoldingValuation",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldDefaultValueSql: "SYSUTCDATETIME()");
        }
    }
}
