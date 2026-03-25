using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgyptTechJobsAdmin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVisibleToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVisibleToUsers",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVisibleToUsers",
                table: "Jobs");
        }
    }
}
