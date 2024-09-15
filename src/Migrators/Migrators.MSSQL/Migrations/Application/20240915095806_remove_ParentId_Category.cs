using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.WebApi.Migrators.MSSQL.Migrations.Application;

/// <inheritdoc />
public partial class Remove_ParentId_Category : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Categories_Categories_ParentId",
            schema: "Catalog",
            table: "Categories");

        migrationBuilder.DropIndex(
            name: "IX_Categories_ParentId",
            schema: "Catalog",
            table: "Categories");

        migrationBuilder.DropColumn(
            name: "Image",
            schema: "Catalog",
            table: "Categories");

        migrationBuilder.DropColumn(
            name: "ParentId",
            schema: "Catalog",
            table: "Categories");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Image",
            schema: "Catalog",
            table: "Categories",
            type: "nvarchar(1024)",
            maxLength: 1024,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ParentId",
            schema: "Catalog",
            table: "Categories",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Categories_ParentId",
            schema: "Catalog",
            table: "Categories",
            column: "ParentId");

        migrationBuilder.AddForeignKey(
            name: "FK_Categories_Categories_ParentId",
            schema: "Catalog",
            table: "Categories",
            column: "ParentId",
            principalSchema: "Catalog",
            principalTable: "Categories",
            principalColumn: "Id");
    }
}
