using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePointlessDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "TransactionSubcategory");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "TransactionCategory");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "InstrumentType");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "TransactionSubcategory",
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
                table: "InstrumentType",
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
        }
    }
}
