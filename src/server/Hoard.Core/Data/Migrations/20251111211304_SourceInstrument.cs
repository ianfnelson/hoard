using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class SourceInstrument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstrumentId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_InstrumentId",
                table: "Transaction",
                column: "InstrumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Instrument_InstrumentId",
                table: "Transaction",
                column: "InstrumentId",
                principalTable: "Instrument",
                principalColumn: "InstrumentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Instrument_InstrumentId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_InstrumentId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "InstrumentId",
                table: "Transaction");
        }
    }
}
