using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicenseTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Licenses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Licenses");
        }
    }
}
