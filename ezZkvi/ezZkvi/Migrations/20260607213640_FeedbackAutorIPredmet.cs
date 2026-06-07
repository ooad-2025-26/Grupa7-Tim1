using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ezZkvi.Migrations
{
    /// <inheritdoc />
    public partial class FeedbackAutorIPredmet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KorisnikId",
                table: "Feedback",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PredmetId",
                table: "Feedback",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_PredmetId",
                table: "Feedback",
                column: "PredmetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Predmet_PredmetId",
                table: "Feedback",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Predmet_PredmetId",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_PredmetId",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "KorisnikId",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "PredmetId",
                table: "Feedback");
        }
    }
}
