using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcselApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveParticipationAndPaperTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaperTitle",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "ParticipationType",
                table: "Registrations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaperTitle",
                table: "Registrations",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParticipationType",
                table: "Registrations",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }
    }
}
