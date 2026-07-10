using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalTickets",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalTickets",
                table: "events");
        }
    }
}
