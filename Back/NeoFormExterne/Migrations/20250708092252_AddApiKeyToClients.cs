using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoForm_Externe.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyToClients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                schema: "AppNeoForm",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                schema: "AppNeoForm",
                table: "Clients");
        }
    }
}
