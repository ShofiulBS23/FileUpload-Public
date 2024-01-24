using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace File_Public.Migrations
{
    /// <inheritdoc />
    public partial class DocumentTypeLUTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocTypes",
                columns: table => new
                {
                    DocTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocTypes", x => x.DocTypeID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocTypes");
        }
    }
}
