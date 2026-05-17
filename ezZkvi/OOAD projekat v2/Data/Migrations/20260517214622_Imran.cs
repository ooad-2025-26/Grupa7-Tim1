using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ezZkvi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Imran : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Odgovor_Pitanje_PitanjeId",
                table: "Odgovor");

            migrationBuilder.AlterColumn<string>(
                name: "Naziv",
                table: "Predmet",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TekstPitanja",
                table: "Pitanje",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Tekst",
                table: "Odgovor",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "PitanjeId",
                table: "Odgovor",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Lozinka",
                table: "Korisnik",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "BrojOdgovorenihPitanja",
                table: "Korisnik",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BrojTacnihOdgovora",
                table: "Korisnik",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Korisnik",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Student_BrojOdgovorenihPitanja",
                table: "Korisnik",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Student_BrojTacnihOdgovora",
                table: "Korisnik",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sadrzaj",
                table: "Feedback",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Odgovor_Pitanje_PitanjeId",
                table: "Odgovor",
                column: "PitanjeId",
                principalTable: "Pitanje",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Odgovor_Pitanje_PitanjeId",
                table: "Odgovor");

            migrationBuilder.DropColumn(
                name: "BrojOdgovorenihPitanja",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "BrojTacnihOdgovora",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "Student_BrojOdgovorenihPitanja",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "Student_BrojTacnihOdgovora",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "Sadrzaj",
                table: "Feedback");

            migrationBuilder.AlterColumn<string>(
                name: "Naziv",
                table: "Predmet",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TekstPitanja",
                table: "Pitanje",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Tekst",
                table: "Odgovor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<int>(
                name: "PitanjeId",
                table: "Odgovor",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Lozinka",
                table: "Korisnik",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Odgovor_Pitanje_PitanjeId",
                table: "Odgovor",
                column: "PitanjeId",
                principalTable: "Pitanje",
                principalColumn: "Id");
        }
    }
}
