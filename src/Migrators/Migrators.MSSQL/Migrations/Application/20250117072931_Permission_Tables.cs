using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.WebApi.Migrators.MSSQL.Migrations.Application;

/// <inheritdoc />
public partial class Permission_Tables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Actions",
            schema: "Identity",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Actions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Functions",
            schema: "Identity",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Functions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ActionInFunctions",
            schema: "Identity",
            columns: table => new
            {
                ActionId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                FunctionId = table.Column<string>(type: "nvarchar(50)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ActionInFunctions", x => new { x.ActionId, x.FunctionId });
                table.ForeignKey(
                    name: "FK_ActionInFunctions_Actions_ActionId",
                    column: x => x.ActionId,
                    principalSchema: "Identity",
                    principalTable: "Actions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ActionInFunctions_Functions_FunctionId",
                    column: x => x.FunctionId,
                    principalSchema: "Identity",
                    principalTable: "Functions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Permissions",
            schema: "Identity",
            columns: table => new
            {
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                FunctionId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                ActionId = table.Column<string>(type: "nvarchar(50)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Permissions", x => new { x.RoleId, x.FunctionId, x.ActionId });
                table.ForeignKey(
                    name: "FK_Permissions_Actions_ActionId",
                    column: x => x.ActionId,
                    principalSchema: "Identity",
                    principalTable: "Actions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Permissions_Functions_FunctionId",
                    column: x => x.FunctionId,
                    principalSchema: "Identity",
                    principalTable: "Functions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ActionInFunctions_FunctionId",
            schema: "Identity",
            table: "ActionInFunctions",
            column: "FunctionId");

        migrationBuilder.CreateIndex(
            name: "IX_Permissions_ActionId",
            schema: "Identity",
            table: "Permissions",
            column: "ActionId");

        migrationBuilder.CreateIndex(
            name: "IX_Permissions_FunctionId",
            schema: "Identity",
            table: "Permissions",
            column: "FunctionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ActionInFunctions",
            schema: "Identity");

        migrationBuilder.DropTable(
            name: "Permissions",
            schema: "Identity");

        migrationBuilder.DropTable(
            name: "Actions",
            schema: "Identity");

        migrationBuilder.DropTable(
            name: "Functions",
            schema: "Identity");
    }
}
