using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ezZkvi.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrvaMigracija : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Predmet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predmet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pitanje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TekstPitanja = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tezina = table.Column<int>(type: "int", nullable: false),
                    PredmetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pitanje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pitanje_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Odgovor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tekst = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsTacan = table.Column<bool>(type: "bit", nullable: false),
                    PitanjeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Odgovor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Odgovor_Pitanje_PitanjeId",
                        column: x => x.PitanjeId,
                        principalTable: "Pitanje",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Odgovor_PitanjeId",
                table: "Odgovor",
                column: "PitanjeId");

            migrationBuilder.CreateIndex(
                name: "IX_Pitanje_PredmetId",
                table: "Pitanje",
                column: "PredmetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Odgovor");

            migrationBuilder.DropTable(
                name: "Pitanje");

            migrationBuilder.DropTable(
                name: "Predmet");
        }
    }
}
