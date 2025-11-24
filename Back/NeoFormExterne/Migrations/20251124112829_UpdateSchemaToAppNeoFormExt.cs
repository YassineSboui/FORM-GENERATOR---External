using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoForm_Externe.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaToAppNeoFormExt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AppNeoFormExt");

            migrationBuilder.RenameTable(
                name: "UserAuthentications",
                newName: "UserAuthentications",
                newSchema: "AppNeoFormExt");

            migrationBuilder.RenameTable(
                name: "Object",
                schema: "AppNeoForm",
                newName: "Object",
                newSchema: "AppNeoFormExt");

            migrationBuilder.RenameTable(
                name: "Clients",
                schema: "AppNeoForm",
                newName: "Clients",
                newSchema: "AppNeoFormExt");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "AspNetUserTokens",
                newSchema: "AppNeoFormExt");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "AspNetUsers",
                newSchema: "AppNeoFormExt");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "AspNetUserRoles",
                newSchema: "AppNeoFormExt");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "AspNetUserLogins",
                newSchema: "AppNeoFormExt");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "AspNetUserClaims",
                newSchema: "AppNeoFormExt");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "AspNetRoles",
                newSchema: "AppNeoFormExt");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "AspNetRoleClaims",
                newSchema: "AppNeoFormExt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AppNeoForm");

            migrationBuilder.RenameTable(
                name: "UserAuthentications",
                schema: "AppNeoFormExt",
                newName: "UserAuthentications");

            migrationBuilder.RenameTable(
                name: "Object",
                schema: "AppNeoFormExt",
                newName: "Object",
                newSchema: "AppNeoForm");

            migrationBuilder.RenameTable(
                name: "Clients",
                schema: "AppNeoFormExt",
                newName: "Clients",
                newSchema: "AppNeoForm");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                schema: "AppNeoFormExt",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "AppNeoFormExt",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "AppNeoFormExt",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                schema: "AppNeoFormExt",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "AppNeoFormExt",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "AppNeoFormExt",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "AppNeoFormExt",
                newName: "AspNetRoleClaims");
        }
    }
}
