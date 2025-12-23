using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInstrumentIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Instrument");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Instrument",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
