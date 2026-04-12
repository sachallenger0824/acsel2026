using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcselApp.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParticipantType",
                table: "Registrations",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParticipantType",
                table: "Registrations");
        }
    }
}
