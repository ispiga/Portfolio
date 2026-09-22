using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExperienceTranslations",
                columns: table => new
                {
                    ExperienceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RoleTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperienceTranslations", x => new { x.ExperienceId, x.LanguageCode });
                    table.CheckConstraint("CK_ExperienceTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')");
                    table.ForeignKey(
                        name: "FK_ExperienceTranslations_Experiences_ExperienceId",
                        column: x => x.ExperienceId,
                        principalTable: "Experiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO [ExperienceTranslations] ([ExperienceId], [LanguageCode], [RoleTitle], [CompanyName], [Summary])
                SELECT [Id], 'es-ES', [RoleTitle], [CompanyName], [Summary]
                FROM [Experiences];
                """);

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "RoleTitle",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Experiences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Experiences",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoleTitle",
                table: "Experiences",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Experiences",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE experiences
                SET [CompanyName] = translations.[CompanyName],
                    [RoleTitle] = translations.[RoleTitle],
                    [Summary] = translations.[Summary]
                FROM [Experiences] experiences
                INNER JOIN [ExperienceTranslations] translations
                    ON translations.[ExperienceId] = experiences.[Id]
                    AND translations.[LanguageCode] = 'es-ES';
                """);

            migrationBuilder.DropTable(
                name: "ExperienceTranslations");
        }
    }
}
