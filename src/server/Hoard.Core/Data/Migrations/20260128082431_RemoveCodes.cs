using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InstrumentType_Code",
                table: "InstrumentType");

            migrationBuilder.DropIndex(
                name: "IX_AssetSubclass_Code",
                table: "AssetSubclass");

            migrationBuilder.DropIndex(
                name: "IX_AssetClass_Code",
                table: "AssetClass");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "InstrumentType");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AssetSubclass");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AssetClass");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "InstrumentType",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AssetSubclass",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AssetClass",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentType_Code",
                table: "InstrumentType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetSubclass_Code",
                table: "AssetSubclass",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetClass_Code",
                table: "AssetClass",
                column: "Code",
                unique: true);
        }
    }
}
