using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ezZkvi.Migrations
{
    /// <inheritdoc />
    public partial class AddOblast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Oblast",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Naziv = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PredmetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oblast", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oblast_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<int>(
                name: "OblastId",
                table: "Pitanje",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OblastId",
                table: "KvizSesija",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO "Oblast" ("Naziv", "PredmetId")
                SELECT 'Općenito', p."Id"
                FROM "Predmet" p
                WHERE NOT EXISTS (
                    SELECT 1 FROM "Oblast" o WHERE o."PredmetId" = p."Id"
                );
                """);

            migrationBuilder.Sql("""
                UPDATE "Pitanje" p
                SET "OblastId" = (
                    SELECT o."Id"
                    FROM "Oblast" o
                    WHERE o."PredmetId" = p."PredmetId"
                    ORDER BY o."Id"
                    LIMIT 1
                )
                WHERE p."OblastId" IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "OblastId",
                table: "Pitanje",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oblast_PredmetId",
                table: "Oblast",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Pitanje_OblastId",
                table: "Pitanje",
                column: "OblastId");

            migrationBuilder.CreateIndex(
                name: "IX_KvizSesija_OblastId",
                table: "KvizSesija",
                column: "OblastId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pitanje_Oblast_OblastId",
                table: "Pitanje",
                column: "OblastId",
                principalTable: "Oblast",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KvizSesija_Oblast_OblastId",
                table: "KvizSesija",
                column: "OblastId",
                principalTable: "Oblast",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KvizSesija_Oblast_OblastId",
                table: "KvizSesija");

            migrationBuilder.DropForeignKey(
                name: "FK_Pitanje_Oblast_OblastId",
                table: "Pitanje");

            migrationBuilder.DropIndex(
                name: "IX_KvizSesija_OblastId",
                table: "KvizSesija");

            migrationBuilder.DropIndex(
                name: "IX_Pitanje_OblastId",
                table: "Pitanje");

            migrationBuilder.DropColumn(
                name: "OblastId",
                table: "KvizSesija");

            migrationBuilder.DropColumn(
                name: "OblastId",
                table: "Pitanje");

            migrationBuilder.DropTable(
                name: "Oblast");
        }
    }
}
