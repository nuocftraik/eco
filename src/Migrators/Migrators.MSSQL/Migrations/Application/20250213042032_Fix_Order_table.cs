using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.WebApi.Migrators.MSSQL.Migrations.Application;

/// <inheritdoc />
public partial class Fix_Order_table : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CartItems_Products_ProductId",
            schema: "Basket",
            table: "CartItems");

        migrationBuilder.DropForeignKey(
            name: "FK_CartItems_Variants_VariantId",
            schema: "Basket",
            table: "CartItems");

        migrationBuilder.DropForeignKey(
            name: "FK_Carts_Users_UserId",
            schema: "Basket",
            table: "Carts");

        migrationBuilder.DropIndex(
            name: "IX_Carts_UserId",
            schema: "Basket",
            table: "Carts");

        migrationBuilder.DropIndex(
            name: "IX_CartItems_ProductId",
            schema: "Basket",
            table: "CartItems");

        migrationBuilder.DropColumn(
            name: "UserId",
            schema: "Basket",
            table: "Carts");

        migrationBuilder.DropColumn(
            name: "ProductId",
            schema: "Basket",
            table: "CartItems");

        migrationBuilder.AlterColumn<Guid>(
            name: "VariantId",
            schema: "Basket",
            table: "CartItems",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<double>(
            name: "Price",
            schema: "Basket",
            table: "CartItems",
            type: "float",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)");

        migrationBuilder.AddForeignKey(
            name: "FK_CartItems_Variants_VariantId",
            schema: "Basket",
            table: "CartItems",
            column: "VariantId",
            principalSchema: "Catalog",
            principalTable: "Variants",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CartItems_Variants_VariantId",
            schema: "Basket",
            table: "CartItems");

        migrationBuilder.AddColumn<string>(
            name: "UserId",
            schema: "Basket",
            table: "Carts",
            type: "nvarchar(450)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AlterColumn<Guid>(
            name: "VariantId",
            schema: "Basket",
            table: "CartItems",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            schema: "Basket",
            table: "CartItems",
            type: "decimal(18,2)",
            nullable: false,
            oldClrType: typeof(double),
            oldType: "float");

        migrationBuilder.AddColumn<Guid>(
            name: "ProductId",
            schema: "Basket",
            table: "CartItems",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateIndex(
            name: "IX_Carts_UserId",
            schema: "Basket",
            table: "Carts",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_CartItems_ProductId",
            schema: "Basket",
            table: "CartItems",
            column: "ProductId");

        migrationBuilder.AddForeignKey(
            name: "FK_CartItems_Products_ProductId",
            schema: "Basket",
            table: "CartItems",
            column: "ProductId",
            principalSchema: "Catalog",
            principalTable: "Products",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CartItems_Variants_VariantId",
            schema: "Basket",
            table: "CartItems",
            column: "VariantId",
            principalSchema: "Catalog",
            principalTable: "Variants",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Carts_Users_UserId",
            schema: "Basket",
            table: "Carts",
            column: "UserId",
            principalSchema: "Identity",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
