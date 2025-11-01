using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class ValuationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountValuation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValuationGbp = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountValuation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountValuation_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentValuation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValuationGbp = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentValuation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrumentValuation_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioValuation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValuationGbp = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioValuation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioValuation_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountValuation_AccountId_AsOfDate",
                table: "AccountValuation",
                columns: new[] { "AccountId", "AsOfDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentValuation_InstrumentId_AsOfDate",
                table: "InstrumentValuation",
                columns: new[] { "InstrumentId", "AsOfDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioValuation_PortfolioId_AsOfDate",
                table: "PortfolioValuation",
                columns: new[] { "PortfolioId", "AsOfDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountValuation");

            migrationBuilder.DropTable(
                name: "InstrumentValuation");

            migrationBuilder.DropTable(
                name: "PortfolioValuation");
        }
    }
}
