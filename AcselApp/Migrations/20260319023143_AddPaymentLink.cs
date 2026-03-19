using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcselApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentLink",
                table: "Registrations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentLink",
                table: "Registrations");
        }
    }
}
