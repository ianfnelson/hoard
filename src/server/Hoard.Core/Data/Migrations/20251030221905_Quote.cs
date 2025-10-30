using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Quote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    RetrievedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bid = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Ask = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    FiftyTwoWeekHigh = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    FiftyTwoWeekLow = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    RegularMarketPrice = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    RegularMarketChange = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    RegularMarketChangePercent = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quote_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quote_InstrumentId",
                table: "Quote",
                column: "InstrumentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quote");
        }
    }
}
