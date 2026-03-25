using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EgyptTechJobsAdmin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockedCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlockedKeywords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Keyword = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedKeywords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedCompanies_CompanyName",
                table: "BlockedCompanies",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedCompanies_IsActive",
                table: "BlockedCompanies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedKeywords_IsActive",
                table: "BlockedKeywords",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedKeywords_Keyword",
                table: "BlockedKeywords",
                column: "Keyword");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedCompanies");

            migrationBuilder.DropTable(
                name: "BlockedKeywords");
        }
    }
}
