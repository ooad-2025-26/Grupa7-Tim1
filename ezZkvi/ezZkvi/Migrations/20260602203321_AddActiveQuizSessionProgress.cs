using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ezZkvi.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveQuizSessionProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KvizSesijaId",
                table: "KvizSesijaPitanje",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OdgovorId",
                table: "KvizSesijaPitanje",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PitanjeId",
                table: "KvizSesijaPitanje",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DatumPocetka",
                table: "KvizSesija",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_KvizSesijaPitanje_KvizSesijaId",
                table: "KvizSesijaPitanje",
                column: "KvizSesijaId");

            migrationBuilder.CreateIndex(
                name: "IX_KvizSesijaPitanje_OdgovorId",
                table: "KvizSesijaPitanje",
                column: "OdgovorId");

            migrationBuilder.CreateIndex(
                name: "IX_KvizSesijaPitanje_PitanjeId",
                table: "KvizSesijaPitanje",
                column: "PitanjeId");

            migrationBuilder.AddForeignKey(
                name: "FK_KvizSesijaPitanje_KvizSesija_KvizSesijaId",
                table: "KvizSesijaPitanje",
                column: "KvizSesijaId",
                principalTable: "KvizSesija",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KvizSesijaPitanje_Odgovor_OdgovorId",
                table: "KvizSesijaPitanje",
                column: "OdgovorId",
                principalTable: "Odgovor",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KvizSesijaPitanje_Pitanje_PitanjeId",
                table: "KvizSesijaPitanje",
                column: "PitanjeId",
                principalTable: "Pitanje",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KvizSesijaPitanje_KvizSesija_KvizSesijaId",
                table: "KvizSesijaPitanje");

            migrationBuilder.DropForeignKey(
                name: "FK_KvizSesijaPitanje_Odgovor_OdgovorId",
                table: "KvizSesijaPitanje");

            migrationBuilder.DropForeignKey(
                name: "FK_KvizSesijaPitanje_Pitanje_PitanjeId",
                table: "KvizSesijaPitanje");

            migrationBuilder.DropIndex(
                name: "IX_KvizSesijaPitanje_KvizSesijaId",
                table: "KvizSesijaPitanje");

            migrationBuilder.DropIndex(
                name: "IX_KvizSesijaPitanje_OdgovorId",
                table: "KvizSesijaPitanje");

            migrationBuilder.DropIndex(
                name: "IX_KvizSesijaPitanje_PitanjeId",
                table: "KvizSesijaPitanje");

            migrationBuilder.DropColumn(
                name: "KvizSesijaId",
                table: "KvizSesijaPitanje");

            migrationBuilder.DropColumn(
                name: "OdgovorId",
                table: "KvizSesijaPitanje");

            migrationBuilder.DropColumn(
                name: "PitanjeId",
                table: "KvizSesijaPitanje");

            migrationBuilder.DropColumn(
                name: "DatumPocetka",
                table: "KvizSesija");
        }
    }
}
