using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ezZkvi.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentStatistika : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentStatistika",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KorisnikId = table.Column<string>(type: "text", nullable: false),
                    PredmetId = table.Column<int>(type: "integer", nullable: false),
                    BrojKvizova = table.Column<int>(type: "integer", nullable: false),
                    UkupnoPitanja = table.Column<int>(type: "integer", nullable: false),
                    TacniOdgovori = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentStatistika", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentStatistika_AspNetUsers_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentStatistika_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentStatistika_KorisnikId_PredmetId",
                table: "StudentStatistika",
                columns: new[] { "KorisnikId", "PredmetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentStatistika_PredmetId",
                table: "StudentStatistika",
                column: "PredmetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentStatistika");
        }
    }
}
