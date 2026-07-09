using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreferenceService.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferenceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "preferences",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "City",
                table: "preferences",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "preferences");

            migrationBuilder.DropColumn(
                name: "City",
                table: "preferences");
        }
    }
}
