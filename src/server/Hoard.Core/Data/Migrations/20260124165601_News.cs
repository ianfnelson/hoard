using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class News : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TickerApi",
                table: "Instrument",
                newName: "TickerPriceUpdates");

            migrationBuilder.RenameColumn(
                name: "Ticker",
                table: "Instrument",
                newName: "TickerDisplay");

            migrationBuilder.AlterColumn<bool>(
                name: "EnablePriceUpdates",
                table: "Instrument",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<bool>(
                name: "EnableNewsUpdates",
                table: "Instrument",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewsImportStartUtc",
                table: "Instrument",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TickerNewsUpdates",
                table: "Instrument",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NewsArticle",
                columns: table => new
                {
                    NewsArticleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    PublishedUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    RetrievedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceArticleId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Headline = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticle", x => x.NewsArticleId);
                    table.ForeignKey(
                        name: "FK_NewsArticle_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_InstrumentId",
                table: "NewsArticle",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_Source_SourceArticleId",
                table: "NewsArticle",
                columns: new[] { "Source", "SourceArticleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsArticle");

            migrationBuilder.DropColumn(
                name: "EnableNewsUpdates",
                table: "Instrument");

            migrationBuilder.DropColumn(
                name: "NewsImportStartUtc",
                table: "Instrument");

            migrationBuilder.DropColumn(
                name: "TickerNewsUpdates",
                table: "Instrument");

            migrationBuilder.RenameColumn(
                name: "TickerPriceUpdates",
                table: "Instrument",
                newName: "TickerApi");

            migrationBuilder.RenameColumn(
                name: "TickerDisplay",
                table: "Instrument",
                newName: "Ticker");

            migrationBuilder.AlterColumn<bool>(
                name: "EnablePriceUpdates",
                table: "Instrument",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
