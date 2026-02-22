using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeColourRequiredAndAddToOtherEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Colour to InstrumentType with temporary default
            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "InstrumentType",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#808080");

            // Add Colour to AssetSubclass with temporary default
            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "AssetSubclass",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#808080");

            // Add Colour to AssetClass with temporary default
            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "AssetClass",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#808080");

            // Update Account.Colour for existing NULL values and make non-nullable
            migrationBuilder.Sql("UPDATE Account SET Colour = '#808080' WHERE Colour IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "Colour",
                table: "Account",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(7)",
                oldMaxLength: 7,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colour",
                table: "InstrumentType");

            migrationBuilder.DropColumn(
                name: "Colour",
                table: "AssetSubclass");

            migrationBuilder.DropColumn(
                name: "Colour",
                table: "AssetClass");

            migrationBuilder.AlterColumn<string>(
                name: "Colour",
                table: "Account",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(7)",
                oldMaxLength: 7);
        }
    }
}
