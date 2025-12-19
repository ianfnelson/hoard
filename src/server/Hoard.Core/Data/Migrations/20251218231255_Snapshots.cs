using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Snapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortfolioSnapshot",
                columns: table => new
                {
                    PortfolioSnapshotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    StartValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Return = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Churn = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalBuys = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSells = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncomeInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncomeLoyaltyBonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncomePromotion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncomeDividends = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalFees = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDealingCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalStampDuty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPtmLevy = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalFxCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositPersonal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositEmployer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositIncomeTaxReclaim = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositTransferIn = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalWithdrawals = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CountTrades = table.Column<int>(type: "int", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioSnapshot", x => x.PortfolioSnapshotId);
                    table.ForeignKey(
                        name: "FK_PortfolioSnapshot_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSnapshot_PortfolioId",
                table: "PortfolioSnapshot",
                column: "PortfolioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioSnapshot");
        }
    }
}
