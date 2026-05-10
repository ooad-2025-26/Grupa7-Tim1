using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ezZkvi.Data.Migrations
{
    /// <inheritdoc />
    public partial class SveKlase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KvizSesijaPitanje",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RedniBroj = table.Column<int>(type: "int", nullable: false),
                    BrojBodova = table.Column<double>(type: "float", nullable: false),
                    Tacno = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KvizSesijaPitanje", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KvizSesijaPitanje");
        }
    }
}
