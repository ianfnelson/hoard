using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountType",
                columns: table => new
                {
                    AccountTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountType", x => x.AccountTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AssetClass",
                columns: table => new
                {
                    AssetClassId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetClass", x => x.AssetClassId);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    CurrencyId = table.Column<string>(type: "char(3)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.CurrencyId);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentType",
                columns: table => new
                {
                    InstrumentTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCash = table.Column<bool>(type: "bit", nullable: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
                    IsFxPair = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentType", x => x.InstrumentTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Portfolio",
                columns: table => new
                {
                    PortfolioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolio", x => x.PortfolioId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCategory",
                columns: table => new
                {
                    TransactionCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCategory", x => x.TransactionCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionLegCategory",
                columns: table => new
                {
                    TransactionLegCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionLegCategory", x => x.TransactionLegCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountTypeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Account_AccountType_AccountTypeId",
                        column: x => x.AccountTypeId,
                        principalTable: "AccountType",
                        principalColumn: "AccountTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetSubclass",
                columns: table => new
                {
                    AssetSubclassId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetClassId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetSubclass", x => x.AssetSubclassId);
                    table.ForeignKey(
                        name: "FK_AssetSubclass_AssetClass_AssetClassId",
                        column: x => x.AssetClassId,
                        principalTable: "AssetClass",
                        principalColumn: "AssetClassId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioAccount",
                columns: table => new
                {
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioAccount", x => new { x.PortfolioId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_PortfolioAccount_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioAccount_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    TransactionTypeId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transaction_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaction_TransactionCategory_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalTable: "TransactionCategory",
                        principalColumn: "TransactionCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Instrument",
                columns: table => new
                {
                    InstrumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstrumentTypeId = table.Column<int>(type: "int", nullable: false),
                    AssetSubclassId = table.Column<int>(type: "int", nullable: false),
                    BaseCurrencyId = table.Column<string>(type: "char(3)", nullable: false),
                    QuoteCurrencyId = table.Column<string>(type: "char(3)", nullable: false),
                    TickerApi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TickerDisplay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EnablePriceUpdates = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrument", x => x.InstrumentId);
                    table.ForeignKey(
                        name: "FK_Instrument_AssetSubclass_AssetSubclassId",
                        column: x => x.AssetSubclassId,
                        principalTable: "AssetSubclass",
                        principalColumn: "AssetSubclassId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrument_Currency_BaseCurrencyId",
                        column: x => x.BaseCurrencyId,
                        principalTable: "Currency",
                        principalColumn: "CurrencyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrument_Currency_QuoteCurrencyId",
                        column: x => x.QuoteCurrencyId,
                        principalTable: "Currency",
                        principalColumn: "CurrencyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrument_InstrumentType_InstrumentTypeId",
                        column: x => x.InstrumentTypeId,
                        principalTable: "InstrumentType",
                        principalColumn: "InstrumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioAssetTarget",
                columns: table => new
                {
                    PortfolioAssetTargetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    AssetSubclassId = table.Column<int>(type: "int", nullable: false),
                    Target = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioAssetTarget", x => x.PortfolioAssetTargetId);
                    table.CheckConstraint("CK_Target_0_100", "[Target] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_PortfolioAssetTarget_AssetSubclass_AssetSubclassId",
                        column: x => x.AssetSubclassId,
                        principalTable: "AssetSubclass",
                        principalColumn: "AssetSubclassId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioAssetTarget_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Holding",
                columns: table => new
                {
                    HoldingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holding", x => x.HoldingId);
                    table.ForeignKey(
                        name: "FK_Holding_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Holding_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Price",
                columns: table => new
                {
                    PriceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RetrievedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    AdjustedClose = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Price", x => x.PriceId);
                    table.ForeignKey(
                        name: "FK_Price_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "TransactionLeg",
                columns: table => new
                {
                    TransactionLegId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    TransactionLegTypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionLeg", x => x.TransactionLegId);
                    table.ForeignKey(
                        name: "FK_TransactionLeg_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionLeg_TransactionLegCategory_TransactionLegTypeId",
                        column: x => x.TransactionLegTypeId,
                        principalTable: "TransactionLegCategory",
                        principalColumn: "TransactionLegCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionLeg_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_AccountTypeId",
                table: "Account",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetSubclass_AssetClassId",
                table: "AssetSubclass",
                column: "AssetClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Holding_AccountId_InstrumentId_AsOfDate",
                table: "Holding",
                columns: new[] { "AccountId", "InstrumentId", "AsOfDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holding_InstrumentId",
                table: "Holding",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_AssetSubclassId",
                table: "Instrument",
                column: "AssetSubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_BaseCurrencyId",
                table: "Instrument",
                column: "BaseCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_InstrumentTypeId",
                table: "Instrument",
                column: "InstrumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_QuoteCurrencyId",
                table: "Instrument",
                column: "QuoteCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioAccount_AccountId",
                table: "PortfolioAccount",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioAssetTarget_AssetSubclassId",
                table: "PortfolioAssetTarget",
                column: "AssetSubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioAssetTarget_PortfolioId_AssetSubclassId",
                table: "PortfolioAssetTarget",
                columns: new[] { "PortfolioId", "AssetSubclassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Price_InstrumentId_AsOfDate",
                table: "Price",
                columns: new[] { "InstrumentId", "AsOfDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quote_InstrumentId",
                table: "Quote",
                column: "InstrumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AccountId",
                table: "Transaction",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionTypeId",
                table: "Transaction",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLeg_InstrumentId",
                table: "TransactionLeg",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLeg_TransactionId",
                table: "TransactionLeg",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLeg_TransactionLegTypeId",
                table: "TransactionLeg",
                column: "TransactionLegTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holding");

            migrationBuilder.DropTable(
                name: "PortfolioAccount");

            migrationBuilder.DropTable(
                name: "PortfolioAssetTarget");

            migrationBuilder.DropTable(
                name: "Price");

            migrationBuilder.DropTable(
                name: "Quote");

            migrationBuilder.DropTable(
                name: "TransactionLeg");

            migrationBuilder.DropTable(
                name: "Portfolio");

            migrationBuilder.DropTable(
                name: "Instrument");

            migrationBuilder.DropTable(
                name: "TransactionLegCategory");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "AssetSubclass");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "InstrumentType");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "TransactionCategory");

            migrationBuilder.DropTable(
                name: "AssetClass");

            migrationBuilder.DropTable(
                name: "AccountType");
        }
    }
}
