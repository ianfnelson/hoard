using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class HoldingValuations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Valuation");

            migrationBuilder.CreateTable(
                name: "HoldingValuation",
                columns: table => new
                {
                    HoldingValuationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoldingId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoldingValuation", x => x.HoldingValuationId);
                    table.ForeignKey(
                        name: "FK_HoldingValuation_Holding_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "Holding",
                        principalColumn: "HoldingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoldingValuation_HoldingId",
                table: "HoldingValuation",
                column: "HoldingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoldingValuation");

            migrationBuilder.CreateTable(
                name: "Valuation",
                columns: table => new
                {
                    ValuationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoldingId = table.Column<int>(type: "int", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Valuation", x => x.ValuationId);
                    table.ForeignKey(
                        name: "FK_Valuation_Holding_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "Holding",
                        principalColumn: "HoldingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Valuation_HoldingId",
                table: "Valuation",
                column: "HoldingId",
                unique: true);
        }
    }
}
