using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoForm_Externe.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionAuditsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionAudits",
                schema: "AppNeoFormExt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonalCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AuthType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OidcUserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TokenId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionAudits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionAudits_ClientId",
                schema: "AppNeoFormExt",
                table: "SessionAudits",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAudits_ClientId_CreatedAt",
                schema: "AppNeoFormExt",
                table: "SessionAudits",
                columns: new[] { "ClientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionAudits_CreatedAt",
                schema: "AppNeoFormExt",
                table: "SessionAudits",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAudits_Guid",
                schema: "AppNeoFormExt",
                table: "SessionAudits",
                column: "Guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionAudits",
                schema: "AppNeoFormExt");
        }
    }
}
