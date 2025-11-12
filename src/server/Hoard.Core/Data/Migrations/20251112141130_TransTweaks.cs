using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class TransTweaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TransactionLeg");

            migrationBuilder.AddColumn<string>(
                name: "ContractNoteReference",
                table: "Transaction",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Class",
                table: "Instrument",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                table: "Instrument",
                type: "char(12)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractNoteReference",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Class",
                table: "Instrument");

            migrationBuilder.DropColumn(
                name: "Isin",
                table: "Instrument");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TransactionLeg",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
