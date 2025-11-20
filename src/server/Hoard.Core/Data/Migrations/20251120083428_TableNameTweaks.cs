using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class TableNameTweaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoldingValuation");

            migrationBuilder.DropTable(
                name: "PortfolioAssetTarget");

            migrationBuilder.CreateTable(
                name: "TargetAllocation",
                columns: table => new
                {
                    TargetAllocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    AssetSubclassId = table.Column<int>(type: "int", nullable: false),
                    Target = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetAllocation", x => x.TargetAllocationId);
                    table.CheckConstraint("CK_Target_0_100", "[Target] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_TargetAllocation_AssetSubclass_AssetSubclassId",
                        column: x => x.AssetSubclassId,
                        principalTable: "AssetSubclass",
                        principalColumn: "AssetSubclassId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TargetAllocation_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Valuation",
                columns: table => new
                {
                    ValuationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoldingId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
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
                name: "IX_TargetAllocation_AssetSubclassId",
                table: "TargetAllocation",
                column: "AssetSubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetAllocation_PortfolioId_AssetSubclassId",
                table: "TargetAllocation",
                columns: new[] { "PortfolioId", "AssetSubclassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Valuation_HoldingId",
                table: "Valuation",
                column: "HoldingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TargetAllocation");

            migrationBuilder.DropTable(
                name: "Valuation");

            migrationBuilder.CreateTable(
                name: "HoldingValuation",
                columns: table => new
                {
                    HoldingValuationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoldingId = table.Column<int>(type: "int", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "PortfolioAssetTarget",
                columns: table => new
                {
                    PortfolioAssetTargetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetSubclassId = table.Column<int>(type: "int", nullable: false),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Target = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioAssetTarget", x => x.PortfolioAssetTargetId);
                    table.CheckConstraint("CK_Target_0_100", "[Target] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_PortfolioAssetTarget_AssetSubclass_AssetSubclassId",
                        column: x => x.AssetSubclassId,
                        principalTable: "AssetSubclass",
                        principalColumn: "AssetSubclassId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioAssetTarget_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoldingValuation_HoldingId",
                table: "HoldingValuation",
                column: "HoldingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioAssetTarget_AssetSubclassId",
                table: "PortfolioAssetTarget",
                column: "AssetSubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioAssetTarget_PortfolioId_AssetSubclassId",
                table: "PortfolioAssetTarget",
                columns: new[] { "PortfolioId", "AssetSubclassId" },
                unique: true);
        }
    }
}
