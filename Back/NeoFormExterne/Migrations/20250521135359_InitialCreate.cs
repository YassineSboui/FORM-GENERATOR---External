using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoForm_Externe.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AppNeoForm");

            migrationBuilder.CreateTable(
                name: "Clients",
                schema: "AppNeoForm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Object",
                schema: "AppNeoForm",
                columns: table => new
                {
                    _id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    guid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, computedColumnSql: "(left(json_value([ObjectJson],'$.guid'),(100)))", stored: false),
                    application = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, computedColumnSql: "(left(json_value([ObjectJson],'$.application'),(100)))", stored: false),
                    objectName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, computedColumnSql: "(left(json_value([ObjectJson],'$.objectName'),(100)))", stored: false),
                    objectType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, computedColumnSql: "(left(json_value([ObjectJson],'$.objectType'),(100)))", stored: false),
                    isEncrypted = table.Column<bool>(type: "bit", nullable: false, computedColumnSql: "CASE WHEN JSON_VALUE([ObjectJson], '$.isEncrypted') = 'true' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", stored: false),
                    ObjectJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Object", x => x._id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clients",
                schema: "AppNeoForm");

            migrationBuilder.DropTable(
                name: "Object",
                schema: "AppNeoForm");
        }
    }
}
