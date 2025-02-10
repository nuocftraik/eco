using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.WebApi.Migrators.MSSQL.Migrations.Application
{
    /// <inheritdoc />
    public partial class Clear_product_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_CustomerId",
                schema: "Ordering",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "OrderTransactions",
                schema: "Ordering");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId",
                schema: "Ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Height",
                schema: "Catalog",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Catalog",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "Length",
                schema: "Catalog",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "RequireShipping",
                schema: "Catalog",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "SKU",
                schema: "Catalog",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "TrackInventory",
                schema: "Catalog",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "Weight",
                schema: "Catalog",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "Width",
                schema: "Catalog",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "Height",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Length",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RequireShipping",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SKU",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TrackInventory",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Weight",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Width",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "Ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Discount",
                schema: "Ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                schema: "Ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingMethod",
                schema: "Ordering",
                table: "Orders");

            migrationBuilder.EnsureSchema(
                name: "Payment");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Transactions",
                newSchema: "Payment");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payments",
                newSchema: "Payment");

            migrationBuilder.RenameColumn(
                name: "GrandTotal",
                schema: "Ordering",
                table: "Orders",
                newName: "Total");

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                schema: "Catalog",
                table: "Products",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                schema: "Payment",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                schema: "Payment",
                table: "Payments",
                column: "OrderId",
                principalSchema: "Ordering",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                schema: "Payment",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                schema: "Payment",
                table: "Payments");

            migrationBuilder.RenameTable(
                name: "Transactions",
                schema: "Payment",
                newName: "Transactions");

            migrationBuilder.RenameTable(
                name: "Payments",
                schema: "Payment",
                newName: "Payments");

            migrationBuilder.RenameColumn(
                name: "Total",
                schema: "Ordering",
                table: "Orders",
                newName: "GrandTotal");

            migrationBuilder.AddColumn<double>(
                name: "Height",
                schema: "Catalog",
                table: "Variants",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Catalog",
                table: "Variants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Length",
                schema: "Catalog",
                table: "Variants",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireShipping",
                schema: "Catalog",
                table: "Variants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                schema: "Catalog",
                table: "Variants",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "TrackInventory",
                schema: "Catalog",
                table: "Variants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                schema: "Catalog",
                table: "Variants",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Width",
                schema: "Catalog",
                table: "Variants",
                type: "float",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                schema: "Catalog",
                table: "Products",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<double>(
                name: "Height",
                schema: "Catalog",
                table: "Products",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Catalog",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Length",
                schema: "Catalog",
                table: "Products",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireShipping",
                schema: "Catalog",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                schema: "Catalog",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TrackInventory",
                schema: "Catalog",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                schema: "Catalog",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                schema: "Catalog",
                table: "Products",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Width",
                schema: "Catalog",
                table: "Products",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                schema: "Ordering",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Discount",
                schema: "Ordering",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ShippingFee",
                schema: "Ordering",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingMethod",
                schema: "Ordering",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrderTransactions",
                schema: "Ordering",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Ordering",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                schema: "Ordering",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTransactions_OrderId",
                schema: "Ordering",
                table: "OrderTransactions",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_CustomerId",
                schema: "Ordering",
                table: "Orders",
                column: "CustomerId",
                principalSchema: "Identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
