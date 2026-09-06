using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861 // EF Core-generated migration uses inline column arrays.

namespace ECommerce.Modules.Cart.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCartCheckoutReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartCheckouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CheckoutId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CartId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    FailedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartCheckouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CartCheckoutItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Sku = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    CartCheckoutId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartCheckoutItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartCheckoutItems_CartCheckouts_CartCheckoutId",
                        column: x => x.CartCheckoutId,
                        principalTable: "CartCheckouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartCheckoutItems_CartCheckoutId_ProductId",
                table: "CartCheckoutItems",
                columns: new[] { "CartCheckoutId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartCheckouts_CartId",
                table: "CartCheckouts",
                column: "CartId",
                unique: true,
                filter: "Status = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CartCheckouts_CheckoutId",
                table: "CartCheckouts",
                column: "CheckoutId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartCheckouts_CreatedAtUtc",
                table: "CartCheckouts",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CartCheckouts_CustomerId",
                table: "CartCheckouts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CartCheckouts_OrderId",
                table: "CartCheckouts",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CartCheckouts_Status",
                table: "CartCheckouts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartCheckoutItems");

            migrationBuilder.DropTable(
                name: "CartCheckouts");
        }
    }
}
#pragma warning restore CA1861
