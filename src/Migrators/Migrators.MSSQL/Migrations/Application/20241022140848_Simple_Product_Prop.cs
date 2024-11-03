using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.WebApi.Migrators.MSSQL.Migrations.Application;

/// <inheritdoc />
public partial class Simple_Product_Prop : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_VariantAttributeValues_AttributeValues_AttributeValueId",
            schema: "Catalog",
            table: "VariantAttributeValues");

        migrationBuilder.DropForeignKey(
            name: "FK_VariantAttributeValues_Variants_VariantId",
            schema: "Catalog",
            table: "VariantAttributeValues");

        migrationBuilder.AddColumn<double>(
            name: "ComparePrice",
            schema: "Catalog",
            table: "Products",
            type: "float",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FileName",
            schema: "Catalog",
            table: "Products",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FileUrl",
            schema: "Catalog",
            table: "Products",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<double>(
            name: "Height",
            schema: "Catalog",
            table: "Products",
            type: "float",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IncludeDownload",
            schema: "Catalog",
            table: "Products",
            type: "bit",
            nullable: false,
            defaultValue: false);

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

        migrationBuilder.AddColumn<double>(
            name: "Price",
            schema: "Catalog",
            table: "Products",
            type: "float",
            nullable: false,
            defaultValue: 0.0);

        migrationBuilder.AddColumn<int>(
            name: "Quantity",
            schema: "Catalog",
            table: "Products",
            type: "int",
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
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<bool>(
            name: "TrackInventory",
            schema: "Catalog",
            table: "Products",
            type: "bit",
            nullable: false,
            defaultValue: false);

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

        migrationBuilder.AddForeignKey(
            name: "FK_VariantAttributeValues_AttributeValues_AttributeValueId",
            schema: "Catalog",
            table: "VariantAttributeValues",
            column: "AttributeValueId",
            principalSchema: "Attribute",
            principalTable: "AttributeValues",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_VariantAttributeValues_Variants_VariantId",
            schema: "Catalog",
            table: "VariantAttributeValues",
            column: "VariantId",
            principalSchema: "Catalog",
            principalTable: "Variants",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_VariantAttributeValues_AttributeValues_AttributeValueId",
            schema: "Catalog",
            table: "VariantAttributeValues");

        migrationBuilder.DropForeignKey(
            name: "FK_VariantAttributeValues_Variants_VariantId",
            schema: "Catalog",
            table: "VariantAttributeValues");

        migrationBuilder.DropColumn(
            name: "ComparePrice",
            schema: "Catalog",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "FileName",
            schema: "Catalog",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "FileUrl",
            schema: "Catalog",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "Height",
            schema: "Catalog",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "IncludeDownload",
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
            name: "Price",
            schema: "Catalog",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "Quantity",
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
            name: "Weight",
            schema: "Catalog",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "Width",
            schema: "Catalog",
            table: "Products");

        migrationBuilder.AddForeignKey(
            name: "FK_VariantAttributeValues_AttributeValues_AttributeValueId",
            schema: "Catalog",
            table: "VariantAttributeValues",
            column: "AttributeValueId",
            principalSchema: "Attribute",
            principalTable: "AttributeValues",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_VariantAttributeValues_Variants_VariantId",
            schema: "Catalog",
            table: "VariantAttributeValues",
            column: "VariantId",
            principalSchema: "Catalog",
            principalTable: "Variants",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
