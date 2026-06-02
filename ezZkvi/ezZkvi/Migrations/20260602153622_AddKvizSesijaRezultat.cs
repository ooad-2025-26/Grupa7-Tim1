using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ezZkvi.Migrations
{
    /// <inheritdoc />
    public partial class AddKvizSesijaRezultat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pitanje_Predmet_PredmetId",
                table: "Pitanje");

            migrationBuilder.AlterColumn<int>(
                name: "PredmetId",
                table: "Pitanje",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BrojTacnih",
                table: "KvizSesija",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DatumZavrsetka",
                table: "KvizSesija",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PredmetId",
                table: "KvizSesija",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Procenat",
                table: "KvizSesija",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "KvizSesija",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KvizSesija_PredmetId",
                table: "KvizSesija",
                column: "PredmetId");

            migrationBuilder.AddForeignKey(
                name: "FK_KvizSesija_Predmet_PredmetId",
                table: "KvizSesija",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pitanje_Predmet_PredmetId",
                table: "Pitanje",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KvizSesija_Predmet_PredmetId",
                table: "KvizSesija");

            migrationBuilder.DropForeignKey(
                name: "FK_Pitanje_Predmet_PredmetId",
                table: "Pitanje");

            migrationBuilder.DropIndex(
                name: "IX_KvizSesija_PredmetId",
                table: "KvizSesija");

            migrationBuilder.DropColumn(
                name: "BrojTacnih",
                table: "KvizSesija");

            migrationBuilder.DropColumn(
                name: "DatumZavrsetka",
                table: "KvizSesija");

            migrationBuilder.DropColumn(
                name: "PredmetId",
                table: "KvizSesija");

            migrationBuilder.DropColumn(
                name: "Procenat",
                table: "KvizSesija");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "KvizSesija");

            migrationBuilder.AlterColumn<int>(
                name: "PredmetId",
                table: "Pitanje",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Pitanje_Predmet_PredmetId",
                table: "Pitanje",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id");
        }
    }
}
