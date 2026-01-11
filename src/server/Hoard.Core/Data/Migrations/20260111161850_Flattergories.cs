using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Flattergories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_TransactionCategory_CategoryId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_TransactionSubcategory_SubcategoryId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "TransactionSubcategory");

            migrationBuilder.DropTable(
                name: "TransactionCategory");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_SubcategoryId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "SubcategoryId",
                table: "Transaction");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Transaction",
                newName: "TransactionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_CategoryId",
                table: "Transaction",
                newName: "IX_Transaction_TransactionTypeId");

            migrationBuilder.CreateTable(
                name: "TransactionType",
                columns: table => new
                {
                    TransactionTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionType", x => x.TransactionTypeId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_TransactionType_TransactionTypeId",
                table: "Transaction",
                column: "TransactionTypeId",
                principalTable: "TransactionType",
                principalColumn: "TransactionTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_TransactionType_TransactionTypeId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "TransactionType");

            migrationBuilder.RenameColumn(
                name: "TransactionTypeId",
                table: "Transaction",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_TransactionTypeId",
                table: "Transaction",
                newName: "IX_Transaction_CategoryId");

            migrationBuilder.AddColumn<int>(
                name: "SubcategoryId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransactionCategory",
                columns: table => new
                {
                    TransactionCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCategory", x => x.TransactionCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionSubcategory",
                columns: table => new
                {
                    TransactionSubcategoryId = table.Column<int>(type: "int", nullable: false),
                    TransactionCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionSubcategory", x => x.TransactionSubcategoryId);
                    table.ForeignKey(
                        name: "FK_TransactionSubcategory_TransactionCategory_TransactionCategoryId",
                        column: x => x.TransactionCategoryId,
                        principalTable: "TransactionCategory",
                        principalColumn: "TransactionCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_SubcategoryId",
                table: "Transaction",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionSubcategory_TransactionCategoryId",
                table: "TransactionSubcategory",
                column: "TransactionCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_TransactionCategory_CategoryId",
                table: "Transaction",
                column: "CategoryId",
                principalTable: "TransactionCategory",
                principalColumn: "TransactionCategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_TransactionSubcategory_SubcategoryId",
                table: "Transaction",
                column: "SubcategoryId",
                principalTable: "TransactionSubcategory",
                principalColumn: "TransactionSubcategoryId");
        }
    }
}
