using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.WebApi.Migrators.MSSQL.Migrations.Application;

/// <inheritdoc />
public partial class Notification_table : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Notification");

        migrationBuilder.CreateTable(
            name: "Notifications",
            schema: "Notification",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReceiverId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Label = table.Column<string>(type: "varchar(50)", nullable: false),
                Message = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Notifications",
            schema: "Notification");
    }
}
