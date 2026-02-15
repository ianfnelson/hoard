using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstrumentDataFeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create DataFeed table
            migrationBuilder.CreateTable(
                name: "DataFeed",
                columns: table => new
                {
                    DataFeedId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataFeed", x => x.DataFeedId);
                });

            // Step 2: Create InstrumentDataFeed table
            migrationBuilder.CreateTable(
                name: "InstrumentDataFeed",
                columns: table => new
                {
                    InstrumentDataFeedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    DataFeedId = table.Column<int>(type: "int", nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentDataFeed", x => x.InstrumentDataFeedId);
                    table.ForeignKey(
                        name: "FK_InstrumentDataFeed_DataFeed_DataFeedId",
                        column: x => x.DataFeedId,
                        principalTable: "DataFeed",
                        principalColumn: "DataFeedId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstrumentDataFeed_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentDataFeed_DataFeedId",
                table: "InstrumentDataFeed",
                column: "DataFeedId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentDataFeed_InstrumentId_DataFeedId",
                table: "InstrumentDataFeed",
                columns: new[] { "InstrumentId", "DataFeedId" },
                unique: true);

            // Step 3: Seed DataFeed reference data
            migrationBuilder.InsertData(
                table: "DataFeed",
                columns: new[] { "DataFeedId", "Name" },
                values: new object[,]
                {
                    { 1, "News" },
                    { 2, "Prices" },
                    { 3, "Quotes" }
                });

            // Step 4: Migrate existing data - News feeds
            migrationBuilder.Sql(@"
                INSERT INTO InstrumentDataFeed (InstrumentId, DataFeedId, Configuration, CreatedUtc)
                SELECT
                    i.InstrumentId,
                    1, -- News feed
                    JSON_OBJECT('Ticker': i.TickerNewsUpdates, 'ImportStartUtc': i.NewsImportStartUtc),
                    SYSUTCDATETIME()
                FROM Instrument i
                WHERE i.EnableNewsUpdates = 1
                  AND i.TickerNewsUpdates IS NOT NULL;
            ");

            // Step 5: Migrate existing data - Prices feeds
            migrationBuilder.Sql(@"
                INSERT INTO InstrumentDataFeed (InstrumentId, DataFeedId, Configuration, CreatedUtc)
                SELECT
                    i.InstrumentId,
                    2, -- Prices feed
                    JSON_OBJECT('Ticker': i.TickerPriceUpdates),
                    SYSUTCDATETIME()
                FROM Instrument i
                WHERE i.EnablePriceUpdates = 1
                  AND i.TickerPriceUpdates IS NOT NULL;
            ");

            // Step 6: Migrate existing data - Quotes feeds
            migrationBuilder.Sql(@"
                INSERT INTO InstrumentDataFeed (InstrumentId, DataFeedId, Configuration, CreatedUtc)
                SELECT
                    i.InstrumentId,
                    3, -- Quotes feed
                    JSON_OBJECT('Ticker': i.TickerPriceUpdates),
                    SYSUTCDATETIME()
                FROM Instrument i
                WHERE i.EnablePriceUpdates = 1
                  AND i.TickerPriceUpdates IS NOT NULL;
            ");

            // Step 7: NOW drop old columns (data is safely migrated)
            migrationBuilder.DropColumn(
                name: "EnableNewsUpdates",
                table: "Instrument");

            migrationBuilder.DropColumn(
                name: "EnablePriceUpdates",
                table: "Instrument");

            migrationBuilder.DropColumn(
                name: "NewsImportStartUtc",
                table: "Instrument");

            migrationBuilder.DropColumn(
                name: "TickerNewsUpdates",
                table: "Instrument");

            migrationBuilder.DropColumn(
                name: "TickerPriceUpdates",
                table: "Instrument");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore old columns first
            migrationBuilder.AddColumn<bool>(
                name: "EnableNewsUpdates",
                table: "Instrument",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnablePriceUpdates",
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

            migrationBuilder.AddColumn<string>(
                name: "TickerPriceUpdates",
                table: "Instrument",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            // Migrate data back from InstrumentDataFeed to Instrument
            migrationBuilder.Sql(@"
                UPDATE i
                SET i.EnableNewsUpdates = 1,
                    i.TickerNewsUpdates = JSON_VALUE(idf.Configuration, '$.Ticker'),
                    i.NewsImportStartUtc = TRY_CAST(JSON_VALUE(idf.Configuration, '$.ImportStartUtc') AS datetime2)
                FROM Instrument i
                INNER JOIN InstrumentDataFeed idf ON i.InstrumentId = idf.InstrumentId
                WHERE idf.DataFeedId = 1;
            ");

            migrationBuilder.Sql(@"
                UPDATE i
                SET i.EnablePriceUpdates = 1,
                    i.TickerPriceUpdates = JSON_VALUE(idf.Configuration, '$.Ticker')
                FROM Instrument i
                INNER JOIN InstrumentDataFeed idf ON i.InstrumentId = idf.InstrumentId
                WHERE idf.DataFeedId = 2 OR idf.DataFeedId = 3;
            ");

            // Drop new tables
            migrationBuilder.DropTable(
                name: "InstrumentDataFeed");

            migrationBuilder.DropTable(
                name: "DataFeed");
        }
    }
}
