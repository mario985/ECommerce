using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationOrderCorrelation : Migration
    {
        private static readonly string[] OrderProductColumns = ["OrderId", "ProductId"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "InventoryReservations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_OrderId",
                table: "InventoryReservations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_OrderId_ProductId",
                table: "InventoryReservations",
                columns: OrderProductColumns,
                unique: true,
                filter: "OrderId IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryReservations_OrderId",
                table: "InventoryReservations");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReservations_OrderId_ProductId",
                table: "InventoryReservations");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "InventoryReservations");
        }
    }
}
