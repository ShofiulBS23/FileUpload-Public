using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace File_Public.Migrations
{
    /// <inheritdoc />
    public partial class DocumentTableMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ISIN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocDate = table.Column<DateTime>(type: "datetime2", maxLength: 20, nullable: false),
                    DocName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DocExt = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents");
        }
    }
}
