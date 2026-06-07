using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ezZkvi.Migrations
{
    /// <inheritdoc />
    public partial class PredmetKreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KreatorId",
                table: "Predmet",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KreatorId",
                table: "Predmet");
        }
    }
}
