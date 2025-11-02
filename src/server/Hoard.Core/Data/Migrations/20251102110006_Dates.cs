using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Dates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "TransactionLegSubcategory",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "TransactionLegCategory",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "TransactionLeg",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "TransactionCategory",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Transaction",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "PortfolioAssetTarget",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Portfolio",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "InstrumentType",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Instrument",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "Holding",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Currency",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "AssetSubclass",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "AssetClass",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "AccountType",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Account",
                type: "datetime2(3)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "TransactionLegSubcategory");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "TransactionLegCategory");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "TransactionLeg");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "TransactionCategory");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "PortfolioAssetTarget");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Portfolio");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "InstrumentType");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Instrument");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "Holding");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Currency");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "AssetSubclass");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "AssetClass");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "AccountType");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Account");
        }
    }
}
